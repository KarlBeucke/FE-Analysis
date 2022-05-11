using FEALibrary.Model.abstractClasses;

namespace FE_Analysis.Heat_Transfer.Model_Data
{
    public class BoundaryCondition : AbstractBoundaryCondition
    {
        // ....Constructor....................................................
        public BoundaryCondition(string boundaryConditionId, string nodeId, double pre)
        {
            SupportId = boundaryConditionId;
            NodeId = nodeId;
            Prescribed = new double[1];
            Prescribed[0] = pre;
            Restrained = new bool[1];
            Restrained[0] = true;
        }
    }
}