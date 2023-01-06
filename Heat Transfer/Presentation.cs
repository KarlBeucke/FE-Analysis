using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using static System.Globalization.CultureInfo;
using static System.Windows.Controls.Canvas;
using static System.Windows.Media.Brushes;
using static System.Windows.Media.Color;

namespace FE_Analysis.Heat_Transfer;

public class Presentation
{
    private readonly FeModel model;
    private AbstractElement currentElement;
    private Node node;
    private readonly Canvas visual;
    public int timeStep;
    private double maxX;
    private double screenH, screenV;
    private readonly double maxScreenLength = 40;
    public double resolution;
    private double resolutionH, resolutionV;
    public double maxY;
    private double temp;
    private double minTemp = 100;
    private double maxTemp;
    public const int BorderTop = 60;
    public const int BorderLeft = 60;

    public Presentation(FeModel feModel, Canvas visual)
    {
        model = feModel;
        this.visual = visual;
        NodeIDs = new List<TextBlock>();
        ElementIDs = new List<TextBlock>();
        LoadNodes = new List<TextBlock>();
        LoadLines = new List<Shape>();
        LoadElements = new List<Shape>();
        NodalTemperatures = new List<TextBlock>();
        NodalGradients = new List<TextBlock>();
        TemperatureElements = new List<Shape>();
        HeatVectors = new List<Shape>();
        BoundaryNode = new List<TextBlock>();
        InitialConditions = new List<TextBlock>();
        EvaluateResolution();
    }

    public List<TextBlock> ElementIDs { get; }
    public List<TextBlock> NodeIDs { get; }
    public List<TextBlock> LoadNodes { get; }
    public List<Shape> LoadLines { get; }
    public List<Shape> LoadElements { get; }
    public List<TextBlock> NodalTemperatures { get; }
    public List<TextBlock> NodalGradients { get; }
    public List<Shape> TemperatureElements { get; }
    public List<Shape> HeatVectors { get; }
    public List<TextBlock> BoundaryNode { get; }
    public List<TextBlock> InitialConditions { get; }


    public void EvaluateResolution()
    {
        const int rand = 100;
        screenH = visual.ActualWidth;
        screenV = visual.ActualHeight;

        foreach (var item in model.Nodes)
        {
            node = item.Value;
            if (node.Coordinates[0] > maxX) maxX = node.Coordinates[0];
            if (node.Coordinates[1] > maxY) maxY = node.Coordinates[1];
        }

        if (screenH / maxX < screenV / maxY) resolution = (screenH - rand) / maxX;
        else resolution = (screenV - rand) / maxY;
    }

    public void NodeIds()
    {
        foreach (var item in model.Nodes)
        {
            var id = new TextBlock
            {
                Name = "Node",
                FontSize = 12,
                Text = item.Key,
                Foreground = Black
            };
            SetTop(id, (-item.Value.Coordinates[1] + maxY) * resolution + BorderTop);
            SetLeft(id, item.Value.Coordinates[0] * resolution + BorderLeft);
            visual.Children.Add(id);
            NodeIDs.Add(id);
        }
    }
    public Shape NodeIndicate(Node feNode, Brush color, double weight)
    {
        var point = TransformNode(feNode, resolution, maxY);

        var nodeIndicate = new GeometryGroup();
        nodeIndicate.Children.Add(
            new EllipseGeometry(new Point(point.X, point.Y), 20, 20)
        );
        Shape nodePath = new Path()
        {
            Stroke = color,
            StrokeThickness = weight,
            Data = nodeIndicate
        };
        SetLeft(nodePath, BorderLeft);
        SetTop(nodePath, BorderTop);
        visual.Children.Add(nodePath);
        return nodePath;
    }

    public void ElementIds()
    {
        foreach (var item in model.Elements)
        {
            var abstract2D = (Abstract2D)item.Value;
            var cg = abstract2D.ComputeCenterOfGravity();
            var id = new TextBlock
            {
                Name = "Element",
                FontSize = 12,
                Text = item.Key,
                Foreground = Blue
            };
            SetTop(id, (-cg.Y + maxY) * resolution + BorderTop);
            SetLeft(id, cg.X * resolution + BorderLeft);
            visual.Children.Add(id);
            ElementIDs.Add(id);
        }
    }

