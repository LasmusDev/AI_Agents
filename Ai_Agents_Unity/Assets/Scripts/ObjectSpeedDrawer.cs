using UnityEngine;

public class ObjectSpeedDrawer : MonoBehaviour
{
    public GraphDrawer drawingTo;
    public Vector3 lastPos = Vector3.zero;

    // Update is called once per frame
    void FixedUpdate()
    {
        float distanceThisFrame = Vector3.Distance(lastPos, this.transform.position);
        drawingTo.AddSample(distanceThisFrame*50 / Time.fixedDeltaTime);
        Debug.Log(distanceThisFrame / Time.fixedDeltaTime);
        lastPos = this.transform.position;
    }
}
