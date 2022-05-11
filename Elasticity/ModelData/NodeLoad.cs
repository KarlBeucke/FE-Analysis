using FEALibrary.Model.abstractClasses;

namespace FE_Analysis.Elasticity.ModelData
{
    public class NodeLoad : AbstractLoad
    {
        // ... Constructor ........................................................
        public NodeLoad(string nodeId, double px, double py)
        {
            NodeId = nodeId;
            Intensity = new double[2];
            Intensity[0] = px;
            Intensity[1] = py;
        }

        public NodeLoad(string nodeId, double px, double py, double pz)
        {
            NodeId = nodeId;
            Intensity = new double[3];
            Intensity[0] = px;
            Intensity[1] = py;
            Intensity[2] = pz;
        }

        public override double[] ComputeLoadVector()
        {
            return Intensity;
        }
    }
}