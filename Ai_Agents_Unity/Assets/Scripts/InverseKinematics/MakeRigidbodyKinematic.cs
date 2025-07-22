using System.Collections;
using UnityEngine;

public class MakeRigidbodyKinematic : MonoBehaviour
{
    public AnimatorOverrideController toAutoSetAnimatorController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SetRBKinematic());
    }

    public IEnumerator SetRBKinematic()
    {
        yield return new WaitForSeconds(0.5f);
        Rigidbody rb = this.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        Collider attachedCollider = GetComponent<Collider>();
        if(attachedCollider != null)
        {
            attachedCollider.enabled = false;
        }

    }

    public void Update()
    {
        if (GetComponent<Animator>().runtimeAnimatorController != toAutoSetAnimatorController)
        {
            Debug.LogWarning("Exchanged Animator controller on avatar");
            GetComponent<Animator>().runtimeAnimatorController = toAutoSetAnimatorController;
        }
    }
}
