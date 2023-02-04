using FEALibrary.Model;
using System.Windows;
using System.Windows.Markup;

namespace FE_Analysis.Heat_Transfer.ModelDataRead;

public partial class TimeInitialTemperatureNew
{
    private readonly FeModel model;
    private int current;

    public TimeInitialTemperatureNew(FeModel model)
    {
        Language = XmlLanguage.GetLanguage("us-US");
        InitializeComponent();
        current = MainWindow.heatModelVisual.timeIntegrationNew.current;

        this.model = model;
        if (model.Timeintegration.FromStationary)
        {
            StationarySolution.IsChecked = true;
            NodeId.Text = "";
            InitialTemperature.Text = "";
        }
        else
        {
            var initial = (NodalValues)model.Timeintegration.InitialConditions[current];
            NodeId.Text = initial.NodeId;
            InitialTemperature.Text = initial.Values[0].ToString("G2");
        }
        Show();
    }
    private void StationarySolutionChecked(object sender, RoutedEventArgs e)
    {
        StationarySolution.IsChecked = true;
        NodeId.Text = "";
        InitialTemperature.Text = "";
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        if (NodeId.Text.Length == 0) Close();
        if (StationarySolution.IsChecked == true)
        {
            model.Timeintegration.FromStationary = true;
            model.Timeintegration.InitialConditions.Clear();
            Close();
            return;
        }

        // add new initial condition
        if (MainWindow.heatModelVisual.timeIntegrationNew.current > model.Timeintegration.InitialConditions.Count)
        {
            if (NodeId.Text == "") return;
            var values = new double[1];
            values[0] = double.Parse(InitialTemperature.Text);
            var nodalValues = new NodalValues(NodeId.Text, values);
            model.Timeintegration.InitialConditions.Add(nodalValues);
            model.Timeintegration.FromStationary = false;
        }
        // edit existing initial condition
        else
        {
            var initial = (NodalValues)model.Timeintegration.InitialConditions[current];
            initial.NodeId = NodeId.Text;
            initial.Values[0] = double.Parse(InitialTemperature.Text);
        }
        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        model.Timeintegration.InitialConditions.RemoveAt(current + 1);
        current = 0;
        if (model.Timeintegration.InitialConditions.Count <= 0)
        {
            Close();
            MainWindow.heatModelVisual.timeIntegrationNew.Close();
            return;
        }
        var initial = (NodalValues)model.Timeintegration.InitialConditions[current];
        NodeId.Text = initial.NodeId;
        InitialTemperature.Text = initial.Values[0].ToString("G2");
        StationarySolution.IsChecked = model.Timeintegration.FromStationary;
        Close();
        MainWindow.heatModelVisual.timeIntegrationNew.Close();
    }
}