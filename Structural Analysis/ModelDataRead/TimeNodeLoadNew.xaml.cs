using FE_Analysis.Structural_Analysis.Model_Data;
using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System;
using System.Windows;
using static System.Globalization.CultureInfo;

namespace FE_Analysis.Structural_Analysis.ModelDataRead;

public partial class TimeNodeLoadNew
{
    private readonly FeModel model;

    public TimeNodeLoadNew(FeModel model)
    {
        InitializeComponent();
        this.model = model;
        LoadId.Text = string.Empty;
        NodeId.Text = string.Empty;
        NodalDof.Text = string.Empty;
        File.IsChecked = false;
        Amplitude.Text = string.Empty;
        Frequency.Text = string.Empty;
        Angle.Text = string.Empty;
        Linear.Text = string.Empty;
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var loadId = LoadId.Text;
        var nodeId = NodeId.Text;
        var knotenDof = int.Parse(NodalDof.Text);
        var zeitabhängigeKnotenlast =
            new TimeDependentNodeLoad(loadId, nodeId, knotenDof, false, false);

        if (File.IsChecked == true)
        {
            Amplitude.Text = string.Empty;
            Frequency.Text = string.Empty;
            Angle.Text = string.Empty;
            Linear.Text = string.Empty;
            zeitabhängigeKnotenlast.File = true;
            zeitabhängigeKnotenlast.VariationType = 0;
            var last = (AbstractTimeDependentNodeLoad)zeitabhängigeKnotenlast;
            model.TimeDependentNodeLoads.Add(loadId, last);
        }
        else if ((Amplitude.Text.Length & Frequency.Text.Length & Angle.Text.Length) != 0)
        {
            Linear.Text = string.Empty;
            File.IsChecked = false;
            zeitabhängigeKnotenlast.VariationType = 2;
            zeitabhängigeKnotenlast.Amplitude = double.Parse(Amplitude.Text, InvariantCulture);
            zeitabhängigeKnotenlast.Frequency = 2 * Math.PI * double.Parse(Frequency.Text, InvariantCulture);
            zeitabhängigeKnotenlast.PhaseAngle = Math.PI / 180 * double.Parse(Angle.Text, InvariantCulture);
        }
        else if (Linear.Text.Length != 0)
        {
            Amplitude.Text = string.Empty;
            Frequency.Text = string.Empty;
            Angle.Text = string.Empty;
            File.IsChecked = false;
            var delimiters = new[] { '\t' };
            var teilStrings = Linear.Text.Split(delimiters);
            var k = 0;
            char[] paarDelimiter = { ';' };
            var interval = new double[2 * (teilStrings.Length - 3)];
            for (var j = 3; j < teilStrings.Length; j++)
            {
                var wertePaar = teilStrings[j].Split(paarDelimiter);
                interval[k] = double.Parse(wertePaar[0], InvariantCulture);
                interval[k + 1] = double.Parse(wertePaar[1], InvariantCulture);
                k += 2;
            }

            zeitabhängigeKnotenlast.VariationType = 3;
            zeitabhängigeKnotenlast.Interval = interval;
            if (GroundExcitation.IsChecked == true) zeitabhängigeKnotenlast.GroundExcitation = true;
        }

        model.TimeDependentNodeLoads.Add(loadId, zeitabhängigeKnotenlast);
        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}