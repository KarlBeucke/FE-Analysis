using FE_Analysis.Structural_Analysis.Model_Data;
using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using static System.Windows.Controls.Canvas;
using static System.Windows.Media.Brushes;

namespace FE_Analysis.Structural_Analysis.Results
{
    public partial class EigensolutionVisualize
    {
        private const int BorderTop = 60;
        private const int BorderLeft = 60;
        private readonly double resolution;
        private readonly double maxY;
        private readonly FeModel model;
        public Presentation presentation;
        private double eigenformScaling;
        private int index;
        private Node node;
        public double screenH, screenV;
        private bool deformationsOn;

        public EigensolutionVisualize(FeModel feModel)
        {
            Language = XmlLanguage.GetLanguage("us-US");
            model = feModel;
            InitializeComponent();
            Deformations = new List<object>();
            Eigenfrequencies = new List<object>();
            Show();

            // number of Eigensolution
            var numberOfEigenforms = model.Eigenstate.NumberOfStates;
            var eigenformNr = new int[numberOfEigenforms];
            for (var i = 0; i < numberOfEigenforms; i++) eigenformNr[i] = i + 1;
            presentation = new Presentation(model, VisualResults);
            presentation.EvaluateResolution();
            maxY = presentation.maxY;
            resolution = presentation.resolution;
            presentation.UndeformedGeometry();
            EigensolutionSelection.ItemsSource = eigenformNr;

            eigenformScaling = double.Parse("10");
            TxtScaling.Text = eigenformScaling.ToString(CultureInfo.CurrentCulture);
        }

        public List<object> Deformations { get; set; }
        public List<object> Eigenfrequencies { get; set; }

        // ComboBox
        private void DropDownEigenformauswahlClosed(object sender, EventArgs e)
        {
            index = EigensolutionSelection.SelectedIndex;
        }

        // Button events
        private void BtnGeometry_Click(object sender, RoutedEventArgs e)
        {
            presentation.UndeformedGeometry();
        }

        private void BtnEigenform_Click(object sender, RoutedEventArgs e)
        {
            Toggle_Eigenform();
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return) return;
            eigenformScaling = double.Parse(TxtScaling.Text);
            Toggle_Eigenform();
        }

        public void Toggle_Eigenform()
        {
            if (!deformationsOn)
            {
                var pathGeometry = EigenformDraw(model.Eigenstate.Eigenvectors[index]);

                Shape path = new Path
                {
                    Stroke = Red,
                    StrokeThickness = 2,
                    Data = pathGeometry
                };
                SetLeft(path, BorderLeft);
                SetTop(path, BorderTop);
                // draw Shape
                VisualResults.Children.Add(path);
                Deformations.Add(path);
                deformationsOn = true;

                var value = Math.Sqrt(model.Eigenstate.Eigenvalues[index]) / 2 / Math.PI;
                var eigenfrequenz = new TextBlock
                {
                    FontSize = 14,
                    Text = "Eigenfrequency Nr. " + (index + 1) + " = " + value.ToString("N2"),
                    Foreground = Blue
                };
                SetTop(eigenfrequenz, -BorderTop + SteuerLeiste.Height);
                SetLeft(eigenfrequenz, BorderLeft);
                VisualResults.Children.Add(eigenfrequenz);
                Eigenfrequencies.Add(eigenfrequenz);
            }
            else
            {
                foreach (Shape path in Deformations) VisualResults.Children.Remove(path);
                foreach (TextBlock eigenfrequenz in Eigenfrequencies) VisualResults.Children.Remove(eigenfrequenz);
                deformationsOn = false;
            }
        }

