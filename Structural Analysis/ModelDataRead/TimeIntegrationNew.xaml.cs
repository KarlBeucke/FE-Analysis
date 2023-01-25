using FE_Analysis.Structural_Analysis.Model_Data;
using FEALibrary.Model;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace FE_Analysis.Structural_Analysis.ModelDataRead;

public partial class TimeIntegrationNew
{
    private readonly FeModel model;
    public int current, eigenForm;
    private TimeNodalInitialConditionsNew initialValuesNew;
    public TimeIntegrationNew(FeModel model)
    {
        Language = XmlLanguage.GetLanguage("us-US");
        InitializeComponent();
        this.model = model;
        if (model.Eigenstate != null)
        {
            eigenForm = 1;
            Eigensolution.Text = model.Eigenstate.NumberOfStates.ToString(CultureInfo.InvariantCulture);
        }
        if (model.Eigenstate != null && model.Eigenstate.DampingRatios.Count > 0)
        {
            Eigenform.Text = "1";
            var modalValues = (ModalValues)model.Eigenstate.DampingRatios[0];
            var rate = modalValues.Damping;
            DampingRatio.Text = rate.ToString(CultureInfo.CurrentCulture);
        }
        if (model.Timeintegration != null)
        {
            MaximumTime.Text = model.Timeintegration.Tmax.ToString(CultureInfo.InvariantCulture);
            switch (model.Timeintegration.Method)
            {
                case 1:
                    Newmark.IsChecked = true;
                    Wilson.IsChecked = false; Taylor.IsChecked = false;
                    Beta.Text = model.Timeintegration.Parameter1.ToString(CultureInfo.InvariantCulture);
                    Gamma.Text = model.Timeintegration.Parameter2.ToString(CultureInfo.InvariantCulture);
                    Theta.Text = "";
                    Alfa.Text = "";
                    break;
                case 2:
                    Wilson.IsChecked = true;
                    Newmark.IsChecked = false; Taylor.IsChecked = false;
                    Beta.Text = ""; Gamma.Text = "";
                    Theta.Text = model.Timeintegration.Parameter1.ToString(CultureInfo.InvariantCulture);
                    Alfa.Text = "";
                    break;
                case 3:
                    Taylor.IsChecked = true;
                    Newmark.IsChecked = false; Wilson.IsChecked = false;
                    Beta.Text = ""; Gamma.Text = "";
                    Theta.Text = "";
                    Alfa.Text = model.Timeintegration.Parameter1.ToString(CultureInfo.InvariantCulture);
                    break;
            }
            TimeInterval.Text = model.Timeintegration.Dt.ToString(CultureInfo.InvariantCulture);
            Total.Text = model.Timeintegration.InitialConditions.Count.ToString(CultureInfo.CurrentCulture);
        }
        Show();
    }
    private void Newmark_OnChecked(object sender, RoutedEventArgs e)
    {
        Newmark.IsChecked = true;
        Wilson.IsChecked = false; Taylor.IsChecked = false;
        Beta.Text = 0.25.ToString("G5");
        Gamma.Text = 0.5.ToString("G5");
        Theta.Text = "";
        Alfa.Text = "";
    }
    private void Wilson_OnChecked(object sender, RoutedEventArgs e)
    {
        Wilson.IsChecked = true;
        Newmark.IsChecked = false; Taylor.IsChecked = false;
        Beta.Text = ""; Gamma.Text = "";
        Theta.Text = 1.420815.ToString("G5");
        Alfa.Text = "";
    }

    private void Taylor_OnChecked(object sender, RoutedEventArgs e)
    {
        Taylor.IsChecked = true;
        Newmark.IsChecked = false; Wilson.IsChecked = false;
        Beta.Text = ""; Gamma.Text = "";
        Theta.Text = "";
        Alfa.Text = (-0.1).ToString("G5");
    }

