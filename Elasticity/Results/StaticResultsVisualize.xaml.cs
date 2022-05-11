using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FE_Analysis.Elasticity.Results
{
    public partial class StaticResultsVisualize
    {
        private readonly List<object> hitList = new List<object>();
        private readonly List<TextBlock> hitTextBlock = new List<TextBlock>();
        private readonly FeModel model;
        private readonly Presentation presentation;
        private bool elementTexteAn = true, knotenTexteAn = true, verformungenAn, spannungenAn, reaktionenAn;
        private EllipseGeometry hitArea;

        public StaticResultsVisualize(FeModel feModel)
        {
            Language = XmlLanguage.GetLanguage("de-DE");
            InitializeComponent();
            Show();

            model = feModel;
            presentation = new Presentation(feModel, VisualErgebnisse);

            // unverformte Geometrie
            presentation.ElementsDraw();

            // mit Element Ids
            presentation.ElementTexts();

            // mit Knoten Ids
            presentation.NodalTexts();
        }

        private void BtnVerformung_Click(object sender, RoutedEventArgs e)
        {
            if (!verformungenAn)
            {
                presentation.DeformedGeometry();
                verformungenAn = true;
            }
            else
            {
                foreach (Shape path in presentation.Deformations) VisualErgebnisse.Children.Remove(path);
                verformungenAn = false;
            }
        }

        private void BtnSpannungen_Click(object sender, RoutedEventArgs e)
        {
            if (!spannungenAn)
            {
                // zeichne Spannungsvektoren in Elementmitte
                presentation.StressesDraw();
                spannungenAn = true;
            }
            else
            {
                // entferne Spannungsvektoren
                foreach (Shape path in presentation.Stresses) VisualErgebnisse.Children.Remove(path);
                spannungenAn = false;
            }
        }

        private void Reaktionen_Click(object sender, RoutedEventArgs e)
        {
            if (!reaktionenAn)
            {
                // zeichne Reaktionen an Festhaltungen
                presentation.ReactionsDraw();
                reaktionenAn = true;
            }
            else
            {
                // entferne Spannungsvektoren
                foreach (Shape path in presentation.Reactions) VisualErgebnisse.Children.Remove(path);
                reaktionenAn = false;
            }
        }

        private void BtnElementIDs_Click(object sender, RoutedEventArgs e)
        {
            if (!elementTexteAn)
            {
                presentation.ElementTexts();
                elementTexteAn = true;
            }
            else
            {
                foreach (TextBlock id in presentation.ElementIDs) VisualErgebnisse.Children.Remove(id);
                elementTexteAn = false;
            }
        }

        private void BtnKnotenIDs_Click(object sender, RoutedEventArgs e)
        {
            if (!knotenTexteAn)
            {
                presentation.NodalTexts();
                knotenTexteAn = true;
            }
            else
            {
                foreach (TextBlock id in presentation.NodeIDs) VisualErgebnisse.Children.Remove(id);
                knotenTexteAn = false;
            }
        }

        private void BtnÜberhöhung_Click(object sender, RoutedEventArgs e)
        {
            presentation.scalingDisplacement = double.Parse(Überhöhung.Text);
            foreach (Shape path in presentation.Deformations) VisualErgebnisse.Children.Remove(path);
            verformungenAn = false;
            presentation.DeformedGeometry();
            verformungenAn = true;
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
            foreach (var item in hitList)
            {
                if (item == null) continue;
                MyPopup.IsOpen = true;

                switch (item)
                {
                    case Polygon polygon:
                        {
                            sb.Clear();
                            if (model.Elements.TryGetValue(polygon.Name, out var multiKnotenElement))
                            {
                                var element2D = (Abstract2D)multiKnotenElement;
                                var elementSpannungen = element2D.ComputeStateVector();
                                sb.Append("Element = " + element2D.ElementId);
                                sb.Append("\nElement center sig-xx\t= " + elementSpannungen[0].ToString("F2"));
                                sb.Append("\nElement center sig-yy\t= " + elementSpannungen[1].ToString("F2"));
                                sb.Append("\nElement center sig-xy\t= " + elementSpannungen[2].ToString("F2"));
                            }

                            MyPopupText.Text = sb.ToString();
                            break;
                        }
                }
            }

            foreach (var item in hitTextBlock)
            {
                if (item == null) continue;
                MyPopup.IsOpen = true;
                if (model.Nodes.TryGetValue(item.Text, out var knoten))
                {
                    sb.Append("Nodes = " + knoten.Id);
                    sb.Append("\nux\t= " + knoten.NodalDof[0].ToString("F4"));
                    sb.Append("\nuy\t= " + knoten.NodalDof[1].ToString("F4"));
                    sb.Append("\n");
                    if (knoten.Reactions != null)
                    {
                        sb.Append("\nRx\t= " + knoten.Reactions[0].ToString("F4"));
                        sb.Append("\nRy\t= " + knoten.Reactions[1].ToString("F4"));
                    }
                }
                else if (model.Elements.TryGetValue(item.Text, out var element))
                {
                    var element2D = (Abstract2D)element;
                    var elementSpannungen = element2D.ComputeStateVector();
                    sb.Append("Element = " + element2D.ElementId);
                    sb.Append("\nElement center sig-xx\t= " + elementSpannungen[0].ToString("F2"));
                    sb.Append("\nElement center sig-yy\t= " + elementSpannungen[1].ToString("F2"));
                    sb.Append("\nElement center sig-xy\t= " + elementSpannungen[2].ToString("F2"));
                }

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
                    hitList.Add(result.VisualHit as Shape);
                    hitTextBlock.Add(result.VisualHit as TextBlock);
                    return HitTestResultBehavior.Continue;
                case IntersectionDetail.FullyInside:
                    return HitTestResultBehavior.Continue;
                case IntersectionDetail.Intersects:
                    hitList.Add(result.VisualHit as Shape);
                    hitTextBlock.Add(result.VisualHit as TextBlock);
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

        //private void OnKeyDownHandler(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Return)
        //    {
        //        überhöhung = Überhöhung.Text;
        //    }
        //}
    }
}