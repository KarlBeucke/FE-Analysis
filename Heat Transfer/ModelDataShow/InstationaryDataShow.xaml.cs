using FE_Analysis.Heat_Transfer.Model_Data;
using FE_Analysis.Heat_Transfer.ModelDataRead;
using FEALibrary.Model;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace FE_Analysis.Heat_Transfer.ModelDataShow
{
    public partial class InstationaryDataShow
    {
        private readonly FeModel model;
        private int removeIndex;
        private string removeKey;

        public InstationaryDataShow(FeModel model)
        {
            Language = XmlLanguage.GetLanguage("us-US");
            this.model = model;
            InitializeComponent();
            DataContext = this.model;
        }

        private void InstationaryLoaded(object sender, RoutedEventArgs e)
        {
            // Initial Conditions
            if (model.Timeintegration != null) All.IsChecked = model.Timeintegration.FromStationary;

            if (model.Timeintegration != null && model.Timeintegration.InitialConditions.Count > 0)
            {
                var initialTemperatures = model.Timeintegration.InitialConditions.Cast<NodalValues>().ToList();
                InitialTemperatureGrid.ItemsSource = initialTemperatures;
            }

            // Boundary Conditions
            if (model.TimeDependentBoundaryConditions.Count > 0)
            {
                var boundaryFile = (from item
                        in model.TimeDependentBoundaryConditions
                                    where item.Value.VariationType == 0
                                    select item.Value).ToList();
                if (boundaryFile.Count > 0) BoundaryFileGrid.ItemsSource = boundaryFile;

                var boundaryConstant = (from item
                        in model.TimeDependentBoundaryConditions
                                        where item.Value.VariationType == 1
                                        select item.Value).ToList();
                if (boundaryConstant.Count > 0) BoundaryConstantGrid.ItemsSource = boundaryConstant;

                var boundaryHarmonic = (from item
                        in model.TimeDependentBoundaryConditions
                                        where item.Value.VariationType == 2
                                        select item.Value).ToList();
                if (boundaryHarmonic.Count > 0) BoundaryHarmonicGrid.ItemsSource = boundaryHarmonic;

                var boundaryLinear = (from item
                        in model.TimeDependentBoundaryConditions
                                      where item.Value.VariationType == 3
                                      select item.Value).ToList();
                if (boundaryLinear.Count > 0) BoundaryLinearGrid.ItemsSource = boundaryLinear;
            }

            // Nodal temperatures
            if (model.TimeDependentNodeLoads.Count > 0)
            {
                var boundaryFile = (from item
                        in model.TimeDependentNodeLoads
                                    where item.Value.VariationType == 0
                                    select item.Value).ToList();
                if (boundaryFile.Count > 0) NodeFileGrid.ItemsSource = boundaryFile;

                var boundaryHarmonic = (from item
                        in model.TimeDependentNodeLoads
                                        where item.Value.VariationType == 2
                                        select item.Value).ToList();
                if (boundaryHarmonic.Count > 0) NodeHarmonicGrid.ItemsSource = boundaryHarmonic;

                var boundaryLinear = (from item
                        in model.TimeDependentNodeLoads
                                      where item.Value.VariationType == 3
                                      select item.Value).ToList();
                if (boundaryLinear.Count > 0) NodeLinearGrid.ItemsSource = boundaryLinear;
            }

            // Element temperatures
            if (model.TimeDependentElementLoads.Count > 0)
            {
                var elementLoads = (from item
                        in model.TimeDependentElementLoads
                                    where item.Value.VariationType == 1
                                    select item.Value).ToList();
                if (elementLoads.Count > 0) ElementLoadGrid.ItemsSource = elementLoads;
            }
        }

        // ************************* Initial Conditions *********************************
        private void ToggleStationary(object sender, RoutedEventArgs e)
        {
            if (All.IsChecked != null && (bool)All.IsChecked)
            {
                All.IsChecked = true;
                model.Timeintegration.FromStationary = true;
            }
            else
            {
                All.IsChecked = false;
                model.Timeintegration.FromStationary = false;
            }
        }

        private void NewInitialTemperature(object sender, MouseButtonEventArgs e)
        {
            _ = new TimeNewInitialTemperature(model);
            MainWindow.analysed = false;
            Close();
        }

        //UnloadingRow
        private void InitialTemperatureRowRemove(object sender, DataGridRowEventArgs e)
        {
            model.Timeintegration.InitialConditions.RemoveAt(removeIndex);
            MainWindow.analysed = false;
            Close();

            var heat = new InstationaryDataShow(model);
            heat.Show();
        }

        //SelectionChanged
        private void InitialTemperatureRowSelected(object sender, SelectionChangedEventArgs e)
        {
            if (InitialTemperatureGrid.SelectedCells.Count <= 0) return;
            var cellInfo = InitialTemperatureGrid.SelectedCells[0];
            removeIndex = model.Timeintegration.InitialConditions.IndexOf(cellInfo.Item);
        }

        // ************************* Time Dependent Boundary Conditions ***********************
        private void TimeNewBoundaryCondition(object sender, MouseButtonEventArgs e)
        {
            _ = new TimeNewBoundaryCondition(model);
            MainWindow.analysed = false;
            Close();
        }

        //SelectionChanged
        private void BoundaryFileSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BoundaryFileGrid.SelectedCells.Count <= 0) return;
            var cellInfo = BoundaryFileGrid.SelectedCells[0];
            var boundaryCondition = (TimeDependentBoundaryCondition)cellInfo.Item;
            removeKey = boundaryCondition.SupportId;
        }

        private void BoundaryConstantSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BoundaryConstantGrid.SelectedCells.Count <= 0) return;
            var cellInfo = BoundaryConstantGrid.SelectedCells[0];
            var boundaryCondition = (TimeDependentBoundaryCondition)cellInfo.Item;
            removeKey = boundaryCondition.SupportId;
        }

        private void BoundaryHarmonicSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BoundaryHarmonicGrid.SelectedCells.Count <= 0) return;
            var cellInfo = BoundaryHarmonicGrid.SelectedCells[0];
            var boundaryCondition = (TimeDependentBoundaryCondition)cellInfo.Item;
            removeKey = boundaryCondition.SupportId;
        }

        private void BoundaryLinearSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BoundaryLinearGrid.SelectedCells.Count <= 0) return;
            var cellInfo = BoundaryLinearGrid.SelectedCells[0];
            var timeBoundary = (TimeDependentBoundaryCondition)cellInfo.Item;
            removeKey = timeBoundary.SupportId;
        }

        //UnloadingRow
        private void BoundaryFileRowRemove(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            model.TimeDependentBoundaryConditions.Remove(removeKey);
            MainWindow.analysed = false;
            Close();
            var heat = new InstationaryDataShow(model);
            heat.Show();
        }

        private void BoundaryConstantRowRemove(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            model.TimeDependentBoundaryConditions.Remove(removeKey);
            MainWindow.analysed = false;
            Close();
            var heat = new InstationaryDataShow(model);
            heat.Show();
        }

        private void BoundaryHarmonicRowRemove(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            model.TimeDependentBoundaryConditions.Remove(removeKey);
            MainWindow.analysed = false;
            Close();
            var heat = new InstationaryDataShow(model);
            heat.Show();
        }

        private void BoundaryLinearRowRemove(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            model.TimeDependentBoundaryConditions.Remove(removeKey);
            MainWindow.analysed = false;
            Close();
            var heat = new InstationaryDataShow(model);
            heat.Show();
        }

        // ************************* time dependent Nodal Temperatures ********************************
        private void TimeNewNodeLoad(object sender, MouseButtonEventArgs e)
        {
            _ = new TimeNewBoundaryCondition(model);
            MainWindow.analysed = false;
            Close();
        }

        //SelectionChanged
        private void NodeFileSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NodeFileGrid.SelectedCells.Count <= 0) return;
            var cellInfo = NodeFileGrid.SelectedCells[0];
            var last = (TimeDependentNodeLoad)cellInfo.Item;
            removeKey = last.LoadId;
        }

        private void NodeHarmonicSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NodeHarmonicGrid.SelectedCells.Count <= 0) return;
            var cellInfo = NodeHarmonicGrid.SelectedCells[0];
            var last = (TimeDependentNodeLoad)cellInfo.Item;
            removeKey = last.LoadId;
        }

        private void NodeLinearSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NodeLinearGrid.SelectedCells.Count <= 0) return;
            var cellInfo = NodeLinearGrid.SelectedCells[0];
            var last = (TimeDependentNodeLoad)cellInfo.Item;
            removeKey = last.LoadId;
        }

        //UnloadingRow
        private void NodeFileRowRemove(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            model.TimeDependentNodeLoads.Remove(removeKey);
            MainWindow.analysed = false;
            Close();
            var heat = new InstationaryDataShow(model);
            heat.Show();
        }

        private void NodeHarmonicRowRemove(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            model.TimeDependentNodeLoads.Remove(removeKey);
            MainWindow.analysed = false;
            Close();
            var heat = new InstationaryDataShow(model);
            heat.Show();
        }

        private void NodeLinearRowRemove(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            model.TimeDependentNodeLoads.Remove(removeKey);
            MainWindow.analysed = false;
            Close();
            var heat = new InstationaryDataShow(model);
            heat.Show();
        }

        // ************************* time dependent Element Temperatures ********************************
        private void TimeNewElementLoad(object sender, MouseButtonEventArgs e)
        {
            _ = new TimeNewElementLoad(model);
            MainWindow.analysed = false;
            Close();
        }

        //SelectionChanged
        private void ElementLoadSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (removeKey == null) return;
            model.TimeDependentElementLoads.Remove(removeKey);
            MainWindow.analysed = false;
            Close();

            var heat = new InstationaryDataShow(model);
            heat.Show();
        }

        //UnloadingRow
        private void ElementLoadRowRemove(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            model.TimeDependentElementLoads.Remove(removeKey);
            MainWindow.analysed = false;
            Close();

            var heat = new InstationaryDataShow(model);
            heat.Show();
        }

        // ************************* Model wurde verändert ********************************
        private void Model_Changed(object sender, DataGridCellEditEndingEventArgs e)
        {
            MainWindow.analysed = false;
        }
    }
}