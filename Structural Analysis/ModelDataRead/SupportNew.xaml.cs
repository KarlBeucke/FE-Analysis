using FE_Analysis.Structural_Analysis.Model_Data;
using FEALibrary.Model;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace FE_Analysis.Structural_Analysis.ModelDataRead;

public partial class SupportNew
{
    private readonly FeModel model;
    private readonly SupportKeys supportKeys;

    public SupportNew(FeModel model)
    {
        InitializeComponent();
        this.model = model;
        supportKeys = new SupportKeys(model);
        supportKeys.Show();
        Show();
    }

    public SupportNew(FeModel model, double preDefX, double preDefY, double preDefRot)
    {
        InitializeComponent();
        this.model = model;
        PreX.Text = preDefX.ToString("0.00");
        PreY.Text = preDefY.ToString("0.00");
        PreRot.Text = preDefRot.ToString("0.00");
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var supportId = SupportId.Text;
        if (supportId == "")
        {
            _ = MessageBox.Show("Support Id must be defined", "new Support");
            return;
        }

        // existing Support
        if (model.BoundaryConditions.Keys.Contains(supportId))
        {
            model.BoundaryConditions.TryGetValue(supportId, out var support);
            Debug.Assert(support != null, nameof(support) + " != null");

            if (SupportId.Text.Length > 0) support.SupportId = NodeId.Text.ToString(CultureInfo.CurrentCulture);

            support.Restrained[0] = false;
            support.Restrained[1] = false;
            support.Restrained[2] = false;
            if (Xfixed.IsChecked != null && (bool)Xfixed.IsChecked) support.Restrained[0] = true;
            if (Yfixed.IsChecked != null && (bool)Yfixed.IsChecked) support.Restrained[1] = true;
            if (Rfixed.IsChecked != null && (bool)Rfixed.IsChecked) support.Restrained[2] = true;
            support.Type = 0;
            if (support.Restrained[0]) support.Type = Support.X_FIXED;
            if (support.Restrained[1]) support.Type += Support.Y_FIXED;
            if (support.Restrained[2]) support.Type += Support.R_FIXED;

            if (PreX.Text.Length > 0) support.Prescribed[0] = double.Parse(PreX.Text);
            if (PreY.Text.Length > 0) support.Prescribed[1] = double.Parse(PreY.Text);
            if (PreRot.Text.Length > 0) support.Prescribed[2] = double.Parse(PreRot.Text);
        }
        // new Supportr
        else
        {
            var prescribed = new double[3];
            if (PreX.Text.Length > 0) prescribed[0] = double.Parse(PreX.Text);
            if (PreY.Text.Length > 0) prescribed[1] = double.Parse(PreY.Text);
            if (PreRot.Text.Length > 0) prescribed[2] = double.Parse(PreRot.Text);
            var typ = 0;
            if (Xfixed.IsChecked != null && (bool)Xfixed.IsChecked) typ = Support.X_FIXED;
            if (Yfixed.IsChecked != null && (bool)Yfixed.IsChecked) typ += Support.Y_FIXED;
            if (Rfixed.IsChecked != null && (bool)Rfixed.IsChecked) typ += Support.R_FIXED;
            var support = new Support(NodeId.Text, typ, prescribed, model) { SupportId = supportId };
            model.BoundaryConditions.Add(supportId, support);
        }

        supportKeys?.Close();
        Close();
        MainWindow.structuralModel.Close();
    }
    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        supportKeys?.Close();
        Close();
    }

    private void SupportIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!model.BoundaryConditions.ContainsKey(SupportId.Text))
        {
            NodeId.Text = "";
            Xfixed.IsChecked = false; Yfixed.IsChecked = false; Rfixed.IsChecked = false;
            return;
        }

        // existing Support definitions
        model.BoundaryConditions.TryGetValue(SupportId.Text, out var support);
        Debug.Assert(support != null, nameof(support) + " != null");

        SupportId.Text = support.SupportId;
        NodeId.Text = support.NodeId;
        Xfixed.IsChecked = false; Yfixed.IsChecked = false; Rfixed.IsChecked = false;
        if (support.Restrained[0]) Xfixed.IsChecked = true;
        if (support.Restrained[1]) Yfixed.IsChecked = true;
        if (support.Restrained[2]) Rfixed.IsChecked = true;
        PreX.Text = support.Prescribed[0].ToString("N2", CultureInfo.CurrentCulture);
        PreY.Text = support.Prescribed[1].ToString("N2", CultureInfo.CurrentCulture);
        PreRot.Text = support.Prescribed[2].ToString("N2", CultureInfo.CurrentCulture);
    }
    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (!model.BoundaryConditions.Keys.Contains(SupportId.Text)) return;
        model.BoundaryConditions.Remove(SupportId.Text);
        supportKeys?.Close();
        Close();
        MainWindow.structuralModel.Close();
        supportKeys?.Close();
    }
}