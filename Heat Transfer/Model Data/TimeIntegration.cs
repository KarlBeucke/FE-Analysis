using FEALibrary.Model.abstractClasses;
using System.Collections.Generic;

namespace FE_Analysis.Heat_Transfer.Model_Data
{
    public class TimeIntegration : AbstractTimeintegration
    {
        public TimeIntegration(double tmax, double dt, double alfa)
        {
            Tmax = tmax;
            Dt = dt;
            Parameter1 = alfa;
            InitialConditions = new List<object>();
        }
    }
}