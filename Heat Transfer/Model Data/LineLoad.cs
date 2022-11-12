using FEALibrary.Model.abstractClasses;
using System;

namespace FE_Analysis.Heat_Transfer.Model_Data
{
    public class LineLoad : AbstractLineLoad
    {
        public LineLoad(string startNodeId, string endNodeId, double[] p)
        {
            StartNodeId = startNodeId;
            EndNodeId = endNodeId;
            Loadvalues = new double[2];
            Loadvalues = p;
        }

        public LineLoad(string id, string startNodeId, string endNodeId, double[] p)
        {
            LoadId = id;
            StartNodeId = startNodeId;
            EndNodeId = endNodeId;
            Loadvalues = new double[2];
            Loadvalues = p;
        }

        // ....Compute concentrated node forces in local coordinate system....
        public override double[] ComputeLoadVector()
        {
            //Lastwerte = new double[2];
            var nStart = StartNode.Coordinates;
            var nEnd = EndNode.Coordinates;
            var vector = new double[2];
            var c1 = nEnd[0] - nStart[0];
            var c2 = nEnd[1] - nStart[1];
            var l = Math.Sqrt(c1 * c1 + c2 * c2) / 6.0;
            vector[0] = l * (2.0 * Loadvalues[0] + Loadvalues[1]);
            vector[1] = l * (2.0 * Loadvalues[1] + Loadvalues[0]);
            return vector;
        }
    }
}