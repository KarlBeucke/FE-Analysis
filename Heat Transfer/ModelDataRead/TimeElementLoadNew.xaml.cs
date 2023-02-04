using FE_Analysis.Heat_Transfer.Model_Data;
using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Markup;

namespace FE_Analysis.Heat_Transfer.ModelDataRead;

public partial class TimeElementLoadNew
{
    private readonly FeModel model;
    private AbstractTimeDependentElementLoad existingLoad;
    private readonly HeatloadKeys loadKeys;

    public TimeElementLoadNew(FeModel model)
    {
        Language = XmlLanguage.GetLanguage("us-US");
        this.model = model;
        InitializeComponent();
        loadKeys = new HeatloadKeys(model);
        loadKeys.Show();
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var elementloadId = LoadId.Text;
        if (elementloadId == "")
        {
            _ = MessageBox.Show("ElementLoad Id must be defined", "new time dependent ElementLoad");
            return;
        }

        // existing time dependent ElementLoad
        if (model.TimeDependentElementLoads.Keys.Contains(elementloadId))
        {
            model.TimeDependentElementLoads.TryGetValue(elementloadId, out existingLoad);
            Debug.Assert(existingLoad != null, nameof(existingLoad) + " != null");

            if (ElementId.Text.Length > 0) { existingLoad.ElementId = ElementId.Text; }
            if (P0.Text.Length > 0) existingLoad.P[0] = double.Parse(P0.Text);
            if (P1.Text.Length > 0) existingLoad.P[1] = double.Parse(P1.Text);
            if (P2.Text.Length > 0) existingLoad.P[2] = double.Parse(P2.Text);
            if (P3.Text.Length > 0) existingLoad.P[3] = double.Parse(P3.Text);
        }
        // new time dependent ElementLoad
        else
        {
            var elementId = "";
            var p = new double[4];
            if (ElementId.Text.Length > 0) elementId = ElementId.Text;
            if (P0.Text.Length > 0) p[0] = double.Parse(P0.Text);
            if (P1.Text.Length > 0) p[1] = double.Parse(P1.Text);
            if (P2.Text.Length > 0) p[2] = double.Parse(P2.Text);
            if (P3.Text.Length > 0) p[3] = double.Parse(P3.Text);
            var timedependentElementload = new TimeDependentElementLoad(elementId, p)
            {
                LoadId = elementloadId
            };
            model.TimeDependentElementLoads.Add(elementloadId, timedependentElementload);
        }
        loadKeys?.Close();
        Close();
        MainWindow.heatModelVisual.Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        loadKeys?.Close();
        Close();
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (!model.TimeDependentElementLoads.Keys.Contains(LoadId.Text)) return;
        model.TimeDependentElementLoads.Remove(LoadId.Text);
        loadKeys?.Close();
        Close();
        MainWindow.heatModelVisual.Close();
    }

    private void LoadIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!model.TimeDependentElementLoads.ContainsKey(LoadId.Text))
        {
            ElementId.Text = "";
            P0.Text = "";
            P1.Text = "";
            P2.Text = "";
            P3.Text = "";
            return;
        }

        // existing time dependent ElementLoad definition
        model.TimeDependentElementLoads.TryGetValue(LoadId.Text, out existingLoad);
        Debug.Assert(existingLoad != null, nameof(existingLoad) + " != null");

        LoadId.Text = existingLoad.LoadId;

        ElementId.Text = existingLoad.ElementId;
        P0.Text = existingLoad.Loadvalues[0].ToString("G2");
        P1.Text = existingLoad.Loadvalues[1].ToString("G2");
        P2.Text = existingLoad.Loadvalues[2].ToString("G2");
        P3.Text = existingLoad.Loadvalues[3].ToString("G2");
    }
}