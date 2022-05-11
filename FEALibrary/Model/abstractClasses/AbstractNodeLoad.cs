using System.Windows;

namespace FEALibrary.Model.abstractClasses
{
    public abstract class AbstractNodeLoad : AbstractLoad
    {
        public Node Node { get; set; }

        public int NodalDof { get; set; }

        public void SetReferences(FeModel modell)
        {
            if (NodeId == "ground") return;
            if (modell.Nodes.TryGetValue(NodeId, out var node)) { }

            if (node != null) return;
            var message = "Node with ID=" + NodeId + " is not contained in Model";
            _ = MessageBox.Show(message, "AbstractNodeLoad");
        }
    }
}