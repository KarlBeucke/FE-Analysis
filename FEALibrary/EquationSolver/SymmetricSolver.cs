using System;

namespace FEALibrary.EquationSolver
{
    internal class SymmetricSolver
    {
        private const double Epsilon = 1.0e-20; //error bound
        private readonly double[][] matrix; //coefficient matrix, will be changed during solution
        private readonly double[] vector; //coefficient vector (includes solution at the end)
        private int dimension; //dimension of quadratic matrix

        public SymmetricSolver(double[][] systemMatrix, double[] systemVector)
        {
            matrix = systemMatrix;
            vector = systemVector;
            dimension = vector.Length;
        }

        public bool Decompose()
        {
            //triangularization of coefficient matrix A = U * U^T
            int s;

            for (s = 0; s < dimension; s++)
            {
                // evaluate non-diagonal-elements in row "s"
                int k;
                int m;
                double sum;
                for (m = 0; m < s; m++)
                {
                    sum = matrix[s][m];
                    for (k = 0; k < m; k++) sum -= matrix[s][k] * matrix[m][k];
                    matrix[s][m] = sum / matrix[m][m];
                }

                // evaluate diagonal element in row "s"
                sum = matrix[s][s];
                for (k = 0; k < s; k++) sum -= matrix[s][k] * matrix[s][k];

                // check diagonal element in row "s"
                if (Math.Abs(sum) < Epsilon)
                    throw new AlgebraicException("SymmetricSolver: Pivot in Zeile " + s +
                                                 " kleiner als Fehlerschranke");
                if (sum < 0)
                    throw new AlgebraicException(
                        "SymmetricSolver: negtive Wurzel eines Elementes auf der Hauptdiagonale");
                matrix[s][s] = Math.Sqrt(sum);
            }

            return true;
        }

        // solve system of linear equations
        public double[] Solve()
        {
            int s, k;

            // forward solution rowwise in U
            for (s = 0; s < dimension; s++)
            {
                for (k = 0; k < s; k++)
                    vector[s] -= matrix[s][k] * vector[k];
                vector[s] /= matrix[s][s];
            }

            // backward solution
            for (s = dimension - 1; s >= 0; s--)
            {
                for (k = s + 1; k < dimension; k++) vector[s] -= matrix[k][s] * vector[k];
                vector[s] /= matrix[s][s];
            }

            return vector;
        }

        // Multiplication: result = matrix * vector (with matrix in symmetric format)
        public double[] Mult(double[][] a, double[] prim)
        {
            dimension = prim.Length;
            var result = new double[dimension];
            for (var i = 0; i < dimension; i++)
            {
                result[i] = 0;
                for (var k = 0; k <= i; k++)
                    result[i] += a[i][k] * prim[k];
                for (var k = i + 1; k < dimension; k++)
                    result[i] += a[k][i] * prim[k];
            }

            return result;
        }
    }
}