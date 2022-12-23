using FE_Analysis.Heat_Transfer.Model_Data;
using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FE_Analysis.Heat_Transfer.Results
{
    public partial class InstationaryResultsShow
    {
        private readonly FeModel model;
        private Node node;
        private readonly double[] time;
        private readonly ModelDataShow.HeatDataVisualize heatModel;
        private Shape lastNode;
        private Shape lastElement;

        private double Dt { get; }
        private int NSteps { get; }
        private int Index { get; set; }

        public InstationaryResultsShow(FeModel model)
        {
            Language = XmlLanguage.GetLanguage("us-US");
            this.model = model;
            this.DataContext = this;
            heatModel = new ModelDataShow.HeatDataVisualize(model);
            heatModel.Show();
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

        //NodalTemperatureGrid
        private void DropDownNodeSelectionClosed(object sender, EventArgs e)
        {
            if (NodeSelection.SelectedIndex < 0)
            {
                _ = MessageBox.Show("no valid Node identificator selected", "Zeitschrittauswahl");
                return;
            }
            var nodeId = (string)NodeSelection.SelectedItem;
            if (model.Nodes.TryGetValue(nodeId, out node)) { }

            if (node != null)
            {
                var maxTemperature = node.NodalVariables[0].Max();
                var maxTime = Dt * Array.IndexOf(node.NodalVariables[0], maxTemperature);
                var maxGradient = node.NodalDerivatives[0].Max();
                var maxTimeGradient = Dt * Array.IndexOf(node.NodalDerivatives[0], maxGradient);
                var maxText = "max. Temperature = " + maxTemperature.ToString("N4") + ", at time =" + maxTime.ToString("N2")
                              + "\nmax. Gradient       = " + maxGradient.ToString("N4") + ", at time =" + maxTimeGradient.ToString("N2");
                MaxText.Text = maxText;
            }

            NodalTemperatureGrid_Show();
        }
        private void NodalTemperatureGrid_Show()
        {
            if (node == null) return;
            var nodalTemperatures = new Dictionary<int, double[]>();
            for (var i = 0; i < NSteps; i++)
            {
                var zustand = new double[3];
                zustand[0] = time[i];
                zustand[1] = node.NodalVariables[0][i];
                zustand[2] = node.NodalDerivatives[0][i];
                nodalTemperatures.Add(i, zustand);
            }
            NodalTemperatureGrid.ItemsSource = nodalTemperatures;

            if (lastNode != null)
            {
                heatModel.VisualHeatModel.Children.Remove(lastNode);
            }
            lastNode = heatModel.presentation.NodeIndicate(node, Brushes.Green, 1);
        }

        //NodalValuesGrid
        private void DropDownTimeStepSelectionClosed(object sender, EventArgs e)
        {
            if (TimeStepSelection.SelectedIndex < 0)
            {
                _ = MessageBox.Show("no valid time step selected", "TimeStepSelection");
                return;
            }
            Index = TimeStepSelection.SelectedIndex;
            IntegrationStep.Text = "Modellzustand  an Zeitschritt  " + Index;

            foreach (var item in model.Nodes)
            {
                item.Value.NodalDof[0] = item.Value.NodalVariables[0][Index];
            }
            NodalValuesGrid_Show();
            HeatFlowVectorGrid_Show();
        }
        private void NodalValuesGrid_Show()
        {
            var timeStep = new Dictionary<string, double[]>();
            foreach (var item in model.Nodes)
            {
                var zustand = new double[2];
                zustand[0] = item.Value.NodalVariables[0][Index];
                zustand[1] = item.Value.NodalDerivatives[0][Index];
                timeStep.Add(item.Key, zustand);
            }
            NodalValuesGrid.Items.Clear();
            NodalValuesGrid.ItemsSource = timeStep;
        }
        //SelectionChanged
        private void NodalValuesRowSelected(object sender, System.Windows.Controls.SelectedCellsChangedEventArgs e)
        {
            if (NodalValuesGrid.SelectedCells.Count <= 0) return;
            var cellInfo = NodalValuesGrid.SelectedCells[0];
            var cell = (KeyValuePair<string, double[]>)cellInfo.Item;
            var nodeId = cell.Key;
            if (model.Nodes.TryGetValue(nodeId, out node)) { }
            if (lastNode != null)
            {
                heatModel.VisualHeatModel.Children.Remove(lastNode);
            }
            lastNode = heatModel.presentation.NodeIndicate(node, Brushes.Green, 1);
        }
        //LostFocus
        private void NoNodalValuesSelected(object sender, RoutedEventArgs e)
        {
            heatModel.VisualHeatModel.Children.Remove(lastNode);
        }

        private void HeatFlowVectorGrid_Show()
        {
            foreach (var item in model.Elements)
            {
                switch (item.Value)
                {
                    case Abstract2D value:
                        {
                            value.ElementState = value.ComputeElementState(0, 0);
                            break;
                        }
                    case Element3D8 value:
                        {
                            value.ElementState = value.ComputeElementState(0, 0, 0);
                            break;
                        }
                }
            }
            if (HeatFlowVectorGrid != null) HeatFlowVectorGrid.ItemsSource = model.Elements;
        }
        //SelectionChanged
        private void ElementRowSelected(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (HeatFlowVectorGrid.SelectedCells.Count <= 0) return;
            var cellInfo = HeatFlowVectorGrid.SelectedCells[0];
            var cell = (KeyValuePair<string, AbstractElement>)cellInfo.Item;
            var element = cell.Value;
            if (lastElement != null)
            {
                heatModel.VisualHeatModel.Children.Remove(lastElement);
            }
            lastElement = heatModel.presentation.ElementFillDraw((Abstract2D)element,
                Brushes.Black, Colors.Green, .2, 2);
        }
        //LostFocus
        private void NoElementSelected(object sender, RoutedEventArgs e)
        {
            heatModel.VisualHeatModel.Children.Remove(lastElement);
            lastNode = null;
        }

        //Unloaded
        private void ModelClose(object sender, RoutedEventArgs e)
        {
            heatModel.Close();
        }
    }
}