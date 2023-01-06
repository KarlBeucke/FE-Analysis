using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using FEALibrary.Model;

namespace FE_Analysis.Structural_Analysis.ModelDataShow;

public partial class ExcitationVisualize
{
    private readonly Presentation presentation;
    private readonly double dt, tmax, tmin;
    private double excitationMax, excitationMin;
    private IList<double> values;
    //private double[] excitation;

    public ExcitationVisualize(FeModel feModel)
    {
        Language = XmlLanguage.GetLanguage("us-US");
        InitializeComponent();
        Show();

        // determination of time axis
        dt = feModel.Timeintegration.Dt;
        tmin = 0;
        tmax = feModel.Timeintegration.Tmax;

        // Initialization of drawing canvas
        presentation = new Presentation(feModel, VisualExcitation);
    }

    private void BtnExcitation_Click(object sender, RoutedEventArgs e)
    {
        const string inputDirectory = "\\FE-Analysis-App\\input\\StructuralAnalysis\\Dynamics\\ExcitationFiles";
        // read ordinate values in time interval dt from file
        values = MainWindow.modelAnalysis.FromFile(inputDirectory);
        excitationMax = values.Max();
        excitationMin = -excitationMax;

        // text representation of duration of excitation with number of data points and time interval
        ExcitationText(values.Count * dt, values.Count);

        var excitation = new double[values.Count];
        for (var i = 0; i < values.Count; i++) excitation[i] = values[i];
        presentation.CoordinateSystem(tmin, tmax, excitationMax, excitationMin);
        presentation.TimeHistoryDraw(dt, tmin, tmax, excitationMax, excitation);
    }
    private void ExcitationText(double duration, int nSteps)
    {
        var excitationValues = duration.ToString("N2") + " [s] excitation with "
                             + nSteps + " excitation values at time interval dt = " + dt.ToString("N3");
        var excitationTextBlock = new TextBlock
        {
            FontSize = 12,
            Foreground = Brushes.Black,
            FontWeight = FontWeights.Bold,
            Text = excitationValues
        };
        Canvas.SetTop(excitationTextBlock, 10);
        Canvas.SetLeft(excitationTextBlock, 20);
        VisualExcitation.Children.Add(excitationTextBlock);
    }
}