using FE_Analysis.Heat_Transfer.Model_Data;
using FEALibrary.Model;
using System.Windows;
using static System.Globalization.CultureInfo;

namespace FE_Analysis.Heat_Transfer.ModelDataRead
{
    public partial class NewLineLoad
    {
        private readonly FeModel model;

        public NewLineLoad(FeModel model)
        {
            this.model = model;
            InitializeComponent();
            LineLoadId.Text = string.Empty;
            StartNodeId.Text = string.Empty;
            Start.Text = string.Empty;
            EndNodeId.Text = string.Empty;
            End.Text = string.Empty;
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var linienlastId = LineLoadId.Text;
            var temperatur = new double[2];
            var startId = StartNodeId.Text;
            if (Start.Text != string.Empty) temperatur[0] = double.Parse(Start.Text, InvariantCulture);
            var endId = EndNodeId.Text;
            if (End.Text != string.Empty) temperatur[1] = double.Parse(End.Text, InvariantCulture);

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