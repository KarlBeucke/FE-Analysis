using FEALibrary.Model;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace FE_Analysis.Heat_Transfer.ModelDataRead;

public partial class NodeNew
{
    private readonly FeModel model;

    public NodeNew()
    {
        InitializeComponent();
    }
    public NodeNew(FeModel feModel)
    {
        InitializeComponent();
        model = feModel;
        // activate evnet handler for Canvas
        MainWindow.heatModel.VisualHeatModel.Background = System.Windows.Media.Brushes.Transparent;
        Show();
    }
    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var nodeId = NodeId.Text;

        if (nodeId == "")
        {
            _ = MessageBox.Show("Node Id must be defined", "new Node");
            return;
        }

        if (model.Nodes.ContainsKey(nodeId))
        {
            model.Nodes.TryGetValue(nodeId, out var existingNode);
            Debug.Assert(existingNode != null, nameof(existingNode) + " != null");
            if (X.Text.Length > 0) existingNode.Coordinates[0] = double.Parse(X.Text);
            if (Y.Text.Length > 0) existingNode.Coordinates[1] = double.Parse(Y.Text);
        }
        else
        {
            var dimension = model.SpatialDimension;
            var coordinates = new double[dimension];
            int anzahlKnotenDof = 1;
            if (X.Text.Length > 0) coordinates[0] = double.Parse(X.Text);
            if (Y.Text.Length > 0) coordinates[1] = double.Parse(Y.Text);
            var newNode = new Node(NodeId.Text, coordinates, anzahlKnotenDof, dimension);
            model.Nodes.Add(nodeId, newNode);
        }

        // remove pilot Node and deactivate event handler for Canvas
        MainWindow.heatModel.VisualHeatModel.Children.Remove(MainWindow.heatModel.Node);
        MainWindow.heatModel.VisualHeatModel.Background = null;
        MainWindow.heatModel.isNode = false;
        MainWindow.heatModel.Close();
        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        // remove pilot node and deactivate event handler for Canvas
        MainWindow.heatModel.VisualHeatModel.Children.Remove(MainWindow.heatModel.Node);
        MainWindow.heatModel.VisualHeatModel.Background = null;
        MainWindow.heatModel.isNode = false;
        Close();
    }

    private void NodeIdLostFocus(object sender, RoutedEventArgs e)
    {
        // remove pilot Node and deactivate event handler for Canvas
        if (!model.Nodes.ContainsKey(NodeId.Text)) return;
        model.Nodes.TryGetValue(NodeId.Text, out var existingNode);
        Debug.Assert(existingNode != null, nameof(existingNode) + " != null");
        X.Text = existingNode.Coordinates[0].ToString("N2", CultureInfo.CurrentCulture);
        Y.Text = existingNode.Coordinates[1].ToString("N2", CultureInfo.CurrentCulture);
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (!model.Nodes.Keys.Contains(NodeId.Text)) return;
        model.Nodes.Remove(NodeId.Text);
        Close();
        MainWindow.heatModel.Close();
    }
}