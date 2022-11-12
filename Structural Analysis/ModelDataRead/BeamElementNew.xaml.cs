using FE_Analysis.Structural_Analysis.Model_Data;
using FEALibrary.Model;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace FE_Analysis.Structural_Analysis.ModelDataRead;

public partial class BeamElementNew
{
    private readonly FeModel model;
    private readonly ElementKeys elementKeys;

    public BeamElementNew(FeModel model)
    {
        InitializeComponent();
        this.model = model;
        elementKeys = new ElementKeys(model);
        elementKeys.Show();
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var elementId = ElementId.Text;
        if (elementId == "")
        {
            _ = MessageBox.Show("Element Id must be defined", "new beam element");
            return;
        }

        // existing Element
        if (model.Elements.Keys.Contains(ElementId.Text))
        {
            model.Elements.TryGetValue(elementId, out var existingElement);
            Debug.Assert(existingElement != null, nameof(existingElement) + " != null");
            existingElement.Type = 0;
            if (Hinge1.IsChecked != null && (bool)Hinge1.IsChecked) existingElement.Type = 1;
            if (Hinge2.IsChecked != null && (bool)Hinge2.IsChecked) existingElement.Type = 2;
            if (Hinge1.IsChecked != null && Hinge2.IsChecked != null && (bool)Hinge1.IsChecked
                && (bool)Hinge2.IsChecked) existingElement.Type = 3;
            if (StartNodeId.Text.Length > 0) existingElement.NodeIds[0] = StartNodeId.Text;
            if (EndNodeId.Text.Length > 0) existingElement.NodeIds[1] = EndNodeId.Text;
            if (MaterialId.Text.Length > 0) existingElement.ElementMaterialId = MaterialId.Text;
            if (CrossSectionId.Text.Length > 0) existingElement.ElementCrossSectionId = CrossSectionId.Text;
            if (existingElement.Type == 0)
            {
                model.Elements.Remove(elementId);
                var nodeIds = existingElement.NodeIds;
                var beam = new Beam(nodeIds, CrossSectionId.Text, MaterialId.Text, model)
                {
                    ElementId = ElementId.Text
                };
                model.Elements.Add(ElementId.Text, beam);
            }
            else if (existingElement.Type is 1 | existingElement.Type is 2)
            {
                {
                    model.Elements.Remove(elementId);
                    var nodeIds = existingElement.NodeIds;
                    var beamHinged = new BeamHinged(nodeIds, MaterialId.Text, CrossSectionId.Text,
                        model, existingElement.Type)
                    {
                        ElementId = ElementId.Text
                    };
                    model.Elements.Add(ElementId.Text, beamHinged);
                }
            }
            else if (existingElement.Type is 3)
            {
                {
                    model.Elements.Remove(elementId);
                    var nodeIds = existingElement.NodeIds;
                    var truss = new Truss(nodeIds, CrossSectionId.Text, MaterialId.Text, model)
                    {
                        ElementId = ElementId.Text
                    };
                    model.Elements.Add(ElementId.Text, truss);
                }
            }
        }
        // new Element
        else
        {
            if (ElementId.Text == "" | StartNodeId.Text == "" | EndNodeId.Text == ""
                | MaterialId.Text == "" | CrossSectionId.Text == "")
            {
                _ = MessageBox.Show("input values must be completely defined", "new beam element");
                return;
            }

            var type = 0;
            if (Hinge1.IsChecked != null && (bool)Hinge1.IsChecked) type = 1;
            if (Hinge2.IsChecked != null && (bool)Hinge2.IsChecked) type = 2;
            if (Hinge1.IsChecked != null && Hinge2.IsChecked != null
              && (bool)Hinge1.IsChecked && (bool)Hinge2.IsChecked) type = 3;

            var nodeIds = new string[2];
            nodeIds[0] = StartNodeId.Text;
            nodeIds[1] = EndNodeId.Text;

            if (type == 0)
            {
                var beam = new Beam(nodeIds, CrossSectionId.Text, MaterialId.Text, model)
                {
                    ElementId = ElementId.Text
                };
                model.Elements.Add(ElementId.Text, beam);
            }
            else if (type == 1 | type == 2)
            {
                var beamHinged = new BeamHinged(nodeIds, MaterialId.Text, CrossSectionId.Text, model, type)
                {
                    ElementId = ElementId.Text
                };
                model.Elements.Add(ElementId.Text, beamHinged);
            }
            else if (type == 3)
            {
                var truss = new Truss(nodeIds, CrossSectionId.Text, MaterialId.Text, model)
                {
                    ElementId = ElementId.Text
                };
                model.Elements.Add(ElementId.Text, truss);
            }
        }
        elementKeys.Close();
        MainWindow.structuralModel.Close();
        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        elementKeys.Close();
        Close();
    }
    private void ElementIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!model.Elements.ContainsKey(ElementId.Text)) return;
        model.Elements.TryGetValue(ElementId.Text, out var existingElement);
        Debug.Assert(existingElement != null, nameof(existingElement) + " != null"); ElementId.Text = "";
        if (existingElement is Truss) { Hinge1.IsChecked = true; Hinge2.IsChecked = true; }
        else if (existingElement is BeamHinged)
        {
            if (existingElement.Type == 1) Hinge1.IsChecked = true;
            if (existingElement.Type == 2) Hinge2.IsChecked = true;
        }

        ElementId.Text = existingElement.ElementId;
        StartNodeId.Text = existingElement.NodeIds[0];
        EndNodeId.Text = existingElement.NodeIds[1];
        MaterialId.Text = existingElement.ElementMaterialId;
        CrossSectionId.Text = existingElement.ElementCrossSectionId;
    }
}