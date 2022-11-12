using FE_Analysis.Elasticity.ModelData;
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

namespace FE_Analysis.Elasticity
{
    public class Presentation
    {
        private const double Eps = 1.0E-10;
        private const int BorderLeft = 60;
        private const double PlacementText = 45;
        private readonly double maxScreenLength = 40;
        private readonly FeModel model;
        private readonly Canvas visualResults;
        private double resolution;
        private double resolutionH;
        private double loadResolution;
        private double minX, maxX, minY, maxY;
        private Node node;
        private int borderTop = 60;
        private double screenH, screenV;
        public double scalingDisplacement = 1;
        private double vectorScaling;

        public Presentation(FeModel feModel, Canvas visual)
        {
            model = feModel;
            visualResults = visual;
            ElementIDs = new List<object>();
            NodeIDs = new List<object>();
            Deformations = new List<object>();
            LoadVectors = new List<object>();
            SupportRepresentation = new List<object>();
            Stresses = new List<object>();
            Reactions = new List<object>();
            DefinitionOfResolution();
        }

        public List<object> ElementIDs { get; }
        public List<object> NodeIDs { get; }
        public List<object> Deformations { get; }
        public List<object> LoadVectors { get; }
        public List<object> SupportRepresentation { get; }
        public List<object> Stresses { get; }
        public List<object> Reactions { get; }

        private void DefinitionOfResolution()
        {
            screenH = visualResults.ActualWidth;
            screenV = visualResults.ActualHeight;

            var x = new List<double>();
            var y = new List<double>();
            foreach (var item in model.Nodes)
            {
                x.Add(item.Value.Coordinates[0]);
                maxX = x.Max();
                minX = x.Min();
                y.Add(item.Value.Coordinates[1]);
                maxY = y.Max();
                minY = y.Min();
            }

            var delta = Math.Abs(maxX - minX);
            if (delta < 1)
                resolutionH = screenH - 5 * BorderLeft;
            else
                resolutionH = (screenH - 5 * BorderLeft) / delta;

            delta = Math.Abs(maxY - minY);
            if (delta < 1)
            {
                resolution = screenV - 5 * borderTop;
                borderTop = (int)(0.5 * screenV);
            }
            else
            {
                resolution = (screenV - 5 * borderTop) / delta;
            }

            if (resolutionH < resolution) resolution = resolutionH;
        }

        public void NodalTexts()
        {
            foreach (var item in model.Nodes)
            {
                var id = new TextBlock
                {
                    Name = item.Key,
                    FontSize = 12,
                    Text = item.Key,
                    Foreground = Red
                };
                SetTop(id, (-item.Value.Coordinates[1] + maxY) * resolution + PlacementText);
                SetLeft(id, item.Value.Coordinates[0] * resolution + BorderLeft);
                visualResults.Children.Add(id);
                NodeIDs.Add(id);
            }
        }

        public void ElementTexts()
        {
            foreach (var item in model.Elements)
            {
                var element = (Abstract2D)item.Value;
                var cg = element.ComputeCenterOfGravity();
                var id = new TextBlock
                {
                    Name = item.Key,
                    FontSize = 12,
                    Text = item.Key,
                    Foreground = Blue
                };
                SetTop(id, (-cg.Y + maxY) * resolution + PlacementText);
                SetLeft(id, cg.X * resolution + BorderLeft);
                visualResults.Children.Add(id);
                ElementIDs.Add(id);
            }
        }

        public void ElementsDraw()
        {
            foreach (var item in model.Elements) CurrentElementDraw(item.Value);
        }

        private void PolygonDraw(AbstractElement elementMultiNodes, PointCollection outline)
        {
            var elementPolygon = new Polygon
            {
                Name = elementMultiNodes.ElementId,
                Stroke = Black,
                Points = outline,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                StrokeThickness = 2
            };
            SetLeft(elementPolygon, BorderLeft);
            SetTop(elementPolygon, borderTop);
            visualResults.Children.Add(elementPolygon);
        }

