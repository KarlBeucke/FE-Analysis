using FE_Analysis.Heat_Transfer.Model_Data;
using FEALibrary.Model;
using System;
using System.Windows;

namespace FE_Analysis.Heat_Transfer.ModelDataRead
{
    public partial class TimeNewNodeLoad
    {
        private readonly FeModel model;

        public TimeNewNodeLoad(FeModel model)
        {
            InitializeComponent();
            this.model = model;
            LoadId.Text = "";
            NodeId.Text = "";
            File.IsChecked = false;
            Constant.Text = "";
            Amplitude.Text = "";
            Frequency.Text = "";
            Angle.Text = "";
            Linear.Text = "";
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var loadId = LoadId.Text;
            var knotenId = NodeId.Text;
            TimeDependentNodeLoad zeitabhängigeKnotentemperatur = null;

            if (File.IsChecked == true)
            {
                Constant.Text = "";
                Amplitude.Text = "";
                Frequency.Text = "";
                Angle.Text = "";
                Linear.Text = "";
                zeitabhängigeKnotentemperatur =
                    new TimeDependentNodeLoad(knotenId, true) { LoadId = loadId, VariationType = 0 };
            }
            else if (Constant.Text.Length != 0)
            {
                Amplitude.Text = "";
                Frequency.Text = "";
                Angle.Text = "";
                Linear.Text = "";
                File.IsChecked = false;
                var konstanteTemperatur = double.Parse(Constant.Text);
                zeitabhängigeKnotentemperatur =
                    new TimeDependentNodeLoad(knotenId, konstanteTemperatur) { LoadId = loadId, VariationType = 1 };
            }
            else if ((Amplitude.Text.Length & Frequency.Text.Length & Angle.Text.Length) != 0)
            {
                Constant.Text = "";
                Linear.Text = "";
                File.IsChecked = false;
                var amplitude = double.Parse(Amplitude.Text);
                var frequency = 2 * Math.PI * double.Parse(Frequency.Text);
                var phaseAngle = Math.PI / 180 * double.Parse(Angle.Text);
                zeitabhängigeKnotentemperatur =
                    new TimeDependentNodeLoad(knotenId, amplitude, frequency, phaseAngle)
                    { LoadId = loadId, VariationType = 2 };
            }
            else if (Linear.Text.Length != 0)
            {
                Constant.Text = "";
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

                zeitabhängigeKnotentemperatur = new TimeDependentNodeLoad(knotenId, interval)
                { LoadId = loadId, VariationType = 3 };
            }

            if (zeitabhängigeKnotentemperatur != null)
                model.TimeDependentNodeLoads.Add(loadId, zeitabhängigeKnotentemperatur);

            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}