    public void AllElementsDraw()
    {
        foreach (var item in model.Elements)
        {
            ElementDraw(item.Value, Black, 2);
        }
    }
    private void ElementDraw(AbstractElement element, Brush color, double weight)
    {
        var pathGeometry = ElementOutlines(element);
        Shape elementPath = new Path()
        {
            Name = element.ElementId,
            Stroke = color,
            StrokeThickness = weight,
            Data = pathGeometry
        };
        SetLeft(elementPath, BorderLeft);
        SetTop(elementPath, BorderTop);
        visual.Children.Add(elementPath);
    }
    public Shape ElementFillDraw(AbstractElement element, Brush outlineColor,
        Color fillColor, double transparent, double weight)
    {
        var pathGeometry = ElementOutlines(element);
        var fill = new SolidColorBrush(fillColor) { Opacity = .2 };

        Shape elementPath = new Path()
        {
            Name = element.ElementId,
            Stroke = outlineColor,
            StrokeThickness = weight,
            Fill = fill,
            Data = pathGeometry
        };
        SetLeft(elementPath, BorderLeft);
        SetTop(elementPath, BorderTop);
        visual.Children.Add(elementPath);
        return elementPath;
    }
    private PathGeometry ElementOutlines(AbstractElement element)
    {
        var pathFigure = new PathFigure();
        var pathGeometry = new PathGeometry();

        if (model.Nodes.TryGetValue(element.NodeIds[0], out node)) { }
        var startPoint = TransformNode(node, resolution, maxY);
        pathFigure.StartPoint = startPoint;
        for (var i = 1; i < element.NodesPerElement; i++)
        {
            if (model.Nodes.TryGetValue(element.NodeIds[i], out node)) { }
            var nextPoint = TransformNode(node, resolution, maxY);
            pathFigure.Segments.Add(new LineSegment(nextPoint, true));
        }
        pathFigure.IsClosed = true;
        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }

    public void NodeLoadDraw()
    {
        const int loadOffset = 10;
        foreach (var item in model.Loads)
        {
            var knotenId = item.Value.NodeId;
            var lastWert = item.Value.Loadvalues[0];
            if (model.Nodes.TryGetValue(knotenId, out node)) { }

            var loadPoint = TransformNode(node, resolution, maxY);
            var nodeLoad = new TextBlock
            {
                FontSize = 12,
                Text = lastWert.ToString(CurrentCulture),
                Foreground = Red
            };
            SetTop(nodeLoad, loadPoint.Y + BorderTop + loadOffset);
            SetLeft(nodeLoad, loadPoint.X + BorderLeft);

            LoadNodes.Add(nodeLoad);
            visual.Children.Add(nodeLoad);
        }
        //time dependent Nodeloads
        foreach (var item in model.TimeDependentNodeLoads)
        {
            var nodeId = item.Value.NodeId;
            //var loadValue = item.Value.Loadvalues[0];
            if (model.Nodes.TryGetValue(nodeId, out node)) { }

            var loadPoint = TransformNode(node, resolution, maxY);
            var timeNodeLoad = new TextBlock
            {
                Name = "TimeNodeLoad",
                Uid = item.Key,
                FontSize = 12,
                Text = item.Key,
                Foreground = DarkViolet
            };
            SetTop(timeNodeLoad, loadPoint.Y + BorderTop + loadOffset);
            SetLeft(timeNodeLoad, loadPoint.X + BorderLeft);

            LoadNodes.Add(timeNodeLoad);
            visual.Children.Add(timeNodeLoad);
        }
    }

