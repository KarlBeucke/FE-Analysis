namespace FE_Analysis.Structural_Analysis.ModelDataShow
{
    public class TimeInterval
    {
        public TimeInterval(string nodeId, double time, double force)
        {
            NodeId = nodeId;
            Time = time;
            Force = force;
        }

        public string NodeId { get; set; }
        public double Time { get; set; }
        public double Force { get; set; }
    }
}