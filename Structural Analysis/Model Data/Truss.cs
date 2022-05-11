using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using FEALibrary.Utils;
using System;
using System.Windows;

namespace FE_Analysis.Structural_Analysis.Model_Data
{
    public class Truss : AbstractBeam
    {
        private static double[,] _stiffnessMatrix = new double[4, 4];

        private static readonly double[] MassMatrix = new double[4];
        private readonly FeModel model;
        private AbstractElement element;

        // ... Constructor ........................................................
        public Truss(string[] eNodes, string crossSectionId, string materialId, FeModel feModel)
        {
            model = feModel;
            NodeIds = eNodes;
            ElementMaterialId = materialId;
            ElementCrossSectionId = crossSectionId;
            ElementDof = 2;
            NodesPerElement = 2;
            Nodes = new Node[2];
            ElementState = new double[2];
            ElementDeformations = new double[2];
        }

        // ... compute element matrix ..................................
        public override double[,] ComputeMatrix()
        {
            ComputeGeometry();
            var factor = ElementMaterial.MaterialValues[0] * ElementCrossSection.CrossSectionValues[0] / length;
            var sx = ComputeSx();
            _stiffnessMatrix = MatrixAlgebra.MultTransposedRect(factor, sx);
            return _stiffnessMatrix;
        }

        // ....Compute diagonal Mass Matrix.................................
        public override double[] ComputeDiagonalMatrix() //throws AlgebraicException
        {
            if (ElementMaterial.MaterialValues.Length < 3)
                throw new ModelException("Truss " + ElementId + ", specific mass not yet defined");
            // Me = specific mass * area * 0.5*length
            MassMatrix[0] = MassMatrix[1] = MassMatrix[2] = MassMatrix[3] =
                ElementMaterial.MaterialValues[2] * ElementCrossSection.CrossSectionValues[0] * length / 2;
            return MassMatrix;
        }

        public static double[] ComputeLoadVector(AbstractElementLoad ael, bool inElementCoordinateSystem)
        {
            if (ael == null) throw new ArgumentNullException(nameof(ael));
            throw new ModelException(
                "Truss element cannot support internal loads! Use BeamHinged");
        }

        // ... compute end forces of frame element........................
        public override double[] ComputeElementState()
        {
            ComputeGeometry();
            ComputeStateVector();
            var c1 = ElementMaterial.MaterialValues[0] * ElementCrossSection.CrossSectionValues[0] / length;
            ElementState[0] = c1 * (ElementDeformations[0] - ElementDeformations[1]);
            ElementState[1] = ElementState[0];
            return ElementState;
        }

        // ... compute displacement vector of frame elements .............
        public override double[] ComputeStateVector()
        {
            // transform to the local coordinate system
            ElementDeformations[0] = rotationMatrix[0, 0] * Nodes[0].NodalDof[0]
                                     + rotationMatrix[1, 0] * Nodes[0].NodalDof[1];
            ElementDeformations[1] = rotationMatrix[0, 0] * Nodes[1].NodalDof[0]
                                     + rotationMatrix[1, 0] * Nodes[1].NodalDof[1];
            return ElementDeformations;
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
            if (!model.Elements.TryGetValue(ElementId, out element))
                throw new ModelException("Truss: " + ElementId + " not found in Model");
            return CenterOfGravity(element);
        }

        public override double[] ComputeElementState(double z0, double z1)
        {
            throw new NotImplementedException();
        }
    }
}