    public void LineLoadDraw()
    {
        foreach (var item in model.LineLoads)
        {
            var lineload = item.Value;
            var pathFigure = new PathFigure();
            var pathGeometry = new PathGeometry();

            if (model.Nodes.TryGetValue(lineload.StartNodeId, out node)) { }
            var startPoint = TransformNode(node, resolution, maxY);
            pathFigure.StartPoint = startPoint;
            if (model.Nodes.TryGetValue(lineload.StartNodeId, out node)) { }
            var endPoint = TransformNode(node, resolution, maxY);
            pathFigure.StartPoint = startPoint;
            pathFigure.Segments.Add(new LineSegment(endPoint, true));

            pathGeometry.Figures.Add(pathFigure);

            Shape linePath = new Path()
            {
                Name = lineload.LoadId,
                Stroke = Red,
                StrokeThickness = 5,
                Data = pathGeometry
            };
            SetLeft(linePath, BorderLeft);
            SetTop(linePath, BorderTop);
            visual.Children.Add(linePath);
            LoadLines.Add(linePath);
        }
    }

    public void ElementLoadDraw()
    {
        foreach (var item in model.ElementLoads)
        {
            if (model.Elements.TryGetValue(item.Value.ElementId, out var element)) { }
            var pathGeometry = ElementOutlines(element);
            var fill = new SolidColorBrush(Colors.Red) { Opacity = .2 };

            Shape elementPath = new Path()
            {
                Name = item.Key,
                Stroke = Black,
                StrokeThickness = 2,
                Fill = fill,
                Data = pathGeometry
            };
            SetLeft(elementPath, BorderLeft);
            SetTop(elementPath, BorderTop);
            visual.Children.Add(elementPath);

            var abstract2D = (Abstract2D)element;
            const int loadOffset = -15;
            if (abstract2D != null)
            {
                var cg = abstract2D.ComputeCenterOfGravity();
                var id = new TextBlock
                {
                    Name = "Load",
                    Uid = item.Value.LoadId,
                    FontSize = 12,
                    Text = item.Key,
                    Foreground = Red
                };
                SetTop(id, (-cg.Y + maxY) * resolution + BorderTop + loadOffset);
                SetLeft(id, cg.X * resolution + BorderLeft);
                visual.Children.Add(id);
                LoadNodes.Add(id);
            }

            LoadElements.Add(elementPath);
        }
        // time dependent element loads
        foreach (var item in model.TimeDependentElementLoads)
        {
            if (model.Elements.TryGetValue(item.Value.ElementId, out var element)) { }
            var pathGeometry = ElementOutlines(element);
            var fill = new SolidColorBrush(Colors.Violet) { Opacity = .2 };

            Shape elementPath = new Path()
            {
                Name = item.Key,
                Stroke = Black,
                StrokeThickness = 2,
                Fill = fill,
                Data = pathGeometry
            };
            SetLeft(elementPath, BorderLeft);
            SetTop(elementPath, BorderTop);
            visual.Children.Add(elementPath);

            var abstract2D = (Abstract2D)element;
            const int loadOffset = -15;
            if (abstract2D != null)
            {
                var cg = abstract2D.ComputeCenterOfGravity();
                var id = new TextBlock
                {
                    Name = "TimeElementLoad",
                    Uid = item.Value.LoadId,
                    FontSize = 12,
                    Text = item.Key,
                    Foreground = DarkViolet
                };
                SetTop(id, (-cg.Y + maxY) * resolution + BorderTop + loadOffset);
                SetLeft(id, cg.X * resolution + BorderLeft);
                visual.Children.Add(id);
                LoadNodes.Add(id);
            }

            LoadElements.Add(elementPath);
        }
    }

