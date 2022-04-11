using System;
using UnityEngine;

namespace DE.Data.Save
{
    [Serializable]
    public class DEGroupSaveData
    {
        [field: SerializeField] public string id { get; set; }
        [field: SerializeField] public string name { get; set; }
        [field: SerializeField] public Vector2 position { get; set; }
    }
}