using FEALibrary.Model;
using System;
using System.Windows;
using System.Windows.Markup;

namespace FE_Analysis.Heat_Transfer.Results
{
    public partial class InstationaryModelStatesVisualize
    {
        private readonly Presentation presentation;
        private int index;
        private bool nodalTemperaturesOn, nodalGradientsOn, elementTemperaturesOn;

        public InstationaryModelStatesVisualize(FeModel model)
        {
            Language = XmlLanguage.GetLanguage("us-US");
            InitializeComponent();
            Show();

            presentation = new Presentation(model, VisualResults);
            presentation.EvaluateResolution();
            presentation.ElementsDraw();

            // selection of time step
            var dt = model.Timeintegration.Dt;
            var tmax = model.Timeintegration.Tmax;
            var nSteps = (int)(tmax / dt) + 1;
            var time = new double[nSteps];
            for (var i = 0; i < nSteps; i++) time[i] = i * dt;
            TimeStepSelection.ItemsSource = time;
        }

        private void DropDownTimeStepSelectionClosed(object sender, EventArgs e)
        {
            if (TimeStepSelection.SelectedIndex < 0)
            {
                _ = MessageBox.Show("no valid time step selected", "TimeStepSelection");
                return;
            }
            index = TimeStepSelection.SelectedIndex;

            presentation.timeStep = index;
            presentation.NodalTemperaturesDraw(index);
        }

        private void BtnNodalTemperatures_Click(object sender, RoutedEventArgs e)
        {
            if (!nodalTemperaturesOn)
            {
                if (index == 0)
                {
                    _ = MessageBox.Show("Time step must be selected first", "instationary Heat Transfer Analysis");
                }
                else
                {
                    presentation.NodalTemperaturesDraw(index);
                    nodalTemperaturesOn = true;
                }
            }
            else
            {
                // remove ALL texts of Nodal Temperatures
                foreach (var knotenTemp in presentation.NodalTemperatures) VisualResults.Children.Remove(knotenTemp);
                nodalTemperaturesOn = false;
            }
        }

        private void BtnNodalGradients_Click(object sender, RoutedEventArgs e)
        {
            if (!nodalGradientsOn)
            {
                if (index == 0)
                {
                    _ = MessageBox.Show("Time step must be selected first", "instationary Heat Transfer Analysis");
                }
                else
                {
                    presentation.NodalHeatFlowDraw(index);
                    nodalGradientsOn = true;
                }
            }
            else
            {
                // remove ALL texts of Nodal Temperature gradients
                foreach (var knotenGrad in presentation.NodalGradients) VisualResults.Children.Remove(knotenGrad);
                nodalGradientsOn = false;
            }
        }

        private void BtnElementTemperatures_Click(object sender, RoutedEventArgs e)
        {
            if (!elementTemperaturesOn)
            {
                if (index == 0)
                {
                    _ = MessageBox.Show("Time step must be selected first", "instationary Heat Transfer Analysis");
                }
                else
                {
                    presentation.ElementTemperaturesDraw();
                    presentation.HeatFlowVectorsDraw();
                    elementTemperaturesOn = true;
                }
            }
            else
            {
                foreach (var path in presentation.TemperatureElements) VisualResults.Children.Remove(path);
                elementTemperaturesOn = false;
            }
        }
    }
}