using FE_Analysis.Elasticity.ModelData;
using FEALibrary.Model;
using System.Collections.Generic;
using System.Windows;
using static System.Windows.Markup.XmlLanguage;

namespace FE_Analysis.Elasticity.ModelDataRead
{
    public partial class NewSupport
    {
        private readonly FeModel model;

        public NewSupport(FeModel model)
        {
            Language = GetLanguage("us-US");
            InitializeComponent();
            this.model = model;
            InitialNodeId.Text = string.Empty;
            NumberNodes.Text = string.Empty;
            PreX.Text = string.Empty;
            PreY.Text = string.Empty;
            PreZ.Text = string.Empty;
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var prescribed = new double[3];
            var faces = new List<string>();


            if (SurfaceId.Text.Length == 0)
            {
                var supportId = SupportId.Text;
                var nodeId = NodeId.Text;
                var conditions = 0;
                if (XFixed.IsChecked != null && (bool)XFixed.IsChecked) conditions += 1;
                if (YFixed.IsChecked != null && (bool)YFixed.IsChecked) conditions += 2;
                if (ZFixed.IsChecked != null && (bool)ZFixed.IsChecked) conditions += 4;

                var boundaryCondition = new Support(nodeId, "0", conditions, prescribed, model);
                model.BoundaryConditions.Add(supportId, boundaryCondition);
            }
            else
            {
                var supportInitial = SupportId.Text;
                var face = SurfaceId.Text;
                faces.Add(face);
                var nodeInitial = InitialNodeId.Text;
                int nNodes = short.Parse(NumberNodes.Text);

                var conditions = 0;
                if (XFixed.IsChecked != null && (bool)XFixed.IsChecked) conditions += 1;
                if (YFixed.IsChecked != null && (bool)YFixed.IsChecked) conditions += 2;
                if (ZFixed.IsChecked != null && (bool)ZFixed.IsChecked) conditions += 4;

                if (PreX.Text.Length > 0) prescribed[0] = double.Parse(PreX.Text);
                if (PreY.Text.Length > 0) prescribed[1] = double.Parse(PreY.Text);
                if (PreZ.Text.Length > 0) prescribed[2] = double.Parse(PreZ.Text);

                for (var m = 0; m < nNodes; m++)
                {
                    var id1 = m.ToString().PadLeft(2, '0');
                    for (var k = 0; k < nNodes; k++)
                    {
                        var id2 = k.ToString().PadLeft(2, '0');
                        var supportName = supportInitial + face + id1 + id2;
                        if (model.BoundaryConditions.TryGetValue(supportName, out _))
                            throw new ParseException($"support condition \"{supportName}\" already exists.");
                        string nodeName;
                        const string faceNode = "00";
                        switch (face.Substring(0, 1))
                        {
                            case "X":
                                nodeName = nodeInitial + faceNode + id1 + id2;
                                break;
                            case "Y":
                                nodeName = nodeInitial + id1 + faceNode + id2;
                                break;
                            case "Z":
                                nodeName = nodeInitial + id1 + id2 + faceNode;
                                break;
                            default:
                                throw new ParseException(
                                    $"wrong SurfaceId = {face.Substring(0, 1)}, must be:\n X, Y or Z");
                        }

                        var support = new Support(nodeName, face, conditions, prescribed, model);
                        model.BoundaryConditions.Add(supportName, support);
                    }
                }
            }

            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}