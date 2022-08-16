using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Shapes;
using static System.Windows.Controls.Canvas;
using static System.Windows.Media.Brushes;

namespace FE_Analysis.Structural_Analysis.Results
{
    public partial class DynamicModelStatesVisualize
    {
        private readonly Presentation presentation;
        private readonly double dt;
        private readonly FeModel model;
        private readonly int nSteps;
        private int dropDownIndex, index, indexN, indexQ, indexM;

        private bool elementTextsOn = true,
            nodeTextsOn = true,
            deformationsOn,
            axialForcesOn,
            shearForcesOn,
            bendingMomentsOn;

        private TextBlock maximumValues;
        private double maxAxialForce, maxShearForce, maxBendingMoment;

        public DynamicModelStatesVisualize(FeModel feModel)
        {
            Language = XmlLanguage.GetLanguage("de-DE");
            model = feModel;

            InitializeComponent();
            Show();

            // Selection of Time Step
            dt = model.Timeintegration.Dt;
            var tmax = model.Timeintegration.Tmax;

            // Selection of time step from a grid, e.g. each 10th
            nSteps = (int)(tmax / dt);
            const int timeGrid = 1;
            //if (nSteps > 1000) timeGrid = 10;
            nSteps = nSteps / timeGrid + 1;
            var zeit = new double[nSteps];
            for (var i = 0; i < nSteps; i++) zeit[i] = i * dt * timeGrid;

            presentation = new Presentation(model, VisualResults);
            presentation.UndeformedGeometry();

            // with Node and Element Ids
            presentation.NodeTexts();
            presentation.ElementTexts();

            MaximumValuesOfTimeHistory();
            TimeStepSelection.ItemsSource = zeit;
        }

        private void DropDownTimeStepSelectionClosed(object sender, EventArgs e)
        {
            if (TimeStepSelection.SelectedIndex < 0)
            {
                _ = MessageBox.Show("no valid time step selected", "TimeStepSelection");
                return;
            }

            dropDownIndex = TimeStepSelection.SelectedIndex;
            index = dropDownIndex;
            foreach (var item in model.Nodes)
                for (var i = 0; i < item.Value.NumberOfNodalDof; i++)
                    item.Value.NodalDof[i] = item.Value.NodalVariables[i][index];

            deformationsOn = false;
            axialForcesOn = false;
            shearForcesOn = false;
            bendingMomentsOn = false;
        }

        private void BtnDeformations_Click(object sender, RoutedEventArgs e)
        {
            if (index == 0)
            {
                _ = MessageBox.Show("Time Step must be selected first", "Structural Dynamic Analysis");
                return;
            }

            if (deformationsOn)
            {
                foreach (Shape path in presentation.Deformations) 
                    VisualResults.Children.Remove(path);
                index++;
                CurrentTimeStep.Text =
                    "current integration time = " + (index * dt).ToString(CultureInfo.InvariantCulture);
                if (index >= nSteps)
                {
                    _ = MessageBox.Show("end of time history analysis", "Structural Dynamic Analysis");
                    index = dropDownIndex;
                    deformationsOn = false;
                    return;
                }
            }
            else
            {
                index = dropDownIndex;
                Clean();
            }

            foreach (var item in model.Nodes)
                for (var i = 0; i < item.Value.NumberOfNodalDof; i++)
                    item.Value.NodalDof[i] = item.Value.NodalVariables[i][index];
            presentation.DeformedGeometry();
            deformationsOn = true;
            axialForcesOn = false;
            shearForcesOn = false;
            bendingMomentsOn = false;
        }

        private void BtnAxialForces_Click(object sender, RoutedEventArgs e)
        {
            if (index == 0)
            {
                _ = MessageBox.Show("time stept must be selected first", "Structural Dynamic Analysis");
                return;
            }

            if (axialForcesOn)
            {
                foreach (Shape path in presentation.AxialForceList) 
                    VisualResults.Children.Remove(path);
                index++;
                CurrentTimeStep.Text =
                    "current time of integration = " + (index * dt).ToString(CultureInfo.InvariantCulture);
                if (index >= nSteps)
                {
                    _ = MessageBox.Show("End of Time History Analysis", "Structural Dynamic Analysis");
                    index = dropDownIndex;
                    axialForcesOn = false;
                    return;
                }
            }
            else
            {
                index = dropDownIndex;
                Clean();
            }

            foreach (var item in model.Nodes)
                for (var i = 0; i < item.Value.NumberOfNodalDof; i++)
                    item.Value.NodalDof[i] = item.Value.NodalVariables[i][index];
            // scaling of axial force distribution
            foreach (var beam in model.Elements.
                         Select(item => item.Value).OfType<AbstractBeam>())
            {
                _ = beam.ComputeElementState();
                presentation.AxialForceDraw(beam, maxAxialForce, false);
            }

            deformationsOn = false;
            axialForcesOn = true;
            shearForcesOn = false;
            bendingMomentsOn = false;
        }

