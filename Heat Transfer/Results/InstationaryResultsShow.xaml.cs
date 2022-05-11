using FEALibrary.Model;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Markup;
using static System.Globalization.CultureInfo;

namespace FE_Analysis.Heat_Transfer.Results
{
    public partial class InstationaryResultsShow
    {
        private readonly double[] time;
        private Node node;
        private readonly FeModel model;

        public InstationaryResultsShow(FeModel model)
        {
            Language = XmlLanguage.GetLanguage("us-US");
            this.model = model;
            InitializeComponent();
            Show();

            NodeSelection.ItemsSource = this.model.Nodes.Keys;

            Dt = this.model.Timeintegration.Dt;
            var tmax = this.model.Timeintegration.Tmax;
            NSteps = (int)(tmax / Dt) + 1;
            time = new double[NSteps];
            for (var i = 0; i < NSteps; i++) time[i] = i * Dt;
            TimeStepSelection.ItemsSource = time;
        }

        private double Dt { get; }
        private int NSteps { get; }
        private int Index { get; set; }
        private string NodeId { get; set; }

        private void DropDownNodeSelectionClosed(object sender, EventArgs e)
        {
            if (NodeSelection.SelectedIndex < 0)
            {
                _ = MessageBox.Show("no valid Node identificator selected", "Zeitschrittauswahl");
                return;
            }
            var nodeId = (string)NodeSelection.SelectedItem;
            if (model.Nodes.TryGetValue(nodeId, out node)) { }
        }

        private void NodalTemperatureGrid_Show(object sender, RoutedEventArgs e)
        {
            if (node == null) return;
            var nodalTemperatures = new Dictionary<string, string>();
            var line = "Temperature at Node " + NodeId;
            line += "\ntime" + "\tTemperature" + "\tHeat Flow";
            nodalTemperatures.Add("Schritt", line);
            for (var i = 0; i < NSteps; i++)
            {
                line = time[i].ToString("N2", InvariantCulture);
                line += "\t" + node.NodalVariables[0][i].ToString("N4", InvariantCulture);
                line += "\t\t" + node.NodalDerivatives[0][i].ToString("N4", InvariantCulture);
                nodalTemperatures.Add(i.ToString(), line);
            }

            NodalTemperatureGrid.ItemsSource = nodalTemperatures;
        }

        private void DropDownTimeStepSelectionClosed(object sender, EventArgs e)
        {
            if (TimeStepSelection.SelectedIndex < 0)
            {
                _ = MessageBox.Show("no valid time step selected", "TimeStepSelection");
                return;
            }
            Index = TimeStepSelection.SelectedIndex;
        }

        private void TimeStepGrid_Show(object sender, RoutedEventArgs e)
        {
            var timeStep = new Dictionary<string, string>();
            var line = "Model state at selected time step  " + Index;
            line += "\nTemperature" + "\tHeat Flow";
            timeStep.Add("Node", line);
            foreach (KeyValuePair<string, Node> item in model.Nodes)
            {
                line = item.Value.NodalVariables[0][Index].ToString("N4", InvariantCulture);
                line += "\t\t" + item.Value.NodalDerivatives[0][Index].ToString("N4", InvariantCulture);
                timeStep.Add(item.Key, line);
            }

            TimeStepGrid.ItemsSource = timeStep;
        }
    }
}