using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerPoseRecorder))]
public class PlayerPoseRecorderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if(GUILayout.Button("Create Snapshot"))
        {
            PlayerPoseRecorder ppr = (PlayerPoseRecorder)target;
            ppr.RecordPoseSnapshot();
        }
        if (GUILayout.Button("Setup for UMA"))
        {
            PlayerPoseRecorder ppr = (PlayerPoseRecorder)target;
            ppr.AutoSetupForUMA();
        }
    }
}
