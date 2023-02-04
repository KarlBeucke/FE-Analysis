using FE_Analysis.Heat_Transfer.Model_Data;
using FE_Analysis.Heat_Transfer.ModelDataRead;
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

namespace FE_Analysis.Heat_Transfer.ModelDataShow;

public partial class HeatDataVisualize
{
    private readonly FeModel model;
    public readonly Presentation presentation;
    private bool nodesOn = true, elementsOn = true, loadsOn = true;
    private bool boundaryConditionsOn = true;

    private readonly List<Shape> hitList = new();
    private readonly List<TextBlock> hitTextBlock = new();
    private EllipseGeometry hitArea;

    private NodeNew newNode;
    private Point midPoint;
    private bool isDragging;
    public bool isNode;
    private bool deleteFlag;
    private DialogueDeleteHeatObjects dialogDelete;
    public TimeIntegrationNew timeIntegrationNew;
    public HeatDataVisualize(FeModel feModel)
    {
        Language = XmlLanguage.GetLanguage("us-US");
        InitializeComponent();
        model = feModel;
        Show();
        VisualHeatModel.Background = Brushes.Transparent;
        VisualHeatModel.Children.Remove(Node);

        presentation = new Presentation(model, VisualHeatModel);
        presentation.AllElementsDraw();

        // with Node and Element Ids
        presentation.NodeIds();
        presentation.ElementIds();
        presentation.NodeLoadDraw();
        presentation.LineLoadDraw();
        presentation.ElementLoadDraw();
        presentation.BoundaryConditionsDraw();
    }

    private void OnBtnNodeIDs_Click(object sender, RoutedEventArgs e)
    {
        if (!nodesOn)
        {
            presentation.NodeIds();
            nodesOn = true;
        }
        else
        {
            foreach (var id in presentation.NodeIDs) VisualHeatModel.Children.Remove(id);
            nodesOn = false;
        }
    }
    private void OnBtnElementIDs_Click(object sender, RoutedEventArgs e)
    {
        if (!elementsOn)
        {
            presentation.ElementIds();
            elementsOn = true;
        }
        else
        {
            foreach (var id in presentation.ElementIDs) VisualHeatModel.Children.Remove(id);
            elementsOn = false;
        }
    }

    private void OnBtnLoads_Click(object sender, RoutedEventArgs e)
    {
        if (!loadsOn)
        {
            presentation.NodeLoadDraw();
            presentation.LineLoadDraw();
            presentation.ElementLoadDraw();
            loadsOn = true;
        }
        else
        {
            foreach (var loadNode in presentation.LoadNodes) VisualHeatModel.Children.Remove(loadNode);
            foreach (var loadLine in presentation.LoadLines) VisualHeatModel.Children.Remove(loadLine);
            foreach (var loadElement in presentation.LoadElements) VisualHeatModel.Children.Remove(loadElement);
            loadsOn = false;
        }
    }

    private void OnBtnBoundaryCondition_Click(object sender, RoutedEventArgs e)
    {
        if (!boundaryConditionsOn)
        {
            presentation.BoundaryConditionsDraw();
            boundaryConditionsOn = true;
        }
        else
        {
            foreach (var randbedingung in presentation.BoundaryNode) VisualHeatModel.Children.Remove(randbedingung);
            boundaryConditionsOn = false;
        }
    }

