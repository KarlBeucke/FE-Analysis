using FE_Analysis.Structural_Analysis.Model_Data;
using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace FE_Analysis.Structural_Analysis.ModelDataRead;

public partial class NodeLoadNew
{
    private readonly FeModel model;
    private AbstractLoad existingNodeload;
    private readonly NodeLoadKeys nodeLoadKeys;
    public NodeLoadNew()
    {
        InitializeComponent();
        Show();
    }
    public NodeLoadNew(FeModel model)
    {
        InitializeComponent();
        this.model = model;
        nodeLoadKeys = new NodeLoadKeys(model);
        nodeLoadKeys.Show();
        Show();
    }
    public NodeLoadNew(FeModel model, string last, string knoten,
        double px, double py, double m)
    {
        InitializeComponent();
        this.model = model;
        LoadId.Text = last;
        NodeId.Text = knoten;
        Px.Text = px.ToString("0.00");
        Py.Text = py.ToString("0.00");
        M.Text = m.ToString("0.00");
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var nodeloadId = LoadId.Text;
        if (nodeloadId == "")
        {
            _ = MessageBox.Show("NodeLoad Id must be defined", "new NodeLoad");
            return;
        }

        // existing NodeLoad
        if (model.Loads.Keys.Contains(LoadId.Text))
        {
            model.Loads.TryGetValue(nodeloadId, out existingNodeload);
            Debug.Assert(existingNodeload != null, nameof(existingNodeload) + " != null");

            if (NodeId.Text.Length > 0) existingNodeload.NodeId = NodeId.Text.ToString(CultureInfo.CurrentCulture);
            if (Px.Text.Length > 0) existingNodeload.Loadvalues[0] = double.Parse(Px.Text);
            if (Py.Text.Length > 0) existingNodeload.Loadvalues[1] = double.Parse(Py.Text);
            if (M.Text.Length > 0) existingNodeload.Loadvalues[2] = double.Parse(M.Text);
        }
        // new NodeLoad
        else
        {
            string nodeId = "";
            double px = 0, py = 0, m = 0;
            if (NodeId.Text.Length > 0) nodeId = NodeId.Text.ToString(CultureInfo.CurrentCulture);
            if (Px.Text.Length > 0) px = double.Parse(Px.Text);
            if (Py.Text.Length > 0) py = double.Parse(Py.Text);
            if (M.Text.Length > 0) m = double.Parse(M.Text);
            var nodeLoad = new NodeLoad(nodeId, px, py, m)
            {
                LoadId = nodeloadId
            };
            model.Loads.Add(nodeloadId, nodeLoad);
        }
        nodeLoadKeys?.Close();
        Close();
        MainWindow.structuralModel.Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        nodeLoadKeys?.Close();
        Close();
    }

    private void LoadIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!model.Loads.ContainsKey(LoadId.Text))
        {
            NodeId.Text = "";
            Px.Text = "";
            Py.Text = "";
            M.Text = "";
            return;
        }

        // existing NodeLoaddefinition
        model.Loads.TryGetValue(LoadId.Text, out existingNodeload);
        Debug.Assert(existingNodeload != null, nameof(existingNodeload) + " != null"); LoadId.Text = "";

        LoadId.Text = existingNodeload.LoadId;

        NodeId.Text = existingNodeload.NodeId;
        Px.Text = existingNodeload.Loadvalues[0].ToString("G3", CultureInfo.CurrentCulture);
        Py.Text = existingNodeload.Loadvalues[1].ToString("G3", CultureInfo.CurrentCulture);
        M.Text = existingNodeload.Loadvalues[2].ToString("G3", CultureInfo.CurrentCulture);
    }
    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (!model.Loads.Keys.Contains(LoadId.Text)) return;
        model.Loads.Remove(LoadId.Text);
        nodeLoadKeys?.Close();
        Close();
        MainWindow.structuralModel.Close();
    }
}