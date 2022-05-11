using FE_Analysis.Heat_Transfer.Model_Data;
using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Windows;

namespace FE_Analysis.Heat_Transfer.ModelDataRead
{
    public partial class NewElement
    {
        private readonly FeModel model;

        public NewElement(FeModel model)
        {
            InitializeComponent();
            this.model = model;
            ElementId.Text = "";
            Node1Id.Text = "";
            Node2Id.Text = "";
            Node3Id.Text = "";
            Node4Id.Text = "";
            Node5Id.Text = "";
            Node6Id.Text = "";
            Node7Id.Text = "";
            Node8Id.Text = "";
            MaterialId.Text = "";
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            AbstractElement element = null;
            string elementId = null;

            if (Element2D2.IsChecked != null && (bool)Element2D2.IsChecked)
            {
                var nodeIds = new string[2];
                nodeIds[0] = Node1Id.Text;
                nodeIds[1] = Node2Id.Text;
                elementId = ElementId.Text;
                element = new Element2D2(elementId, nodeIds, MaterialId.Text, model);
                model.Elements.Add(ElementId.Text, element);
            }
            else if (Element2D3.IsChecked != null && (bool)Element2D3.IsChecked)
            {
                var nodeIds = new string[3];
                nodeIds[0] = Node1Id.Text;
                nodeIds[1] = Node2Id.Text;
                nodeIds[2] = Node3Id.Text;
                elementId = ElementId.Text;
                element = new Element2D3(elementId, nodeIds, MaterialId.Text, model);
                model.Elements.Add(ElementId.Text, element);
            }
            else if (Element2D4.IsChecked != null && (bool)Element2D4.IsChecked)
            {
                var nodeIds = new string[4];
                nodeIds[0] = Node1Id.Text;
                nodeIds[1] = Node2Id.Text;
                nodeIds[2] = Node3Id.Text;
                nodeIds[3] = Node4Id.Text;
                elementId = ElementId.Text;
                element = new Element2D4(elementId, nodeIds, MaterialId.Text, model);
                model.Elements.Add(ElementId.Text, element);
            }
            else if (Element3D8.IsChecked != null && (bool)Element3D8.IsChecked)
            {
                var nodeIds = new string[8];
                nodeIds[0] = Node1Id.Text;
                nodeIds[1] = Node2Id.Text;
                nodeIds[2] = Node3Id.Text;
                nodeIds[3] = Node4Id.Text;
                nodeIds[4] = Node5Id.Text;
                nodeIds[5] = Node6Id.Text;
                nodeIds[6] = Node7Id.Text;
                nodeIds[7] = Node8Id.Text;
                elementId = ElementId.Text;
                element = new Element3D8(elementId, nodeIds, MaterialId.Text, model);
                model.Elements.Add(ElementId.Text, element);
            }

            if (elementId != null) model.Elements.Add(elementId, element);
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}