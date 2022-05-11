using FE_Analysis.Heat_Transfer.Model_Data;
using FEALibrary.Model;
using System.Windows;
using System.Windows.Markup;

namespace FE_Analysis.Heat_Transfer.ModelDataRead
{
    public partial class TimeNewElementLoad
    {
        private readonly FeModel model;

        public TimeNewElementLoad(FeModel model)
        {
            Language = XmlLanguage.GetLanguage("us-US");
            this.model = model;
            InitializeComponent();
            LoadId.Text = "";
            ElementId.Text = "";
            P0.Text = "";
            P1.Text = "";
            P2.Text = "";
            P3.Text = "";
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var loadId = LoadId.Text;
            var elementId = ElementId.Text;

            var knotenWerte = new double[4];
            knotenWerte[0] = double.Parse(P0.Text);
            knotenWerte[1] = double.Parse(P1.Text);
            knotenWerte[2] = double.Parse(P2.Text);
            if (P3.Text.Length > 0) knotenWerte[3] = double.Parse(P3.Text);
            var zeitabhängigeElementLast = new TimeDependentElementLoad(elementId, knotenWerte)
            { LoadId = loadId, VariationType = 1 };

            model.TimeDependentElementLoads.Add(loadId, zeitabhängigeElementLast);
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}