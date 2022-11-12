using FEALibrary.Model;
using System.Linq;

namespace FE_Analysis.Structural_Analysis.ModelDataRead;

public partial class NodeLoadKeys
{
    public NodeLoadKeys(FeModel model)
    {
        InitializeComponent();
        this.Left = 2 * this.Width;
        this.Top = this.Height;
        var loads = model.Loads.Select(item => item.Value).ToList();
        LoadKey.ItemsSource = loads;
    }
}