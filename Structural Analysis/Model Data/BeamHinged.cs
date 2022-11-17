using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using FEALibrary.Utils;
using System.Linq;
using System.Windows;

namespace FE_Analysis.Structural_Analysis.Model_Data
{
    public class BeamHinged : AbstractBeam
    {
        private readonly int[] c;
        private readonly int[] chigh = { 0, 1, 2, 3, 4 };

        // dof Identificators
        private readonly int[] clow = { 0, 1, 3, 4, 5 };

        private readonly int first = 1;
        private readonly int second = 2;

        private readonly double[,] kcc = new double[5, 5];
        private readonly double[] kcl = new double[5];
        private readonly double[,] kclxkllxklc = new double[5, 5];
        private readonly double[] klc = new double[5];
        private readonly double[] kll = new double[1];
        private readonly double[] kllxklc = new double[5];
        private readonly int[] l;
        private readonly int[] lhigh = { 5 };
        private readonly int[] llow = { 2 };
        private readonly double[] massMatrix = new double[6];
        private readonly FeModel model;
        private readonly double[] uc = new double[5];
        private AbstractElement element;

        // temp Variable for a hinge
        private double invkll;
        private double[] kcxuc = new double[5];
        private double[,] redStiffnessMatrix = new double[5, 5];
        private double[,] stiffnessMatrix = new double[6, 6];

        public BeamHinged(string[] eNodeIds, string eMaterialId, string eCrossSectionId, FeModel feModel, int type)
        {
            model = feModel;
            ElementDof = 3;
            NodeIds = eNodeIds;
            ElementMaterialId = eMaterialId;
            ElementCrossSectionId = eCrossSectionId;
            NodesPerElement = 2;
            Nodes = new Node[2];
            ElementState = new double[6];
            ElementDeformations = new double[6];

            Type = type;
            if (Type == first)
            {
                c = clow;
                l = llow;
            }
            else if (Type == second)
            {
                c = chigh;
                l = lhigh;
            }
            else
            {
                throw new ModelException("BeamHinged: hinge type was not recognized!");
            }
        }

        // ... compute local stiffness ................................
        private double[,] ComputeLocalMatrix()
        {
            ComputeGeometry();
            var h2 = ElementMaterial.MaterialValues[0] * ElementCrossSection.CrossSectionValues[1]; // EI
            var c1 = ElementMaterial.MaterialValues[0] * ElementCrossSection.CrossSectionValues[0] / length; // AE/L
            var c2 = 12.0 * h2 / length / length / length;
            var c3 = 6.0 * h2 / length / length;
            var c4 = 4.0 * h2 / length;
            var c5 = 0.5 * c4;

            double[,] localMatrix =
            {
                { c1, 0, 0, -c1, 0, 0 },
                { 0, c2, c3, 0, -c2, c3 },
                { 0, c3, c4, 0, -c3, c5 },
                { -c1, 0, 0, c1, 0, 0 },
                { 0, -c2, -c3, 0, c2, -c3 },
                { 0, c3, c5, 0, -c3, c4 }
            };
            return localMatrix;
        }

        //private double[] ComputeLoadVector(AbstractElementLoad ael, bool inElementCoordinateSystem)
        //{
        //    var superLoadVector = ComputeLoadVector(ael, inElementCoordinateSystem);
        //    Array.Copy(superLoadVector, 0, p, 0, 6); //length 6, calculates the kcc, kcl ... matrices for this element

        //    if (inElementCoordinateSystem)
        //        ComputeLocalMatrix();
        //    else
        //        ComputeMatrix();
        //    if (Type == first)
        //    {
        //        pc[0] = p[0]; pc[1] = p[1]; pc[2] = p[3]; pc[3] = p[4]; pc[4] = p[5]; pl[0] = p[2];
        //    }
        //    else if (Type == second)
        //    {
        //        pc[0] = p[0]; pc[1] = p[1]; pc[2] = p[2]; pc[3] = p[3]; pc[4] = p[4]; pl[0] = p[5];
        //    }

        //    for (var k = 0; k < 5; k++) kclxinvkll[k] = kcl[k] * invkll;
        //    for (var i = 0; i < 5; i++)
        //        for (var j = 0; j < 5; j++) kclxinvkllxpl[i] += kclxinvkll[i] * pl[j];
        //    for (var k = 0; k < 5; k++) kclxinvkllxpl[k] = kclxinvkllxpl[k] * -1;
        //    for (var k = 0; k < 5; k++) loadVector[k] = pc[k] + kclxinvkllxpl[k];
        //    return loadVector;
        //}

