using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FE_Analysis.Structural_Analysis.Results
{
    public partial class StaticResultsShow
    {
        private readonly FeModel model;
        private Shape lastElement;
        private Shape lastNode;

        public StaticResultsShow(FeModel feModel)
        {
            Language = XmlLanguage.GetLanguage("us-US");
            model = feModel;
            InitializeComponent();
        }

        private void NodeDeformations_Loaded(object sender, RoutedEventArgs e)
        {
            NodeDeformationsGrid.ItemsSource = model.Nodes;
        }
        //SelectionChanged
        private void NodeRowSelected(object sender, SelectionChangedEventArgs e)
        {
            if (NodeDeformationsGrid.SelectedCells.Count <= 0) return;
            var cellInfo = NodeDeformationsGrid.SelectedCells[0];
            var cell = (KeyValuePair<string, Node>)cellInfo.Item;
            var node = cell.Value;
            if (lastNode != null)
            {
                MainWindow.staticResults.VisualResults.Children.Remove(lastNode);
            }
            lastNode = MainWindow.staticResults.presentation.NodeIndicate(node, Brushes.Green, 1);
        }
        private void NoNodeSelected(object sender, RoutedEventArgs e)
        {
            MainWindow.staticResults.VisualResults.Children.Remove(lastNode);
        }

        private void ElementEndForces_Loaded(object sender, RoutedEventArgs e)
        {
            var elementForces = new List<BeamEndForces>();
            foreach (var item in model.Elements)
            {
                if (!(item.Value is AbstractBeam beam)) continue;
                double[] beamEndForces = beam.ComputeElementState();
                elementForces.Add(new BeamEndForces(beam.ElementId, beamEndForces));
            }

            ElementEndForcesGrid.ItemsSource = elementForces;
        }
        // SelectionChanged
        private void ElementRowSelected(object sender, SelectionChangedEventArgs e)
        {
            if (ElementEndForcesGrid.SelectedCells.Count <= 0) return;
            var cellInfo = ElementEndForcesGrid.SelectedCells[0];
            var beamEndForces = (BeamEndForces)cellInfo.Item;
            if (!model.Elements.TryGetValue(beamEndForces.ElementId, out var element)) return;
            if (lastElement != null)
            {
                MainWindow.staticResults.VisualResults.Children.Remove(lastElement);
            }
            lastElement = MainWindow.staticResults.presentation.ElementDraw(element, Brushes.Green, 5);
        }
        private void NoElementSelected(object sender, RoutedEventArgs e)
        {
            MainWindow.staticResults.VisualResults.Children.Remove(lastElement);
        }

        private void SupportReactions_Loaded(object sender, RoutedEventArgs e)
        {
            var knotenReaktionen = new Dictionary<string, NodalReactions>();
            foreach (var item in model.BoundaryConditions)
            {
                var knotenId = item.Value.NodeId;
                if (!model.Nodes.TryGetValue(knotenId, out var knoten)) break;
                var knotenReaktion = new NodalReactions(knoten.Reactions);
                knotenReaktionen.Add(knotenId, knotenReaktion);
            }

            SupportReactionsGrid = sender as DataGrid;
            if (SupportReactionsGrid != null) SupportReactionsGrid.ItemsSource = knotenReaktionen;
        }

        internal class NodalReactions
        {
            public NodalReactions(double[] reactions)
            {
                Reactions = reactions;
            }
            public double[] Reactions { get; }
        }
    }
}