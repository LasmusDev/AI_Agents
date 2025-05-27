using System.Collections;
using UnityEngine;

public class MakeRigidbodyKinematic : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SetRBKinematic());
    }

    public IEnumerator SetRBKinematic()
    {
        yield return new WaitForSeconds(0.1f);
        this.GetComponent<Rigidbody>().isKinematic = true;
    }
}
