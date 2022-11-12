using FE_Analysis.Structural_Analysis.Model_Data;
using FEALibrary.Model;
using System.Linq;

namespace FE_Analysis.Structural_Analysis.ModelDataRead;

public partial class LineLoadKeys
{
    public LineLoadKeys(FeModel model)
    {
        InitializeComponent();
        this.Left = 2 * this.Width;
        this.Top = this.Height;
        var loads = model.ElementLoads.
            Where(item => item.Value is LineLoad).
            Select(item => item.Value).ToList();
        LineloadKeys.ItemsSource = loads;
    }
}