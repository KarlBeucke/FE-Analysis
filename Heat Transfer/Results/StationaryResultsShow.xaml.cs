using FE_Analysis.Heat_Transfer.Model_Data;
using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace FE_Analysis.Heat_Transfer.Results
{
    public partial class StationaryResultsShow
    {
        private readonly FeModel model;

        public StationaryResultsShow(FeModel feModel)
        {
            Language = XmlLanguage.GetLanguage("us-US");
            model = feModel;
            InitializeComponent();
        }

        private void Nodes_Loaded(object sender, RoutedEventArgs e)
        {
            NodesGrid = sender as DataGrid;
            if (NodesGrid != null) NodesGrid.ItemsSource = model.Nodes;
        }

        private void TemperatureVectors_Loaded(object sender, RoutedEventArgs e)
        {
            TemperatureVectorsGrid = sender as DataGrid;
            foreach (var item in model.Elements)
                switch (item.Value)
                {
                    case Abstract2D value:
                        {
                            var element = value;
                            element.ElementState = element.ComputeElementState(0, 0);
                            break;
                        }
                    case Element3D8 value:
                        {
                            var element3d8 = value;
                            element3d8.HeatStatus = element3d8.ComputeElementState(0, 0, 0);
                            break;
                        }
                }

            if (TemperatureVectorsGrid != null) TemperatureVectorsGrid.ItemsSource = model.Elements;
        }

        private void HeatFlow_Loaded(object sender, RoutedEventArgs e)
        {
            HeatFlowGrid = sender as DataGrid;
            if (HeatFlowGrid != null) HeatFlowGrid.ItemsSource = model.BoundaryConditions;
        }
    }
}