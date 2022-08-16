using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using FEALibrary.Utils;
using System;
using System.Windows;

namespace FE_Analysis.Heat_Transfer.Model_Data
{
    public class Element2D4 : AbstractLinear2D4
    {
        private readonly double[,] elementMatrix = new double[4, 4];
        private readonly double[] elementTemperatures = new double[4]; // at element nodes
        private Material material;

        // ....Constructor................................................
        public Element2D4(string id, string[] eNodes, string materialId, FeModel feModel)
        {
            Model = feModel ?? throw new ArgumentNullException(nameof(feModel));
            ElementId = id ?? throw new ArgumentNullException(nameof(id));
            ElementDof = 1;
            NodesPerElement = 4;
            NodeIds = eNodes ?? throw new ArgumentNullException(nameof(eNodes));
            Nodes = new Node[NodesPerElement];
            for (var i = 0; i < NodesPerElement; i++)
            {
                if (Model.Nodes.TryGetValue(NodeIds[i], out var node))
                {
                }

                if (node != null) Nodes[i] = node;
            }

            ElementMaterialId = materialId ?? throw new ArgumentNullException(nameof(materialId));
        }

        public FeModel Model { get; set; }

        // ....Compute element Matrix.....................................
        public override double[,] ComputeMatrix()
        {
            double[] gaussCoord = { -1 / Math.Sqrt(3), 1 / Math.Sqrt(3) };
            if (Model.Material.TryGetValue(ElementMaterialId, out var abstractMaterial))
            {
            }

            material = (Material)abstractMaterial;
            ElementMaterial = material ?? throw new ArgumentNullException(nameof(material));
            var conductivity = material.MaterialValues[0];

            MatrixAlgebra.Clear(elementMatrix);
            foreach (var coor1 in gaussCoord)
                foreach (var coor2 in gaussCoord)
                {
                    var z0 = coor1;
                    var z1 = coor2;
                    ComputeGeometry(z0, z1);
                    Sx = ComputeSx(z0, z1);
                    // Ke = C*Sx*SxT*determinant
                    MatrixAlgebra.MultAddMatrixTransposed(elementMatrix, Determinant * conductivity, Sx, Sx);
                }

            return elementMatrix;
        }

        // ....Compute diagonal Specific Heat Matrix.................................
        public override double[] ComputeDiagonalMatrix()
        {
            throw new ModelException("*** specific heat matrix not yet implemented in Heat2D4");
        }

        // ....Compute the heat state at the (z0,z1) of the element......
        public override double[] ComputeStateVector()
        {
            var elementWärmeStatus = new double[2]; // in element
            return elementWärmeStatus;
        }

        public override double[] ComputeElementState(double z0, double z1)
        {
            _ = new double[2]; // in element
            ComputeGeometry(z0, z1);
            Sx = ComputeSx(z0, z1);
            for (var i = 0; i < NodesPerElement; i++)
                elementTemperatures[i] = Nodes[i].NodalDof[0];
            var conductivity = material.MaterialValues[0];
            var midpointHeatState = MatrixAlgebra.MultTransposed(-conductivity, Sx, elementTemperatures);
            return midpointHeatState;
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
            throw new NotImplementedException();
        }
    }
}