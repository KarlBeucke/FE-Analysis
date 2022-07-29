using FE_Analysis.Structural_Analysis.Model_Data;
using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using static System.Globalization.CultureInfo;

namespace FE_Analysis.Structural_Analysis.Results
{
    public partial class StaticResultsVisualize
    {
        public readonly Presentation presentation;
        private readonly List<Shape> hitList = new List<Shape>();
        private readonly List<TextBlock> hitTextBlock = new List<TextBlock>();
        private readonly FeModel model;

        private bool elementTextsOn = true,
            nodesTextsOn = true,
            deformationsOn,
            axialForcesOn,
            shearForcesOn,
            bendingMomentsOn;

        private EllipseGeometry hitArea;

        public StaticResultsVisualize(FeModel feModel)
        {
            Language = XmlLanguage.GetLanguage("us-US");
            model = feModel;
            InitializeComponent();
            Show();

            presentation = new Presentation(model, VisualResults);

            presentation.UndeformedGeometry();

            // with Element Ids
            presentation.ElementTexts();

            // with Node Ids
            presentation.NodeTexts();

            // factor for scaling deformations
            presentation.scalingDisplacement = int.Parse(Displacement.Text);
            presentation.scalingRotation = int.Parse(Rotation.Text);
        }

        private void BtnDeformation_Click(object sender, RoutedEventArgs e)
        {
            if (!deformationsOn)
            {
                presentation.DeformedGeometry();
                deformationsOn = true;
            }
            else
            {
                foreach (Shape path in presentation.Deformations) VisualResults.Children.Remove(path);
                deformationsOn = false;
            }
        }

        private void BtnAxialForces_Click(object sender, RoutedEventArgs e)
        {
            double maxAxialForce = 0;
            if (shearForcesOn)
            {
                foreach (Shape path in presentation.ShearForceList) VisualResults.Children.Remove(path);
                shearForcesOn = false;
            }

            if (bendingMomentsOn)
            {
                foreach (Shape path in presentation.BendingMomentList) VisualResults.Children.Remove(path);
                VisualResults.Children.Remove(presentation.maxMomentText);
                bendingMomentsOn = false;
            }

            if (!axialForcesOn)
            {
                // Evaluation of maximum axial force
                IEnumerable<AbstractBeam> Beams()
                {
                    foreach (var item in model.Elements)
                        if (item.Value is AbstractBeam beam)
                            yield return beam;
                }

                foreach (var beam in Beams())
                {
                    var barEndForces = beam.ComputeElementState();
                    if (Math.Abs(barEndForces[0]) > maxAxialForce) maxAxialForce = Math.Abs(barEndForces[0]);
                    if (barEndForces.Length > 2)
                    {
                        if (Math.Abs(barEndForces[3]) > maxAxialForce) maxAxialForce = Math.Abs(barEndForces[3]);
                    }
                    else
                    {
                        if (Math.Abs(barEndForces[1]) > maxAxialForce) maxAxialForce = Math.Abs(barEndForces[1]);
                    }
                }

                // scaling and drawing of axial forces distribution
                foreach (var beam in Beams())
                {
                    _ = beam.ComputeElementState();
                    presentation.AxialForceDraw(beam, maxAxialForce, false);
                }

                axialForcesOn = true;
            }
            else
            {
                foreach (Shape path in presentation.AxialForceList) VisualResults.Children.Remove(path);
                axialForcesOn = false;
            }
        }

