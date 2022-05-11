namespace FEALibrary.Model
{
    public class CrossSection
    {
        public CrossSection(double area, double ixx)
        {
            CrossSectionValues = new double[2];
            CrossSectionValues[0] = area;
            CrossSectionValues[1] = ixx;
        }

        public CrossSection(double area)
        {
            CrossSectionValues = new double[1];
            CrossSectionValues[0] = area;
        }

        public string CrossSectionId { get; set; }
        public double[] CrossSectionValues { get; set; }
    }
}