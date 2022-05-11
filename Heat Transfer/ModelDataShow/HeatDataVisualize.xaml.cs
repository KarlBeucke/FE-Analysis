using FEALibrary.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FE_Analysis.Heat_Transfer.ModelDataShow
{
    public partial class HeatDataVisualize
    {
        private readonly Presentation presentation;
        private readonly List<Shape> hitList = new List<Shape>();
        private readonly List<TextBlock> hitTextBlock = new List<TextBlock>();
        private readonly FeModel model;
        private EllipseGeometry hitArea;
        private bool nodesOn = true, elementsOn = true;
        private bool nodeLoadsOn, elementLoadsOn, boundaryConditionsOn;

        public HeatDataVisualize(FeModel feModel)
        {
            model = feModel;
            Language = XmlLanguage.GetLanguage("us-US");
            InitializeComponent();
            Show();

            presentation = new Presentation(model, VisualModel);
            presentation.ElementsDraw();

            // mit Knoten und Element Ids
            presentation.NodeIds();
            presentation.ElementIds();
        }

        private void BtnKnoten_Click(object sender, RoutedEventArgs e)
        {
            if (!nodesOn)
            {
                presentation.NodeIds();
                nodesOn = true;
            }
            else
            {
                foreach (TextBlock id in presentation.NodeIDs) VisualModel.Children.Remove(id);
                nodesOn = false;
            }
        }

        private void BtnElemente_Click(object sender, RoutedEventArgs e)
        {
            if (!elementsOn)
            {
                presentation.ElementIds();
                elementsOn = true;
            }
            else
            {
                foreach (TextBlock id in presentation.ElementIDs) VisualModel.Children.Remove(id);
                elementsOn = false;
            }
        }

        private void BtnKnotenlasten_Click(object sender, RoutedEventArgs e)
        {
            if (!nodeLoadsOn)
            {
                presentation.NodeLoadDraw();
                nodeLoadsOn = true;
            }
            else
            {
                foreach (TextBlock id in presentation.LoadNodes) VisualModel.Children.Remove(id);
                nodeLoadsOn = false;
            }
        }

        private void BtnElementlasten_Click(object sender, RoutedEventArgs e)
        {
            if (!elementLoadsOn)
            {
                presentation.ElementLoadDraw();
                elementLoadsOn = true;
            }
            else
            {
                foreach (Shape lastElement in presentation.LoadElements) VisualModel.Children.Remove(lastElement);
                elementLoadsOn = false;
            }
        }

        private void BtnRandbedingungen_Click(object sender, RoutedEventArgs e)
        {
            if (!boundaryConditionsOn)
            {
                presentation.BoundaryConditionDraw();
                boundaryConditionsOn = true;
            }
            else
            {
                foreach (TextBlock randbedingung in presentation.BoundaryNode) VisualModel.Children.Remove(randbedingung);
                boundaryConditionsOn = false;
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MyPopup.IsOpen = false;
            hitList.Clear();
            hitTextBlock.Clear();
            var hitPoint = e.GetPosition(VisualModel);
            hitArea = new EllipseGeometry(hitPoint, 1.0, 1.0);
            VisualTreeHelper.HitTest(VisualModel, null, HitTestCallBack,
                new GeometryHitTestParameters(hitArea));

            var sb = new StringBuilder();
            foreach (var item in hitList)
            {
                MyPopup.IsOpen = true;

                if (model.Elements.TryGetValue(item.Name, out var element))
                {
                    sb.Append("\nElement\t= " + element.ElementId);
                    foreach (var id in element.NodeIds)
                        if (model.Nodes.TryGetValue(id, out var knoten))
                            sb.Append("\nNode " + id + " ("
                                      + knoten.Coordinates[0].ToString("g2") + ";"
                                      + knoten.Coordinates[1].ToString("g2") + ")");

                    if (model.Material.TryGetValue(element.ElementMaterialId, out var material))
                        sb.Append("\nLeitfähigkeit = " + material.MaterialValues[0].ToString("g3"));
                }

                foreach (var last in model.ElementLoads.Where(last => last.Value.ElementId == item.Name))
                    sb.Append("\nElementload = " + last.Value.Intensity[0].ToString("g2") + ", "
                              + last.Value.Intensity[1].ToString("g2") + ", "
                              + last.Value.Intensity[2].ToString("g2") + "\n");

                MyPopupText.Text = sb.ToString();
            }

            foreach (var item in hitTextBlock.Where(item => item != null))
            {
                MyPopup.IsOpen = true;
                if (item.Name == "Element")
                {
                    if (model.Elements.TryGetValue(item.Text, out var element))
                    {
                        sb.Append("Element\t= " + element.ElementId);
                        foreach (var id in element.NodeIds)
                            if (model.Nodes.TryGetValue(id, out var knoten))
                                sb.Append("\nNode " + id + " ("
                                          + knoten.Coordinates[0].ToString("g2") + ";"
                                          + knoten.Coordinates[1].ToString("g2") + ")");
                        if (model.Material.TryGetValue(element.ElementMaterialId, out var material))
                            sb.Append("\nConductivity = " + material.MaterialValues[0].ToString("g3") + "\n");
                    }

                    foreach (var last in model.ElementLoads.Where(last => last.Value.ElementId == item.Text))
                        sb.Append("\nElementload = " + last.Value.Intensity[0].ToString("g2") + ", "
                                  + last.Value.Intensity[1].ToString("g2") + ", "
                                  + last.Value.Intensity[2].ToString("g2") + "\n");

                    MyPopupText.Text = sb.ToString();
                    break;
                }

                if (item.Name == "Node")
                {
                    if (model.Nodes.TryGetValue(item.Text, out var knoten))
                        sb.Append("Node\t= " + knoten.Id + " ("
                                  + knoten.Coordinates[0].ToString("g2") + ";"
                                  + knoten.Coordinates[1].ToString("g2") + ")");
                    foreach (var last in model.Loads.Where(last => last.Value.NodeId == item.Text))
                        sb.Append("\nNodeload = " + last.Value.Intensity[0].ToString("g2") + "\n");
                    foreach (var rand in model.BoundaryConditions.Where(rand => rand.Value.NodeId == item.Text))
                        sb.Append("\npredefined Boundary Temperature = " + rand.Value.Prescribed[0].ToString("g2") + "\n");

                    MyPopupText.Text = sb.ToString();
                    break;
                }

                if (item.Name != "Support") continue;
                {
                    if (model.BoundaryConditions.TryGetValue(item.Uid, out _))
                    {
                        if (model.BoundaryConditions.TryGetValue(item.Uid, out var rand))
                            sb.Append("predefined Temperature " + rand.SupportId + " at Node " + rand.NodeId);

                        MyPopupText.Text = sb.ToString();
                    }

                    break;
                }
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