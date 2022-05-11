using FEALibrary.Model.abstractClasses;
using System.Collections.Generic;

namespace FE_Analysis.Heat_Transfer.Model_Data
{
    public class TimeIntegration : AbstractTimeintegration
    {
        //private double[] initial;
        //private double[][] forceFunction;
        //public double[] Initial { get { return initial; } set { initial = value; } }
        //public double[][] ForceFunction { get { return forceFunction; } set { forceFunction = value; } }

        public TimeIntegration()
        {
        }

        public TimeIntegration(double tmax, double dt, double alfa)
        {
            Tmax = tmax;
            Dt = dt;
            Parameter1 = alfa;
            InitialConditions = new List<object>();
        }
    }
}