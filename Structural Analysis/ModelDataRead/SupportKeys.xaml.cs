using FEALibrary.Model;
using System.Linq;

namespace FE_Analysis.Structural_Analysis.ModelDataRead;


public partial class SupportKeys
{
    public SupportKeys(FeModel model)
    {
        InitializeComponent();
        this.Left = 2 * this.Width;
        this.Top = this.Height;
        var support = model.BoundaryConditions.Select(item => item.Value).ToList();
        SupportKey.ItemsSource = support;
    }
}