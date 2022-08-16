using FEALibrary.Model;
using System;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Linq;
using FEALibrary.Model.abstractClasses;

namespace FE_Analysis.Heat_Transfer.Results
{
    public partial class InstationaryModelStatesVisualize
    {
        private readonly FeModel model;
        private readonly Presentation presentation;
        private int index;
        private bool nodalTemperaturesOn, nodalGradientsOn, elementTemperaturesOn;
        private readonly List<Shape> hitList = new List<Shape>();
        private EllipseGeometry hitArea;

        public InstationaryModelStatesVisualize(FeModel model)
        {
            this.model = model;
            Language = XmlLanguage.GetLanguage("us-US");
            InitializeComponent();
            Show();

            presentation = new Presentation(model, VisualResults);
            presentation.EvaluateResolution();
            presentation.AllElementsDraw();

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

            foreach (var item in model.Nodes)
            {
                item.Value.NodalDof[0] = item.Value.NodalVariables[0][index];
            }

            presentation.timeStep = index;
            NodalTemperaturesDraw();
            presentation.HeatFlowVectorsDraw();
            ElementTemperaturesDraw();
        }
        private void NodalTemperaturesDraw()
        {
            if (!nodalTemperaturesOn)
            {
                if (index == 0)
                {
                    _ = MessageBox.Show("Time step must be selected first", "instationary Heat Transfer Analysis");
                }
                else
                {
                    presentation.NodalTemperaturesDraw();
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
        private void ElementTemperaturesDraw()
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
        
        private void BtnNodalTemperatures_Click(object sender, RoutedEventArgs e)
        {
            NodalTemperaturesDraw();
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
            ElementTemperaturesDraw();
        }
        
        private void OnMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            hitList.Clear();
            var hitPoint = e.GetPosition(VisualResults);
            hitArea = new EllipseGeometry(hitPoint, 0.2, 0.2);
            VisualTreeHelper.HitTest(VisualResults, null, HitTestCallBack,
                new GeometryHitTestParameters(hitArea));

            MyPopup.IsOpen = false;

            var sb = new StringBuilder();
            foreach (var item in hitList.Where(item => !(item == null | item?.Name == string.Empty)))
            {
                sb.Clear();
                MyPopup.IsOpen = true;

                if (!model.Elements.TryGetValue(item.Name, out var element2D)) continue;
                sb.Clear();
                var heatElement = (Abstract2D)element2D;
                var heatFlow = heatElement.ComputeElementState(0, 0);

                sb.Append("Element = " + heatElement.ElementId);
                sb.Append("\nheat flow x\t= " + heatFlow[0].ToString("G4"));
                sb.Append("\nheat flow y\t= " + heatFlow[1].ToString("G4"));

                MyPopupText.Text = sb.ToString();
            }
        }
        private HitTestResultBehavior HitTestCallBack(HitTestResult result)
        {
            var intersectionDetail = ((GeometryHitTestResult)result).IntersectionDetail;

            switch (intersectionDetail)
            {
                case IntersectionDetail.Empty:
                    return HitTestResultBehavior.Continue;
                case IntersectionDetail.FullyContains:
                    switch (result.VisualHit)
                    {
                        case Shape hit:
                            hitList.Add(hit);
                            break;
                        //case TextBlock hit:
                        //    hitTextBlock.Add(hit);
                        //    break;
                    }
                    return HitTestResultBehavior.Continue;
                case IntersectionDetail.FullyInside:
                    return HitTestResultBehavior.Continue;
                case IntersectionDetail.Intersects:
                    switch (result.VisualHit)
                    {
                        case Shape hit:
                            hitList.Add(hit);
                            break;
                    }
                    return HitTestResultBehavior.Continue;
                case IntersectionDetail.NotCalculated:
                    return HitTestResultBehavior.Continue;
                default:
                    return HitTestResultBehavior.Stop;
            }
        }

        private void OnMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MyPopup.IsOpen = false;
        }
    }
}