namespace FEALibrary.Model
{
    public class NodalValues
    {
        public NodalValues(string nodeId, double[] values)
        {
            NodeId = nodeId;
            Values = values;
        }

        public string NodeId { get; set; }
        public double[] Values { get; set; }
    }
}