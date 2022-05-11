using FEALibrary.Model;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace FE_Analysis.Elasticity.Results
{
    public partial class StaticResultsShow
    {
        private readonly FeModel model;

        public StaticResultsShow(FeModel feModel)
        {
            Language = XmlLanguage.GetLanguage("us-US");
            model = feModel;
            InitializeComponent();
            DataContext = this;
        }

        private void NodeDeformations_Loaded(object sender, RoutedEventArgs e)
        {
            NodeDeformationGrid = sender as DataGrid;
            if (NodeDeformationGrid != null) NodeDeformationGrid.ItemsSource = model.Nodes;
        }

        private void ElementStressesGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var elementStresses = new Dictionary<string, ElementStress>();
            foreach (var item in model.Elements)
            {
                var elementStress = new ElementStress(item.Value.ComputeStateVector());
                elementStresses.Add(item.Key, elementStress);
            }

            ElementStressesGrid = sender as DataGrid;
            if (ElementStressesGrid != null) ElementStressesGrid.ItemsSource = elementStresses;
        }

        private void ReactionGrid_Loaded(object sender, RoutedEventArgs e)
        {
            ReactionGrid = sender as DataGrid;
            if (ReactionGrid != null) ReactionGrid.ItemsSource = model.BoundaryConditions;
        }

        internal class ElementStress
        {
            public ElementStress(double[] stresses)
            {
                Stresses = stresses;
            }

            public double[] Stresses { get; }
        }

        internal class NodeReactions
        {
            public NodeReactions(double[] reactions)
            {
                Reactions = reactions;
            }

            public double[] Reactions { get; }
        }
    }
}