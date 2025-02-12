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

        foreach (var body in data)
        {
            // If no body, skip
            if (body == null)
                continue;

            if (body.IsTracked)
            {
                // If body isn't tracked, create body
                if (!mBodies.ContainsKey(body.TrackingId))
                    mBodies[body.TrackingId] = CreateBodyObject(body.TrackingId);

                // Update positions
                UpdateBodyObject(body, mBodies[body.TrackingId]);
            }
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

            // Get joint, set new position
            Transform jointObject = bodyObject.transform.Find(_joint.ToString());
            jointObject.localPosition = targetPosition * scaleMovement;
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
            if (handState == HandState.Open)
            {
                SendRaycastItemClick(bodyObject, isRightHand, ActionHand.Enter);
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

    private ItemClick currentItemClickRight = null;
    private ItemClick currentItemClickLeft = null;
    public void SendRaycastItemClick(GameObject bodyObject, bool handRight, ActionHand actionHand)
    {
        // Trouver la main dans le modèle Kinect
        Transform handTransform = bodyObject.transform.Find(handRight ? "HandRight" : "HandLeft");
        if (handTransform == null)
        {
            Debug.LogWarning("Main UI non trouvée !");
            return;
        }

        // Récupérer la position de la main dans le Canvas (écran)
        Vector3 screenPosition = handTransform.position;

        // Tirer un rayon depuis la caméra à cette position écran
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);

        // Visualiser le rayon dans la scène Unity
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 1.0f);

        // Effectuer le Raycast
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            Debug.Log("Touching object: " + hit.collider.gameObject.name);

            // Vérifier si l'objet touché a un ItemClick
            ItemClick itemClick = hit.collider.GetComponent<ItemClick>();
            if (itemClick != null)
            {
                // Simuler un clic sur l'objet
                PointerEventData pointerData = new PointerEventData(EventSystem.current);

                ExitHandler(handRight, pointerData);
                if (handRight)
                {
                    currentItemClickRight = itemClick;
                }
                else
                {
                    currentItemClickLeft = itemClick;
                }
               
                switch (actionHand)
                {
                    case ActionHand.Click: 
                        ExecuteEvents.Execute(itemClick.gameObject, pointerData, ExecuteEvents.pointerClickHandler);
                        DefaultHandVisuel(bodyObject);
                        ChangeColorHands(bodyObject,Color.black);
                        break;
                    case ActionHand.Enter:
                        if (!GameManager.Instance.UIManager.IsShowingInfoImage)
                        {
                            handTransform.GetChild(0).gameObject.SetActive(true);
                            Cursor.SetCursor(closedMat.texture, Vector3.zero, CursorMode.Auto);
                            ExecuteEvents.Execute(itemClick.gameObject, pointerData, ExecuteEvents.pointerEnterHandler);
                        }
                        break;
                }
            }
            else
            {
                handTransform.GetChild(0).gameObject.SetActive(false);
                PointerEventData pointerData = new PointerEventData(EventSystem.current);

                ExitHandler(handRight, pointerData);

                handTransform.GetComponent<Image>().sprite = openMat;
                Cursor.SetCursor(openMat.texture, Vector3.zero, CursorMode.Auto);

            }
        }
        else
        {
            handTransform.GetChild(0).gameObject.SetActive(false);
        }

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

    private void ExitHandler(bool handRight, PointerEventData pointerData)
    {
        if (handRight && currentItemClickRight != null)
        {
            ExecuteEvents.Execute(currentItemClickRight.gameObject, pointerData, ExecuteEvents.pointerExitHandler);
            currentItemClickRight = null;


        }
        else if (currentItemClickLeft != null)
        {
            ExecuteEvents.Execute(currentItemClickLeft.gameObject, pointerData, ExecuteEvents.pointerExitHandler);
            currentItemClickLeft = null;

        }
    }


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