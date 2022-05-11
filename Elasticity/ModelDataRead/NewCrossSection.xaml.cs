using FEALibrary.Model;
using System.Windows;
using System.Windows.Markup;
using static System.Globalization.CultureInfo;

namespace FE_Analysis.Elasticity.ModelDataRead
{
    public partial class NewCrossSection
    {
        private readonly FeModel model;

        public NewCrossSection(FeModel model)
        {
            Language = XmlLanguage.GetLanguage("us-US");
            this.model = model;
            InitializeComponent();
            CrossSectionId.Text = "";
            Thickness.Text = "";
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var crossSectionId = CrossSectionId.Text;
            double dicke = 0;
            if (Thickness.Text.Length != 0) dicke = double.Parse(Thickness.Text, InvariantCulture);
            var querschnitt = new CrossSection(dicke)
            {
                CrossSectionId = crossSectionId
            };
            model.CrossSection.Add(crossSectionId, querschnitt);
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}