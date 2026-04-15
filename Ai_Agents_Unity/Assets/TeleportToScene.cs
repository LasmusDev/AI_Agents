using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportToScene : MonoBehaviour
{
    public string sceneName;
    void OnTriggerEnter(Collider other)
    {
        SceneManager.LoadScene(sceneName); 
    }
}
