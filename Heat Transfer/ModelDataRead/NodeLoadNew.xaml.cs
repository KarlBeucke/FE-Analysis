using FE_Analysis.Heat_Transfer.Model_Data;
using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace FE_Analysis.Heat_Transfer.ModelDataRead
{
    public partial class NodeLoadNew
    {
        private readonly FeModel model;
        private AbstractLoad existingLoad;
        private readonly HeatloadKeys loadKeys;
        public NodeLoadNew()
        {
            InitializeComponent();
            Show();
        }
        public NodeLoadNew(FeModel model)
        {
            this.model = model;
            InitializeComponent();
            loadKeys = new HeatloadKeys(model);
            loadKeys.Show();
            Show();
        }
        public NodeLoadNew(FeModel model, string load, string node, double t)
        {
            InitializeComponent();
            this.model = model;
            NodeLoadId.Text = load;
            NodeId.Text = node;
            Temperature.Text = t.ToString("0.00");
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var nodeloadId = NodeLoadId.Text;
            if (nodeloadId == "")
            {
                _ = MessageBox.Show("Nodelad Id must be defined", "new Nodeload");
                return;
            }

            // existing Nodeload
            if (model.Loads.Keys.Contains(NodeLoadId.Text))
            {
                model.Loads.TryGetValue(nodeloadId, out existingLoad);
                Debug.Assert(existingLoad != null, nameof(existingLoad) + " != null");

                if (NodeId.Text.Length > 0) existingLoad.NodeId = NodeId.Text.ToString(CultureInfo.CurrentCulture);
                if (Temperature.Text.Length > 0) existingLoad.Loadvalues[0] = double.Parse(Temperature.Text);
            }
            // new Nodeload
            else
            {
                string nodeId = "";
                double[] t = new double[1];
                if (NodeId.Text.Length > 0) nodeId = NodeId.Text.ToString(CultureInfo.CurrentCulture);
                if (Temperature.Text.Length > 0) t[0] = double.Parse(Temperature.Text);
                var nodeload = new NodeLoad(nodeloadId, nodeId, t);
                model.Loads.Add(nodeloadId, nodeload);
            }
            loadKeys?.Close();
            Close();
            MainWindow.heatModel.Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            loadKeys?.Close();
            Close();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!model.Loads.Keys.Contains(NodeLoadId.Text)) return;
            model.Loads.Remove(NodeLoadId.Text);
            loadKeys?.Close();
            Close();
            MainWindow.heatModel.Close();
        }

        private void NodeLoadIdLostFocus(object sender, RoutedEventArgs e)
        {
            if (!model.Loads.ContainsKey(NodeLoadId.Text))
            {
                NodeId.Text = "";
                Temperature.Text = "";
                return;
            }

            // existing Nodeload definition
            model.Loads.TryGetValue(NodeLoadId.Text, out existingLoad);
            Debug.Assert(existingLoad != null, nameof(existingLoad) + " != null"); NodeLoadId.Text = "";

            NodeLoadId.Text = existingLoad.LoadId;

            NodeId.Text = existingLoad.NodeId;
            Temperature.Text = existingLoad.Loadvalues[0].ToString("G3", CultureInfo.CurrentCulture);
        }
    }
}