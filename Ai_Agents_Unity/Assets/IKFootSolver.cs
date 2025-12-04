using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKFootSolver : MonoBehaviour
{
    [SerializeField] LayerMask terrainLayer = default;
    [SerializeField] Transform body = default;
    [SerializeField] IKFootSolver otherFoot = default;
    [SerializeField] float speed = 1;
    [SerializeField] float stepDistance = 4;
    [SerializeField] float stepLength = 4;
    [SerializeField] float stepHeight = 1;
    [SerializeField] Vector3 footOffset = default;
    float footSpacing;
    Vector3 oldPosition, currentPosition, newPosition;
    Vector3 oldNormal, currentNormal, newNormal;
    float lerp;
    Quaternion oldRotation, currentRotation, newRotation;

    private void Start()
    {
        footSpacing = transform.localPosition.x;
        currentPosition = newPosition = oldPosition = transform.position;
        currentNormal = newNormal = oldNormal = transform.up;
        lerp = 1;

        
        Vector3 forwardOnPlane = Vector3.ProjectOnPlane(body.forward, currentNormal);
        if (forwardOnPlane.sqrMagnitude < 0.0001f) forwardOnPlane = transform.forward;
        oldRotation = currentRotation = newRotation = Quaternion.LookRotation(forwardOnPlane, currentNormal);
    }

    

    void Update()
    {
        transform.position = currentPosition;
        
        transform.rotation = currentRotation;

        Ray ray = new Ray(body.position + (body.right * footSpacing), Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit info, 10, terrainLayer.value))
        {
            if (Vector3.Distance(newPosition, info.point) > stepDistance && !otherFoot.IsMoving() && lerp >= 1)
            {
                lerp = 0;
                int direction = body.InverseTransformPoint(info.point).z > body.InverseTransformPoint(newPosition).z ? 1 : -1;
                newPosition = info.point + (body.forward * stepLength * direction) + footOffset;
                newNormal = info.normal;

                // neue Zielrotation berechnen: Körper-Forward auf die Bodenebene projizieren
                Vector3 targetForward = Vector3.ProjectOnPlane(body.forward, newNormal);
                if (targetForward.sqrMagnitude < 0.0001f) targetForward = transform.forward;
                newRotation = Quaternion.LookRotation(targetForward, newNormal);
            }
        }

        if (lerp < 1)
        {
            Vector3 tempPosition = Vector3.Lerp(oldPosition, newPosition, lerp);
            tempPosition.y += Mathf.Sin(lerp * Mathf.PI) * stepHeight;

            currentPosition = tempPosition;
            currentNormal = Vector3.Lerp(oldNormal, newNormal, lerp);

          
            currentRotation = Quaternion.Slerp(oldRotation, newRotation, lerp);

            lerp += Time.deltaTime * speed;
        }
        else
        {
            oldPosition = newPosition;
            oldNormal = newNormal;

           
            oldRotation = newRotation;
            currentRotation = newRotation;
        }
    }
    private void OnDrawGizmos()
    {

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(newPosition, 0.5f);
    }



    public bool IsMoving()
    {
        return lerp < 1;
    }
}