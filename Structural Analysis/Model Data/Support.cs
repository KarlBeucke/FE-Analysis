using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Collections.Generic;

namespace FE_Analysis.Structural_Analysis.Model_Data
{
    public class Support : AbstractBoundaryCondition
    {
        public static readonly int X_FIXED = 1, Y_FIXED = 2, R_FIXED = 4,
            XY_FIXED = 3, XR_FIXED = 5, YR_FIXED = 6,
            XYR_FIXED = 7;

        public Support(string nodeId, int conditions, IReadOnlyList<double> pre, FeModel model)
        {
            Type = conditions;
            if (model.Nodes.TryGetValue(nodeId, out _)) { }
            else
            {
                throw new ModelException("Support node  " + nodeId + " not defined");
            }

            Prescribed = new double[pre.Count];
            Restrained = new bool[pre.Count];
            for (var i = 0; i < pre.Count; i++) Restrained[i] = false;
            NodeId = nodeId;

            if (conditions == X_FIXED) { Prescribed[0] = pre[0]; Restrained[0] = true; }
            if (conditions == Y_FIXED) { Prescribed[1] = pre[1]; Restrained[1] = true; }
            if (conditions == R_FIXED) { Prescribed[2] = pre[2]; Restrained[2] = true; }
            if (conditions == XY_FIXED)
            {
                Prescribed[0] = pre[0]; Restrained[0] = true;
                Prescribed[1] = pre[1]; Restrained[1] = true;
            }
            if ((conditions) == XR_FIXED)
            {
                Prescribed[0] = pre[0]; Restrained[0] = true;
                Prescribed[2] = pre[2]; Restrained[2] = true;
            }
            if ((conditions) == YR_FIXED)
            {
                Prescribed[1] = pre[1]; Restrained[1] = true;
                Prescribed[2] = pre[2]; Restrained[2] = true;
            }
            if ((conditions) == XYR_FIXED)
            {
                Prescribed[0] = pre[0]; Restrained[0] = true;
                Prescribed[1] = pre[1]; Restrained[1] = true;
                Prescribed[2] = pre[2]; Restrained[2] = true;
            }
        }

        public bool XFixed()
        {
            return (X_FIXED & Type) == X_FIXED;
        }

        public bool YFixed()
        {
            return (Y_FIXED & Type) == Y_FIXED;
        }

        public bool RotationFixed()
        {
            return (R_FIXED & Type) == R_FIXED;
        }
    }
}