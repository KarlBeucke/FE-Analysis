using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using FEALibrary.Utils;
using System;
using System.Windows;

namespace FE_Analysis.Structural_Analysis.Model_Data
{
    public class Beam : AbstractBeam
    {
        //private readonly double[] shapeFunction = new double[6];
        private readonly double[] loadVector = new double[6];
        private readonly double[] massMatrix = new double[6];
        private readonly FeModel model;
        protected CrossSection crossSection;
        private AbstractElement element;
        protected AbstractMaterial material;

        private double[,] stiffnessMatrix = new double[6, 6];
        //private readonly double gaussPoint = 1.0 / Math.Sqrt(3.0);

        // ... Constructor ........................................................
        public Beam(string[] eNodeIds, string eCrossSectionId, string eMaterialId, FeModel feModel)
        {
            model = feModel;
            NodeIds = eNodeIds;
            ElementCrossSectionId = eCrossSectionId;
            ElementMaterialId = eMaterialId;
            ElementDof = 3;
            NodesPerElement = 2;
            Nodes = new Node[2];
            ElementState = new double[6];
            ElementDeformations = new double[6];
        }

        // ... Compute element matrix ........................................
        public override double[,] ComputeMatrix()
        {
            stiffnessMatrix = ComputeLocalMatrix();
            // ... transform local matrix to compute global stiffness ....
            stiffnessMatrix = TransformMatrix(stiffnessMatrix);
            return stiffnessMatrix;
        }