    private void Node_MouseDown(object sender, MouseButtonEventArgs e)
    {
        Node.CaptureMouse();
        isDragging = true;
    }
    private void Node_MouseMove(object sender, MouseEventArgs e)
    {
        if (!isDragging) return;
        var canvPosToWindow = VisualHeatModel.TransformToAncestor(this).Transform(new Point(0, 0));

        if (sender is not Ellipse node) return;
        var upperlimit = canvPosToWindow.Y + node.Height / 2;
        var lowerlimit = canvPosToWindow.Y + VisualHeatModel.ActualHeight - node.Height / 2;

        var leftlimit = canvPosToWindow.X + node.Width / 2;
        var rightlimit = canvPosToWindow.X + VisualHeatModel.ActualWidth - node.Width / 2;


        var absmouseXpos = e.GetPosition(this).X;
        var absmouseYpos = e.GetPosition(this).Y;

        if (!(absmouseXpos > leftlimit) || !(absmouseXpos < rightlimit)
                                        || !(absmouseYpos > upperlimit) || !(absmouseYpos < lowerlimit)) return;

        midPoint = new Point(e.GetPosition(VisualHeatModel).X, e.GetPosition(VisualHeatModel).Y);

        Canvas.SetLeft(node, midPoint.X - Node.Width / 2);
        Canvas.SetTop(node, midPoint.Y - Node.Height / 2);

        var coordinates = presentation.TransformScreenPoint(midPoint);
        newNode.X.Text = coordinates[0].ToString("N2", CultureInfo.CurrentCulture);
        newNode.Y.Text = coordinates[1].ToString("N2", CultureInfo.CurrentCulture);
    }
    private void Node_MouseUp(object sender, MouseButtonEventArgs e)
    {
        Node.ReleaseMouseCapture();
        isDragging = false;
    }

    private void OnBtnNodeNew_Click(object sender, RoutedEventArgs e)
    {
        newNode = new NodeNew(model);
        MainWindow.analysed = false;
    }

    private void MenuElementNew(object sender, RoutedEventArgs e)
    {
        _ = new ElementNew(model);
        MainWindow.analysed = false;
    }
    private void MenuMaterialNew(object sender, RoutedEventArgs e)
    {
        _ = new MaterialNew(model);
    }

    private void MenuNodeLoadNew(object sender, RoutedEventArgs e)
    {
        _ = new NodeLoadNew(model);
        MainWindow.analysed = false;
    }
    private void MenuLineLoadNew(object sender, RoutedEventArgs e)
    {
        _ = new LineLoadNew(model);
        MainWindow.analysed = false;
    }
    private void MenuElementLoadNew(object sender, RoutedEventArgs e)
    {
        _ = new ElementLoadNew(model);
        MainWindow.analysed = false;
    }

    private void MenuTimeNodeLoadNew(object sender, RoutedEventArgs e)
    {
        _ = new TimeNodeLoadNew(model);
        MainWindow.analysed = false;
    }
    private void MenuTimeElementLoadNew(object sender, RoutedEventArgs e)
    {
        _ = new TimeElementLoadNew(model);
        MainWindow.analysed = false;
    }

    private void MenuBoundaryConditionNew(object sender, RoutedEventArgs e)
    {
        _ = new BoundaryConditionNew(model);
        MainWindow.analysed = false;
    }
    private void MenuTimeInitialConditionNew(object sender, RoutedEventArgs e)
    {
        _ = new TimeInitialTemperatureNew(model);
        MainWindow.analysed = false;
    }
    private void MenuTimeBoundaryConditionNew(object sender, RoutedEventArgs e)
    {
        _ = new TimeBoundaryConditionNew(model);
        MainWindow.analysed = false;
    }

    private void OnBtnTimeInterationNew_Click(object sender, RoutedEventArgs e)
    {
        timeIntegrationNew = new TimeIntegrationNew(model);
    }

