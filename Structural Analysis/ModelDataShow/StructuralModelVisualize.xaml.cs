using FE_Analysis.Structural_Analysis.Model_Data;
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
using static System.Globalization.CultureInfo;

namespace FE_Analysis.Structural_Analysis.ModelDataShow
{
    public partial class StructuralModelVisualize
    {
        public readonly Presentation presentation;

        //all "Shapes" found are collected in a List
        private readonly List<Shape> hitList = new List<Shape>();
        private readonly List<TextBlock> hitTextBlock = new List<TextBlock>();
        private readonly FeModel model;
        private EllipseGeometry hitArea;
        private bool loadsOn = true, supportsOn = true, nodeTextsOn = true, elementTextsOn = true;

        public StructuralModelVisualize(FeModel feModel)
        {
            Language = XmlLanguage.GetLanguage("us-US");
            InitializeComponent();
            Show();

            model = feModel;
            presentation = new Presentation(feModel, VisualModel);
            presentation.UndeformedGeometry();

            // with Node and Element Ids
            presentation.NodeTexts();
            presentation.ElementTexts();
            presentation.LoadsDraw();
            presentation.ConstraintsDraw();
        }

        private void BtnNodeIds_Click(object sender, RoutedEventArgs e)
        {
            if (!nodeTextsOn)
            {
                presentation.NodeTexts();
                nodeTextsOn = true;
            }
            else
            {
                foreach (TextBlock id in presentation.NodeIDs) VisualModel.Children.Remove(id);
                nodeTextsOn = false;
            }
        }

        private void BtnElementIds_Click(object sender, RoutedEventArgs e)
        {
            if (!elementTextsOn)
            {
                presentation.ElementTexts();
                elementTextsOn = true;
            }
            else
            {
                foreach (TextBlock id in presentation.ElementIDs) VisualModel.Children.Remove(id);
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
                foreach (Shape lasten in presentation.LoadVectors) VisualModel.Children.Remove(lasten);
                loadsOn = false;
            }
        }

        private void BtnConstraints_Click(object sender, RoutedEventArgs e)
        {
            if (!supportsOn)
            {
                presentation.ConstraintsDraw();
                supportsOn = true;
            }
            else
            {
                foreach (Shape path in presentation.SupportRepresentation) VisualModel.Children.Remove(path);
                supportsOn = false;
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            hitList.Clear();
            hitTextBlock.Clear();
            var hitPoint = e.GetPosition(VisualModel);
            hitArea = new EllipseGeometry(hitPoint, 1.0, 1.0);
            VisualTreeHelper.HitTest(VisualModel, null, HitTestCallBack,
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
                                sb.Append("Element\t= " + element.ElementId);
                                if (element is SpringElement)
                                {
                                    if (model.Elements.TryGetValue(element.ElementId, out var feder))
                                    {
                                        if (model.Material.TryGetValue(feder.ElementMaterialId, out var material))
                                        {
                                        }

                                        for (var i = 0; i < 3; i++)
                                            if (material != null)
                                                sb.Append("\nSpring stiffnesses " + i + "\t= " +
                                                          material.MaterialValues[i].ToString("g3", InvariantCulture));
                                    }
                                }
                                else
                                {
                                    sb.Append("\nNode 1\t= " + element.NodeIds[0]);
                                    sb.Append("\nNode 2\t= " + element.NodeIds[1]);
                                    if (model.Material.TryGetValue(element.ElementMaterialId, out var material))
                                        sb.Append("\nE-Mod.\t= " + material.MaterialValues[0].ToString("g3", InvariantCulture));
                                    if (model.CrossSection.TryGetValue(element.ElementCrossSectionId, out var crossSection))
                                    {
                                        sb.Append("\nArea\t= " + crossSection.CrossSectionValues[0]);
                                        if (crossSection.CrossSectionValues.Length > 1)
                                            sb.Append("\nIxx\t= " + crossSection.CrossSectionValues[1].ToString("g3", InvariantCulture) + "\n");
                                    }
                                }
                            }

                            if (model.Loads.TryGetValue(path.Name, out var nodeLoad))
                            {
                                sb.Append("Load\t= " + path.Name);
                                for (var i = 0; i < nodeLoad.Intensity.Length; i++)
                                    sb.Append("\nLoad Value " + i + "\t= " + nodeLoad.Intensity[i]);
                            }

                            else if (model.PointLoads.TryGetValue(path.Name, out var pointLoad))
                            {
                                sb.Append("Point Load\t= " + path.Name);
                                for (var i = 0; i < pointLoad.Intensity.Length; i++)
                                    sb.Append("\nLoad Value " + i + "\t= " + pointLoad.Intensity[i]);
                            }

                            else if (model.ElementLoads.TryGetValue(path.Name, out var elementLoad))
                            {
                                sb.Append("Load\t= " + elementLoad.LoadId);
                                for (var i = 0; i < elementLoad.Intensity.Length; i++)
                                    sb.Append("\nLoad Value " + i + "\t= " + elementLoad.Intensity[i]);
                            }
                        }
                        break;
                }
            }

            foreach (var item in hitTextBlock.Where(item => item != null))
            {
                sb.Clear();
                MyPopup.IsOpen = true;

                if (model.Nodes.TryGetValue(item.Text, out var node))
                {
                    sb.Append("Node\t= " + node.Id);
                    sb.Append("\nx-Coordinate\t\t= " + node.Coordinates[0].ToString("g3", InvariantCulture));
                    sb.Append("\ny-Coordinate\t\t= " + node.Coordinates[1].ToString("g3", InvariantCulture));
                }

                if (model.Elements.TryGetValue(item.Text, out var element))
                {
                    if (element is SpringElement)
                    {
                        if (model.Material.TryGetValue(element.ElementMaterialId, out var material))
                        {
                            sb.Append("Spring stiffnesses\t= " + material.MaterialId);
                            sb.Append("\nSpring stiffness x\t\t= " + material.MaterialValues[0].ToString("g3", InvariantCulture));
                            sb.Append("\nSpring stiffness y\t\t= " + material.MaterialValues[1].ToString("g3", InvariantCulture));
                            sb.Append("\nRotational stiffness\t= " + material.MaterialValues[2].ToString("g3", InvariantCulture));
                        }
                    }
                    else
                    {
                        sb.Append("Element\t= " + element.ElementId);
                        sb.Append("\nNode 1\t= " + element.NodeIds[0]);
                        sb.Append("\nNode 2\t= " + element.NodeIds[1]);
                        if (model.Material.TryGetValue(element.ElementMaterialId, out var material))
                            sb.Append("\nE-Modulus\t= " + material.MaterialValues[0].ToString("g3", InvariantCulture));
                        if (model.CrossSection.TryGetValue(element.ElementCrossSectionId, out var crossSection))
                        {
                            sb.Append("\nArea\t= " + crossSection.CrossSectionValues[0]);
                            if (crossSection.CrossSectionValues.Length > 1)
                                sb.Append("\nIxx\t= " + crossSection.CrossSectionValues[1].ToString("g3", InvariantCulture));
                        }
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