    private void TimeIntervalCalculate(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        int number;
        Analysis modelAnalysis = null;

        try { number = int.Parse(Eigensolution.Text, CultureInfo.InvariantCulture); }
        catch (FormatException) { _ = MessageBox.Show("number of eigensolutions has wrong format", "new TimeIntegration"); return; }

        if (MainWindow.modelAnalysis == null) modelAnalysis = new Analysis(model);
        model.Eigenstate ??= new Eigenstates("Tmin", number);

        if (model.Eigenstate.Eigenvalues == null)
        {
            if (!MainWindow.analysed)
            {
                modelAnalysis?.ComputeSystemMatrix();
                MainWindow.analysed = true;
            }
            model.Eigenstate = new Eigenstates("Tmin", number);
            if (modelAnalysis != null | !MainWindow.timeintegrationAnalysed) modelAnalysis?.Eigenstates();
        }

        var omegaMax = model.Eigenstate.Eigenvalues[number - 1];
        // smallest period for largest eigenvalue considered
        var tmin = 2 * Math.PI / Math.Sqrt(omegaMax);
        TimeInterval.Text = tmin.ToString("F3");
    }

    private void InitialConditionNext(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        current++;
        initialValuesNew ??= new TimeNodalInitialConditionsNew(model);
        if (model.Timeintegration.InitialConditions.Count < current)
        {
            initialValuesNew.NodeId.Text = "";
            initialValuesNew.Dof1D0.Text = ""; initialValuesNew.Dof1V0.Text = "";
            initialValuesNew.Dof2D0.Text = ""; initialValuesNew.Dof2V0.Text = "";
            initialValuesNew.Dof3D0.Text = ""; initialValuesNew.Dof3V0.Text = "";
            MainWindow.structuralModel.timeIntegrationNew.InitialCondition.Text = current.ToString(CultureInfo.CurrentCulture);
        }
        else
        {
            var nodalValues = (NodalValues)model.Timeintegration.InitialConditions[current - 1];
            MainWindow.structuralModel.timeIntegrationNew.InitialCondition.Text =
                current.ToString(CultureInfo.CurrentCulture);
            MainWindow.structuralModel.timeIntegrationNew.Show();

            initialValuesNew.NodeId.Text = nodalValues.NodeId;
            initialValuesNew.Dof1D0.Text = nodalValues.Values[0].ToString(CultureInfo.CurrentCulture);
            initialValuesNew.Dof1V0.Text = nodalValues.Values[1].ToString(CultureInfo.CurrentCulture);
            if (nodalValues.Values.Length > 2)
            {
                initialValuesNew.Dof2D0.Text = nodalValues.Values[2].ToString(CultureInfo.CurrentCulture);
                initialValuesNew.Dof2V0.Text = nodalValues.Values[3].ToString(CultureInfo.CurrentCulture);
            }

            if (nodalValues.Values.Length > 4)
            {
                initialValuesNew.Dof3D0.Text = nodalValues.Values[4].ToString(CultureInfo.CurrentCulture);
                initialValuesNew.Dof3V0.Text = nodalValues.Values[5].ToString(CultureInfo.CurrentCulture);
            }
            var start = current.ToString("D");
            MainWindow.structuralModel.timeIntegrationNew.InitialCondition.Text = start;
        }
    }

    private void DampingRatioNext(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        eigenForm++;
        MainWindow.structuralModel.timeIntegrationNew.Eigenform.Text =
            eigenForm.ToString(CultureInfo.CurrentCulture);
        _ = new TimeDampingRatioNew(model);

        var modalValues = (ModalValues)model.Eigenstate.DampingRatios[eigenForm - 1];
        MainWindow.structuralModel.timeIntegrationNew.DampingRatio.Text = modalValues.Damping.ToString(CultureInfo.CurrentCulture);
    }

