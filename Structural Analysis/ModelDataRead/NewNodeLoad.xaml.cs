using FEALibrary.Model;
using System.Windows;

namespace FE_Analysis.Structural_Analysis.ModelDataRead
{
    public partial class NewNodeLoad
    {
        private readonly FeModel model;

        public NewNodeLoad()
        {
            InitializeComponent();
            Show();
        }

        public NewNodeLoad(FeModel model, string last, string knoten,
            double px, double py, double m)
        {
            InitializeComponent();
            this.model = model;
            LoadId.Text = last;
            NodeId.Text = knoten;
            Px.Text = px.ToString("0.00");
            Py.Text = py.ToString("0.00");
            M.Text = m.ToString("0.00");
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var loadId = LoadId.Text;
            var nodeId = NodeId.Text;
            var p = new double[3];
            p[0] = double.Parse(Px.Text);
            p[1] = double.Parse(Py.Text);
            p[2] = double.Parse(M.Text);
            var knotenLast = new Structural_Analysis.Model_Data.NodeLoad(nodeId, p[0], p[1], p[2])
            {
                LoadId = loadId
            };
            model.Loads.Add(loadId, knotenLast);
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}