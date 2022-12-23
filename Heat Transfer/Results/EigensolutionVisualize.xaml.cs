using FEALibrary.Model;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using static System.Windows.Controls.Canvas;
using static System.Windows.FontWeights;
using static System.Windows.Media.Brushes;

namespace FE_Analysis.Heat_Transfer.Results
{
    public partial class EigensolutionVisualize
    {
        private const int BorderLeft = 60;
        private readonly FeModel model;
        private double resolution, maxY;
        private Presentation presentation;
        private int index;
        private bool nodalTemperaturesOn;

        public EigensolutionVisualize(FeModel model)
        {
            Language = XmlLanguage.GetLanguage("us-US");
            this.model = model;
            InitializeComponent();
            NodalTemperatures = new List<object>();
            Eigenvalues = new List<object>();
        }

        public List<object> NodalTemperatures { get; set; }
        public List<object> Eigenvalues { get; set; }

        private void ModelGrid_Loaded(object sender, RoutedEventArgs e)
        {
            // choose specific Eigensolution
            var numberEigenstate = model.Eigenstate.NumberOfStates;
            var eigenstateNr = new int[numberEigenstate];
            for (var i = 0; i < numberEigenstate; i++) eigenstateNr[i] = i + 1;
            EigensolutionChoice.ItemsSource = eigenstateNr;

            presentation = new Presentation(model, VisualResults);
            presentation.EvaluateResolution();
            maxY = presentation.maxY;
            resolution = presentation.resolution;
            presentation.AllElementsDraw();
        }

        // Combobox event
        private void DropDownEigensolutionChoiceClosed(object sender, EventArgs e)
        {
            index = EigensolutionChoice.SelectedIndex;
        }

        // Button event
        private void BtnEigensolution_Click(object sender, RoutedEventArgs e)
        {
            //Toggle Nodal Temperatures
            if (!nodalTemperaturesOn)
            {
                // draw value of each boundary condition as text toboundary node
                Eigenstate_Draw(model.Eigenstate.Eigenvectors[index]);
                nodalTemperaturesOn = true;

                var eigenvalue = new TextBlock
                {
                    FontSize = 14,
                    Text = "Eigenvalue Nr. " + (index + 1) + " = " + model.Eigenstate.Eigenvalues[index].ToString("N2"),
                    Foreground = Blue
                };
                SetTop(eigenvalue, -10);
                SetLeft(eigenvalue, BorderLeft);
                VisualResults.Children.Add(eigenvalue);
                Eigenvalues.Add(eigenvalue);
            }
            else
            {
                // remove ALL temperature texts from nodes
                foreach (var nodalTemp in NodalTemperatures)
                    VisualResults.Children.Remove(nodalTemp as TextBlock);
                foreach (TextBlock eigenvalue in Eigenvalues) VisualResults.Children.Remove(eigenvalue);
                nodalTemperaturesOn = false;
            }
        }

        private void Eigenstate_Draw(double[] state)
        {
            double maxTemp = 0, minTemp = 100;
            foreach (var item in model.Nodes)
            {
                var node = item.Value;
                var temperature = state[node.SystemIndices[0]].ToString("N2");
                var temp = state[node.SystemIndices[0]];
                if (temp > maxTemp) maxTemp = temp;
                if (temp < minTemp) minTemp = temp;
                var fensterKnoten = TransformNode(node, resolution, maxY);

                var id = new TextBlock
                {
                    FontSize = 12,
                    Background = Red,
                    FontWeight = Bold,
                    Text = temperature
                };
                NodalTemperatures.Add(id);
                SetTop(id, fensterKnoten[1]);
                SetLeft(id, fensterKnoten[0]);
                VisualResults.Children.Add(id);
            }
        }

        private int[] TransformNode(Node knoten, double res, double mY)
        {
            resolution = res;
            maxY = mY;
            var screenNode = new int[2];
            screenNode[0] = (int)(knoten.Coordinates[0] * resolution);
            screenNode[1] = (int)(-knoten.Coordinates[1] * resolution + maxY);
            return screenNode;
        }
    }
}