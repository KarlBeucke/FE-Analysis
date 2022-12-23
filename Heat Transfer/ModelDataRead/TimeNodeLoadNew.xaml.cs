using FEALibrary.Model;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using FEALibrary.Model.abstractClasses;
using System.Globalization;

namespace FE_Analysis.Heat_Transfer.ModelDataRead
{
    public partial class TimeNodeLoadNew
    {
        private readonly FeModel model;
        private AbstractTimeDependentNodeLoad existingNodeload;
        private string loadId;

        public TimeNodeLoadNew(FeModel model)
        {
            InitializeComponent();
            this.model = model;
            var loadKeys = new HeatloadKeys(model);
            loadKeys.Show();
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            loadId = LoadId.Text;
            if (loadId == "")
            {
                _ = MessageBox.Show("time dependent NodeLoad Id must be defined", "new time dependent NodeLoad");
                return;
            }

            // existing NodeLoad
            if (model.TimeDependentNodeLoads.Keys.Contains(loadId))
            {
                model.TimeDependentNodeLoads.TryGetValue(loadId, out existingNodeload);
                Debug.Assert(existingNodeload != null, nameof(existingNodeload) + " != null");

                if (NodeId.Text.Length > 0)
                    existingNodeload.NodeId = NodeId.Text.ToString(CultureInfo.CurrentCulture);

                if (File.IsChecked == true) existingNodeload.VariationType = 0;
                else if (Constant.Text.Length > 0)
                {
                    existingNodeload.VariationType = 1;
                    existingNodeload.ConstantTemperature = double.Parse(Constant.Text);
                }
                else if (Amplitude.Text.Length > 0 && Frequency.Text.Length > 0 && Angle.Text.Length > 0)
                {
                    existingNodeload.VariationType = 2;
                    existingNodeload.Amplitude = double.Parse(Amplitude.Text);
                    existingNodeload.Frequency = double.Parse(Frequency.Text);
                    existingNodeload.PhaseAngle = double.Parse(Angle.Text);
                }
                else if (Linear.Text.Length > 0)
                {
                    existingNodeload.VariationType = 3;
                    var delimiters = new[] { '\t' };
                    var teilStrings = Linear.Text.Split(delimiters);
                    var k = 0;
                    char[] paarDelimiter = { ';' };
                    var intervall = new double[2 * teilStrings.Length];
                    for (var i = 0; i < intervall.Length; i += 2)
                    {
                        var wertePaar = teilStrings[k].Split(paarDelimiter);
                        intervall[i] = double.Parse(wertePaar[0]);
                        intervall[i + 1] = double.Parse(wertePaar[1]);
                        k++;
                    }
                    existingNodeload.Interval = intervall;
                }
            }
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!model.TimeDependentNodeLoads.Keys.Contains(LoadId.Text)) return;
            model.TimeDependentNodeLoads.Remove(LoadId.Text);
            Close();
            MainWindow.heatModel.Close();
        }

        private void LoadIdLostFocus(object sender, RoutedEventArgs e)
        {
            NodeId.Text = "";
            File.IsChecked = false;
            Constant.Text = "";
            Amplitude.Text = "";
            Frequency.Text = "";
            Angle.Text = "";
            Linear.Text = "";

            // new element definition
            if (!model.TimeDependentNodeLoads.ContainsKey(LoadId.Text)) return;

            // existing element definitionse
            model.TimeDependentNodeLoads.TryGetValue(LoadId.Text, out var existingLoad);
            Debug.Assert(existingLoad != null, nameof(existingLoad) + " != null"); LoadId.Text = "";

            NodeId.Text = existingLoad.NodeId;
            switch (existingLoad.VariationType)
            {
                case 0:
                    File.IsChecked = true;
                    break;
                case 1:
                    existingLoad.ConstantTemperature = double.Parse(Constant.Text);
                    break;
                case 2:
                    existingLoad.Amplitude = double.Parse(Amplitude.Text);
                    existingLoad.Frequency = double.Parse(Frequency.Text);
                    existingLoad.PhaseAngle = double.Parse(Angle.Text);
                    break;
                case 3:
                {
                    var delimiters = new[] { '\t' };
                    var partialString = Linear.Text.Split(delimiters);
                    var k = 0;
                    char[] pairDelimiter = { ';' };
                    var interval = new double[2 * (partialString.Length - 3)];
                    for (var j = 3; j < partialString.Length; j++)
                    {
                        var valuePair = partialString[j].Split(pairDelimiter);
                        interval[k] = double.Parse(valuePair[0]);
                        interval[k + 1] = double.Parse(valuePair[1]);
                        k += 2;
                    }
                    existingLoad.Interval = interval;
                    break;
                }
            }
        }
    }
}