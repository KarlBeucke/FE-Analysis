using FEALibrary.Model;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace FE_Analysis.Structural_Analysis.ModelDataRead;

public partial class CrossSectionNew
{
    private readonly FeModel model;
    private CrossSection crossSection, existingCrossSection;
    private readonly CrossSectionKeys querschnittKeys;
    public CrossSectionNew(FeModel model)
    {
        InitializeComponent();
        this.model = model;
        querschnittKeys = new CrossSectionKeys(model);
        querschnittKeys.Show();
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var crossSectionId = CrossSectionId.Text;
        if (crossSectionId == "")
        {
            _ = MessageBox.Show("Cross Section Id must be defined", "neuer Querschnitt");
            return;
        }

        // existing cross section
        if (model.CrossSection.Keys.Contains(CrossSectionId.Text))
        {
            model.CrossSection.TryGetValue(crossSectionId, out existingCrossSection);
            Debug.Assert(existingCrossSection != null, nameof(existingCrossSection) + " != null");

            if (Area.Text == string.Empty)
            {
                _ = MessageBox.Show("at least the cross section area must be defined", "new Cross Section");
                return;
            }
            existingCrossSection.CrossSectionValues[0] = double.Parse(Area.Text);

            if (Ixx.Text == string.Empty) { }
            else existingCrossSection.CrossSectionValues[1] = double.Parse(Ixx.Text);
        }
        // new cross section
        else
        {
            if (Area.Text != string.Empty)
            {
                double ixx = 0;
                var area = double.Parse(Area.Text);
                if (Ixx.Text != string.Empty) ixx = double.Parse(Ixx.Text);
                crossSection = new CrossSection(area, ixx)
                {
                    CrossSectionId = crossSectionId
                };
                model.CrossSection.Add(crossSectionId, crossSection);
            }
        }
        querschnittKeys?.Close();
        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        querschnittKeys?.Close();
        Close();
    }

    private void CrossSectionIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!model.CrossSection.ContainsKey(CrossSectionId.Text))
        {
            Area.Text = "";
            Ixx.Text = "";
            return;
        }

        // vorhandene Querschnittdefinition
        model.CrossSection.TryGetValue(CrossSectionId.Text, out existingCrossSection);
        Debug.Assert(existingCrossSection != null, nameof(existingCrossSection) + " != null"); CrossSectionId.Text = "";

        CrossSectionId.Text = existingCrossSection.CrossSectionId;

        Area.Text = existingCrossSection.CrossSectionValues[0].ToString("G3", CultureInfo.CurrentCulture);
        if (Ixx.Text == "") Ixx.Text = existingCrossSection.CrossSectionValues[1].ToString("G3", CultureInfo.CurrentCulture);
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (existingCrossSection != null) model.CrossSection.Remove(existingCrossSection.CrossSectionId);
        querschnittKeys.Close();
        Close();
    }
}