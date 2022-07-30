using FEALibrary.Utils;
using System;
using System.Windows;

namespace FEALibrary.Model.abstractClasses
{
    public abstract class AbstractLinear2D4 : Abstract2D
    {
        private readonly double[,] xz = new double[2, 2];
        protected double[,] Sx { get; set; } = new double[4, 2];

        // ... compute geometry ..........................................
        public void ComputeGeometry(double z0, double z1)
        {
            xz[0, 0] = 0.25 * (-Nodes[0].Coordinates[0] * (1 - z1)
                               + Nodes[1].Coordinates[0] * (1 - z1)
                               + Nodes[2].Coordinates[0] * (1 + z1)
                               - Nodes[3].Coordinates[0] * (1 + z1));
            xz[0, 1] = 0.25 * (-Nodes[0].Coordinates[0] * (1 - z0)
                               - Nodes[1].Coordinates[0] * (1 + z0)
                               + Nodes[2].Coordinates[0] * (1 + z0)
                               + Nodes[3].Coordinates[0] * (1 - z0));
            xz[1, 0] = 0.25 * (-Nodes[0].Coordinates[1] * (1 - z1)
                               + Nodes[1].Coordinates[1] * (1 - z1)
                               + Nodes[2].Coordinates[1] * (1 + z1)
                               - Nodes[3].Coordinates[1] * (1 + z1));
            xz[1, 1] = 0.25 * (-Nodes[0].Coordinates[1] * (1 - z0)
                               - Nodes[1].Coordinates[1] * (1 + z0)
                               + Nodes[2].Coordinates[1] * (1 + z0)
                               + Nodes[3].Coordinates[1] * (1 - z0));
            Determinant = xz[0, 0] * xz[1, 1] - xz[0, 1] * xz[1, 0];

            if (Math.Abs(Determinant) < double.Epsilon)
                throw new AlgebraicException("Area = 0 in Element " + ElementId);
            if (Determinant < 0)
                throw new ModelException("negative Area in Element " + ElementId);
        }

        protected double[,] ComputeSx(double z0, double z1)
        {
            var fac = 0.25 / Determinant;
            Sx[0, 0] = fac * (-xz[1, 1] * (1 - z1) + xz[1, 0] * (1 - z0));
            Sx[1, 0] = fac * (xz[1, 1] * (1 - z1) + xz[1, 0] * (1 + z0));
            Sx[2, 0] = fac * (xz[1, 1] * (1 + z1) - xz[1, 0] * (1 + z0));
            Sx[3, 0] = fac * (-xz[1, 1] * (1 + z1) - xz[1, 0] * (1 - z0));
            Sx[0, 1] = fac * (xz[0, 1] * (1 - z1) - xz[0, 0] * (1 - z0));
            Sx[1, 1] = fac * (-xz[0, 1] * (1 - z1) - xz[0, 0] * (1 + z0));
            Sx[2, 1] = fac * (-xz[0, 1] * (1 + z1) + xz[0, 0] * (1 + z0));
            Sx[3, 1] = fac * (xz[0, 1] * (1 + z1) + xz[0, 0] * (1 - z0));
            return Sx;
        }

        public static double[] ComputeS(double z0, double z1)
        {
            var s = new double[4];
            s[0] = 0.25 * (1 - z0) * (1 - z1);
            s[1] = 0.25 * (1 + z0) * (1 - z1);
            s[2] = 0.25 * (1 + z0) * (1 + z1);
            s[3] = 0.25 * (1 - z0) * (1 + z1);
            return s;
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

            cg.X /= 4.0;
            cg.Y /= 4.0;
            return cg;
        }
    }
}