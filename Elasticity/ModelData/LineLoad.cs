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
            Intensity = new double[4]; // 2 nodes, 2 dimensions
            Intensity[0] = p1X;
            Intensity[1] = p2X;
            Intensity[2] = p1Y;
            Intensity[3] = p2Y;
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
            load[0] = l * (2.0 * Intensity[0] + Intensity[2]);
            load[2] = l * (2.0 * Intensity[2] + Intensity[0]);
            load[1] = l * (2.0 * Intensity[1] + Intensity[3]);
            load[3] = l * (2.0 * Intensity[3] + Intensity[1]);
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