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

namespace FE_Analysis.Elasticity.ModelDataShow
{
    public partial class ElasticityModelVisualize
    {
        private readonly Presentation presentation;
        private readonly List<Shape> hitList = new List<Shape>();
        private readonly List<TextBlock> hitTextBlock = new List<TextBlock>();
        private readonly FeModel model;
        private EllipseGeometry hitArea;
        private bool loadsOn = true, supportsOn = true, nodeTextsOn = true, elementTextsOn = true;

        public ElasticityModelVisualize(FeModel feModel)
        {
            Language = XmlLanguage.GetLanguage("us-US");
            model = feModel;
            InitializeComponent();
            Show();
            presentation = new Presentation(feModel, VisualErgebnisse);
            presentation.ElementsDraw();

            // with Element and Node Ids
            presentation.NodalTexts();
            presentation.ElementTexts();
            presentation.LoadsDraw();
            presentation.SupportDraw();
        }

        private void BtnNodeIDs_Click(object sender, RoutedEventArgs e)
        {
            if (!nodeTextsOn)
            {
                presentation.NodalTexts();
                nodeTextsOn = true;
            }
            else
            {
                foreach (TextBlock id in presentation.NodeIDs) VisualErgebnisse.Children.Remove(id);
                nodeTextsOn = false;
            }
        }

        private void BtnElementIDs_Click(object sender, RoutedEventArgs e)
        {
            if (!elementTextsOn)
            {
                presentation.ElementTexts();
                elementTextsOn = true;
            }
            else
            {
                foreach (TextBlock id in presentation.ElementIDs) VisualErgebnisse.Children.Remove(id);
                elementTextsOn = false;
            }
        }

        private void BtnLoads_Click(object sender, RoutedEventArgs e)
        {
            if (!loadsOn)
            {
                presentation.LoadsDraw();
                loadsOn = true;
            }
            else
            {
                foreach (Shape lasten in presentation.LoadVectors) VisualErgebnisse.Children.Remove(lasten);
                loadsOn = false;
            }
        }

        private void BtnSupports_Click(object sender, RoutedEventArgs e)
        {
            if (!supportsOn)
            {
                presentation.SupportDraw();
                supportsOn = true;
            }
            else
            {
                foreach (Shape path in presentation.SupportRepresentation) VisualErgebnisse.Children.Remove(path);
                supportsOn = false;
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            hitList.Clear();
            hitTextBlock.Clear();
            var hitPoint = e.GetPosition(VisualErgebnisse);
            hitArea = new EllipseGeometry(hitPoint, 1.0, 1.0);
            VisualTreeHelper.HitTest(VisualErgebnisse, null, HitTestCallBack,
                new GeometryHitTestParameters(hitArea));

            MyPopup.IsOpen = false;

            var sb = new StringBuilder();
            foreach (var item in hitList.Where(item => item != null))
            {
                MyPopup.IsOpen = true;

                switch (item)
                {
                    case Shape path:
                        {
                            if (path.Name == null) continue;
                            if (model.Elements.TryGetValue(path.Name, out var element))
                            {
                                sb.Append("\nElement\t= " + element.ElementId);

                                foreach (var id in element.NodeIds)
                                    if (model.Nodes.TryGetValue(id, out var knoten))
                                    {
                                        sb.Append("\nNodes " + id + "\t= " + knoten.Coordinates[0]);
                                        for (var k = 1; k < knoten.Coordinates.Length; k++)
                                            sb.Append(", " + knoten.Coordinates[k]);
                                    }

                                if (model.Material.TryGetValue(element.ElementMaterialId, out var material))
                                {
                                    sb.Append("\nMaterial\t= " + element.ElementMaterialId + "\t= " +
                                              material.MaterialValues[0]);

                                    for (var i = 1; i < material.MaterialValues.Length; i++)
                                        sb.Append(", " + material.MaterialValues[i].ToString("g3"));
                                }
                            }

                            if (model.Loads.TryGetValue(path.Name, out var nodeLoad))
                            {
                                sb.Append("Last\t= " + path.Name);
                                for (var i = 0; i < nodeLoad.Intensity.Length; i++)
                                    sb.Append("\nload value " + i + "\t= " + nodeLoad.Intensity[i]);
                            }

                            sb.Append("\n");
                        }
                        break;
                }
            }

            foreach (var item in hitTextBlock.Where(item => item != null))
            {
                sb.Clear();
                MyPopup.IsOpen = true;

                if (model.Nodes.TryGetValue(item.Text, out var knoten))
                {
                    sb.Append("Node\t= " + knoten.Id);
                    for (var i = 0; i < knoten.Coordinates.Length; i++)
                        sb.Append("\nCoordinate " + i + "\t= " + knoten.Coordinates[i].ToString("g3"));
                }

                if (model.Elements.TryGetValue(item.Text, out var element))
                {
                    sb.Append("Element\t= " + element.ElementId);
                    for (var i = 0; i < element.NodeIds.Length; i++)
                        sb.Append("\nNode " + i + "\t= " + element.NodeIds[i]);
                    if (model.Material.TryGetValue(element.ElementMaterialId, out var material))
                        sb.Append("\nE-Modulus\t= " + material.MaterialValues[0].ToString("g3"));
                    if (model.CrossSection.TryGetValue(element.ElementCrossSectionId, out var crossSection))
                    {
                        sb.Append("\nArea\t= " + crossSection.CrossSectionValues[0]);
                        if (crossSection.CrossSectionValues.Length > 1)
                            sb.Append("\nIxx\t= " + crossSection.CrossSectionValues[1].ToString("g3"));
                    }
                }
            }

            MyPopupText.Text = sb.ToString();
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