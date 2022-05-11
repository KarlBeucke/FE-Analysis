using System.Collections.Generic;

namespace FE_Analysis.Structural_Analysis.Results
{
    public class BeamEndForces
    {
        public BeamEndForces(string elementId, IReadOnlyList<double> beamEndForces)
        {
            ElementId = elementId;
            switch (beamEndForces.Count)
            {
                case 2:
                    Na = beamEndForces[0];
                    Nb = beamEndForces[1];
                    break;
                case 6:
                    Na = beamEndForces[0];
                    Qa = beamEndForces[1];
                    Ma = beamEndForces[2];
                    Nb = beamEndForces[3];
                    Qb = beamEndForces[4];
                    Mb = beamEndForces[5];
                    break;
            }
        }

        public string ElementId { get; set; }
        public double Na { get; set; }
        public double Qa { get; set; }
        public double Ma { get; set; }
        public double Nb { get; set; }
        public double Qb { get; set; }
        public double Mb { get; set; }
    }
}