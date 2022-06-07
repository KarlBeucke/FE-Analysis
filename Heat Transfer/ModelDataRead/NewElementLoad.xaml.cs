using FE_Analysis.Heat_Transfer.Model_Data;
using FEALibrary.Model;
using System.Windows;

namespace FE_Analysis.Heat_Transfer.ModelDataRead
{
    public partial class NewElementLoad
    {
        private readonly FeModel model;

        public NewElementLoad(FeModel model)
        {
            this.model = model;
            InitializeComponent();
            ElementLoadId.Text = string.Empty;
            ElementId.Text = string.Empty;
            Node1.Text = string.Empty;
            Node2.Text = string.Empty;
            Node3.Text = string.Empty;
            Node4.Text = string.Empty;
            Node5.Text = string.Empty;
            Node6.Text = string.Empty;
            Node7.Text = string.Empty;
            Node8.Text = string.Empty;
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var elementlastId = ElementLoadId.Text;
            var elementId = ElementId.Text;
            var temperatur = new double[8];
            if (Node1.Text != "") temperatur[0] = double.Parse(Node1.Text);
            if (Node2.Text != "") temperatur[1] = double.Parse(Node2.Text);
            if (Node3.Text != "") temperatur[2] = double.Parse(Node3.Text);
            if (Node4.Text != "") temperatur[3] = double.Parse(Node4.Text);
            if (Node5.Text != "") temperatur[4] = double.Parse(Node5.Text);
            if (Node6.Text != "") temperatur[5] = double.Parse(Node6.Text);
            if (Node7.Text != "") temperatur[6] = double.Parse(Node7.Text);
            if (Node8.Text != "") temperatur[7] = double.Parse(Node8.Text);

            var elementlast = new ElementLoad4(elementlastId, elementId, temperatur);

            model.ElementLoads.Add(elementlastId, elementlast);
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}