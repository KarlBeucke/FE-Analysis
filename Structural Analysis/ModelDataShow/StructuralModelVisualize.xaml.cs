using FE_Analysis.Structural_Analysis.Model_Data;
using FE_Analysis.Structural_Analysis.ModelDataRead;
using FEALibrary.Model;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FE_Analysis.Structural_Analysis.ModelDataShow;

public partial class StructuralModelVisualize
{
    private readonly FeModel model;
    public readonly Presentation presentation;
    private bool loadsOn = true, supportsOn = true, nodeTextsOn = true, elementTextsOn = true;

    //all "Shapes" found are collected in a List
    private readonly List<Shape> hitList = new();
    //all "TextBlocks" found are collected in a List
    private readonly List<TextBlock> hitTextBlock = new();

    private EllipseGeometry hitArea;
    private NodeNew nodeNew;
    private Point midPoint;
    private bool isDragging;
    public bool isNode;
    private bool deleteFlag;
    private DialogDeleteStructuralObjects dialogDelete;
    public TimeIntegrationNew timeIntegrationNew;

    public StructuralModelVisualize(FeModel feModel)
    {
        Language = XmlLanguage.GetLanguage("us-US");
        InitializeComponent();
        VisualStructuralModel.Children.Remove(Node);
        Show();
        VisualStructuralModel.Background = Brushes.Transparent;
        model = feModel;
        presentation = new Presentation(feModel, VisualStructuralModel);
        presentation.UndeformedGeometry();

        // with Node and Element Ids
        presentation.NodeTexts();
        presentation.ElementTexts();
        // with load and support representation and Ids
        presentation.LoadsDraw();
        presentation.LoadsTexts();
        presentation.ConstraintsDraw();
        presentation.ConstraintsTexts();
    }

    private void OnBtnNodeIDs_Click(object sender, RoutedEventArgs e)
    {
        if (!nodeTextsOn)
        {
            presentation.NodeTexts();
            nodeTextsOn = true;
        }
        else
        {
            foreach (var id in presentation.NodeIDs.Cast<TextBlock>()) VisualStructuralModel.Children.Remove(id);
            nodeTextsOn = false;
        }
    }
    private void OnBtnElementIDs_Click(object sender, RoutedEventArgs e)
    {
        if (!elementTextsOn)
        {
            presentation.ElementTexts();
            elementTextsOn = true;
        }
        else
        {
            foreach (var id in presentation.ElementIDs.Cast<TextBlock>()) VisualStructuralModel.Children.Remove(id);
            elementTextsOn = false;
        }
    }
    private void OnBtnLoads_Click(object sender, RoutedEventArgs e)
    {
        if (!loadsOn)
        {
            presentation.LoadsDraw();
            presentation.LoadsTexts();
            loadsOn = true;
        }
        else
        {
            foreach (var lasten in presentation.LoadVectors.Cast<Shape>())
            {
                VisualStructuralModel.Children.Remove(lasten);
                foreach (var id in presentation.LoadIDs.Cast<TextBlock>()) VisualStructuralModel.Children.Remove(id);
            }
            loadsOn = false;
        }
    }
    private void OnBtnSupport_Click(object sender, RoutedEventArgs e)
    {
        if (!supportsOn)
        {
            presentation.ConstraintsDraw();
            presentation.ConstraintsTexts();
            supportsOn = true;
        }
        else
        {
            foreach (var path in presentation.SupportRepresentation.Cast<Shape>())
            {
                VisualStructuralModel.Children.Remove(path);
                foreach (var id in presentation.SupportIDs.Cast<TextBlock>()) VisualStructuralModel.Children.Remove(id);
            }
            supportsOn = false;
        }
    }

    private void OnBtnNodeNew_Click(object sender, RoutedEventArgs e)
    {
        nodeNew = new NodeNew(model) { Topmost = true, Owner = (Window)Parent };
        MainWindow.analysed = false;
    }

