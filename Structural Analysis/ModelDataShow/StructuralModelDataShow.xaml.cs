using FE_Analysis.Structural_Analysis.ModelDataRead;
using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FE_Analysis.Structural_Analysis.ModelDataShow
{
    public partial class StructuralModelDataShow
    {
        private readonly FeModel model;
        private string removeKey;
        private Shape lastElement;
        private Shape lastNode;

        public StructuralModelDataShow(FeModel feModel)
        {
            Language = XmlLanguage.GetLanguage("us-US");
            model = feModel;
            InitializeComponent();
            lastElement = null;
        }

        private void Nodes_Loaded(object sender, RoutedEventArgs e)
        {
            var knoten = model.Nodes.Select(item => item.Value).ToList();
            NodesGrid = sender as DataGrid;
            if (NodesGrid != null) NodesGrid.ItemsSource = knoten;
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

            var structure = new StructuralModelDataShow(model);
            structure.Show();
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
                MainWindow.structuralModel.VisualModel.Children.Remove(lastNode);
            }
            lastNode = MainWindow.structuralModel.presentation.NodeIndicate(node, Brushes.Green, 1);
        }
        //LostFocus
        private void NoNodeSelected(object sender, RoutedEventArgs e)
        {
            MainWindow.structuralModel.VisualModel.Children.Remove(lastNode);
        }

        private void ElementsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var elemente = model.Elements.Select(item => item.Value).ToList();
            ElementGrid = sender as DataGrid;
            if (ElementGrid != null) ElementGrid.ItemsSource = elemente;
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

            var structure = new StructuralModelDataShow(model);
            structure.Show();
        }
        //SelectionChanged
        private void ElementRowSelected(object sender, SelectionChangedEventArgs e)
        {
            if (ElementGrid.SelectedCells.Count <= 0) return;
            var cellInfo = ElementGrid.SelectedCells[0];
            var element = (Abstract2D)cellInfo.Item;
            removeKey = element.ElementId;
            if (lastElement != null)
            {
                MainWindow.structuralModel.VisualModel.Children.Remove(lastElement);
            }
            lastElement = MainWindow.structuralModel.presentation.ElementDraw(element, Brushes.Green, 5);
        }
        private void NoElementSelected(object sender, RoutedEventArgs e)
        {
            MainWindow.structuralModel.VisualModel.Children.Remove(lastElement);
        }

        private void Material_Loaded(object sender, RoutedEventArgs e)
        {
            var material = model.Material.Select(item => item.Value).ToList();
            MaterialGrid = sender as DataGrid;
            if (MaterialGrid != null) MaterialGrid.ItemsSource = material;
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

            var structure = new StructuralModelDataShow(model);
            structure.Show();
        }
        //SelectionChanged
        private void MaterialRowSelected(object sender, SelectionChangedEventArgs e)
        {
            if (MaterialGrid.SelectedCells.Count <= 0) return;
            var cellInfo = MaterialGrid.SelectedCells[0];
            var material = (Model_Data.Material)cellInfo.Item;
            removeKey = material.MaterialId;
        }

        private void CrossSection_Loaded(object sender, RoutedEventArgs e)
        {
            var crossSection = model.CrossSection.Select(item => item.Value).ToList();
            CrossSectionGrid = sender as DataGrid;
            if (CrossSectionGrid != null) CrossSectionGrid.ItemsSource = crossSection;
        }
        private void NewCrossSection(object sender, MouseButtonEventArgs e)
        {
            _ = new CrossSectionNew(model);
            Close();
        }
        //UnloadingRow
        private void CrossSectionRowRemove(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            model.CrossSection.Remove(removeKey);
            MainWindow.analysed = false;
            Close();

            var structure = new StructuralModelDataShow(model);
            structure.Show();
        }
        //SelectionChanged
        private void CrossSectionRowSelected(object sender, SelectionChangedEventArgs e)
        {
            if (CrossSectionGrid.SelectedCells.Count <= 0) return;
            var cellInfo = CrossSectionGrid.SelectedCells[0];
            var crossSection = (CrossSection)cellInfo.Item;
            removeKey = crossSection.CrossSectionId;
        }

        private void Lager_Loaded(object sender, RoutedEventArgs e)
        {
            var support = new List<AbstractBoundaryCondition>();
            foreach (var item in model.BoundaryConditions)
            {
                for (var i = 0; i < item.Value.Prescribed.Length; i++)
                    //if (!item.Value.Restrained[i]) item.Value.Prescribed[i] = Double.NaN;
                    if (!item.Value.Restrained[i])
                        item.Value.Prescribed[i] = double.PositiveInfinity;
                support.Add(item.Value);
            }

            SupportGrid = sender as DataGrid;
            if (SupportGrid != null) SupportGrid.ItemsSource = support;
        }
        private void NewSupport(object sender, MouseButtonEventArgs e)
        {
            const double vorX = 0, vorY = 0, vorRot = 0;
            _ = new SupportNew(model, vorX, vorY, vorRot);
            MainWindow.analysed = false;
            Close();
        }
        //UnloadingRow
        private void SupportRowRemove(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            model.BoundaryConditions.Remove(removeKey);
            MainWindow.analysed = false;
            Close();

            var structure = new StructuralModelDataShow(model);
            structure.Show();
        }
        //SelectionChanged
        private void SupportRowSelected(object sender, SelectionChangedEventArgs e)
        {
            if (SupportGrid.SelectedCells.Count <= 0) return;
            var cellInfo = SupportGrid.SelectedCells[0];
            var support = (Model_Data.Support)cellInfo.Item;
            removeKey = support.SupportId;
        }

        private void NodeLoads_Loaded(object sender, RoutedEventArgs e)
        {
            var loads = model.Loads.Select(item => item.Value).ToList();
            NodeLoadsGrid = sender as DataGrid;
            if (NodeLoadsGrid != null) NodeLoadsGrid.ItemsSource = loads;
        }
        private void NewNodeLoad(object sender, MouseButtonEventArgs e)
        {
            _ = new NodeLoadNew(model, string.Empty, string.Empty, 0, 0, 0);
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

            var structure = new StructuralModelDataShow(model);
            structure.Show();
        }
        //SelectionChanged
        private void NodeLoadRowSelected(object sender, SelectionChangedEventArgs e)
        {
            if (NodeLoadsGrid.SelectedCells.Count <= 0) return;
            var cellInfo = NodeLoadsGrid.SelectedCells[0];
            var nodeLoad = (AbstractLoad)cellInfo.Item;
            removeKey = nodeLoad.LoadId;
        }

        private void PointLoads_Loaded(object sender, RoutedEventArgs e)
        {
            var loads = model.PointLoads.Select(item => item.Value).ToList();
            PointLoadsGrid = sender as DataGrid;
            if (PointLoadsGrid != null) PointLoadsGrid.ItemsSource = loads;
        }
        private void NewPointLoad(object sender, MouseButtonEventArgs e)
        {
            _ = new PointLoadNew(model, string.Empty, string.Empty, 0, 0, 0);
            MainWindow.analysed = false;
            Close();
        }
        //UnloadingRow
        private void PointLoadRowRemove(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            model.PointLoads.Remove(removeKey);
            MainWindow.analysed = false;
            Close();

            var structure = new StructuralModelDataShow(model);
            structure.Show();
        }
        //SelectionChanged
        private void PointLoadRowSelected(object sender, SelectionChangedEventArgs e)
        {
            if (PointLoadsGrid.SelectedCells.Count <= 0) return;
            var cellInfo = PointLoadsGrid.SelectedCells[0];
            var pointLoad = (AbstractElementLoad)cellInfo.Item;
            removeKey = pointLoad.LoadId;
        }

        private void LinenLoads_Loaded(object sender, RoutedEventArgs e)
        {
            var lasten = model.ElementLoads.Select(item => item.Value).ToList();
            LineLoadsGrid = sender as DataGrid;
            if (LineLoadsGrid != null) LineLoadsGrid.ItemsSource = lasten;
        }
        private void NewLineLoad(object sender, MouseButtonEventArgs e)
        {
            _ = new LineLoadNew(model, string.Empty, string.Empty, 0, 0, 0, 0, false);
            MainWindow.analysed = false;
            Close();
        }
        //UnloadingRow
        private void LineLoadRowRemove(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            model.ElementLoads.Remove(removeKey);
            MainWindow.analysed = false;
            Close();

            var structure = new StructuralModelDataShow(model);
            structure.Show();
        }
        //SelectionChanged
        private void LineLoadsRowSelected(object sender, RoutedEventArgs e)
        {
            if (LineLoadsGrid.SelectedCells.Count <= 0) return;
            var cellInfo = LineLoadsGrid.SelectedCells[0];
            var lineLoad = (Model_Data.LineLoad)cellInfo.Item;
            removeKey = lineLoad.LoadId;
        }

        private void Model_Changed(object sender, DataGridCellEditEndingEventArgs e)
        {
            MainWindow.analysed = false;
        }
    }
}