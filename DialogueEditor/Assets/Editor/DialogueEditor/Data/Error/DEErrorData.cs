using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DE.Data.Error
{
    public class DEErrorData
    {
        public Color color { get; set; }
        public DEErrorData()
        {
            GenerateRandomColor();
        }
        private  void GenerateRandomColor()
        {
            color = new Color32
                (
                    (byte) Random.Range(65, 256),
                    (byte) Random.Range(50,176),
                    (byte) Random.Range(50, 176),
                    255
                );
        }
    }
}