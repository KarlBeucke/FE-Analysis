using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace FE_Analysis.Heat_Transfer.ModelDataRead;

public partial class BoundaryConditionNew
{
    private readonly FeModel model;
    private AbstractBoundaryCondition existingBoundaryCondition;
    private readonly BoundaryCondionsKeys boundaryConditionKeys;

    public BoundaryConditionNew(FeModel model)
    {
        this.model = model;
        InitializeComponent();
        boundaryConditionKeys = new BoundaryCondionsKeys(model);
        boundaryConditionKeys.Show();
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var boundaryConditionId = BoundaryConditionId.Text;
        var nodeId = NodeId.Text;
        double temperature = 0;
        if (boundaryConditionId == "")
        {
            _ = MessageBox.Show("Boundary condition Id must be defined", "new BoundaryCondition");
            return;
        }

        // existing boundary condition
        if (model.BoundaryConditions.Keys.Contains(boundaryConditionId))
        {
            model.BoundaryConditions.TryGetValue(boundaryConditionId, out existingBoundaryCondition);
            Debug.Assert(existingBoundaryCondition != null, nameof(existingBoundaryCondition) + " != null");

            if (Temperature.Text.Length > 0) existingBoundaryCondition.Prescribed[0] = double.Parse(Temperature.Text);
        }
        // new boundary condition
        else
        {
            if (Temperature.Text.Length > 0)
                temperature = double.Parse(Temperature.Text);

            var boundaryCondition = new Model_Data.BoundaryCondition(boundaryConditionId, nodeId, temperature)
            {
                SupportId = boundaryConditionId
            };
            model.BoundaryConditions.Add(boundaryConditionId, boundaryCondition);
        }
        boundaryConditionKeys?.Close();
        Close();
        MainWindow.heatModelVisual.Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        boundaryConditionKeys?.Close();
        Close();
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (!model.BoundaryConditions.Keys.Contains(BoundaryConditionId.Text)) return;
        model.BoundaryConditions.Remove(BoundaryConditionId.Text);
        boundaryConditionKeys?.Close();
        Close();
        MainWindow.heatModelVisual.Close();
    }

    private void BoundaryConditionIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!model.BoundaryConditions.ContainsKey(BoundaryConditionId.Text))
        {
            NodeId.Text = "";
            Temperature.Text = "";
            return;
        }

        // existing definitions for boundary conditions
        model.BoundaryConditions.TryGetValue(BoundaryConditionId.Text, out existingBoundaryCondition);
        Debug.Assert(existingBoundaryCondition != null, nameof(existingBoundaryCondition) + " != null");
        NodeId.Text = existingBoundaryCondition.NodeId;
        Temperature.Text = existingBoundaryCondition.Prescribed[0].ToString("G3");
    }
}