    public void BoundaryConditionsDraw()
    {
        const int boundaryOffset = 15;
        // draw value of each boundary condition as text at boundary Node
        foreach (var item in model.BoundaryConditions)
        {
            var nodeId = item.Value.NodeId;
            if (model.Nodes.TryGetValue(nodeId, out node)) { }
            var displayNode = TransformNode(node, resolution, maxY);

            var boundaryValue = item.Value.Prescribed[0];
            var boundaryCondition = new TextBlock
            {
                Name = "Support",
                Uid = item.Value.SupportId,
                FontSize = 12,
                Text = boundaryValue.ToString("N2"),
                Foreground = DarkOliveGreen,
                Background = LightGreen
            };
            BoundaryNode.Add(boundaryCondition);
            SetTop(boundaryCondition, displayNode.Y + BorderTop + boundaryOffset);
            SetLeft(boundaryCondition, displayNode.X + BorderLeft);
            visual.Children.Add(boundaryCondition);
        }

        // time dependent boundary condition
        foreach (var item in model.TimeDependentBoundaryConditions)
        {
            var nodeId = item.Value.NodeId;
            if (model.Nodes.TryGetValue(nodeId, out node)) { }
            var displayNode = TransformNode(node, resolution, maxY);

            var boundaryCondition = new TextBlock
            {
                Name = "TimeBoundaryCondition",
                Uid = item.Value.SupportId,
                FontSize = 12,
                Text = item.Value.SupportId,
                Foreground = DarkGreen,
                Background = LawnGreen
            };
            BoundaryNode.Add(boundaryCondition);
            SetTop(boundaryCondition, displayNode.Y + BorderTop + 15);
            SetLeft(boundaryCondition, displayNode.X + BorderLeft);
            visual.Children.Add(boundaryCondition);
        }
    }

    public void NodalTemperaturesDraw()
    {
        foreach (var item in model.Nodes)
        {
            node = item.Value;
            var temperature = node.NodalDof[0].ToString("N2");
            temp = node.NodalDof[0];
            if (temp > maxTemp) maxTemp = temp;
            if (temp < minTemp) minTemp = temp;
            var displayNode = TransformNode(node, resolution, maxY);

            var id = new TextBlock
            {
                Name = item.Key,
                FontSize = 12,
                Background = LightGray,
                FontWeight = FontWeights.Bold,
                Text = temperature
            };
            NodalTemperatures.Add(id);
            SetTop(id, displayNode.Y + BorderTop);
            SetLeft(id, displayNode.X + BorderLeft);
            visual.Children.Add(id);
        }
    }

    public void InitialConditionsDraw()
    {
        const int boundaryOffset = 15;
        // draw value of each Initial Condition as text at node

        foreach (var item in model.Timeintegration.InitialConditions)
        {
            var nodalValues = (NodalValues)item;
            var nodeId = nodalValues.NodeId;
            if (nodeId == "all") continue;

            if (model.Nodes.TryGetValue(nodeId, out node)) { }

            var displayNode = TransformNode(node, resolution, maxY);

            var initialCondition = new TextBlock
            {
                Name = "InitialCondition",
                //Uid = "",
                FontSize = 12,
                Text = nodalValues.Values[0].ToString("N2"),
                Foreground = Black,
                Background = Turquoise
            };
            SetTop(initialCondition, displayNode.Y + BorderTop + boundaryOffset);
            SetLeft(initialCondition, displayNode.X + BorderLeft);
            visual.Children.Add(initialCondition);
        }
    }
    public void InitialConditionsDraw(string nodeId, double nodalValue, string start)
    {
        const int boundaryOffset = 15;
        // draws value of an InitialCondition as text at Node

        if (model.Nodes.TryGetValue(nodeId, out node)) { }
        var dispayNode = TransformNode(node, resolution, maxY);

        var initialCondition = new TextBlock
        {
            Name = "InitialCondition",
            Uid = start,
            FontSize = 12,
            Text = nodalValue.ToString("N2"),
            Foreground = Black,
            Background = Turquoise
        };
        SetTop(initialCondition, dispayNode.Y + BorderTop + boundaryOffset);
        SetLeft(initialCondition, dispayNode.X + BorderLeft);
        visual.Children.Add(initialCondition);
        InitialConditions.Add(initialCondition);
    }

    public void AnfangsbedingungenEntfernen()
    {
        foreach (var item in InitialConditions) visual.Children.Remove(item);
        InitialConditions.Clear();
    }

