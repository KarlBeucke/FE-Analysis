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

namespace FE_Analysis.Heat_Transfer
{
    public class Presentation
    {
        private readonly FeModel model;
        private AbstractElement currentElement;
        private Node node;
        private readonly Canvas visualResults;
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
        private const int BorderTop = 60;
        private const int BorderLeft = 60;

        //private double vectorScaling, vectorLength, vectorAngle;

        public Presentation(FeModel feModel, Canvas visual)
        {
            model = feModel;
            visualResults = visual;
            NodeIDs = new List<object>();
            ElementIDs = new List<object>();
            LoadNodes = new List<object>();
            LoadElements = new List<object>();
            NodalTemperatures = new List<TextBlock>();
            NodalGradients = new List<TextBlock>();
            TemperatureElements = new List<Shape>();
            HeatVectors = new List<object>();
            BoundaryNode = new List<object>();
            EvaluateResolution();
        }

        public List<object> ElementIDs { get; }
        public List<object> NodeIDs { get; }
        public List<object> LoadNodes { get; }
        public List<object> LoadElements { get; }
        public List<TextBlock> NodalTemperatures { get; }
        public List<TextBlock> NodalGradients { get; }
        public List<Shape> TemperatureElements { get; }
        public List<object> HeatVectors { get; }
        public List<object> BoundaryNode { get; }

