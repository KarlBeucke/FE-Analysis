﻿using FE_Analysis.Heat_Transfer.Model_Data;
using FE_Analysis.Heat_Transfer.ModelDataRead;
using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FE_Analysis.Heat_Transfer.ModelDataShow
{
    public partial class HeatDataShow
    {
        private readonly FeModel model;
        private string removeKey;
        private Shape lastElement;
        private Shape lastNode;

        public HeatDataShow(FeModel model)
        {
            Language = XmlLanguage.GetLanguage("us-US");
            this.model = model;
            InitializeComponent();
            DataContext = this.model;
        }

        private void Nodes_Loaded(object sender, RoutedEventArgs e)
        {
            var node = model.Nodes.Select(item => item.Value).ToList();
            NodesGrid = sender as DataGrid;
            if (NodesGrid != null) NodesGrid.ItemsSource = node;
        }
        private void NewNode(object sender, MouseButtonEventArgs e)
        {
            _ = new NodeNew(model);
            MainWindow.analysed = false;
            Close();
        }
        //UnloadingRow
        private void NodeRowRemove(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            model.Nodes.Remove(removeKey);
            MainWindow.analysed = false;
            Close();

            var heat = new HeatDataShow(model);
            heat.Show();
        }
        //SelectionChanged
        private void NodeRowSelected(object sender, SelectionChangedEventArgs e)
        {
            if (NodesGrid.SelectedCells.Count <= 0) return;
            var cellInfo = NodesGrid.SelectedCells[0];
            var node = (Node)cellInfo.Item;
            removeKey = node.Id;
            if (lastNode != null)
            {
                MainWindow.heatModelVisual.VisualHeatModel.Children.Remove(lastNode);
            }
            lastNode = MainWindow.heatModelVisual.presentation.NodeIndicate(node, Brushes.Green, 1);
        }
        private void NoNodeSelected(object sender, RoutedEventArgs e)
        {
            MainWindow.heatModelVisual.VisualHeatModel.Children.Remove(lastNode);
        }

        private void Elements_Loaded(object sender, RoutedEventArgs e)
        {
            var elemente = model.Elements.Select(item => item.Value).ToList();
            ElementGrid = sender as DataGrid;
            if (ElementGrid == null) return;
            ElementGrid.Items.Clear();
            ElementGrid.ItemsSource = elemente;
        }
        private void NewElement(object sender, MouseButtonEventArgs e)
        {
            _ = new ElementNew(model);
            MainWindow.analysed = false;
            Close();
        }
        //UnloadingRow
        private void ElementRowRemove(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            model.Elements.Remove(removeKey);
            MainWindow.analysed = false;
            Close();

            var heat = new HeatDataShow(model);
            heat.Show();
        }
        //SelectionChanged
        private void ElementRowSelected(object sender, SelectionChangedEventArgs e)
        {
            if (ElementGrid.SelectedCells.Count <= 0) return;
            var cellInfo = ElementGrid.SelectedCells[0];
            var element = (AbstractElement)cellInfo.Item;
            removeKey = element.ElementId;
            if (lastElement != null)
            {
                MainWindow.heatModelVisual.VisualHeatModel.Children.Remove(lastElement);
            }
            lastElement = MainWindow.heatModelVisual.presentation.ElementFillDraw((Abstract2D)element,
                Brushes.Black, Colors.Green, .2, 2);
        }
        private void NoElementSelected(object sender, RoutedEventArgs e)
        {
            MainWindow.heatModelVisual.VisualHeatModel.Children.Remove(lastElement);
        }

        private void Material_Loaded(object sender, RoutedEventArgs e)
        {
            var material = model.Material.Select(item => item.Value).ToList();
            MaterialGrid = sender as DataGrid;
            if (MaterialGrid == null) return;
            MaterialGrid.Items.Clear();
            MaterialGrid.ItemsSource = material;
        }
        private void NewMaterial(object sender, MouseButtonEventArgs e)
        {
            _ = new MaterialNew(model);
            Close();
        }
        //UnloadingRow
        private void MaterialRowRemove(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            model.Material.Remove(removeKey);
            MainWindow.analysed = false;
            Close();

            var heat = new HeatDataShow(model);
            heat.Show();
        }
        //SelectionChanged
        private void MaterialRowSelected(object sender, SelectionChangedEventArgs e)
        {
            if (MaterialGrid.SelectedCells.Count <= 0) return;
            var cellInfo = MaterialGrid.SelectedCells[0];
            var material = (Material)cellInfo.Item;
            removeKey = material.MaterialId;
        }

        private void BoundaryConditions_Loaded(object sender, RoutedEventArgs e)
        {
            var rand = model.BoundaryConditions.Select(item => item.Value).ToList();
            BoundaryConditionGrid = sender as DataGrid;
            if (BoundaryConditionGrid != null) BoundaryConditionGrid.ItemsSource = rand;
        }
        private void NewBoundaryCondition(object sender, MouseButtonEventArgs e)
        {
            _ = new BoundaryConditionNew(model);
            MainWindow.analysed = false;
            Close();
        }
        //UnloadingRow
        private void BoundaryConditionRowRemove(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            model.BoundaryConditions.Remove(removeKey);
            MainWindow.analysed = false;
            Close();

            var heat = new HeatDataShow(model);
            heat.Show();
        }
        //SelectionChanged
        private void BoundaryConditionRowSelected(object sender, SelectionChangedEventArgs e)
        {
            if (BoundaryConditionGrid.SelectedCells.Count <= 0) return;
            var cellInfo = BoundaryConditionGrid.SelectedCells[0];
            var support = (BoundaryCondition)cellInfo.Item;
            removeKey = support.SupportId;
        }

        private void NodalInfluences_Loaded(object sender, RoutedEventArgs e)
        {
            var lasten = model.Loads.Select(item => item.Value).ToList();
            NodalInfluenceGrid = sender as DataGrid;
            if (NodalInfluenceGrid != null) NodalInfluenceGrid.ItemsSource = lasten;
        }
        private void NewNodeLoad(object sender, MouseButtonEventArgs e)
        {
            _ = new NodeLoadNew(model);
            MainWindow.analysed = false;
            Close();
        }
        //UnloadingRow
        private void NodeLoadRowRemove(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            model.Loads.Remove(removeKey);
            MainWindow.analysed = false;
            Close();

            var heat = new HeatDataShow(model);
            heat.Show();
        }
        //SelectionChanged
        private void NodeLoadRowSelected(object sender, SelectionChangedEventArgs e)
        {
            if (NodalInfluenceGrid.SelectedCells.Count <= 0) return;
            var cellInfo = NodalInfluenceGrid.SelectedCells[0];
            var last = (NodeLoad)cellInfo.Item;
            removeKey = last.LoadId;
        }

        private void LineLoads_Loaded(object sender, RoutedEventArgs e)
        {
            var lasten = model.LineLoads.Select(item => item.Value).Cast<AbstractLoad>().ToList();
            LineInfluencesGrid = sender as DataGrid;
            if (LineInfluencesGrid != null) LineInfluencesGrid.ItemsSource = lasten;
        }
        private void NewLineLoad(object sender, MouseButtonEventArgs e)
        {
            _ = new LineLoadNew(model);
            MainWindow.analysed = false;
            Close();
        }
        //UnloadingRow
        private void LineLoadRowRemove(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            model.LineLoads.Remove(removeKey);
            MainWindow.analysed = false;
            Close();

            var heat = new HeatDataShow(model);
            heat.Show();
        }
        //SelectionChanged
        private void LineLoadRowSelected(object sender, SelectionChangedEventArgs e)
        {
            if (LineInfluencesGrid.SelectedCells.Count <= 0) return;
            var cellInfo = LineInfluencesGrid.SelectedCells[0];
            var last = (LineLoad)cellInfo.Item;
            removeKey = last.LoadId;
        }

        private void ElementLoads_Loaded(object sender, RoutedEventArgs e)
        {
            var loads = model.ElementLoads.Select(item => item.Value).Cast<AbstractLoad>().ToList();
            ElementInfluencesGrid = sender as DataGrid;
            if (ElementInfluencesGrid != null) ElementInfluencesGrid.ItemsSource = loads;
        }
        private void NewElementLoad(object sender, MouseButtonEventArgs e)
        {
            _ = new ElementLoadNew(model);
            MainWindow.analysed = false;
            Close();
        }
        //UnloadingRow
        private void ElementLoadRowRemove(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            model.ElementLoads.Remove(removeKey);
            MainWindow.analysed = false;
            Close();

            var heat = new HeatDataShow(model);
            heat.Show();
        }
        //SelectionChanged
        private void ElementLoadRowSelected(object sender, SelectionChangedEventArgs e)
        {
            if (ElementInfluencesGrid.SelectedCells.Count <= 0) return;
            var cellInfo = ElementInfluencesGrid.SelectedCells[0];
            var load = (AbstractElementLoad)cellInfo.Item;
            removeKey = load.LoadId;
        }

        private void Model_Changed(object sender, DataGridCellEditEndingEventArgs e)
        {
            MainWindow.analysed = false;
        }

        //private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        //{
        //    // ... hol die TextBox, die editiert wurde
        //    var element = e.EditingElement as TextBox;
        //    var text = element.Text;

        //    // ... pruef, ob die Textveraenderung abgelehnt werden soll
        //    // ... Ablehnung, falls der Nutzer ein ? eingibt
        //    if (text == "?")
        //    {
        //        Title = "Invalid";
        //        e.Cancel = true;
        //    }
        //    else
        //    {
        //        // ... zeige den Zellenwert im Titel
        //        Title = "Eingabe: " + text;
        //    }
        //}
    }
}