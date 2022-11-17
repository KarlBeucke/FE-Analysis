using FE_Analysis.Structural_Analysis.Model_Data;
using FEALibrary.Model;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace FE_Analysis.Structural_Analysis.ModelDataRead;

public partial class ElementNew
{
    private readonly FeModel model;
    private readonly ElementKeys elementKeys;

    public ElementNew(FeModel model)
    {
        InitializeComponent();
        this.model = model;
        ElementId.Text = string.Empty;
        StartNodeId.Text = string.Empty;
        EndNodeId.Text = string.Empty;
        MaterialId.Text = string.Empty;
        CrossSectionId.Text = string.Empty;
        Show();
        elementKeys = new ElementKeys(model) { Owner = this };
        elementKeys.Show();
    }
    private void TrussChecked(object sender, RoutedEventArgs e)
    {
        Hinge1.IsChecked = true; Hinge2.IsChecked = true;
        BeamCheck.IsChecked = false;
        SpringCheck.IsChecked = false;
    }
    private void BeamChecked(object sender, RoutedEventArgs e)
    {
        Hinge1.IsChecked = false; Hinge2.IsChecked = false;
        TrussCheck.IsChecked = false;
        SpringCheck.IsChecked = false;
    }
    private void SpringChecked(object sender, RoutedEventArgs e)
    {
        Hinge1.IsChecked = false; Hinge2.IsChecked = false;
        TrussCheck.IsChecked = false;
        BeamCheck.IsChecked = false;
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        if (ElementId.Text == "")
        {
            _ = MessageBox.Show("Element Id must be defined", "new Element");
            return;
        }

        // existing element will be completely removed as element definition
        // (Truss, Beam, BeamHinged) may be changed
        // new element definition will be generated and stored under existing key
        if (model.Elements.ContainsKey(ElementId.Text))
        {
            model.Elements.Remove(ElementId.Text);
        }
        var nodeIds = new string[2];
        nodeIds[0] = StartNodeId.Text;
        if (EndNodeId.Text.Length != 0) nodeIds[1] = EndNodeId.Text;

        if (TrussCheck.IsChecked != null && (bool)TrussCheck.IsChecked)
        {
            var element = new Truss(nodeIds, CrossSectionId.Text, MaterialId.Text, model)
            {
                ElementId = ElementId.Text
            };
            model.Elements.Add(ElementId.Text, element);
        }
        else if (BeamCheck.IsChecked != null && (bool)BeamCheck.IsChecked)
        {
            if ((Hinge1.IsChecked != null && !(bool)Hinge1.IsChecked) &
               (Hinge2.IsChecked != null && !(bool)Hinge2.IsChecked))
            {
                var element = new Beam(nodeIds, CrossSectionId.Text, MaterialId.Text, model)
                {
                    ElementId = ElementId.Text
                };
                model.Elements.Add(ElementId.Text, element);
            }
            if (Hinge1.IsChecked != null && (bool)Hinge1.IsChecked)
            {
                var element = new BeamHinged(nodeIds, CrossSectionId.Text, MaterialId.Text, model, 1)
                {
                    ElementId = ElementId.Text
                };
                model.Elements.Add(ElementId.Text, element);
            }
            else if (Hinge2.IsChecked != null && (bool)Hinge2.IsChecked)
            {
                var element = new BeamHinged(nodeIds, CrossSectionId.Text, MaterialId.Text, model, 2)
                {
                    ElementId = ElementId.Text
                };
                model.Elements.Add(ElementId.Text, element);
            }
        }
        else if (SpringCheck.IsChecked != null && (bool)SpringCheck.IsChecked)
        {
            var element = new SpringElement(nodeIds, MaterialId.Text, model)
            {
                ElementId = ElementId.Text
            };
            model.Elements.Add(ElementId.Text, element);
        }

        Close();
        MainWindow.structuralModel.Close();
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
        MainWindow.structuralModel.Close();
        elementKeys?.Close();
    }

    private void ElementIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!model.Elements.ContainsKey(ElementId.Text))
        {
            StartNodeId.Text = "";
            EndNodeId.Text = "";
            MaterialId.Text = "";
            CrossSectionId.Text = "";
            return;
        }

        // existing element definitions
        model.Elements.TryGetValue(ElementId.Text, out var existingElement);
        Debug.Assert(existingElement != null, nameof(existingElement) + " != null"); ElementId.Text = "";

        ElementId.Text = existingElement.ElementId;
        switch (existingElement)
        {
            case Truss:
                TrussCheck.IsChecked = true;
                Hinge1.IsChecked = true;
                Hinge2.IsChecked = true;
                BeamCheck.IsChecked = false;
                EndNodeId.Text = existingElement.NodeIds[1];
                break;
            case Beam:
                TrussCheck.IsChecked = false;
                BeamCheck.IsChecked = true;
                EndNodeId.Text = existingElement.NodeIds[1];
                break;
            case BeamHinged:
                {
                    BeamCheck.IsChecked = true;
                    if (existingElement.Type == 1) Hinge1.IsChecked = true;
                    if (existingElement.Type == 2) Hinge2.IsChecked = true;
                    TrussCheck.IsChecked = false;
                    EndNodeId.Text = existingElement.NodeIds[1];
                    break;
                }
            case SpringElement:
                SpringCheck.IsChecked = true;
                break;
        }

        StartNodeId.Text = existingElement.NodeIds[0];
        MaterialId.Text = existingElement.ElementMaterialId;
        CrossSectionId.Text = existingElement.ElementCrossSectionId;
    }
}