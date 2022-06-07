using FE_Analysis.Elasticity.ModelData;
using FEALibrary.Model;
using System.Windows;
using System.Windows.Markup;
using static System.Globalization.CultureInfo;

namespace FE_Analysis.Elasticity.ModelDataRead
{
    public partial class NewNodeLoad
    {
        private readonly FeModel model;

        public NewNodeLoad()
        {
            Language = XmlLanguage.GetLanguage("us-US");
            InitializeComponent();
            Show();
        }

        public NewNodeLoad(FeModel model, string last, string knoten,
            double px, double py, double pz)
        {
            Language = XmlLanguage.GetLanguage("us-US");
            InitializeComponent();
            this.model = model;
            LoadId.Text = last;
            NodeId.Text = knoten;
            Px.Text = px.ToString("0.00");
            Py.Text = py.ToString("0.00");
            Pz.Text = pz.ToString("0.00");
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var loadId = LoadId.Text;
            var nodeId = NodeId.Text;
            var p = new double[3];
            p[0] = double.Parse(Px.Text, InvariantCulture);
            p[1] = double.Parse(Py.Text, InvariantCulture);
            p[2] = double.Parse(Pz.Text, InvariantCulture);
            var nodeLoad = new NodeLoad(nodeId, p[0], p[1], p[2])
            {
                LoadId = loadId
            };
            model.Loads.Add(loadId, nodeLoad);
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}