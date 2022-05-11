using FEALibrary.Model;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using static System.Globalization.CultureInfo;

namespace FE_Analysis.Elasticity.Results
{
    public partial class StaticResults3DVisualize
    {
        // modification of distance, if +/- key is pressed
        private const double CameraDr = 1;

        // horizontal shift le/ri
        private const double CameraDx = 1;

        // vertical shift up/down
        private const double CameraDy = 1;
        private readonly Presentation3D presentation3D;

        private readonly Model3DGroup model3DGroup = new Model3DGroup();

        // initial position of camera
        private double cameraPhi = 0.13; // 7,45 degrees
        private double cameraR = 60.0;
        private double cameraTheta = 1.65; // 94,5 degrees
        private double cameraX;
        private double cameraY;
        private string maxKey_xx, minKey_xx, maxKey_yy, minKey_yy, maxKey_zz, minKey_zz;
        private string maxKey_xy, minKey_xy, maxKey_yz, minKey_yz, maxKey_zx, minKey_zx;
        private double maxSigma_xx, minSigma_xx, maxSigma_yy, minSigma_yy, maxSigma_zz, minSigma_zz;
        private double maxSigma_xy, minSigma_xy, maxSigma_yz, minSigma_yz, maxSigma_zx, minSigma_zx;
        private ModelVisual3D modelVisual;
        private Dictionary<string, ElementStresses> sigma;

        private PerspectiveCamera theCamera;

        public StaticResults3DVisualize(FeModel feModel)
        {
            Language = XmlLanguage.GetLanguage("us-US");
            presentation3D = new Presentation3D(feModel);
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // definition of initial camera position
            theCamera = new PerspectiveCamera { FieldOfView = 60 };
            Viewport.Camera = theCamera;
            PositionCamera();

            // definition of lights
            DefinitionOfLights();

            // coordinate system
            presentation3D.CoordinateSystem(model3DGroup);

            // undeformed geometry as wire frame model and optionally as surface model
            presentation3D.UndeformedGeometry(model3DGroup, false);

            // element stresses
            sigma = new Dictionary<string, ElementStresses>();
            foreach (var item in presentation3D.model.Elements)
                sigma.Add(item.Key, new ElementStresses(item.Value.ComputeStateVector()));
            // selecting stress to be displayed
            var direction = new List<string>
                { "sigma_xx", "sigma_yy", "sigma_xy", "sigma_zz", "sigma_yz", "sigma_zx", "none" };
            //StressSelection.SelectedValue = "sigma_xx";
            StressSelection.ItemsSource = direction;

            // adding mainModel3DGroup to a new ModelVisual3D
            modelVisual = new ModelVisual3D { Content = model3DGroup };

            // displaying "modelVisual" in Viewport
            Viewport.Children.Add(modelVisual);
        }

        private void DropDownStressSelectionClosed(object sender, EventArgs e)
        {
            var stresses = (string)StressSelection.SelectedItem;
            switch (stresses)
            {
                case "sigma_xx":
                    ShowStresses_xx();
                    break;
                case "sigma_yy":
                    ShowStresses_yy();
                    break;
                case "sigma_xy":
                    ShowStresses_xy();
                    break;
                case "sigma_zz":
                    ShowStresses_zz();
                    break;
                case "sigma_yz":
                    ShowStresses_yz();
                    break;
                case "sigma_zx":
                    ShowStresses_zx();
                    break;
                case "keine":
                    RemoveStresses();
                    break;
            }
        }

        private void PositionCamera()
        {
            // z-view direction, y-up, x-sidewards, _cameraR=distance
            // evaluate camera position in cartesian coordinates
            // y=distance*sin(tilt angle) (up, down)
            // hypotenuse = distance*cos(tilt angle)
            // x= hypotenuse * cos(rotation angle) (left, right)
            // z= hypotenuse * sin(rotation angle)
            var y = cameraR * Math.Sin(cameraPhi);
            var hyp = cameraR * Math.Cos(cameraPhi);
            var x = hyp * Math.Cos(cameraTheta);
            var z = hyp * Math.Sin(cameraTheta);
            theCamera.Position = new Point3D(x + cameraX, y + cameraY, z);
            double offset = 0;

            // viewing in direction of coordinate origin (0; 0; 0), centered
            // if coordinate origin upper left, shift display by offset
            if (presentation3D.minX >= 0) offset = 10;
            theCamera.LookDirection = new Vector3D(-(x - offset), -(y + offset), -z);

            // set Up direction
            theCamera.UpDirection = new Vector3D(0, 1, 0);

            //_ = MessageBox.Show("Camera.Position: (" + x + ", " + y + ", " + z + ")", "3D Wireframe");
        }

