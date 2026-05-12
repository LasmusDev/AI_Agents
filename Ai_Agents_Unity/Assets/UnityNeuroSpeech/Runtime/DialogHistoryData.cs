using System;
using System.Collections.Generic;

namespace UnityNeuroSpeech.Runtime
{
    [Serializable]
    public struct DialogData
    {
        public string userMessage, llmResponse;

        public DialogData(string userMessage, string llmResponse)
        {
            this.userMessage = userMessage;
            this.llmResponse = llmResponse;
        }
    }

    [Serializable]
    public struct DialogHistoryData
    {
        public List<DialogData> dialogHistory;

        public DialogHistoryData(List<DialogData> dialogHistory) => this.dialogHistory = dialogHistory;
    }
}