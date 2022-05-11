namespace FEALibrary.Model.abstractClasses
{
    public abstract class AbstractMaterial
    {
        public string MaterialId { get; set; }

        public double[] MaterialValues { get; set; }
    }
}