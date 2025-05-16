using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using Windows.Kinect;
using Joint = Windows.Kinect.Joint;
using System.Linq;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BodySourceView : MonoBehaviour
{
    public BodySourceManager mBodySourceManager;
    public GameObject mJointObject;
    [SerializeField] Transform parentHand;
    [SerializeField] float scaleMovement = 50;
    [SerializeField] private Sprite openMat;
    [SerializeField] private Sprite closedMat;
    [SerializeField] private Sprite lassoMat;
    [SerializeField] private Sprite unknownMat;

    [SerializeField] private AudioClip selectSound;
    [SerializeField] private AudioClip selectTimedSound;


    private Dictionary<ulong, GameObject> mBodies = new Dictionary<ulong, GameObject>();

    private Dictionary<bool, float> hoverStartTime = new Dictionary<bool, float> { { true, 0f }, { false, 0f } };


    private List<JointType> _joints = new List<JointType>
    {
        JointType.HandLeft,
        JointType.HandRight,
    };

    private void Start()
    {
        Cursor.SetCursor(openMat.texture, Vector3.zero, CursorMode.Auto);
    }

    void Update()
    {
        #region Get Kinect data

        Body[] data = mBodySourceManager.GetData();
        if (data == null)
            return;

        List<ulong> trackedIds = new List<ulong>();
        foreach (var body in data)
        {
            if (body == null)
                continue;

            if (body.IsTracked)
                trackedIds.Add(body.TrackingId);
        }

        #endregion

        #region Delete Kinect bodies

        List<ulong> knownIds = new List<ulong>(mBodies.Keys);
        foreach (ulong trackingId in knownIds)
        {
            if (!trackedIds.Contains(trackingId))
            {
                // Destroy body object
                Destroy(mBodies[trackingId]);

                // Remove from list
                mBodies.Remove(trackingId);
            }
        }

        #endregion

        #region Create Kinect bodies

        Body closestBody = null;
        float closestZ = float.MaxValue;

        // Étape 1 : trouver le corps le plus proche
        foreach (var body in data)
        {
            if (body == null || !body.IsTracked)
                continue;

            float z = body.Joints[JointType.SpineBase].Position.Z;

            if (z < closestZ)
            {
                closestZ = z;
                closestBody = body;
            }
        }

        // Étape 2 : gérer uniquement ce corps
        if (closestBody != null)
        {
            if (!mBodies.ContainsKey(closestBody.TrackingId))
                mBodies[closestBody.TrackingId] = CreateBodyObject(closestBody.TrackingId);

            UpdateBodyObject(closestBody, mBodies[closestBody.TrackingId]);
        }

        #endregion

    }


    private GameObject CreateBodyObject(ulong id)
    {
        // Create body parent
        GameObject body = new GameObject("Body:" + id);

        // Create joints
        foreach (JointType joint in _joints)
        {
            // Create Object
            GameObject newJoint = Instantiate(mJointObject);
            newJoint.name = joint.ToString();
            if (newJoint.name.Contains("Left"))
            {
                newJoint.transform.Rotate(0, 180, 0);
            }

            // Parent to body
            newJoint.transform.parent = body.transform;
        }
        body.transform.parent = parentHand;
        body.transform.localPosition = Vector3.zero;
        body.transform.localRotation = Quaternion.Euler(0,0,0);
        return body;
    }

    private void UpdateBodyObject(Body body, GameObject bodyObject)
    {
        // Update joints
        foreach (JointType _joint in _joints)
        {
            // Get new target position
            Joint sourceJoint = body.Joints[_joint];
            Vector3 targetPosition = GetVector3FromJoint(sourceJoint);
            targetPosition.z = 0;

            // Get joint, smooth to new position
            Transform jointObject = bodyObject.transform.Find(_joint.ToString());

            if (jointObject != null)
            {
                // Smoothing factor (adjustable, e.g., 5f = very smooth, 20f = very reactive)
                float smoothFactor = 10f;

                jointObject.localPosition = Vector3.Lerp(
                    jointObject.localPosition,
                    targetPosition * scaleMovement,
                    Time.deltaTime * smoothFactor
                );
            }
        }

        TryRaycastInteraction(bodyObject, true);
        TryRaycastInteraction(bodyObject, false);

    }


    private Vector3 GetVector3FromJoint(Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }

    public enum ActionHand
    {
        Click,
        Enter,
        Exit
    }

    private Dictionary<bool, ItemClick> hoveredItems = new Dictionary<bool, ItemClick>();
    private Dictionary<bool, Button> hoveredButtons = new Dictionary<bool, Button>();

    private void TryRaycastInteraction(GameObject bodyObject, bool handRight)
    {
        Transform hand = bodyObject.transform.Find(handRight ? "HandRight" : "HandLeft");
        if (hand == null) return;

        Vector3 screenPos = hand.position;
        Ray ray = Camera.main.ScreenPointToRay(screenPos);

        bool hitSomething = false;

        // 1. Raycast 3D pour ItemClick
        if (!GameManager.Instance.UIManager.IsShowingInfoImage)
        {
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                ItemClick item = hit.collider.GetComponent<ItemClick>();
                if (item != null)
                {
                    HandleItemHover(bodyObject, handRight, item, hand);
                    hitSomething = true;
                }
            }
        }

        // 2. Raycast UI pour Button
        if (GameManager.Instance.UIManager.IsShowingInfoImage)
        {
            PointerEventData data = new(EventSystem.current) { position = screenPos };
            List<RaycastResult> results = new();
            EventSystem.current.RaycastAll(data, results);

            Button button = results.Select(r => r.gameObject.GetComponent<Button>()).FirstOrDefault(b => b != null);
            if (button != null)
            {
                HandleButtonHover(bodyObject, handRight, button, data);
                hitSomething = true;
            }
        }

        // Aucun hit
        if (!hitSomething)
        {
            ClearHoverState(bodyObject, handRight, hand);
        }
    }

    private void HandleItemHover(GameObject bodyObject, bool handRight, ItemClick item, Transform hand)
    {
        if (!hoveredItems.TryGetValue(handRight, out var current) || current != item)
        {
            current?.ActionExit();
            hoveredItems[handRight] = item;
            hoverStartTime[handRight] = Time.time;
            item.ActionEnter();
            AudioManager.Instance.PlayInterfaceSound(selectSound);
            hand.GetChild(0).gameObject.SetActive(true);
            Cursor.SetCursor(closedMat.texture, Vector3.zero, CursorMode.Auto);
        }

        float duration = Time.time - hoverStartTime[handRight];
        FillHandAmount(bodyObject, handRight, Mathf.Clamp01(duration / 2f));
        if (duration >= .1f)
            AudioManager.Instance.PlayOnlyOneSoundStepEffect(selectTimedSound, duration / 2 + 1);

        if (duration >= 2f)
        {
            item.ActionClick();
            ChangeColorHands(bodyObject, Color.black);
            item.ActionExit();
            hoveredItems.Remove(handRight);
            hand.GetChild(0).gameObject.SetActive(false);
            FillHandAmount(bodyObject, handRight, 0);
            hoverStartTime[handRight] = Time.time;
        }
    }

    private void HandleButtonHover(GameObject bodyObject, bool handRight, Button button, PointerEventData data)
    {
        if (!hoveredButtons.TryGetValue(handRight, out var current) || current != button)
        {
            if (current != null)
                ExecuteEvents.Execute(current.gameObject, data, ExecuteEvents.pointerExitHandler);

            hoveredButtons[handRight] = button;
            hoverStartTime[handRight] = Time.time;
            AudioManager.Instance.PlayInterfaceSound(selectSound);

            ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.pointerEnterHandler);
        }

        float duration = Time.time - hoverStartTime[handRight];
        FillHandAmount(bodyObject, handRight, Mathf.Clamp01(duration / 2f));

        if(duration>=.1f)
            AudioManager.Instance.PlayOnlyOneSoundStepEffect(selectTimedSound, duration/2 + 1);

        if (duration >= 2f)
        {
            ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.pointerClickHandler);
            ChangeColorHands(bodyObject, Color.white);
            hoveredButtons.Remove(handRight);
            FillHandAmount(bodyObject, handRight, 0);
        }
    }


    private void ClearHoverState(GameObject bodyObject, bool handRight, Transform hand)
    {
        if (hoveredItems.TryGetValue(handRight, out var item))
        {
            item.ActionExit();
            hoveredItems.Remove(handRight);
        }

        if (hoveredButtons.TryGetValue(handRight, out var button))
        {
            ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerExitHandler);
            hoveredButtons.Remove(handRight);
        }

        hoverStartTime[handRight] = 0;
        FillHandAmount(bodyObject, handRight, 0);
        hand.GetChild(0).gameObject.SetActive(false);
        hand.GetComponent<Image>().sprite = openMat;
        Cursor.SetCursor(openMat.texture, Vector3.zero, CursorMode.Auto);
    }


    private void ChangeColorHands(GameObject bodyObject, Color color)
    {
        bodyObject.transform.Find("HandRight").GetComponent<Image>().color = color;
        bodyObject.transform.Find("HandLeft").GetComponent<Image>().color = color;
    }

    private void DefaultHandVisuel(GameObject bodyObject)
    {
        bodyObject.transform.Find("HandRight").GetChild(0).gameObject.SetActive(false);
        bodyObject.transform.Find("HandLeft").GetChild(0).gameObject.SetActive(false);
    }

    private void FillHandAmount(GameObject bodyObject, bool handRight, float amount)
    {
        if (handRight)
        {
            FillAmount(bodyObject.transform.Find("HandRight").GetComponentInChildren<Slider>(),amount);
        }
        else
        {
            FillAmount(bodyObject.transform.Find("HandLeft").GetComponentInChildren<Slider>(), amount);
        }
    }

    private void FillAmount(Slider slider, float amount)
    {
        slider.value = amount;
    }

    //private void ExitHandler(bool handRight, PointerEventData pointerData)
    //{
    //    if (handRight && currentItemClickRight != null)
    //    {
    //        currentItemClickRight.ActionExit();
    //        //ExecuteEvents.Execute(currentItemClickRight.gameObject, pointerData, ExecuteEvents.pointerExitHandler);
    //        currentItemClickRight = null;


    //    }
    //    else if (currentItemClickLeft != null)
    //    {
    //        currentItemClickLeft.ActionExit();
    //        //ExecuteEvents.Execute(currentItemClickLeft.gameObject, pointerData, ExecuteEvents.pointerExitHandler);
    //        currentItemClickLeft = null;
    //    }
    //}


    //private void ChangeHandState(GameObject bodyObject,HandState handState, bool handRight)
    //{

    //    if (handRight)
    //    {
    //        handRightPreviousState = handState;
    //    }
    //    else
    //    {
    //        handLeftPreviousState = handState;
    //    }

    //    Image imageHand = bodyObject.GetComponentsInChildren<Image>().
    //        SingleOrDefault(obj => obj.gameObject.name == (handRight ? "HandRight" : "HandLeft")); 

    //        switch (handState)
    //        {
    //            case HandState.Closed: imageHand.sprite = closedMat; break;
    //            case HandState.Open: imageHand.sprite = openMat; break;
    //            case HandState.Lasso: imageHand.sprite = lassoMat; break;
    //            case HandState.Unknown: imageHand.sprite = unknownMat; break;
    //        }
    //}
}