        private void CurrentElementDraw(AbstractElement element)
        {
            // node at element start
            if (model.Nodes.TryGetValue(element.NodeIds[0], out var transformNode)) { }

            var startPoint = TransformNode(transformNode, resolution, maxY);

            switch (element)
            {
                // elements with ultiple nodes
                default:
                    {
                        // PointCollection for polygon representation
                        var elementPointCollection = new PointCollection { startPoint };
                        for (var i = 1; i < element.NodeIds.Length; i++)
                        {
                            if (model.Nodes.TryGetValue(element.NodeIds[i], out transformNode)) { }

                            var endPoint = TransformNode(transformNode, resolution, maxY);
                            elementPointCollection.Add(endPoint);
                        }

                        PolygonDraw(element, elementPointCollection);
                        return;
                    }
            }
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

            //int scaling = 1;
            //const int rotationScaling = 1;
            var pathGeometry = new PathGeometry();

            IEnumerable<AbstractElement> Elements()
            {
                foreach (var item in model.Elements)
                    if (item.Value is AbstractElement element)
                        yield return element;
            }

            foreach (var element in Elements())
            {
                //element.ElementState = element.ComputeElementState();
                var pathFigure = new PathFigure();

                switch (element)
                {
                    case Element2D3 _:
                        {
                            if (model.Nodes.TryGetValue(element.NodeIds[0], out node)) { }

                            var start = TransformDeformedNode(node, resolution, maxY);
                            pathFigure.StartPoint = start;

                            for (var i = 1; i < element.NodeIds.Length; i++)
                            {
                                if (model.Nodes.TryGetValue(element.NodeIds[i], out node)) { }

                                var end = TransformDeformedNode(node, resolution, maxY);
                                pathFigure.Segments.Add(new LineSegment(end, true));
                            }

                            break;
                        }
                }

                if (element.NodeIds.Length > 2) pathFigure.IsClosed = true;
                pathGeometry.Figures.Add(pathFigure);
            }

            // all elements are added to GeometryGroup structure
            var structure = new GeometryGroup();
            structure.Children.Add(pathGeometry);

            Shape path = new Path
            {
                Stroke = Red,
                StrokeThickness = 1,
                Data = structure
            };
            // set top/left position for drawing on Canvas
            SetLeft(path, BorderLeft);
            SetTop(path, borderTop);
            // draw Shape
            visualResults.Children.Add(path);
            Deformations.Add(path);
        }

        public void LoadsDraw()
        {
            AbstractLoad load;
            Shape path;

            // Nodeloads
            double maxLoadValue = 1;
            const int maxLoadScreen = 50;
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

            foreach (var lineLoad in model.ElementLoads.Select(item => (AbstractLineLoad)item.Value))
            {
                if (Math.Abs(lineLoad.Loadvalues[0]) > maxLoadValue) maxLoadValue = Math.Abs(lineLoad.Loadvalues[0]);
                if (Math.Abs(lineLoad.Loadvalues[1]) > maxLoadValue) maxLoadValue = Math.Abs(lineLoad.Loadvalues[1]);
            }

            loadResolution = maxLoadScreen / maxLoadValue;

            foreach (var item in model.Loads)
            {
                load = item.Value;
                var pathGeometry = NodeLoadDraw(load);
                path = new Path
                {
                    Name = load.LoadId,
                    Stroke = Red,
                    StrokeThickness = 3,
                    Data = pathGeometry
                };
                LoadVectors.Add(path);

                SetLeft(path, BorderLeft);
                SetTop(path, borderTop);
                visualResults.Children.Add(path);
            }

            foreach (var item in model.ElementLoads)
            {
                var lineLoad = (AbstractLineLoad)item.Value;
                var pathGeometry = LineLoadDraw(lineLoad);
                var red = FromArgb(60, 255, 0, 0);
                var blue = FromArgb(60, 0, 0, 255);
                var myBrush = new SolidColorBrush(red);
                if (lineLoad.Loadvalues[1] > 0) myBrush = new SolidColorBrush(blue);
                path = new Path
                {
                    Name = lineLoad.LoadId,
                    Fill = myBrush,
                    Stroke = Red,
                    StrokeThickness = 1,
                    Data = pathGeometry
                };
                LoadVectors.Add(path);

                SetLeft(path, BorderLeft);
                SetTop(path, borderTop);
                visualResults.Children.Add(path);
            }
        }

