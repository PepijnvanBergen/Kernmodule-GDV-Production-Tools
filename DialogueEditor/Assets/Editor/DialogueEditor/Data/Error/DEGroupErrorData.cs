using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace DE.Data.Error
{
    using Elements;
    public class DEGroupErrorData
    {
        public DEErrorData errorData { get; set; }
        public List<DEGroup> groups { get; set; }

        public DEGroupErrorData()
        {
            errorData = new DEErrorData();
            groups = new List<DEGroup>();
        }
    }
}