using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Windows;

namespace FE_Analysis.Structural_Analysis.Model_Data
{
    public class SpringElement : Abstract2D
    {
        private readonly FeModel model;

        private readonly double[,] stiffnessMatrix = new double[3, 3];
        private Node node;

        // ... Constructor ........................................................
        public SpringElement(string[] springNode, string eMaterialId, FeModel feModel)
        {
            model = feModel;
            NodeIds = springNode;
            ElementMaterialId = eMaterialId;
            ElementDof = 3;
            NodesPerElement = 1;
            Nodes = new Node[1];
        }

        // ... compute element matrix ..................................
        public override double[,] ComputeMatrix()
        {
            stiffnessMatrix[0, 0] = ElementMaterial.MaterialValues[0];
            stiffnessMatrix[1, 1] = ElementMaterial.MaterialValues[1];
            stiffnessMatrix[2, 2] = ElementMaterial.MaterialValues[2];
            return stiffnessMatrix;
        }

        // ....Compute diagonal Spring Matrix.................................
        public override double[] ComputeDiagonalMatrix()
        {
            throw new ModelException("*** mass matrix not relevant for spring support");
        }

        // ... compute forces of spring element........................
        public override double[] ComputeStateVector()
        {
            ElementState = new double[3];
            ElementState[0] = ElementMaterial.MaterialValues[0] * Nodes[0].NodalDof[0];
            ElementState[1] = ElementMaterial.MaterialValues[1] * Nodes[0].NodalDof[1];
            ElementState[2] = ElementMaterial.MaterialValues[2] * Nodes[0].NodalDof[2];
            return ElementState;
        }

        public override double[] ComputeElementState(double z0, double z1)
        {
            var springForces = new double[3];
            return springForces;
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
            var cg = new Point();

            if (!model.Nodes.TryGetValue(NodeIds[0], out node))
                throw new ModelException("SpringElement: " + ElementId + " not found in Model");

            cg.X = node.Coordinates[0];
            cg.Y = node.Coordinates[1];
            return cg;
        }
    }
}