using System.Windows;

namespace FEALibrary.Model.abstractClasses
{
    public abstract class Abstract2D : AbstractElement
    {
        protected CrossSection ElementCrossSection { get; set; }

        public void SetCrossSectionReferences(FeModel modell)
        {
            // Elementquerschnitt für 2D Elemente
            if (ElementCrossSectionId == null) return;
            if (modell.CrossSection.TryGetValue(ElementCrossSectionId, out var crossSection))
            {
                ElementCrossSection = crossSection;
            }
            else
            {
                var querschnitt =
                    MessageBox.Show("Querschnitt " + ElementCrossSectionId + " ist nicht im Modell enthalten.",
                        "Abstract2D");
                _ = querschnitt;
            }
        }

        public abstract double[] ComputeElementState(double z0, double z1);
        public abstract Point ComputeCenterOfGravity();
    }
}