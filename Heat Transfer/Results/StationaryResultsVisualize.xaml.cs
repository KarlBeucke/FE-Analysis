using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FE_Analysis.Heat_Transfer.Results
{
    public partial class StationaryResultsVisualize
    {
        private readonly List<object> hitList = new List<object>();
        private readonly List<TextBlock> hitTextBlock = new List<TextBlock>();
        private readonly FeModel model;
        private Presentation presentation;
        private EllipseGeometry hitArea;
        private bool nodalTemperaturesOn, elementTemperaturesOn, heatFlowOn;

        public StationaryResultsVisualize(FeModel model)
        {
            Language = XmlLanguage.GetLanguage("us-US");
            this.model = model;
            InitializeComponent();
        }

        private void ModelGrid_Loaded(object sender, RoutedEventArgs e)
        {
            presentation = new Presentation(model, VisualResults);
            presentation.EvaluateResolution();
            presentation.AllElementsDraw();
            presentation.NodalTemperaturesDraw();
            nodalTemperaturesOn = true;
        }

        private void BtnNodalTemperatures_Click(object sender, RoutedEventArgs e)
        {
            if (!nodalTemperaturesOn)
            {
                // draw value of each boundary condition as text to boundary node
                presentation.NodalTemperaturesDraw();
                nodalTemperaturesOn = true;
            }
            else
            {
                // remove ALL texts of temperatures at boundary
                foreach (var knotenTemp in presentation.NodalTemperatures) VisualResults.Children.Remove(knotenTemp);
                nodalTemperaturesOn = false;
            }
        }

        private void BtnHeatFlow_Click(object sender, RoutedEventArgs e)
        {
            if (!heatFlowOn)
            {
                // zeichne ALLE resultierenden Wärmeflussvektoren in Elementschwerpunkten
                presentation.HeatFlowVectorsDraw();

                // zeichne den Wert einer jeden Randbedingung als Text an Randknoten
                presentation.BoundaryConditionDraw();
                heatFlowOn = true;
            }
            else
            {
                // entferne ALLE resultierenden Wärmeflussvektoren in Elementschwerpunkten
                foreach (Shape path in presentation.HeatVectors) VisualResults.Children.Remove(path);

                // entferne ALLE Textdarstellungen der Randbedingungen
                foreach (var rand in presentation.BoundaryNode) VisualResults.Children.Remove((TextBlock)rand);
                heatFlowOn = false;
            }
        }

        private void BtnElementTemperatures_Click(object sender, RoutedEventArgs e)
        {
            if (!elementTemperaturesOn)
            {
                presentation.ElementTemperaturesDraw();
                elementTemperaturesOn = true;
            }
            else
            {
                foreach (var path in presentation.TemperatureElements) VisualResults.Children.Remove(path);
                elementTemperaturesOn = false;
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            hitList.Clear();
            hitTextBlock.Clear();
            var hitPoint = e.GetPosition(VisualResults);
            hitArea = new EllipseGeometry(hitPoint, 1, 1);
            VisualTreeHelper.HitTest(VisualResults, null, HitTestCallBack,
                new GeometryHitTestParameters(hitArea));

            MyPopup.IsOpen = true;

            var sb = new StringBuilder();
            foreach (var item in hitList.Where(item => item != null))
                switch (item)
                {
                    case Polygon polygon:
                        {
                            MyPopup.IsOpen = true;
                            if (model.Elements.TryGetValue(polygon.Name, out var multiKnotenElement))
                            {
                                var element2D = (Abstract2D)multiKnotenElement;
                                var elementTemperaturen = element2D.ComputeElementState(0, 0);
                                sb.Append("\nElement\t= " + element2D.ElementId);
                                sb.Append("\ncenter of Element Tx\t= " + elementTemperaturen[0].ToString("F2"));
                                sb.Append("\ncenter of Element Ty\t= " + elementTemperaturen[1].ToString("F2") + "\n");
                            }

                            MyPopupText.Text = sb.ToString();
                            break;
                        }
                    case Path path:
                        {
                            MyPopup.IsOpen = true;
                            if (model.Elements.TryGetValue(path.Name, out var multiKnotenElement))
                            {
                                var element2D = (Abstract2D)multiKnotenElement;
                                var elementTemperaturen = element2D.ComputeElementState(0, 0);
                                sb.Append("\nElement\t= " + element2D.ElementId);
                                sb.Append("\ncenter of Element Tx\t= " + elementTemperaturen[0].ToString("F2"));
                                sb.Append("\ncenter of Element Ty\t= " + elementTemperaturen[1].ToString("F2") + "\n");
                            }

                            MyPopupText.Text = sb.ToString();
                            break;
                        }
                }

            foreach (var item in hitTextBlock.Where(item => item != null))
            {
                if (!model.Nodes.TryGetValue(item.Name, out var knoten)) continue;
                sb.Append("Node\t\t = " + knoten.Id);
                sb.Append("\nTemperatur\t= " + knoten.NodalDof[0].ToString("F2"));
                if (knoten.Reactions != null)
                    sb.Append("\nHeat Flow\t= " + knoten.Reactions[0].ToString("F2"));
                MyPopupText.Text = sb.ToString();
                break;
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
                        case TextBlock hit:
                            hitTextBlock.Add(hit);
                            break;
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

        private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            MyPopup.IsOpen = false;
        }
    }
}