        public PathGeometry EigenformDraw(double[] zustand)
        {
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
                Point start, end;
                double startWinkel, endWinkel;

                switch (element)
                {
                    case Truss _:
                        {
                            if (model.Nodes.TryGetValue(element.NodeIds[0], out node))
                            {
                            }

                            start = TransformNode(node, zustand, resolution, maxY);
                            pathFigure.StartPoint = start;

                            for (var i = 1; i < element.NodeIds.Length; i++)
                            {
                                if (model.Nodes.TryGetValue(element.NodeIds[i], out node))
                                {
                                }

                                end = TransformNode(node, zustand, resolution, maxY);
                                pathFigure.Segments.Add(new LineSegment(end, true));
                            }

                            break;
                        }
                    case Beam _:
                        {
                            if (model.Nodes.TryGetValue(element.NodeIds[0], out node))
                            {
                            }

                            start = TransformNode(node, zustand, resolution, maxY);
                            pathFigure.StartPoint = start;
                            startWinkel = -zustand[node.SystemIndices[2]] * 180 / Math.PI;

                            for (var i = 1; i < element.NodeIds.Length; i++)
                            {
                                if (model.Nodes.TryGetValue(element.NodeIds[i], out node))
                                {
                                }

                                end = TransformNode(node, zustand, resolution, maxY);
                                var richtung = end - start;
                                richtung.Normalize();

                                richtung = RotateVectorScreen(richtung, startWinkel);
                                var control1 = start + richtung * element.length / 4 * resolution;
                                richtung = start - end;
                                richtung.Normalize();

                                endWinkel = -zustand[node.SystemIndices[2]] * 180 / Math.PI;
                                richtung = RotateVectorScreen(richtung, endWinkel);
                                var control2 = end + richtung * element.length / 4 * resolution;

                                pathFigure.Segments.Add(new BezierSegment(control1, control2, end, true));
                            }

                            break;
                        }
                    case BeamHinged _:
                        {
                            if (model.Nodes.TryGetValue(element.NodeIds[0], out node))
                            {
                            }

                            start = TransformNode(node, zustand, resolution, maxY);
                            pathFigure.StartPoint = start;
                            startWinkel = -zustand[node.SystemIndices[2]] * 180 / Math.PI;

                            var control = start;
                            for (var i = 1; i < element.NodeIds.Length; i++)
                            {
                                if (model.Nodes.TryGetValue(element.NodeIds[i], out node))
                                {
                                }

                                end = TransformNode(node, zustand, resolution, maxY);
                                endWinkel = -zustand[node.SystemIndices[2]] * 180 / Math.PI;

                                Vector richtung;
                                switch (element.Type)
                                {
                                    case 1:
                                        richtung = start - end;
                                        richtung.Normalize();
                                        richtung = RotateVectorScreen(richtung, endWinkel);
                                        control = end + richtung * element.length / 4 * resolution;
                                        break;
                                    case 2:
                                        richtung = end - start;
                                        richtung.Normalize();
                                        richtung = RotateVectorScreen(richtung, startWinkel);
                                        control = start + richtung * element.length / 4 * resolution;
                                        break;
                                }

                                pathFigure.Segments.Add(new QuadraticBezierSegment(control, end, true));
                            }

                            break;
                        }
                }

                if (element.NodeIds.Length > 2) pathFigure.IsClosed = true;
                pathGeometry.Figures.Add(pathFigure);
            }

            return pathGeometry;
        }

        public Point TransformNode(Node modellKnoten, double[] state, double res, double max)
        {
            var screenNode = new int[2];
            screenNode[0] = (int)(modellKnoten.Coordinates[0] * res +
                                     state[modellKnoten.SystemIndices[0]] * eigenformScaling);
            screenNode[1] = (int)((-modellKnoten.Coordinates[1] + max) * res -
                                     state[modellKnoten.SystemIndices[1]] * eigenformScaling);
            var punkt = new Point(screenNode[0], screenNode[1]);
            return punkt;
        }

        public static Vector RotateVectorScreen(Vector vec, double ang) // clockwise in degree
        {
            var vector = vec;
            var angle = ang * Math.PI / 180;
            var rotated = new Vector(vector.X * Math.Cos(angle) - vector.Y * Math.Sin(angle),
                vector.X * Math.Sin(angle) + vector.Y * Math.Cos(angle));
            return rotated;
        }
    }
}