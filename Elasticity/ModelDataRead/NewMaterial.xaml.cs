using FE_Analysis.Elasticity.ModelData;
using FEALibrary.Model;
using System.Windows;
using System.Windows.Markup;

namespace FE_Analysis.Elasticity.ModelDataRead
{
    public partial class NewMaterial
    {
        private readonly FeModel model;

        public NewMaterial(FeModel model)
        {
            Language = XmlLanguage.GetLanguage("us-US");
            this.model = model;
            InitializeComponent();
            MaterialId.Text = string.Empty;
            YoungsModulus.Text = string.Empty;
            Poisson.Text = string.Empty;
            ShearModulus.Text = string.Empty;
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var materialId = MaterialId.Text;
            double elastizität = 0, poisson = 0, schub = 0;
            if (YoungsModulus.Text != string.Empty) elastizität = double.Parse(YoungsModulus.Text);
            if (Poisson.Text != string.Empty) poisson = double.Parse(Poisson.Text);
            if (ShearModulus.Text != string.Empty) schub = double.Parse(ShearModulus.Text);
            var material = new Material(elastizität, poisson, schub)
            {
                MaterialId = materialId
            };
            model.Material.Add(materialId, material);
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}