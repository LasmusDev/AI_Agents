using UnityEngine;

public class RootCalculator : MonoBehaviour
{
    public GameObject lHand;
    public GameObject rHand;
    [Range(-1f, 1f)] public float dotTresholdForShift;    // Update is called once per frame
    void Update()
    {
        Vector3 camPos = Camera.main.transform.position;
        this.transform.position = new Vector3(camPos.x, 0, camPos.z);
        
        Vector3 betweenHands = Vector3.Lerp(lHand.transform.position, rHand.transform.position, 0.5f);
        betweenHands = new Vector3(betweenHands.x, 0, betweenHands.z);
        this.transform.LookAt(betweenHands);
        if (Vector3.Dot(Camera.main.transform.forward, Vector3.up) < dotTresholdForShift)
        {
            this.transform.position = this.transform.position - this.transform.forward * 0.15f;
        }
    }
}
