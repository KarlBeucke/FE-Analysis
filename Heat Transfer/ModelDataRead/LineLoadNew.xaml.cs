using FE_Analysis.Heat_Transfer.Model_Data;
using FEALibrary.Model;
using System.Linq;
using System.Windows;
using FEALibrary.Model.abstractClasses;
using System.Diagnostics;
using System.Globalization;

namespace FE_Analysis.Heat_Transfer.ModelDataRead;

public partial class LineLoadNew
{
    private readonly FeModel model;
    private AbstractLineLoad existingLoad;
    private readonly HeatloadKeys loadKeys;
    public LineLoadNew(FeModel model)
    {
        this.model = model;
        InitializeComponent();
        loadKeys = new HeatloadKeys(model);
        loadKeys.Show();
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var lineloadId = LineLoadId.Text;
        if (lineloadId == "")
        {
            _ = MessageBox.Show("Lineload Id must be defined", "new LineLoad");
            return;
        }

        // existing Lineload
        if (model.LineLoads.Keys.Contains(lineloadId))
        {
            model.LineLoads.TryGetValue(lineloadId, out existingLoad);
            Debug.Assert(existingLoad != null, nameof(existingLoad) + " != null");

            if (StartNodeId.Text.Length > 0) existingLoad.StartNodeId = StartNodeId.Text.ToString(CultureInfo.CurrentCulture);
            if (Start.Text.Length > 0) existingLoad.Loadvalues[0] = double.Parse(Start.Text);
            if (EndNodeId.Text.Length > 0) existingLoad.EndNodeId = EndNodeId.Text.ToString(CultureInfo.CurrentCulture);
            if (End.Text.Length > 0) existingLoad.Loadvalues[1] = double.Parse(End.Text);
        }
        // new Lineload
        else
        {
            var startknotenId = "";
            var endknotenId = "";
            var t = new double[2];
            if (StartNodeId.Text.Length > 0) startknotenId = StartNodeId.Text.ToString(CultureInfo.CurrentCulture);
            if (Start.Text.Length > 0) t[0] = double.Parse(Start.Text);
            if (EndNodeId.Text.Length > 0) endknotenId = EndNodeId.Text.ToString(CultureInfo.CurrentCulture);
            if (End.Text.Length > 0) t[1] = double.Parse(End.Text);
            var lineload = new LineLoad(startknotenId, endknotenId, t)
            {
                LoadId = lineloadId
            };
            model.LineLoads.Add(lineloadId, lineload);
        }
        loadKeys?.Close();
        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        loadKeys?.Close();
        Close();
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (!model.LineLoads.Keys.Contains(LineLoadId.Text)) return;
        model.LineLoads.Remove(LineLoadId.Text);
        loadKeys?.Close();
        Close();
    }

    private void LineloadIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!model.LineLoads.ContainsKey(LineLoadId.Text))
        {
            StartNodeId.Text = "";
            Start.Text = "";
            EndNodeId.Text = "";
            End.Text = "";
            return;
        }

        // existing LineLoad definition
        model.LineLoads.TryGetValue(LineLoadId.Text, out existingLoad);
        Debug.Assert(existingLoad != null, nameof(existingLoad) + " != null");

        LineLoadId.Text = existingLoad.LoadId;
        StartNodeId.Text = existingLoad.StartNodeId;
        Start.Text = existingLoad.Loadvalues[0].ToString("G3", CultureInfo.CurrentCulture);
        EndNodeId.Text = existingLoad.EndNodeId;
        End.Text = existingLoad.Loadvalues[1].ToString("G3", CultureInfo.CurrentCulture);
    }
}