    private void EigenformKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (Eigenform.Text.Length == 0) return;
        if (int.Parse(Eigenform.Text) > model.Eigenstate.NumberOfStates) return;
        eigenForm = int.Parse(Eigenform.Text);
        DampingRatio.Text = model.Eigenstate.DampingRatios[eigenForm].ToString();
    }

    private void EigenformGotFocus(object sender, RoutedEventArgs e)
    {
        Eigenform.Clear();
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
            short method;
            int numberEigensolutions;
            double dt, tmax;
            try { dt = double.Parse(TimeInterval.Text, CultureInfo.InvariantCulture); }
            catch (FormatException) { _ = MessageBox.Show("time interval deltaT has wrong format", "new TimeIntegration"); return; }

            try { tmax = double.Parse(MaximumTime.Text, CultureInfo.InvariantCulture); }
            catch (FormatException) { _ = MessageBox.Show("maximum integration time has wrong format", "new TimeIntegration"); return; }

            try { numberEigensolutions = int.Parse(Eigensolution.Text, CultureInfo.InvariantCulture); }
            catch (FormatException) { _ = MessageBox.Show("number of eigensolutions has wrong format", "new TimeIntegration"); return; }
            model.Eigenstate = new Eigenstates("eigen", numberEigensolutions);

            if (Newmark.IsChecked == true)
            {
                method = 1;
                double beta, gamma;
                try { beta = double.Parse(Beta.Text, CultureInfo.InvariantCulture); }
                catch (FormatException) { _ = MessageBox.Show("parameter Beta has wrong format", "new TimeIntegration"); return; }

                try { gamma = double.Parse(Gamma.Text, CultureInfo.InvariantCulture); }
                catch (FormatException) { _ = MessageBox.Show("parameter Gamma has wrong format", "new TimeIntegration"); return; }

                model.Timeintegration = new TimeIntegration(tmax, dt, method, beta, gamma);
            }
            else if (Wilson.IsChecked == true)
            {
                method = 2;
                double theta;
                try { theta = double.Parse(Theta.Text, CultureInfo.InvariantCulture); }
                catch (FormatException) { _ = MessageBox.Show("parameter Theta has wrong format", "new TimeIntegration"); return; }
                model.Timeintegration = new TimeIntegration(tmax, dt, method, theta);
            }
            else if (Taylor.IsChecked == true)
            {
                method = 3;
                double alfa;
                try { alfa = double.Parse(Alfa.Text, CultureInfo.InvariantCulture); }
                catch (FormatException) { _ = MessageBox.Show("parameter Alfa has wrong format", "new TimeIntegration"); return; }
                model.Timeintegration = new TimeIntegration(tmax, dt, method, alfa);
            }
            MainWindow.timeintegrationData = true;
        }
        else
        {
            try { model.Eigenstate.NumberOfStates = int.Parse(Eigensolution.Text, CultureInfo.InvariantCulture); }
            catch (FormatException) { _ = MessageBox.Show("number of eigenstates has wrong format", "new TimeIntegration"); return; }

            if (model.Timeintegration == null) return;
            try { model.Timeintegration.Dt = double.Parse(TimeInterval.Text, CultureInfo.InvariantCulture); }
            catch (FormatException) { _ = MessageBox.Show("time step has wrong format", "new TimeIntegration"); }

            try { model.Timeintegration.Tmax = double.Parse(MaximumTime.Text); }
            catch (FormatException) { _ = MessageBox.Show("maximum integration time has wrong format", "new TimeIntegration"); }

            if (DampingRatio.Text.Length == 0)
            {
                if (model.Eigenstate.DampingRatios.Count > 0) model.Eigenstate.DampingRatios.Clear();
                Eigenform.Text = "";
                eigenForm = 0;
            }
            else
            {
                try { model.Eigenstate.DampingRatios[0] = double.Parse(DampingRatio.Text); }
                catch (FormatException) { _ = MessageBox.Show("Damping ratio has wrong Format", "new TimeIntegration"); }
            }

            if (Newmark.IsChecked == true)
            {
                try { model.Timeintegration.Parameter1 = double.Parse(Beta.Text, CultureInfo.InvariantCulture); }
                catch (FormatException) { _ = MessageBox.Show("parameter beta has wrong format", "new TimeIntegration"); }

                try { model.Timeintegration.Parameter2 = double.Parse(Gamma.Text, CultureInfo.InvariantCulture); }
                catch (FormatException) { _ = MessageBox.Show("parameter gamma has wrong format", "new TimeIntegration"); }
            }
            else if (Wilson.IsChecked == true)
            {
                try { model.Timeintegration.Parameter1 = double.Parse(Theta.Text, CultureInfo.InvariantCulture); }
                catch (FormatException) { _ = MessageBox.Show("parameter theta has wrong format", "new TimeIntegration"); }
            }
            else if (Taylor.IsChecked == true)
            {
                try { model.Timeintegration.Parameter1 = double.Parse(Alfa.Text, CultureInfo.InvariantCulture); }
                catch (FormatException) { _ = MessageBox.Show("parameter alfa has wrong format", "new TimeIntegration"); }
            }
        }
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}