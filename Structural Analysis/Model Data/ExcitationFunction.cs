namespace FE_Analysis.Structural_Analysis.Model_Data
{
    internal class ExcitationFunction
    {
        private readonly int dimension;
        private readonly double dt;
        private readonly int nSteps;
        private double[][] f;
        private double time;

        public ExcitationFunction(double dt, int nSteps, int dimension)
        {
            this.dt = dt;
            this.nSteps = nSteps;
            this.dimension = dimension;
        }

        public double[][] GetForce()
        {
            f = new double[nSteps + 1][];
            for (var i = 0; i < nSteps + 1; i++) f[i] = new double[dimension];
            const double t1 = 0.8;

            for (var counter = 1; counter < nSteps; counter++)
            {
                time += dt;
                double force;
                if ((time >= 0) & (time <= t1)) force = time / t1;
                else if ((time > t1) & (time <= 2 * t1)) force = 2 - time / t1;
                else if ((time > 2 * t1) & (time <= 4 * t1)) force = 1 - time / (2 * t1);
                else if ((time > 4 * t1) & (time <= 6 * t1)) force = -3 + time / (2 * t1);
                else if ((time > 6 * t1) & (time <= 7 * t1)) force = -6 + time / t1;
                else if ((time > 7 * t1) & (time <= 8 * t1)) force = 8 - time / t1;
                else force = 0;
                for (var i = 0; i < dimension; i++)
                    f[counter][i] = force;
            }

            return f;
        }
    }
}