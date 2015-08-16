using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSMToSCT2.Geo
{
    public class Shape
    {
        public enum ShapeType
        {
            Runway,
            Taxiway,
            Apron,
            Terminal,
            Hangar,
            Other1
        }

        private List<Node> mNodes;

        public Shape()
        {
            mNodes = new List<Node>();
        }

        public int ID
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public ShapeType Type
        {
            get;
            set;
        }

        public List<Node> Nodes
        {
            get { return mNodes; }
        }
    }
}
