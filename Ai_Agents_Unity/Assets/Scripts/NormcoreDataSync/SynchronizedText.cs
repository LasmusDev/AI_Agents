using Normal.Realtime;
using NormcoreDataSync;
using TMPro;
using UnityEngine;

namespace NormcoreDataSync{
    
    public class SynchronizedText : RealtimeComponent<TextModel>
    {
        public TMP_Text synchronizedTextElement;

        protected override void OnRealtimeModelReplaced(TextModel oldModel, TextModel newModel)
        {
            if (oldModel != null)
            {
                // Unregister from events
                oldModel.containedTextDidChange -= TextChanged;
            }

            if (newModel != null)
            {
                // If this is a model that has no data set on it, populate it with the current mesh renderer color.
                if (newModel.isFreshModel)
                    newModel.containedText = synchronizedTextElement.text;


                // Register for events so we'll know if the color changes later
                newModel.containedTextDidChange += TextChanged;
            }
        }

        private void TextChanged(TextModel model, string text)
        {
            synchronizedTextElement.text = text;
        }
        public void SetText(string text)
        {
            // Set the color on the model
            // This will fire the colorChanged event on the model, which will update the renderer for both the local player and all remote players.
            model.containedText = text;
        }

    }


}
