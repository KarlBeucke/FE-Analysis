using FE_Analysis.Structural_Analysis.Model_Data;
using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System;
using System.Windows;

namespace FE_Analysis.Structural_Analysis.ModelDataRead
{
    public partial class NewTimeNodeLoad
    {
        private readonly FeModel model;

        public NewTimeNodeLoad(FeModel model)
        {
            InitializeComponent();
            this.model = model;
            LoadId.Text = "";
            NodeId.Text = "";
            NodalDof.Text = "";
            File.IsChecked = false;
            Amplitude.Text = "";
            Frequency.Text = "";
            Angle.Text = "";
            Linear.Text = "";
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
                Amplitude.Text = "";
                Frequency.Text = "";
                Angle.Text = "";
                Linear.Text = "";
                zeitabhängigeKnotenlast.File = true;
                zeitabhängigeKnotenlast.VariationType = 0;
                var last = (AbstractTimeDependentNodeLoad)zeitabhängigeKnotenlast;
                model.TimeDependentNodeLoads.Add(loadId, last);
            }
            else if ((Amplitude.Text.Length & Frequency.Text.Length & Angle.Text.Length) != 0)
            {
                Linear.Text = "";
                File.IsChecked = false;
                zeitabhängigeKnotenlast.VariationType = 2;
                zeitabhängigeKnotenlast.Amplitude = double.Parse(Amplitude.Text);
                zeitabhängigeKnotenlast.Frequency = 2 * Math.PI * double.Parse(Frequency.Text);
                zeitabhängigeKnotenlast.PhaseAngle = Math.PI / 180 * double.Parse(Angle.Text);
            }
            else if (Linear.Text.Length != 0)
            {
                Amplitude.Text = "";
                Frequency.Text = "";
                Angle.Text = "";
                File.IsChecked = false;
                var delimiters = new[] { '\t' };
                var teilStrings = Linear.Text.Split(delimiters);
                var k = 0;
                char[] paarDelimiter = { ';' };
                var interval = new double[2 * (teilStrings.Length - 3)];
                for (var j = 3; j < teilStrings.Length; j++)
                {
                    var wertePaar = teilStrings[j].Split(paarDelimiter);
                    interval[k] = double.Parse(wertePaar[0]);
                    interval[k + 1] = double.Parse(wertePaar[1]);
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
}