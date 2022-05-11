using FEALibrary.EquationSolver;
using FEALibrary.Model;
using FEALibrary.Utils;

namespace FEALibrary.DynamicSolver
{
    internal class TimeIntegration2NdOrderStatus
    {
        private readonly double[] c;
        private readonly int dimension, method;
        private readonly double dt, parameter1, parameter2;
        private readonly double[][] k, forcingFunction;
        private readonly double[] m;
        private readonly int[] profile;
        private readonly bool[] status;
        public double[][] displacement, velocity, acceleration;

        public TimeIntegration2NdOrderStatus(Equations systemEquations, double[] damping,
            double dt, int method, double parameter1, double parameter2, double[][] displ, double[][] veloc,
            double[][] anregung)
        {
            m = systemEquations.DiagonalMatrix;
            c = damping;
            k = systemEquations.Matrix;
            profile = systemEquations.Profile;
            status = systemEquations.Status;
            this.dt = dt;
            this.method = method;
            this.parameter1 = parameter1;
            this.parameter2 = parameter2;
            displacement = displ;
            velocity = veloc;
            forcingFunction = anregung;
            dimension = k.Length;
        }

        public TimeIntegration2NdOrderStatus(double[] mass, double[] damping, double[][] stiffness,
            int[] profile, bool[] status,
            double dt, int method, double parameter1, double parameter2, double[][] displ, double[][] veloc,
            double[][] anregung)
        {
            m = mass;
            c = damping;
            k = stiffness;
            this.profile = profile;
            this.status = status;
            this.dt = dt;
            this.method = method;
            this.parameter1 = parameter1;
            this.parameter2 = parameter2;
            displacement = displ;
            velocity = veloc;
            forcingFunction = anregung;
            dimension = k.Length;
        }

        public void Perform()
        {
            double alfa, beta, gamma, theta;
            if (method == 1)
            {
                alfa = 0;
                theta = 1;
                beta = parameter1;
                gamma = parameter2;
            }
            else if (method == 2)
            {
                beta = 1.0 / 6;
                gamma = 0.5;
                alfa = 0;
                theta = parameter1;
            }
            else if (method == 3)
            {
                theta = 1;
                alfa = parameter1;
                gamma = 0.5 - alfa;
                beta = 0.25 * (1 - alfa) * (1 - alfa);
            }
            else
            {
                throw new AlgebraicException("TimeIntegration2NdOrderStatus: invalid method identifier entered");
            }

            var gammaDt = gamma * dt;
            var betaDt2 = beta * dt * dt;
            var gammaDtTheta = gamma * dt * theta;
            var dt1MGamma = dt * (1 - gamma);
            var dt2MBetaDt2 = dt * dt / 2 - beta * dt * dt;
            var thetaDt = theta * dt;
            var thetaDt1MGamma = theta * dt * (1 - gamma);
            var theta2Dt2MBetaDt2 = theta * theta * dt2MBetaDt2;
            var betaDt2Theta2AlfaP1 = beta * dt * dt * theta * theta * (1 + alfa);

            var primal = new double[dimension];
            var dual = new double[dimension];
            var timeSteps = displacement.Length;
            acceleration = new double[timeSteps][];
            for (var i = 0; i < timeSteps; i++)
                acceleration[i] = new double[dimension];

            // calculate initial accelerations at unrestrained dof, for M[i]>0
            var rhs = MatrixAlgebra.Mult(k, displacement[0], status, profile);
            for (var i = 0; i < dimension; i++)
            {
                // if (status[i]) continue; ODER wenn M[i]=0 continue --> RHS[i]=0
                if (status[i] | (m[i] == 0)) continue;
                rhs[i] = (forcingFunction[0][i] - c[i] * velocity[0][i] - rhs[i]) / m[i];
                acceleration[0][i] = rhs[i];
            }

            // constant coefficient matrix
            var cm = new double[dimension][];
            for (var row = 0; row < dimension; row++)
            {
                cm[row] = new double[row + 1 - profile[row]];
                for (var col = 0; col <= row - profile[row]; col++)
                    cm[row][col] = betaDt2Theta2AlfaP1 * k[row][col];
                cm[row][row - profile[row]] += m[row] + gammaDtTheta * c[row];
            }

            var profileSolverStatus = new ProfileSolverStatus
                (cm, rhs, primal, dual, status, profile);
            profileSolverStatus.Decompose();

            for (var counter = 1; counter < timeSteps; counter++)
            {
                // calculate displacement(hat) and velocity(hat) at n+1
                for (var i = 0; i < dimension; i++)
                {
                    displacement[counter][i] = displacement[counter - 1][i]
                                               + thetaDt * velocity[0][i]
                                               + theta2Dt2MBetaDt2 * acceleration[counter - 1][i];
                    velocity[1][i] = velocity[0][i] + thetaDt1MGamma * acceleration[counter - 1][i];
                }

                // calculate new RHS
                for (var i = 0; i < dimension; i++)
                    rhs[i] = (1 + alfa) * displacement[counter][i] - alfa * displacement[counter - 1][i];
                rhs = MatrixAlgebra.Mult(k, rhs, status, profile);
                for (var i = 0; i < dimension; i++)
                    if (!status[i])
                        rhs[i] = (1 - theta) * forcingFunction[counter - 1][i]
                                 + theta * forcingFunction[counter][i]
                                 - c[i] * velocity[1][i] - rhs[i];

                // backsubstitution
                profileSolverStatus.SetRhs(rhs);
                profileSolverStatus.SolvePrimal();

                // displacements, velocities and accelerations at next time step
                for (var i = 0; i < dimension; i++)
                {
                    if (status[i]) continue;
                    acceleration[counter][i] = acceleration[counter - 1][i]
                                               + (primal[i]
                                                  - acceleration[counter - 1][i]) / theta;
                    displacement[counter][i] = displacement[counter - 1][i]
                                               + dt * velocity[0][i]
                                               + dt2MBetaDt2 * acceleration[counter - 1][i]
                                               + betaDt2 * acceleration[counter][i];
                    velocity[0][i] = velocity[0][i]
                                     + dt1MGamma * acceleration[counter - 1][i]
                                     + gammaDt * acceleration[counter][i];
                }
            }
        }
    }
}