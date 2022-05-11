using System.Collections.Generic;

namespace FEALibrary.Model.abstractClasses
{
    public abstract class AbstractTimeintegration
    {
        public string Id { get; set; }
        public double Tmax { get; set; }
        public double Dt { get; set; }
        public int Method { get; set; }
        public bool FromStationary { get; set; }
        public double Parameter1 { get; set; }
        public double Parameter2 { get; set; }
        public List<object> InitialConditions { get; set; }
    }
}