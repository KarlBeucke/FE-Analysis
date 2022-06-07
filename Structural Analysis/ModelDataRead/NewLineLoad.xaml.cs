using FE_Analysis.Structural_Analysis.Model_Data;
using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Windows;
using static System.Globalization.CultureInfo;

namespace FE_Analysis.Structural_Analysis.ModelDataRead
{
    public partial class NewLineLoad
    {
        private readonly FeModel model;
        private AbstractLineLoad lineLoad;

        public NewLineLoad()
        {
            InitializeComponent();
            Show();
        }

        public NewLineLoad(FeModel model, string load, string element,
            double pxa, double pya, double pxb, double pyb, string inElement)
        {
            InitializeComponent();
            this.model = model;
            LoadId.Text = load;
            ElementId.Text = element;
            Pxa.Text = pxa.ToString("0.00");
            Pxa.Text = pxa.ToString("0.00");
            Pya.Text = pya.ToString("0.00");
            Pxb.Text = pxb.ToString("0.00");
            Pyb.Text = pyb.ToString("0.00");
            InElement.Text = "false";
            Show();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var lastId = LoadId.Text;
            var elementId = ElementId.Text;
            var pxa = double.Parse(Pxa.Text, InvariantCulture);
            var pya = double.Parse(Pya.Text, InvariantCulture);
            var pxb = double.Parse(Pxb.Text, InvariantCulture);
            var pyb = double.Parse(Pyb.Text, InvariantCulture);
            var inElement = InElement.Text == "true";
            lineLoad =
                new LineLoad(elementId, pxa, pya, pxb, pyb, inElement)
                {
                    LoadId = lastId
                };
            model.ElementLoads.Add(lastId, lineLoad);
            Close();
        }
    }
}