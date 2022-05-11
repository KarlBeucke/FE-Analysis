using FE_Analysis.Structural_Analysis.Model_Data;
using FEALibrary.Model;
using System.Windows;

namespace FE_Analysis.Structural_Analysis.ModelDataRead
{
    public partial class NewSupport
    {
        private readonly FeModel model;

        public NewSupport(FeModel model)
        {
            InitializeComponent();
            this.model = model;
            Show();
        }

        public NewSupport(FeModel model, double preDefX, double preDefY, double preDefRot)
        {
            InitializeComponent();
            this.model = model;
            PreX.Text = preDefX.ToString("0.00");
            PreY.Text = preDefY.ToString("0.00");
            PreRot.Text = preDefRot.ToString("0.00");
            Show();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var supportId = SupportId.Text;
            var nodeId = NodeId.Text;
            var prescribed = new double[3];
            var conditions = 0;
            var type = Fixed.Text;
            for (var k = 0; k < type.Length; k++)
            {
                var subType = type.Substring(k, 1);
                switch (subType)
                {
                    case "x":
                        conditions += Support.X_FIXED;
                        break;
                    case "y":
                        conditions += Support.Y_FIXED;
                        break;
                    case "r":
                        conditions += Support.R_FIXED;
                        break;
                }
            }

            var lager = new Support(SupportId.Text, conditions, prescribed, model) { SupportId = supportId };
            model.BoundaryConditions.Add(supportId, lager);
            Close();
        }
    }
}