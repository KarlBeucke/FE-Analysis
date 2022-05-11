using FE_Analysis.Heat_Transfer.Model_Data;
using FEALibrary.Model;
using System;
using System.Windows;

namespace FE_Analysis.Heat_Transfer.ModelDataRead
{
    public partial class TimeNewBoundaryCondition
    {
        private readonly FeModel model;

        public TimeNewBoundaryCondition(FeModel model)
        {
            InitializeComponent();
            this.model = model;
            SupportId.Text = "";
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
            var supportId = SupportId.Text;
            var knotenId = NodeId.Text;
            TimeDependentBoundaryCondition timeDependentBoundaryCondition = null;

            if (File.IsChecked == true)
            {
                Constant.Text = "";
                Amplitude.Text = "";
                Frequency.Text = "";
                Angle.Text = "";
                Linear.Text = "";
                timeDependentBoundaryCondition =
                    new TimeDependentBoundaryCondition(knotenId, true)
                    { SupportId = supportId, VariationType = 0, Prescribed = new double[1] };
            }
            else if (Constant.Text.Length != 0)
            {
                File.IsChecked = false;
                Amplitude.Text = "";
                Frequency.Text = "";
                Angle.Text = "";
                Linear.Text = "";
                var konstanteTemperatur = double.Parse(Constant.Text);
                timeDependentBoundaryCondition =
                    new TimeDependentBoundaryCondition(knotenId, konstanteTemperatur)
                    { SupportId = supportId, VariationType = 1, Prescribed = new double[1] };
            }
            else if ((Amplitude.Text.Length & Frequency.Text.Length & Angle.Text.Length) != 0)
            {
                File.IsChecked = false;
                Constant.Text = "";
                Linear.Text = "";
                var amplitude = double.Parse(Amplitude.Text);
                var frequency = 2 * Math.PI * double.Parse(Frequency.Text);
                var phaseAngle = Math.PI / 180 * double.Parse(Angle.Text);
                timeDependentBoundaryCondition =
                    new TimeDependentBoundaryCondition(knotenId, amplitude, frequency, phaseAngle)
                    { SupportId = supportId, VariationType = 2, Prescribed = new double[1] };
            }
            else if (Linear.Text.Length != 0)
            {
                File.IsChecked = false;
                Constant.Text = "";
                Amplitude.Text = "";
                Frequency.Text = "";
                Angle.Text = "";

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

                timeDependentBoundaryCondition = new TimeDependentBoundaryCondition(knotenId, interval)
                { SupportId = supportId, VariationType = 3, Prescribed = new double[1] };
            }

            if (timeDependentBoundaryCondition != null)
                model.TimeDependentBoundaryConditions.Add(supportId, timeDependentBoundaryCondition);
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}