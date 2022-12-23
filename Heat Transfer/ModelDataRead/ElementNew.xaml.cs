using FE_Analysis.Heat_Transfer.Model_Data;
using FEALibrary.Model;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace FE_Analysis.Heat_Transfer.ModelDataRead;

public partial class ElementNew
{
    private readonly FeModel model;
    private readonly ElementKeys elementKeys;

    public ElementNew(FeModel model)
    {
        InitializeComponent();
        this.model = model;
        Show();
        elementKeys = new ElementKeys(model) { Owner = this };
        elementKeys.Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        if (ElementId.Text == "")
        {
            _ = MessageBox.Show("Element Id must be defined", "new Element");
            return;
        }

        // existing element will be completely removed, as element definitions
        // (Element2D2, Element2D3, Element2D4, Element3D8) may be changed
        // new element will be instantiated and stored under existing element key
        if (model.Elements.ContainsKey(ElementId.Text))
        {
            model.Elements.Remove(ElementId.Text);
        }
        var nodeIds = new string[2];
        nodeIds[0] = Node1Id.Text;
        if (Node2Id.Text.Length != 0) nodeIds[1] = Node2Id.Text;

        if (Element2D2Check.IsChecked != null && (bool)Element2D2Check.IsChecked)
        {
            var element = new Element2D2(nodeIds, MaterialId.Text, model)
            {
                ElementId = ElementId.Text
            };
            model.Elements.Add(ElementId.Text, element);
        }
        else if (Element2D3Check.IsChecked != null && (bool)Element2D3Check.IsChecked)
        {
            if (Node3Id.Text.Length != 0) nodeIds[2] = Node3Id.Text;
            var element = new Element2D3(nodeIds, MaterialId.Text, model)
            {
                ElementId = ElementId.Text
            };
            model.Elements.Add(ElementId.Text, element);
        }
        else if (Element2D4Check.IsChecked != null && (bool)Element2D4Check.IsChecked)
        {
            if (Node3Id.Text.Length != 0) nodeIds[2] = Node3Id.Text;
            if (Node4Id.Text.Length != 0) nodeIds[3] = Node4Id.Text;
            var element = new Element2D4(ElementId.Text, nodeIds, MaterialId.Text, model);
            model.Elements.Add(ElementId.Text, element);
        }
        else if (Element3D8Check.IsChecked != null && (bool)Element3D8Check.IsChecked)
        {
            if (Node3Id.Text.Length != 0) nodeIds[2] = Node3Id.Text;
            if (Node4Id.Text.Length != 0) nodeIds[3] = Node4Id.Text;
            if (Node5Id.Text.Length != 0) nodeIds[4] = Node5Id.Text;
            if (Node6Id.Text.Length != 0) nodeIds[5] = Node6Id.Text;
            if (Node7Id.Text.Length != 0) nodeIds[6] = Node7Id.Text;
            if (Node8Id.Text.Length != 0) nodeIds[7] = Node8Id.Text;
            var element = new Element3D8(ElementId.Text, nodeIds, MaterialId.Text, model);
            model.Elements.Add(ElementId.Text, element);
        }
        Close();
        MainWindow.heatModel.Close();
        elementKeys?.Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        elementKeys?.Close();
        Close();
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (!model.Elements.Keys.Contains(ElementId.Text)) return;
        model.Elements.Remove(ElementId.Text);
        elementKeys?.Close();
        Close();
        MainWindow.heatModel.Close();
    }

    private void ElementIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!model.Elements.ContainsKey(ElementId.Text))
        {
            Node1Id.Text = "";
            Node2Id.Text = "";
            Node3Id.Text = "";
            Node4Id.Text = "";
            Node5Id.Text = "";
            Node6Id.Text = "";
            Node7Id.Text = "";
            Node8Id.Text = "";
            MaterialId.Text = "";
            return;
        }

        // existing element definitionse
        model.Elements.TryGetValue(ElementId.Text, out var existingElement);
        Debug.Assert(existingElement != null, nameof(existingElement) + " != null"); ElementId.Text = "";

        ElementId.Text = existingElement.ElementId;
        Node1Id.Text = existingElement.NodeIds[0];
        switch (existingElement)
        {
            case Element2D2:
                Element2D2Check.IsChecked = true;
                Element2D3Check.IsChecked = false; Element2D4Check.IsChecked = false; Element3D8Check.IsChecked = false;
                Node2Id.Text = existingElement.NodeIds[1];
                break;
            case Element2D3:
                Element2D3Check.IsChecked = true;
                Element2D2Check.IsChecked = false; Element2D4Check.IsChecked = false; Element3D8Check.IsChecked = false;
                Node2Id.Text = existingElement.NodeIds[1];
                Node3Id.Text = existingElement.NodeIds[2];
                break;
            case Element2D4:
                Element2D4Check.IsChecked = true;
                Element2D2Check.IsChecked = false; Element2D3Check.IsChecked = false; Element3D8Check.IsChecked = false;
                Node2Id.Text = existingElement.NodeIds[1];
                Node3Id.Text = existingElement.NodeIds[2];
                Node4Id.Text = existingElement.NodeIds[3];
                break;
            case Element3D8:
                Element3D8Check.IsChecked = true;
                Element2D2Check.IsChecked = false; Element2D3Check.IsChecked = false; Element2D4Check.IsChecked = false;
                Node2Id.Text = existingElement.NodeIds[1];
                Node3Id.Text = existingElement.NodeIds[2];
                Node4Id.Text = existingElement.NodeIds[3];
                Node5Id.Text = existingElement.NodeIds[4];
                Node6Id.Text = existingElement.NodeIds[5];
                Node7Id.Text = existingElement.NodeIds[6];
                Node8Id.Text = existingElement.NodeIds[7];
                break;
        }
        MaterialId.Text = existingElement.ElementMaterialId;
    }
}