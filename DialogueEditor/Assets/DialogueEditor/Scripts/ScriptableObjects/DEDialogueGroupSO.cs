using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DE.ScriptableObjects
{
    public class DEDialogueGroupSO : ScriptableObject
    {
        [field: SerializeField] public string groupName { get; set; }

        public void Initialize(string _groupName)
        {
            groupName = _groupName;
        }
    }
}
