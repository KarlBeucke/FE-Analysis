using FEALibrary.Model;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace FE_Analysis.Heat_Transfer.ModelDataShow;

public partial class HeatExcitationVisualize
{
    private readonly Presentation presentation;
    private readonly double dt, tmax, tmin;
    private double excitationMax, excitationMin;
    private readonly double[] excitation;

    public HeatExcitationVisualize(FeModel feModel)
    {
        Language = XmlLanguage.GetLanguage("us-US");
        InitializeComponent();
        Show();

        // determination of time axis
        dt = feModel.Timeintegration.Dt;
        tmin = 0;
        tmax = feModel.Timeintegration.Tmax;
        var nSteps = (int)(tmax / dt) + 1;
        excitation = new double[nSteps];

        // Initialization of drawing canvas
        presentation = new Presentation(feModel, VisualExcitation);
    }

    private void BtnExcitation_Click(object sender, RoutedEventArgs e)
    {
        const string inputDirectory = "\\FE-Analysis-App\\input\\HeatTransfer\\instationary\\ExcitationFiles";
        // read ordinate values in time interval dt from file
        MainWindow.modelAnalysis.FromFile(inputDirectory,1, excitation);
        excitationMax = excitation.Max();
        excitationMin = -excitationMax;

        // text representation of duration of excitation with number of data points and time interval
        ExcitationText(excitation.Length * dt, excitation.Length);

        presentation.CoordinateSystem(tmin, tmax, excitationMax, excitationMin);
        presentation.TimeHistoryDraw(dt, tmin, tmax, excitationMax, excitation);
    }
    private void ExcitationText(double duration, int nSteps)
    {
        var excitationValues = duration.ToString("N0") + "[s] resp. " + (duration/60/60).ToString("N0") + "[h] excitation with "
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