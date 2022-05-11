using FEALibrary.Model;
using System.Windows;

namespace FE_Analysis.Structural_Analysis.ModelDataRead
{
    public partial class NewCrossSection
    {
        private readonly FeModel model;

        public NewCrossSection(FeModel model)
        {
            InitializeComponent();
            this.model = model;
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var crossSectionId = CrossSectionId.Text;
            double fläche = 0, ixx = 0;
            if (Area.Text != "") fläche = double.Parse(Area.Text);
            if (Ixx.Text != "") ixx = double.Parse(Ixx.Text);
            {
                var querschnitt = new CrossSection(fläche, ixx) { CrossSectionId = crossSectionId };
                model.CrossSection.Add(crossSectionId, querschnitt);
            }
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}