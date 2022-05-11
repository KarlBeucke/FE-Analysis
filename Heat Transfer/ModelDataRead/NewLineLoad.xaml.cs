using FE_Analysis.Heat_Transfer.Model_Data;
using FEALibrary.Model;
using System.Windows;

namespace FE_Analysis.Heat_Transfer.ModelDataRead
{
    public partial class NewLineLoad
    {
        private readonly FeModel model;

        public NewLineLoad(FeModel model)
        {
            this.model = model;
            InitializeComponent();
            LineLoadId.Text = "";
            StartNodeId.Text = "";
            Start.Text = "";
            EndNodeId.Text = "";
            End.Text = "";
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var linienlastId = LineLoadId.Text;
            var temperatur = new double[2];
            var startId = StartNodeId.Text;
            if (Start.Text != "") temperatur[0] = double.Parse(Start.Text);
            var endId = EndNodeId.Text;
            if (End.Text != "") temperatur[1] = double.Parse(End.Text);

            var knotenlast = new LineLoad(linienlastId, startId, endId, temperatur);

            model.LineLoads.Add(linienlastId, knotenlast);
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}