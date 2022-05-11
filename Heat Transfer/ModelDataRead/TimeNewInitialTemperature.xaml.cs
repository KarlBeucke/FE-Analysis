using FEALibrary.Model;
using System.Windows;
using System.Windows.Markup;

namespace FE_Analysis.Heat_Transfer.ModelDataRead
{
    public partial class TimeNewInitialTemperature
    {
        private readonly FeModel model;

        public TimeNewInitialTemperature(FeModel model)
        {
            Language = XmlLanguage.GetLanguage("us-US");
            InitializeComponent();
            this.model = model;
            NodeId.Text = "";
            InitialTemperature.Text = "";
            StationarySolution.IsChecked = model.Timeintegration.FromStationary;
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            if (StationarySolution.IsChecked == true)
            {
                model.Timeintegration.FromStationary = true;
                model.Timeintegration.InitialConditions.Clear();
                Close();
                return;
            }

            var knotenId = NodeId.Text;
            var anfang = new double[1];
            if (InitialTemperature.Text != "") anfang[0] = double.Parse(InitialTemperature.Text);
            model.Timeintegration.InitialConditions.Add(new NodalValues(knotenId, anfang));
            model.Timeintegration.FromStationary = false;
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}