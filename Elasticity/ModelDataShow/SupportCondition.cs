namespace FE_Analysis.Elasticity.ModelDataShow
{
    internal class SupportCondition
    {
        public SupportCondition(string supportId, string nodeId, string[] predefined)
        {
            SupportId = supportId;
            NodeId = nodeId;
            Predefined = predefined;
        }

        public string SupportId { get; }
        public string NodeId { get; }
        public string[] Predefined { get; }
    }
}