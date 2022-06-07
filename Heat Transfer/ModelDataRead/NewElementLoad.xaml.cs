using FE_Analysis.Heat_Transfer.Model_Data;
using FEALibrary.Model;
using System.Windows;
using static System.Globalization.CultureInfo;

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
            if (Node1.Text != string.Empty) temperatur[0] = double.Parse(Node1.Text, InvariantCulture);
            if (Node2.Text != string.Empty) temperatur[1] = double.Parse(Node2.Text, InvariantCulture);
            if (Node3.Text != string.Empty) temperatur[2] = double.Parse(Node3.Text, InvariantCulture);
            if (Node4.Text != string.Empty) temperatur[3] = double.Parse(Node4.Text, InvariantCulture);
            if (Node5.Text != string.Empty) temperatur[4] = double.Parse(Node5.Text, InvariantCulture);
            if (Node6.Text != string.Empty) temperatur[5] = double.Parse(Node6.Text, InvariantCulture);
            if (Node7.Text != string.Empty) temperatur[6] = double.Parse(Node7.Text, InvariantCulture);
            if (Node8.Text != string.Empty) temperatur[7] = double.Parse(Node8.Text, InvariantCulture);

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