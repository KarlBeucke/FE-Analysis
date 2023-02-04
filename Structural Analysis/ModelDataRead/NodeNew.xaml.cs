using FEALibrary.Model;
using System.Diagnostics;
using System.Globalization;
using System.Windows;

namespace FE_Analysis.Structural_Analysis.ModelDataRead;

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
        // activate event handler for Canvas
        MainWindow.structuresModelVisual.VisualStructuralModel.Background = System.Windows.Media.Brushes.Transparent;
        Show();
        var nodeKeys = new NodeKeys(model) { Owner = this };
        nodeKeys.Show();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        // remove pilot node and deactivate event handler for Canvas
        MainWindow.structuresModelVisual.VisualStructuralModel.Children.Remove(MainWindow.structuresModelVisual.Node);
        MainWindow.structuresModelVisual.VisualStructuralModel.Background = null;
        MainWindow.structuresModelVisual.isNode = false;
        Close();
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
            if (NumberDof.Text.Length > 0) existingNode.NumberOfNodalDof = int.Parse(NumberDof.Text);
            if (X.Text.Length > 0) existingNode.Coordinates[0] = double.Parse(X.Text);
            if (Y.Text.Length > 0) existingNode.Coordinates[1] = double.Parse(Y.Text);
        }
        else
        {
            var dimension = model.SpatialDimension;
            var coordinates = new double[dimension];
            int numberNodalDof = 3;
            if (NumberDof.Text.Length > 0) numberNodalDof = int.Parse(NumberDof.Text);
            if (X.Text.Length > 0) coordinates[0] = double.Parse(X.Text);
            if (Y.Text.Length > 0) coordinates[1] = double.Parse(Y.Text);
            var newNode = new Node(NodeId.Text, coordinates, numberNodalDof, dimension);
            model.Nodes.Add(nodeId, newNode);
        }

        // remove pilot node and deactivate event handler for Canvas
        MainWindow.structuresModelVisual.VisualStructuralModel.Children.Remove(MainWindow.structuresModelVisual.Node);
        MainWindow.structuresModelVisual.VisualStructuralModel.Background = null;
        MainWindow.structuresModelVisual.isNode = false;
        MainWindow.structuresModelVisual.Close();
        Close();
    }

    private void NodeIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!model.Nodes.ContainsKey(NodeId.Text)) return;
        model.Nodes.TryGetValue(NodeId.Text, out var existingNode);
        Debug.Assert(existingNode != null, nameof(existingNode) + " != null");
        NumberDof.Text = existingNode.NumberOfNodalDof.ToString();
        X.Text = existingNode.Coordinates[0].ToString("N2", CultureInfo.CurrentCulture);
        Y.Text = existingNode.Coordinates[1].ToString("N2", CultureInfo.CurrentCulture);
    }
}