        private void Clean()
        {
            //index = dropDownIndex;
            foreach (Shape path in presentation.Deformations) 
                VisualResults.Children.Remove(path);
            foreach (Shape path in presentation.AxialForceList) 
                VisualResults.Children.Remove(path);
            foreach (Shape path in presentation.ShearForceList) 
                VisualResults.Children.Remove(path);
            foreach (Shape path in presentation.BendingMomentList) 
                VisualResults.Children.Remove(path);
        }

        private void BtnShearForces_Click(object sender, RoutedEventArgs e)
        {
            if (index == 0)
            {
                _ = MessageBox.Show("Time Step must be selected first", "Structural Dynamic Analysis");
                return;
            }

            if (shearForcesOn)
            {
                foreach (Shape path in presentation.ShearForceList) VisualResults.Children.Remove(path);
                index++;
                CurrentTimeStep.Text =
                    "current time of Integration = " + (index * dt).ToString(CultureInfo.InvariantCulture);
                if (index >= nSteps)
                {
                    _ = MessageBox.Show("End of Time History Analysis", "Structural Dynamic Analysis");
                    index = dropDownIndex;
                    shearForcesOn = false;
                    return;
                }
            }
            else
            {
                index = dropDownIndex;
                Clean();
            }

            foreach (var item in model.Nodes)
                for (var i = 0; i < item.Value.NumberOfNodalDof; i++)
                    item.Value.NodalDof[i] = item.Value.NodalVariables[i][index];
            // draw scaled shear force distribution
            foreach (var beam in model.Elements.Select(item => item.Value).OfType<AbstractBeam>())
            {
                _ = beam.ComputeElementState();
                presentation.ShearForceDraw(beam, maxShearForce, false);
            }

            deformationsOn = false;
            axialForcesOn = false;
            shearForcesOn = true;
            bendingMomentsOn = false;
        }

        private void BtnBendingMoments_Click(object sender, RoutedEventArgs e)
        {
            if (index == 0)
            {
                _ = MessageBox.Show("Time Step of integration must be selected first", "Structural Dynamic Analysis");
                return;
            }

            if (bendingMomentsOn)
            {
                foreach (Shape path in presentation.BendingMomentList) 
                    VisualResults.Children.Remove(path);
                index++;
                CurrentTimeStep.Text =
                    "current time of integration = " + (index * dt).ToString(CultureInfo.InvariantCulture);
                if (index >= nSteps)
                {
                    _ = MessageBox.Show("End of time history analysis", "Structural Dynamic Analysis");
                    index = dropDownIndex;
                    bendingMomentsOn = false;
                    return;
                }
            }
            else
            {
                index = dropDownIndex;
                Clean();
            }

            foreach (var item in model.Nodes)
                for (var i = 0; i < item.Value.NumberOfNodalDof; i++)
                    item.Value.NodalDof[i] = item.Value.NodalVariables[i][index];
            // draw scaled bending moment distribution
            foreach (var beam in model.Elements.Select(item => item.Value).OfType<AbstractBeam>())
            {
                _ = beam.ComputeElementState();
                presentation.BendingMomentDraw(beam, maxBendingMoment, false);
            }

            deformationsOn = false;
            axialForcesOn = false;
            shearForcesOn = false;
            bendingMomentsOn = true;
        }

