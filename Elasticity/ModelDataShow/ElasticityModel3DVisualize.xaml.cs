using FEALibrary.Model;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace FE_Analysis.Elasticity.ModelDataShow
{
    public partial class ElasticityModel3DVisualize
    {
        // change of tilt angle, if up/down key is pressed
        // private const double CameraDPhi = 0.1;
        // change of rotation angle, if left/right key is pressed
        // private const double CameraDTheta = 0.1;
        // change of camera distance, if PgUp/PgDn is pressed
        private const double CameraDr = 10;

        // horizontal shift left/right
        private const double CameraDx = 10;

        // vertical shift uo/down
        private const double CameraDy = 5;

        // 3D model group
        private readonly Model3DGroup model3DGroup = new();

        private readonly Presentation3D presentation3D;

        // start position of camera
        private double cameraPhi = 0.13; // 7.45 degrees
        private double cameraR = 60.0;
        private double cameraTheta = 1.65; // 94.5 degrees
        private double cameraX;
        private double cameraY;
        private ModelVisual3D modelVisual;
        private PerspectiveCamera theCamera;

        public ElasticityModel3DVisualize(FeModel feModel)
        {
            Language = XmlLanguage.GetLanguage("us-US");
            presentation3D = new Presentation3D(feModel);
            InitializeComponent();
        }

        // create 3D-scene
        // Viewport is defined as Viewport3D in XAML-Code, that displays everything 
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // definition of start position for camera
            theCamera = new PerspectiveCamera { FieldOfView = 60 };
            Viewport.Camera = theCamera;
            PositionCamera();

            // deinition of lights
            DefinitionOfLights();

            // coordinate system
            presentation3D.CoordinateSystem(model3DGroup);

            // creating model
            presentation3D.UndeformedGeometry(model3DGroup, true);

            presentation3D.BoundaryConditions(model3DGroup);

            presentation3D.NodeLoads(model3DGroup);

            // adding model group (mainModel3DGroup) to a new ModelVisual3D
            modelVisual = new ModelVisual3D { Content = model3DGroup };

            // presentation of "modelVisual" in Viewport
            Viewport.Children.Add(modelVisual);
        }

        private void PositionCamera()
        {
            // z-viewing direction, y-up, x-move, _cameraR=distance
            // evaluate camera position in cartesian coordinates
            // y=distance*sin(tilt angle) (up, down)
            // hypotenuse = Abstand*cos(Kippwinkel)
            // x= hypotenuse * cos(rotation angle) (left, right)
            // z= hypotenuse * sin(rotation angle)
            var y = cameraR * Math.Sin(cameraPhi);
            var hyp = cameraR * Math.Cos(cameraPhi);
            var x = hyp * Math.Cos(cameraTheta);
            var z = hyp * Math.Sin(cameraTheta);
            theCamera.Position = new Point3D(x + cameraX, y + cameraY, z);
            double offset = 0;

            // view in direction of coordinate origin (0; 0; 0), centered
            // if origing is upper left, move presentation by offset
            if (presentation3D.minX >= 0) offset = 10;
            theCamera.LookDirection = new Vector3D(-(x - offset), -(y + offset), -z);

            // setting Up direction
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

        // changing camera position using keys up/down, left/right, PgUp/PgDn
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up: // Vertikalverschiebung
                    //cameraPhi -= CameraDPhi;
                    //if (cameraPhi > Math.PI / 2.0) cameraPhi = Math.PI / 2.0;
                    //ScrPhi.Value = cameraPhi;
                    cameraY -= CameraDy;
                    break;
                case Key.Down:
                    //cameraPhi += CameraDPhi;
                    //if (cameraPhi < -Math.PI / 2.0) cameraPhi = -Math.PI / 2.0;
                    //ScrPhi.Value = cameraPhi;
                    cameraY += CameraDy;
                    break;

                case Key.Left: // Horizontalverschiebung
                    //cameraTheta -= CameraDTheta;
                    //ScrTheta.Value = cameraTheta;
                    cameraX += CameraDx;
                    break;
                case Key.Right:
                    //cameraTheta += CameraDTheta;
                    //ScrTheta.Value = cameraTheta;
                    cameraX -= CameraDx;
                    break;

                case Key.Add: //  + Ziffernblock
                case Key.OemPlus: //  + alfanumerisch
                    cameraR -= CameraDr;
                    if (cameraR < CameraDr) cameraR = CameraDr;
                    break;
                case Key.PageUp:
                    cameraR -= CameraDr;
                    if (cameraR < CameraDr) cameraR = CameraDr;
                    break;

                case Key.Subtract: //  - Ziffernblock
                case Key.OemMinus: //  - alfanumerisch
                    cameraR += CameraDr;
                    break;
                case Key.PageDown:
                    cameraR += CameraDr;
                    if (cameraR < CameraDr) cameraR = CameraDr;
                    break;
            }

            // Neufestlegung der Kameraposition
            PositionCamera();
        }

        // changing camera position using scroll bars
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

        // On and Off of individual model presentations (GeometryModel3Ds)
        private void ShowCoordinates(object sender, RoutedEventArgs e)
        {
            foreach (var coordinates in presentation3D.coordinates) model3DGroup.Children.Add(coordinates);
        }

        private void RemoveCoordinates(object sender, RoutedEventArgs e)
        {
            foreach (var coordinates in presentation3D.coordinates) model3DGroup.Children.Remove(coordinates);
        }

        private void ShowSurfaces(object sender, RoutedEventArgs e)
        {
            foreach (var surfaces in presentation3D.surfaces) model3DGroup.Children.Add(surfaces);
        }

        private void RemoveSurfaces(object sender, RoutedEventArgs e)
        {
            foreach (var surfaces in presentation3D.surfaces) model3DGroup.Children.Remove(surfaces);
        }

        private void ShowWireFrameModel(object sender, RoutedEventArgs e)
        {
            foreach (var wires in presentation3D.edges) model3DGroup.Children.Add(wires);
        }

        private void RemoveWireFrameModel(object sender, RoutedEventArgs e)
        {
            foreach (var wires in presentation3D.edges) model3DGroup.Children.Remove(wires);
        }

        private void ShowFixedBoundaryConditions(object sender, RoutedEventArgs e)
        {
            foreach (var boundaryConditionsFixed in presentation3D.boundaryConditionsFixed)
                model3DGroup.Children.Add(boundaryConditionsFixed);
        }

        private void RemoveFixedBoundaryConditions(object sender, RoutedEventArgs e)
        {
            foreach (var boundaryConditionsFixed in presentation3D.boundaryConditionsFixed)
                model3DGroup.Children.Remove(boundaryConditionsFixed);
        }

        private void ShowPreBoundaryConditions(object sender, RoutedEventArgs e)
        {
            foreach (var boundaryConditionsPre in presentation3D.boundaryConditionsPre)
                model3DGroup.Children.Add(boundaryConditionsPre);
        }

        private void RemovePreBoundaryConditions(object sender, RoutedEventArgs e)
        {
            foreach (var boundaryConditionsPre in presentation3D.boundaryConditionsPre)
                model3DGroup.Children.Remove(boundaryConditionsPre);
        }

        private void ShowNodeLoads(object sender, RoutedEventArgs e)
        {
            foreach (var nodeLoads in presentation3D.nodeLoads) model3DGroup.Children.Add(nodeLoads);
        }

        private void RemoveNodeLoads(object sender, RoutedEventArgs e)
        {
            foreach (var nodeLoads in presentation3D.nodeLoads) model3DGroup.Children.Remove(nodeLoads);
        }
    }
}