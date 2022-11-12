using FEALibrary.Model;
using System.Linq;

namespace FE_Analysis.Structural_Analysis.ModelDataRead;

public partial class CrossSectionKeys
{
    public CrossSectionKeys(FeModel model)
    {
        InitializeComponent();
        this.Left = 2 * this.Width;
        this.Top = this.Height;
        var crossSection = model.CrossSection.Select(item => item.Value).ToList();
        CrossSSectionKey.ItemsSource = crossSection;
    }
}