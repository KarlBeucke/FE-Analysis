using FEALibrary.Model.abstractClasses;

namespace FE_Analysis.Structural_Analysis.Model_Data
{
    public class NodeLoad : AbstractNodeLoad
    {
        // ... Constructor ........................................................
        public NodeLoad(string nodeId, double px, double py, double moment)
        {
            NodeId = nodeId;
            Loadvalues = new double[3];
            Loadvalues[0] = px;
            Loadvalues[1] = py;
            Loadvalues[2] = moment;
        }

        public NodeLoad(string nodeId, double px, double py)
        {
            NodeId = nodeId;
            Loadvalues = new double[2];
            Loadvalues[0] = px;
            Loadvalues[1] = py;
        }

        public override double[] ComputeLoadVector()
        {
            return Loadvalues;
        }
    }
}