        private double[,] TransformMatrix(double[,] matrix)
        {
            var elementDof = ElementDof;
            for (var i = 0; i < matrix.GetLength(0); i += elementDof)
                for (var k = 0; k < matrix.GetLength(0); k += elementDof)
                {
                    var m11 = matrix[i, k];
                    var m12 = matrix[i, k + 1];
                    var m13 = matrix[i, k + 2];

                    var m21 = matrix[i + 1, k];
                    var m22 = matrix[i + 1, k + 1];
                    var m23 = matrix[i + 1, k + 2];

                    var m31 = matrix[i + 2, k];
                    var m32 = matrix[i + 2, k + 1];

                    var e11 = rotationMatrix[0, 0];
                    var e12 = rotationMatrix[0, 1];
                    var e21 = rotationMatrix[1, 0];
                    var e22 = rotationMatrix[1, 1];

                    var h11 = e11 * m11 + e12 * m21;
                    var h12 = e11 * m12 + e12 * m22;
                    var h21 = e21 * m11 + e22 * m21;
                    var h22 = e21 * m12 + e22 * m22;

                    matrix[i, k] = h11 * e11 + h12 * e12;
                    matrix[i, k + 1] = h11 * e21 + h12 * e22;
                    matrix[i + 1, k] = h21 * e11 + h22 * e12;
                    matrix[i + 1, k + 1] = h21 * e21 + h22 * e22;

                    matrix[i, k + 2] = e11 * m13 + e12 * m23;
                    matrix[i + 1, k + 2] = e21 * m13 + e22 * m23;
                    matrix[i + 2, k] = m31 * e11 + m32 * e12;
                    matrix[i + 2, k + 1] = m31 * e21 + m32 * e22;
                }

            return matrix;
        }

        public override double[] ComputeElementState()
        {
            var matrix = ComputeLocalRedMatrix();
            ElementDeformations = ComputeStateVector();

            // Beitrag der Knotenverformungen
            kcxuc = MatrixAlgebra.Mult(matrix, ElementDeformations);

            // Beitrag der Balkenlasten
            foreach (var last in model.ElementLoads.Select(item => item.Value))
            {
                if (last is AbstractElementLoad linienLast)
                {
                    var ll = (LineLoad)linienLast;
                    ElementDeformations = ll.ComputeLocalLoadVector();
                    for (var k = 0; k < 5; k++) kcxuc[k] -= ElementDeformations[k];
                }

                if (!(last is AbstractElementLoad punktLast)) continue;
                {
                    var last1 = (PointLoad)punktLast;
                    ElementDeformations = last1.ComputeLocalLoadVector();
                    for (var k = 0; k < 5; k++) kcxuc[k] -= ElementDeformations[k];
                }
            }

            if (Type == first)
            {
                ElementState[0] = -kcxuc[0];
                ElementState[1] = -kcxuc[1];
                ElementState[2] = 0.0;
                ElementState[3] = kcxuc[2];
                ElementState[4] = kcxuc[3];
                ElementState[5] = kcxuc[4];
            }
            else if (Type == second)
            {
                ElementState[0] = -kcxuc[0];
                ElementState[1] = -kcxuc[1];
                ElementState[2] = -kcxuc[2];
                ElementState[3] = kcxuc[3];
                ElementState[4] = kcxuc[4];
                ElementState[5] = 0.0;
            }

            return ElementState;
        }

        // ... compute deformation vector for truss elements .............
        public int[] GetSystemIndices()
        {
            int[] indices;
            if (Type == first)
            {
                var reduced = new int[5];
                indices = Nodes[0].SystemIndices;
                reduced[0] = indices[0];
                reduced[1] = indices[1];
                indices = Nodes[1].SystemIndices;
                reduced[2] = indices[0];
                reduced[3] = indices[1];
                reduced[4] = indices[2];
                return reduced;
            }

            if (Type != second) throw new ModelException("Beam Hinged GetSystemIndices: invalid hinge type");
            {
                var reduced = new int[5];
                indices = Nodes[0].SystemIndices;
                reduced[0] = indices[0];
                reduced[1] = indices[1];
                reduced[2] = indices[2];
                indices = Nodes[1].SystemIndices;
                reduced[3] = indices[0];
                reduced[4] = indices[1];
                return reduced;
            }
        }

