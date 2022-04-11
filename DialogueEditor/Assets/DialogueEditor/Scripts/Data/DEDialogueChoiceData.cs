using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DE.Data
{
    using ScriptableObjects;
    [Serializable]
    public class DEDialogueChoiceData
    {
        [field: SerializeField] public string text { get; set; }
        [field: SerializeField] public DEDialogueSO nextDialogue { get; set; }
    }
}
