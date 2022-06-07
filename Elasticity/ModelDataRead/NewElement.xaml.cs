using FE_Analysis.Elasticity.ModelData;
using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Windows;
using System.Windows.Markup;

namespace FE_Analysis.Elasticity.ModelDataRead
{
    public partial class NewElement
    {
        private readonly FeModel model;

        public NewElement(FeModel model)
        {
            Language = XmlLanguage.GetLanguage("us-US");
            InitializeComponent();
            this.model = model;
            ElementId.Text = string.Empty;
            Node1Id.Text = string.Empty;
            Node2Id.Text = string.Empty;
            Node3Id.Text = string.Empty;
            Node4Id.Text = string.Empty;
            Node5Id.Text = string.Empty;
            Node6Id.Text = string.Empty;
            Node7Id.Text = string.Empty;
            Node8Id.Text = string.Empty;
            MaterialId.Text = string.Empty;
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            AbstractElement element = null;
            string elementId = null;

            if (Element2D3.IsChecked != null && (bool)Element2D3.IsChecked)
            {
                var nodeIds = new string[3];
                nodeIds[0] = Node1Id.Text;
                nodeIds[1] = Node2Id.Text;
                nodeIds[2] = Node3Id.Text;
                elementId = ElementId.Text;
                var querschnittId = CrossSectionId.Text;
                var materialId = MaterialId.Text;
                element = new Element2D3(nodeIds, querschnittId, materialId, model) { ElementId = elementId };
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
                var materialId = MaterialId.Text;
                element = new Element3D8(nodeIds, materialId, model) { ElementId = elementId };
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