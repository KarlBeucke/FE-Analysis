using FE_Analysis.Structural_Analysis.Model_Data;
using FEALibrary.Model;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace FE_Analysis.Structural_Analysis.ModelDataRead;

public partial class PointLoadNew
{
    private readonly FeModel model;
    private readonly PointLoadKeys pointLoadKeys;

    public PointLoadNew(FeModel model)
    {
        InitializeComponent();
        this.model = model;
        pointLoadKeys = new PointLoadKeys(model);
        pointLoadKeys.Show();
        Show();
    }

    public PointLoadNew(FeModel model, string load, string element, double px, double py, double offset)
    {
        InitializeComponent();
        this.model = model;
        LoadId.Text = load;
        ElementId.Text = element;
        Px.Text = px.ToString("0.00");
        Py.Text = py.ToString("0.00");
        Offset.Text = offset.ToString("0.00");
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var pointLoadId = LoadId.Text;
        if (pointLoadId == "")
        {
            _ = MessageBox.Show("PointLoad Id must be defined", "new PointLoad");
            return;
        }

        // existing PointLoad
        if (model.PointLoads.Keys.Contains(LoadId.Text))
        {
            model.PointLoads.TryGetValue(pointLoadId, out var load);
            Debug.Assert(load != null, nameof(load) + " != null");

            var pointLoad = (PointLoad)load;
            if (ElementId.Text.Length > 0) pointLoad.ElementId = ElementId.Text.ToString(CultureInfo.CurrentCulture);
            if (Px.Text.Length > 0) pointLoad.Loadvalues[0] = double.Parse(Px.Text);
            if (Py.Text.Length > 0) pointLoad.Loadvalues[1] = double.Parse(Py.Text);
            if (Offset.Text.Length > 0) pointLoad.Offset = double.Parse(Offset.Text);
        }
        // new pointLoad
        else
        {
            var elementId = "";
            double px = 0, py = 0, offset = 0;
            if (ElementId.Text.Length > 0) elementId = ElementId.Text.ToString(CultureInfo.CurrentCulture);
            if (Px.Text.Length > 0) px = double.Parse(Px.Text);
            if (Py.Text.Length > 0) py = double.Parse(Py.Text);
            if (Offset.Text.Length > 0) offset = double.Parse(Offset.Text);
            var pointLoad = new PointLoad(elementId, px, py, offset)
            {
                LoadId = pointLoadId
            };
            model.PointLoads.Add(pointLoadId, pointLoad);
        }
        pointLoadKeys?.Close();
        Close();
        MainWindow.structuralModel.Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        pointLoadKeys?.Close();
        Close();
    }

    private void PointLoadIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!model.PointLoads.ContainsKey(LoadId.Text))
        {
            ElementId.Text = "";
            Px.Text = "";
            Py.Text = "";
            Offset.Text = "";
            return;
        }

        // existing PointLoad definitions
        model.PointLoads.TryGetValue(LoadId.Text, out var load);
        Debug.Assert(load != null, nameof(load) + " != null");

        var pointLoad = (PointLoad)load;
        LoadId.Text = pointLoad.LoadId;

        ElementId.Text = pointLoad.ElementId;
        Px.Text = pointLoad.Loadvalues[0].ToString("G3", CultureInfo.CurrentCulture);
        Py.Text = pointLoad.Loadvalues[1].ToString("G3", CultureInfo.CurrentCulture);
        Offset.Text = pointLoad.Offset.ToString("G3", CultureInfo.CurrentCulture);
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (!model.PointLoads.Keys.Contains(LoadId.Text)) return;
        model.PointLoads.Remove(LoadId.Text);
        pointLoadKeys?.Close();
        Close();
        MainWindow.structuralModel.Close();
        pointLoadKeys?.Close();
    }
}