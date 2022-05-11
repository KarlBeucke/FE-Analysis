using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Collections.Generic;

namespace FE_Analysis.Elasticity.ModelData
{
    public class Support : AbstractBoundaryCondition
    {
        public const int XFixed = 1, YFixed = 2, ZFixed = 4;
        private const int XYFixed = 3, XZFixed = 5, YZFixed = 6, XYZFixed = 7;

        protected double[] deflection;

        //private int supportType;
        private string face;
        protected bool timeDependent = false;

        public Support(string nodeId, string face, int supportType, IReadOnlyList<double> pre, FeModel model)
        {
            this.face = face;
            int ndof;
            //switch (supportType)
            //{
            //    case 1:
            //        nodeId = "N00" + nodeId.Substring(3, 4);
            //        break;
            //    case 2:
            //        nodeId = nodeId.Substring(0, 3) + "00" + nodeId.Substring(5,2);
            //        break;
            //    case 3:
            //        nodeId = nodeId.Substring(0, 3) + nodeId.Substring(5, 2) + "00";
            //        break;
            //}
            if (model.Nodes.TryGetValue(nodeId, out var node))
                ndof = node.NumberOfNodalDof;
            else
                throw new ModelException("Lagerknoten nicht definiert");
            Type = supportType;
            Prescribed = new double[ndof];
            Restrained = new bool[ndof];
            for (var i = 0; i < ndof; i++) Restrained[i] = false;
            NodeId = nodeId;

            if (supportType == XFixed)
            {
                Prescribed[0] = pre[0];
                Restrained[0] = true;
            }

            if (supportType == YFixed)
            {
                Prescribed[1] = pre[1];
                Restrained[1] = true;
            }

            if (supportType == ZFixed)
            {
                Prescribed[2] = pre[2];
                Restrained[2] = true;
            }

            if (supportType == XYFixed)
            {
                Prescribed[0] = pre[0];
                Restrained[0] = true;
                Prescribed[1] = pre[1];
                Restrained[1] = true;
            }

            if (supportType == XZFixed)
            {
                Prescribed[0] = pre[0];
                Restrained[0] = true;
                Prescribed[2] = pre[2];
                Restrained[2] = true;
            }

            if (supportType == YZFixed)
            {
                Prescribed[1] = pre[1];
                Restrained[1] = true;
                Prescribed[2] = pre[2];
                Restrained[2] = true;
            }

            if (supportType == XYZFixed)
            {
                Prescribed[0] = pre[0];
                Restrained[0] = true;
                Prescribed[1] = pre[1];
                Restrained[1] = true;
                Prescribed[2] = pre[2];
                Restrained[2] = true;
            }
        }
    }
}