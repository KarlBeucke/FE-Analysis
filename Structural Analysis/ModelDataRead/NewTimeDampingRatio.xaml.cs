using FEALibrary.Model;
using System.Globalization;
using System.Windows;

namespace FE_Analysis.Structural_Analysis.ModelDataRead
{
    public partial class NewTimeDampingRatio
    {
        private readonly FeModel model;

        public NewTimeDampingRatio(FeModel model)
        {
            InitializeComponent();
            this.model = model;
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            model.Eigenstate.DampingRatios.
                Add(new ModalValues(double.Parse(Xi.Text, CultureInfo.InvariantCulture)));
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}