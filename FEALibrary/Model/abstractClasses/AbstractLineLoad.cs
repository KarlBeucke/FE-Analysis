namespace FEALibrary.Model.abstractClasses
{
    public abstract class AbstractLineLoad : AbstractElementLoad
    {
        public string StartNodeId { get; set; }
        public Node StartNode { get; set; }
        public string EndNodeId { get; set; }
        public Node EndNode { get; set; }
    }
}