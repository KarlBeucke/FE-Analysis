using FE_Analysis.Structural_Analysis.Model_Data;
using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace FE_Analysis.Structural_Analysis.ModelDataRead;

public partial class MaterialNew
{
    private readonly FeModel model;
    private AbstractMaterial material, existingMaterial;
    private readonly MaterialKeys materialKeys;

    public MaterialNew(FeModel model)
    {
        InitializeComponent();
        this.model = model;
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

        // existing Material
        if (model.Material.Keys.Contains(MaterialId.Text))
        {
            model.Material.TryGetValue(materialId, out existingMaterial);
            Debug.Assert(existingMaterial != null, nameof(existingMaterial) + " != null");

            if (EModulus.Text.Length > 0) existingMaterial.MaterialValues[0] = double.Parse(EModulus.Text);
            if (Poisson.Text.Length > 0) existingMaterial.MaterialValues[1] = double.Parse(Poisson.Text);
            if (Mass.Text.Length > 0) existingMaterial.MaterialValues[2] = double.Parse(Mass.Text);
            if (SpringX.Text.Length > 0) existingMaterial.MaterialValues[3] = double.Parse(SpringX.Text);
            if (SpringY.Text.Length > 0) existingMaterial.MaterialValues[4] = double.Parse(SpringY.Text);
            if (SpringPhi.Text.Length > 0) existingMaterial.MaterialValues[5] = double.Parse(SpringPhi.Text);
        }
        // new Material
        else
        {
            if (EModulus.Text.Length > 0)
            {
                var eModulus = double.Parse(EModulus.Text);
                double poisson = 0, mass = 0;
                if (Poisson.Text.Length > 0) poisson = double.Parse(Poisson.Text);
                if (Mass.Text.Length > 0) mass = double.Parse(Mass.Text);
                material = new Material(eModulus, poisson, mass)
                {
                    MaterialId = materialId
                };
                model.Material.Add(materialId, material);
                SpringX.Text = "";
                SpringY.Text = "";
                SpringPhi.Text = "";
            }
            else if (SpringX.Text.Length > 0 | SpringY.Text.Length > 0 | SpringPhi.Text.Length > 0)
            {
                EModulus.Text = "";
                Poisson.Text = "";
                Mass.Text = "";
                double springX = 0, springY = 0, springPhi = 0;
                if (SpringX.Text.Length > 0) springX = double.Parse(SpringX.Text);
                if (SpringY.Text.Length > 0) springY = double.Parse(SpringY.Text);
                if (SpringPhi.Text.Length > 0) springPhi = double.Parse(SpringPhi.Text);
                material = new Material(true, springX, springY, springPhi)
                {
                    MaterialId = materialId
                };
                model.Material.Add(materialId, material);
            }
            else
            {
                _ = MessageBox.Show("either E-Modulus or 1 spring stiffness must be defined", "neues Material");
                return;
            }
        }
        materialKeys?.Close();
        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        materialKeys?.Close();
        Close();
    }

    private void MaterialIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!model.Material.ContainsKey(MaterialId.Text))
        {
            EModulus.Text = "";
            Poisson.Text = "";
            Mass.Text = "";
            SpringX.Text = "";
            SpringY.Text = "";
            SpringPhi.Text = "";
            return;
        }

        // existing material definitions
        model.Material.TryGetValue(MaterialId.Text, out existingMaterial);
        Debug.Assert(existingMaterial != null, nameof(existingMaterial) + " != null"); MaterialId.Text = "";

        MaterialId.Text = existingMaterial.MaterialId;
        if (!existingMaterial.Spring)
        {
            EModulus.Text = existingMaterial.MaterialValues[0].ToString("G3", CultureInfo.CurrentCulture);
            if (Poisson.Text == "") Poisson.Text = existingMaterial.MaterialValues[1].ToString("G3", CultureInfo.CurrentCulture);
            Mass.Text = existingMaterial.MaterialValues[2].ToString("G3", CultureInfo.CurrentCulture);
            SpringX.Text = "";
            SpringY.Text = "";
            SpringPhi.Text = "";
        }
        else
        {
            EModulus.Text = "";
            Poisson.Text = "";
            Mass.Text = "";
            SpringX.Text = existingMaterial.MaterialValues[0].ToString("G3", CultureInfo.CurrentCulture);
            SpringY.Text = existingMaterial.MaterialValues[1].ToString("G3", CultureInfo.CurrentCulture);
            SpringPhi.Text = existingMaterial.MaterialValues[2].ToString("G3", CultureInfo.CurrentCulture);
        }
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (existingMaterial != null) model.Material.Remove(existingMaterial.MaterialId);
        materialKeys.Close();
        Close();
    }
}