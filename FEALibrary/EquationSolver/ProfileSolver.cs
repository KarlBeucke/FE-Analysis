using System;

namespace FEALibrary.EquationSolver
{
    internal class ProfileSolver
    {
        private const double Epsilon = 1.0e-20;
        private int dimension;
        private double[][] matrix; // system stiffness matrix A

        private int[] profile; // index of first element in column != 0

        private int s, m;
        private double[] vector; // system load vector and result

        //... Constructor ........................................................

        public ProfileSolver(double[][] systemMatrix, int[] systemProfile, double[] systemVector)
        {
            dimension = systemVector.Length;
            matrix = systemMatrix;
            profile = systemProfile;
            vector = systemVector;
        }

        //... triangularization() ........................................................

        public bool Decompose()
        {
            // A[i][m] = A[i][m] - Sum(A[i][k]*A[k][m]) / A[k][k]
            for (s = 0; s < dimension; s++)
            {
                double sum;
                for (m = profile[s]; m < s; m++)
                {
                    var start = Math.Max(profile[s], profile[m]);
                    sum = matrix[s][m - profile[s]];
                    for (var k = start; k < m; k++)
                        sum -= matrix[s][k - profile[s]] * matrix[m][k - profile[m]];
                    matrix[s][m - profile[s]] = sum / matrix[m][m - profile[m]];
                }

                // A[i][i] = sqrt{(A[i][i] - Sum(A[i][m]*A[m][i])}
                sum = matrix[s][s - profile[s]];
                for (var k = profile[s]; k < s; k++)
                {
                    if (sum < Epsilon)
                        throw new AlgebraicException("ProfileSolver: Element <= 0 in Dreieckszerlegung von Zeile " + s);
                    matrix[s][s - profile[s]] = Math.Sqrt(sum);
                }
            }

            return true;
        }

        //... solve() ............................................................

        public void Solve()
        {
            // Compute vector : forward sweep by rows
            for (s = 0; s < dimension; s++)
            {
                for (m = profile[s]; m < s; m++) vector[s] -= matrix[s][m - profile[s]] * vector[m];
                vector[s] /= matrix[s][s - profile[s]];
            }

            // Compute vector : backward sweep by rows
            for (m = dimension - 1; m >= 0; m--)
            {
                vector[m] /= matrix[m][m - profile[m]];
                for (s = profile[m]; s < m; s++) vector[s] -= matrix[m][s - profile[m]] * vector[m];
            }
        }

        // Multiplication: result = matrix * vector (with matrix in profile format)
        public double[] Mult(double[][] systemMatrix, double[] systemVector, int[] systemProfile)
        {
            matrix = systemMatrix;
            profile = systemProfile;
            vector = systemVector;
            dimension = matrix.Length;

            var result = new double[dimension];
            for (var i = 0; i < dimension; i++)
            {
                var sum = 0.0;
                for (var k = 0; k <= i - profile[i]; k++)
                    sum += matrix[i][k] * vector[k + profile[i]];
                for (var k = i + 1; k < dimension; k++)
                    if (profile[k] <= i)
                        sum += matrix[k][i - profile[k]] * vector[k];
                result[i] = sum;
            }

            return result;
        }
    }
}