    private void OnBtnDelete_Click(object sender, RoutedEventArgs e)
    {
        deleteFlag = true;
        dialogDelete = new DialogueDeleteHeatObjects(deleteFlag);
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        MyPopup.IsOpen = false;
        hitList.Clear();
        hitTextBlock.Clear();
        var hitPoint = e.GetPosition(VisualHeatModel);
        hitArea = new EllipseGeometry(hitPoint, 1.0, 1.0);
        VisualTreeHelper.HitTest(VisualHeatModel, null, HitTestCallBack,
            new GeometryHitTestParameters(hitArea));

        var sb = new StringBuilder();
        // click on Shape representation
        foreach (var item in hitList)
        {
            if (isNode | item is not Path) return;
            if (item.Name == null) continue;

            // Elements
            if (model.Elements.TryGetValue(item.Name, out var element))
            {
                if (deleteFlag)
                {
                    if (MessageBox.Show("Element " + model.Elements + " wird gelöscht.", "Wärmemodell",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        model.Elements.Remove(element.ElementId);
                        MainWindow.heatModelVisual.Close();
                    }
                }

                MyPopup.IsOpen = true;
                sb.Append("Element " + element.ElementId + ": ");
                switch (element)
                {
                    case Element2D2:
                        {
                            sb.Append("Node 1 = " + element.NodeIds[0]);
                            sb.Append("Node 2 = " + element.NodeIds[1]);
                            if (model.Material.TryGetValue(element.ElementMaterialId, out var material))
                            {
                                sb.Append("\nConductivity = " + material.MaterialValues[0].ToString("g3"));
                            }

                            break;
                        }
                    case Element2D3:
                        {
                            sb.Append("\nNode 1 = " + element.NodeIds[0]);
                            sb.Append("\nNode 2 = " + element.NodeIds[1]);
                            sb.Append("\nNode 3 = " + element.NodeIds[2]);
                            if (model.Material.TryGetValue(element.ElementMaterialId, out var material))
                            {
                                sb.Append("\nConductivity = " + material.MaterialValues[0].ToString("g3"));
                            }

                            break;
                        }
                    case Element2D4:
                        {
                            sb.Append("Node 1 = " + element.NodeIds[0]);
                            sb.Append("Node 2 = " + element.NodeIds[1]);
                            sb.Append("Node 3 = " + element.NodeIds[2]);
                            sb.Append("Node 4 = " + element.NodeIds[3]);
                            if (model.Material.TryGetValue(element.ElementMaterialId, out var material))
                            {
                                sb.Append("\nConductivity = " + material.MaterialValues[0].ToString("g3"));
                            }

                            break;
                        }
                }
                sb.Append("\n");
            }

            // Loads
            // LineLoads
            else if (model.LineLoads.TryGetValue(item.Name, out var load))
            {
                if (deleteFlag)
                {
                    if (MessageBox.Show("Lineload " + load.LoadId + " will be deleted.", "Heat Transfer model",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        model.LineLoads.Remove(load.LoadId);
                        MainWindow.heatModelVisual.Close();
                    }

                }
                sb.Append("Lineload = " + load.LoadId);
                sb.Append("\nStartnode " + load.StartNodeId + "\t= " + load.Loadvalues[0].ToString("g2"));
                sb.Append("\nEndnode " + load.EndNodeId + "\t= " + load.Loadvalues[1].ToString("g2"));
                sb.Append("\n");
            }

            // Elementloads
            else if (model.ElementLoads.TryGetValue(item.Name, out var elementLoad))
            {
                if (deleteFlag)
                {
                    if (MessageBox.Show("Elementload " + elementLoad.LoadId + " will be deleted.", "Heat Transfer model",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        model.ElementLoads.Remove(elementLoad.LoadId);
                        MainWindow.heatModelVisual.Close();
                    }
                }
                model.Elements.TryGetValue(elementLoad.ElementId, out var loadElement);
                if (loadElement == null) continue;
                switch (elementLoad)
                {
                    case ElementLoad3:
                        sb.Append("Elementload = " + elementLoad.LoadId + "\n"
                                                     + loadElement.NodeIds[0] + " = " + elementLoad.Loadvalues[0].ToString("g2") +
                                                     ", "
                                                     + loadElement.NodeIds[1] + " = " + elementLoad.Loadvalues[1].ToString("g2") +
                                                     ", "
                                                     + loadElement.NodeIds[2] + " = " + elementLoad.Loadvalues[2].ToString("g2"));
                        sb.Append("\n");
                        break;
                    case ElementLoad4:
                        sb.Append("\nElementlast = " + elementLoad.LoadId + "\n"
                                                     + loadElement.NodeIds[0] + " = " + elementLoad.Loadvalues[0].ToString("g2") +
                                                     ", "
                                                     + loadElement.NodeIds[1] + " = " + elementLoad.Loadvalues[1].ToString("g2") +
                                                     ", "
                                                     + loadElement.NodeIds[2] + " = " + elementLoad.Loadvalues[2].ToString("g2") +
                                                     ", "
                                                     + loadElement.NodeIds[3] + " = " + elementLoad.Loadvalues[3].ToString("g2"));
                        sb.Append("\n");
                        break;
                }
            }

            // time dependent Elementloads
            else if (model.TimeDependentElementLoads.TryGetValue(item.Name, out var timeDependentElementLoad))
            {
                if (deleteFlag)
                {
                    if (MessageBox.Show("time dependent Elementload " + timeDependentElementLoad.LoadId + " will be deleted.", "Heat Transfer model",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        model.TimeDependentElementLoads.Remove(timeDependentElementLoad.LoadId);
                        MainWindow.heatModelVisual.Close();
                    }
                }

                model.Elements.TryGetValue(timeDependentElementLoad.ElementId, out var loadElement);
                if (loadElement == null) continue;

                sb.Append("time dependent Elementload = " + timeDependentElementLoad.LoadId + "\n"
                          + loadElement.NodeIds[0] + " = " + timeDependentElementLoad.P[0].ToString("g2") +
                          ", "
                          + loadElement.NodeIds[1] + " = " + timeDependentElementLoad.P[1].ToString("g2") +
                          ", "
                          + loadElement.NodeIds[2] + " = " + timeDependentElementLoad.P[2].ToString("g2"));
                sb.Append("\n");
            }
            sb.Append("\n");
            MyPopupText.Text = sb.ToString();
            dialogDelete?.Close();
        }

        // click on Node text --> properties of an existing Node may be edited interactively
        foreach (var item in hitTextBlock)
        {
            if (!model.Nodes.TryGetValue(item.Text, out var node)) continue;
            if (deleteFlag)
            {
                if (MessageBox.Show("Node " + node.Id + " will be deleted.", "Heat model",
                        MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                else
                {
                    model.Nodes.Remove(node.Id);
                    MainWindow.heatModelVisual.Close();
                }
                return;
            }
            if (item.Text != node.Id) _ = MessageBox.Show("Node Id may not be changed here", "Node ID");
            newNode = new NodeNew(model)
            {
                NodeId = { Text = item.Text },
                X = { Text = node.Coordinates[0].ToString("N2", CultureInfo.CurrentCulture) },
                Y = { Text = node.Coordinates[1].ToString("N2", CultureInfo.CurrentCulture) }
            };

            midPoint = new Point(node.Coordinates[0] * presentation.resolution + Presentation.BorderLeft,
                (-node.Coordinates[1] + presentation.maxY) * presentation.resolution + Presentation.BorderTop);
            Canvas.SetLeft(Node, midPoint.X - Node.Width / 2);
            Canvas.SetTop(Node, midPoint.Y - Node.Height / 2);
            VisualHeatModel.Children.Add(Node);
            isNode = true;
            MyPopup.IsOpen = false;
        }
        MyPopupText.Text = sb.ToString();

        // click on text representation - except for node texts which will be treated above
        if (isNode) return;
        foreach (var item in hitTextBlock.Where(item => item != null))
        {
            // Text is Element
            if (model.Elements.TryGetValue(item.Text, out var element))
            {
                if (deleteFlag)
                {
                    if (MessageBox.Show("Element " + element.ElementId + " will be deleted.", "Heat Transfer modell",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    {
                    }
                    else
                    {
                        model.Elements.Remove(element.ElementId);
                        MainWindow.heatModelVisual.Close();
                    }
                }

                switch (element)
                {
                    case Element2D2:
                        _ = new ElementNew(model)
                        {
                            Element2D2Check = { IsChecked = true },
                            ElementId = { Text = element.ElementId },
                            Node1Id = { Text = element.NodeIds[0] },
                            Node2Id = { Text = element.NodeIds[1] },
                            MaterialId = { Text = element.ElementMaterialId }
                        };
                        break;
                    case Element2D3:
                        _ = new ElementNew(model)
                        {
                            Element2D3Check = { IsChecked = true },
                            ElementId = { Text = element.ElementId },
                            Node1Id = { Text = element.NodeIds[0] },
                            Node2Id = { Text = element.NodeIds[1] },
                            Node3Id = { Text = element.NodeIds[2] },
                            MaterialId = { Text = element.ElementMaterialId }
                        };
                        break;
                    case Element2D4:
                        _ = new ElementNew(model)
                        {
                            Element2D4Check = { IsChecked = true },
                            ElementId = { Text = element.ElementId },
                            Node1Id = { Text = element.NodeIds[0] },
                            Node2Id = { Text = element.NodeIds[1] },
                            Node3Id = { Text = element.NodeIds[2] },
                            Node4Id = { Text = element.NodeIds[3] },
                            MaterialId = { Text = element.ElementMaterialId }
                        };
                        break;
                    case Element3D8:
                        _ = new ElementNew(model)
                        {
                            Element3D8Check = { IsChecked = true },
                            ElementId = { Text = element.ElementId },
                            Node1Id = { Text = element.NodeIds[0] },
                            Node2Id = { Text = element.NodeIds[1] },
                            Node3Id = { Text = element.NodeIds[2] },
                            Node4Id = { Text = element.NodeIds[3] },
                            Node5Id = { Text = element.NodeIds[4] },
                            Node6Id = { Text = element.NodeIds[5] },
                            Node7Id = { Text = element.NodeIds[6] },
                            Node8Id = { Text = element.NodeIds[7] },
                            MaterialId = { Text = element.ElementMaterialId }
                        };
                        break;
                }
            }

            // Text is Nodeload
            else if (model.Loads.TryGetValue(item.Uid, out var nodeload))
            {
                if (deleteFlag)
                {
                    if (MessageBox.Show("Nodeload " + nodeload.LoadId + " will be deleted.", "Heat Transfer model",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    {
                    }
                    else
                    {
                        model.BoundaryConditions.Remove(nodeload.LoadId);
                        MainWindow.heatModelVisual.Close();
                    }
                }

                _ = new BoundaryConditionNew(model)
                {
                    BoundaryConditionId = { Text = nodeload.LoadId },
                    NodeId = { Text = nodeload.NodeId },
                    Temperature = { Text = nodeload.Loadvalues[0].ToString("g3") }
                };
            }
            // Text is time dependent Nodeload
            else if (model.TimeDependentNodeLoads.TryGetValue(item.Uid, out var timeNodeload))
            {
                if (deleteFlag)
                {
                    if (MessageBox.Show("time dependent Nodeload " + timeNodeload.LoadId + " will be deleted.",
                            "Heat Transfer model",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    {
                    }
                    else
                    {
                        model.TimeDependentNodeLoads.Remove(timeNodeload.LoadId);
                        MainWindow.heatModelVisual.Close();
                    }
                }

                var timeNodalTemperature = new TimeNodeLoadNew(model)
                {
                    LoadId = { Text = timeNodeload.LoadId },
                    NodeId = { Text = timeNodeload.NodeId }
                };
                switch (timeNodeload.VariationType)
                {
                    case 0:
                        timeNodalTemperature.File.IsChecked = true;
                        break;
                    case 1:
                        timeNodalTemperature.Constant.Text = timeNodeload.ConstantTemperature.ToString("g3");
                        break;
                    case 2:
                        timeNodalTemperature.Constant.Text = timeNodeload.ConstantTemperature.ToString("g3");
                        timeNodalTemperature.Amplitude.Text = timeNodeload.Amplitude.ToString("g3");
                        timeNodalTemperature.Frequency.Text = timeNodeload.Frequency.ToString("g3");
                        timeNodalTemperature.Angle.Text = timeNodeload.PhaseAngle.ToString("g3");
                        break;
                    case 3:
                        var intervall = timeNodeload.Interval;
                        for (var i = 0; i < intervall.Length; i += 2)
                        {
                            sb.Append(intervall[i].ToString("N0"));
                            sb.Append(";");
                            sb.Append(intervall[i + 1].ToString("N0"));
                            sb.Append(" ");
                        }

                        timeNodalTemperature.Linear.Text = sb.ToString();
                        break;
                }
            }
            // Text is Elementload
            else if (model.ElementLoads.TryGetValue(item.Uid, out var elementLoad))
            {
                if (deleteFlag)
                {
                    if (MessageBox.Show("Elementload " + elementLoad.LoadId + " will be deleted.", "Heat Transfer model",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        model.ElementLoads.Remove(elementLoad.LoadId);
                        MainWindow.heatModelVisual.Close();
                    }
                }
                var elementload = new ElementLoadNew(model)
                {
                    ElementLoadId = { Text = elementLoad.LoadId },
                    ElementId = { Text = elementLoad.ElementId },
                    Node1 = { Text = elementLoad.Loadvalues[0].ToString(CultureInfo.CurrentCulture) },
                    Node2 = { Text = elementLoad.Loadvalues[1].ToString(CultureInfo.CurrentCulture) }
                };
                for (var i = 0; i < elementLoad.Loadvalues.Length; i++)
                {
                    switch (i)
                    {
                        case 3:
                            elementload.Node3.Text = elementLoad.Loadvalues[2].ToString(CultureInfo.CurrentCulture);
                            break;
                        case 4:
                            elementload.Node3.Text = elementLoad.Loadvalues[2].ToString(CultureInfo.CurrentCulture);
                            elementload.Node4.Text = elementLoad.Loadvalues[3].ToString(CultureInfo.CurrentCulture);
                            break;
                        case 5:
                            elementload.Node3.Text = elementLoad.Loadvalues[2].ToString(CultureInfo.CurrentCulture);
                            elementload.Node4.Text = elementLoad.Loadvalues[3].ToString(CultureInfo.CurrentCulture);
                            elementload.Node5.Text = elementLoad.Loadvalues[4].ToString(CultureInfo.CurrentCulture);
                            break;
                        case 6:
                            elementload.Node3.Text = elementLoad.Loadvalues[2].ToString(CultureInfo.CurrentCulture);
                            elementload.Node4.Text = elementLoad.Loadvalues[3].ToString(CultureInfo.CurrentCulture);
                            elementload.Node5.Text = elementLoad.Loadvalues[4].ToString(CultureInfo.CurrentCulture);
                            elementload.Node6.Text = elementLoad.Loadvalues[5].ToString(CultureInfo.CurrentCulture);
                            break;
                        case 7:
                            elementload.Node3.Text = elementLoad.Loadvalues[2].ToString(CultureInfo.CurrentCulture);
                            elementload.Node4.Text = elementLoad.Loadvalues[3].ToString(CultureInfo.CurrentCulture);
                            elementload.Node5.Text = elementLoad.Loadvalues[4].ToString(CultureInfo.CurrentCulture);
                            elementload.Node6.Text = elementLoad.Loadvalues[5].ToString(CultureInfo.CurrentCulture);
                            elementload.Node7.Text = elementLoad.Loadvalues[6].ToString(CultureInfo.CurrentCulture);
                            break;
                        case 8:
                            elementload.Node3.Text = elementLoad.Loadvalues[2].ToString(CultureInfo.CurrentCulture);
                            elementload.Node4.Text = elementLoad.Loadvalues[3].ToString(CultureInfo.CurrentCulture);
                            elementload.Node5.Text = elementLoad.Loadvalues[4].ToString(CultureInfo.CurrentCulture);
                            elementload.Node6.Text = elementLoad.Loadvalues[5].ToString(CultureInfo.CurrentCulture);
                            elementload.Node7.Text = elementLoad.Loadvalues[6].ToString(CultureInfo.CurrentCulture);
                            elementload.Node8.Text = elementLoad.Loadvalues[7].ToString(CultureInfo.CurrentCulture);
                            break;
                    }
                }
            }
            // Text is time dependent Elementload
            else if (model.TimeDependentElementLoads.TryGetValue(item.Uid, out var timeDependentElementLoad))
            {
                if (deleteFlag)
                {
                    if (MessageBox.Show("time dependent Elementload " + timeDependentElementLoad.LoadId + " will be deleted.", "Heat Transfer model",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        model.TimeDependentElementLoads.Remove(timeDependentElementLoad.LoadId);
                        MainWindow.heatModelVisual.Close();
                    }
                }
                var elementload = new TimeElementLoadNew(model)
                {
                    LoadId = { Text = timeDependentElementLoad.LoadId },
                    ElementId = { Text = timeDependentElementLoad.ElementId },
                    P0 = { Text = timeDependentElementLoad.P[0].ToString("G2") },
                    P1 = { Text = timeDependentElementLoad.P[1].ToString("G2") }
                };
                switch (timeDependentElementLoad.P.Length)
                {
                    case 3:
                        elementload.P2.Text = timeDependentElementLoad.P[2].ToString("G2");
                        break;
                    case 4:
                        elementload.P2.Text = timeDependentElementLoad.P[2].ToString("G2");
                        elementload.P3.Text = timeDependentElementLoad.P[3].ToString("G2");
                        break;
                }
            }

            // Text is BoundaryTemperature
            else if (model.BoundaryConditions.TryGetValue(item.Uid, out var boundaryCondition))
            {
                if (deleteFlag)
                {
                    if (MessageBox.Show("Boundary Condition " + boundaryCondition.SupportId + " will be deleted.", "Heat Transfer model",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        model.BoundaryConditions.Remove(boundaryCondition.SupportId);
                        MainWindow.heatModelVisual.Close();
                    }
                }
                _ = new BoundaryConditionNew(model)
                {
                    BoundaryConditionId = { Text = boundaryCondition.SupportId },
                    NodeId = { Text = boundaryCondition.NodeId },
                    Temperature = { Text = boundaryCondition.Prescribed[0].ToString("g3") }
                };
            }
            // Text is time dependent BoundaryTemperature
            else if (model.TimeDependentBoundaryConditions.TryGetValue(item.Uid, out var timeDependentBoundaryCondition))
            {
                if (deleteFlag)
                {
                    if (MessageBox.Show("time dependent BoundaryCondition " + timeDependentBoundaryCondition.SupportId + " will be deleted.", "Heat Transfer model",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        model.TimeDependentBoundaryConditions.Remove(timeDependentBoundaryCondition.SupportId);
                        MainWindow.heatModelVisual.Close();
                    }
                }

                var boundary = new TimeBoundaryConditionNew(model)
                {
                    SupportId = { Text = timeDependentBoundaryCondition.SupportId },
                    NodeId = { Text = timeDependentBoundaryCondition.NodeId },
                };
                switch (timeDependentBoundaryCondition.VariationType)
                {
                    case 0:
                        boundary.File.IsChecked = true;
                        break;
                    case 1:
                        boundary.Constant.Text = timeDependentBoundaryCondition.ConstantTemperature.ToString("g3");
                        break;
                    case 2:
                        boundary.Constant.Text = timeDependentBoundaryCondition.GetHashCode().ToString("g3");
                        boundary.Amplitude.Text = timeDependentBoundaryCondition.Amplitude.ToString("g3");
                        boundary.Frequency.Text = timeDependentBoundaryCondition.Frequency.ToString("g3");
                        boundary.Angle.Text = timeDependentBoundaryCondition.PhaseAngle.ToString("g3");
                        break;
                    case 3:
                        var intervall = timeDependentBoundaryCondition.Interval;
                        for (var i = 0; i < intervall.Length; i += 2)
                        {
                            sb.Append(intervall[i].ToString("N0"));
                            sb.Append(";");
                            sb.Append(intervall[i + 1].ToString("N0"));
                            sb.Append(" ");
                        }
                        boundary.Linear.Text = sb.ToString();
                        break;
                }
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