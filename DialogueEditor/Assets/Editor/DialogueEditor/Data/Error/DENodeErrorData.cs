using System.Collections.Generic;

namespace DE.Data.Error
{
    using Elements;
    public class DENodeErrorData
    {
        public DEErrorData errorData { get; set; }
        public List<DENode> nodes { get; set; }
        public DENodeErrorData()
        {
            errorData = new DEErrorData();
            nodes = new List<DENode>();
        }
    }
}