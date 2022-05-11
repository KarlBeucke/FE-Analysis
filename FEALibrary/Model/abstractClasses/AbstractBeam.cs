namespace FEALibrary.Model.abstractClasses
{
    public abstract class AbstractBeam : AbstractLinear2D2
    {
        public abstract double[] ComputeElementState();
    }
}