namespace FEALibrary.Model.abstractClasses
{
    public abstract class AbstractLoad
    {
        public string LoadId { get; set; }
        public string NodeId { get; set; }
        public double[] Loadvalues { get; set; }
        public abstract double[] ComputeLoadVector();
    }
}