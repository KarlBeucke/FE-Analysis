using FE_Analysis.Heat_Transfer.Model_Data;
using FEALibrary.Model;
using System;
using System.Collections.Generic;
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
    private FeModel model;

    public HeatExcitationVisualize(FeModel feModel)
    {
        Language = XmlLanguage.GetLanguage("us-US");
        model = feModel;
        InitializeComponent();
        Show();

        // determination of time axis
        dt = model.Timeintegration.Dt;
        tmin = 0;
        tmax = model.Timeintegration.Tmax;
        var nSteps = (int)(tmax / dt) + 1;
        excitation = new double[nSteps];

        // Initialization of drawing canvas
        presentation = new Presentation(model, VisualExcitation);
    }

    private void BtnExcitation_Click(object sender, RoutedEventArgs e)
    {
        const string inputDirectory = "\\FE-Analysis-App\\input\\HeatTransfer\\instationary\\ExcitationFiles";
        // read ordinate values in time interval dt from file
        MainWindow.modelAnalysis.FromFile(inputDirectory, 1, excitation);
        excitationMax = excitation.Max();
        excitationMin = -excitationMax;

        // text representation of duration of excitation with number of data points and time interval
        ExcitationText(excitation.Length * dt, excitation.Length);

        presentation.CoordinateSystem(tmin, tmax, excitationMax, excitationMin);
        presentation.TimeHistoryDraw(dt, tmin, tmax, excitationMax, excitation);
    }
    private void ExcitationText(double duration, int nSteps)
    {
        var excitationValues = duration.ToString("N0") + "[s] resp. " + (duration / 60 / 60).ToString("N0") + "[h] excitation with "
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
    private void BtnIntervals_Click(object sender, RoutedEventArgs e)
    {
        var boundaryLinear = (from item
                in model.TimeDependentBoundaryConditions
                              where item.Value.VariationType == 3
                              select item.Value).ToList();
        foreach (var item in boundaryLinear)
        {
            _ = MessageBox.Show("time dependent boundary condition at node " + item.NodeId, "Heat Transfer Analysis");
            var timeDependentBc = (TimeDependentBoundaryCondition)item;
            var interval = timeDependentBc.Interval;
            PiecewiseLinear(interval, excitation);

            excitationMax = excitation.Max();
            excitationMin = -excitationMax;

            // text representation of duration of excitation with number of data points and time interval
            ExcitationText(excitation.Length * dt, excitation.Length);

            presentation.CoordinateSystem(tmin, tmax, excitationMax, excitationMin);
            presentation.TimeHistoryDraw(dt, tmin, tmax, excitationMax, excitation);
        }

    }
    private void PiecewiseLinear(IReadOnlyList<double> interval, IList<double> force)
    {
        int counter = 0, nSteps = force.Count;
        double endLoad = 0;
        var startTime = interval[0];
        var startLoad = interval[1];
        force[counter] = startLoad;
        for (var j = 2; j < interval.Count; j += 2)
        {
            var endTime = interval[j];
            endLoad = interval[j + 1];
            var stepsPerInterval = (int)Math.Round((endTime - startTime) / dt);
            var increment = (endLoad - startLoad) / stepsPerInterval;
            for (var k = 1; k <= stepsPerInterval; k++)
            {
                counter++;
                if (counter == nSteps) return;
                force[counter] = force[counter - 1] + increment;
            }

            startTime = endTime;
            startLoad = endLoad;
        }

        for (var k = counter + 1; k < nSteps; k++) force[k] = endLoad;
    }

}