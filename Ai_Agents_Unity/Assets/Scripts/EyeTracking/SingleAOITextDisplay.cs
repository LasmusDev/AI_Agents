using UnityEngine;
using TMPro; // Oder using UnityEngine.UI; falls du das alte UI.Text verwendest

/// <summary>
/// Zeigt die Blickzeit für ein SPECIFISCHES Area of Interest an.
/// Dieses Skript muss auf einem GameObject platziert werden, das eine TextMeshProUGUI-Komponente hat.
/// </summary>
public class SingleAOITextDisplay : MonoBehaviour
{
    [Tooltip("Der genaue Name des AOI-Objekts, dessen Zeit dieses Textfeld anzeigen soll.")]
    public string targetAOIName; // Dies wird im Editor zugewiesen!

    private TextMeshProUGUI displayTextMesh; // Die Text-Komponente, die aktualisiert wird.
    // Oder: private Text displayText; falls du UnityEngine.UI.Text verwendest.

    void Awake()
    {
        displayTextMesh = GetComponent<TextMeshProUGUI>();
        // Oder: displayText = GetComponent<Text>();

        if (displayTextMesh == null) // Oder displayText == null
        {
            Debug.LogError($"SingleAOITextDisplay auf '{gameObject.name}': Keine TextMeshProUGUI-Komponente gefunden!", this);
            enabled = false; // Deaktiviert das Skript, wenn keine Text-Komponente gefunden wurde.
            return;
        }

        if (string.IsNullOrEmpty(targetAOIName))
        {
            Debug.LogWarning($"SingleAOITextDisplay auf '{gameObject.name}': 'Target AOI Name' wurde nicht zugewiesen! Dieses Textfeld wird keine Daten anzeigen.", this);
        }

        // Initialisiere den Text (optional)
        displayTextMesh.text = $"{targetAOIName}: 0.00s";
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
        if (aoiName == targetAOIName)
        {
            if (displayTextMesh != null)
            {
                displayTextMesh.text = $"{aoiName}: {gazeTime:F2}s";
            }
        }
    }
}