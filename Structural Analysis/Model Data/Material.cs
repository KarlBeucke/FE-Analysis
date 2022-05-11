using FEALibrary.Model.abstractClasses;

namespace FE_Analysis.Structural_Analysis.Model_Data
{
    public class Material : AbstractMaterial
    {
        public Material(double emodulus, double poisson, double mass)
        {
            MaterialValues = new double[3];
            MaterialValues[0] = emodulus;
            MaterialValues[1] = poisson;
            MaterialValues[2] = mass;
        }

        public Material(double emodulus, double poisson)
        {
            MaterialValues = new double[2];
            MaterialValues[0] = emodulus;
            MaterialValues[1] = poisson;
        }

        public Material(double emodulus)
        {
            MaterialValues = new double[1];
            MaterialValues[0] = emodulus;
        }

        public Material(string spring, double fkx, double fky, double fkphi)
        {
            Spring = spring;
            MaterialValues = new double[3];
            MaterialValues[0] = fkx;
            MaterialValues[1] = fky;
            MaterialValues[2] = fkphi;
        }

        public string Spring { get; }
    }
}