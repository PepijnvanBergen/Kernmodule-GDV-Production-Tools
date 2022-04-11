using System;
using System.Collections.Generic;
using UnityEngine;

namespace DE.Data.Save
{
    [Serializable]
    public class DEGraphSaveDataSO : ScriptableObject
    {
        [field: SerializeField] public string fileName { get; set; }
        [field: SerializeField] public List<DEGroupSaveData> groups { get; set; }
        [field: SerializeField]  public List<DENodeSaveData> nodes { get; set; }
        [field: SerializeField] public List<string> oldGroupNames { get; set; }
        [field: SerializeField] public List<string> oldUngroupedNodeNames { get; set; }
        [field: SerializeField] public SerializableDictionary<string, List<string>> oldGroupedNodeNames { get; set; }

        public void Initialize(string _fileName)
        {
            fileName = _fileName;
            groups = new List<DEGroupSaveData>();
            nodes = new List<DENodeSaveData>();
        }
    }
}