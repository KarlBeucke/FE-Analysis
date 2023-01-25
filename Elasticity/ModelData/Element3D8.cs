using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using FEALibrary.Utils;
using System;
using System.Windows.Media.Media3D;

namespace FE_Analysis.Elasticity.ModelData;

public class Element3D8 : AbstractLinear3D8
{
    private static readonly double[] GCoord = { -1.0 / Math.Sqrt(5.0 / 3.0), 0.0, 1.0 / Math.Sqrt(5.0 / 3.0) };
    private static readonly double[] GWeight = { 5.0 / 9.0, 8.0 / 9.0, 5.0 / 9.0 }; // gaussian coordinates, weights
    private readonly double[,] b = new double[6, 24]; // strain-displacement transformation
    private readonly double[,] e = new double[6, 6]; // material matrix
    private readonly double[] elementDeformations = new double[24]; // at element nodes
    private readonly double[,] elementMatrix = new double[24, 24];
    private readonly FeModel model;
    private AbstractElement element;
    private double z0, z1, z2, g0, g1, g2;

    public Element3D8(string[] eNodes, string eMaterialId, FeModel feModel)
    {
        model = feModel;
        ElementDof = 3;
        NodesPerElement = 8;
        NodeIds = eNodes;
        Nodes = new Node[NodesPerElement];
        ElementMaterialId = eMaterialId;
    }

    // ....Compute element matrix.....................................
    public override double[,] ComputeMatrix()
    {
        MatrixAlgebra.Clear(elementMatrix);
        ComputeMaterial();
        for (var i = 0; i < GCoord.Length; i++)
        {
            z0 = GCoord[i];
            g0 = GWeight[i];
            for (var j = 0; j < GCoord.Length; j++)
            {
                z1 = GCoord[j];
                g1 = GWeight[j];
                for (var k = 0; k < GCoord.Length; k++)
                {
                    z2 = GCoord[k];
                    g2 = GWeight[k];
                    ComputeGeometry(z0, z1, z2);
                    Sx = ComputeSx(z0, z1, z2);
                    ComputeStrainDisplacementTransformation();
                    // Ke = determinant*g0*g1*g2*BT*E*B
                    var temp = MatrixAlgebra.MultTransposedMatrix(Determinant * g0 * g1 * g2, b, e);
                    MatrixAlgebra.MultAddMatrix(elementMatrix, temp, b);
                }
            }
        }

        return elementMatrix;
    }

    // ....Compute mass Matrix.................................
    public override double[] ComputeDiagonalMatrix()
    {
        throw new ModelException("*** Massenmatrix noch nicht implementiert in Element3D8");
    }

    // compute strain-displacement transformation matrix eps = B * u
    private void ComputeStrainDisplacementTransformation()
    {
        int i, j, k;
        for (i = 0; i < 3; i++)
            for (j = i, k = 0; k < 8; j += 3, k++)
                b[i, j] = Sx[k, i];
        for (i = 0, k = 0; i < 8; i++, k += 3) b[3, k] = Sx[i, 1];
        for (i = 0, k = 1; i < 8; i++, k += 3) b[3, k] = Sx[i, 0];
        for (i = 0, k = 1; i < 8; i++, k += 3) b[4, k] = Sx[i, 2];
        for (i = 0, k = 2; i < 8; i++, k += 3) b[4, k] = Sx[i, 1];
        for (i = 0, k = 0; i < 8; i++, k += 3) b[5, k] = Sx[i, 2];
        for (i = 0, k = 2; i < 8; i++, k += 3) b[5, k] = Sx[i, 0];
    }

    // compute material matrix for plane strain
    private void ComputeMaterial()
    {
        var emod = ElementMaterial.MaterialValues[0];
        var nue = ElementMaterial.MaterialValues[1];
        var factor = emod / (1.0 + nue); // E/(1+nue)

        e[0, 0] = e[1, 1] = e[2, 2] = factor * (1.0 - nue) / (1.0 - 2.0 * nue);
        e[3, 3] = e[4, 4] = e[5, 5] = factor * 0.5;
        e[0, 1] = e[0, 2] = e[1, 2] =
            e[1, 0] = e[2, 0] = e[2, 1] = factor * nue / (1.0 - 2.0 * nue);
    }

    // --- Elementverhalten ----------------------------------

    // ....Berechne Elementspannungen: sigma = E * B * Ue (Elementverformungen) ......
    public override double[] ComputeStateVector()
    {
        for (var i = 0; i < 8; i++)
        {
            var nodalDof = i * ElementDof;
            elementDeformations[nodalDof] = Nodes[i].NodalDof[0];
            elementDeformations[nodalDof + 1] = Nodes[i].NodalDof[1];
            elementDeformations[nodalDof + 2] = Nodes[i].NodalDof[2];
        }

        var temp = MatrixAlgebra.Mult(e, b);
        var elementStresses = MatrixAlgebra.Mult(temp, elementDeformations);
        return elementStresses;
    }

    public override double[] ComputeElementState(double z0, double z1, double z2)
    {
        var elementStresses = new double[6];
        return elementStresses;
    }

    public override void SetSystemIndicesOfElement()
    {
        SystemIndicesOfElement = new int[NodesPerElement * ElementDof];
        var counter = 0;
        for (var i = 0; i < NodesPerElement; i++)
            for (var j = 0; j < ElementDof; j++)
                SystemIndicesOfElement[counter++] = Nodes[i].SystemIndices[j];
    }

    public override Point3D ComputeCenterOfGravity3D()
    {
        if (!model.Elements.TryGetValue(ElementId, out element))
            throw new ModelException("Element3D8: " + ElementId + " nicht im Modell gefunden");
        element.SetReferences(model);
        return CenterOfGravity3D(element);
    }
}