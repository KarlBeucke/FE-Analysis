using FEALibrary.Model.abstractClasses;
using System.Collections.Generic;

namespace FE_Analysis.Heat_Transfer.Model_Data
{
    public class Material : AbstractMaterial
    {
        public Material(string id, IReadOnlyList<double> conduct)
        {
            MaterialId = id;
            MaterialValues = new double[4];
            for (var i = 0; i < conduct.Count; i++) MaterialValues[i] = conduct[i];
        }

        public Material(string id, IReadOnlyList<double> conduct, double rhoC)
        {
            MaterialId = id;
            MaterialValues = new double[4];
            for (var i = 0; i < conduct.Count; i++) MaterialValues[i] = conduct[i];
            MaterialValues[3] = rhoC;
        }
    }
}