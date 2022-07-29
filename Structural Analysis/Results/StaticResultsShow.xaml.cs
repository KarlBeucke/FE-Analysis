using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace FE_Analysis.Structural_Analysis.Results
{
    public partial class StaticResultsShow
    {
        private readonly FeModel model;
        private Node lastNode;

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
        private void KnotenZeileSelected(object sender, SelectionChangedEventArgs e)
        {
            if (NodeDeformationsGrid.SelectedCells.Count <= 0) return;
            var cellInfo = NodeDeformationsGrid.SelectedCells[0];
            var cell = (KeyValuePair<string, Node>)cellInfo.Item;
            var node = cell.Value;
            if (lastNode != null)
            {
                MainWindow.staticResults.presentation.NodeIndicate(lastNode, Brushes.White, 2);
            }
            MainWindow.staticResults.presentation.NodeIndicate(node, Brushes.Red, 1);
            lastNode = node;
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