        private void DefinitionOfLights()
        {
            var ambientLight = new AmbientLight(Colors.Gray);
            var directionalLight =
                new DirectionalLight(Colors.Gray, new Vector3D(-1.0, -3.0, -2.0));
            model3DGroup.Children.Add(ambientLight);
            model3DGroup.Children.Add(directionalLight);
        }

        private void ShowCoordinates(object sender, RoutedEventArgs e)
        {
            foreach (var coordinates in presentation3D.coordinates) model3DGroup.Children.Add(coordinates);
        }

        private void RemoveCoordinates(object sender, RoutedEventArgs e)
        {
            foreach (var coordinates in presentation3D.coordinates) model3DGroup.Children.Remove(coordinates);
        }

        private void ShowWireFrameModel(object sender, RoutedEventArgs e)
        {
            foreach (var edges in presentation3D.edges) model3DGroup.Children.Add(edges);
        }

        private void RemoveWireFrameModel(object sender, RoutedEventArgs e)
        {
            foreach (var edges in presentation3D.edges) model3DGroup.Children.Remove(edges);
        }

        private void ShowDeformations(object sender, RoutedEventArgs e)
        {
            if (presentation3D.deformations.Count == 0)
                // deformed geometry as wire frame model
                presentation3D.DeformedGeometry(model3DGroup);
            else
                foreach (var deformations in presentation3D.deformations)
                    model3DGroup.Children.Add(deformations);
        }

        private void RemoveDeformations(object sender, RoutedEventArgs e)
        {
            foreach (var deformations in presentation3D.deformations) model3DGroup.Children.Remove(deformations);
        }

        private void ShowStresses_xx()
        {
            //var maxSigma_xx = sigma.Max(elementStress => elementStresses.Value.Stresses[0]);
            //var minSigma_xx = sigma.Min(elementStress => elementStresses.Value.Stresses[0]);
            RemoveStresses();
            foreach (var item in sigma)
            {
                if (item.Value.Stresses[0] > maxSigma_xx)
                {
                    maxSigma_xx = item.Value.Stresses[0];
                    maxKey_xx = item.Key;
                }

                if (!(item.Value.Stresses[0] < minSigma_xx)) continue;
                minSigma_xx = item.Value.Stresses[0];
                minKey_xx = item.Key;
            }

            MaxMin.Text = "maxSigma_xx = " + maxSigma_xx.ToString("0.###E+00", InvariantCulture) + " in Element " + maxKey_xx
                          + ",  minSigma_xx = " + minSigma_xx.ToString("0.###E+00", InvariantCulture) + " in Element " + minKey_xx;

            if (presentation3D.stressesXx.Count == 0)
            {
                var maxValue = maxSigma_xx;
                if (Math.Abs(minSigma_xx) > maxValue) maxValue = Math.Abs(minSigma_xx);
                presentation3D.ElementStresses_xx(model3DGroup, maxValue);
            }
            else
            {
                foreach (var geometryModel3D in presentation3D.stressesXx) model3DGroup.Children.Add(geometryModel3D);
            }

            // adding mainModel3DGroup to a new ModelVisual3D
            modelVisual = new ModelVisual3D { Content = model3DGroup };

            // displaying "modelVisual" in Viewport
            Viewport.Children.Add(modelVisual);
        }

        private void ShowStresses_yy()
        {
            RemoveStresses();
            foreach (var item in sigma)
            {
                if (item.Value.Stresses[1] > maxSigma_yy)
                {
                    maxSigma_yy = item.Value.Stresses[1];
                    maxKey_yy = item.Key;
                }

                if (!(item.Value.Stresses[1] < minSigma_yy)) continue;
                minSigma_yy = item.Value.Stresses[1];
                minKey_yy = item.Key;
            }

            MaxMin.Text = "maxSigma_yy = " + maxSigma_yy.ToString("0.###E+00", InvariantCulture) + " in Element " + maxKey_yy
                          + ",  minSigma_yy = " + minSigma_yy.ToString("0.###E+00", InvariantCulture) + " in Element " + minKey_yy;

            if (presentation3D.stressesYy.Count == 0)
            {
                var maxValue = maxSigma_yy;
                if (Math.Abs(minSigma_yy) > maxValue) maxValue = Math.Abs(minSigma_yy);
                presentation3D.ElementStresses_yy(model3DGroup, maxValue);
            }
            else
            {
                foreach (var geometryModel3D in presentation3D.stressesYy) model3DGroup.Children.Add(geometryModel3D);
            }

            // adding mainModel3DGroup to a new ModelVisual3D
            modelVisual = new ModelVisual3D { Content = model3DGroup };

            // displaying "modelVisual" in Viewport
            Viewport.Children.Add(modelVisual);
        }

