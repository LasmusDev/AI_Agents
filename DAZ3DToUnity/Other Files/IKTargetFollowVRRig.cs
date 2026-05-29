/* using UnityEngine;

[System.Serializable]
public class VRMap
{
    public Transform vrTarget;
    public Transform ikTarget;
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;

    public void Map()
    {
        ikTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
        ikTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
    }
}

public class IKTargetFollowVRRig : MonoBehaviour
{
    [Range(0, 1)]
    public float turnSmoothness = 0.1f;

    public VRMap head;
    public VRMap leftHand;
    public VRMap rightHand;

    public Vector3 headBodyPositionOffset;
    public float headBodyYawOffset;

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = head.ikTarget.position + headBodyPositionOffset;
        
        float yaw = head.vrTarget.eulerAngles.y;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.eulerAngles.x, yaw, transform.eulerAngles.z), turnSmoothness);

        head.Map();
        leftHand.Map();
        rightHand.Map();
    }
} */

//Normcore version
 using UnityEngine;
//using Normal.Realtime;

[System.Serializable]
public class VRMap
{
    public Transform vrTarget; 
    public Transform ikTarget; 
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;

    public void Map()
    {
        if (vrTarget == null || ikTarget == null) return;

        ikTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
        ikTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
    }
}
 
public class IKTargetFollowVRRig : MonoBehaviour
{
    [Header("Normcore Setup")]
    //public RealtimeView realtimeView;  

    [Header("Settings")]
    [Range(0, 1)]
    public float turnSmoothness = 0.1f;

    public VRMap head;
    public VRMap leftHand;
    public VRMap rightHand;

    public Vector3 headBodyPositionOffset;
    public float headBodyYawOffset;

    
    void Start()
    {
      
         //if (realtimeView != null && realtimeView.isOwnedLocallyInHierarchy)
        //{
        //    FindHardware();
        //} 
    }

    void LateUpdate()
    {
       
        //if (realtimeView != null && !realtimeView.isOwnedLocallyInHierarchy) return;
        if (head.vrTarget == null) FindHardware();
        if (head.vrTarget == null) return; 

       
        transform.position = head.ikTarget.position + headBodyPositionOffset;
        
        float yaw = head.vrTarget.eulerAngles.y;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.eulerAngles.x, yaw, transform.eulerAngles.z), turnSmoothness);

       
        head.Map();
        leftHand.Map();
        rightHand.Map();
    }

    void FindHardware()
    {
        
        if (Camera.main != null) 
            head.vrTarget = Camera.main.transform;

        
        GameObject leftController = GameObject.Find("Left Controller");
        if (leftController != null) leftHand.vrTarget = leftController.transform;

        GameObject rightController = GameObject.Find("Right Controller");
        if (rightController != null) rightHand.vrTarget = rightController.transform;
    }
} 