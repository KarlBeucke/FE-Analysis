using System;
using System.Windows;

namespace FEALibrary.Model.abstractClasses
{
    public abstract class AbstractLinear2D3 : Abstract2D
    {
        protected readonly double[,] xzu = new double[2, 2]; // dx = Xzu * dzu
        protected double[,] Sx { get; set; } = new double[3, 2];

        // ... compute geometry ..........................................
        public void ComputeGeometry()
        {
            xzu[0, 0] = Nodes[0].Coordinates[0] - Nodes[2].Coordinates[0];
            xzu[0, 1] = Nodes[1].Coordinates[0] - Nodes[2].Coordinates[0];
            xzu[1, 0] = Nodes[0].Coordinates[1] - Nodes[2].Coordinates[1];
            xzu[1, 1] = Nodes[1].Coordinates[1] - Nodes[2].Coordinates[1];
            Determinant = xzu[0, 0] * xzu[1, 1] - xzu[0, 1] * xzu[1, 0];

            if (Math.Abs(Determinant) < double.Epsilon)
                throw new ModelException("AbstractLinear2D3: *** Error!!! *** Area = 0 in Element " + ElementId);
            if (Determinant < 0)
                throw new ModelException("negative Area in Element " + ElementId);

            Sx = ComputeSx();
        }

        private double[,] ComputeSx()
        {
            Sx[0, 0] = xzu[1, 1] / Determinant;
            Sx[0, 1] = -xzu[0, 1] / Determinant;
            Sx[1, 0] = -xzu[1, 0] / Determinant;
            Sx[1, 1] = xzu[0, 0] / Determinant;
            Sx[2, 0] = (xzu[1, 0] - xzu[1, 1]) / Determinant;
            Sx[2, 1] = (xzu[0, 1] - xzu[0, 0]) / Determinant;
            return Sx;
        }

        protected static Point CenterOfGravity(AbstractElement element)
        {
            var cg = new Point();
            var nodes = element.Nodes;
            cg.X = 0;
            for (var i = 0; i < element.Nodes.Length; i++)
            {
                cg.X += nodes[i].Coordinates[0];
                cg.Y += nodes[i].Coordinates[1];
            }

            cg.X /= 3.0;
            cg.Y /= 3.0;
            return cg;
        }
    }
}