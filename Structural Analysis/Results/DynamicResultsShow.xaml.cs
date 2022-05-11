using FEALibrary.Model;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Markup;

namespace FE_Analysis.Structural_Analysis.Results
{
    public partial class DynamicResultsShow
    {
        private readonly FeModel model;
        private Node node;

        public DynamicResultsShow(FeModel feModel)
        {
            Language = XmlLanguage.GetLanguage("us-US");
            model = feModel;
            InitializeComponent();
            Show();

            NodeSelection.ItemsSource = model.Nodes.Keys;

            // Selection of time step from grid, e.g. every 10th
            Dt = model.Timeintegration.Dt;
            var tmax = model.Timeintegration.Tmax;
            NSteps = (int)(tmax / Dt);
            const int timeGrid = 1;
            //if (NSteps > 1000) timeGrid = 10;
            NSteps = NSteps / timeGrid + 1;
            var time = new double[NSteps];
            for (var i = 0; i < NSteps; i++) time[i] = i * Dt * timeGrid;

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

        private void NodeDeformationsShow(object sender, RoutedEventArgs e)
        {
            if (node == null) return;

            var nodeDeformations = new List<NodeDeformations>();
            var dt = model.Timeintegration.Dt;
            var nSteps = node.NodalVariables[0].Length;
            var time = new double[nSteps + 1];
            time[0] = 0;

            NodeDeformations nodalDeformations = null;
            for (var i = 0; i < nSteps; i++)
            {
                switch (node.NodalVariables.Length)
                {
                    case 2:
                        nodalDeformations = new NodeDeformations(time[i], node.NodalVariables[0][i],
                            node.NodalVariables[1][i],
                            node.NodalDerivatives[0][i], node.NodalDerivatives[1][i]);
                        break;
                    case 3:
                        nodalDeformations = new NodeDeformations(time[i], node.NodalVariables[0][i],
                            node.NodalVariables[1][i], node.NodalVariables[2][i],
                            node.NodalDerivatives[0][i], node.NodalDerivatives[1][i],
                            node.NodalDerivatives[2][i]);
                        break;
                }

                nodeDeformations.Add(nodalDeformations);
                time[i + 1] = time[i] + dt;
            }

            NodeDeformationsGrid.ItemsSource = nodeDeformations;
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

        private void TimeStepGridShow(object sender, RoutedEventArgs e)
        {
            if (Index == 0) return;
            var timeStep = new List<NodeDeformations>();
            var dt = model.Timeintegration.Dt;
            var tmax = model.Timeintegration.Tmax;
            var nSteps = (int)(tmax / dt) + 1;
            var time = new double[nSteps + 1];
            time[0] = 0;

            NodeDeformations nodeDeformations = null;
            foreach (var item in model.Nodes)
            {
                // unit of input e.g. in m, unit of deformation e.g. cm, unit of acceleration e.g. cm/s/s
                const int deformationUnit = 1;
                node = item.Value;
                switch (node.NodalVariables.Length)
                {
                    case 2:
                        nodeDeformations = new NodeDeformations(item.Value.Id,
                            node.NodalVariables[0][Index] * deformationUnit,
                            node.NodalVariables[1][Index] * deformationUnit,
                            node.NodalDerivatives[0][Index] * deformationUnit,
                            node.NodalDerivatives[1][Index] * deformationUnit);
                        break;
                    case 3:
                        nodeDeformations = new NodeDeformations(item.Value.Id,
                            node.NodalVariables[0][Index] * deformationUnit,
                            node.NodalVariables[1][Index] * deformationUnit,
                            node.NodalVariables[2][Index] * deformationUnit,
                            node.NodalDerivatives[0][Index] * deformationUnit,
                            node.NodalDerivatives[1][Index] * deformationUnit,
                            node.NodalDerivatives[2][Index] * deformationUnit);
                        break;
                }

                timeStep.Add(nodeDeformations);
            }

            TimeStepGrid.ItemsSource = timeStep;
        }
    }
}