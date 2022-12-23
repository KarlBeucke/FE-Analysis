using FEALibrary.Model;
using System.Globalization;
using System.Windows;

namespace FE_Analysis.Structural_Analysis.ModelDataRead;

public partial class TimeDampingRatioNew
{
    private readonly FeModel model;
    private int eigenform;

    public TimeDampingRatioNew(FeModel model)
    {
        InitializeComponent();
        this.model = model;
        eigenform = MainWindow.structuralModel.timeIntegrationNew.eigenForm;
        if (eigenform > model.Eigenstate.DampingRatios.Count)
        {
            Xi.Text = "";
        }
        else
        {
            var start = (ModalValues)model.Eigenstate.DampingRatios[eigenform - 1];
            Xi.Text = start.Damping.ToString(CultureInfo.CurrentCulture);
        }
        ShowDialog();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        // add new damping ratio
        if (eigenform > model.Eigenstate.DampingRatios.Count)
        {
            model.Eigenstate.DampingRatios.Add(new ModalValues(double.Parse(Xi.Text)));
        }
        // edit existing damping ratio
        else
        {
            var start = (ModalValues)model.Eigenstate.DampingRatios[eigenform];
            start.Damping = double.Parse(Xi.Text);
        }
        Close();

        model.Eigenstate.DampingRatios.
            Add(new ModalValues(double.Parse(Xi.Text, CultureInfo.InvariantCulture)));
        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
        MainWindow.structuralModel.timeIntegrationNew.Close();
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        model.Eigenstate.DampingRatios.RemoveAt(eigenform);
        eigenform = 0;
        if (model.Eigenstate.DampingRatios.Count <= 0)
        {
            Close();
            MainWindow.structuralModel.timeIntegrationNew.Close();
            return;
        }
        var startValues = (ModalValues)model.Eigenstate.DampingRatios[eigenform];
        Xi.Text = startValues.Damping.ToString("G2");
        Close();
        MainWindow.structuralModel.timeIntegrationNew.Close();
    }
}