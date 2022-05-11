using System.Collections.Generic;

namespace FEALibrary.Model
{
    public class Eigenstates
    {
        // Properties
        public string Id { get; set; }
        public int NumberOfStates { get; set; }
        public double[] Eigenvalues { get; set; }
        public double[][] Eigenvectors { get; set; }
        public bool[] Status { get; set; }
        public List<object> DampingRatios { get; set; }

        // ....Constructor....................................................
        public Eigenstates(string id, int numberOfStates)
        {
            Id = id;
            NumberOfStates = numberOfStates;
            DampingRatios = new List<object>();
        }
    }
}