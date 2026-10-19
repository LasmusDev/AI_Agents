using UnityEditor;
using UnityEngine;


public class TreeConverter : MonoBehaviour
{
#if UNITY_EDITOR
    [ContextMenu("Convert")]
    public void ConvertTrees()
    {
        ParticleSystem[] treeRoots = GetComponentsInChildren<ParticleSystem>();
        foreach (var particleSystem in treeRoots)
        {
            if (particleSystem.GetComponent<LODGroup>() != null)
            {
                break; //This tree is already converted
            }
            GameObject newTreeRoot = particleSystem.GetComponentInChildren<LODGroup>().gameObject;
            newTreeRoot.AddComponent<ParticleSystem>();
            UnityEditor.EditorUtility.CopySerialized(particleSystem, newTreeRoot.GetComponent<ParticleSystem>());
            if (UnityEditor.PrefabUtility.IsPartOfAnyPrefab(particleSystem.gameObject))
            {
                UnityEditor.PrefabUtility.UnpackPrefabInstance(particleSystem.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
            PrefabUtility.SaveAsPrefabAsset(newTreeRoot, "Assets/Realistic Tree/Prefabs/URP/URP Trees for Terrain/" + newTreeRoot.name + ".prefab");
        }
    }
#endif
}

