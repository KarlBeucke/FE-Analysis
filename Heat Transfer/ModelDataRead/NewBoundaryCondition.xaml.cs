using FE_Analysis.Heat_Transfer.Model_Data;
using FEALibrary.Model;
using System.Windows;

namespace FE_Analysis.Heat_Transfer.ModelDataRead
{
    public partial class NewBoundaryCondition
    {
        private readonly FeModel model;

        public NewBoundaryCondition(FeModel model)
        {
            this.model = model;
            InitializeComponent();
            RandbedingungId.Text = string.Empty;
            KnotenId.Text = string.Empty;
            Temperature.Text = string.Empty;
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var randbedingungId = RandbedingungId.Text;
            var knotenId = KnotenId.Text;
            double temperatur = 0;
            if (Temperature.Text != "") temperatur = double.Parse(Temperature.Text);

            var randbedingung = new BoundaryCondition(randbedingungId, knotenId, temperatur);

            model.BoundaryConditions.Add(randbedingungId, randbedingung);
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}