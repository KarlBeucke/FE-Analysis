using FEALibrary.Model.abstractClasses;

namespace FE_Analysis.Elasticity.ModelData
{
    public class Material : AbstractMaterial
    {
        public Material(double _emodulus, double poisson)
        {
            MaterialValues = new double[2];
            MaterialValues[0] = _emodulus;
            MaterialValues[1] = poisson;
        }

        public Material(double _emodulus, double poisson, double mass)
        {
            MaterialValues = new double[3];
            MaterialValues[0] = _emodulus;
            MaterialValues[1] = poisson;
            MaterialValues[2] = mass;
        }
    }
}