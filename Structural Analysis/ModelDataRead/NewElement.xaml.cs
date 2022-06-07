using FE_Analysis.Structural_Analysis.Model_Data;
using FEALibrary.Model;
using System.Windows;

namespace FE_Analysis.Structural_Analysis.ModelDataRead
{
    public partial class NewElement
    {
        private readonly FeModel model;

        public NewElement(FeModel model)
        {
            InitializeComponent();
            this.model = model;
            ElementId.Text = string.Empty;
            StartNodeId.Text = string.Empty;
            EndNodeId.Text = string.Empty;
            MaterialId.Text = string.Empty;
            CrossSectionId.Text = string.Empty;
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var nodeIds = new string[2];
            nodeIds[0] = StartNodeId.Text;
            nodeIds[1] = EndNodeId.Text;

            if (Truss.IsChecked != null && (bool)Truss.IsChecked)
            {
                var element = new Truss(nodeIds, CrossSectionId.Text, MaterialId.Text, model)
                {
                    ElementId = ElementId.Text
                };
                model.Elements.Add(ElementId.Text, element);
            }
            else if (Beam.IsChecked != null && (bool)Beam.IsChecked)
            {
                var element = new Beam(nodeIds, CrossSectionId.Text, MaterialId.Text, model)
                {
                    ElementId = ElementId.Text
                };
                model.Elements.Add(ElementId.Text, element);
            }
            else if (BeamHinged.IsChecked != null && (bool)BeamHinged.IsChecked)
            {
                var type = int.Parse(Hinge.Text);
                var element = new BeamHinged(nodeIds, CrossSectionId.Text, MaterialId.Text, model, type)
                {
                    ElementId = ElementId.Text
                };
                model.Elements.Add(ElementId.Text, element);
            }
            else if (Spring.IsChecked != null && (bool)Spring.IsChecked)
            {
                var springSupport = new SpringElement(nodeIds, MaterialId.Text, model)
                {
                    ElementId = ElementId.Text
                };
                model.Elements.Add(ElementId.Text, springSupport);
            }

            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}