using FEALibrary.Model.abstractClasses;

namespace FE_Analysis.Structural_Analysis.Model_Data
{
    public class PointLoad : AbstractElementLoad
    {
        // constructor for point load .....
        public PointLoad(string elementId, double fx, double fy, double o)
        {
            ElementId = elementId;
            Loadvalues = new double[2];
            Loadvalues[0] = fx;
            Loadvalues[1] = fy;
            Offset = o;
        }

        public double Offset { get; set; }

        // --- get global load vector ---------------------------------------------
        public override double[] ComputeLoadVector()
        {
            var balken = (Beam)Element;
            return balken.ComputeLoadVector(this, false);
        }

        // ... get load vector ....................................................
        public double[] ComputeLocalLoadVector()
        {
            var balken = (Beam)Element;
            return balken.ComputeLoadVector(this, true);
        }
    }
}