    public void ElementTemperaturesDraw()
    {
        foreach (var path in TemperatureElements)
        {
            visual.Children.Remove(path);
        }
        TemperatureElements.Clear();
        foreach (var item in model.Nodes)
        {
            node = item.Value;
            temp = node.NodalDof[0];
            if (temp > maxTemp) maxTemp = temp;
            if (temp < minTemp) minTemp = temp;
        }

        foreach (var item in model.Elements)
        {
            currentElement = item.Value;
            var pathGeometry = ElementOutlines((Abstract2D)currentElement);
            double elementTemperature = 0;
            for (var i = 0; i < currentElement.NodesPerElement; i++)
            {
                if (model.Nodes.TryGetValue(currentElement.NodeIds[i], out node))
                {
                    elementTemperature += node.NodalDof[0];
                }
            }
            elementTemperature /= currentElement.NodesPerElement;

            var intens = (byte)(255 * (elementTemperature - minTemp) / (maxTemp - minTemp));
            var rot = FromArgb(intens, 255, 0, 0);
            var myBrush = new SolidColorBrush(rot);


            Shape path = new Path
            {
                Stroke = Blue,
                StrokeThickness = 1,
                Name = currentElement.ElementId,
                Opacity = 0.5,
                Fill = myBrush,
                Data = pathGeometry
            };
            TemperatureElements.Add(path);
            // setz oben/links Position zum Zeichnen auf dem Canvas
            SetLeft(path, BorderLeft);
            SetTop(path, BorderTop);
            // zeichne Shape
            visual.Children.Add(path);
        }
    }

    public void NodalHeatFlowDraw(int index)
    {
        foreach (var item in model.Nodes)
        {
            node = item.Value;
            var gradient = node.NodalDerivatives[0][index].ToString("N2");
            temp = node.NodalDof[0];
            if (temp > maxTemp) maxTemp = temp;
            if (temp < minTemp) minTemp = temp;
            var fensterKnoten = TransformNode(node, resolution, maxY);

            var id = new TextBlock
            {
                FontSize = 12,
                Background = LightBlue,
                FontWeight = FontWeights.Bold,
                Text = gradient
            };
            NodalGradients.Add(id);
            SetTop(id, fensterKnoten.Y + BorderTop + 15);
            SetLeft(id, fensterKnoten.X + BorderLeft);
            visual.Children.Add(id);
        }
    }

    public void HeatFlowVectorsDraw()
    {
        foreach (var path in HeatVectors)
        {
            visual.Children.Remove(path);
        }
        HeatVectors.Clear();
        double maxVektor = 0;
        foreach (var abstract2D in model.Elements.Select(item => (Abstract2D)item.Value))
        {
            abstract2D.ElementState = abstract2D.ComputeElementState(0, 0);
            var vektor = Math.Sqrt(abstract2D.ElementState[0] * abstract2D.ElementState[0] +
                                   abstract2D.ElementState[1] * abstract2D.ElementState[1]);
            if (maxVektor < vektor) maxVektor = vektor;
        }

        var vectorScaling = maxScreenLength / maxVektor;

        foreach (var abstract2D in model.Elements.Select(item => (Abstract2D)item.Value))
        {
            abstract2D.ElementState = abstract2D.ComputeElementState(0, 0);
            var vectorLength = Math.Sqrt(abstract2D.ElementState[0] * abstract2D.ElementState[0] +
                                         abstract2D.ElementState[1] * abstract2D.ElementState[1]) * vectorScaling;
            var vectorAngle = Math.Atan2(abstract2D.ElementState[1], abstract2D.ElementState[0]) * 180 / Math.PI;
            // zeichne den resultierenden Vektor mit seinem Mittelpunkt im Elementschwerpunkt
            // füge am Endpunkt Pfeilspitzen an und füge Wärmeflusspfeil als pathFigure zur pathGeometry hinzu
            var pathGeometry = HeatFlowElementCenter(abstract2D, vectorLength);

            Shape path = new Path
            {
                Name = abstract2D.ElementId,
                Stroke = Black,
                StrokeThickness = 2,
                Data = pathGeometry
            };
            // rotiere Wärmeflusspfeil im Schwerpunkt um den Vektorwinkel
            var cg = abstract2D.ComputeCenterOfGravity();
            var rotateTransform = new RotateTransform(-vectorAngle)
            {
                CenterX = (int)(cg.X * resolution),
                CenterY = (int)((-cg.Y + maxY) * resolution)
            };
            path.RenderTransform = rotateTransform;
            // sammle alle Wärmeflusspfeile in der Liste Wärmevektoren, um deren Darstellung löschen zu können
            HeatVectors.Add(path);

            // setz oben/links Position zum Zeichnen auf dem Canvas
            SetLeft(path, BorderLeft);
            SetTop(path, BorderTop);
            // zeichne Shape
            visual.Children.Add(path);
        }
    }

