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
    private HandState handLeftPreviousState;
    private HandState handRightPreviousState;

    private Dictionary<ulong, GameObject> mBodies = new Dictionary<ulong, GameObject>();

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

        //foreach (var body in data)
        //{
        //    // If no body, skip
        //    if (body == null)
        //        continue;

        //    if (body.IsTracked)
        //    {
        //        // If body isn't tracked, create body
        //        if (!mBodies.ContainsKey(body.TrackingId))
        //            mBodies[body.TrackingId] = CreateBodyObject(body.TrackingId);

        //        // Update positions
        //        UpdateBodyObject(body, mBodies[body.TrackingId]);
        //    }
        //}

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
    private float handLeftClosedTime = 0f;
    private float handRightClosedTime = 0f;
    private bool isHandLeftClosed = false;
    private bool isHandRightClosed = false;
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

        // Vérifier les mains avec délai
        CheckHandState(body.HandLeftState, ref isHandLeftClosed, ref handLeftClosedTime, bodyObject, false);
        CheckHandState(body.HandRightState, ref isHandRightClosed, ref handRightClosedTime, bodyObject, true);
    }

    private void CheckHandState(HandState handState, ref bool isHandClosed, ref float handClosedTime, GameObject bodyObject, bool isRightHand)
    {
        if (handState == HandState.Closed)
        {
            if (!isHandClosed)
            {
                handClosedTime = Time.time;
                isHandClosed = true;
            }
            FillHandAmount(bodyObject, isRightHand, Time.time - handClosedTime);
            // Si la main est fermée depuis au moins 1 seconde
            if (Time.time - handClosedTime >= 1.0f)
            {
                SendRaycastButton(bodyObject, isRightHand);
                SendRaycastItemClick(bodyObject, isRightHand, ActionHand.Click);                
                ChangeHandState(bodyObject, HandState.Closed, isRightHand);
            }
        }
        else
        {
            isHandClosed = false;
            handClosedTime = 0f;
            FillHandAmount(bodyObject, isRightHand, 0);
            SendRaycastItemClick(bodyObject, isRightHand, ActionHand.Enter);
            if (handState == HandState.Open)
            {
                ChangeHandState(bodyObject, HandState.Open, isRightHand);
            }
            else if (handState == HandState.Lasso)
            {
                ChangeHandState(bodyObject, HandState.Lasso, isRightHand);
            }
            else
            {
                ChangeHandState(bodyObject, HandState.Unknown, isRightHand);
            }
        }
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

    public void SendRaycastItemClick(GameObject bodyObject, bool handRight, ActionHand actionHand)
    {
        Transform handTransform = bodyObject.transform.Find(handRight ? "HandRight" : "HandLeft");

        // Nettoyer les entrées avec objets détruits
        List<bool> handsToRemove = new List<bool>();
        foreach (var kvp in hoveredItems)
        {
            if (kvp.Value == null)
                handsToRemove.Add(kvp.Key);
        }
        foreach (var hand in handsToRemove)
        {
            hoveredItems.Remove(hand);
        }

        if (handTransform == null)
        {
            Debug.LogWarning("Main UI non trouvée !");
            return;
        }

        Vector3 screenPosition = handTransform.position;
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            ItemClick hitItem = hit.collider.GetComponent<ItemClick>();

            if (hitItem != null)
            {
                // Si cette main survolait un autre item, le désactiver
                if (hoveredItems.TryGetValue(handRight, out var hovered) && hovered != null)
                {
                    hovered.ActionExit();
                    hoveredItems.Remove(handRight);
                }

                // Mémoriser le nouvel item hoveré
                hoveredItems[handRight] = hitItem;

                switch (actionHand)
                {
                    case ActionHand.Click:
                        hitItem.ActionClick();
                        DefaultHandVisuel(bodyObject);
                        ChangeColorHands(bodyObject, Color.black);
                        break;

                    case ActionHand.Enter:
                        if (!GameManager.Instance.UIManager.IsShowingInfoImage)
                        {
                            hitItem.ActionEnter();
                            handTransform.GetChild(0).gameObject.SetActive(true);
                            Cursor.SetCursor(closedMat.texture, Vector3.zero, CursorMode.Auto);
                        }
                        break;
                }

                return; // Pas besoin d’appeler ClearItemHover
            }
        }

        // Si aucun item touché ou plus de hit => on quitte le hover
        ClearItemHover(handRight, handTransform);
    }

    private void ClearItemHover(bool handRight, Transform handTransform)
    {
        if (hoveredItems.TryGetValue(handRight, out var hovered))
        {
            hovered.ActionExit();
            hoveredItems.Remove(handRight);
        }

        handTransform.GetChild(0).gameObject.SetActive(false);
        handTransform.GetComponent<Image>().sprite = openMat;
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


    private void SendRaycastButton(GameObject bodyObject, bool handRight)
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = bodyObject.transform.Find(handRight ? "HandRight" : "HandLeft").position;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        foreach (RaycastResult result in results)
        {
            Debug.Log(results);
            Button button = result.gameObject.GetComponent<Button>();
            if (button != null)
            {
                ExecuteEvents.Execute(button.gameObject, pointerData, ExecuteEvents.pointerClickHandler);
                ChangeColorHands(bodyObject, Color.white);
                break;
            }
            
        }
    }

    //var rend = bodyObject.GetComponentsInChildren<Renderer>()
    //           .SingleOrDefault(obj => obj.gameObject.name == "HandRight");
    //        if (rend != null) rend.material = unknownMat;
    private void ChangeHandState(GameObject bodyObject,HandState handState, bool handRight)
    {
        
        if (handRight)
        {
            handRightPreviousState = handState;
        }
        else
        {
            handLeftPreviousState = handState;
        }
        
        Image imageHand = bodyObject.GetComponentsInChildren<Image>().
            SingleOrDefault(obj => obj.gameObject.name == (handRight ? "HandRight" : "HandLeft")); 
     
            switch (handState)
            {
                case HandState.Closed: imageHand.sprite = closedMat; break;
                case HandState.Open: imageHand.sprite = openMat; break;
                case HandState.Lasso: imageHand.sprite = lassoMat; break;
                case HandState.Unknown: imageHand.sprite = unknownMat; break;
            }
    }
}