        private void MaximumValuesOfTimeHistory()
        {
            double maxUx = 0, minUx = 0, maxUy = 0, minUy = 0;
            string nodeUxMax = "", nodeUxMin = "", nodeUyMax = "", nodeUyMin = "";
            double maxUxTime = 0, minUxTime = 0, maxUyTime = 0, minUyTime = 0;
            var sb = new StringBuilder();
            foreach (var item in model.Nodes)
            {
                var temp = item.Value.NodalVariables[0].Max();
                if (maxUx < temp)
                {
                    maxUx = temp;
                    nodeUxMax = item.Value.Id;
                    maxUxTime = dt * Array.IndexOf(item.Value.NodalVariables[0], maxUx);
                }
                temp = item.Value.NodalVariables[0].Min();
                if (minUx > temp)
                {
                    minUx = temp;
                    nodeUxMin = item.Value.Id;
                    minUxTime = dt * Array.IndexOf(item.Value.NodalVariables[0], minUx);
                }

                temp = item.Value.NodalVariables[1].Max();
                if (maxUy < temp)
                {
                    maxUy = temp;
                    nodeUyMax = item.Value.Id;
                    maxUyTime = dt * Array.IndexOf(item.Value.NodalVariables[1], maxUy);
                }
                temp = item.Value.NodalVariables[1].Min();
                if (minUy > temp)
                {
                    minUy = temp;
                    nodeUyMin = item.Value.Id;
                    minUyTime = dt * Array.IndexOf(item.Value.NodalVariables[1], minUy);
                }
            }

            if (nodeUxMax.Length == 0)
            {
                sb.Append("ux = " + maxUx.ToString("G4"));
                sb.Append(", max. uy = " + maxUy.ToString("G4") + ", at node "
                          + nodeUyMax + " at time " + maxUyTime.ToString("G4")
                          + ", min. uy = " + minUy.ToString("G4") + ", at node "
                          + nodeUyMin + " at time " + minUyTime.ToString("G4"));
            }
            else if (nodeUyMax.Length == 0)
            {
                sb.Append("max. ux = " + maxUx.ToString("G4") + ", at node "
                          + nodeUxMax + " at time " + maxUxTime.ToString("G4")
                          + ", min. ux = " + minUx.ToString("G4") + ", at node "
                          + nodeUxMin + " at time " + minUxTime.ToString("G4"));
                sb.Append(", uy = " + maxUy.ToString("G4"));
            }
            else
            {
                sb.Append("max. ux = " + maxUx.ToString("G4") + ", at node "
                          + nodeUxMax + " at time " + maxUxTime.ToString("G4")
                          + ", min. ux = " + minUx.ToString("G4") + ", at node "
                          + nodeUxMin + " at time " + minUxTime.ToString("G4"));
                sb.Append(", max. uy = " + maxUy.ToString("G4") + ", at node "
                          + nodeUyMax + " at time " + maxUyTime.ToString("G4")
                          + ", min. uy = " + minUy.ToString("G4") + ", at node "
                          + nodeUyMin + " at time " + minUyTime.ToString("G4"));
            }
            var maxDeformations = new TextBlock
            {
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Text = sb.ToString(),
                Foreground = Red
            };
            SetTop(maxDeformations, 0);
            SetLeft(maxDeformations, 5);
            VisualResults.Children.Add(maxDeformations);

            // loop over all time steps
            for (var i = 0; i < nSteps; i++)
            {
                foreach (var item in model.Nodes)
                    for (var k = 0; k < item.Value.NumberOfNodalDof; k++)
                        item.Value.NodalDof[k] = item.Value.NodalVariables[k][i];

                IEnumerable<AbstractBeam> Beams()
                {
                    foreach (var item in model.Elements)
                        if (item.Value is AbstractBeam element)
                            yield return element;
                }

                // state of all truss and beam elements at a specified time stept
                foreach (var element in Beams())
                {
                    element.ElementState = element.ComputeElementState();

                    // trusses
                    if (element.ElementState.Length == 2)
                    {
                        if (Math.Abs(element.ElementState[0]) > maxAxialForce)
                        {
                            indexN = i;
                            maxAxialForce = Math.Abs(element.ElementState[0]);
                        }

                        if (Math.Abs(element.ElementState[1]) > maxAxialForce)
                        {
                            indexN = i;
                            maxAxialForce = Math.Abs(element.ElementState[1]);
                        }
                    }

                    // bending beams
                    else
                    {
                        if (Math.Abs(element.ElementState[0]) > maxAxialForce) { indexN = i; maxAxialForce = Math.Abs(element.ElementState[0]); }
                        if (Math.Abs(element.ElementState[3]) > maxAxialForce) { indexN = i; maxAxialForce = Math.Abs(element.ElementState[3]); }
                        if (Math.Abs(element.ElementState[1]) > maxShearForce) { indexQ = i; maxShearForce = Math.Abs(element.ElementState[1]); }
                        if (Math.Abs(element.ElementState[4]) > maxShearForce) { indexQ = i; maxShearForce = Math.Abs(element.ElementState[4]); }
                        if (Math.Abs(element.ElementState[2]) > maxBendingMoment) { indexM = i; maxBendingMoment = Math.Abs(element.ElementState[2]); }
                        if (Math.Abs(element.ElementState[5]) > maxBendingMoment) { indexM = i; maxBendingMoment = Math.Abs(element.ElementState[5]); }
                    }
                }
            }

            maximumValues = new TextBlock
            {
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Text = "max Axial Force = " + maxAxialForce.ToString("G4") + " at time = " +
                       (indexN * dt).ToString("N2") +
                       ", max Shear Force = " + maxShearForce.ToString("G4") + " at time = " +
                       (indexQ * dt).ToString("N2") +
                       " and max Bending Moment = " + maxBendingMoment.ToString("G4") + " at time = " +
                       (indexM * dt).ToString("N2"),
                Foreground = Red
            };
            SetTop(maximumValues, 20);
            SetLeft(maximumValues, 5);
            VisualResults.Children.Add(maximumValues);
        }

        private void BtnElementIds_Click(object sender, RoutedEventArgs e)
        {
            if (!elementTextsOn)
            {
                presentation.ElementTexts();
                elementTextsOn = true;
            }
            else
            {
                foreach (TextBlock id in presentation.ElementIDs) VisualResults.Children.Remove(id);
                elementTextsOn = false;
            }
        }

        private void BtnNodeIds_Click(object sender, RoutedEventArgs e)
        {
            if (!nodeTextsOn)
            {
                presentation.NodeTexts();
                nodeTextsOn = true;
            }
            else
            {
                foreach (TextBlock id in presentation.NodeIDs) VisualResults.Children.Remove(id);
                nodeTextsOn = false;
            }
        }

        private void BtnScalingDeformations_Click(object sender, RoutedEventArgs e)
        {
            presentation.scalingDisplacement = int.Parse(Deformation.Text);
            foreach (Shape path in presentation.Deformations) VisualResults.Children.Remove(path);
            deformationsOn = false;
            presentation.DeformedGeometry();
            deformationsOn = true;
        }
    }
}