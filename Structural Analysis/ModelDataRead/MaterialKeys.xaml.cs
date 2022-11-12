using FEALibrary.Model;
using System.Linq;

namespace FE_Analysis.Structural_Analysis.ModelDataRead;

public partial class MaterialKeys
{
    public MaterialKeys(FeModel model)
    {
        InitializeComponent();
        this.Left = 2 * this.Width;
        this.Top = this.Height;
        var material = model.Material.Select(item => item.Value).ToList();
        MaterialKey.ItemsSource = material;
    }
}