        private void ShowStresses_xy()
        {
            RemoveStresses();
            foreach (var item in sigma)
            {
                if (item.Value.Stresses[2] > maxSigma_xy)
                {
                    maxSigma_xy = item.Value.Stresses[2];
                    maxKey_xy = item.Key;
                }

                if (!(item.Value.Stresses[2] < minSigma_xy)) continue;
                minSigma_xy = item.Value.Stresses[2];
                minKey_xy = item.Key;
            }

            MaxMin.Text = "maxSigma_xy = " + maxSigma_xy.ToString("0.###E+00", InvariantCulture) + " in Element " + maxKey_xy
                          + ",  minSigma_xy = " + minSigma_xy.ToString("0.###E+00", InvariantCulture) + " in Element " + minKey_xy;

            if (presentation3D.stressesXy.Count == 0)
            {
                var maxValue = maxSigma_xy;
                if (Math.Abs(minSigma_xy) > maxValue) maxValue = Math.Abs(minSigma_xy);
                presentation3D.ElementStresses_xy(model3DGroup, maxValue);
            }
            else
            {
                foreach (var geometryModel3D in presentation3D.stressesXy) model3DGroup.Children.Add(geometryModel3D);
            }

            // adding mainModel3DGroup to a new ModelVisual3D
            modelVisual = new ModelVisual3D { Content = model3DGroup };

            // displaying "modelVisual" in Viewport
            Viewport.Children.Add(modelVisual);
        }

        private void ShowStresses_zz()
        {
            RemoveStresses();
            foreach (var item in sigma)
            {
                if (item.Value.Stresses[3] > maxSigma_zz)
                {
                    maxSigma_zz = item.Value.Stresses[3];
                    maxKey_zz = item.Key;
                }

                if (!(item.Value.Stresses[3] < minSigma_zz)) continue;
                minSigma_zz = item.Value.Stresses[3];
                minKey_zz = item.Key;
            }

            MaxMin.Text = "maxSigma_zz = " + maxSigma_zz.ToString("0.###E+00", InvariantCulture) + " in Element " + maxKey_zz
                          + ",  minSigma_zz = " + minSigma_zz.ToString("0.###E+00", InvariantCulture) + " in Element " + minKey_zz;

            if (presentation3D.stressesZz.Count == 0)
            {
                var maxValue = maxSigma_zz;
                if (Math.Abs(minSigma_zz) > maxValue) maxValue = Math.Abs(minSigma_zz);
                presentation3D.ElementStresses_zz(model3DGroup, maxValue);
            }
            else
            {
                foreach (var geometryModel3D in presentation3D.stressesZz) model3DGroup.Children.Add(geometryModel3D);
            }

            // adding mainModel3DGroup to a new ModelVisual3D
            modelVisual = new ModelVisual3D { Content = model3DGroup };

            // displaying "modelVisual" in Viewport
            Viewport.Children.Add(modelVisual);
        }

        private void ShowStresses_yz()
        {
            RemoveStresses();
            foreach (var item in sigma)
            {
                if (item.Value.Stresses[4] > maxSigma_yz)
                {
                    maxSigma_yz = item.Value.Stresses[4];
                    maxKey_yz = item.Key;
                }

                if (!(item.Value.Stresses[4] < minSigma_yz)) continue;
                minSigma_yz = item.Value.Stresses[4];
                minKey_yz = item.Key;
            }

            MaxMin.Text = "maxSigma_yz = " + maxSigma_yz.ToString("0.###E+00", InvariantCulture) + " in Element " + maxKey_yz
                          + ",  minSigma_yz = " + minSigma_yz.ToString("0.###E+00", InvariantCulture) + " in Element " + minKey_yz;

            if (presentation3D.stressesYz.Count == 0)
            {
                var maxValue = maxSigma_yz;
                if (Math.Abs(minSigma_yz) > maxValue) maxValue = Math.Abs(minSigma_yz);
                presentation3D.ElementSpannungen_yz(model3DGroup, maxValue);
            }
            else
            {
                foreach (var geometryModel3D in presentation3D.stressesYz) model3DGroup.Children.Add(geometryModel3D);
            }

            // adding mainModel3DGroup to a new ModelVisual3D
            modelVisual = new ModelVisual3D { Content = model3DGroup };

            // displaying "modelVisual" in Viewport
            Viewport.Children.Add(modelVisual);
        }

