using FE_Analysis.Heat_Transfer.Model_Data;
using FEALibrary.Model;
using System.Windows;

namespace FE_Analysis.Heat_Transfer.ModelDataRead
{
    public partial class NewMaterial
    {
        private readonly FeModel model;

        public NewMaterial(FeModel model)
        {
            this.model = model;
            InitializeComponent();
            MaterialId.Text = string.Empty;
            ConductivityX.Text = string.Empty;
            ConductivityY.Text = string.Empty;
            ConductivityZ.Text = string.Empty;
            DensityConductivity.Text = string.Empty;
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var materialId = MaterialId.Text;
            var conductivity = new double[3];
            double densityConductivity = 0;
            if (ConductivityX.Text != "") conductivity[0] = double.Parse(ConductivityX.Text);
            if (ConductivityY.Text != "") conductivity[1] = double.Parse(ConductivityY.Text);
            if (ConductivityZ.Text != "") conductivity[2] = double.Parse(ConductivityZ.Text);
            if (DensityConductivity.Text != "") densityConductivity = double.Parse(DensityConductivity.Text);
            var material = new Material(materialId, conductivity, densityConductivity);

            model.Material.Add(materialId, material);
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}