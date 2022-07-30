using System;
using System.Windows;

namespace FEALibrary.Model.abstractClasses
{
    public abstract class AbstractLinear2D2 : Abstract2D
    {
        public double length;
        protected double[,] rotationMatrix = new double[2, 2];
        protected double sin, cos;
        private double[] Sx { get; } = new double[4];

        //public double ComputeLength()
        //{
        //    var delx = Nodes[1].Coordinates[0] - Nodes[0].Coordinates[0];
        //    var dely = Nodes[1].Coordinates[1] - Nodes[0].Coordinates[1];
        //    return length = Math.Sqrt(delx * delx + dely * dely);
        //}

        protected void ComputeGeometry()
        {
            var delx = Nodes[1].Coordinates[0] - Nodes[0].Coordinates[0];
            var dely = Nodes[1].Coordinates[1] - Nodes[0].Coordinates[1];
            //var angle = Math.Atan2(dely, delx);
            length = Math.Sqrt(delx * delx + dely * dely);
            sin = dely / length;
            cos = delx / length;
            rotationMatrix[0, 0] = cos;
            rotationMatrix[1, 0] = sin;
            rotationMatrix[0, 1] = -sin;
            rotationMatrix[1, 1] = cos;
        }

        protected double[] ComputeSx()
        {
            Sx[0] = -cos;
            Sx[1] = -sin;
            Sx[2] = cos;
            Sx[3] = sin;
            return Sx;
        }

        public override void SetSystemIndicesOfElement()
        {
            SystemIndicesOfElement = new int[NodesPerElement * ElementDof];
            var counter = 0;
            for (var i = 0; i < NodesPerElement; i++)
                for (var j = 0; j < ElementDof; j++)
                    SystemIndicesOfElement[counter++] = Nodes[i].SystemIndices[j];
        }

        protected static Point CenterOfGravity(AbstractElement element)
        {
            var cg = new Point();
            var nodes = element.Nodes;

            cg.X = nodes[0].Coordinates[0];
            cg.Y = nodes[0].Coordinates[1];

            cg.X += 0.5 * (nodes[1].Coordinates[0] - cg.X);
            cg.Y += 0.5 * (nodes[1].Coordinates[1] - cg.Y);

            return cg;
        }
    }
}