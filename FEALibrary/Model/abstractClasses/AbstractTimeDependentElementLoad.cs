namespace FEALibrary.Model.abstractClasses
{
    public abstract class AbstractTimeDependentElementLoad : AbstractElementLoad
    {
        public int VariationType { get; set; }
        public double[] P { get; set; }
        public abstract override double[] ComputeLoadVector();
    }
}