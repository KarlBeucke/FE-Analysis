using FEALibrary.Model;
using System.Windows;
using static System.Globalization.CultureInfo;

namespace FE_Analysis.Structural_Analysis.ModelDataRead
{
    public partial class NewTimeNodalInitialConditions
    {
        private readonly FeModel model;

        public NewTimeNodalInitialConditions(FeModel model)
        {
            InitializeComponent();
            this.model = model;
            NodeId.Text = string.Empty;
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var knotenId = NodeId.Text;
            if (model.Nodes.TryGetValue(knotenId, out var knoten))
            {
                var nodalDof = knoten.NumberOfNodalDof;
                var anfangsWerte = new double[2 * nodalDof];
                if (D0.Text != string.Empty) anfangsWerte[0] = double.Parse(D0.Text, InvariantCulture);
                if (V0.Text != string.Empty) anfangsWerte[1] = double.Parse(V0.Text, InvariantCulture);

                if (nodalDof == 2)
                {
                    if (D1.Text != string.Empty) anfangsWerte[2] = double.Parse(D1.Text, InvariantCulture);
                    if (V1.Text != string.Empty) anfangsWerte[3] = double.Parse(V1.Text, InvariantCulture);
                }

                if (nodalDof == 3)
                {
                    if (D2.Text != string.Empty) anfangsWerte[4] = double.Parse(D2.Text, InvariantCulture);
                    if (V2.Text != string.Empty) anfangsWerte[5] = double.Parse(V2.Text, InvariantCulture);
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