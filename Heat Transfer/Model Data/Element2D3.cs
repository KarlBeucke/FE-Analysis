using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using FEALibrary.Utils;
using System.Windows;

namespace FE_Analysis.Heat_Transfer.Model_Data
{
    public class Element2D3 : AbstractLinear2D3
    {
        private AbstractElement element;
        private double[,] elementMatrix = new double[3, 3];
        private Node node;
        private Material material;
        public double[] SpecificHeatMatrix { get; }
        private readonly double[] elementTemperatures = new double[3]; // at element nodes
        public FeModel Modell { get; }

        public Element2D3(string[] eNodes, string eMaterialId, FeModel feModel)
        {
            Modell = feModel;
            ElementDof = 1;
            NodesPerElement = 3;
            NodeIds = eNodes;
            Nodes = new Node[NodesPerElement];
            ElementMaterialId = eMaterialId;
            SpecificHeatMatrix = new double[3];
        }

        public Element2D3(string id, string[] eNodes, string eMaterialId, FeModel feModel)
        {
            Modell = feModel;
            ElementId = id;
            ElementDof = 1;
            NodesPerElement = 3;
            NodeIds = eNodes;
            Nodes = new Node[NodesPerElement];
            ElementMaterialId = eMaterialId;
            SpecificHeatMatrix = new double[3];
        }

        // ....Compute element Matrix.....................................
        public override double[,] ComputeMatrix()
        {
            ComputeGeometry();
            if (Modell.Material.TryGetValue(ElementMaterialId, out var abstractMaterial))
            {
            }

            material = (Material)abstractMaterial;
            ElementMaterial = material;
            if (material == null) return elementMatrix;
            var conductivity = material.MaterialValues[0];
            // Ke = area*c*Sx*SxT
            elementMatrix = MatrixAlgebra.RectMultMatrixTransposed(0.5 * Determinant * conductivity, Sx, Sx);

            return elementMatrix;
        }

        // ....Compute diagonal Specific Heat Matrix.................................
        public override double[] ComputeDiagonalMatrix()
        {
            ComputeGeometry();
            // Me = density * conductivity * 0.5*determinant / 3    (area/3)
            SpecificHeatMatrix[0] = material.MaterialValues[3] * Determinant / 6;
            SpecificHeatMatrix[1] = SpecificHeatMatrix[0];
            if (SpecificHeatMatrix.Length > 2) SpecificHeatMatrix[2] = SpecificHeatMatrix[0];
            return SpecificHeatMatrix;
        }

        // ....Compute the heat state at the midpoint of the element......
        public override double[] ComputeStateVector()
        {
            var elementState = new double[2];
            return elementState;
        }

        public override double[] ComputeElementState(double z0, double z1)
        {
            var elementHeatStatus = new double[2]; // in element
            ComputeGeometry();
            if (Modell.Material.TryGetValue(ElementMaterialId, out var abstractMaterial))
            {
            }

            material = (Material)abstractMaterial;
            ElementMaterial = material;
            if (Modell.Elements.TryGetValue(ElementId, out element))
            {
                for (var i = 0; i < element.Nodes.Length; i++)
                {
                    if (Modell.Nodes.TryGetValue(element.NodeIds[i], out node))
                    {
                    }

                    //Debug.Assert(node != null, nameof(node) + " != null");
                    if (node != null) elementTemperatures[i] = node.NodalDof[0];
                }

                if (material == null) return elementHeatStatus;
                var conductivity = material.MaterialValues[0];
                elementHeatStatus = MatrixAlgebra.MultTransposed(-conductivity, Sx, elementTemperatures);
            }
            else
            {
                throw new ModelException("Element2D3: " + ElementId + " not found in model");
            }

            return elementHeatStatus;
        }

        public override void SetSystemIndicesOfElement()
        {
            SystemIndicesOfElement = new int[NodesPerElement * ElementDof];
            var counter = 0;
            for (var i = 0; i < NodesPerElement; i++)
                for (var j = 0; j < ElementDof; j++)
                    SystemIndicesOfElement[counter++] = Nodes[i].SystemIndices[j];
        }

        public override Point ComputeCenterOfGravity()
        {
            if (!Modell.Elements.TryGetValue(ElementId, out element))
                throw new ModelException("Element2D3: " + ElementId + " not found in model");
            element.SetReferences(Modell);
            return CenterOfGravity(element);
        }
    }
}