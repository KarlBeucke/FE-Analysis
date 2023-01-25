using FE_Analysis.Heat_Transfer.Model_Data;
using FEALibrary.Model;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace FE_Analysis.Heat_Transfer.ModelDataRead;

public partial class TimeIntegrationNew
{
    private readonly FeModel model;
    public int current;
    private TimeInitialTemperatureNew initialTemperaturesNew;
    public TimeIntegrationNew(FeModel model)
    {
        Language = XmlLanguage.GetLanguage("us-US");
        InitializeComponent();
        this.model = model;
        if (model.Eigenstate != null)
            Eigensolution.Text = model.Eigenstate.NumberOfStates.ToString(CultureInfo.InvariantCulture);
        if (model.Timeintegration != null)
        {
            TimeInterval.Text = model.Timeintegration.Dt.ToString(CultureInfo.InvariantCulture);
            MaximumTime.Text = model.Timeintegration.Tmax.ToString(CultureInfo.InvariantCulture);
            Total.Text = model.Timeintegration.InitialConditions.Count.ToString(CultureInfo.CurrentCulture);
            Parameter.Text = model.Timeintegration.Parameter1.ToString(CultureInfo.InvariantCulture);
            InitialCondition.Text = model.Timeintegration.FromStationary ? "stationary solution" : "";
        }
        Show();
    }

    private void TimeIntervalCalculate(object sender, MouseButtonEventArgs e)
    {
        var number = int.Parse(Eigensolution.Text);
        string betaM;
        var modelAnalysis = new Analysis(model);
        model.Eigenstate ??= new Eigenstates("new", number);

        if (model.Eigenstate.Eigenvalues == null)
        {
            if (!MainWindow.analysed)
            {
                modelAnalysis.ComputeSystemMatrix();
                MainWindow.analysed = true;
            }
            model.Eigenstate = new Eigenstates("neu", number);
            modelAnalysis.Eigenstates();
        }

        var alfa = double.Parse(Parameter.Text);
        var betaMax = model.Eigenstate.Eigenvalues[number - 1];
        if (alfa < 0.5)
        {
            var deltatCrit = 2 / (betaMax * (1 - 2 * alfa));
            TimeInterval.Text = deltatCrit.ToString(CultureInfo.InvariantCulture);
            betaM = betaMax.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            TimeInterval.Text = "";
            betaM = betaMax.ToString(CultureInfo.InvariantCulture);
        }

        _ = MessageBox.Show("critical time step for β max = " + betaM
                            + " is unrestricted for stability and must be set for accuracy", "TimeIntegration");
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        model.Timeintegration = null;
        Close();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        if (TimeInterval.Text == "")
        {
            _ = MessageBox.Show("critical time step is unrestricted for stability and must be set for accuracy", "TimeIntegration");
            return;
        }

        if (model.Timeintegration == null)
        {
            int numberEigensolutions;
            try { numberEigensolutions = int.Parse(Eigensolution.Text, CultureInfo.InvariantCulture); }
            catch (FormatException) { _ = MessageBox.Show("number of eigensolutions has wrong format", "new TimeIntegration"); return; }

            double dt;
            try { dt = double.Parse(TimeInterval.Text, CultureInfo.InvariantCulture); }
            catch (FormatException) { _ = MessageBox.Show("time interval of integration has wrong format", "new TimeIntegration"); return; }

            double tmax;
            try { tmax = double.Parse(MaximumTime.Text, CultureInfo.InvariantCulture); }
            catch (FormatException) { _ = MessageBox.Show("maximum integration time has wrong format", "new TimeIntegration"); return; }

            double alfa;
            try { alfa = double.Parse(Parameter.Text, CultureInfo.InvariantCulture); }
            catch (FormatException) { _ = MessageBox.Show("parameter alfa has wrong format", "new TimeIntegration"); return; }

            model.Eigenstate = new Eigenstates("eigen", numberEigensolutions);
            model.Timeintegration = new TimeIntegration(tmax, dt, alfa) { FromStationary = false };
            MainWindow.timeintegrationData = true;
        }
        else
        {
            try { model.Eigenstate.NumberOfStates = int.Parse(Eigensolution.Text, CultureInfo.InvariantCulture); }
            catch (FormatException) { _ = MessageBox.Show("number of eigensolutions has wrong format", "new TimeIntegration"); return; }

            try { model.Timeintegration.Dt = double.Parse(TimeInterval.Text, CultureInfo.InvariantCulture); }
            catch (FormatException) { _ = MessageBox.Show("time interval of integration has wrong format", "new TimeIntegration"); return; }

            try { model.Timeintegration.Tmax = double.Parse(MaximumTime.Text, CultureInfo.InvariantCulture); }
            catch (FormatException) { _ = MessageBox.Show("maximum integration time has wrong format", "new TimeIntegration"); return; }

            try { model.Timeintegration.Parameter1 = double.Parse(Parameter.Text, CultureInfo.InvariantCulture); }
            catch (FormatException) { _ = MessageBox.Show("parameter alfa has wrong format", "new TimeIntegration"); return; }
        }
        MainWindow.heatModel.presentation.AnfangsbedingungenEntfernen();
        initialTemperaturesNew?.Close();
        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        initialTemperaturesNew?.Close();
        Close();
    }

    private void InitialConditionNext(object sender, MouseButtonEventArgs e)
    {
        current++;
        initialTemperaturesNew ??= new TimeInitialTemperatureNew(model);
        if (model.Timeintegration.InitialConditions.Count < current)
        {
            initialTemperaturesNew.NodeId.Text = "";
            initialTemperaturesNew.InitialTemperature.Text = "";
            MainWindow.heatModel.timeIntegrationNew.InitialCondition.Text = current.ToString(CultureInfo.CurrentCulture);
        }
        else
        {
            var nodalValues = (NodalValues)model.Timeintegration.InitialConditions[current - 1];
            MainWindow.heatModel.timeIntegrationNew.InitialCondition.Text =
                current.ToString(CultureInfo.CurrentCulture);
            MainWindow.heatModel.timeIntegrationNew.Show();
            if (model.Timeintegration.FromStationary)
            {
                initialTemperaturesNew.StationarySolution.IsChecked = true;
            }

            else if (nodalValues.NodeId == "all")
            {
                initialTemperaturesNew.NodeId.Text = "alle";
                initialTemperaturesNew.InitialTemperature.Text = nodalValues.Values[0].ToString(CultureInfo.CurrentCulture);
            }
            else
            {
                initialTemperaturesNew.NodeId.Text = nodalValues.NodeId;
                initialTemperaturesNew.InitialTemperature.Text = nodalValues.Values[0].ToString(CultureInfo.CurrentCulture);
                var start = current.ToString("D");
                MainWindow.heatModel.timeIntegrationNew.InitialCondition.Text = start;
                MainWindow.heatModel.presentation.InitialConditionsDraw(nodalValues.NodeId, nodalValues.Values[0], start);
            }
        }
    }
}