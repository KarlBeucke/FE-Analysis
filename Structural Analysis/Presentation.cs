using FE_Analysis.Structural_Analysis.Model_Data;
using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using static System.Windows.Controls.Canvas;
using static System.Windows.Media.Brushes;
using static System.Windows.Media.Color;

namespace FE_Analysis.Structural_Analysis;

public class Presentation
{
    private readonly FeModel model;
    private Node node;
    public double resolution;
    private double resolutionH, resolutionV, loadResolution;
    public double maxY;
    private double minX, maxX, minY;
    public double placementV, placementH;
    private double screenH, screenV;
    public int scalingRotation = 1;
    public int scalingDisplacement = 1;
    private const int BorderTop = 60, BorderLeft = 30;
    private const int MaxAxialForceScreen = 30;
    private const int MaxShearForceScreen = 30;
    private const int MaxMomentScreen = 50;
    private readonly Canvas visualResults;
    public TextBlock maxMomentText;
    private Point placementText;


    public List<object> ElementIDs { get; }
    public List<object> NodeIDs { get; }
    public List<object> LoadIDs { get; }
    public List<object> SupportIDs { get; }
    public List<object> MaxTexts { get; }
    public List<object> Deformations { get; }
    public List<object> LoadVectors { get; }
    public List<object> SupportRepresentation { get; }
    public List<object> AxialForceList { get; }
    public List<object> ShearForceList { get; }
    public List<object> BendingMomentList { get; }

    public Presentation(FeModel feModel, Canvas visual)
    {
        model = feModel;
        visualResults = visual;
        ElementIDs = new List<object>();
        NodeIDs = new List<object>();
        LoadIDs = new List<object>();
        SupportIDs = new List<object>();
        Deformations = new List<object>();
        LoadVectors = new List<object>();
        SupportRepresentation = new List<object>();
        AxialForceList = new List<object>();
        ShearForceList = new List<object>();
        BendingMomentList = new List<object>();
        MaxTexts = new List<object>();
        EvaluateResolution();
    }
    public void EvaluateResolution()
    {
        screenH = visualResults.ActualWidth;
        screenV = visualResults.ActualHeight;

        var x = new List<double>();
        var y = new List<double>();
        foreach (var item in model.Nodes)
        {
            x.Add(item.Value.Coordinates[0]);
            y.Add(item.Value.Coordinates[1]);
        }
        maxX = x.Max(); minX = x.Min();
        maxY = y.Max(); minY = y.Min();

        // vertical model
        var delta = Math.Abs(maxX - minX);
        if (delta < 1)
        {
            resolutionH = screenH - 2 * BorderLeft;
            placementH = (int)(0.5 * screenH);
        }
        else
        {
            resolutionH = (screenH - 2 * BorderLeft) / delta;
            placementH = BorderLeft;
        }

        //plazierungH = randLinks;
        //if (maxX < double.Epsilon) placementH = screenH / 2;

        // horizontal model
        delta = Math.Abs(maxY - minY);
        if (delta < 1)
        {
            resolution = screenV - 2 * BorderTop;
            placementV = (int)(0.5 * screenV);
        }
        else
        {
            resolution = (screenV - 2 * BorderTop) / delta;
            placementV = BorderTop;
        }

        if (resolutionH < resolution) resolution = resolutionH;
    }

    public void UndeformedGeometry()
    {
        // Element contours will be added as Shapes (PathGeometry) with names
        // pathGeometry contains ONE specific element
        // all elements will be added to structure

        foreach (var item in model.Elements)
        {
            ElementDraw(item.Value, Black, 2);
        }

        // nodal hinges will be added as EllipseGeometry to GeometryGroup structure
        var structure = new GeometryGroup();
        foreach (var gelenk in from item in model.Nodes
                               select item.Value
                 into node
                               where node.NumberOfNodalDof == 2
                               select TransformNode(node, resolution, maxY)
                 into gelenkPunkt
                               select new EllipseGeometry(gelenkPunkt, 5, 5))
            structure.Children.Add(gelenk);

        // Knotengelenke werden gezeichnet
        Shape structurePath = new Path
        {
            Stroke = Black,
            StrokeThickness = 1,
            Data = structure
        };
        SetLeft(structurePath, placementH);
        SetTop(structurePath, placementV);
        visualResults.Children.Add(structurePath);
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
        SetLeft(nodePath, placementH);
        SetTop(nodePath, placementV);
        visualResults.Children.Add(nodePath);
        return nodePath;
    }

    public Shape ElementDraw(AbstractElement element, Brush color, double weight)
    {
        PathGeometry pathGeometry;

        switch (element)
        {
            // spring element
            case SpringElement _:
                {
                    pathGeometry = SpringElementDraw(element);
                    break;
                }

            case Truss _:
                {
                    // hinges will be added as half-circles at truss nodes
                    pathGeometry = TrussElementDraw(element);
                    break;
                }
            case Beam _:
                {
                    pathGeometry = BeamDraw(element);
                    break;
                }

            case BeamHinged _:
                {
                    // add hinge to start resp. end node of beam
                    pathGeometry = BeamHingedDraw(element);
                    break;
                }

            // elements with multiple nodes
            default:
                {
                    pathGeometry = MultiNodeElementDraw(element);
                    break;
                }
        }

        Shape elementPath = new Path
        {
            Name = element.ElementId,
            Stroke = color,
            StrokeThickness = weight,
            Data = pathGeometry
        };
        SetLeft(elementPath, placementH);
        SetTop(elementPath, placementV);
        visualResults.Children.Add(elementPath);
        return elementPath;
    }

    public void DeformedGeometry()
    {
        if (!MainWindow.analysed)
        {
            var analysis = new Analysis(model);
            analysis.ComputeSystemMatrix();
            analysis.ComputeSystemVector();
            analysis.SolveEquations();
            MainWindow.analysed = true;
        }

        var pathGeometry = new PathGeometry();

        IEnumerable<AbstractBeam> Beams()
        {
            foreach (var item in model.Elements)
                if (item.Value is AbstractBeam element)
                    yield return element;
        }

        foreach (var element in Beams())
        {
            var pathFigure = new PathFigure();
            Point start;
            Point end;
            double winkel;

            switch (element)
            {
                case Truss _:
                    {
                        if (model.Nodes.TryGetValue(element.NodeIds[0], out node)) { }

                        start = TransformDeformedNode(node, resolution, maxY);
                        pathFigure.StartPoint = start;

                        for (var i = 1; i < element.NodeIds.Length; i++)
                        {
                            if (model.Nodes.TryGetValue(element.NodeIds[i], out node))
                            {
                            }

                            end = TransformDeformedNode(node, resolution, maxY);
                            pathFigure.Segments.Add(new LineSegment(end, true));
                        }

                        pathGeometry.Figures.Add(pathFigure);
                        break;
                    }
                case Beam _:
                    {
                        element.ComputeStateVector();
                        if (model.Nodes.TryGetValue(element.NodeIds[0], out node))
                        {
                        }

                        start = TransformDeformedNode(node, resolution, maxY);
                        pathFigure.StartPoint = start;

                        for (var i = 1; i < element.NodeIds.Length; i++)
                        {
                            if (model.Nodes.TryGetValue(element.NodeIds[i], out node))
                            {
                            }

                            end = TransformDeformedNode(node, resolution, maxY);
                            var richtung = end - start;
                            richtung.Normalize();
                            winkel = -element.ElementDeformations[2] * 180 / Math.PI * scalingRotation;
                            richtung = RotateVectorScreen(richtung, winkel);
                            var control1 = start + richtung * element.length / 4 * resolution;

                            richtung = start - end;
                            richtung.Normalize();
                            winkel = -element.ElementDeformations[5] * 180 / Math.PI * scalingRotation;
                            richtung = RotateVectorScreen(richtung, winkel);
                            var control2 = end + richtung * element.length / 4 * resolution;

                            pathFigure.Segments.Add(new BezierSegment(control1, control2, end, true));
                        }

                        pathGeometry.Figures.Add(pathFigure);
                        break;
                    }
                case BeamHinged _:
                    {
                        if (model.Nodes.TryGetValue(element.NodeIds[0], out node))
                        {
                        }

                        start = TransformDeformedNode(node, resolution, maxY);
                        pathFigure.StartPoint = start;

                        var control = start;
                        for (var i = 1; i < element.NodeIds.Length; i++)
                        {
                            if (model.Nodes.TryGetValue(element.NodeIds[i], out node))
                            {
                            }

                            end = TransformDeformedNode(node, resolution, maxY);

                            switch (element.Type)
                            {
                                case 1:
                                    {
                                        var richtung = start - end;
                                        richtung.Normalize();
                                        winkel = element.ElementDeformations[4] * 180 / Math.PI * scalingRotation;
                                        richtung = RotateVectorScreen(richtung, winkel);
                                        control = end + richtung * element.length / 4 * resolution;
                                        break;
                                    }
                                case 2:
                                    {
                                        var richtung = end - start;
                                        richtung.Normalize();
                                        winkel = element.ElementDeformations[2] * 180 / Math.PI * scalingRotation;
                                        richtung = RotateVectorScreen(richtung, winkel);
                                        control = start + richtung * element.length / 4 * resolution;
                                        break;
                                    }
                            }

                            pathFigure.Segments.Add(new QuadraticBezierSegment(control, end, true));
                        }

                        pathGeometry.Figures.Add(pathFigure);
                        break;
                    }
                default:
                    {
                        if (model.Nodes.TryGetValue(element.NodeIds[0], out node))
                        {
                        }

                        start = TransformDeformedNode(node, resolution, maxY);
                        pathFigure.StartPoint = start;

                        for (var i = 1; i < element.NodeIds.Length; i++)
                        {
                            if (model.Nodes.TryGetValue(element.NodeIds[i], out node))
                            {
                            }

                            var next = TransformDeformedNode(node, resolution, maxY);
                            pathFigure.Segments.Add(new LineSegment(next, true));
                        }

                        pathFigure.IsClosed = true;
                        pathGeometry.Figures.Add(pathFigure);
                        break;
                    }
            }

            Shape path = new Path
            {
                Name = element.ElementId,
                Stroke = Red,
                StrokeThickness = 2,
                Data = pathGeometry
            };

            SetLeft(path, placementH);
            SetTop(path, placementV);
            visualResults.Children.Add(path);
            Deformations.Add(path);
        }
    }

