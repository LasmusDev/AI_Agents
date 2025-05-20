using UMA;
using UnityEditor;
using UnityEngine;

public class PlayerPoseRecorder : MonoBehaviour
{
    public bool recordHead;
    public bool recordHands;
    public bool recordFeet;
    public float poseTolerance;
    public string poseName;

    public GameObject headObject;
    public GameObject lHandObject;
    public GameObject rHandObject;
    public GameObject lFootObject;
    public GameObject rFootObject;

    public GameObject UMABase;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AutoSetupForUMA()
    {
        headObject = UMABase.transform.FindRecursive("Head").gameObject;
        lHandObject = UMABase.transform.FindRecursive("LeftHand").gameObject;
        rHandObject = UMABase.transform.FindRecursive("RightHand").gameObject;
        lFootObject = UMABase.transform.FindRecursive("LeftFoot").gameObject;
        rFootObject = UMABase.transform.FindRecursive("RightFoot").gameObject;
    }

    public void RecordPoseSnapshot()
    {
        PlayerPose playerPose = ScriptableObject.CreateInstance<PlayerPose>();
        playerPose.Init();
        if (recordHead)
        {
            LimbRequirement headRequirement = new LimbRequirement();
            headRequirement.limb = Limb.HEAD;
            headRequirement.relativePos = headObject.transform.position;
            headRequirement.tolerance = poseTolerance;
            playerPose.limbRequirements.Add(headRequirement);
        }
        if (recordHands)
        {
            LimbRequirement rHandReq = new LimbRequirement();
            rHandReq.limb = Limb.RHAND;
            rHandReq.relativePos = rHandObject.transform.position;
            rHandReq.tolerance = poseTolerance;
            playerPose.limbRequirements.Add(rHandReq);
            LimbRequirement lHandReq = new LimbRequirement();
            lHandReq.limb = Limb.LHAND;
            lHandReq.relativePos = lHandObject.transform.position;
            lHandReq.tolerance = poseTolerance;
            playerPose.limbRequirements.Add(lHandReq);
        }
        if (recordFeet)
        {
            LimbRequirement rFootReq = new LimbRequirement();
            rFootReq.limb = Limb.RFOOT;
            rFootReq.relativePos = lFootObject.transform.position;
            rFootReq.tolerance = poseTolerance;
            playerPose.limbRequirements.Add(rFootReq);
            LimbRequirement lFootReq = new LimbRequirement();
            lFootReq.limb = Limb.LFOOT;
            lFootReq.relativePos = lFootObject.transform.position;
            lFootReq.tolerance = poseTolerance;
            playerPose.limbRequirements.Add(lFootReq);
        }
        //Ignore asset database warning. It wont work outside of the editor, but it wont need to.
        if(AssetDatabase.AssetPathExists("Assets/Poses/" + poseName + ".asset"))
        {
            poseName += "_";
        }
        AssetDatabase.CreateAsset(playerPose, "Assets/Poses/" + poseName + ".asset");
    }
}
