using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace FE_Analysis.Structural_Analysis.Results
{
    public partial class StaticResultsShow
    {
        private readonly FeModel model;

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