        /**
         * |Kcc Klc|
         * |       |
         * |Kcl Kll|
         * 
         * | Kcc - Kcl*Kll^-1*klc |
         */
        private double[,] CondensateMatrix(double[,] ke)
        {
            MatrixAlgebra.ExtractSubMatrix(ke, kcc, c);
            MatrixAlgebra.ExtractSubMatrix(ke, kcl, c, l);
            MatrixAlgebra.ExtractSubMatrix(ke, klc, l, c);
            MatrixAlgebra.ExtractSubMatrix(ke, kll, l, l);
            invkll = 1 / kll[0];
            for (var k = 0; k < 5; k++) kllxklc[k] = invkll * klc[k];
            for (var i = 0; i < 5; i++)
                for (var j = 0; j < 5; j++)
                    kclxkllxklc[i, j] = kcl[j] * kllxklc[i];
            for (var i = 0; i < 5; i++)
                for (var j = 0; j < 5; j++)
                    redStiffnessMatrix[i, j] = kcc[i, j] - kclxkllxklc[i, j];
            //MatrixAlgebra.Subtract(redStiffnessMatrix, kcc, kclxkllxklc);
            return redStiffnessMatrix;
        }

        private double[,] ComputeLocalRedMatrix()
        {
            return CondensateMatrix(ComputeLocalMatrix());
        }

        public override double[,] ComputeMatrix()
        {
            stiffnessMatrix = ComputeLocalMatrix();
            // transform local matrix to compute global stiffness
            stiffnessMatrix = TransformMatrix(stiffnessMatrix);

            redStiffnessMatrix = CondensateMatrix(stiffnessMatrix);
            return redStiffnessMatrix;
        }

        public override double[] ComputeDiagonalMatrix()
        {
            if (ElementMaterial.MaterialValues.Length < 3)
                throw new ModelException("BeamHinged " + ElementId + ", specific mass not yet defined");
            // Me = specific mass * area * 0.5*length
            massMatrix[0] = massMatrix[1] = massMatrix[3] = massMatrix[4] =
                ElementMaterial.MaterialValues[2] * ElementCrossSection.CrossSectionValues[0] * length / 2;
            massMatrix[2] = massMatrix[5] = 1;
            return massMatrix;
        }

        public override void SetSystemIndicesOfElement()
        {
            SystemIndicesOfElement = new int[5];
            var counter = 0;
            if (Type == first)
            {
                for (var j = 0; j < ElementDof - 1; j++)
                    SystemIndicesOfElement[counter++] = Nodes[0].SystemIndices[j];
                for (var j = 0; j < ElementDof; j++)
                    SystemIndicesOfElement[counter++] = Nodes[1].SystemIndices[j];
            }
            else if (Type == second)
            {
                for (var j = 0; j < ElementDof; j++)
                    SystemIndicesOfElement[counter++] = Nodes[0].SystemIndices[j];
                for (var j = 0; j < ElementDof - 1; j++)
                    SystemIndicesOfElement[counter++] = Nodes[1].SystemIndices[j];
            }
            else
            {
                throw new ModelException("BeamHinged SetSystemIndices: hinge type was not recognized!");
            }
        }

        public override double[] ComputeStateVector()
        {
            ComputeGeometry();
            if (Type == first)
            {
                uc[0] = Nodes[0].NodalDof[0] * cos + Nodes[0].NodalDof[1] * sin;
                uc[1] = Nodes[0].NodalDof[0] * -sin + Nodes[0].NodalDof[1] * cos;
                uc[2] = Nodes[1].NodalDof[0] * cos + Nodes[1].NodalDof[1] * sin;
                uc[3] = Nodes[1].NodalDof[0] * -sin + Nodes[1].NodalDof[1] * cos;
                uc[4] = Nodes[1].NodalDof[2];
            }
            else if (Type == second)
            {
                uc[0] = Nodes[0].NodalDof[0] * cos + Nodes[0].NodalDof[1] * sin;
                uc[1] = Nodes[0].NodalDof[0] * -sin + Nodes[0].NodalDof[1] * cos;
                uc[2] = Nodes[0].NodalDof[2];
                uc[3] = Nodes[1].NodalDof[0] * cos + Nodes[1].NodalDof[1] * sin;
                uc[4] = Nodes[1].NodalDof[0] * -sin + Nodes[1].NodalDof[1] * cos;
            }

            return uc;
        }

        public override double[] ComputeElementState(double z0, double z1)
        {
            return uc;
        }

        public override Point ComputeCenterOfGravity()
        {
            if (!model.Elements.TryGetValue(ElementId, out element))
                throw new ModelException("BeamHinged: " + ElementId + " not found in Model");
            return CenterOfGravity(element);
        }
    }
}