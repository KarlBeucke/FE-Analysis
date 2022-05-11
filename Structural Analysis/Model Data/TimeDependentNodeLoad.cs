using FEALibrary.Model.abstractClasses;
using System;

namespace FE_Analysis.Structural_Analysis.Model_Data
{
    public class TimeDependentNodeLoad : AbstractTimeDependentNodeLoad
    {
        public TimeDependentNodeLoad(string loadId, string nodeId, int nodalDof,
            bool file, bool ground)
        {
            LoadId = loadId;
            NodeId = nodeId;
            NodalDof = nodalDof;
            File = file;
            GroundExcitation = ground;
            VariationType = 0;
        }

        public override double[] ComputeLoadVector()
        {
            throw new NotImplementedException();
        }
    }
}