using FEALibrary.Model;
using System.Windows;

namespace FE_Analysis.Heat_Transfer.ModelDataRead
{
    public partial class NewNode
    {
        private readonly FeModel model;

        public NewNode(FeModel model)
        {
            this.model = model;
            InitializeComponent();
            NodeId.Text = string.Empty;
            NumberDOF.Text = "1";
            X.Text = string.Empty;
            Y.Text = string.Empty;
            Z.Text = string.Empty;
            Show();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var dimension = model.SpatialDimension;
            var knotenId = NodeId.Text;
            const int numberNodalDof = 1;
            var crds = new double[dimension];
            if (X.Text.Length > 0) crds[0] = double.Parse(X.Text);
            if (Y.Text.Length > 0) crds[1] = double.Parse(Y.Text);
            if (Z.Text.Length > 0) crds[2] = double.Parse(Z.Text);
            if (NodeId.Text.Length > 0)
            {
                var knoten = new Node(knotenId, crds, numberNodalDof, dimension);
                model.Nodes.Add(knotenId, knoten);
            }

            Close();
        }
    }
}