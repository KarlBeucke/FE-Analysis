using FEALibrary.Model.abstractClasses;

namespace FE_Analysis.Heat_Transfer.Model_Data
{
    public class NodeLoad : AbstractNodeLoad
    {
        private int[] systemIndices;

        // ....Constructor....................................................
        public NodeLoad(string nodeId)
        {
            NodeId = nodeId;
        }

        public NodeLoad(string nodeId, double[] stream)
        {
            NodeId = nodeId;
            Loadvalues = stream;
        }

        public NodeLoad(string id, string nodeId)
        {
            LoadId = id;
            NodeId = nodeId;
        }

        public NodeLoad(string id, string nodeId, double[] stream)
        {
            LoadId = id;
            NodeId = nodeId;
            Loadvalues = stream;
        }

        // ....Compute the system indices of a node ..............................
        public int[] ComputeSystemIndices()
        {
            systemIndices = Node.SystemIndices;
            return systemIndices;
        }

        public override double[] ComputeLoadVector()
        {
            return Loadvalues;
        }
    }
}