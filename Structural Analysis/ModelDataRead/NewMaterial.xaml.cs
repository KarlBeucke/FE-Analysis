using FE_Analysis.Structural_Analysis.Model_Data;
using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Windows;

namespace FE_Analysis.Structural_Analysis.ModelDataRead
{
    public partial class NewMaterial
    {
        private readonly FeModel model;
        private AbstractMaterial material;

        public NewMaterial(FeModel model)
        {
            InitializeComponent();
            this.model = model;
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var materialId = MaterialId.Text;
            double eModulus = 0, mass = 0, poisson = 0;
            if (EModulus.Text != "") eModulus = double.Parse(EModulus.Text);
            if (Mass.Text != "") mass = double.Parse(Mass.Text);
            if (Poisson.Text != "") poisson = double.Parse(Poisson.Text);
            if (SpringX.Text == "" && SpringY.Text == "" && SpringPhi.Text == "")
            {
                material = new Material(eModulus, poisson, mass)
                {
                    MaterialId = materialId
                };
            }
            else
            {
                double springX = 0;
                if (SpringX.Text != "") springX = double.Parse(SpringX.Text);
                double springY = 0;
                if (SpringY.Text != "") springY = double.Parse(SpringY.Text);
                double springPhi = 0;
                if (SpringPhi.Text != "") springPhi = double.Parse(SpringPhi.Text);
                material = new Material("Spring Support", springX, springY, springPhi)
                {
                    MaterialId = materialId
                };
            }

            model.Material.Add(materialId, material);
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}