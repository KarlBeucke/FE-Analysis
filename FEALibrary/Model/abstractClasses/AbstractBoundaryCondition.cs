namespace FEALibrary.Model.abstractClasses
{
    public abstract class AbstractBoundaryCondition
    {
        public string SupportId { get; set; }
        public string NodeId { get; set; }
        public Node Node { get; set; }
        public int Type { get; set; }
        public double[] Prescribed { get; set; }
        public bool[] Restrained { get; set; }

        public void SetReferences(FeModel modell)
        {
            if (NodeId != null)
            {
                if (modell.Nodes.TryGetValue(NodeId, out var node)) Node = node;

                if (node == null)
                    throw new ModelException("Node with ID = " + NodeId + " is not contained in Model");
            }
            else
            {
                throw new ModelException("Nodal Identifier for Boundarz Condition " + SupportId +
                                         " is not defined");
            }
        }
    }
}