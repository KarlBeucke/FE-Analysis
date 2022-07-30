using System;

namespace FEALibrary.EquationSolver
{
    //--------------------------------------------------------------------
    //  CLASS : ProfileSolverStatus             Linear System of Equations
    //--------------------------------------------------------------------
    //  FUNCTION :
    //
    //  Construction and solution of a linear system of
    //  equations with symmetric profile structure :
    //
    //      A * u = w + q
    //
    //  A   system matrix with given coefficients
    /// u   primal solution vector  (result vector)
    //  q   dual solution vector     (vector of boundary reactions)
    //  w   system vector with given coefficients  (RHS vector)
    //
    //  In each row, either u[i] or q[i] is given .
    //
    //-------------------------------------------------------------------
    //  METHODS :
    //
    // public ProfileSolverStatus(double matrix [][], double vector [],
    // double primal[], double dual[],
    // boolean status[], int profile[])
    //
    // public ProfileSolverStatus(double matrix [][],
    // double primal[], double dual[],
    // boolean status[], int profile[])
    //
    //  public void SetRHS(double [] newVector)
    //  public void Decompose() throws AlgebraicException
    //  public void Solve()
    //
    //-------------------------------------------------------------------
    public class ProfileSolverStatus
    {
        private const double Epsilon = 1.0e-20;
        private readonly int dimension;
        private readonly double[] dual; // dual   solution vector
        private readonly double[][] matrix; // system matrix A

        private readonly double[] primal; // primal solution vector

        // false : dual   prescribed
        private readonly int[] profile; // index of 1. column != 0
        private readonly bool[] status; // true  : primal prescribed
        private int row, column;
        private double[] vector; // system vector w

        //..Construction of the system of equations..........................
        public ProfileSolverStatus(double[][] mat, double[] vec,
            double[] prim, double[] dua,
            bool[] stat, int[] prof)
        {
            matrix = mat;
            vector = vec;
            primal = prim;
            dual = dua;
            status = stat;
            profile = prof;
            dimension = matrix.Length;
        }

        //..if there are no dual conditions
        public ProfileSolverStatus(double[][] mat, double[] vec,
            double[] prim,
            bool[] stat, int[] prof)
        {
            matrix = mat;
            vector = vec;
            primal = prim;
            status = stat;
            profile = prof;
            dimension = matrix.Length;
        }

        //..if matrix is only to be decomposed
        public ProfileSolverStatus(double[][] mat,
            bool[] stat, int[] prof)
        {
            matrix = mat;
            status = stat;
            profile = prof;
            dimension = matrix.Length;
        }

        public void SetRhs(double[] newVector)
        {
            vector = newVector;
        }

        // triangularization of the system matrix .........................
        public void Decompose()
        {
            //..A[i][m] = A[i][m] - Sum(A[i][k]*A[k][m]) / A[k][k]..........
            for (row = 0; row < dimension; row++)
            {
                if (status[row]) continue;
                double sum;
                for (column = profile[row]; column < row; column++)
                {
                    if (status[column]) continue;
                    var start = Math.Max(profile[row], profile[column]);
                    sum = matrix[row][column - profile[row]];
                    for (var m = start; m < column; m++)
                    {
                        if (status[m]) continue;
                        sum -= matrix[row][m - profile[row]] * matrix[column][m - profile[column]];
                    }

                    sum /= matrix[column][column - profile[column]];
                    matrix[row][column - profile[row]] = sum;
                }

                //..A[i][i] = sqrt{(A[i][i] - Sum(A[i][m]*A[m][i])}...................
                sum = matrix[row][row - profile[row]];
                for (var m = profile[row]; m < row; m++)
                {
                    if (status[m]) continue;
                    sum -= matrix[row][m - profile[row]] * matrix[row][m - profile[row]];
                }

                if (sum < Epsilon)
                    throw new AlgebraicException("Gleichungslöser: Element <= 0 in Dreieckszerlegung von Zeile " + row);
                matrix[row][row - profile[row]] = Math.Sqrt(sum);
            }
        }

        //__SOLVE THE SYSTEM EQUATIONS_______________________________________
        //..Substitute the prescribed variables in the rows without
        //  prescribed primary variables : u = c1 + y1 - A12 * x2
        public void Solve()
        {
            SolvePrimal();
            SolveDual();
        }

        public void SolvePrimal()
        {
            for (row = 0; row < dimension; row++)
            {
                if (status[row]) continue;
                primal[row] = vector[row];
                for (column = profile[row]; column < row; column++)
                {
                    if (!status[column]) continue;
                    primal[row] -= matrix[row][column - profile[row]] * primal[column];
                }
            }

            for (column = 0; column < dimension; column++)
            {
                if (!status[column]) continue;
                for (row = profile[column]; row < column; row++)
                {
                    if (status[row]) continue;
                    primal[row] -= matrix[column][row - profile[column]] * primal[column];
                }
            }

            //..Compute primal variables : forward sweep by rows.................
            for (row = 0; row < dimension; row++)
            {
                if (status[row]) continue;
                for (column = profile[row]; column < row; column++)
                {
                    if (status[column]) continue;
                    primal[row] -= matrix[row][column - profile[row]] * primal[column];
                }

                primal[row] /= matrix[row][row - profile[row]];
            }

            //..Compute primal variables : backward sweep by rows................
            for (column = dimension - 1; column >= 0; column--)
            {
                if (status[column]) continue;
                primal[column] /= matrix[column][column - profile[column]];
                for (row = profile[column]; row < column; row++)
                {
                    if (status[row]) continue;
                    primal[row] -= matrix[column][row - profile[column]] * primal[column];
                }
            }
        }

        private void SolveDual()
        {
            //..Compute dual variables : substitute the primal variables.........
            //  in the rows with prescribed primal variables :
            for (row = 0; row < dimension; row++)
            {
                if (!status[row]) continue;
                dual[row] = -vector[row];
                for (column = profile[row]; column <= row; column++)
                    dual[row] += matrix[row][column - profile[row]] * primal[column];
            }

            for (column = 0; column < dimension; column++)
                for (row = profile[column]; row < column; row++)
                {
                    if (!status[row]) continue;
                    dual[row] += matrix[column][row - profile[column]] * primal[column];
                }
        }
    }
}