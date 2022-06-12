using FE_Analysis.Elasticity.ModelData;
using FE_Analysis.Elasticity.ModelDataRead;
using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace FE_Analysis.Elasticity.ModelDataShow
{
    public partial class ElasticityDataShow
    {
        private readonly FeModel model;
        private string removeKey;

        public ElasticityDataShow(FeModel model)
        {
            Language = XmlLanguage.GetLanguage("us-US");
            this.model = model;
            InitializeComponent();
        }

        private void DatenLoaded(object sender, RoutedEventArgs e)
        {
            // Nodes
            var node = model.Nodes.Select(item => item.Value).ToList();
            NodeGrid.ItemsSource = node;

            // Elements
            var elements = model.Elements.Select(item => item.Value).ToList();
            ElementGrid.ItemsSource = elements;

            // Material
            var material = model.Material.Select(item => item.Value).ToList();
            MaterialGrid.Items.Clear();
            MaterialGrid.ItemsSource = material;

            // CrossSections
            var crossSection = model.CrossSection.Select(item => item.Value).ToList();
            CrossSectionGrid.Items.Clear();
            CrossSectionGrid.ItemsSource = crossSection;

            // Loads
            var nodeLoad = model.Loads.Select(item => item.Value).ToList();
            NodeLoadGrid.Items.Clear();
            NodeLoadGrid.ItemsSource = nodeLoad;

            // Support Conditions
            var boundary = new Dictionary<string, SupportCondition>();
            foreach (var item in model.BoundaryConditions)
            {
                var nodeId = item.Value.NodeId;
                var supportName = item.Value.SupportId;
                string[] prescribed = { "free", "free", "free" };

                switch (item.Value.Type)
                {
                    case 1:
                        {
                            if (item.Value.Restrained[0]) prescribed[0] = item.Value.Prescribed[0].ToString("F4");
                            if (model.SpatialDimension == 2) prescribed[2] = string.Empty;
                            break;
                        }
                    case 2:
                        {
                            if (item.Value.Restrained[1]) prescribed[1] = item.Value.Prescribed[1].ToString("F4");
                            if (model.SpatialDimension == 2) prescribed[2] = string.Empty;
                            break;
                        }
                    case 3:
                        {
                            if (item.Value.Restrained[0]) prescribed[0] = item.Value.Prescribed[0].ToString("F4");
                            if (item.Value.Restrained[1]) prescribed[1] = item.Value.Prescribed[1].ToString("F4");
                            if (model.SpatialDimension == 2) prescribed[2] = string.Empty;
                            break;
                        }
                    case 4:
                        {
                            if (item.Value.Restrained[2]) prescribed[2] = item.Value.Prescribed[2].ToString("F4");
                            break;
                        }
                    case 5:
                        {
                            if (item.Value.Restrained[0]) prescribed[0] = item.Value.Prescribed[0].ToString("F4");
                            if (item.Value.Restrained[2]) prescribed[2] = item.Value.Prescribed[2].ToString("F4");
                            break;
                        }
                    case 6:
                        {
                            if (item.Value.Restrained[1]) prescribed[1] = item.Value.Prescribed[1].ToString("F4");
                            if (item.Value.Restrained[2]) prescribed[2] = item.Value.Prescribed[2].ToString("F4");
                            break;
                        }
                    case 7:
                        {
                            if (item.Value.Restrained[0]) prescribed[0] = item.Value.Prescribed[0].ToString("F4");
                            if (item.Value.Restrained[1]) prescribed[1] = item.Value.Prescribed[1].ToString("F4");
                            if (item.Value.Restrained[2]) prescribed[2] = item.Value.Prescribed[2].ToString("F4");
                            break;
                        }
                    default:
                        throw new ModelException("Boundary Condition for Support " + supportName + " wrong defined");
                }

                var support = new SupportCondition(item.Key, nodeId, prescribed);
                boundary.Add(item.Key, support);
            }
            var supportCondition = boundary.Select(item => item.Value).ToList();
            SupportGrid.Items.Clear();
            SupportGrid.ItemsSource = supportCondition;
        }

        // Nodes
        private void NewNode(object sender, MouseButtonEventArgs e)
        {
            const int numberNodalDegreesOfFreedom = 3;
            _ = new NewNode(model, numberNodalDegreesOfFreedom);
            MainWindow.analysed = false;
            Close();
        }

        //UnloadingRow
        private void NodeRowDelete(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            model.Nodes.Remove(removeKey);
            MainWindow.analysed = false;
            Close();

            var structure = new ElasticityDataShow(model);
            structure.Show();
        }

        //SelectionChanged
        private void NodeRowSelected(object sender, SelectionChangedEventArgs e)
        {
            if (NodeGrid.SelectedCells.Count <= 0) return;
            var cellInfo = NodeGrid.SelectedCells[0];
            var node = (Node)cellInfo.Item;
            removeKey = node.Id;
        }

        // Elemente
        private void NewElement(object sender, MouseButtonEventArgs e)
        {
            _ = new NewElement(model);
            MainWindow.analysed = false;
            Close();
        }

        //UnloadingRow
        private void ElementRowDelete(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            model.Elements.Remove(removeKey);
            MainWindow.analysed = false;
            Close();

            var structure = new ElasticityDataShow(model);
            structure.Show();
        }

        //SelectionChanged
        private void ElementRowSelected(object sender, SelectionChangedEventArgs e)
        {
            if (ElementGrid.SelectedCells.Count <= 0) return;
            var cellInfo = ElementGrid.SelectedCells[0];
            var element = (AbstractElement)cellInfo.Item;
            removeKey = element.ElementId;
        }

        // Material
        private void NewMaterial(object sender, MouseButtonEventArgs e)
        {
            _ = new NewMaterial(model);
            Close();
        }

        //UnloadingRow
        private void MaterialRowDelete(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            model.Material.Remove(removeKey);
            MainWindow.analysed = false;
            Close();

            var structure = new ElasticityDataShow(model);
            structure.Show();
        }

        //SelectionChanged
        private void MaterialRowSelected(object sender, SelectionChangedEventArgs e)
        {
            if (MaterialGrid.SelectedCells.Count <= 0) return;
            var cellInfo = MaterialGrid.SelectedCells[0];
            var material = (Material)cellInfo.Item;
            removeKey = material.MaterialId;
        }

        // CrossSection
        private void NewCrossSection(object sender, MouseButtonEventArgs e)
        {
            _ = new NewCrossSection(model);
            Close();
        }

        //UnloadingRow
        private void CrossSectionRowDelete(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            model.CrossSection.Remove(removeKey);
            MainWindow.analysed = false;
            Close();

            var structure = new ElasticityDataShow(model);
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

        // Loads
        private void NewNodeLoad(object sender, MouseButtonEventArgs e)
        {
            _ = new NewNodeLoad(model, string.Empty, string.Empty, 0, 0, 0);
            MainWindow.analysed = false;
            Close();
        }

        //UnloadingRow
        private void NodeLoadRowDelete(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            model.Loads.Remove(removeKey);
            MainWindow.analysed = false;
            Close();

            var tragwerk = new ElasticityDataShow(model);
            tragwerk.Show();
        }

        //SelectionChanged
        private void NodeLoadRowSelected(object sender, SelectionChangedEventArgs e)
        {
            if (NodeLoadGrid.SelectedCells.Count <= 0) return;
            var cellInfo = NodeLoadGrid.SelectedCells[0];
            var knotenlast = (AbstractLoad)cellInfo.Item;
            removeKey = knotenlast.LoadId;
        }

        // Boundary Conditions
        private void NewBoundaryCondition(object sender, MouseButtonEventArgs e)
        {
            _ = new NewSupport(model);
            MainWindow.analysed = false;
            Close();
        }

        //UnloadingRow.
        private void BoundaryConditionRowDelete(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            model.BoundaryConditions.Remove(removeKey);
            MainWindow.analysed = false;
            Close();

            var structure = new ElasticityDataShow(model);
            structure.Show();
        }

        //SelectionChanged
        private void BoundaryConditionRowSelected(object sender, SelectionChangedEventArgs e)
        {
            if (SupportGrid.SelectedCells.Count <= 0) return;
            var name = (SupportCondition)SupportGrid.SelectedCells[0].Item;
            removeKey = name.SupportId;
        }

        private void Model_Changed(object sender, DataGridCellEditEndingEventArgs e)
        {
            MainWindow.analysed = false;
        }

    }
}