        // ... compute local stiffness ..................................
        private double[,] ComputeLocalMatrix()
        {
            ComputeGeometry();
            var h2 = ElementMaterial.MaterialValues[0] * ElementCrossSection.CrossSectionValues[1]; // EI
            var c1 = ElementMaterial.MaterialValues[0] * ElementCrossSection.CrossSectionValues[0] / length; // EA/L
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

        // ....Compute diagonal Mass Matrix.................................
        public override double[] ComputeDiagonalMatrix()
        {
            if (ElementMaterial.MaterialValues.Length < 3)
                throw new ModelException("Beam " + ElementId + ", specific mass not yet defined");
            // Verschiebungen: Me = specific mass * area * 0.5*length
            massMatrix[0] = massMatrix[1] = massMatrix[3] = massMatrix[4] =
                ElementMaterial.MaterialValues[2] * ElementCrossSection.CrossSectionValues[0] * length / 2;
            // Rotationsmassen = 0
            massMatrix[2] = massMatrix[5] = 0.0;
            return massMatrix;
        }

        public double[] ComputeLoadVector(AbstractLoad ael, bool inElementCoordinateSystem)
        {
            ComputeGeometry();
            for (var i = 0; i < loadVector.Length; i++) loadVector[i] = 0.0;

            switch (ael)
            {
                case LineLoad ll:
                    {
                        double na, nb, qa, qb;
                        if (!ll.IsInElementCoordinateSystem())
                        {
                            na = ll.Loadvalues[0] * cos + ll.Loadvalues[0] * sin;
                            nb = ll.Loadvalues[2] * cos + ll.Loadvalues[2] * sin;
                            qa = ll.Loadvalues[1] * -sin + ll.Loadvalues[1] * cos;
                            qb = ll.Loadvalues[3] * -sin + ll.Loadvalues[3] * cos;
                        }
                        else
                        {
                            na = ll.Loadvalues[0];
                            nb = ll.Loadvalues[2];
                            qa = ll.Loadvalues[1];
                            qb = ll.Loadvalues[3];
                        }

                        loadVector[0] = na * 0.5 * length;
                        loadVector[3] = nb * 0.5 * length;

                        // constant LineLoad
                        if (Math.Abs(qa - qb) < double.Epsilon)
                        {
                            loadVector[1] = loadVector[4] = qa * 0.5 * length;
                            loadVector[2] = qa * length * length / 12;
                            loadVector[5] = -qa * length * length / 12;
                        }
                        // triangular load ascending from a to b
                        else if (Math.Abs(qa) < Math.Abs(qb))
                        {
                            loadVector[1] = qa * 0.5 * length + (qb - qa) * 3 / 20 * length;
                            loadVector[4] = qa * 0.5 * length + (qb - qa) * 7 / 20 * length;
                            loadVector[2] = qa * length * length / 12 + (qb - qa) * length * length / 30;
                            loadVector[5] = -qa * length * length / 12 - (qb - qa) * length * length / 20;
                        }
                        // triangular load descending from a to b
                        else if (Math.Abs(qa) > Math.Abs(qb))
                        {
                            loadVector[1] = qb * 0.5 * length + (qa - qb) * 7 / 20 * length;
                            loadVector[4] = qb * 0.5 * length + (qa - qb) * 3 / 20 * length;
                            loadVector[2] = qb * length * length / 12 + (qa - qb) * length * length / 20;
                            loadVector[5] = -qb * length * length / 12 - (qa - qb) * length * length / 30;
                        }

                        break;
                    }

                case PointLoad pl:
                    {
                        //GetShapeFunctionValues(pl.Offset);

                        double xLoad;
                        double yLoad;

                        if (!pl.IsInElementCoordinateSystem())
                        {
                            xLoad = pl.Loadvalues[0] * cos + pl.Loadvalues[1] * sin;
                            yLoad = pl.Loadvalues[0] * -sin + pl.Loadvalues[1] * cos;
                        }
                        else
                        {
                            xLoad = pl.Loadvalues[0];
                            yLoad = pl.Loadvalues[1];
                        }

                        var a = pl.Offset * length;
                        var b = length - a;
                        loadVector[0] = xLoad / 2;
                        loadVector[1] = yLoad * b * b / length / length / length * (length + 2 * a);
                        loadVector[2] = yLoad * a * b * b / length / length;
                        loadVector[3] = xLoad / 2;
                        loadVector[4] = yLoad * a * a / length / length / length * (length + 2 * b);
                        loadVector[5] = -yLoad * a * a * b / length / length;
                        break;
                    }
                default:
                    throw new ModelException("Load " + ael + " is not supported in this element type ");
            }

            if (inElementCoordinateSystem) return loadVector;
            var tmpLoadVector = new double[6];
            Array.Copy(loadVector, tmpLoadVector, loadVector.Length);
            // transforms the loadvector to the global coordinate system.
            loadVector[0] = tmpLoadVector[0] * cos + tmpLoadVector[1] * -sin;
            loadVector[1] = tmpLoadVector[0] * sin + tmpLoadVector[1] * cos;
            loadVector[2] = tmpLoadVector[2];
            loadVector[3] = tmpLoadVector[3] * cos + tmpLoadVector[4] * -sin;
            loadVector[4] = tmpLoadVector[3] * sin + tmpLoadVector[4] * cos;
            loadVector[5] = tmpLoadVector[5];
            return loadVector;
        }

        //private void GetShapeFunctionValues(double z)
        //{
        //    ComputeGeometry();
        //    if (z < 0 || z > 1)
        //        throw new ModellAusnahme("Biegebalken: Formfunktion ungültig : " + z + " liegt außerhalb des Elements");
        //    // Shape functions. 0 <= z <= 1
        //    shapeFunction[0] = 1 - z;                           //x translation - low node
        //    shapeFunction[1] = 2 * z * z * z - 3 * z * z + 1;   //y translation - low node
        //    shapeFunction[2] = length * z * (z - 1) * (z - 1);  //z rotation - low node
        //    shapeFunction[3] = z;                               //x translation - high node
        //    shapeFunction[4] = z * z * (3 - 2 * z);             //y translation - high node
        //    shapeFunction[5] = length * z * z * (z - 1);        //z rotation - high node
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

        // ... compute end forces of frame element........................
        public override double[] ComputeElementState()
        {
            var localMatrix = ComputeLocalMatrix();
            var vector = ComputeStateVector();

            // contribution of the node deformations
            ElementState = MatrixAlgebra.Mult(localMatrix, vector);

            // contribution of the beam loads
            foreach (var item in model.PointLoads)
            {
                if (!(item.Value is PointLoad punktLast)) continue;
                if (punktLast.ElementId != ElementId) continue;
                vector = punktLast.ComputeLocalLoadVector();
                for (var i = 0; i < vector.Length; i++) ElementState[i] -= vector[i];
            }

            foreach (var item in model.ElementLoads)
            {
                if (!(item.Value is LineLoad linienLast)) continue;
                if (linienLast.ElementId != ElementId) continue;
                vector = linienLast.ComputeLocalLoadVector();
                for (var i = 0; i < vector.Length; i++) ElementState[i] -= vector[i];
            }

            ElementState[0] = -ElementState[0];
            ElementState[1] = -ElementState[1];
            ElementState[2] = -ElementState[2];
            return ElementState;
        }

        // ... compute displacement vector of frame elements .............
        public override double[] ComputeStateVector()
        {
            ComputeGeometry();
            const int ndof = 3;
            for (var i = 0; i < ndof; i++)
            {
                ElementDeformations[i] = Nodes[0].NodalDof[i];
                ElementDeformations[i + ndof] = Nodes[1].NodalDof[i];
            }

            // transform to the local coordinate system
            var temp0 = rotationMatrix[0, 0] * ElementDeformations[0]
                        + rotationMatrix[1, 0] * ElementDeformations[1];

            var temp1 = rotationMatrix[0, 1] * ElementDeformations[0]
                        + rotationMatrix[1, 1] * ElementDeformations[1];
            ElementDeformations[0] = temp0;
            ElementDeformations[1] = temp1;

            temp0 = rotationMatrix[0, 0] * ElementDeformations[3]
                    + rotationMatrix[1, 0] * ElementDeformations[4];
            temp1 = rotationMatrix[0, 1] * ElementDeformations[3]
                    + rotationMatrix[1, 1] * ElementDeformations[4];
            ElementDeformations[3] = temp0;
            ElementDeformations[4] = temp1;

            return ElementDeformations;
        }

        public override double[] ComputeElementState(double z0, double z1)
        {
            return ElementState;
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
                throw new ModelException("Beam: " + ElementId + " not found in Model");
            return CenterOfGravity(element);
        }
    }
}