using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Collections.Generic;

namespace FE_Analysis.Structural_Analysis.Model_Data
{
    public class Support : AbstractBoundaryCondition
    {
        public const int Xfixed = 1, Yfixed = 2, Rfixed = 4,
                         XYfixed = 3, XRfixed = 5, YRfixed = 6,
                         XYRfixed = 7;

        public Support(string nodeId, int conditions, IReadOnlyList<double> pre, FeModel model)
        {
            if (nodeId.Length == 0) return;
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

            if (conditions == Xfixed) { Prescribed[0] = pre[0]; Restrained[0] = true; }
            if (conditions == Yfixed) { Prescribed[1] = pre[1]; Restrained[1] = true; }
            if (conditions == Rfixed) { Prescribed[2] = pre[2]; Restrained[2] = true; }
            if (conditions == XYfixed)
            {
                Prescribed[0] = pre[0]; Restrained[0] = true;
                Prescribed[1] = pre[1]; Restrained[1] = true;
            }
            if ((conditions) == XRfixed)
            {
                Prescribed[0] = pre[0]; Restrained[0] = true;
                Prescribed[2] = pre[2]; Restrained[2] = true;
            }
            if ((conditions) == YRfixed)
            {
                Prescribed[1] = pre[1]; Restrained[1] = true;
                Prescribed[2] = pre[2]; Restrained[2] = true;
            }
            if ((conditions) == XYRfixed)
            {
                Prescribed[0] = pre[0]; Restrained[0] = true;
                Prescribed[1] = pre[1]; Restrained[1] = true;
                Prescribed[2] = pre[2]; Restrained[2] = true;
            }
        }

        public bool XFixed()
        {
            return (Xfixed & Type) == Xfixed;
        }

        public bool YFixed()
        {
            return (Yfixed & Type) == Yfixed;
        }

        public bool RotationFixed()
        {
            return (Rfixed & Type) == Rfixed;
        }
    }
}