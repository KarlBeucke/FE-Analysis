using System.Collections.Generic;
using FE_Analysis.Heat_Transfer.Model_Data;
using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FE_Analysis.Heat_Transfer.Results
{
    public partial class StationaryResultsShow
    {
        private readonly FeModel model;
        private Shape lastElement;
        private Shape lastNode;

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
        private void NodeRowSelected(object sender, SelectionChangedEventArgs e)
        {
            if (NodesGrid.SelectedCells.Count <= 0) return;
            var cellInfo = NodesGrid.SelectedCells[0];
            var cell = (KeyValuePair<string, Node>)cellInfo.Item;
            var node = cell.Value;
            if (lastNode != null)
            {
                MainWindow.stationaryResults.VisualResults.Children.Remove(lastNode);
            }
            lastNode = MainWindow.stationaryResults.presentation.NodeIndicate(node, Brushes.Green, 1);
        }
        private void NoNodeSelected(object sender, RoutedEventArgs e)
        {
            MainWindow.stationaryResults.VisualResults.Children.Remove(lastNode);
            lastElement = null;
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
                            element3d8.ComputeElementState(0, 0, 0);
                            break;
                        }
                }

            if (TemperatureVectorsGrid != null) TemperatureVectorsGrid.ItemsSource = model.Elements;
        }
        private void ElementRowSelected(object sender, SelectionChangedEventArgs e)
        {
            if (TemperatureVectorsGrid.SelectedCells.Count <= 0) return;
            var cellInfo = TemperatureVectorsGrid.SelectedCells[0];
            var cell = (KeyValuePair<string, AbstractElement>)cellInfo.Item;
            var element = cell.Value;
            if (lastElement != null)
            {
                MainWindow.stationaryResults.VisualResults.Children.Remove(lastElement);
            }
            lastElement = MainWindow.stationaryResults.presentation.ElementFillDraw((Abstract2D)element,
                Brushes.Black, Colors.Green, .2, 2);
        }
        private void NoElementSelected(object sender, RoutedEventArgs e)
        {
            MainWindow.stationaryResults.VisualResults.Children.Remove(lastElement);
            lastNode = null;
        }

        private void HeatFlow_Loaded(object sender, RoutedEventArgs e)
        {
            HeatFlowGrid = sender as DataGrid;
            if (HeatFlowGrid != null) HeatFlowGrid.ItemsSource = model.BoundaryConditions;
        }
    }
}