        public void EvaluateResolution()
        {
            const int rand = 100;
            screenH = visualResults.ActualWidth;
            screenV = visualResults.ActualHeight;

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
                    Foreground = Red
                };
                SetTop(id, (-item.Value.Coordinates[1] + maxY) * resolution + BorderTop);
                SetLeft(id, item.Value.Coordinates[0] * resolution + BorderLeft);
                visualResults.Children.Add(id);
                NodeIDs.Add(id);
            }
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
                visualResults.Children.Add(id);
                ElementIDs.Add(id);
            }
        }

        public void ElementsDraw()
        {
            foreach (var path in from item in model.Elements
                                 select item.Value
                     into element
                                 let pathGeometry = CurrentElementDraw(element)
                                 select new Path
                                 {
                                     Name = element.ElementId,
                                     Stroke = Black,
                                     StrokeThickness = 1,
                                     Data = pathGeometry
                                 })
            {
                // setz oben/links Position zum Zeichnen auf dem Canvas
                SetLeft(path, BorderLeft);
                SetTop(path, BorderTop);
                visualResults.Children.Add(path);
            }
        }

        private PathGeometry CurrentElementDraw(AbstractElement element)
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
            foreach (var item in model.Loads)
            {
                var knotenId = item.Value.NodeId;
                var lastWert = item.Value.Intensity[0];
                if (model.Nodes.TryGetValue(knotenId, out node)) { }

                var lastPunkt = TransformNode(node, resolution, maxY);
                var knotenLast = new TextBlock
                {
                    FontSize = 12,
                    Text = lastWert.ToString(CurrentCulture),
                    Foreground = Red
                };
                SetTop(knotenLast, lastPunkt.Y + BorderTop + 10);
                SetLeft(knotenLast, lastPunkt.X + BorderLeft);

                LoadNodes.Add(knotenLast);
                visualResults.Children.Add(knotenLast);
            }
        }

        public void ElementLoadDraw()
        {
            foreach (var item in model.ElementLoads)
            {
                if (model.Elements.TryGetValue(item.Value.ElementId, out var element))
                {
                }

                var pathGeometry = CurrentElementDraw((Abstract2D)element);

                var mySolidColorBrush = new SolidColorBrush(Colors.Red) { Opacity = .2 };
                var lastElement = new Path
                {
                    Stroke = Black,
                    StrokeThickness = 1,
                    Fill = mySolidColorBrush,
                    Data = pathGeometry
                };
                LoadElements.Add(lastElement);
                // setz oben/links Position zum Zeichnen auf dem Canvas
                SetLeft(lastElement, BorderLeft);
                SetTop(lastElement, BorderTop);
                // zeichne Shape
                visualResults.Children.Add(lastElement);
            }
        }

        public void BoundaryConditionDraw()
        {
            // zeichne den Wert einer jeden Randbedingung als Text an Randknoten
            foreach (var item in model.BoundaryConditions)
            {
                var knotenId = item.Value.NodeId;
                if (model.Nodes.TryGetValue(knotenId, out node))
                {
                }

                var displayNode = TransformNode(node, resolution, maxY);

                var boundaryValue = item.Value.Prescribed[0];
                var boundaryCondition = new TextBlock
                {
                    Name = "Support",
                    Uid = item.Value.SupportId,
                    FontSize = 12,
                    Text = boundaryValue.ToString("N2"),
                    //Foreground = Brushes.DarkOliveGreen
                    Background = LightBlue
                };
                BoundaryNode.Add(boundaryCondition);
                SetTop(boundaryCondition, displayNode.Y + BorderTop + 15);
                SetLeft(boundaryCondition, displayNode.X + BorderLeft);
                visualResults.Children.Add(boundaryCondition);
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
                visualResults.Children.Add(id);
            }
        }

        public void NodalTemperaturesDraw(int index)
        {
            foreach (var item in model.Nodes)
            {
                node = item.Value;
                var temperatur = node.NodalVariables[0][index].ToString("N2");
                temp = node.NodalDof[0];
                if (temp > maxTemp) maxTemp = temp;
                if (temp < minTemp) minTemp = temp;
                var displayNode = TransformNode(node, resolution, maxY);

                var id = new TextBlock
                {
                    FontSize = 12,
                    Background = LightGray,
                    FontWeight = FontWeights.Bold,
                    Text = temperatur
                };
                NodalTemperatures.Add(id);
                SetTop(id, displayNode.Y + BorderTop);
                SetLeft(id, displayNode.X + BorderLeft);
                visualResults.Children.Add(id);
            }
        }

        public void ElementTemperaturesDraw()
        {
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
                var pathGeometry = CurrentElementDraw((Abstract2D)currentElement);
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
                    Fill = myBrush,
                    Data = pathGeometry
                };
                TemperatureElements.Add(path);
                // setz oben/links Position zum Zeichnen auf dem Canvas
                SetLeft(path, BorderLeft);
                SetTop(path, BorderTop);
                // zeichne Shape
                visualResults.Children.Add(path);
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
                visualResults.Children.Add(id);
            }
        }

        public void HeatFlowVectorsDraw()
        {
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
                visualResults.Children.Add(path);
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
            if (double.IsPositiveInfinity(mY)) mY = screenV;
            var nSteps = (int)(tmax / dt) + 1;
            var timeHistory = new Polyline
            {
                Stroke = Red,
                StrokeThickness = 2
            };
            var supportPoints = new PointCollection();
            for (var i = 0; i < nSteps; i++)
            {
                var point = new Point(dt * i * resolutionH, -ordinates[i] * resolutionV);
                supportPoints.Add(point);
            }

            timeHistory.Points = supportPoints;

            SetLeft(timeHistory, BorderLeft);
            SetTop(timeHistory, mY * resolutionV + BorderTop);
            // draw Shape
            visualResults.Children.Add(timeHistory);
        }

        public void CoordinateSystem(double tmin, double tmax, double max, double min)
        {
            const int border = 20;
            const int maxOrdinateDisplay = 100;
            screenH = visualResults.ActualWidth;
            screenV = visualResults.ActualHeight;
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
            visualResults.Children.Add(xAchse);
            var yAchse = new Line
            {
                Stroke = Black,
                X1 = BorderLeft,
                Y1 = max * resolutionV - min * resolutionV + 2 * BorderTop,
                X2 = BorderLeft,
                Y2 = BorderTop,
                StrokeThickness = 2
            };
            visualResults.Children.Add(yAchse);
        }

        private static Point TransformNode(Node node, double resolution, double maxY)
        {
            return new Point(node.Coordinates[0] * resolution, (-node.Coordinates[1] + maxY) * resolution);
        }
    }
}