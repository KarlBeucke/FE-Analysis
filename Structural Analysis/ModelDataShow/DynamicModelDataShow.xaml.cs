using FE_Analysis.Structural_Analysis.ModelDataRead;
using FE_Analysis.Structural_Analysis.Results;
using FEALibrary.Model;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace FE_Analysis.Structural_Analysis.ModelDataShow
{
    public partial class DynamicModelDataShow
    {
        private readonly FeModel model;
        private int removeIndex;
        private string removeKey;

        public DynamicModelDataShow(FeModel feModel)
        {
            Language = XmlLanguage.GetLanguage("us-US");
            model = feModel;
            InitializeComponent();
            //DataContext for Integration parameter
            DataContext = model;
        }

        private void DynamicLoaded(object sender, RoutedEventArgs e)
        {
            // ************************* Initial Conditions *********************************
            if (model.Timeintegration.InitialConditions.Count > 0)
            {
                var anfangsverformungen = model.Timeintegration.InitialConditions.Cast<NodalValues>().ToList();
                InitialConditionsGrid.ItemsSource = anfangsverformungen;
            }

            // ************************* time dependent Node Loads ***********************
            if (model.TimeDependentNodeLoads.Count > 0)
            {
                var nodeGround = (from item
                        in model.TimeDependentNodeLoads
                                  where item.Value.GroundExcitation
                                  select item.Value).ToList();
                if (nodeGround.Count > 0) Ground.Content = "Ground Excitation";

                var nodeFile = (from item
                        in model.TimeDependentNodeLoads
                                where item.Value.VariationType == 0
                                select item.Value).ToList();
                if (nodeFile.Count > 0) FileGrid.ItemsSource = nodeFile;

                var nodeHarmonic = (from item
                        in model.TimeDependentNodeLoads
                                    where item.Value.VariationType == 2
                                    select item.Value).ToList();

                HarmonicGrid.Items.Clear();
                if (nodeHarmonic.Count > 0) HarmonicGrid.ItemsSource = nodeHarmonic;

                var nodeLinear = (from item
                        in model.TimeDependentNodeLoads
                                  where item.Value.VariationType == 1
                                  select item.Value).ToList();
                if (nodeLinear.Count > 0) LinearGrid.ItemsSource = nodeLinear;
            }

            // ************************* Modal Damping Ratios ***********************
            if (model.Eigenstate.DampingRatios.Count <= 0) return;
            var dampingRatios = model.Eigenstate.DampingRatios.Cast<ModalValues>().ToList();
            dampingRatios[0].Text = dampingRatios.Count == 1 ? "alle Eigenmodes" : "";
            DampingGrid.ItemsSource = dampingRatios;
        }

        // ************************* damping ratios *********************************
        private void NewModalDampingRatio(object sender, MouseButtonEventArgs e)
        {
            _ = new NewTimeDampingRatio(model);
            MainWindow.analysed = false;
            Close();
        }

        //UnloadingRow
        private void DampingRowRemove(object sender, DataGridRowEventArgs e)
        {
            model.Eigenstate.DampingRatios.RemoveAt(removeIndex);
            MainWindow.analysed = false;
            Close();

            var structure = new DynamicModelDataShow(model);
            structure.Show();
        }

        //SelectionChanged
        private void DampingRowSelected(object sender, SelectionChangedEventArgs e)
        {
            if (DampingGrid.SelectedCells.Count <= 0) return;
            var cellInfo = DampingGrid.SelectedCells[0];
            removeIndex = model.Eigenstate.DampingRatios.IndexOf(cellInfo.Item);
        }

        // ************************* Initial Conditions *********************************
        private void NewTimeNodalInitialConditions(object sender, MouseButtonEventArgs e)
        {
            _ = new NewTimeNodalInitialConditions(model);
            MainWindow.analysed = false;
            Close();
        }

        //UnloadingRow
        private void InitialConditionRowRemove(object sender, DataGridRowEventArgs e)
        {
            model.Timeintegration.InitialConditions.RemoveAt(removeIndex);
            MainWindow.analysed = false;
            Close();

            var structure = new DynamicModelDataShow(model);
            structure.Show();
        }

        //SelectionChanged
        private void InitialConditionRowSelected(object sender, SelectionChangedEventArgs e)
        {
            if (InitialConditionsGrid.SelectedCells.Count <= 0) return;
            var cellInfo = InitialConditionsGrid.SelectedCells[0];
            removeIndex = model.Timeintegration.InitialConditions.IndexOf(cellInfo.Item);
        }

        // ************************* Time Dependent Node Loads *********************************
        private void NewTimeNodeLoad(object sender, MouseButtonEventArgs e)
        {
            _ = new NewTimeNodeLoad(model);
            MainWindow.analysed = false;
            Close();
        }

        //UnloadingRow
        private void NodeLoadFileRowRemove(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            model.TimeDependentNodeLoads.Remove(removeKey);
            MainWindow.analysed = false;
            Close();
            var structure = new DynamicResultsShow(model);
            structure.Show();
        }

        private void NodeLoadHarmonicRowRemove(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            model.TimeDependentNodeLoads.Remove(removeKey);
            MainWindow.analysed = false;
            Close();
            var structure = new DynamicResultsShow(model);
            structure.Show();
        }

        private void NodeLoadLinearRowRemove(object sender, DataGridRowEventArgs e)
        {
            if (removeKey == null) return;
            model.TimeDependentNodeLoads.Remove(removeKey);
            MainWindow.analysed = false;
            Close();
            var structure = new DynamicResultsShow(model);
            structure.Show();
        }

        //SelectionChanged
        private void NodeLoadFileSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FileGrid.SelectedCells.Count <= 0) return;
            var cellInfo = FileGrid.SelectedCells[0];
            var load = (Model_Data.TimeDependentNodeLoad)cellInfo.Item;
            removeKey = load.LoadId;
        }

        private void NodeLoadHarmonicSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HarmonicGrid.SelectedCells.Count <= 0) return;
            var cellInfo = HarmonicGrid.SelectedCells[0];
            var load = (Model_Data.TimeDependentNodeLoad)cellInfo.Item;
            removeKey = load.LoadId;
        }

        private void NodeLoadLinearSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LinearGrid.SelectedCells.Count <= 0) return;
            var cellInfo = LinearGrid.SelectedCells[0];
            var load = (Model_Data.TimeDependentNodeLoad)cellInfo.Item;
            removeKey = load.LoadId;
        }

        // ************************* Model must be reevaluated ****************
        private void Model_Changed(object sender, DataGridCellEditEndingEventArgs e)
        {
            MainWindow.analysed = false;
        }
    }
}