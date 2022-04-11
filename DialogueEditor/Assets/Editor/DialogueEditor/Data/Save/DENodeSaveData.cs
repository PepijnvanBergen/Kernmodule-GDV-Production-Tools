using System;
using System.Collections.Generic;
using UnityEngine;

namespace DE.Data.Save
{
    using Enumerations;
    [Serializable]
    public class DENodeSaveData
    {
        [field: SerializeField] public string id { get; set; }
        [field: SerializeField] public string name { get; set; }
        [field: SerializeField] public string text { get; set; }
        [field: SerializeField] public List<DEChoiceSaveData> choices { get; set; }
        [field: SerializeField] public string groupID { get; set; }
        [field: SerializeField] public DEDialogueType dialogueType { get; set; }
        [field: SerializeField] public Vector2 position { get; set; }
    }
}
