using UnityEngine;

public class Mathtester : MonoBehaviour
{
    public GameObject testPosition;
    public GameObject playerOrigin;
    public GameObject projectedPosition;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        projectedPosition.transform.position = playerOrigin.transform.localToWorldMatrix.MultiplyPoint3x4(testPosition.transform.position);
    }
}