    private void MenuBeamElementNew(object sender, RoutedEventArgs e)
    {
        _ = new ElementNew(model) { Topmost = true, Owner = (Window)Parent };
        MainWindow.analysed = false;
    }
    private void MenuCrossSectionNew(object sender, RoutedEventArgs e)
    {
        _ = new CrossSectionNew(model) { Topmost = true, Owner = (Window)Parent };

    }
    private void MenuMaterialNew(object sender, RoutedEventArgs e)
    {
        _ = new MaterialNew(model) { Topmost = true, Owner = (Window)Parent };

    }

    private void MenuNodeLoadNew(object sender, RoutedEventArgs e)
    {
        _ = new NodeLoadNew(model) { Topmost = true, Owner = (Window)Parent };
        MainWindow.analysed = false;
    }
    private void MenuLineLoadNew(object sender, RoutedEventArgs e)
    {
        _ = new LineLoadNew(model) { Topmost = true, Owner = (Window)Parent };
        MainWindow.analysed = false;
    }
    private void MenuPointLoadNew(object sender, RoutedEventArgs e)
    {
        _ = new PointLoadNew(model) { Topmost = true, Owner = (Window)Parent };
        MainWindow.analysed = false;
    }

    private void OnBtnSupportNew_Click(object sender, RoutedEventArgs e)
    {
        _ = new SupportNew(model) { Topmost = true, Owner = (Window)Parent };
        MainWindow.analysed = false;
    }

    private void OnBtnTimeintegrationNew_Click(object sender, RoutedEventArgs e)
    {
        timeIntegrationNew = new TimeIntegrationNew(model);
    }

    private void OnBtnDelete_Click(object sender, RoutedEventArgs e)
    {
        deleteFlag = true;
        dialogDelete = new DialogDeleteStructuralObjects(deleteFlag);
    }

