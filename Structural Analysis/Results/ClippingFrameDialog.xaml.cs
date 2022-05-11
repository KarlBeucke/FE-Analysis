using System.Globalization;
using System.Windows;

namespace FE_Analysis.Structural_Analysis.Results
{
    public partial class ClippingFrameDialog
    {
        public double tmin, tmax;
        public double maxDeformation;
        public double maxAcceleration;

        public ClippingFrameDialog(double tmin, double tmax, double maxDeformation, double maxAcceleration)
        {
            InitializeComponent();
            this.tmin = tmin;
            this.tmax = tmax;
            this.maxDeformation = maxDeformation;
            this.maxAcceleration = maxAcceleration;
            //TxtMinTime.Text = this.tmin.ToString(CultureInfo.CurrentCulture);
            TxtMaxTime.Text = this.tmax.ToString(CultureInfo.CurrentCulture);
            TxtMaxDeformation.Text = this.maxDeformation.ToString("N4");
            TxtMaxAcceleration.Text = this.maxAcceleration.ToString("N4");
            ShowDialog();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            //tmin = double.Parse(TxtMinTime.Text);
            tmax = double.Parse(TxtMaxTime.Text);
            maxDeformation = double.Parse(TxtMaxDeformation.Text);
            maxAcceleration = double.Parse(TxtMaxAcceleration.Text);
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}