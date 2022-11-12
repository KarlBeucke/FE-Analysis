using System.Windows;

namespace FEALibrary.Model.abstractClasses
{
    public abstract class AbstractElement
    {
        public string ElementId { get; set; }
        public string[] NodeIds { get; protected set; }
        public Node[] Nodes { get; protected set; }
        protected int ElementDof { get; set; }
        public int NodesPerElement { get; set; }
        public int[] SystemIndicesOfElement { get; protected set; }
        public string ElementMaterialId { get; set; }
        public string ElementCrossSectionId { get; set; }
        public AbstractMaterial ElementMaterial { get; set; }
        public int Type { get; set; }
        public double[] ElementState { get; set; }
        public double[] ElementDeformations { get; protected set; }
        public double Determinant { get; protected set; }
        public abstract double[,] ComputeMatrix();
        public abstract double[] ComputeDiagonalMatrix();
        public abstract void SetSystemIndicesOfElement();
        public abstract double[] ComputeStateVector();

        public void SetReferences(FeModel modell)
        {
            for (var i = 0; i < NodesPerElement; i++)
            {
                if (modell.Nodes.TryGetValue(NodeIds[i], out var node)) Nodes[i] = node;

                if (node != null) continue;
                var message = "Element with ID = " + NodeIds[i] + " is not contained in Model";
                _ = MessageBox.Show(message, "AbstractElement");
            }

            if (modell.Material.TryGetValue(ElementMaterialId, out var material)) ElementMaterial = material;
            if (material == null)
            {
                var message = "Material with ID=" + ElementMaterialId + " is not contained in Model";
                _ = MessageBox.Show(message, "AbstractElement");
            }
        }
    }
}