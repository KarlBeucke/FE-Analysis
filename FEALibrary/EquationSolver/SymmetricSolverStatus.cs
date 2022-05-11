using System;

namespace FEALibrary.EquationSolver
{
    //--------------------------------------------------------------------
    //  CLASS : SymmetricSolverStatus                 Linear System of Equations
    //--------------------------------------------------------------------
    //  FUNCTION :
    //
    //  solution of a linear system of
    //  equations with symmetric structure :
    //
    //      A * u = w + q
    //
    //  A   system matrix with given coefficients
    /// u   primal solution vector
    //  q   dual solution vector
    //  w   system vector with given coefficients
    //
    //  In each row, either u[i] or q[i] is given .
    //
    //-------------------------------------------------------------------
    //  METHODS :
    //
    //        Equation  (int dimension)       Constructor
    //  void  triangularization()             triangularize A
    //  void  solve()                         solve the equations
    //
    //-------------------------------------------------------------------
    internal class SymmetricSolverStatus
    {
        private const double Epsilon = 1.0e-20;
        private readonly int dimension;

        private readonly double[] dual; // dual   solution vector

        // false : dual   prescribed
        private readonly double[][] matrix; // system matrix A
        private readonly double[] primal; // primal solution vector

        private readonly bool[] status; // true  : primal prescribed

        private readonly double[] vector; // system vector w
        //double[] check;                 // check results: A * primal

        private int row, column;

        public SymmetricSolverStatus(double[][] systemMatrix, double[] systemVector,
            double[] systemPrimal, double[] systemDual,
            bool[] systemStatus)
        {
            matrix = systemMatrix;
            vector = systemVector;
            primal = systemPrimal;
            dual = systemDual;
            status = systemStatus;
            dimension = vector.Length;
        }

        //..Read/write a coefficient in the system matrix....................
        private double GetValue(int i, int m)
        {
            return matrix[i][m];
        }

        private void SetValue(int i, int m, double value)
        {
            matrix[i][m] = value;
        }

        //__DECOMPOSE THE SYSTEM MATRIX______________________________________
        public void Decompose()
        {
            //..A[i][m] = A[i][m] - Sum(A[i][k]*A[k][m]) / A[k][k]...............
            for (row = 0; row < dimension; row++)
            {
                if (status[row]) continue;
                double sum;
                for (column = 0; column < row; column++)
                {
                    if (status[column]) continue;
                    sum = GetValue(row, column);
                    for (var m = 0; m < column; m++)
                    {
                        if (status[m]) continue;
                        sum -= GetValue(row, m) * GetValue(column, m);
                    }

                    sum /= GetValue(column, column);

                    SetValue(row, column, sum);
                }

                //..A[i][i] = sqr{(A[i][i] - Sum(A[i][m]*A[m][i])}...................
                sum = GetValue(row, row);
                for (var m = 0; m < row; m++)
                {
                    if (status[m]) continue;
                    sum -= GetValue(row, m) * GetValue(row, m);
                }

                if (sum < Epsilon)
                    throw new AlgebraicException("SymmetricSolverStatus: Element <= 0 bei Dreieckszerlegung in Zeile " +
                                                 row);
                SetValue(row, row, Math.Sqrt(sum));
            }
        }

        //__SOLVE THE SYSTEM EQUATIONS_______________________________________
        //..Substitute the prescribed variables in the rows without
        //  prescribed primary variables : u = c1 + y1 - A12 * x2
        public void Solve()
        {
            for (row = 0; row < dimension; row++)
            {
                if (status[row]) continue;
                primal[row] = vector[row] + dual[row];
                for (column = 0; column < row; column++)
                {
                    if (!status[column]) continue;
                    primal[row] -= GetValue(row, column) * primal[column];
                }
            }

            for (column = 0; column < dimension; column++)
            {
                if (!status[column]) continue;
                for (row = 0; row < column; row++)
                {
                    if (status[row]) continue;
                    primal[row] -= GetValue(column, row) * primal[column];
                }
            }

            //..Compute primal variables : forward sweep by rows.................
            for (row = 0; row < dimension; row++)
            {
                if (status[row]) continue;
                for (column = 0; column < row; column++)
                {
                    if (status[column]) continue;
                    primal[row] -= GetValue(row, column) * primal[column];
                }

                primal[row] /= GetValue(row, row);
            }

            //..Compute primal variables : backward sweep by rows................
            for (column = dimension - 1; column >= 0; column--)
            {
                if (status[column]) continue;
                primal[column] /= GetValue(column, column);
                for (row = 0; row < column; row++)
                {
                    if (status[row]) continue;
                    primal[row] -= GetValue(column, row) * primal[column];
                }
            }

            //..Compute dual variables : substitute the primal variables.........
            //  in the rows with prescribed primal variables :
            for (row = 0; row < dimension; row++)
            {
                if (!status[row]) continue;
                dual[row] = -vector[row];
                for (column = 0; column <= row; column++)
                    dual[row] += GetValue(row, column) * primal[column];
            }

            for (column = 0; column < dimension; column++)
            for (row = 0; row < column; row++)
            {
                if (!status[row]) continue;
                dual[row] += GetValue(column, row) * primal[column];
            }
        }
    }
}