using FEALibrary.Model.abstractClasses;

namespace FE_Analysis.Elasticity.ModelData
{
    public class NodeLoad : AbstractLoad
    {
        // ... Constructor ........................................................
        public NodeLoad(string nodeId, double px, double py)
        {
            NodeId = nodeId;
            Loadvalues = new double[2];
            Loadvalues[0] = px;
            Loadvalues[1] = py;
        }

        public NodeLoad(string nodeId, double px, double py, double pz)
        {
            NodeId = nodeId;
            Loadvalues = new double[3];
            Loadvalues[0] = px;
            Loadvalues[1] = py;
            Loadvalues[2] = pz;
        }

        public override double[] ComputeLoadVector()
        {
            return Loadvalues;
        }
    }
}