        private void BtnShearForce_Click(object sender, RoutedEventArgs e)
        {
            double maxShearForce = 0;
            if (axialForcesOn)
            {
                foreach (Shape path in presentation.AxialForceList) VisualResults.Children.Remove(path);
                axialForcesOn = false;
            }

            if (bendingMomentsOn)
            {
                foreach (Shape path in presentation.BendingMomentList) VisualResults.Children.Remove(path);
                VisualResults.Children.Remove(presentation.maxMomentText);
                bendingMomentsOn = false;
            }

            if (!shearForcesOn)
            {
                // evaluation of maximum shear force
                IEnumerable<AbstractBeam> Beams()
                {
                    foreach (var item in model.Elements)
                        if (item.Value is AbstractBeam beam)
                            yield return beam;
                }

                foreach (var beam in Beams())
                {
                    beam.ElementState = beam.ComputeElementState();
                    if (beam.ElementState.Length <= 2) continue;
                    if (Math.Abs(beam.ElementState[1]) > maxShearForce) maxShearForce = Math.Abs(beam.ElementState[1]);
                    if (Math.Abs(beam.ElementState[4]) > maxShearForce) maxShearForce = Math.Abs(beam.ElementState[4]);
                }

                // scaling and drawing distribution of shear forces
                foreach (var beam in Beams())
                {
                    var elementlast = false;
                    if (beam.ElementState.Length <= 2) continue;
                    if (Math.Abs(beam.ElementState[1] - beam.ElementState[4]) > double.Epsilon) elementlast = true;
                    presentation.ShearForceDraw(beam, maxShearForce, elementlast);
                }

                shearForcesOn = true;
            }
            else
            {
                foreach (Shape path in presentation.ShearForceList) VisualResults.Children.Remove(path);
                shearForcesOn = false;
            }
        }

        private void BtnBendingMoments_Click(object sender, RoutedEventArgs e)
        {
            double maxMoment = 0;
            if (axialForcesOn)
            {
                foreach (Shape path in presentation.AxialForceList) VisualResults.Children.Remove(path);
                axialForcesOn = false;
            }

            if (shearForcesOn)
            {
                foreach (Shape path in presentation.ShearForceList) VisualResults.Children.Remove(path);
                shearForcesOn = false;
            }

            if (!bendingMomentsOn)
            {
                // evaluation of maximum bending moment
                IEnumerable<AbstractBeam> Beams()
                {
                    foreach (var item in model.Elements)
                        if (item.Value is AbstractBeam beam)
                            yield return beam;
                }

                foreach (var beam in Beams())
                {
                    beam.ElementState = beam.ComputeElementState();
                    if (beam.ElementState.Length <= 2) continue;
                    if (Math.Abs(beam.ElementState[2]) > maxMoment) maxMoment = Math.Abs(beam.ElementState[2]);
                    if (Math.Abs(beam.ElementState[5]) > maxMoment) maxMoment = Math.Abs(beam.ElementState[5]);
                }

                // if nodal bending moments = 0, evaluate local element bending moments for scaling
                if (maxMoment < 1E-5)
                {
                    AbstractElement element = null;
                    AbstractBeam loadBeam;
                    double localMoment;

                    IEnumerable<PointLoad> PointLoads()
                    {
                        foreach (var load in model.ElementLoads.Select(item =>
                                     (PointLoad)item.Value).Where(load =>
                                     model.Elements.TryGetValue(load.ElementId, out element)))
                            yield return load;
                    }

                    foreach (var load in PointLoads())
                    {
                        loadBeam = (AbstractBeam)element;
                        localMoment = loadBeam.ElementState[1] * load.Offset * loadBeam.length;
                        if (Math.Abs(localMoment) > maxMoment) maxMoment = Math.Abs(localMoment);
                    }

                    IEnumerable<LineLoad> LineLoads()
                    {
                        foreach (var last in model.ElementLoads.Select(item =>
                                     (LineLoad)item.Value).Where(last =>
                                     model.Elements.TryGetValue(last.ElementId, out element)))
                            yield return last;
                    }

                    foreach (var last in LineLoads())
                    {
                        loadBeam = (AbstractBeam)element;
                        var stabEndkräfte = loadBeam.ElementState;
                        // only continuous load with max.ordinate considered for scaling
                        double max = Math.Abs(last.Intensity[1]);
                        if (Math.Abs(last.Intensity[3]) > max) max = last.Intensity[3];
                        localMoment = stabEndkräfte[1] * loadBeam.length / 2 -
                                        max * loadBeam.length / 2 * loadBeam.length / 4;
                        if (Math.Abs(localMoment) > maxMoment) maxMoment = Math.Abs(localMoment);
                    }
                }

                // scaling and drawing of moment distribution for all beams
                foreach (var beam in Beams())
                {
                    var elementload = false;
                    if (beam.ElementState.Length <= 2) continue;
                    if (Math.Abs(beam.ElementState[1] - beam.ElementState[4]) > double.Epsilon) elementload = true;
                    presentation.BendingMomentDraw(beam, maxMoment, elementload);
                }

                bendingMomentsOn = true;
            }
            else
            {
                foreach (Shape path in presentation.BendingMomentList) VisualResults.Children.Remove(path);
                foreach (TextBlock maxWerte in presentation.MaxTexts) VisualResults.Children.Remove(maxWerte);
                bendingMomentsOn = false;
            }
        }

