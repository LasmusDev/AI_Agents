using UnityEngine;
using System; // Wichtig für Action

/// <summary>
/// Statische Klasse zur Verwaltung von Events im Zusammenhang mit Blickdaten.
/// </summary>
public static class GazeDataEvents
{
    // NEU: Event, das ausgelöst wird, wenn die Blickzeit EINES bestimmten AOIs aktualisiert wird.
    // Es übergibt den Namen des AOIs und seine aktualisierte Verweildauer.
    public static event Action<string, float> OnSingleAOITimeUpdated;

    /// <summary>
    /// Löst das OnSingleAOITimeUpdated-Event für ein spezifisches AOI aus.
    /// </summary>
    /// <param name="aoiName">Der Name des Area of Interest-Objekts.</param>
    /// <param name="gazeTime">Die aktuelle Verweildauer auf diesem AOI.</param>
    public static void TriggerSingleAOITimeUpdated(string aoiName, float gazeTime)
    {
        OnSingleAOITimeUpdated?.Invoke(aoiName, gazeTime);
    }

    // Falls du den alten Gesamt-Text noch irgendwo anders nutzen willst, kannst du das alte Event behalten:
    // public static event Action<string> OnAllAOITimesUpdated;
    // public static void TriggerAllAOITimesUpdated(string formattedText)
    // {
    //     OnAllAOITimesUpdated?.Invoke(formattedText);
    // }
}