using UnityEngine;
using TMPro; // Oder using UnityEngine.UI; falls du das alte UI.Text verwendest
using NormcoreDataSync;

/// <summary>
/// Zeigt die Blickzeit für ein SPECIFISCHES Area of Interest an.
/// Dieses Skript muss auf einem GameObject platziert werden, das eine TextMeshProUGUI-Komponente hat.
/// </summary>
public class SingleAOITextDisplay : MonoBehaviour
{
    [Tooltip("Der genaue Name des AOI-Objekts, dessen Zeit dieses Textfeld anzeigen soll.")]
    public GameObject targetAOI;

    public SynchronizedText toSync; // Die Text-Komponente, die aktualisiert wird.
    // Oder: private Text displayText; falls du UnityEngine.UI.Text verwendest.

    public void Update()
    {
       // toSync.SetText(targetAOI.name + "1"); //Debug
    }

    void OnEnable()
    {
        // Abonniere das Event, sobald dieses GameObject aktiviert wird.
        GazeDataEvents.OnSingleAOITimeUpdated += HandleAOITimeUpdate;
        Debug.Log($"SingleAOITextDisplay auf '{gameObject.name}': Event 'OnSingleAOITimeUpdated' abonniert.");
    }

    void OnDisable()
    {
        // Kündige das Abonnement, sobald dieses GameObject deaktiviert wird,
        // um Memory Leaks zu vermeiden. SEHR WICHTIG!
        GazeDataEvents.OnSingleAOITimeUpdated -= HandleAOITimeUpdate;
        Debug.Log($"SingleAOITextDisplay auf '{gameObject.name}': Event 'OnSingleAOITimeUpdated' gekündigt.");
    }

    /// <summary>
    /// Diese Methode wird aufgerufen, wenn das OnSingleAOITimeUpdated-Event ausgelöst wird.
    /// </summary>
    /// <param name="aoiName">Der Name des AOI, das aktualisiert wurde.</param>
    /// <param name="gazeTime">Die aktuelle Verweildauer auf diesem AOI.</param>
    private void HandleAOITimeUpdate(string aoiName, float gazeTime)
    {
        // Prüfen, ob der Event-Name mit dem Namen dieses spezifischen AOI-Displays übereinstimmt.
        if (aoiName == targetAOI.name)
        {         
           toSync.SetText($"{gazeTime:F2}s");
        }
    }
}