        private void ShowStresses_zx()
        {
            RemoveStresses();
            foreach (var item in sigma)
            {
                if (item.Value.Stresses[5] > maxSigma_zx)
                {
                    maxSigma_zx = item.Value.Stresses[5];
                    maxKey_zx = item.Key;
                }

                if (!(item.Value.Stresses[5] < minSigma_zx)) continue;
                minSigma_zx = item.Value.Stresses[5];
                minKey_zx = item.Key;
            }

            MaxMin.Text = "maxSigma_zx = " + maxSigma_zx.ToString("0.###E+00", InvariantCulture) + " in Element " + maxKey_zx
                          + ",  minSigma_zx = " + minSigma_zx.ToString("0.###E+00", InvariantCulture) + " in Element " + minKey_zx;

            if (presentation3D.stressesZx.Count == 0)
            {
                var maxValue = maxSigma_zx;
                if (Math.Abs(minSigma_yz) > maxValue) maxValue = Math.Abs(minSigma_zx);
                presentation3D.ElementStresses_zx(model3DGroup, maxValue);
            }
            else
            {
                foreach (var geometryModel3D in presentation3D.stressesZx) model3DGroup.Children.Add(geometryModel3D);
            }

            // adding mainModel3DGroup to a new ModelVisual3D
            modelVisual = new ModelVisual3D { Content = model3DGroup };

            // displaying "modelVisual" in Viewport
            Viewport.Children.Add(modelVisual);
        }

        private void RemoveStresses()
        {
            foreach (var sigmaModell in presentation3D.stressesXx) model3DGroup.Children.Remove(sigmaModell);
            foreach (var sigmaModell in presentation3D.stressesYy) model3DGroup.Children.Remove(sigmaModell);
            foreach (var sigmaModell in presentation3D.stressesXy) model3DGroup.Children.Remove(sigmaModell);
            foreach (var sigmaModell in presentation3D.stressesZz) model3DGroup.Children.Remove(sigmaModell);
            foreach (var sigmaModell in presentation3D.stressesYz) model3DGroup.Children.Remove(sigmaModell);
            foreach (var sigmaModell in presentation3D.stressesZx) model3DGroup.Children.Remove(sigmaModell);
        }

        // changing camera position using Scrollbars
        private void ScrThetaScroll(object sender, ScrollEventArgs e)
        {
            cameraTheta = ScrTheta.Value;
            PositionCamera();
        }

        private void ScrPhiScroll(object sender, ScrollEventArgs e)
        {
            cameraPhi = ScrPhi.Value;
            PositionCamera();
        }

        // changing camera position using keys up/down, left/right, PgUp/PgDn
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up: // vertical position
                    cameraY -= CameraDy;

                    break;
                case Key.Down:
                    cameraY += CameraDy;
                    break;

                case Key.Left: // horizontal position
                    cameraX += CameraDx;
                    break;
                case Key.Right:
                    cameraX -= CameraDx;
                    break;

                case Key.Add: //  + numeric keypad
                case Key.OemPlus: //  + alfanumeric
                    cameraR -= CameraDr;
                    if (cameraR < CameraDr) cameraR = CameraDr;
                    break;
                case Key.PageUp:
                    cameraR -= CameraDr;
                    if (cameraR < CameraDr) cameraR = CameraDr;
                    break;

                case Key.Subtract: //  - numeric keypad
                case Key.PageDown:
                    cameraR += CameraDr;
                    if (cameraR < CameraDr) cameraR = CameraDr;
                    break;
            }

            // set new camera position
            PositionCamera();
        }

        // scaling factor for representation of deformations
        private void BtnScaling_Click(object sender, RoutedEventArgs e)
        {
            presentation3D.scalingDeformation = double.Parse(Scaling.Text);
            foreach (var deformations in presentation3D.deformations) model3DGroup.Children.Remove(deformations);
            presentation3D.DeformedGeometry(model3DGroup);
        }

        private class ElementStresses
        {
            public ElementStresses(double[] stresses)
            {
                Stresses = stresses;
            }

            public double[] Stresses { get; }
        }
    }
}