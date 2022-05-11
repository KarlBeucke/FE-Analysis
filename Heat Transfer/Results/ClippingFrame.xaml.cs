using System.Globalization;
using System.Windows;

namespace FE_Analysis.Heat_Transfer.Results
{
    public partial class ClippingFrame
    {
        public double tmin, tmax;
        public double maxTemperature;
        public double maxHeatFlow;

        public ClippingFrame(double tmin, double tmax, double maxTemperature, double maxHeatFlow)
        {
            InitializeComponent();
            this.tmin = tmin;
            this.tmax = tmax;
            this.maxTemperature = maxTemperature;
            this.maxHeatFlow = maxHeatFlow;
            //TxtMinTime.Text = this.tmin.ToString(CultureInfo.InvariantCultur);
            TxtMaxTime.Text = this.tmax.ToString(CultureInfo.InvariantCulture);
            TxtMaxTemperature.Text = this.maxTemperature.ToString("N2");
            TxtMaxHeatFlow.Text = this.maxHeatFlow.ToString("N2");
            ShowDialog();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            //tmin = double.Parse(TxtMaxTime.Text);
            tmax = double.Parse(TxtMaxTime.Text);
            maxTemperature = double.Parse(TxtMaxTemperature.Text);
            maxHeatFlow = double.Parse(TxtMaxHeatFlow.Text);
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}