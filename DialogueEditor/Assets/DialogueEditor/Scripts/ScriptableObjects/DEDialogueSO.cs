using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DE.ScriptableObjects
{
    using Data;
    using Enumerations;
    public class DEDialogueSO : ScriptableObject
    {
        [field: SerializeField] public string dialogueName { get; set; }
        [field: SerializeField] public string text { get; set; }
        [field: SerializeField] public List<DEDialogueChoiceData> choices { get; set; }
        [field: SerializeField] public DEDialogueType dialogueType { get; set; }
        [field: SerializeField] public bool IsStartingDialogue { get; set; }
        public void Initialize(string _dialogueName, string _text, List<DEDialogueChoiceData> _choices, DEDialogueType _dialogueType, bool _IsStartingDialogue)
        {
            dialogueName = _dialogueName;
            text = _text;
            choices = _choices;
            dialogueType = _dialogueType;
            IsStartingDialogue = _IsStartingDialogue;
        }
    }
}