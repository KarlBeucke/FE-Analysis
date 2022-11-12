using FEALibrary.Model;
using System.Linq;

namespace FE_Analysis.Structural_Analysis.ModelDataRead;

public partial class PointLoadKeys
{
    public PointLoadKeys(FeModel model)
    {
        InitializeComponent();
        this.Left = 2 * this.Width;
        this.Top = this.Height;
        var pointLoads = model.PointLoads.Select(item => item.Value).ToList();
        PointLoadKey.ItemsSource = pointLoads;
    }
}