        private PathGeometry NodeLoadDraw(AbstractLoad nodeLoad)
        {
            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure();
            const int loadArrowSize = 10;

            if (model.Nodes.TryGetValue(nodeLoad.NodeId, out var loadNode)) { }

            if (loadNode != null)
            {
                var endPoint = new Point(
                    loadNode.Coordinates[0] * resolution - nodeLoad.Loadvalues[0] * loadResolution,
                    (-loadNode.Coordinates[1] + maxY) * resolution + nodeLoad.Loadvalues[1] * loadResolution);
                pathFigure.StartPoint = endPoint;

                var startPoint = TransformNode(loadNode, resolution, maxY);
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

                if (nodeLoad.Loadvalues.Length > 2 && Math.Abs(nodeLoad.Loadvalues[2]) > Eps)
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

        private PathGeometry LineLoadDraw(AbstractElementLoad load)
        {
            var lineLoad = (LineLoad)load;
            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure();
            const int loadArrowSize = 8;
            const int lineLoadScaling = 1;
            var lineLoadResolution = lineLoadScaling * loadResolution;

            load.SetReferences(model);
            if (model.Elements.TryGetValue(lineLoad.ElementId, out var element)) { }

            if (element == null) return pathGeometry;

            if (model.Nodes.TryGetValue(element.NodeIds[0], out var startNode)) { }

            var startPoint = TransformNode(startNode, resolution, maxY);

            // second element node
            if (model.Nodes.TryGetValue(element.NodeIds[1], out var endNode)) { }

            var endPoint = TransformNode(endNode, resolution, maxY);
            var vector = endPoint - startPoint;

            pathFigure.StartPoint = startPoint;

            var loadVector = RotateVectorScreen(vector, -90);
            loadVector.Normalize();
            var vec = loadVector * lineLoadResolution * lineLoad.Loadvalues[1];
            var nextPoint = new Point(startPoint.X - vec.X, startPoint.Y - vec.Y);

            loadVector *= loadArrowSize;
            loadVector = RotateVectorScreen(loadVector, -150);
            var point = new Point(startPoint.X - loadVector.X, startPoint.Y - loadVector.Y);
            pathFigure.Segments.Add(new LineSegment(point, true));

            loadVector = RotateVectorScreen(loadVector, -60);
            point = new Point(startPoint.X - loadVector.X, startPoint.Y - loadVector.Y);
            pathFigure.Segments.Add(new LineSegment(point, false));
            pathFigure.Segments.Add(new LineSegment(startPoint, true));
            pathFigure.Segments.Add(new LineSegment(nextPoint, true));

            loadVector = RotateVectorScreen(vector, 90);
            loadVector.Normalize();
            vec = loadVector * lineLoadResolution * lineLoad.Loadvalues[1];
            nextPoint = new Point(endPoint.X + vec.X, endPoint.Y + vec.Y);
            pathFigure.Segments.Add(new LineSegment(nextPoint, true));
            pathFigure.Segments.Add(new LineSegment(endPoint, true));

            loadVector *= loadArrowSize;
            loadVector = RotateVectorScreen(loadVector, 30);
            nextPoint = new Point(endPoint.X - loadVector.X, endPoint.Y - loadVector.Y);
            pathFigure.Segments.Add(new LineSegment(nextPoint, true));

            loadVector = RotateVectorScreen(loadVector, -60);
            nextPoint = new Point(endPoint.X - loadVector.X, endPoint.Y - loadVector.Y);
            pathFigure.Segments.Add(new LineSegment(nextPoint, false));
            pathFigure.Segments.Add(new LineSegment(endPoint, true));
            pathFigure.Segments.Add(new LineSegment(startPoint, false));

            pathGeometry.Figures.Add(pathFigure);

            return pathGeometry;
        }

        public void SupportDraw()
        {
            foreach (var item in model.BoundaryConditions)
            {
                var support = item.Value;
                var pathGeometry = new PathGeometry();

                if (model.Nodes.TryGetValue(support.NodeId, out var supportNode)) { }

                var pivotPoint = TransformNode(supportNode, resolution, maxY);

                switch (support.Type)
                {
                    // X_FIXED = 1, Y_FIXED = 2, R_FIXED = 4, XY_FIXED = 3, 
                    // XR_FIXED = 5, YR_FIXED = 6, XYR_FIXED = 7
                    case 1:
                        {
                            pathGeometry = SingleSupportDraw(supportNode);
                            double rotationAngle = 45;
                            if (supportNode != null &&
                                supportNode.Coordinates[0] - minX < maxX - supportNode.Coordinates[0]) rotationAngle = -45;
                            pathGeometry.Transform = new RotateTransform(rotationAngle, pivotPoint.X, pivotPoint.Y);
                            break;
                        }
                    case 2:
                        pathGeometry = SingleSupportDraw(supportNode);
                        break;
                    case 3:
                        pathGeometry = DoubleSupportDraw(supportNode);
                        break;
                }

                Shape path = new Path
                {
                    Stroke = Green,
                    StrokeThickness = 2,
                    Data = pathGeometry
                };
                SupportRepresentation.Add(path);

                // set top/left position for drawing on Canvas
                SetLeft(path, BorderLeft);
                SetTop(path, borderTop);
                // draw Shape
                visualResults.Children.Add(path);
            }
        }

        private PathGeometry SingleSupportDraw(Node lagerKnoten)
        {
            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure();
            const int supportSymbol = 20;

            var startPoint = TransformNode(lagerKnoten, resolution, maxY);
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

        private PathGeometry DoubleSupportDraw(Node supportNode)
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

        public void StressesDraw()
        {
            double[] elementStresses;
            double maxVector = 0;
            foreach (var abstract2D in model.Elements.Select(item => (Abstract2D)item.Value))
            {
                elementStresses = abstract2D.ComputeStateVector();
                maxVector = elementStresses.Select(Math.Abs).Prepend(maxVector).Max();
            }

            vectorScaling = maxScreenLength / maxVector;

            foreach (var abstract2D in model.Elements.Select(item => (Abstract2D)item.Value))
            {
                elementStresses = abstract2D.ComputeStateVector();
                var sigxx = elementStresses[0] * vectorScaling;
                var sigyy = elementStresses[1] * vectorScaling;
                var cg = abstract2D.ComputeCenterOfGravity();
                // draw resulting stress vector centered at element center of gravity
                // add arrow heads to endpoint and add heat flow arrow to pathGeometry
                StressesElements(cg, sigxx, sigyy);
            }
        }

        private void StressesElements(Point cg, double sigxx, double sigyy)
        {
            var midPoint = new Point(cg.X * resolution, (-cg.Y + maxY) * resolution);

            // stress arrow in x-direction
            var color = Black;
            var angle = 0.0;
            var length = Math.Abs(sigxx);
            if (sigxx < 0)
            {
                color = Red;
                angle = 180.0;
            }

            if ((int)length > 1)
            {
                var pathGeometry = StressArrow(midPoint, length, angle);
                Shape path = new Path
                {
                    Stroke = color,
                    StrokeThickness = 2,
                    Data = pathGeometry
                };
                Stresses.Add(path);

                // set top/left position for drawing on Canvas
                SetLeft(path, BorderLeft);
                SetTop(path, borderTop);
                // draw Shape
                visualResults.Children.Add(path);
            }

            // stress arrow in y-direction
            color = Black;
            angle = -90.0;
            length = Math.Abs(sigyy);
            if (sigyy < 0)
            {
                color = Red;
                angle = 90.0;
            }

            if ((int)length <= 1) return;
            {
                var pathGeometry = StressArrow(midPoint, sigyy, angle);
                Shape path = new Path
                {
                    Stroke = color,
                    StrokeThickness = 2,
                    Data = pathGeometry
                };
                Stresses.Add(path);

                // set top/left position for drawing on Canvas
                SetLeft(path, BorderLeft);
                SetTop(path, borderTop);
                // draw Shape
                visualResults.Children.Add(path);
            }
        }

        private static PathGeometry StressArrow(Point point, double length, double angle)
        {
            var stressArrow = new PathGeometry();
            var pathFigure = new PathFigure { StartPoint = point };
            var endPoint = new Point(point.X + Math.Abs(length), point.Y);
            pathFigure.Segments.Add(new LineSegment(endPoint, true));
            pathFigure.Segments.Add(new LineSegment(new Point(endPoint.X - 3, endPoint.Y - 2), true));
            pathFigure.Segments.Add(new LineSegment(new Point(endPoint.X - 3, endPoint.Y + 2), true));
            pathFigure.Segments.Add(new LineSegment(new Point(endPoint.X, endPoint.Y), true));

            stressArrow.Figures.Add(pathFigure);
            stressArrow.Transform = new RotateTransform(angle, point.X, point.Y);
            return stressArrow;
        }

        public void ReactionsDraw()
        {
            double[] reactions;
            double maxVector = 0;
            var nodeIds = new List<string>();
            foreach (var boundaryCondition in model.BoundaryConditions.Select(item => item.Value))
            {
                if (nodeIds.Contains(boundaryCondition.NodeId)) break;
                nodeIds.Add(boundaryCondition.NodeId);
                if (!model.Nodes.TryGetValue(boundaryCondition.NodeId, out node)) break;
                reactions = node.Reactions;
                maxVector = reactions.Select(Math.Abs).Prepend(maxVector).Max();
            }

            const double maxArrowSize = 50;
            vectorScaling = maxArrowSize / maxVector;

            foreach (var boundaryCondition in model.BoundaryConditions.Select(item => item.Value))
            {
                if (!model.Nodes.TryGetValue(boundaryCondition.NodeId, out node)) break;
                reactions = node.Reactions;
                var kx = reactions[0] * vectorScaling;
                var ky = reactions[1] * vectorScaling;
                node = boundaryCondition.Node;
                NodalReactions(node, kx, ky);
            }
        }

        private void NodalReactions(Node supportNode, double kx, double ky)
        {
            var point = new Point(supportNode.Coordinates[0] * resolution,
                (-supportNode.Coordinates[1] + maxY) * resolution);
            var color = Black;

            // reaction arrow in x-direction
            if (Math.Abs(kx) > 5)
            {
                var reactionArrow = ReactionArrow(point, Math.Abs(kx));
                if (kx < 0)
                {
                    reactionArrow.Transform = new RotateTransform(180, point.X + kx / 2, point.Y);
                    color = Red;
                }

                Shape path = new Path
                {
                    Stroke = color,
                    StrokeThickness = 3,
                    Data = reactionArrow
                };
                Reactions.Add(path);

                // // set top/left position for drawing on Canvas
                SetLeft(path, BorderLeft);
                SetTop(path, borderTop);
                // draw Shape
                visualResults.Children.Add(path);
            }

            // reaction arrow in y-direction
            if (!(Math.Abs(ky) > 5)) return;
            {
                var reactionArrow = ReactionArrow(point, ky);
                if (ky > 0)
                {
                    reactionArrow.Transform = new RotateTransform(-90, point.X, point.Y);
                    color = Black;
                }
                else
                {
                    reactionArrow.Transform = new RotateTransform(90, point.X, point.Y);
                    color = Red;
                }

                Shape path = new Path
                {
                    Stroke = color,
                    StrokeThickness = 4,
                    Data = reactionArrow
                };
                Reactions.Add(path);

                // // set top/left position for drawing on Canvas
                SetLeft(path, BorderLeft);
                SetTop(path, borderTop);
                // draw Shape
                visualResults.Children.Add(path);
            }
        }

        private static PathGeometry ReactionArrow(Point point, double length)
        {
            var reactionArrow = new PathGeometry();

            var pathFigure = new PathFigure { StartPoint = new Point(point.X - Math.Abs(length), point.Y) };
            pathFigure.Segments.Add(new LineSegment(point, true));
            pathFigure.Segments.Add(new LineSegment(new Point(point.X - 3, point.Y - 2), true));
            pathFigure.Segments.Add(new LineSegment(new Point(point.X - 3, point.Y + 2), true));
            pathFigure.Segments.Add(new LineSegment(point, true));

            reactionArrow.Figures.Add(pathFigure);
            return reactionArrow;
        }

        private static Vector RotateVectorScreen(Vector vec, double winkel) // clockwise in degree
        {
            var vector = vec;
            var angle = winkel * Math.PI / 180;
            return new Vector(vector.X * Math.Cos(angle) - vector.Y * Math.Sin(angle),
                vector.X * Math.Sin(angle) + vector.Y * Math.Cos(angle));
        }

        private static Point TransformNode(Node node, double resolution, double maxY)
        {
            return new Point(node.Coordinates[0] * resolution, (-node.Coordinates[1] + maxY) * resolution);
        }

        private Point TransformDeformedNode(Node transformNode, double transformResolution, double max)
        {
            // unit of input e.g. in m, unit of deformation e.g.in  cm --> Scaling
            return new Point((transformNode.Coordinates[0] + transformNode.NodalDof[0] * scalingDisplacement) * transformResolution,
                (-transformNode.Coordinates[1] - transformNode.NodalDof[1] * scalingDisplacement + max) * transformResolution);
        }
    }
}