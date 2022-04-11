using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DE.ScriptableObjects
{
    public class DEDialogueContainerSO : ScriptableObject
    {
        [field: SerializeField] public string fileName { get; set; }
        [field: SerializeField] public SerializableDictionary<DEDialogueGroupSO, List<DEDialogueSO>> dialogueGroups { get; set; }
        [field: SerializeField] public List<DEDialogueSO> unGroupedDialogues { get; set; }

        public void Initialize(string _fileName)
        {
            fileName = _fileName;
            dialogueGroups = new SerializableDictionary<DEDialogueGroupSO, List<DEDialogueSO>>();
            unGroupedDialogues = new List<DEDialogueSO>();
        }
    }
}