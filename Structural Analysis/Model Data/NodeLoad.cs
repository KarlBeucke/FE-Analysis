using FEALibrary.Model.abstractClasses;

namespace FE_Analysis.Structural_Analysis.Model_Data
{
    public class NodeLoad : AbstractNodeLoad
    {
        // ... Constructor ........................................................
        public NodeLoad(string nodeId, double px, double py, double moment)
        {
            NodeId = nodeId;
            Intensity = new double[3];
            Intensity[0] = px;
            Intensity[1] = py;
            Intensity[2] = moment;
        }

        public NodeLoad(string nodeId, double px, double py)
        {
            NodeId = nodeId;
            Intensity = new double[2];
            Intensity[0] = px;
            Intensity[1] = py;
        }

        public override double[] ComputeLoadVector()
        {
            return Intensity;
        }
    }
}