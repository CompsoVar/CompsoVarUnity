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
            //targetPosition.z = Camera.main.transform.position.z + 3;
            targetPosition.z = 0;
            // Get joint, set new position
            Transform jointObject = bodyObject.transform.Find(_joint.ToString());
            jointObject.localPosition = targetPosition * scaleMovement;
        }

        if (body.HandLeftState == HandState.Closed)
        {

            if (handLeftPreviousState != HandState.Closed)
            {
                SendRaycastButton(bodyObject, false);
                SendRaycastItemClick(bodyObject, false);
            }

            ChangeHandState(bodyObject,HandState.Closed, false);

        }
        else if (body.HandLeftState == HandState.Open)
        {

            ChangeHandState(bodyObject, HandState.Open, false);

        }
        else if (body.HandLeftState == HandState.Lasso)
        {

            ChangeHandState(bodyObject, HandState.Lasso, false);
        }
        else
        {
            ChangeHandState(bodyObject, HandState.Unknown, false);
        }

        if (body.HandRightState == HandState.Closed)
        {

            if (handRightPreviousState != HandState.Closed)
            {
                SendRaycastButton(bodyObject, true);
                SendRaycastItemClick(bodyObject, true);
            }

            ChangeHandState(bodyObject, HandState.Closed, true);
        }
        else if (body.HandRightState == HandState.Open)
        {

            ChangeHandState(bodyObject, HandState.Open, true);

        }
        else if (body.HandRightState == HandState.Lasso)
        {

            ChangeHandState(bodyObject, HandState.Open, true);

        }
        else
        {
            ChangeHandState(bodyObject, HandState.Unknown, true);

        }
    }

    private Vector3 GetVector3FromJoint(Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }

    public void SendRaycastItemClick(GameObject bodyObject, bool handRight)
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
                ExecuteEvents.Execute(itemClick.gameObject, pointerData, ExecuteEvents.pointerClickHandler);
            }
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