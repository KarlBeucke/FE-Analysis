using FEALibrary.Model;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using static System.Globalization.CultureInfo;
using static System.Windows.Controls.Canvas;
using static System.Windows.Media.Brushes;
using static System.Windows.Media.Color;

namespace FE_Analysis.Structural_Analysis.Results
{
    public partial class NodalTimeHistoriesVisualize
    {
        private readonly FeModel model;
        private Node node;
        private readonly double dt;
        private double time;
        private double maxDeformation, minDeformation;
        private double absMaxDeformation;
        private double maxAcceleration, minAcceleration;
        private double absMaxAcceleration;
        private double clippingMax, clippingMin;

        private readonly Presentation presentation;
        private ClippingFrameDialog clippingFrame;
        private bool clippingFrameNew;
        private bool deltaXTimeHistory, deltaYTimeHistory;
        private bool accXTimeHistory, accYTimeHistory;
        private TextBlock maximum;

        public NodalTimeHistoriesVisualize(FeModel feModel)
        {
            Language = XmlLanguage.GetLanguage("us-US");
            model = feModel;
            InitializeComponent();
            Show();

            // definition of time axis
            dt = model.Timeintegration.Dt;
            double tmin = 0;
            var tmax = model.Timeintegration.Tmax;
            clippingMin = tmin;
            clippingMax = tmax;

            // Auswahl des Knotens         
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

        private void BtnDeltaX_Click(object sender, RoutedEventArgs e)
        {
            deltaYTimeHistory = false; accXTimeHistory = false; accYTimeHistory = false;
            maxDeformation = node.NodalVariables[0].Max();
            minDeformation = node.NodalVariables[0].Min();
            if (maxDeformation > Math.Abs(minDeformation))
            {
                time = dt * Array.IndexOf(node.NodalVariables[0], maxDeformation);
                absMaxDeformation = maxDeformation;
                DeltaXNewDraw();
            }
            else
            {
                time = dt * Array.IndexOf(node.NodalVariables[0], minDeformation);
                absMaxDeformation = minDeformation;
                DeltaXNewDraw();
            }
        }

        private void DeltaXNewDraw()
        {
            if (node == null)
            {
                _ = MessageBox.Show("Node must be selcted first", "dynamic Structural Analysis");
            }
            else
            {
                if (clippingFrameNew)
                {
                    VisualResults.Children.Clear();
                    clippingMin = clippingFrame.tmin;
                    clippingMax = clippingFrame.tmax;
                    maxDeformation = Math.Abs(clippingFrame.maxDeformation);
                    minDeformation = -maxDeformation;
                }
                else
                {
                    VisualResults.Children.Clear();
                    maxDeformation = Math.Abs(absMaxDeformation);
                    minDeformation = -maxDeformation;
                }

                ClippingFrame.Text = clippingMin.ToString("N2", InvariantCulture) + " <= time <= "
                                                        + clippingMax.ToString("N2", InvariantCulture);
                if (maxDeformation < double.Epsilon) return;
                presentation.CoordinateSystem(clippingMin, clippingMax, maxDeformation, minDeformation);

                // text representation of maximum value with integration time
                MaximumValueText("Deformation x", absMaxDeformation, time);

                presentation.TimeHistoryDraw(dt, clippingMin, clippingMax, maxDeformation,
                    node.NodalVariables[0]);

                deltaXTimeHistory = true; deltaYTimeHistory = false;
                accXTimeHistory = false; accYTimeHistory = false;
                clippingFrameNew = false;
            }
        }

        private void BtnDeltaY_Click(object sender, RoutedEventArgs e)
        {
            deltaXTimeHistory = false; accXTimeHistory = false; accYTimeHistory = false;
            maxDeformation = node.NodalVariables[1].Max();
            minDeformation = node.NodalVariables[1].Min();
            if (maxDeformation > Math.Abs(minDeformation))
            {
                time = dt * Array.IndexOf(node.NodalVariables[1], maxDeformation);
                absMaxDeformation = maxDeformation;
                DeltaYNewDraw();
            }
            else
            {
                time = dt * Array.IndexOf(node.NodalVariables[1], minDeformation);
                absMaxDeformation = minDeformation;
                DeltaYNewDraw();
            }
        }
        private void DeltaYNewDraw()
        {
            if (node == null)
            {
                _ = MessageBox.Show("Node must be selected first", "dynamic Structural Analysis");
            }
            else
            {
                if (clippingFrameNew)
                {
                    VisualResults.Children.Clear();
                    clippingMin = clippingFrame.tmin;
                    clippingMax = clippingFrame.tmax;
                    maxDeformation = Math.Abs(clippingFrame.maxDeformation);
                    minDeformation = -maxDeformation;
                }
                else
                {
                    VisualResults.Children.Clear();
                    maxDeformation = Math.Abs(absMaxDeformation);
                    minDeformation = -maxDeformation;
                }

                ClippingFrame.Text = clippingMin.ToString("N2", InvariantCulture) + " <= time <= "
                                                                        + clippingMax.ToString("N2", InvariantCulture);
                if (maxDeformation < double.Epsilon) return;
                presentation.CoordinateSystem(clippingMin, clippingMax, maxDeformation, minDeformation);

                // text representation of maximum value with integration time
                MaximumValueText("Deformation y", absMaxDeformation, time);

                presentation.TimeHistoryDraw(dt, clippingMin, clippingMax, maxDeformation,
                    node.NodalVariables[1]);

                deltaXTimeHistory = false; deltaYTimeHistory = true;
                accXTimeHistory = false; accYTimeHistory = false;
                clippingFrameNew = false;
            }
        }

        private void BtnAccX_Click(object sender, RoutedEventArgs e)
        {
            deltaXTimeHistory = false; deltaYTimeHistory = false; accYTimeHistory = false;
            maxAcceleration = node.NodalDerivatives[0].Max();
            minAcceleration = node.NodalDerivatives[0].Min();
            if (maxAcceleration > Math.Abs(minAcceleration))
            {
                time = dt * Array.IndexOf(node.NodalDerivatives[0], maxAcceleration);
                absMaxAcceleration = maxAcceleration;
                AccXNewDraw();
            }
            else
            {
                time = dt * Array.IndexOf(node.NodalDerivatives[0], minAcceleration);
                absMaxAcceleration = minAcceleration;
                AccXNewDraw();
            }
        }
        private void AccXNewDraw()
        {
            if (node == null)
            {
                _ = MessageBox.Show("Node must be selected first", "dynamic Structural Analysis");
            }
            else
            {
                if (clippingFrameNew)
                {
                    VisualResults.Children.Clear();
                    clippingMin = clippingFrame.tmin;
                    clippingMax = clippingFrame.tmax;
                    maxAcceleration = Math.Abs(clippingFrame.maxAcceleration);
                    minAcceleration = -maxAcceleration;
                }
                else
                {
                    VisualResults.Children.Clear();
                    maxAcceleration = Math.Abs(absMaxAcceleration);
                    minAcceleration = -maxAcceleration;
                }

                ClippingFrame.Text = clippingMin.ToString("N2", InvariantCulture) + " <= time <= "
                                                                        + clippingMax.ToString("N2", InvariantCulture);
                if (maxAcceleration < double.Epsilon) return;
                presentation.CoordinateSystem(clippingMin, clippingMax, maxAcceleration, minAcceleration);

                // text representation of maximum value with integration time
                MaximumValueText("Acceleration x", absMaxAcceleration, time);

                presentation.TimeHistoryDraw(dt, clippingMin, clippingMax, maxAcceleration,
                    node.NodalDerivatives[0]);

                deltaXTimeHistory = false; deltaYTimeHistory = false;
                accXTimeHistory = true; accYTimeHistory = false;
                clippingFrameNew = false;
            }
        }

        private void BtnAccY_Click(object sender, RoutedEventArgs e)
        {
            deltaXTimeHistory = false; deltaYTimeHistory = false; accXTimeHistory = false;
            maxAcceleration = node.NodalDerivatives[1].Max();
            minAcceleration = node.NodalDerivatives[1].Min();
            if (maxAcceleration > Math.Abs(minAcceleration))
            {
                time = dt * Array.IndexOf(node.NodalDerivatives[1], maxAcceleration);
                absMaxAcceleration = maxAcceleration;
                AccYNewDraw();
            }
            else
            {
                time = dt * Array.IndexOf(node.NodalDerivatives[1], minAcceleration);
                absMaxAcceleration = minAcceleration;
                AccYNewDraw();
            }
        }
        private void AccYNewDraw()
        {

            if (node == null)
            {
                _ = MessageBox.Show("Node must be selected first", "dynamic Structural Analysis");
            }
            else
            {
                if (clippingFrameNew)
                {
                    VisualResults.Children.Clear();
                    clippingMin = clippingFrame.tmin;
                    clippingMax = clippingFrame.tmax;
                    maxAcceleration = Math.Abs(clippingFrame.maxAcceleration);
                    minAcceleration = -maxAcceleration;
                }
                else
                {
                    VisualResults.Children.Clear();
                    maxAcceleration = Math.Abs(absMaxAcceleration);
                    minAcceleration = -maxAcceleration;
                }

                ClippingFrame.Text = clippingMin.ToString("N2", InvariantCulture) + " <= time <= "
                                                                        + clippingMax.ToString("N2", InvariantCulture);
                if (maxAcceleration < double.Epsilon) return;
                presentation.CoordinateSystem(clippingMin, clippingMax, maxAcceleration, minAcceleration);

                // text representation of maximum value with integration time
                MaximumValueText("Acceleration y", absMaxAcceleration, time);

                presentation.TimeHistoryDraw(dt, clippingMin, clippingMax, maxAcceleration,
                    node.NodalDerivatives[1]);

                deltaXTimeHistory = false; deltaYTimeHistory = false;
                accXTimeHistory = false; accYTimeHistory = true;
                clippingFrameNew = false;
            }
        }

        private void ClippingFrameChangeDialog_Click(object sender, RoutedEventArgs e)
        {
            if (node == null)
            {
                _ = MessageBox.Show("Node must be selected first", "dynamic Structural Analysis");
            }
            else
            {
                VisualResults.Children.Clear();
                clippingFrame = new ClippingFrameDialog(clippingMin, clippingMax, absMaxDeformation, absMaxAcceleration);
                clippingMin = clippingFrame.tmin;
                clippingMax = clippingFrame.tmax;
                maxDeformation = clippingFrame.maxDeformation;
                maxAcceleration = clippingFrame.maxAcceleration;
                clippingFrameNew = true;
                if (deltaXTimeHistory) DeltaXNewDraw();
                else if (deltaYTimeHistory) DeltaYNewDraw();
                else if (accXTimeHistory) AccXNewDraw();
                else if (accYTimeHistory) AccYNewDraw();
            }
        }

        private void MaximumValueText(string ordinate, double wert, double zeit)
        {
            var red = FromArgb(120, 255, 0, 0);
            var myBrush = new SolidColorBrush(red);
            var maxValue = "Maximum Value for " + ordinate + " = " + wert.ToString("N4", InvariantCulture) + Environment.NewLine +
                          "at Time = " + zeit.ToString("N2", InvariantCulture);
            maximum = new TextBlock
            {
                FontSize = 12,
                Background = myBrush,
                Foreground = Black,
                FontWeight = FontWeights.Bold,
                Text = maxValue
            };
            SetTop(maximum, 10);
            SetLeft(maximum, 20);
            VisualResults.Children.Add(maximum);
        }
    }
}