using System;

namespace FEALibrary.EquationSolver
{
    public class GaussSolver
    {
        private const double Epsilon = 1.0e-20; //error bound
        private readonly int dimension; //dimension of quadratic matrix
        private double[][] matrix; //coefficient matrix, will be changed during solution
        private double[] vector; //coefficient vector (includes solution at the end)

        public GaussSolver(double[][] systemMatrix, double[] systemVector)
        {
            matrix = systemMatrix;
            vector = systemVector;
            dimension = vector.Length;
        }

        public double[] MatrixVectorMultiply(double[][] systemMatrix, double[] systemVector)
        {
            matrix = systemMatrix;
            vector = systemVector;
            var result = new double[vector.Length];
            for (var i = 0; i < vector.Length; i++)
            {
                result[i] = 0;
                for (var k = 0; k < vector.Length; k++)
                    result[i] += matrix[i][k] * vector[k];
            }

            return result;
        }

        public bool Decompose()
        {
            //triangularization of coefficient matrix A = L * R
            int s;

            // evaluate elements in row "s"
            for (s = 1; s < dimension; s++)
            {
                int k;
                int m;
                double sum;
                for (m = 0; m < s; m++)
                {
                    sum = matrix[s][m];
                    for (k = 0; k < m; k++) sum -= matrix[s][k] * matrix[k][m];
                    matrix[s][m] = sum / matrix[m][m];
                }

                // evaluate elements in column "s"
                int i;
                for (i = 0; i <= s; i++)
                {
                    sum = matrix[i][s];
                    for (k = 0; k < i; k++) sum -= matrix[i][k] * matrix[k][s];
                    matrix[i][s] = sum;
                }

                // check diagonal element in row "s"
                if (Math.Abs(matrix[s][s]) < Epsilon)
                    throw new AlgebraicException("GaussSolver: Pivot in Zeile " + s + " kleiner als Fehlerschranke");
            }

            return true;
        }

        // solve system of linear equations
        public double[] Solve()
        {
            int s, k;

            // forward solution
            for (s = 0; s < dimension; s++)
            for (k = 0; k < s; k++)
                vector[s] -= matrix[s][k] * vector[k];

            // backward solution
            for (s = dimension - 1; s >= 0; s--)
            {
                for (k = s + 1; k < dimension; k++) vector[s] -= matrix[s][k] * vector[k];
                vector[s] /= matrix[s][s];
            }

            return vector;
        }
    }
}