    private PathGeometry HeatFlowElementCenter(AbstractElement abstraktElement, double length)
    {
        var abstrakt2D = (Abstract2D)abstraktElement;
        var pathFigure = new PathFigure();
        var pathGeometry = new PathGeometry();
        var cg = abstrakt2D.ComputeCenterOfGravity();
        int[] fensterKnoten = { (int)(cg.X * resolution), (int)((-cg.Y + maxY) * resolution) };
        var startPoint = new Point(fensterKnoten[0] - length / 2, fensterKnoten[1]);
        var endPoint = new Point(fensterKnoten[0] + length / 2, fensterKnoten[1]);
        pathFigure.StartPoint = startPoint;
        pathFigure.Segments.Add(new LineSegment(endPoint, true));
        pathFigure.Segments.Add(new LineSegment(new Point(endPoint.X - 3, endPoint.Y - 2), true));
        pathFigure.Segments.Add(new LineSegment(new Point(endPoint.X - 3, endPoint.Y + 2), true));
        pathFigure.Segments.Add(new LineSegment(new Point(endPoint.X, endPoint.Y), true));
        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }

    public void TimeHistoryDraw(double dt, double tmin, double tmax, double mY, double[] ordinates)
    {
        if (ordinates[0] > double.MaxValue) ordinates[0] = mY;
        var timeHistory = new Polyline
        {
            Stroke = Red,
            StrokeThickness = 2
        };
        var supportPoints = new PointCollection();
        var start = (int)Math.Round(tmin / dt);
        for (var i = 0; i < ordinates.Length - start; i++)
        {
            var point = new Point(dt * i * resolutionH, -ordinates[i + start] * resolutionV);
            supportPoints.Add(point);
        }

        timeHistory.Points = supportPoints;

        SetLeft(timeHistory, BorderLeft);
        SetTop(timeHistory, mY * resolutionV + BorderTop);
        // draw Shape
        visual.Children.Add(timeHistory);
    }

    public void CoordinateSystem(double tmin, double tmax, double max, double min)
    {
        const int border = 20;
        const int maxOrdinateDisplay = 100;
        screenH = visual.ActualWidth;
        screenV = visual.ActualHeight;
        if (double.IsNaN(max)) { max = maxOrdinateDisplay; min = -max; }
        if ((max - min) < double.Epsilon) resolutionV = screenV - border;
        else resolutionV = (screenV - border) / (max - min);
        resolutionH = (screenH - border) / (tmax - tmin);

        var xAchse = new Line
        {
            Stroke = Black,
            X1 = 0,
            Y1 = max * resolutionV + BorderTop,
            X2 = (tmax - tmin) * resolutionH + BorderLeft,
            Y2 = max * resolutionV + BorderTop,
            StrokeThickness = 2
        };
        visual.Children.Add(xAchse);
        var yAchse = new Line
        {
            Stroke = Black,
            X1 = BorderLeft,
            Y1 = max * resolutionV - min * resolutionV + 2 * BorderTop,
            X2 = BorderLeft,
            Y2 = BorderTop,
            StrokeThickness = 2
        };
        visual.Children.Add(yAchse);
    }

    private static Point TransformNode(Node node, double resolution, double maxY)
    {
        return new Point(node.Coordinates[0] * resolution, (-node.Coordinates[1] + maxY) * resolution);
    }
    public double[] TransformScreenPoint(Point point)
    {
        var coordinates = new double[2];
        coordinates[0] = (point.X - Presentation.BorderLeft) / resolution;
        coordinates[1] = (-point.Y + Presentation.BorderTop) / resolution + maxY;
        return coordinates;
    }
}