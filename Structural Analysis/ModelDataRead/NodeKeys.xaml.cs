using FEALibrary.Model;
using System.Linq;

namespace FE_Analysis.Structural_Analysis.ModelDataRead;

public partial class NodeKeys
{
    public NodeKeys(FeModel model)
    {
        InitializeComponent();
        this.Left = 2 * this.Width;
        this.Top = this.Height;
        var node = model.Nodes.Select(item => item.Value).ToList();
        NodeKey.ItemsSource = node;
    }
}