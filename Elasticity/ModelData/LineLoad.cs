using FEALibrary.Model.abstractClasses;
using System;
using System.Runtime.Serialization;

namespace FE_Analysis.Elasticity.ModelData
{
    public class LineLoad : AbstractLineLoad
    {
        // ... Constructors ........................................................
        public LineLoad(string _startNodeId, double p1X, double p1Y, string endNodeId, double p2X, double p2Y)
        {
            StartNodeId = _startNodeId;
            EndNodeId = endNodeId;
            Loadvalues = new double[4]; // 2 nodes, 2 dimensions
            Loadvalues[0] = p1X;
            Loadvalues[1] = p2X;
            Loadvalues[2] = p1Y;
            Loadvalues[3] = p2Y;
        }

        public int StartNDOF { get; set; }

        public int EndNDOF { get; set; }

        public override double[] ComputeLoadVector()
        {
            var load = new double[4];
            double c1, c2, l;
            var nStart = StartNode.Coordinates;
            var nEnd = EndNode.Coordinates;
            c1 = nEnd[0] - nStart[0];
            c2 = nEnd[1] - nStart[1];
            l = Math.Sqrt(c1 * c1 + c2 * c2) / 6.0;
            load[0] = l * (2.0 * Loadvalues[0] + Loadvalues[2]);
            load[2] = l * (2.0 * Loadvalues[2] + Loadvalues[0]);
            load[1] = l * (2.0 * Loadvalues[1] + Loadvalues[3]);
            load[3] = l * (2.0 * Loadvalues[3] + Loadvalues[1]);
            return load;
        }

        [Serializable]
        private class RuntimeException : Exception
        {
            public RuntimeException()
            {
            }

            public RuntimeException(string message) : base(message)
            {
            }

            public RuntimeException(string message, Exception innerException) : base(message, innerException)
            {
            }

            protected RuntimeException(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }
        }
    }
}