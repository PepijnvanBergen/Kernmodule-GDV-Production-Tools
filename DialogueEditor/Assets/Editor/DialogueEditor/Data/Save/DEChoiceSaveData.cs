 using System;
using UnityEngine;

namespace DE.Data.Save
{
    [Serializable]
    public class DEChoiceSaveData
    {
        [field: SerializeField] public string text { get; set; }
        [field: SerializeField] public string nodeID { get; set; }

    }
}
