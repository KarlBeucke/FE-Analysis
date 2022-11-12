namespace FEALibrary.Model.abstractClasses
{
    public abstract class AbstractMaterial
    {
        public bool Spring { get; set; }
        public string MaterialId { get; set; }

        public double[] MaterialValues { get; set; }
    }
}