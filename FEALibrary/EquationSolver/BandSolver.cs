using System;

namespace FEALibrary.EquationSolver
{
    public class BandSolver
    {
        private const double Epsilon = 1.0e-20;
        private readonly int band;
        private readonly int dimension;
        private readonly double[][] matrix; // system stiffness matrix A
        private readonly double[] vector; // system load vector and result

        private int s, m;

        //... Constructor ........................................................

        public BandSolver(double[][] systemMatrix, int systemBand, double[] systemVector)
        {
            dimension = vector.Length;
            matrix = systemMatrix;
            vector = systemVector;
            band = systemBand;
        }

        //... triangularization() ........................................................

        public bool Decompose()
        {
            // A[i][m] = A[i][m] - Sum(A[i][k]*A[k][m]) / A[k][k]
            for (s = 0; s < dimension; s++)
            {
                var start = Math.Max(0, s - band + 1);
                double sum;
                for (m = start; m < s; m++)
                {
                    var start1 = Math.Max(0, m - band + 1);
                    var start2 = Math.Max(start, start1);
                    sum = matrix[s][m - start];
                    for (var k = start2; k < m; k++)
                        sum -= matrix[s][k - start] * matrix[m][k - start1];
                    matrix[s][m - start] = sum / matrix[m][m - start1];
                }

                // A[i][i] = sqrt{(A[i][i] - Sum(A[i][m]*A[m][i])}
                sum = matrix[s][s - start];
                for (var k = start; k < s; k++)
                    sum -= matrix[s][k - start] * matrix[s][k - start];

                if (sum < Epsilon)
                    throw new AlgebraicException("BandSolver: Element <= 0 in Dreieckszerlegung von Zeile " + s);
                matrix[s][s - start] = Math.Sqrt(sum);
            }

            return true;
        }

        //... solve() ............................................................

        public void Solve()
        {
            int start;
            // Compute vector : forward sweep by rows
            for (s = 0; s < dimension; s++)
            {
                start = Math.Max(0, s - band + 1);
                for (m = start; m < s; m++) vector[s] -= matrix[s][m - start] * vector[m];
                vector[s] /= matrix[s][s - start];
            }

            // Compute vector : backward sweep by rows
            for (m = dimension - 1; m >= 0; m--)
            {
                start = Math.Max(0, m - band + 1);
                vector[m] /= matrix[m][m - start];
                for (s = start; s < m; s++) vector[s] -= matrix[m][s - start] * vector[m];
            }
        }
    }
}