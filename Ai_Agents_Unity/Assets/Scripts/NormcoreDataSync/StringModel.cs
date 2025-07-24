using UnityEngine;

namespace NormcoreDataSync {

    [RealtimeModel]
    public partial class TextModel
    {
        [RealtimeProperty(1, true, true)] 
        private string _containedText;
    }

}
