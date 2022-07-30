using FEALibrary.EquationSolver;
using FEALibrary.Utils;
using System;

namespace FEALibrary.DynamicSolver
{
    public class Eigensolver
    {
        private const double RaleighFactor = 1.0e-3;
        private const int SMax = 200;

        private readonly double[][] a; // coefficients of matrix A
        private readonly double[][] b; // coefficients of matrix B
        private readonly int dimension;

        private readonly int[] profile; // row profile
        private readonly bool[] status; // true: displacement prescribed
        private readonly double[] w; // w[s]  = m[s] z[s]
        private readonly double[] y; // y[s]  = A[-1] w[s-1]
        private double deltaRaleigh; // rNew - rOld
        private double[] eigenValue;
        private double m2; // m2[s] = 1 / (y[s,t] z[s])
        private int numberOfStates; // currently computed
        private double[][] p; // w = B * x
        private double raleigh; // r     = m2[s] y[s,t] w[s-1]

        private int state, s, row;

        private double[][] x; // normalised eigenvectors x
        private double[] z; // z[s]  = B y[s]

        //... Constructors ......................................................
        public Eigensolver(double[][] mA, double[][] mB,
            int[] mProfile, bool[] mStatus, int mNumberOfStates)
        {
            a = mA;
            b = mB;
            profile = mProfile;
            status = mStatus;
            numberOfStates = mNumberOfStates;

            dimension = a.Length;
            z = new double[dimension];
            w = new double[dimension];
            y = new double[dimension];
        }

        //... get() .............................................................
        public double GetEigenValue(int index)
        {
            return eigenValue[index];
        }

        public double[] GetEigenVector(int index)
        {
            return x[index];
        }

        //... solveEigenstates() ....................................................
        public void SolveEigenstates()
        {
            // allocate the solution vectors		
            x = new double[numberOfStates][];
            p = new double[numberOfStates][];
            for (var i = 0; i < numberOfStates; i++)
            {
                x[i] = new double[dimension];
                p[i] = new double[dimension];
            }

            eigenValue = new double[numberOfStates];

            // reduce the number of eigenvalues to the maximum possible number
            var counter = 0;
            for (row = 0; row < dimension; row++)
                if (status[row])
                    counter++;
            if (numberOfStates > dimension - counter)
                numberOfStates = dimension - counter;

            var profileSolverStatus =
                new ProfileSolverStatus(a, w, y, status, profile);

            // loop over the specified number of eigenstates
            for (state = 0; state < numberOfStates; state++)
            {
                raleigh = 0;
                s = 0;
                // set start vector w0
                for (row = 0; row < dimension; row++)
                    if (status[row]) w[row] = 0;
                    else w[row] = 1;

                // start iteration for next eigenstate
                double m;
                do
                {
                    // increment iteration counter
                    s++;
                    // check if number of iterations is greater Smax
                    if (s > SMax) throw new AlgebraicException("Eigensolver: too many iterations " + s);

                    // B-orthogonalization of w(s-1) with respect to all smaller 
                    // eigenvectors x[0] to x[state-1]
                    for (var i = 0; i < state; i++)
                    {
                        var c = 0.0;
                        // compute c(i) and subtract c(i)*p(i) from w
                        for (row = 0; row < dimension; row++)
                            if (!status[row])
                                c += w[row] * x[i][row];
                        for (row = 0; row < dimension; row++)
                            if (!status[row])
                                w[row] -= c * p[i][row];
                    }

                    // solve A * y(s) = w(s-1) for y(s)
                    profileSolverStatus.SetRhs(w);
                    profileSolverStatus.SolvePrimal();

                    // compute z(s) = B * y(s)
                    z = MatrixAlgebra.Mult(b, y, status, profile);

                    // compute m2 = 1 / (y[s] * z[s])
                    double sum = 0;
                    for (row = 0; row < dimension; row++)
                        if (!status[row])
                            sum += y[row] * z[row];
                    m2 = 1 / sum;


                    //	compute Rayleigh quotient r = m2 * y(s)(T) * w(s-1)
                    // and the difference ( r(s) - r(s-1) )
                    sum = 0;
                    for (row = 0; row < dimension; row++)
                        if (!status[row])
                            sum += y[row] * w[row];
                    sum *= m2;
                    deltaRaleigh = sum - raleigh;
                    raleigh = sum;

                    //	compute w(s) = m(s) * z(s)
                    m = Math.Sqrt(Math.Abs(m2));
                    for (row = 0; row < dimension; row++)
                        if (!status[row])
                            w[row] = m * z[row];

                    // continue iteration as long as change in Rayleigh factor (r(s)-r(s-1)
                    // is greater than error bound
                } while (Math.Abs(deltaRaleigh) > Math.Abs(RaleighFactor * raleigh));

                // store computed eigenstate and vector p=w=Bx for B-orthogonalization
                eigenValue[state] = raleigh;
                for (row = 0; row < dimension; row++)
                {
                    x[state][row] = m * y[row];
                    p[state][row] = w[row];
                }
            }
        }
    }
}