using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System;
using System.Windows;

namespace FE_Analysis.Heat_Transfer.Model_Data
{
    public class Element2D2 : AbstractLinear2D2
    {
        private readonly double[,] elementMatrix;
        private readonly FeModel model;
        private readonly double[] specificHeatMatrix;
        private AbstractElement element;
        private Material material;

        public Element2D2(string[] eNodes, string eMaterialId, FeModel feModel)
        {
            if (feModel != null) model = feModel ?? throw new ArgumentNullException(nameof(feModel));
            NodeIds = eNodes ?? throw new ArgumentNullException(nameof(eNodes));
            ElementMaterialId = eMaterialId;
            ElementDof = 1;
            NodesPerElement = 2;
            elementMatrix = new double[NodesPerElement, NodesPerElement];
            specificHeatMatrix = new double[NodesPerElement];
            Nodes = new Node[NodesPerElement];
        }

        public Element2D2(string id, string[] eNodes, string eMaterialId, FeModel feModel)
        {
            model = feModel ?? throw new ArgumentNullException(nameof(feModel));
            ElementId = id ?? throw new ArgumentNullException(nameof(id));
            NodeIds = eNodes ?? throw new ArgumentNullException(nameof(eNodes));
            ElementMaterialId = eMaterialId ?? throw new ArgumentNullException(nameof(eMaterialId));
            ElementDof = 1;
            NodesPerElement = 2;
            elementMatrix = new double[NodesPerElement, NodesPerElement];
            specificHeatMatrix = new double[NodesPerElement];
            Nodes = new Node[NodesPerElement];
        }

        // ... compute element matrix ..................................
        public override double[,] ComputeMatrix()
        {
            if (model.Material.TryGetValue(ElementMaterialId, out var abstractMaterial))
            {
            }

            material = (Material)abstractMaterial;
            ElementMaterial = material ?? throw new ArgumentNullException(nameof(material));
            length = Math.Abs(Nodes[1].Coordinates[0] - Nodes[0].Coordinates[0]);
            if (material == null) return elementMatrix;
            var factor = material.MaterialValues[0] / length;
            elementMatrix[0, 0] = elementMatrix[1, 1] = factor;
            elementMatrix[0, 1] = elementMatrix[1, 0] = -factor;
            return elementMatrix;
        }

        // ....Compute diagonal Specific Heat Matrix.................................
        public override double[] ComputeDiagonalMatrix()
        {
            length = Math.Abs(Nodes[1].Coordinates[0] - Nodes[0].Coordinates[0]);
            // Me = specific heat * density * 0.5*length
            specificHeatMatrix[0] = specificHeatMatrix[1] = material.MaterialValues[3] * length / 2;
            return specificHeatMatrix;
        }

        public override double[] ComputeStateVector()
        {
            var elementHeatStatus = new double[2]; // in element
            return elementHeatStatus;
        }

        public override double[] ComputeElementState(double z0, double z1)
        {
            var elementHeatStatus = new double[2]; // in element
            return elementHeatStatus;
        }

        public override Point ComputeCenterOfGravity()
        {
            if (!model.Elements.TryGetValue(ElementId, out element))
                throw new ModelException("Element2D2: " + ElementId + " not found in model");
            element.SetReferences(model);
            return CenterOfGravity(element);
        }
    }
}