        private void BtnElementIDs_Click(object sender, RoutedEventArgs e)
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

        private void BtnNodeIDs_Click(object sender, RoutedEventArgs e)
        {
            if (!nodesTextsOn)
            {
                presentation.NodeTexts();
                nodesTextsOn = true;
            }
            else
            {
                foreach (TextBlock id in presentation.NodeIDs) VisualResults.Children.Remove(id);
                nodesTextsOn = false;
            }
        }

        private void BtnDisplacement_Click(object sender, RoutedEventArgs e)
        {
            presentation.scalingDisplacement = int.Parse(Displacement.Text);
            foreach (Shape path in presentation.Deformations) VisualResults.Children.Remove(path);
            deformationsOn = false;
            presentation.DeformedGeometry();
            deformationsOn = true;
        }

        private void BtnRotation_Click(object sender, RoutedEventArgs e)
        {
            presentation.scalingRotation = int.Parse(Rotation.Text);
            foreach (Shape path in presentation.Deformations) VisualResults.Children.Remove(path);
            deformationsOn = false;
            presentation.DeformedGeometry();
            deformationsOn = true;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            hitList.Clear();
            hitTextBlock.Clear();
            var hitPoint = e.GetPosition(VisualResults);
            hitArea = new EllipseGeometry(hitPoint, 0.1, 0.1);
            VisualTreeHelper.HitTest(VisualResults, null, HitTestCallBack,
                new GeometryHitTestParameters(hitArea));

            MyPopup.IsOpen = false;

            var sb = new StringBuilder();
            foreach (var item in hitList)
            {
                sb.Clear();
                MyPopup.IsOpen = true;

                if (!model.Elements.TryGetValue(item.Name, out var lineElement)) continue;
                sb.Clear();
                if (lineElement is SpringElement) continue;
                var balken = (AbstractBeam)lineElement;
                var balkenEndKräfte = balken.ComputeElementState();

                switch (balkenEndKräfte.Length)
                {
                    case 2:
                        sb.Append("Element = " + balken.ElementId);
                        sb.Append("\nNa\t= " + balkenEndKräfte[0].ToString("F2", InvariantCulture));
                        sb.Append("\nNb\t= " + balkenEndKräfte[1].ToString("F2", InvariantCulture));
                        break;
                    case 6:
                        sb.Append("Element = " + lineElement.ElementId);
                        sb.Append("\nNa\t= " + balkenEndKräfte[0].ToString("F2", InvariantCulture));
                        sb.Append("\nQa\t= " + balkenEndKräfte[1].ToString("F2", InvariantCulture));
                        sb.Append("\nMa\t= " + balkenEndKräfte[2].ToString("F2", InvariantCulture));
                        sb.Append("\nNb\t= " + balkenEndKräfte[3].ToString("F2", InvariantCulture));
                        sb.Append("\nQb\t= " + balkenEndKräfte[4].ToString("F2", InvariantCulture));
                        sb.Append("\nMb\t= " + balkenEndKräfte[5].ToString("F2", InvariantCulture));
                        break;
                }

                sb.Append("\n");
                MyPopupText.Text = sb.ToString();
            }

            foreach (var item in hitTextBlock)
            {
                if (item == null | item.Text == string.Empty) { continue; }

                sb.Clear();
                MyPopup.IsOpen = true;
                if (model.Nodes.TryGetValue(item.Text, out var node))
                {
                    sb.Append("Node  = " + node.Id);
                    sb.Append("\nux\t= " + node.NodalDof[0].ToString("F4", InvariantCulture));
                    sb.Append("\nuy\t= " + node.NodalDof[1].ToString("F4", InvariantCulture));
                    if (node.NodalDof.Length == 3)
                        sb.Append("\nphi\t= " + node.NodalDof[2].ToString("F4", InvariantCulture));
                    if (node.Reactions != null)
                        for (var i = 0; i < node.Reactions.Length; i++)
                            sb.Append("\nSupport Reaction " + i + "\t=" + node.Reactions[i].ToString("F2", InvariantCulture));
                    MyPopupText.Text = sb.ToString();
                    break;
                }

                if (!model.Elements.TryGetValue(item.Text, out var lineElement)) continue;
                sb.Clear();
                if (lineElement is SpringElement)
                {
                    lineElement.ComputeStateVector();
                    sb.Append("Spring = " + lineElement.ElementId);
                    sb.Append("\nFx\t= " + lineElement.ElementState[0].ToString("F2", InvariantCulture));
                    sb.Append("\nFy\t= " + lineElement.ElementState[1].ToString("F2", InvariantCulture));
                    sb.Append("\nM\t= " + lineElement.ElementState[2].ToString("F2", InvariantCulture));
                }
                else
                {
                    var beam = (AbstractBeam)lineElement;
                    var beamEndForces = beam.ComputeElementState();

                    switch (beamEndForces.Length)
                    {
                        case 2:
                            sb.Append("Element = " + beam.ElementId);
                            sb.Append("\nNa\t= " + beamEndForces[0].ToString("F2", InvariantCulture));
                            sb.Append("\nNb\t= " + beamEndForces[1].ToString("F2", InvariantCulture));
                            break;
                        case 6:
                            sb.Append("Element = " + lineElement.ElementId);
                            sb.Append("\nNa\t= " + beamEndForces[0].ToString("F2", InvariantCulture));
                            sb.Append("\nQa\t= " + beamEndForces[1].ToString("F2", InvariantCulture));
                            sb.Append("\nMa\t= " + beamEndForces[2].ToString("F2", InvariantCulture));
                            sb.Append("\nNb\t= " + beamEndForces[3].ToString("F2", InvariantCulture));
                            sb.Append("\nQb\t= " + beamEndForces[4].ToString("F2", InvariantCulture));
                            sb.Append("\nMb\t= " + beamEndForces[5].ToString("F2", InvariantCulture));
                            break;
                    }
                }

                MyPopupText.Text = sb.ToString();
            }
        }
        private HitTestResultBehavior HitTestCallBack(HitTestResult result)
        {
            var intersectionDetail = ((GeometryHitTestResult)result).IntersectionDetail;

            switch (intersectionDetail)
            {
                case IntersectionDetail.Empty:
                    return HitTestResultBehavior.Continue;
                case IntersectionDetail.FullyContains:
                    switch (result.VisualHit)
                    {
                        case Shape hit:
                            hitList.Add(hit);
                            break;
                        case TextBlock hit:
                            hitTextBlock.Add(hit);
                            break;
                    }

                    return HitTestResultBehavior.Continue;
                case IntersectionDetail.FullyInside:
                    return HitTestResultBehavior.Continue;
                case IntersectionDetail.Intersects:
                    switch (result.VisualHit)
                    {
                        case Shape hit:
                            hitList.Add(hit);
                            break;
                    }

                    return HitTestResultBehavior.Continue;
                case IntersectionDetail.NotCalculated:
                    return HitTestResultBehavior.Continue;
                default:
                    return HitTestResultBehavior.Stop;
            }
        }
        private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            MyPopup.IsOpen = false;
        }
    }
}