using System.Windows.Media.Media3D;

namespace FEALibrary.Model.abstractClasses
{
    public abstract class Abstract3D : AbstractElement
    {
        public abstract double[] ComputeElementState(double z0, double z1, double z2);
        public abstract Point3D ComputeCenterOfGravity3D();
    }
}