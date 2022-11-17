using FE_Analysis.Structural_Analysis.Model_Data;
using FEALibrary.Model;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace FE_Analysis.Structural_Analysis.ModelDataRead;

public partial class LineLoadNew
{
    private readonly FeModel model;
    private readonly LineLoadKeys lineLoadKeys;

    public LineLoadNew(FeModel model)
    {
        InitializeComponent();
        this.model = model;
        lineLoadKeys = new LineLoadKeys(model);
        lineLoadKeys.Show();
        Show();
    }

    public LineLoadNew(FeModel model, string load, string element,
        double pxa, double pya, double pxb, double pyb, bool inElement)
    {
        InitializeComponent();
        this.model = model;
        LoadId.Text = load;
        ElementId.Text = element;
        Pxa.Text = pxa.ToString("0.00");
        Pxa.Text = pxa.ToString("0.00");
        Pya.Text = pya.ToString("0.00");
        Pxb.Text = pxb.ToString("0.00");
        Pyb.Text = pyb.ToString("0.00");
        InElement.IsChecked = inElement; Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var lineLoadId = LoadId.Text;
        if (lineLoadId == "")
        {
            _ = MessageBox.Show("LineLoad Id must be defined", "new LineLoad");
            return;
        }

        // existing LineLoad
        if (model.ElementLoads.Keys.Contains(LoadId.Text))
        {
            model.ElementLoads.TryGetValue(lineLoadId, out var existingLineLoad);
            Debug.Assert(existingLineLoad != null, nameof(existingLineLoad) + " != null");

            if (ElementId.Text.Length > 0)
                existingLineLoad.ElementId = ElementId.Text.ToString(CultureInfo.CurrentCulture);
            if (Pxa.Text.Length > 0) existingLineLoad.Loadvalues[0] = double.Parse(Pxa.Text);
            if (Pya.Text.Length > 0) existingLineLoad.Loadvalues[1] = double.Parse(Pya.Text);
            if (Pxb.Text.Length > 0) existingLineLoad.Loadvalues[2] = double.Parse(Pxb.Text);
            if (Pyb.Text.Length > 0) existingLineLoad.Loadvalues[3] = double.Parse(Pyb.Text);
            existingLineLoad.InElementCoordinateSystem = InElement.IsChecked != null && (bool)InElement.IsChecked;
        }
        // neue Linienlast
        else
        {
            var inElement = false;
            var elementId = "";
            double pxa = 0, pxb = 0, pya = 0, pyb = 0;
            if (ElementId.Text.Length > 0) elementId = ElementId.Text.ToString(CultureInfo.CurrentCulture);
            if (Pxa.Text.Length > 0) pxa = double.Parse(Pxa.Text);
            if (Pya.Text.Length > 0) pya = double.Parse(Pya.Text);
            if (Pxb.Text.Length > 0) pxb = double.Parse(Pxb.Text);
            if (Pyb.Text.Length > 0) pyb = double.Parse(Pyb.Text);
            if (InElement.IsChecked != null && (bool)InElement.IsChecked) inElement = true;
            var lineLoad = new LineLoad(elementId, pxa, pya, pxb, pyb, inElement)
            {
                LoadId = lineLoadId
            };
            model.ElementLoads.Add(lineLoadId, lineLoad);
        }

        lineLoadKeys?.Close();
        Close();
        MainWindow.structuralModel.Close();
    }
    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        lineLoadKeys?.Close();
        Close();
    }

    private void LoadIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!model.ElementLoads.ContainsKey(LoadId.Text))
        {
            ElementId.Text = "";
            Pxa.Text = "";
            Pya.Text = "";
            Pxb.Text = "";
            Pyb.Text = "";
            InElement.IsChecked = true;
            return;
        }

        // existing LineLoad definitions
        model.ElementLoads.TryGetValue(LoadId.Text, out var existingLineLoad);
        Debug.Assert(existingLineLoad != null, nameof(existingLineLoad) + " != null"); LoadId.Text = "";

        LoadId.Text = existingLineLoad.LoadId;

        ElementId.Text = existingLineLoad.ElementId;
        Pxa.Text = existingLineLoad.Loadvalues[0].ToString("G3", CultureInfo.CurrentCulture);
        Pya.Text = existingLineLoad.Loadvalues[1].ToString("G3", CultureInfo.CurrentCulture);
        Pxb.Text = existingLineLoad.Loadvalues[2].ToString("G3", CultureInfo.CurrentCulture);
        Pyb.Text = existingLineLoad.Loadvalues[3].ToString("G3", CultureInfo.CurrentCulture);
        existingLineLoad.InElementCoordinateSystem = InElement.IsChecked != null && (bool)InElement.IsChecked;
    }
    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (!model.ElementLoads.Keys.Contains(LoadId.Text)) return;
        model.ElementLoads.Remove(LoadId.Text);
        lineLoadKeys?.Close();
        Close();
        MainWindow.structuralModel.Close();
        lineLoadKeys?.Close();
    }
}