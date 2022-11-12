using FE_Analysis.Structural_Analysis.Model_Data;
using FEALibrary.Model;
using System.Diagnostics;
using System.Windows;

namespace FE_Analysis.Structural_Analysis.ModelDataRead;

public partial class SpringElementNew
{
    private readonly FeModel model;

    public SpringElementNew(FeModel model)
    {
        this.model = model;
        InitializeComponent();
        Show();
        //var elementKeys = new ElementKeys(modell) { Owner = this };
        //elementKeys.Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var elementId = ElementId.Text;
        if (elementId == "")
        {
            _ = MessageBox.Show("Element Id muss definiert sein", "neues Federelement");
            return;
        }

        if (model.Elements.ContainsKey(elementId))
        {
            model.Elements.TryGetValue(elementId, out var existingElement);
            Debug.Assert(existingElement != null, nameof(existingElement) + " != null");
            if (NodeId.Text.Length > 0) existingElement.NodeIds[0] = NodeId.Text;
            if (MaterialId.Text.Length > 0) existingElement.ElementMaterialId = MaterialId.Text;
        }
        else
        {
            var nodeIds = new string[2];
            nodeIds[0] = NodeId.Text;
            var materialId = "";
            if (MaterialId.Text.Length > 0) materialId = MaterialId.Text;
            var springSupport = new SpringElement(nodeIds, materialId, model)
            {
                ElementId = ElementId.Text
            };
            model.Elements.Add(ElementId.Text, springSupport);
        }
        MainWindow.structuralModel.Close();
        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void NodeIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!model.Elements.ContainsKey(ElementId.Text))
        {
            NodeId.Text = "";
            MaterialId.Text = "";
            return;
        }
        model.Elements.TryGetValue(ElementId.Text, out var existingElement);
        Debug.Assert(existingElement != null, nameof(existingElement) + " != null");
        NodeId.Text = existingElement.NodeIds[0];
        MaterialId.Text = existingElement.ElementMaterialId;
    }
}