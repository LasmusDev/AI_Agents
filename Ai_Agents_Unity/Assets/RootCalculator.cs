using UnityEngine;

public class RootCalculator : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        Vector3 camPos = Camera.main.transform.position;
        this.transform.position = new Vector3(camPos.x, 0, camPos.z);
    }
}
