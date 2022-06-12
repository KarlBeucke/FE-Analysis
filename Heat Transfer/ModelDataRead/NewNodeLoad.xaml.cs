using FE_Analysis.Heat_Transfer.Model_Data;
using FEALibrary.Model;
using System.Windows;
using static System.Globalization.CultureInfo;

namespace FE_Analysis.Heat_Transfer.ModelDataRead
{
    public partial class NewNodeLoad
    {
        private readonly FeModel model;

        public NewNodeLoad(FeModel model)
        {
            this.model = model;
            InitializeComponent();
            NodeLoadId.Text = string.Empty;
            NodeId.Text = string.Empty;
            Temperature.Text = string.Empty;
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var nodeLoadId = NodeLoadId.Text;
            var nodeId = NodeId.Text;
            var temperature = new double[1];
            if (Temperature.Text != string.Empty) temperature[0] = double.Parse(Temperature.Text, InvariantCulture);

            var nodeLoad = new NodeLoad(nodeLoadId, nodeId, temperature);

            model.Loads.Add(nodeLoadId, nodeLoad);
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}