    private void Node_MouseDown(object sender, MouseButtonEventArgs e)
    {
        Node.CaptureMouse();
        isDragging = true;
    }
    private void Node_MouseMove(object sender, MouseEventArgs e)
    {
        if (!isDragging) return;
        var canvPosToWindow = VisualStructuralModel.TransformToAncestor(this).Transform(new Point(0, 0));

        if (sender is not Ellipse node) return;
        var upperlimit = canvPosToWindow.Y + node.Height / 2;
        var lowerlimit = canvPosToWindow.Y + VisualStructuralModel.ActualHeight - node.Height / 2;

        var leftlimit = canvPosToWindow.X + node.Width / 2;
        var rightlimit = canvPosToWindow.X + VisualStructuralModel.ActualWidth - node.Width / 2;


        var absmouseXpos = e.GetPosition(this).X;
        var absmouseYpos = e.GetPosition(this).Y;

        if (!(absmouseXpos > leftlimit) || !(absmouseXpos < rightlimit)
                                        || !(absmouseYpos > upperlimit) || !(absmouseYpos < lowerlimit)) return;

        midPoint = new Point(e.GetPosition(VisualStructuralModel).X, e.GetPosition(VisualStructuralModel).Y);

        Canvas.SetLeft(node, midPoint.X - Node.Width / 2);
        Canvas.SetTop(node, midPoint.Y - Node.Height / 2);

        var coordinates = presentation.TransformScreenPoint(midPoint);
        nodeNew.X.Text = coordinates[0].ToString("N2", CultureInfo.CurrentCulture);
        nodeNew.Y.Text = coordinates[1].ToString("N2", CultureInfo.CurrentCulture);
    }
    private void Node_MouseUp(object sender, MouseButtonEventArgs e)
    {
        Node.ReleaseMouseCapture();
        isDragging = false;
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        hitList.Clear();
        hitTextBlock.Clear();
        var hitPoint = e.GetPosition(VisualStructuralModel);
        hitArea = new EllipseGeometry(hitPoint, 1.0, 1.0);
        VisualTreeHelper.HitTest(VisualStructuralModel, null, HitTestCallBack,
            new GeometryHitTestParameters(hitArea));

        // click on Canvas neither text nor Shape --> new pilot node will be placed and mat be moved
        if (hitList.Count == 0 && hitTextBlock.Count == 0)
        {
            if (deleteFlag | nodeNew == null) return;
            midPoint = new Point(e.GetPosition(VisualStructuralModel).X, e.GetPosition(VisualStructuralModel).Y);
            Canvas.SetLeft(nodeNew, midPoint.X - Node.Width / 2);
            Canvas.SetTop(nodeNew, midPoint.Y - Node.Height / 2);
            VisualStructuralModel.Children.Add(Node);
            isNode = true;
            var coordinates = presentation.TransformScreenPoint(midPoint);
            nodeNew.X.Text = coordinates[0].ToString("N2", CultureInfo.CurrentCulture);
            nodeNew.Y.Text = coordinates[1].ToString("N2", CultureInfo.CurrentCulture);
            MyPopup.IsOpen = false;
            return;
        }

        var sb = new StringBuilder();
        // click on Shape representations
        foreach (var shape in hitList)
        {
            if (isNode | shape is not Path) return;
            if (shape.Name.Length == 0) continue;

            // Elements
            if (model.Elements.TryGetValue(shape.Name, out var element))
            {
                if (deleteFlag)
                {
                    if (MessageBox.Show("Element " + element.ElementId + " will be deleted", "Structural Model",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        model.Elements.Remove(element.ElementId);
                        MainWindow.structuralModel.Close();
                        dialogDelete.Close();
                    }
                    continue;
                }

                MyPopup.IsOpen = true;
                sb.Append("Element\t= " + element.ElementId);
                if (element is SpringElement)
                {
                    if (model.Elements.TryGetValue(element.ElementId, out var feder))
                    {
                        if (model.Material.TryGetValue(feder.ElementMaterialId, out var material))
                        {
                        }

                        for (var i = 0; i < 3; i++)
                        {
                            if (material != null)
                                sb.Append("\nspring stiffness " + i + "\t= " +
                                          material.MaterialValues[i].ToString("g3"));
                        }
                    }
                }
                else
                {
                    sb.Append("\nNode 1\t= " + element.NodeIds[0]);
                    sb.Append("\nNode 2\t= " + element.NodeIds[1]);
                    if (model.Material.TryGetValue(element.ElementMaterialId, out var material))
                    {
                        sb.Append("\nYoung's modulus\t= " + material.MaterialValues[0].ToString("g3"));
                    }

                    if (model.CrossSection.TryGetValue(element.ElementCrossSectionId, out var crossSection))
                    {
                        sb.Append("\nArea\t= " + crossSection.CrossSectionValues[0]);
                        if (crossSection.CrossSectionValues.Length > 1)
                            sb.Append("\nIxx\t= " + crossSection.CrossSectionValues[1].ToString("g3"));
                    }
                }

            }

            // Loads
            if (model.Loads.TryGetValue(shape.Name, out var nodeLoad))
            {
                if (deleteFlag)
                {
                    if (MessageBox.Show("Nodeload " + nodeLoad.LoadId + " will be deleted", "Structural Model",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        model.Loads.Remove(nodeLoad.LoadId);
                        MainWindow.structuralModel.Close();
                        dialogDelete.Close();
                    }
                    continue;
                }

                MyPopup.IsOpen = true;
                sb.Append("Load\t= " + shape.Name);
                for (var i = 0; i < nodeLoad.Loadvalues.Length; i++)
                {
                    sb.Append("\nLoad value " + i + "\t= " + nodeLoad.Loadvalues[i]);
                }

                sb.Append("\n");
            }
            else if (model.PointLoads.TryGetValue(shape.Name, out var abstractElementLoad))
            {
                if (deleteFlag)
                {
                    if (MessageBox.Show("Pointload " + abstractElementLoad.LoadId + " will be deleted", "Structural Model",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        model.PointLoads.Remove(abstractElementLoad.LoadId);
                        MainWindow.structuralModel.Close();
                        dialogDelete.Close();
                    }
                    continue;
                }
                var pointLoad = (PointLoad)abstractElementLoad;
                MyPopup.IsOpen = true;
                sb.Append("PointLoad\t= " + shape.Name);
                for (var i = 0; i < pointLoad.Loadvalues.Length; i++)
                {
                    sb.Append("\nPointLoad value " + i + "\t= " + pointLoad.Loadvalues[i]);
                }

                sb.Append("\nOffset on element\t= " + pointLoad.Offset);
                sb.Append("\n");
            }
            else if (model.ElementLoads.TryGetValue(shape.Name, out var elementLoad))
            {
                if (deleteFlag)
                {
                    if (MessageBox.Show("Elementload " + elementLoad.LoadId + " will be deleted", "Structural Model",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        model.ElementLoads.Remove(elementLoad.LoadId);
                        MainWindow.structuralModel.Close();
                        dialogDelete.Close();
                    }
                    continue;
                }

                MyPopup.IsOpen = true;
                sb.Append("ElementLoad\t= " + elementLoad.LoadId);
                for (var i = 0; i < elementLoad.Loadvalues.Length; i++)
                {
                    sb.Append("\nElementLoad value " + i + "\t= " + elementLoad.Loadvalues[i]);
                }

                sb.Append("\n");
            }

            // Support
            if (!model.BoundaryConditions.TryGetValue(shape.Name, out var support)) continue;
            if (deleteFlag)
            {
                if (MessageBox.Show("Support " + support.SupportId + " will be deleted", "Structural Model",
                        MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                else
                {
                    model.BoundaryConditions.Remove(support.SupportId);
                    MainWindow.structuralModel.Close();
                    dialogDelete.Close();
                }
                continue;
            }

            MyPopup.IsOpen = true;
            sb.Append("Support\t= " + support.SupportId);
            sb.Append("\nfixed\t= " + SupportType(support.Type));
            if (support.Type == 1 | support.Type == 3 | support.Type == 7)
                sb.Append("\nprescribed boundary value x \t= " + support.Prescribed[0]);
            if (support.Type == 2 | support.Type == 3 | support.Type == 7)
                sb.Append("\nprescribed boundary value y \t= " + support.Prescribed[1]);
            if (support.Type == 4 | support.Type == 7)
                sb.Append("\nprescribed boundary value r \t= " + support.Prescribed[2]);

            sb.Append("\n");
        }


        // click on Node text --> Properties of an existing Node may be modified interactively
        foreach (var item in hitTextBlock)
        {
            if (!model.Nodes.TryGetValue(item.Text, out var node)) continue;
            if (deleteFlag)
            {
                if (MessageBox.Show("Node " + node.Id + " will be deleted", "Structural Model",
                        MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                else
                {
                    model.Nodes.Remove(node.Id);
                    MainWindow.structuralModel.Close();
                    dialogDelete.Close();
                }
                continue;
            }

            if (item.Text != node.Id)
                _ = MessageBox.Show("Node Id cannot be modified here", "Node text");
            nodeNew = new NodeNew(model)
            {
                NodeId = { Text = item.Text },
                NumberDof = { Text = node.NumberOfNodalDof.ToString("N0", CultureInfo.CurrentCulture) },
                X = { Text = node.Coordinates[0].ToString("N2", CultureInfo.CurrentCulture) },
                Y = { Text = node.Coordinates[1].ToString("N2", CultureInfo.CurrentCulture) }
            };

            midPoint = new Point(node.Coordinates[0] * presentation.resolution + presentation.placementH,
                (-node.Coordinates[1] + presentation.maxY) * presentation.resolution + presentation.placementV);
            Canvas.SetLeft(Node, midPoint.X - Node.Width / 2);
            Canvas.SetTop(Node, midPoint.Y - Node.Height / 2);
            VisualStructuralModel.Children.Add(Node);
            isNode = true;
            MyPopup.IsOpen = false;
        }
        MyPopupText.Text = sb.ToString();

        // click on text representations - except Node texts (will be treated separately above)
        if (isNode) return;
        foreach (var item in hitTextBlock)
        {
            // text representation is an element
            if (model.Elements.TryGetValue(item.Text, out var element))
            {
                if (deleteFlag)
                {
                    if (MessageBox.Show("Element " + element.ElementId + " will be deleted", "Structural Model",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        model.Elements.Remove(element.ElementId);
                        MainWindow.structuralModel.Close();
                        dialogDelete.Close();
                    }
                    continue;
                }
                switch (element)
                {
                    case SpringElement:
                        {
                            _ = new ElementNew(model)
                            {
                                SpringCheck = { IsChecked = true },
                                ElementId = { Text = item.Text },
                                StartNodeId = { Text = element.NodeIds[0] },
                                MaterialId = { Text = element.ElementMaterialId }
                            };
                            break;
                        }
                    case Truss:
                        {
                            _ = new ElementNew(model)
                            {
                                TrussCheck = { IsChecked = true },
                                Hinge1 = { IsChecked = true },
                                Hinge2 = { IsChecked = true },
                                ElementId = { Text = item.Text },
                                StartNodeId = { Text = element.NodeIds[0] },
                                EndNodeId = { Text = element.NodeIds[1] },
                                MaterialId = { Text = element.ElementMaterialId },
                                CrossSectionId = { Text = element.ElementCrossSectionId }
                            };
                            break;
                        }
                    case Beam:
                        {
                            _ = new ElementNew(model)
                            {
                                BeamCheck = { IsChecked = true },
                                ElementId = { Text = item.Text },
                                StartNodeId = { Text = element.NodeIds[0] },
                                EndNodeId = { Text = element.NodeIds[1] },
                                MaterialId = { Text = element.ElementMaterialId },
                                CrossSectionId = { Text = element.ElementCrossSectionId },
                                Hinge1 = { IsChecked = false },
                                Hinge2 = { IsChecked = false }
                            };
                            break;
                        }
                    case BeamHinged:
                        {
                            var newElement = new ElementNew(model)
                            {
                                BeamCheck = { IsChecked = true },
                                ElementId = { Text = item.Text },
                                StartNodeId = { Text = element.NodeIds[0] },
                                EndNodeId = { Text = element.NodeIds[1] },
                                MaterialId = { Text = element.ElementMaterialId },
                                CrossSectionId = { Text = element.ElementCrossSectionId }
                            };
                            switch (element.Type)
                            {
                                case 1: { newElement.Hinge1.IsChecked = true; break; }
                                case 2: { newElement.Hinge2.IsChecked = true; break; }
                            }
                            break;
                        }
                }
            }
            // Text representation is a NodeLoad
            else if (model.Loads.TryGetValue(item.Text, out var nodeLoad))
            {
                if (deleteFlag)
                {
                    if (MessageBox.Show("Nodeload " + nodeLoad.LoadId + " will be deleted", "Structural Model",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        model.Loads.Remove(nodeLoad.LoadId);
                        MainWindow.structuralModel.Close();
                        dialogDelete.Close();
                    }
                    continue;
                }

                var load = new NodeLoadNew(model)
                {
                    LoadId = { Text = item.Text },
                    NodeId = { Text = nodeLoad.NodeId.ToString(CultureInfo.CurrentCulture) },
                    Px = { Text = nodeLoad.Loadvalues[0].ToString(CultureInfo.CurrentCulture) },
                    Py = { Text = nodeLoad.Loadvalues[1].ToString(CultureInfo.CurrentCulture) }
                };
                if (nodeLoad.Loadvalues.Length > 2)
                    load.M.Text = nodeLoad.Loadvalues[2].ToString(CultureInfo.CurrentCulture);

            }
            // Text representation is a LineLoad
            else if (model.LineLoads.TryGetValue(item.Text, out var lineLoad))
            {
                if (deleteFlag)
                {
                    if (MessageBox.Show("Lineload " + lineLoad.LoadId + " will be deleted", "Structural Model",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        model.LineLoads.Remove(lineLoad.LoadId);
                        MainWindow.structuralModel.Close();
                        dialogDelete.Close();
                    }
                    continue;
                }

                _ = new LineLoadNew(model)
                {
                    LoadId = { Text = item.Text },
                    ElementId = { Text = lineLoad.ElementId.ToString(CultureInfo.CurrentCulture) },
                    Pxa = { Text = lineLoad.Loadvalues[0].ToString(CultureInfo.CurrentCulture) },
                    Pya = { Text = lineLoad.Loadvalues[1].ToString(CultureInfo.CurrentCulture) },
                    Pxb = { Text = lineLoad.Loadvalues[2].ToString(CultureInfo.CurrentCulture) },
                    Pyb = { Text = lineLoad.Loadvalues[3].ToString(CultureInfo.CurrentCulture) },
                    InElement = { IsChecked = lineLoad.InElementCoordinateSystem }
                };
            }
            // Text representation is a PointLoad
            else if (model.PointLoads.TryGetValue(item.Text, out var pointload))
            {
                if (deleteFlag)
                {
                    if (MessageBox.Show("Pointload " + pointload.LoadId + " will be deleted", "Structural Model",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        model.PointLoads.Remove(pointload.LoadId);
                        MainWindow.structuralModel.Close();
                        dialogDelete.Close();
                    }
                }

                var pointLoad = (PointLoad)pointload;
                _ = new PointLoadNew(model)
                {
                    LoadId = { Text = item.Text },
                    ElementId = { Text = pointLoad.ElementId.ToString(CultureInfo.CurrentCulture) },
                    Px = { Text = pointLoad.Loadvalues[0].ToString(CultureInfo.CurrentCulture) },
                    Py = { Text = pointLoad.Loadvalues[1].ToString(CultureInfo.CurrentCulture) },
                    Offset = { Text = pointLoad.Offset.ToString(CultureInfo.CurrentCulture) },
                };
            }
            // Text representation is Support
            else if (model.BoundaryConditions.TryGetValue(item.Text, out var support))
            {
                if (deleteFlag)
                {
                    if (MessageBox.Show("Support " + support.SupportId + " will be deleted", "Structural Model",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        model.BoundaryConditions.Remove(support.SupportId);
                        MainWindow.structuralModel.Close();
                        dialogDelete.Close();
                    }
                    continue;
                }

                var supportNew = new SupportNew(model)
                {
                    SupportId = { Text = item.Text },
                    NodeId = { Text = support.NodeId.ToString(CultureInfo.CurrentCulture) },
                    Xfixed = { IsChecked = support.Type == 1 | support.Type == 3 | support.Type == 7 },
                    Yfixed = { IsChecked = support.Type == 2 | support.Type == 3 | support.Type == 7 },
                    Rfixed = { IsChecked = support.Type == 4 | support.Type == 7 }
                };
                if ((bool)supportNew.Xfixed.IsChecked) supportNew.PreX.Text = support.Prescribed[0].ToString("0.00");
                if ((bool)supportNew.Yfixed.IsChecked) supportNew.PreY.Text = support.Prescribed[1].ToString("0.00");
                if ((bool)supportNew.Rfixed.IsChecked) supportNew.PreRot.Text = support.Prescribed[2].ToString("0.00");
            }
        }
    }
    private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        MyPopup.IsOpen = false;
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

    private string SupportType(int type)
    {
        var supportType = type switch
        {
            1 => "x",
            2 => "y",
            3 => "xy",
            7 => "xyr",
            _ => ""
        };
        return supportType;
    }
}