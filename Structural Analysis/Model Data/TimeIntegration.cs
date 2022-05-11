using FEALibrary.Model.abstractClasses;
using System.Collections.Generic;

namespace FE_Analysis.Structural_Analysis.Model_Data
{
    public class TimeIntegration : AbstractTimeintegration
    {
        public TimeIntegration(double tmax, double dt, short method, double parameter1)
        {
            Tmax = tmax;
            Dt = dt;
            Method = method;
            Parameter1 = parameter1;
            InitialConditions = new List<object>();
        }

        public TimeIntegration(double tmax, double dt, short method, double parameter1, double parameter2)
        {
            Tmax = tmax;
            Dt = dt;
            Method = method;
            Parameter1 = parameter1;
            Parameter2 = parameter2;
            InitialConditions = new List<object>();
        }
    }
}