using FEALibrary.Model.abstractClasses;

namespace FE_Analysis.Heat_Transfer.Model_Data
{
    public class ElementLoad3 : AbstractElementLoad
    {
        private int[] systemIndices;
        // ....Constructors...................................................
        public ElementLoad3(string elementId, double[] p)
        {
            ElementId = elementId;
            Intensity = p;
        }

        public ElementLoad3(string id, string elementId, double[] p)
        {
            LoadId = id;
            ElementId = elementId;
            Intensity = p;
        }

        // ....Compute the element load vector.................................
        public override double[] ComputeLoadVector()
        {
            //Element.ComputeGeometry();
            var area = 0.5 * Element.Determinant;

            const double gaussWeight = 1.0 / 3.0;
            const double gc0 = 2.0 / 3.0;
            const double gc12 = 1.0 / 6.0;
            var vector = new double[3];
            var qp0 = gc0 * Intensity[0] + gc12 * Intensity[1] + gc12 * Intensity[2];
            var qp1 = gc12 * Intensity[0] + gc0 * Intensity[1] + gc12 * Intensity[2];
            var qp2 = gc12 * Intensity[0] + gc12 * Intensity[1] + gc0 * Intensity[2];
            vector[0] = (gc0 * qp0 + gc12 * qp1 + gc12 * qp2) * gaussWeight * area;
            vector[1] = (gc12 * qp0 + gc0 * qp1 + gc12 * qp2) * gaussWeight * area;
            vector[2] = (gc12 * qp0 + gc12 * qp1 + gc0 * qp2) * gaussWeight * area;
            return vector;
        }

        // ....Compute the element system indices .................................
        public int[] ComputeSystemIndices()
        {
            systemIndices = Element.SystemIndicesOfElement;
            return systemIndices;
        }
    }
}