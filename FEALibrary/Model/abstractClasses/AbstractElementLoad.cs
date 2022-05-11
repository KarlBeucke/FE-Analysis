using System.Windows;

namespace FEALibrary.Model.abstractClasses
{
    public abstract class AbstractElementLoad : AbstractLoad
    {
        private AbstractElement element;
        public string ElementId { get; set; }

        public AbstractElement Element
        {
            get => element;
            set => element = value;
        }

        public bool InElementCoordinateSystem { get; set; } = true;

        public void SetReferences(FeModel modell)
        {
            if (modell.Elements.TryGetValue(ElementId, out element)) Element = element;

            if (element != null) return;
            var message = "Element with ID=" + ElementId + " is not contained in Model";
            _ = MessageBox.Show(message, "AbstractElementLoad");
        }

        public bool IsInElementCoordinateSystem()
        {
            return InElementCoordinateSystem;
        }
    }
}