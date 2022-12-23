using FE_Analysis.Structural_Analysis.ModelDataRead;
using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace FE_Analysis.Heat_Transfer.ModelDataRead
{
    public partial class MaterialNew
    {
        private readonly FeModel model;
        private AbstractMaterial material, existingMaterial;
        private readonly MaterialKeys materialKeys;

        public MaterialNew(FeModel model)
        {
            this.model = model;
            InitializeComponent();
            materialKeys = new MaterialKeys(model);
            materialKeys.Show();
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var materialId = MaterialId.Text;
            if (materialId == "")
            {
                _ = MessageBox.Show("Material Id must be defined", "new Material");
                return;
            }

            var conductivity = new double[3];
            double densityConductivity = 0;
            // existing Material
            if (model.Material.Keys.Contains(MaterialId.Text))
            {
                model.Material.TryGetValue(materialId, out existingMaterial);
                Debug.Assert(existingMaterial != null, nameof(existingMaterial) + " != null");

                if (ConductivityX.Text.Length > 0) existingMaterial.MaterialValues[0] = double.Parse(ConductivityX.Text);
                if (ConductivityY.Text.Length > 0) existingMaterial.MaterialValues[1] = double.Parse(ConductivityY.Text);
                if (ConductivityZ.Text.Length > 0) existingMaterial.MaterialValues[2] = double.Parse(ConductivityZ.Text);
                if (DensityConductivity.Text.Length > 0) existingMaterial.MaterialValues[3] = double.Parse(DensityConductivity.Text);
            }
            // neues Material
            else
            {
                if (ConductivityX.Text.Length > 0)
                    conductivity[0] = double.Parse(ConductivityX.Text);
                if (ConductivityY.Text.Length > 0)
                    conductivity[1] = double.Parse(ConductivityY.Text);
                if (ConductivityZ.Text.Length > 0)
                    conductivity[2] = double.Parse(ConductivityZ.Text);
                if (DensityConductivity.Text.Length > 0)
                    densityConductivity = double.Parse(DensityConductivity.Text);
                material = new Model_Data.Material(materialId, conductivity, densityConductivity);
                model.Material.Add(materialId, material);
            }
            materialKeys?.Close();
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            materialKeys?.Close();
            Close();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (existingMaterial != null) model.Material.Remove(existingMaterial.MaterialId);
            materialKeys.Close();
            Close();
        }

        private void MaterialIdLostFocus(object sender, RoutedEventArgs e)
        {
            if (!model.Material.ContainsKey(MaterialId.Text))
            {
                ConductivityX.Text = "";
                ConductivityY.Text = "";
                ConductivityZ.Text = "";
                DensityConductivity.Text = "";
                return;
            }

            // vorhandene Materialdefinition
            model.Material.TryGetValue(MaterialId.Text, out existingMaterial);
            Debug.Assert(existingMaterial != null, nameof(existingMaterial) + " != null"); MaterialId.Text = "";

            MaterialId.Text = existingMaterial.MaterialId;

            ConductivityX.Text = existingMaterial.MaterialValues[0].ToString("G3", CultureInfo.CurrentCulture);
            ConductivityY.Text = existingMaterial.MaterialValues[1].ToString("G3", CultureInfo.CurrentCulture);
            ConductivityZ.Text = existingMaterial.MaterialValues[2].ToString("G3", CultureInfo.CurrentCulture);
            DensityConductivity.Text = existingMaterial.MaterialValues[3].ToString("G3", CultureInfo.CurrentCulture);
        }
    }
}