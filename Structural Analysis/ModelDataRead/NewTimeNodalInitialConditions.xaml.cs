using FEALibrary.Model;
using System.Windows;

namespace FE_Analysis.Structural_Analysis.ModelDataRead
{
    public partial class NewTimeNodalInitialConditions
    {
        private readonly FeModel model;

        public NewTimeNodalInitialConditions(FeModel model)
        {
            InitializeComponent();
            this.model = model;
            NodeId.Text = "";
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var knotenId = NodeId.Text;
            if (model.Nodes.TryGetValue(knotenId, out var knoten))
            {
                var nodalDof = knoten.NumberOfNodalDof;
                var anfangsWerte = new double[2 * nodalDof];
                if (D0.Text != "") anfangsWerte[0] = double.Parse(D0.Text);
                if (V0.Text != "") anfangsWerte[1] = double.Parse(V0.Text);

                if (nodalDof == 2)
                {
                    if (D1.Text != "") anfangsWerte[2] = double.Parse(D1.Text);
                    if (V1.Text != "") anfangsWerte[3] = double.Parse(V1.Text);
                }

                if (nodalDof == 3)
                {
                    if (D2.Text != "") anfangsWerte[4] = double.Parse(D2.Text);
                    if (V2.Text != "") anfangsWerte[5] = double.Parse(V2.Text);
                }

                model.Timeintegration.InitialConditions.Add(new NodalValues(knotenId, anfangsWerte));
            }

            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}