    private PathGeometry SpringElementDraw(AbstractElement element)
    {
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();
        // placement point of spring element
        if (model.Nodes.TryGetValue(element.NodeIds[0], out node))
        {
        }

        var startPunkt = TransformNode(node, resolution, maxY);

        // set references for material values
        element.SetReferences(model);

        // x-Spring
        if (Math.Abs(element.ElementMaterial.MaterialValues[0]) > 0)
        {
            TranslationSpringDraw(pathFigure, startPunkt);
            pathGeometry.Figures.Add(pathFigure);
            pathGeometry.Transform = new RotateTransform(90, startPunkt.X, startPunkt.Y);
        }

        // y-Spring
        if (Math.Abs(element.ElementMaterial.MaterialValues[1]) > 0)
        {
            TranslationSpringDraw(pathFigure, startPunkt);
            pathGeometry.Figures.Add(pathFigure);
        }

        // Rotation Spring draw
        if (!(Math.Abs(element.ElementMaterial.MaterialValues[2]) > 0)) return pathGeometry;
        const int b = 10;
        pathFigure.StartPoint = startPunkt;
        var zielPunkt = new Point(startPunkt.X - b, startPunkt.Y - b);
        pathFigure.Segments.Add(
            new ArcSegment(zielPunkt, new Size(b, b - 3), 200, true, 0, true));
        zielPunkt = new Point(startPunkt.X + b, startPunkt.Y);
        pathFigure.Segments.Add(
            new ArcSegment(zielPunkt, new Size(b, b + 2), 190, false, 0, true));
        pathGeometry.Figures.Add(pathFigure);

        return pathGeometry;
    }

