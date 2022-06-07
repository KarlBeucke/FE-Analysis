using FE_Analysis.Structural_Analysis.Model_Data;
using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Windows;
using static System.Globalization.CultureInfo;

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
            if (EModulus.Text != string.Empty) eModulus = double.Parse(EModulus.Text, InvariantCulture);
            if (Mass.Text != string.Empty) mass = double.Parse(Mass.Text, InvariantCulture);
            if (Poisson.Text != string.Empty) poisson = double.Parse(Poisson.Text, InvariantCulture);
            if (SpringX.Text == string.Empty && SpringY.Text == string.Empty && SpringPhi.Text == string.Empty)
            {
                material = new Material(eModulus, poisson, mass)
                {
                    MaterialId = materialId
                };
            }
            else
            {
                double springX = 0;
                if (SpringX.Text != string.Empty) springX = double.Parse(SpringX.Text, InvariantCulture);
                double springY = 0;
                if (SpringY.Text != string.Empty) springY = double.Parse(SpringY.Text, InvariantCulture);
                double springPhi = 0;
                if (SpringPhi.Text != string.Empty) springPhi = double.Parse(SpringPhi.Text, InvariantCulture);
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