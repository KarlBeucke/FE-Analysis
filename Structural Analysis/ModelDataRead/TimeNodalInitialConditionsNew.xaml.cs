using FEALibrary.Model;
using System.Windows;

namespace FE_Analysis.Structural_Analysis.ModelDataRead;

public partial class TimeNodalInitialConditionsNew
{
    private readonly FeModel model;
    private int current;

    public TimeNodalInitialConditionsNew(FeModel model)
    {
        InitializeComponent();
        this.model = model;
        current = MainWindow.structuralModel.timeIntegrationNew.current;
        var start = (NodalValues)model.Timeintegration.InitialConditions[current];
        NodeId.Text = start.NodeId;
        Dof1D0.Text = start.Values[0].ToString("G2");
        Dof1V0.Text = start.Values[1].ToString("G2");
        if (start.Values.Length > 2)
        {
            Dof2D0.Text = start.Values[2].ToString("G2");
            Dof2V0.Text = start.Values[3].ToString("G2");
        }
        if (start.Values.Length > 4)
        {
            Dof3D0.Text = start.Values[4].ToString("G2");
            Dof3V0.Text = start.Values[5].ToString("G2");
        }
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        if (NodeId.Text.Length == 0) Close();

        // add new initial condition
        if (MainWindow.structuralModel.timeIntegrationNew.current > model.Timeintegration.InitialConditions.Count)
        {
            if (NodeId.Text == "") return;
            var nodeId = NodeId.Text;
            if (model.Nodes.TryGetValue(nodeId, out var node))
            {
                var nodalDof = node.NumberOfNodalDof;
                var initialValues = new double[2 * nodalDof];
                if (Dof1D0.Text != string.Empty) { initialValues[0] = double.Parse(Dof1D0.Text); }
                if (Dof1V0.Text != string.Empty) { initialValues[1] = double.Parse(Dof1V0.Text); }

                switch (nodalDof)
                {
                    case 2:
                        {
                            if (Dof2D0.Text != string.Empty) { initialValues[2] = double.Parse(Dof2D0.Text); }
                            if (Dof2V0.Text != string.Empty) { initialValues[3] = double.Parse(Dof2V0.Text); }
                            break;
                        }
                    case 3:
                        {
                            if (Dof3D0.Text != string.Empty) { initialValues[4] = double.Parse(Dof3D0.Text); }
                            if (Dof3V0.Text != string.Empty) { initialValues[5] = double.Parse(Dof3V0.Text); }
                            break;
                        }
                }
                model.Timeintegration.InitialConditions.Add(new NodalValues(NodeId.Text, initialValues));
            }
            // change existing initial condition
            else
            {
                var start = (NodalValues)model.Timeintegration.InitialConditions[current];
                start.NodeId = NodeId.Text;
                start.Values[0] = double.Parse(Dof1D0.Text); start.Values[1] = double.Parse(Dof1V0.Text);
                start.Values[2] = double.Parse(Dof2D0.Text); start.Values[3] = double.Parse(Dof2V0.Text);
                start.Values[4] = double.Parse(Dof3D0.Text); start.Values[5] = double.Parse(Dof3V0.Text);
            }
        }
        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
        MainWindow.structuralModel.timeIntegrationNew.Close();
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        model.Timeintegration.InitialConditions.RemoveAt(current + 1);
        current = 0;
        if (model.Timeintegration.InitialConditions.Count <= 0)
        {
            Close();
            MainWindow.structuralModel.timeIntegrationNew.Close();
            return;
        }
        var initialValues = (NodalValues)model.Timeintegration.InitialConditions[current];
        NodeId.Text = initialValues.NodeId;
        Dof1D0.Text = initialValues.Values[0].ToString("G2");
        Dof1V0.Text = initialValues.Values[1].ToString("G2");

        if (initialValues.Values.Length > 2)
        {
            Dof2D0.Text = initialValues.Values[2].ToString("G2");
            Dof2V0.Text = initialValues.Values[3].ToString("G2");
        }

        if (initialValues.Values.Length > 4)
        {
            Dof3D0.Text = initialValues.Values[4].ToString("G2");
            Dof3V0.Text = initialValues.Values[5].ToString("G2");
        }
        Close();
        MainWindow.structuralModel.timeIntegrationNew.Close();
    }
}