    private static void TranslationSpringDraw(PathFigure pathFigure, Point startPoint)
    {
        const double b = 6.0;
        const int h = 3;
        pathFigure.StartPoint = startPoint;
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPoint.X, startPoint.Y + 2 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPoint.X - b, startPoint.Y + 3 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPoint.X + b, startPoint.Y + 5 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPoint.X - b, startPoint.Y + 7 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPoint.X + b, startPoint.Y + 9 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPoint.X, startPoint.Y + 10 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPoint.X, startPoint.Y + 12 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPoint.X - b, startPoint.Y + 12 * h), false));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPoint.X + b, startPoint.Y + 12 * h), true));

        pathFigure.Segments.Add(
            new LineSegment(new Point(startPoint.X + b - h, startPoint.Y + 13 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPoint.X + b / 2, startPoint.Y + 12 * h), false));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPoint.X + b / 2 - h, startPoint.Y + 13 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPoint.X, startPoint.Y + 12 * h), false));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPoint.X - h, startPoint.Y + 13 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPoint.X - b / 2, startPoint.Y + 12 * h), false));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPoint.X - b / 2 - h, startPoint.Y + 13 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPoint.X - b, startPoint.Y + 12 * h), false));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPoint.X - b - h, startPoint.Y + 13 * h), true));
    }

    private PathGeometry TrussElementDraw(AbstractElement element)
    {
        if (model.Nodes.TryGetValue(element.NodeIds[0], out node)) { }
        var startPoint = TransformNode(node, resolution, maxY);
        if (model.Nodes.TryGetValue(element.NodeIds[1], out node)) { }
        var endPoint = TransformNode(node, resolution, maxY);

        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure { StartPoint = startPoint };
        pathFigure.Segments.Add(new LineSegment(endPoint, true));

        // hinge as half circle at start node of truss element
        var direction = endPoint - startPoint;
        var start = RotateVectorScreen(direction, 90);
        start.Normalize();
        var zielPunkt = startPoint + 5 * start;
        pathFigure.Segments.Add(new LineSegment(zielPunkt, false));
        var ziel = RotateVectorScreen(direction, -90);
        ziel.Normalize();
        zielPunkt = startPoint + 5 * ziel;
        // ArcSegment begins at last point of pathFigure
        // target point, size in x,y, opening angle isLargeArc, sweepDirection isStroked
        pathFigure.Segments.Add(new ArcSegment(zielPunkt, new Size(2.5, 2.5), 180, true, 0, true));
        pathFigure.Segments.Add(new LineSegment(startPoint, false));

        // hinge as half circle at end node of truss element
        direction = startPoint - endPoint;
        start = RotateVectorScreen(direction, -90);
        start.Normalize();
        zielPunkt = endPoint + 5 * start;
        pathFigure.Segments.Add(new LineSegment(zielPunkt, false));
        var end = RotateVectorScreen(direction, 90);
        end.Normalize();
        zielPunkt = endPoint + 5 * end;
        pathFigure.Segments.Add(new ArcSegment(zielPunkt, new Size(2.5, 2.5), 180, true, (SweepDirection)1, true));
        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }

    private PathGeometry BeamDraw(AbstractElement element)
    {
        if (model.Nodes.TryGetValue(element.NodeIds[0], out node))
        {
        }

        var startPoint = TransformNode(node, resolution, maxY);
        if (model.Nodes.TryGetValue(element.NodeIds[1], out node))
        {
        }

        var endPoint = TransformNode(node, resolution, maxY);

        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure { StartPoint = startPoint };
        pathFigure.Segments.Add(new LineSegment(endPoint, true));

        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }

    private PathGeometry BeamHingedDraw(AbstractElement element)
    {
        Vector direction, start;
        Point targetPoint;

        if (model.Nodes.TryGetValue(element.NodeIds[0], out node)) { }
        var startPoint = TransformNode(node, resolution, maxY);
        if (model.Nodes.TryGetValue(element.NodeIds[1], out node)) { }
        var endPoint = TransformNode(node, resolution, maxY);

        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure { StartPoint = startPoint };
        pathFigure.Segments.Add(new LineSegment(endPoint, true));

        // hinge at 1st node of beam
        if (element is BeamHinged && element.Type == 1)
        {
            direction = endPoint - startPoint;
            start = RotateVectorScreen(direction, 90);
            start.Normalize();
            targetPoint = startPoint + 5 * start;
            pathFigure.Segments.Add(new LineSegment(targetPoint, false));
            var target = RotateVectorScreen(direction, -90);
            target.Normalize();
            targetPoint = startPoint + 5 * target;
            // ArcSegment begins at last Point of pathFigure
            // target point, size in x,y, opening angle: isLargeArc, sweepDirection: isStroked
            pathFigure.Segments.Add(new ArcSegment(targetPoint, new Size(2.5, 2.5), 180, true, 0, true));
            pathFigure.Segments.Add(new LineSegment(startPoint, false));
        }

        // hinge at 2nd node of beam
        if (element is BeamHinged && element.Type == 2)
        {
            direction = startPoint - endPoint;
            start = RotateVectorScreen(direction, -90);
            start.Normalize();
            targetPoint = endPoint + 5 * start;
            pathFigure.Segments.Add(new LineSegment(targetPoint, false));
            var end = RotateVectorScreen(direction, 90);
            end.Normalize();
            targetPoint = endPoint + 5 * end;
            pathFigure.Segments.Add(new ArcSegment(targetPoint, new Size(2.5, 2.5), 180, true, (SweepDirection)1,
                true));
            pathFigure.Segments.Add(new LineSegment(endPoint, false));
        }

        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }

    private PathGeometry MultiNodeElementDraw(AbstractElement element)
    {
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();
        if (model.Nodes.TryGetValue(element.NodeIds[0], out node))
        {
        }

        var startPoint = TransformNode(node, resolution, maxY);
        pathFigure.StartPoint = startPoint;
        for (var i = 1; i < element.NodeIds.Length; i++)
        {
            if (model.Nodes.TryGetValue(element.NodeIds[i], out node))
            {
            }

            var nextPoint = TransformNode(node, resolution, maxY);
            pathFigure.Segments.Add(new LineSegment(nextPoint, true));
        }

        pathFigure.IsClosed = true;
        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }

    public void ElementTexts()
    {
        foreach (var item in model.Elements)
        {
            if (!(item.Value is Abstract2D element)) continue;
            element.SetReferences(model);
            var cg = element.ComputeCenterOfGravity();
            var id = new TextBlock
            {
                FontSize = 12,
                Text = item.Key,
                Foreground = Blue
            };
            SetTop(id, (-cg.Y + maxY) * resolution + placementV);
            SetLeft(id, cg.X * resolution + placementH);
            visualResults.Children.Add(id);
            ElementIDs.Add(id);
        }
    }

    public void NodeTexts()
    {
        foreach (var item in model.Nodes)
        {
            var id = new TextBlock
            {
                FontSize = 12,
                Text = item.Key,
                Foreground = Red
            };
            SetTop(id, (-item.Value.Coordinates[1] + maxY) * resolution + placementV);
            SetLeft(id, item.Value.Coordinates[0] * resolution + placementH);
            visualResults.Children.Add(id);
            NodeIDs.Add(id);
        }
    }

    public void LoadsDraw()
    {
        AbstractLoad load;
        Shape path;

        // Node loads
        var maxLoadValue = 1.0;
        const int maxLastScreen = 50;
        foreach (var item in model.Loads)
        {
            load = item.Value;
            if (Math.Abs(load.Loadvalues[0]) > maxLoadValue) maxLoadValue = Math.Abs(load.Loadvalues[0]);
            if (Math.Abs(load.Loadvalues[1]) > maxLoadValue) maxLoadValue = Math.Abs(load.Loadvalues[1]);
        }

        foreach (var item in model.PointLoads)
        {
            load = item.Value;
            if (Math.Abs(load.Loadvalues[0]) > maxLoadValue) maxLoadValue = Math.Abs(load.Loadvalues[0]);
            if (Math.Abs(load.Loadvalues[1]) > maxLoadValue) maxLoadValue = Math.Abs(load.Loadvalues[1]);
        }

        maxLoadValue =
            (from linienLast in model.ElementLoads.Select(item => (AbstractLineLoad)item.Value)
             from loadValue in linienLast.Loadvalues
             select Math.Abs(loadValue)).Prepend(maxLoadValue).Max();
        loadResolution = maxLastScreen / maxLoadValue;

        foreach (var item in model.Loads)
        {
            load = item.Value;
            load.LoadId = item.Key;
            var pathGeometry = NodeLoadDraw(load);
            path = new Path
            {
                Name = load.LoadId,
                Stroke = Red,
                StrokeThickness = 3,
                Data = pathGeometry
            };
            LoadVectors.Add(path);

            SetLeft(path, placementH);
            SetTop(path, placementV);
            visualResults.Children.Add(path);
        }

        foreach (var item in model.PointLoads)
        {
            var pathGeometry = PointLoadDraw(item.Value);
            path = new Path
            {
                Name = item.Key,
                Stroke = Red,
                StrokeThickness = 3,
                Data = pathGeometry
            };
            LoadVectors.Add(path);

            SetLeft(path, placementH);
            SetTop(path, placementV);
            visualResults.Children.Add(path);
        }

        foreach (var item in model.ElementLoads)
        {
            var lineLoad = (AbstractLineLoad)item.Value;
            var pathGeometry = LineLoadDraw(lineLoad);
            var rot = FromArgb(60, 255, 0, 0);
            var blau = FromArgb(60, 0, 0, 255);
            var myBrush = new SolidColorBrush(rot);
            if (lineLoad.Loadvalues[1] > 0) myBrush = new SolidColorBrush(blau);
            path = new Path
            {
                Name = lineLoad.LoadId,
                Fill = myBrush,
                Stroke = Red,
                StrokeThickness = 1,
                Data = pathGeometry
            };
            LoadVectors.Add(path);

            SetLeft(path, placementH);
            SetTop(path, placementV);
            visualResults.Children.Add(path);
        }
    }

    public void LoadsTexts()
    {
        foreach (var item in model.Loads)
        {
            if (item.Value is not { }) continue;
            var id = new TextBlock
            {
                FontSize = 12,
                Text = item.Key,
                Foreground = Black
            };
            if (model.Nodes.TryGetValue(item.Value.NodeId, out var loadNode))
            {
                placementText = TransformNode(loadNode, resolution, maxY);
                const int knotenOffset = 20;
                SetTop(id, placementText.Y + placementV - knotenOffset);
                SetLeft(id, placementText.X + placementH);
                visualResults.Children.Add(id);
                LoadIDs.Add(id);
            }
        }
        foreach (var item in model.ElementLoads)
        {
            if (item.Value is not { } linienlast) continue;
            const int nodeOffset = 10;

            var id = new TextBlock
            {
                FontSize = 12,
                Text = item.Key,
                Foreground = Black
            };
            placementText = TransformNode(item.Value.Element.Nodes[0], resolution, maxY);
            SetTop(id, placementText.Y + placementV + nodeOffset);
            SetLeft(id, placementText.X + placementH);
            visualResults.Children.Add(id);
            LoadIDs.Add(id);

            var id2 = new TextBlock
            {
                FontSize = 12,
                Text = item.Key,
                Foreground = Black
            };
            placementText = TransformNode(item.Value.Element.Nodes[1], resolution, maxY);
            SetTop(id2, placementText.Y + placementV + nodeOffset);
            SetLeft(id2, placementText.X + placementH);
            visualResults.Children.Add(id2);
            LoadIDs.Add(id2);
        }
        foreach (var item in model.PointLoads)
        {
            if (item.Value is not PointLoad last) continue;
            var punktlast = (PointLoad)last;
            var id = new TextBlock
            {
                FontSize = 12,
                Text = item.Key,
                Foreground = Black
            };

            var startPoint = TransformNode(last.Element.Nodes[0], resolution, maxY);
            var endPoint = TransformNode(last.Element.Nodes[1], resolution, maxY);
            placementText = (Point)(startPoint + (endPoint - startPoint) * punktlast.Offset);
            const int knotenOffset = 15;
            SetTop(id, placementText.Y + placementV + knotenOffset);
            SetLeft(id, placementText.X + placementH);
            visualResults.Children.Add(id);
            LoadIDs.Add(id);
        }
    }

    private PathGeometry NodeLoadDraw(AbstractLoad nodeLoad)
    {
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();
        const int loadArrowSize = 10;

        if (model.Nodes.TryGetValue(nodeLoad.NodeId, out node))
        {
        }

        if (node != null)
        {
            var endPoint = new Point(node.Coordinates[0] * resolution - nodeLoad.Loadvalues[0] * loadResolution,
                (-node.Coordinates[1] + maxY) * resolution + nodeLoad.Loadvalues[1] * loadResolution);
            pathFigure.StartPoint = endPoint;

            var startPoint = TransformNode(node, resolution, maxY);
            pathFigure.Segments.Add(new LineSegment(startPoint, true));

            var vector = startPoint - endPoint;
            vector.Normalize();
            vector *= loadArrowSize;
            vector = RotateVectorScreen(vector, 30);
            endPoint = new Point(startPoint.X - vector.X, startPoint.Y - vector.Y);
            pathFigure.Segments.Add(new LineSegment(endPoint, true));

            vector = RotateVectorScreen(vector, -60);
            endPoint = new Point(startPoint.X - vector.X, startPoint.Y - vector.Y);
            pathFigure.Segments.Add(new LineSegment(endPoint, false));
            pathFigure.Segments.Add(new LineSegment(startPoint, true));

            if (nodeLoad.Loadvalues.Length > 2 && Math.Abs(nodeLoad.Loadvalues[2]) > double.Epsilon)
            {
                startPoint.X += 30;
                pathFigure.Segments.Add(new LineSegment(startPoint, false));
                startPoint.X -= 30;
                startPoint.Y += 30;
                pathFigure.Segments.Add(new ArcSegment
                    (startPoint, new Size(30, 30), 270, true, new SweepDirection(), true));

                vector = new Vector(1, 0);
                vector *= loadArrowSize;
                vector = RotateVectorScreen(vector, 45);
                endPoint = new Point(startPoint.X - vector.X, startPoint.Y - vector.Y);
                pathFigure.Segments.Add(new LineSegment(endPoint, true));

                vector = RotateVectorScreen(vector, -60);
                endPoint = new Point(startPoint.X - vector.X, startPoint.Y - vector.Y);
                pathFigure.Segments.Add(new LineSegment(endPoint, false));
                pathFigure.Segments.Add(new LineSegment(startPoint, true));
            }
        }

        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }

    private PathGeometry PointLoadDraw(AbstractElementLoad last)
    {
        var punktlast = (PointLoad)last;
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();
        const int lastPfeilGroesse = 10;

        punktlast.SetReferences(model);
        if (model.Elements.TryGetValue(punktlast.ElementId, out var element)) { }

        if (element == null) return pathGeometry;
        if (model.Nodes.TryGetValue(element.NodeIds[0], out var startNode)) { }

        var startPunkt = TransformNode(startNode, resolution, maxY);

        // 2nd element node
        if (model.Nodes.TryGetValue(element.NodeIds[1], out var endNode)) { }

        var endPoint = TransformNode(endNode, resolution, maxY);

        var vector = new Vector(endPoint.X, endPoint.Y) - new Vector(startPunkt.X, startPunkt.Y);
        var loadPoint = (Point)(punktlast.Offset * vector);

        loadPoint.X = startPunkt.X + loadPoint.X;
        loadPoint.Y = startPunkt.Y + loadPoint.Y;

        endPoint = new Point(loadPoint.X - punktlast.Loadvalues[0] * loadResolution,
            -loadPoint.Y + punktlast.Loadvalues[1] * loadResolution);
        pathFigure.StartPoint = endPoint;

        pathFigure.Segments.Add(new LineSegment(loadPoint, true));

        vector = loadPoint - endPoint;
        vector.Normalize();
        vector *= lastPfeilGroesse;
        vector = RotateVectorScreen(vector, 30);
        endPoint = new Point(loadPoint.X - vector.X, loadPoint.Y - vector.Y);
        pathFigure.Segments.Add(new LineSegment(endPoint, true));

        vector = RotateVectorScreen(vector, -60);
        endPoint = new Point(loadPoint.X - vector.X, loadPoint.Y - vector.Y);
        pathFigure.Segments.Add(new LineSegment(endPoint, false));
        pathFigure.Segments.Add(new LineSegment(loadPoint, true));

        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }

    private PathGeometry LineLoadDraw(AbstractLineLoad last)
    {
        var lineLoad = (LineLoad)last;
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();
        const int loadArrowSize = 8;
        const int lineLoadScaling = 1;
        var lineLoadResolution = lineLoadScaling * loadResolution;

        last.SetReferences(model);
        if (model.Elements.TryGetValue(lineLoad.ElementId, out var element)) { }

        if (element == null) return pathGeometry;

        if (model.Nodes.TryGetValue(element.NodeIds[0], out var startNode)) { }
        var startPoint = TransformNode(startNode, resolution, maxY);

        // 2nd element node
        if (model.Nodes.TryGetValue(element.NodeIds[1], out var endNode)) { }

        var endPoint = TransformNode(endNode, resolution, maxY);
        var vector = endPoint - startPoint;

        // start point and load point at start
        pathFigure.StartPoint = startPoint;
        var loadVector = RotateVectorScreen(vector, -90);
        loadVector.Normalize();
        var vec = loadVector * lineLoadResolution * lineLoad.Loadvalues[1];
        var nextPoint = new Point(startPoint.X - vec.X, startPoint.Y - vec.Y);

        if (Math.Abs(vec.Length) > double.Epsilon)
        {
            // Load arrow at start
            loadVector *= loadArrowSize;
            loadVector = RotateVectorScreen(loadVector, -150);
            var point = new Point(startPoint.X - loadVector.X, startPoint.Y - loadVector.Y);
            pathFigure.Segments.Add(new LineSegment(point, true));
            loadVector = RotateVectorScreen(loadVector, -60);
            point = new Point(startPoint.X - loadVector.X, startPoint.Y - loadVector.Y);
            pathFigure.Segments.Add(new LineSegment(point, false));
            pathFigure.Segments.Add(new LineSegment(startPoint, true));

            // Line from start point to start of load
            pathFigure.Segments.Add(new LineSegment(nextPoint, true));
        }

        // Line to end of load
        loadVector = RotateVectorScreen(vector, 90);
        loadVector.Normalize();
        vec = loadVector * lineLoadResolution * lineLoad.Loadvalues[3];
        nextPoint = new Point(endPoint.X + vec.X, endPoint.Y + vec.Y);
        pathFigure.Segments.Add(new LineSegment(nextPoint, true));

        // Line to endpoint
        pathFigure.Segments.Add(new LineSegment(endPoint, true));

        if (Math.Abs(vec.Length) > double.Epsilon)
        {
            // Load arrow at end
            loadVector *= loadArrowSize;
            loadVector = RotateVectorScreen(loadVector, 30);
            nextPoint = new Point(endPoint.X - loadVector.X, endPoint.Y - loadVector.Y);
            pathFigure.Segments.Add(new LineSegment(nextPoint, true));
            loadVector = RotateVectorScreen(loadVector, -60);
            nextPoint = new Point(endPoint.X - loadVector.X, endPoint.Y - loadVector.Y);
            pathFigure.Segments.Add(new LineSegment(nextPoint, false));
            pathFigure.Segments.Add(new LineSegment(endPoint, true));
        }

        // close pathFigure
        pathFigure.IsClosed = true;
        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }

    public void ConstraintsDraw()
    {
        foreach (var item in model.BoundaryConditions)
        {
            var support = item.Value;
            var pathGeometry = new PathGeometry();

            if (model.Nodes.TryGetValue(support.NodeId, out var supportNode))
            {
            }

            var pivotPoint = TransformNode(supportNode, resolution, maxY);
            double pivotAngle = 0;
            bool left = false, bottom = false, right = false, beam = false;

            if (supportNode != null)
            {
                if (Math.Abs(supportNode.Coordinates[0] - minX) < double.Epsilon) left = true;
                else if (Math.Abs(supportNode.Coordinates[0] - maxX) < double.Epsilon) right = true;
                if (Math.Abs(supportNode.Coordinates[1] - minY) < double.Epsilon) bottom = true;

                if (Math.Abs(maxY - minY) < double.Epsilon) beam = true;
            }

            switch (support.Type)
            {
                // X_FIXED = 1, Y_FIXED = 2, XY_FIXED = 3, XYR_FIXED = 7
                // R_FIXED = 4, XR_FIXED = 5, YR_FIXED = 6 werden in Balkentheorie nicht dargestellt
                case 1:
                    {
                        pathGeometry = SingleConstraintDraw(supportNode);
                        if (left) pivotAngle = 90;
                        else if (right) pivotAngle = -90;
                        pathGeometry.Transform = new RotateTransform(pivotAngle, pivotPoint.X, pivotPoint.Y);
                        break;
                    }
                case 2:
                    pathGeometry = SingleConstraintDraw(supportNode);
                    break;
                case 3:
                    pathGeometry = DoubleConstraintDraw(supportNode);
                    if (left && !beam) pivotAngle = 90;
                    else if (right) pivotAngle = -90;
                    if (bottom && !beam) pivotAngle = 0;
                    pathGeometry.Transform = new RotateTransform(pivotAngle, pivotPoint.X, pivotPoint.Y);
                    break;
                case 7:
                    {
                        pathGeometry = TripleConstraintDraw(supportNode);
                        if (left) pivotAngle = 90;
                        else if (right) pivotAngle = -90;
                        if (bottom && !beam) pivotAngle = 0;
                        pathGeometry.Transform = new RotateTransform(pivotAngle, pivotPoint.X, pivotPoint.Y);
                        break;
                    }
            }

            Shape path = new Path
            {
                Name = support.SupportId,
                Stroke = Green,
                StrokeThickness = 2,
                Data = pathGeometry
            };
            SupportRepresentation.Add(path);

            SetLeft(path, placementH);
            SetTop(path, placementV);
            // draw Shape
            visualResults.Children.Add(path);
        }
    }

    private PathGeometry SingleConstraintDraw(Node supportNode)
    {
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();
        const int supportSymbol = 20;

        var startPoint = TransformNode(supportNode, resolution, maxY);
        pathFigure.StartPoint = startPoint;

        var endPoint = new Point(startPoint.X - supportSymbol, startPoint.Y + supportSymbol);
        pathFigure.Segments.Add(new LineSegment(endPoint, true));
        endPoint = new Point(endPoint.X + 2 * supportSymbol, startPoint.Y + supportSymbol);
        pathFigure.Segments.Add(new LineSegment(endPoint, true));
        pathFigure.Segments.Add(new LineSegment(startPoint, true));

        startPoint = new Point(endPoint.X + 5, endPoint.Y + 5);
        pathFigure.Segments.Add(new LineSegment(startPoint, false));
        endPoint = new Point(startPoint.X - 50, startPoint.Y);
        pathFigure.Segments.Add(new LineSegment(endPoint, true));

        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }

    private PathGeometry DoubleConstraintDraw(Node supportNode)
    {
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();
        const int supportSymbol = 20;

        var startPoint = TransformNode(supportNode, resolution, maxY);
        pathFigure.StartPoint = startPoint;

        var endPoint = new Point(startPoint.X - supportSymbol, startPoint.Y + supportSymbol);
        pathFigure.Segments.Add(new LineSegment(endPoint, true));
        endPoint = new Point(endPoint.X + 2 * supportSymbol, startPoint.Y + supportSymbol);
        pathFigure.Segments.Add(new LineSegment(endPoint, true));
        pathFigure.Segments.Add(new LineSegment(startPoint, true));

        startPoint = endPoint;
        pathFigure.Segments.Add(new LineSegment(startPoint, false));
        endPoint = new Point(startPoint.X - 5, startPoint.Y + 5);
        pathFigure.Segments.Add(new LineSegment(endPoint, true));

        pathFigure.Segments.Add(new LineSegment(new Point(startPoint.X - 10, startPoint.Y), false));
        pathFigure.Segments.Add(new LineSegment(new Point(endPoint.X - 10, endPoint.Y), true));

        pathFigure.Segments.Add(new LineSegment(new Point(startPoint.X - 20, startPoint.Y), false));
        pathFigure.Segments.Add(new LineSegment(new Point(endPoint.X - 20, endPoint.Y), true));

        pathFigure.Segments.Add(new LineSegment(new Point(startPoint.X - 30, startPoint.Y), false));
        pathFigure.Segments.Add(new LineSegment(new Point(endPoint.X - 30, endPoint.Y), true));

        pathFigure.Segments.Add(new LineSegment(new Point(startPoint.X - 40, startPoint.Y), false));
        pathFigure.Segments.Add(new LineSegment(new Point(endPoint.X - 40, endPoint.Y), true));

        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }

    private PathGeometry TripleConstraintDraw(Node supportNode)
    {
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();
        const int supportSymbol = 20;

        var startPoint = TransformNode(supportNode, resolution, maxY);

        startPoint = new Point(startPoint.X - supportSymbol, startPoint.Y);
        pathFigure.StartPoint = startPoint;
        var endPoint = new Point(startPoint.X + 2 * supportSymbol, startPoint.Y);
        pathFigure.Segments.Add(new LineSegment(endPoint, true));
        pathGeometry.Figures.Add(pathFigure);
        pathFigure = new PathFigure
        {
            StartPoint = startPoint
        };
        endPoint = new Point(startPoint.X - 10, startPoint.Y + 10);
        pathFigure.Segments.Add(new LineSegment(endPoint, true));
        pathGeometry.Figures.Add(pathFigure);
        for (var i = 0; i < 4; i++)
        {
            pathFigure = new PathFigure();
            startPoint = new Point(startPoint.X + 10, startPoint.Y);
            pathFigure.StartPoint = startPoint;
            endPoint = new Point(startPoint.X - 10, startPoint.Y + 10);
            pathFigure.Segments.Add(new LineSegment(endPoint, true));
            pathGeometry.Figures.Add(pathFigure);
        }

        return pathGeometry;
    }

    public void ConstraintsTexts()
    {
        foreach (var item in model.BoundaryConditions)
        {
            if (item.Value is not { } support) continue;
            var id = new TextBlock
            {
                FontSize = 12,
                Text = item.Key,
                Foreground = Green
            };
            item.Value.SetReferences(model);
            placementText = TransformNode(item.Value.Node, resolution, maxY);
            const int supportSymbol = 25;
            SetTop(id, placementText.Y + placementV + supportSymbol);
            SetLeft(id, placementText.X + placementH);
            visualResults.Children.Add(id);
            SupportIDs.Add(id);
        }
    }

    //public void AccelerationsDraw()
    //{
    //    var screenPoint = new int[2];
    //    var accelerationResolution = 0.5;
    //    foreach (var item in model.Nodes)
    //    {
    //        node = item.Value;
    //        var pathGeometry = new PathGeometry();
    //        var pathFigure = new PathFigure();
    //        var deformed = TransformDeformedNode(node, resolution, maxY);
    //        pathFigure.StartPoint = deformed;

    //        screenPoint[0] = (int)(deformed.X - item.Value.NodalDerivatives[0][timeStep] * accelerationResolution);
    //        screenPoint[1] = (int)(deformed.Y + item.Value.NodalDerivatives[1][timeStep] * accelerationResolution);

    //        var acceleration = new Point(screenPoint[0], screenPoint[1]);
    //        pathFigure.Segments.Add(new LineSegment(acceleration, true));

    //        pathGeometry.Figures.Add(pathFigure);
    //        Shape path = new Path()
    //        {
    //            Stroke = Blue,
    //            StrokeThickness = 2,
    //            Data = pathGeometry
    //        };
    //        SetLeft(path, BorderLeft);
    //        SetTop(path, BorderTop);
    //        visualResults.Children.Add(path);
    //        Accelerations.Add(path);
    //    }
    //}

    public void AxialForceDraw(AbstractBeam element, double maxAxialForce, bool elementLoad)
    {
        var axialForce1Scaled = element.ElementState[0] / maxAxialForce * MaxAxialForceScreen;
        double axialForce2Scaled;
        if (element.ElementState.Length == 2)
            axialForce2Scaled = element.ElementState[1] / maxAxialForce * MaxAxialForceScreen;
        else
            axialForce2Scaled = element.ElementState[3] / maxAxialForce * MaxAxialForceScreen;

        Point nextPoint;
        Vector vec, vec2;
        var red = FromArgb(120, 255, 0, 0);
        var blue = FromArgb(120, 0, 0, 255);

        if (model.Nodes.TryGetValue(element.NodeIds[0], out node))
        {
        }

        var startPoint = TransformNode(node, resolution, maxY);

        if (model.Nodes.TryGetValue(element.NodeIds[1], out node))
        {
        }
        var endPoint = TransformNode(node, resolution, maxY);

        if (!elementLoad)
        {
            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure();

            var myBrush = new SolidColorBrush(blue);
            if (axialForce1Scaled < 0) myBrush = new SolidColorBrush(red);

            pathFigure.StartPoint = startPoint;
            vec = endPoint - startPoint;
            vec.Normalize();
            vec2 = RotateVectorScreen(vec, -90);
            nextPoint = startPoint + vec2 * axialForce1Scaled;
            pathFigure.Segments.Add(new LineSegment(nextPoint, true));
            nextPoint = endPoint + vec2 * axialForce2Scaled;
            pathFigure.Segments.Add(new LineSegment(nextPoint, true));
            pathFigure.Segments.Add(new LineSegment(endPoint, true));
            pathFigure.IsClosed = true;
            pathGeometry.Figures.Add(pathFigure);

            Shape path = new Path
            {
                Fill = myBrush,
                Stroke = Black,
                StrokeThickness = 1,
                Data = pathGeometry
            };
            SetLeft(path, placementH);
            SetTop(path, placementV);
            visualResults.Children.Add(path);
            AxialForceList.Add(path);
        }
        else
        {
            // contribution of a point load
            double pointLoadN = 0, pointLoadOffset = 0;

            IEnumerable<PointLoad> PointLoads()
            {
                foreach (var load in model.ElementLoads.Select(item => (PointLoad)item.Value)
                             .Where(load => load.ElementId == element.ElementId))
                    yield return load;
            }

            foreach (var pointLoad in PointLoads())
            {
                pointLoadN = pointLoad.Loadvalues[0];
                pointLoadOffset = pointLoad.Offset;
            }

            // contribution of a line load
            IEnumerable<LineLoad> LineLoads()
            {
                foreach (var item in model.ElementLoads)
                    if (item.Value is LineLoad lineLoad && item.Value.ElementId == element.ElementId)
                        yield return lineLoad;
            }

            foreach (var lineLoad in LineLoads())
            {
                var pathGeometry = new PathGeometry();
                var pathFigure = new PathFigure();

                var myBrush = new SolidColorBrush(blue);
                if (axialForce1Scaled < 0) myBrush = new SolidColorBrush(red);

                pathFigure.StartPoint = startPoint;
                vec = endPoint - startPoint;
                vec.Normalize();
                vec2 = RotateVectorScreen(vec, -90);
                nextPoint = startPoint + vec2 * axialForce1Scaled;
                pathFigure.Segments.Add(new LineSegment(nextPoint, true));

                if (pointLoadOffset > double.Epsilon)
                {
                    nextPoint += pointLoadOffset * (endPoint - startPoint);

                    var na = lineLoad.Loadvalues[0];
                    var nb = lineLoad.Loadvalues[2];
                    var constant = na * pointLoadOffset * element.length;
                    var linear = (nb - na) * pointLoadOffset / 2 * element.length;
                    if (nb < na)
                    {
                        constant = nb * pointLoadOffset * element.length;
                        linear = (na - nb) * (1 - pointLoadOffset) / 2 * element.length;
                    }

                    nextPoint += vec2 * (constant + linear) / maxAxialForce * MaxAxialForceScreen;
                    pathFigure.Segments.Add(new LineSegment(nextPoint, true));
                    nextPoint += vec2 * pointLoadN / maxAxialForce * MaxAxialForceScreen;
                    pathFigure.Segments.Add(new LineSegment(nextPoint, true));
                }

                nextPoint = endPoint - vec2 * axialForce2Scaled;
                pathFigure.Segments.Add(new LineSegment(nextPoint, true));
                pathFigure.Segments.Add(new LineSegment(endPoint, true));
                pathFigure.IsClosed = true;
                pathGeometry.Figures.Add(pathFigure);

                Shape path = new Path
                {
                    Fill = myBrush,
                    Stroke = Black,
                    StrokeThickness = 1,
                    Data = pathGeometry
                };
                SetLeft(path, placementH);
                SetTop(path, placementV);
                visualResults.Children.Add(path);
                AxialForceList.Add(path);
            }
        }
    }

    public void ShearForceDraw(AbstractBeam element, double maxShearForce, bool elementLoad)
    {
        if (element is Truss) return;
        var shearForce1Scaled = element.ElementState[1] / maxShearForce * MaxShearForceScreen;
        var shearForce2Scaled = element.ElementState[4] / maxShearForce * MaxShearForceScreen;

        Point nextPoint;
        Vector vec, vec2;
        var red = FromArgb(120, 255, 0, 0);
        var blue = FromArgb(120, 0, 0, 255);
        SolidColorBrush myBrush;

        if (model.Nodes.TryGetValue(element.NodeIds[0], out var startNode)) { }

        var startPoint = TransformNode(startNode, resolution, maxY);

        if (model.Nodes.TryGetValue(element.NodeIds[1], out var endNode)) { }

        var endPoint = TransformNode(endNode, resolution, maxY);

        if (!elementLoad)
        {
            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure();

            myBrush = new SolidColorBrush(blue);
            if (shearForce1Scaled < 0) myBrush = new SolidColorBrush(red);

            pathFigure.StartPoint = startPoint;
            vec = endPoint - startPoint;
            vec.Normalize();
            vec2 = RotateVectorScreen(vec, -90);
            nextPoint = startPoint + vec2 * shearForce1Scaled;
            pathFigure.Segments.Add(new LineSegment(nextPoint, true));
            nextPoint = endPoint + vec2 * shearForce1Scaled;
            pathFigure.Segments.Add(new LineSegment(nextPoint, true));
            pathFigure.Segments.Add(new LineSegment(endPoint, true));
            pathFigure.IsClosed = true;
            pathGeometry.Figures.Add(pathFigure);

            Shape path = new Path
            {
                Fill = myBrush,
                Stroke = Black,
                StrokeThickness = 1,
                Data = pathGeometry
            };
            SetLeft(path, placementH);
            SetTop(path, placementV);
            visualResults.Children.Add(path);
            ShearForceList.Add(path);
        }
        // element has 1 point and/or 1 line load
        else
        {
            // test, if element has point load
            bool beamPointLoad = false, beamContinuousLoad = false;
            double pointLoadQ = 0, pointLoadOffset = 0;
            AbstractElementLoad lineLoad = null;

            foreach (var item in model.PointLoads)
            {
                if (!(item.Value is PointLoad last) || item.Value.ElementId != element.ElementId) continue;
                beamPointLoad = true;
                pointLoadQ = last.Loadvalues[1];
                pointLoadOffset = last.Offset;
                break;
            }

            // test, ob element has line load
            foreach (var item in model.ElementLoads)
            {
                if (!(item.Value is LineLoad last) || item.Value.ElementId != element.ElementId) continue;
                beamContinuousLoad = true;
                lineLoad = last;
                break;
            }

            // only point load on beam and no continuous load
            if (beamPointLoad && !beamContinuousLoad)
            {
                var pathGeometry = new PathGeometry();
                var pathFigure = new PathFigure();

                // Shear Force line from start to load point
                myBrush = new SolidColorBrush(blue);
                if (shearForce1Scaled < 0) myBrush = new SolidColorBrush(red);

                pathFigure.StartPoint = startPoint;
                vec = endPoint - startPoint;
                vec.Normalize();
                vec2 = RotateVectorScreen(vec, -90);
                nextPoint = startPoint + vec2 * shearForce1Scaled;
                pathFigure.Segments.Add(new LineSegment(nextPoint, true));

                nextPoint += pointLoadOffset * (endPoint - startPoint);
                pathFigure.Segments.Add(new LineSegment(nextPoint, true));

                startPoint += pointLoadOffset * (endPoint - startPoint);
                pathFigure.Segments.Add(new LineSegment(startPoint, true));
                pathFigure.IsClosed = true;
                pathGeometry.Figures.Add(pathFigure);
                Shape path = new Path
                {
                    Fill = myBrush,
                    Stroke = Black,
                    StrokeThickness = 1,
                    Data = pathGeometry
                };
                SetLeft(path, placementH);
                SetTop(path, placementV);
                visualResults.Children.Add(path);
                ShearForceList.Add(path);

                // Shear Force line from load to end point
                pathGeometry = new PathGeometry();
                pathFigure = new PathFigure();
                myBrush = new SolidColorBrush(blue);
                if (shearForce1Scaled + pointLoadQ / maxShearForce * MaxShearForceScreen > 0)
                    myBrush = new SolidColorBrush(red);
                pathFigure.StartPoint = startPoint;
                nextPoint -= vec2 * pointLoadQ / maxShearForce * MaxShearForceScreen;
                pathFigure.Segments.Add(new LineSegment(nextPoint, true));

                nextPoint = endPoint + vec2 * shearForce2Scaled;
                pathFigure.Segments.Add(new LineSegment(nextPoint, true));

                pathFigure.Segments.Add(new LineSegment(endPoint, true));
                pathFigure.IsClosed = true;
                pathGeometry.Figures.Add(pathFigure);

                path = new Path
                {
                    Fill = myBrush,
                    Stroke = Black,
                    StrokeThickness = 1,
                    Data = pathGeometry
                };
                SetLeft(path, placementH);
                SetTop(path, placementV);
                visualResults.Children.Add(path);
                ShearForceList.Add(path);
            }

            // continuous load on beam and additional point load if applicable
            else if (beamContinuousLoad)
            {
                var pathGeometry = new PathGeometry();
                var pathFigure = new PathFigure();

                myBrush = new SolidColorBrush(blue);
                if (shearForce1Scaled < 0) myBrush = new SolidColorBrush(red);

                // Shear Force line on left side
                pathFigure.StartPoint = startPoint;
                vec = endPoint - startPoint;
                vec.Normalize();
                vec2 = RotateVectorScreen(vec, -90);
                nextPoint = startPoint + vec2 * shearForce1Scaled;
                pathFigure.Segments.Add(new LineSegment(nextPoint, true));

                if (pointLoadOffset < double.Epsilon)
                {
                    startPoint += 0.5 * (endPoint - startPoint);
                    pathFigure.Segments.Add(new LineSegment(startPoint, true));
                }
                else
                {
                    nextPoint += pointLoadOffset * (endPoint - startPoint);
                    var loadOffset = pointLoadOffset * element.length;
                    var qa = lineLoad.Loadvalues[1];
                    var qb = lineLoad.Loadvalues[3];
                    var constant = qa * loadOffset;
                    var linear = (qb - qa) * loadOffset / 2;
                    if (qb < qa)
                    {
                        constant = qb * loadOffset;
                        linear = (qa - qb) * (1 - pointLoadOffset) * element.length / 2;
                    }

                    nextPoint -= vec2 * (constant + linear) / maxShearForce * MaxShearForceScreen;
                    pathFigure.Segments.Add(new LineSegment(nextPoint, true));
                    startPoint += pointLoadOffset * (endPoint - startPoint);
                    pathFigure.Segments.Add(new LineSegment(startPoint, true));
                }

                pathFigure.IsClosed = true;
                pathGeometry.Figures.Add(pathFigure);

                Shape path = new Path
                {
                    Fill = myBrush,
                    Stroke = Black,
                    StrokeThickness = 1,
                    Data = pathGeometry
                };
                SetLeft(path, placementH);
                SetTop(path, placementV);
                visualResults.Children.Add(path);
                ShearForceList.Add(path);

                // Shear Force line on right side
                pathGeometry = new PathGeometry();
                pathFigure = new PathFigure();
                myBrush = new SolidColorBrush(blue);
                if (shearForce2Scaled < 0) myBrush = new SolidColorBrush(red);
                pathFigure.StartPoint = startPoint;

                if (pointLoadOffset > double.Epsilon)
                {
                    nextPoint -= vec2 * pointLoadQ / maxShearForce * MaxShearForceScreen;
                    pathFigure.Segments.Add(new LineSegment(nextPoint, true));
                }

                nextPoint = endPoint + vec2 * shearForce2Scaled;
                pathFigure.Segments.Add(new LineSegment(nextPoint, true));
                pathFigure.Segments.Add(new LineSegment(endPoint, true));

                pathFigure.IsClosed = true;
                pathGeometry.Figures.Add(pathFigure);

                path = new Path
                {
                    Fill = myBrush,
                    Stroke = Black,
                    StrokeThickness = 1,
                    Data = pathGeometry
                };
                SetLeft(path, placementH);
                SetTop(path, placementV);
                visualResults.Children.Add(path);
                ShearForceList.Add(path);
            }
        }
    }

    public void BendingMomentDraw(AbstractBeam element, double scalingMoment, bool elementLoad)
    {
        if (element is Truss) return;
        var moment1Scaled = element.ElementState[2] / scalingMoment * MaxMomentScreen;
        var moment2Scaled = element.ElementState[5] / scalingMoment * MaxMomentScreen;

        var red = FromArgb(120, 255, 0, 0);
        var blue = FromArgb(120, 0, 0, 255);

        if (model.Nodes.TryGetValue(element.NodeIds[0], out node)) { }
        var startPoint = TransformNode(node, resolution, maxY);

        if (model.Nodes.TryGetValue(element.NodeIds[1], out node)) { }
        var endPoint = TransformNode(node, resolution, maxY);

        double pointLoadOffset = 0;
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();

        var myBrush = new SolidColorBrush(blue);
        if ((int)moment1Scaled < 0) { myBrush = new SolidColorBrush(red); }
        else if ((int)moment1Scaled == 0) { if ((int)moment2Scaled < 0) { myBrush = new SolidColorBrush(red); } }

        pathFigure.StartPoint = startPoint;
        var vec = endPoint - startPoint;
        vec.Normalize();

        // line from start to Moment1 scaled
        var vec2 = RotateVectorScreen(vec, 90);
        var nextPoint = startPoint + vec2 * moment1Scaled;
        pathFigure.Segments.Add(new LineSegment(nextPoint, true));

        // only node loads, no Point-/LineLoads, i.e. only bar end forces
        if (!elementLoad)
        {
            //line from Moment1 scaled to Moment2 scaled
            nextPoint = endPoint + vec2 * moment2Scaled;
            pathFigure.Segments.Add(new LineSegment(nextPoint, true));

            // line to end and close pathFigure
            pathFigure.Segments.Add(new LineSegment(endPoint, true));
            pathFigure.IsClosed = true;
            pathGeometry.Figures.Add(pathFigure);

            Shape path = new Path
            {
                Fill = myBrush,
                Stroke = Black,
                StrokeThickness = 1,
                Data = pathGeometry
            };
            SetLeft(path, placementH);
            SetTop(path, placementV);
            visualResults.Children.Add(path);
            BendingMomentList.Add(path);
        }

        // ElementLoads (LineLoad, PointLoad) exist
        // Element has Point- and/or LineLoad
        else
        {
            bool elementHasPointLoad = false, elementHasLineLoad = false;
            LineLoad lineLoad = null;

            // find PointLoad on beam element
            foreach (var item in model.PointLoads)
            {
                if (!(item.Value is PointLoad load) || item.Value.ElementId != element.ElementId) continue;
                pointLoadOffset = load.Offset;
                elementHasPointLoad = true;
                break;
            }

            var maxPoint = new Point(0, 0);
            double mmax = 0;

            // find LineLoad on beam element
            foreach (var item in model.ElementLoads)
            {
                if (!(item.Value is LineLoad last) || item.Value.ElementId != element.ElementId) continue;
                lineLoad = last;
                elementHasLineLoad = true;
                break;
            }

            // draw bending moment line, no Point, no LineLoad
            if (elementHasPointLoad && !elementHasLineLoad)
            {
                // line from Moment1 scaled to Mmax scaled
                mmax = element.ElementState[2] - element.ElementState[1] * pointLoadOffset * element.length;
                var mmaxScaled = mmax / scalingMoment * MaxMomentScreen;

                maxPoint = startPoint + vec * pointLoadOffset * element.length * resolution + vec2 * mmaxScaled;
                pathFigure.Segments.Add(new LineSegment(maxPoint, true));

                // line from Mmax skaliert to Moment2 scaled
                nextPoint = endPoint + vec2 * moment2Scaled;
                pathFigure.Segments.Add(new LineSegment(nextPoint, true));

                // line to end and close pathFigure
                pathFigure.Segments.Add(new LineSegment(endPoint, true));
            }

            // draw bending moment under continuous or triangular load
            else if (elementHasLineLoad)
            {
                var qa = lineLoad.Loadvalues[1];
                var qb = lineLoad.Loadvalues[3];
                var l = element.length;
                double controlOffset = 3;
                double offsetMmax, constant, linear;

                // constant load or linear ascending triangular load
                if (Math.Abs(qb) >= Math.Abs(qa))
                {
                    var q = qb - qa;
                    offsetMmax = l / 2;
                    if (Math.Abs(q) > double.Epsilon)
                    {
                        offsetMmax = ((-qa / q) + Math.Sqrt(Math.Abs(Math.Pow(qa / q, 2)
                                                                     + 2 / l / q * element.ElementState[1]))) * l;
                    }

                    constant = qa * offsetMmax;
                    linear = q / l * offsetMmax * offsetMmax / 2;
                    mmax = element.ElementState[2] - element.ElementState[1] * offsetMmax
                           + constant * offsetMmax / 2
                           + linear * offsetMmax / 3;
                }
                // linear descending triangular load
                else
                {
                    // local coordinate from end of beam
                    var q = qa - qb;
                    offsetMmax = (-qb / q + Math.Sqrt(Math.Abs(Math.Pow(qb / q, 2)
                                                               + 2 / l / q * element.ElementState[4]))) * l;
                    constant = qb * offsetMmax;
                    linear = q / l * offsetMmax * offsetMmax / 2;
                    mmax = element.ElementState[5] + element.ElementState[4] * offsetMmax
                                                   + constant * offsetMmax / 2
                                                   + linear * offsetMmax / 3;
                    offsetMmax = l - offsetMmax;
                }

                var mmaxScaled = mmax / scalingMoment * MaxMomentScreen;

                // draw bending moment curve as quadratic Bezier-Spline
                // only Line- no PointLoad
                if (!elementHasPointLoad)
                {
                    // maxPoint as maximum moment, control point by scaling (controlOffset) of max. moment
                    maxPoint = startPoint + offsetMmax / element.length * (endPoint - startPoint)
                                          + vec2 * controlOffset * mmaxScaled;
                    nextPoint = endPoint + vec2 * moment2Scaled;
                    pathFigure.Segments.Add(new QuadraticBezierSegment(maxPoint, nextPoint, true));
                    pathFigure.Segments.Add(new LineSegment(endPoint, true));
                }

                // Element has PointLoad
                else
                {
                    double m1, m2, deltaM1, deltaM2;
                    var offsetPointLoad = pointLoadOffset * element.length;
                    var offset1 = offsetPointLoad / 2;
                    var offset2 = (l - offsetPointLoad) / 2;

                    // disconiuity at PointLoad, moment curve by 2 quadratic Bezier-Segments
                    // qa <= qb   continuous load or triangular load linearly ascending
                    if (Math.Abs(qb) >= Math.Abs(qa))
                    {
                        var q = qb - qa;
                        constant = qa * offsetPointLoad;
                        linear = q / l * offsetPointLoad * offsetPointLoad / 2;
                        mmax = element.ElementState[2] - element.ElementState[1] * offsetPointLoad
                               + constant * offsetPointLoad / 2
                               + linear * offsetPointLoad / 3;

                        // moment in middle of 1st segment (to the left of PointLoad)
                        constant = qa * offset1;
                        linear = q / l * offset1 * offset1 / 2;
                        m1 = element.ElementState[2] - element.ElementState[1] * offset1
                             + constant * offset1 / 2
                             + linear * offset1 / 3;
                        deltaM1 = m1 - (element.ElementState[2] + (Math.Abs(element.ElementState[2]) + mmax) / 2);

                        // moment in middle of 2nd segment (to the right of PointLoad)
                        var lastOrdinate = qa + q * (1 - offset2 / l);
                        constant = lastOrdinate * offset2;
                        linear = (qb - lastOrdinate) * offset2 / 2;
                        m2 = element.ElementState[5] + element.ElementState[4] * offset2
                                                     + constant * offset2 / 2
                                                     + linear * offset2 * 2 / 3;
                        deltaM2 = m2 - (element.ElementState[5] + (Math.Abs(element.ElementState[5]) + mmax) / 2);
                    }

                    // triangular load linearly descending, local coordinate from right end
                    else
                    {
                        var q = qa - qb;
                        constant = qb * offsetPointLoad;
                        linear = q / l * offsetPointLoad * offsetPointLoad / 2;
                        mmax = element.ElementState[5] + element.ElementState[4] * offsetPointLoad
                                                       + constant * offsetPointLoad / 2
                                                       + linear * offsetPointLoad / 3;

                        // moment in middle of 2nd segment (to the right of PointLoad)
                        constant = qb * offset2;
                        linear = q / l * offset2 * offset2 / 2;
                        m2 = element.ElementState[5] + element.ElementState[4] * offset2
                                                     + constant * offset2 / 2
                                                     + linear * offset2 / 3;
                        deltaM2 = m2 - (element.ElementState[5] - (Math.Abs(element.ElementState[5]) + mmax) / 2);

                        // moment in middle of 1st segment (to the left of PointLoad)
                        var loadOrdinate = qb + q * (1 - offset1 / l);
                        constant = loadOrdinate * offset1;
                        linear = (qa - loadOrdinate) * offset1 / 2;
                        m1 = element.ElementState[2] - element.ElementState[1] * offset1
                             + constant * offset1 / 2
                             + linear * offset1 * 2 / 3;
                        deltaM1 = m1 - (element.ElementState[2] + (Math.Abs(element.ElementState[2]) + mmax) / 2);
                    }

                    maxPoint = startPoint + pointLoadOffset * (endPoint - startPoint)
                                          + vec2 * mmax / scalingMoment * MaxMomentScreen;

                    var controlVector = (startPoint - maxPoint);
                    controlVector.Normalize();
                    controlVector = RotateVectorScreen(controlVector, -90);
                    controlOffset = 1;
                    var controlPoint1 = startPoint + (maxPoint - startPoint) / 2
                                                   + controlVector * controlOffset * deltaM1 / scalingMoment * MaxMomentScreen;

                    controlVector = (endPoint - maxPoint);
                    controlVector.Normalize();
                    controlVector = RotateVectorScreen(controlVector, -90);
                    var controlPoint2 = endPoint - (endPoint - maxPoint) / 2
                                                 - controlVector * controlOffset * deltaM2 / scalingMoment * MaxMomentScreen;

                    nextPoint = endPoint + vec2 * moment2Scaled;
                    // startpoint is endpoint in PathFigure
                    // controlpoint1 for ordinate in middle of 1. Segment, maxPoint at end of 1. Segments,
                    // controlpoint2 für ordinate in middle of 2. Segment,endpoint of curve
                    var bezierPoints = new PointCollection(4)
                    {
                        controlPoint1,
                        maxPoint,
                        controlPoint2,
                        nextPoint
                    };
                    pathFigure.Segments.Add(new PolyQuadraticBezierSegment(bezierPoints, true));
                    pathFigure.Segments.Add(new LineSegment(endPoint, true));
                }
            }

            pathFigure.IsClosed = true;
            pathGeometry.Figures.Add(pathFigure);

            Shape path = new Path
            {
                Fill = myBrush,
                Stroke = Black,
                StrokeThickness = 1,
                Data = pathGeometry
            };
            SetLeft(path, placementH);
            SetTop(path, placementV);
            visualResults.Children.Add(path);
            BendingMomentList.Add(path);

            maxMomentText = new TextBlock
            {
                FontSize = 12,
                Text = "Bending Moment = " + mmax.ToString("N2"),
                Foreground = Blue
            };
            SetTop(maxMomentText, maxPoint.Y + placementV);
            SetLeft(maxMomentText, maxPoint.X);
            visualResults.Children.Add(maxMomentText);
            MaxTexts.Add(maxMomentText);
        }
    }

    public void CoordinateSystem(double tmin, double tmax, double max, double min)
    {
        const int border = 20;
        screenH = visualResults.ActualWidth;
        screenV = visualResults.ActualHeight;
        resolutionV = (screenV - border) / (max - min);
        resolutionH = (screenH - border) / (tmax - tmin);
        var xAxis = new Line
        {
            Stroke = Black,
            X1 = 0,
            Y1 = max * resolutionV + placementV,
            X2 = (tmax - tmin) * resolutionH + placementH,
            Y2 = max * resolutionV + placementV,
            StrokeThickness = 2
        };
        _ = visualResults.Children.Add(xAxis);
        var yAxis = new Line
        {
            Stroke = Black,
            X1 = BorderLeft,
            Y1 = max * resolutionV - min * resolutionV + 2 * placementV,
            X2 = BorderLeft,
            Y2 = placementV,
            StrokeThickness = 2
        };
        visualResults.Children.Add(yAxis);
    }

    // time history drawn from tmin
    public void TimeHistoryDraw(double dt, double tmin, double tmax, double mY, double[] ordinates)
    {
        var timeHistory = new Polyline
        {
            Stroke = Red,
            StrokeThickness = 2
        };
        var supportPoints = new PointCollection();

        var start = (int)Math.Round(tmin / dt);
        var nSteps = (int)Math.Round((tmax - tmin) / dt) + 1;

        for (var i = 0; i < nSteps - start; i++)
        {
            var point = new Point(dt * i * resolutionH, -ordinates[i + start] * resolutionV);
            supportPoints.Add(point);
        }

        timeHistory.Points = supportPoints;

        SetLeft(timeHistory, BorderLeft);
        SetTop(timeHistory, mY * resolutionV + placementV);
        // draw Shape
        visualResults.Children.Add(timeHistory);
    }

    private static Vector RotateVectorScreen(Vector vec, double rotationAngle) // clockwise in degree
    {
        var vector = vec;
        var angle = rotationAngle * Math.PI / 180;
        return new Vector(vector.X * Math.Cos(angle) - vector.Y * Math.Sin(angle),
            vector.X * Math.Sin(angle) + vector.Y * Math.Cos(angle));
    }

    private static Point TransformNode(Node node, double resolution, double maxY)
    {
        return new Point(node.Coordinates[0] * resolution, (-node.Coordinates[1] + maxY) * resolution);
    }
    private Point TransformDeformedNode(Node deformed, double resol, double max)
    {
        // input unit e.g. m, deformation unit e.g. cm --> Scaling
        return new Point((deformed.Coordinates[0] + deformed.NodalDof[0] * scalingDisplacement) * resol,
            (-deformed.Coordinates[1] - deformed.NodalDof[1] * scalingDisplacement + max) * resol);
    }
    public double[] TransformScreenPoint(Point point)
    {
        var coordinates = new double[2];
        coordinates[0] = (point.X - placementH) / resolution;
        coordinates[1] = (-point.Y + placementV) / resolution + maxY;
        return coordinates;
    }
    public Point TransformNodeScreenPoint(double[] coordinates)
    {
        var bildPunkt = new Point
        {
            X = coordinates[0] * resolution + placementH,
            Y = (-coordinates[1] + maxY) * resolution + placementV
        };
        return bildPunkt;
    }
}