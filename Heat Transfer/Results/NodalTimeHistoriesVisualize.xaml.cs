using FEALibrary.Model;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using static System.Globalization.CultureInfo;
using static System.Windows.Media.Brushes;
using static System.Windows.Media.Color;

namespace FE_Analysis.Heat_Transfer.Results
{
    public partial class NodalTimeHistoriesVisualize
    {
        private readonly FeModel model;
        private Node node;
        private readonly double dt;
        private double time;
        private double maxTemperature, minTemperature;
        private double absMaxTemperature;
        private double maxHeatFlow, minHeatFlow;
        private double absMaxHeatFlow;

        private readonly Presentation presentation;
        private ClippingFrame clipping;
        private double clippingMax, clippingMin;
        private bool clippingFrameNew;
        private bool temperatureTimeHistory, heatFlowTimeHistory;
        private TextBlock maximum;

        public NodalTimeHistoriesVisualize(FeModel model)
        {
            Language = XmlLanguage.GetLanguage("us-US");
            this.model = model;
            InitializeComponent();
            Show();

            // Definition of Time Axis
            dt = model.Timeintegration.Dt;
            double tmin = 0;
            var tmax = model.Timeintegration.Tmax;
            clippingMin = tmin;
            clippingMax = tmax;

            // Selection of Node
            NodeSelection.ItemsSource = model.Nodes.Keys;

            // initialization of drawing area
            presentation = new Presentation(model, VisualResults);
        }

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

        private void BtnNodalTemperature_Click(object sender, RoutedEventArgs e)
        {
            heatFlowTimeHistory = false;
            maxTemperature = node.NodalVariables[0].Max();
            minTemperature = node.NodalVariables[0].Min();
            if (maxTemperature > Math.Abs(minTemperature))
            {
                time = dt * Array.IndexOf(node.NodalVariables[0], maxTemperature);
                absMaxTemperature = maxTemperature;
                TemperatureNewDraw();
            }
            else
            {
                time = dt * Array.IndexOf(node.NodalVariables[0], minTemperature);
                absMaxTemperature = minTemperature;
                TemperatureNewDraw();
            }
        }
        private void TemperatureNewDraw()
        {
            if (node == null)
            {
                _ = MessageBox.Show("Node must be selected first", "instationary Heat Transfer Analysis");
            }
            else
            {
                if (clippingFrameNew)
                {
                    VisualResults.Children.Clear();
                    clippingMin = clipping.tmin;
                    clippingMax = clipping.tmax;
                    maxTemperature = Math.Abs(clipping.maxTemperature);
                    minTemperature = -maxTemperature;
                }
                else
                {
                    VisualResults.Children.Clear();
                    maxTemperature = Math.Abs(node.NodalVariables[0].Max());
                    minTemperature = -maxTemperature;
                }

                ClippingFrame.Text = clippingMin.ToString("N2", InvariantCulture) + " <= time <= "
                                                                      + clippingMax.ToString("N2", InvariantCulture);
                if (maxTemperature < double.Epsilon) return;
                presentation.CoordinateSystem(clippingMin, clippingMax, maxTemperature, minTemperature);

                // text for maximum temperatures with corresponding time step
                MaximumValueText("Temperature", absMaxTemperature, time);

                presentation.TimeHistoryDraw(dt, clippingMin, clippingMax, maxTemperature, node.NodalVariables[0]);

                temperatureTimeHistory = true;
                heatFlowTimeHistory = false;
                clippingFrameNew = false;
            }
        }

        private void BtnHeatFlow_Click(object sender, RoutedEventArgs e)
        {
            temperatureTimeHistory = false;
            maxHeatFlow = node.NodalDerivatives[0].Max();
            minHeatFlow = node.NodalDerivatives[0].Min();
            if (maxHeatFlow > Math.Abs(minHeatFlow))
            {
                time = dt * Array.IndexOf(node.NodalDerivatives[0], maxHeatFlow);
                absMaxHeatFlow = maxHeatFlow;
                HeatFlowNewDraw();
            }
            else
            {
                time = dt * Array.IndexOf(node.NodalDerivatives[0], minHeatFlow);
                absMaxHeatFlow = minHeatFlow;
                HeatFlowNewDraw();
            }
        }
        private void HeatFlowNewDraw()
        {
            const int infiniteHeatFlowDisplay = 100;
            if (node == null)
            {
                _ = MessageBox.Show("Node must be selected first", "instationary Heat Transfer Analysis");
            }
            else
            {
                if (clippingFrameNew)
                {
                    VisualResults.Children.Clear();
                    clippingMin = clipping.tmin;
                    clippingMax = clipping.tmax;
                    maxHeatFlow = Math.Abs(clipping.maxHeatFlow);
                    minHeatFlow = -maxHeatFlow;
                }
                else
                {
                    VisualResults.Children.Clear();
                    maxHeatFlow = Math.Abs(absMaxHeatFlow);
                    minHeatFlow = -maxHeatFlow;
                }

                ClippingFrame.Text = clippingMin.ToString("N2", InvariantCulture) + " <= time <= "
                                                                + clippingMax.ToString("N2", InvariantCulture);
                if (maxHeatFlow > double.MaxValue) { maxHeatFlow = infiniteHeatFlowDisplay; minHeatFlow = -maxHeatFlow; }
                presentation.CoordinateSystem(clippingMin, clippingMax, maxHeatFlow, minHeatFlow);

                // text for maximum heat flow with corresponding time step
                VisualResults.Children.Remove(maximum);
                MaximumValueText("Heat Flow", absMaxHeatFlow, time);

                presentation.TimeHistoryDraw(dt, clippingMin, clippingMax, maxHeatFlow, node.NodalDerivatives[0]);

                temperatureTimeHistory = false;
                heatFlowTimeHistory = true;
                clippingFrameNew = false;
            }
        }

        private void ClippingFrameDialog_Click(object sender, RoutedEventArgs e)
        {
            if (node == null)
            {
                _ = MessageBox.Show("Node must be selected first", "instationary Heat Transfer Analysis");
            }
            else
            {
                VisualResults.Children.Clear();
                clipping = new ClippingFrame(clippingMin, clippingMax, absMaxTemperature, absMaxHeatFlow);
                clippingMin = clipping.tmin;
                clippingMax = clipping.tmax;
                maxTemperature = clipping.maxTemperature;
                maxHeatFlow = clipping.maxHeatFlow;
                clippingFrameNew = true;
                if (temperatureTimeHistory) TemperatureNewDraw();
                else if (heatFlowTimeHistory) HeatFlowNewDraw();
            }
        }

        private void MaximumValueText(string ordinate, double value, double maxTime)
        {
            var rot = FromArgb(120, 255, 0, 0);
            var myBrush = new SolidColorBrush(rot);
            var maxValue = "Maximum Value for " + ordinate + " = " + value.ToString("N2", InvariantCulture) + Environment.NewLine +
                          "at Time = " + maxTime.ToString("N2", InvariantCulture);
            maximum = new TextBlock
            {
                FontSize = 12,
                Background = myBrush,
                Foreground = Black,
                FontWeight = FontWeights.Bold,
                Text = maxValue
            };
            Canvas.SetTop(maximum, 10);
            Canvas.SetLeft(maximum, 20);
            VisualResults.Children.Add(maximum);
        }
    }
}