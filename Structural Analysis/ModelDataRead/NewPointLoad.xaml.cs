using FE_Analysis.Structural_Analysis.Model_Data;
using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Windows;
using static System.Globalization.CultureInfo;

namespace FE_Analysis.Structural_Analysis.ModelDataRead
{
    public partial class NewPointLoad
    {
        private readonly FeModel modell;
        private AbstractElementLoad pointLoad;

        public NewPointLoad()
        {
            InitializeComponent();
            Show();
        }

        public NewPointLoad(FeModel modell, string last, string element, double px, double py, double offset)
        {
            InitializeComponent();
            this.modell = modell;
            LoadId.Text = last;
            ElementId.Text = element;
            Px.Text = px.ToString("0.00");
            Py.Text = py.ToString("0.00");
            Offset.Text = offset.ToString("0.00");
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var loadId = LoadId.Text;
            var elementId = ElementId.Text;
            var px = double.Parse(Px.Text, InvariantCulture);
            var py = double.Parse(Py.Text, InvariantCulture);
            var offset = double.Parse(Offset.Text, InvariantCulture);
            pointLoad =
                new PointLoad(elementId, px, py, offset)
                {
                    LoadId = loadId
                };
            modell.ElementLoads.Add(loadId, pointLoad);
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}