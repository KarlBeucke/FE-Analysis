using FE_Analysis.Elasticity.ModelDataRead;
using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace FE_Analysis.Elasticity
{
    public class Presentation3D
    {
        public readonly List<GeometryModel3D> edges = new List<GeometryModel3D>();
        public readonly List<GeometryModel3D> nodeLoads = new List<GeometryModel3D>();

        public readonly List<GeometryModel3D> coordinates = new List<GeometryModel3D>();
        private readonly double maxX, minY, maxY, minZ, maxZ;
        public readonly double minX;
        public readonly FeModel model;
        public readonly List<GeometryModel3D> surfaces = new List<GeometryModel3D>();

        // create Dictionary for finding triangular points efficiently
        private readonly Dictionary<Point3D, int> pointDictionary = new Dictionary<Point3D, int>();
        public readonly List<GeometryModel3D> boundaryConditionsFixed = new List<GeometryModel3D>();
        public readonly List<GeometryModel3D> boundaryConditionsPre = new List<GeometryModel3D>();
        public readonly List<GeometryModel3D> stressesXx = new List<GeometryModel3D>();
        public readonly List<GeometryModel3D> stressesXy = new List<GeometryModel3D>();
        public readonly List<GeometryModel3D> stressesYy = new List<GeometryModel3D>();
        public readonly List<GeometryModel3D> stressesYz = new List<GeometryModel3D>();
        public readonly List<GeometryModel3D> stressesZx = new List<GeometryModel3D>();
        public readonly List<GeometryModel3D> stressesZz = new List<GeometryModel3D>();
        public readonly List<GeometryModel3D> deformations = new List<GeometryModel3D>();
        private GeometryModel3D wireFrameModel;
        private GeometryModel3D nodeLoadModel;
        private GeometryModel3D surfaceModel;
        private GeometryModel3D boundaryConditionBoussinesqModel;
        private GeometryModel3D boundaryConditionModel;
        private GeometryModel3D stressesModel;
        public double scalingDeformation = 1;
        private GeometryModel3D deformationsModel;

        public Presentation3D(FeModel feModel)
        {
            model = feModel;

            var x = new List<double>();
            var y = new List<double>();
            var z = new List<double>();
            foreach (var item in model.Nodes)
            {
                x.Add(item.Value.Coordinates[0]);
                maxX = x.Max();
                minX = x.Min();
                y.Add(item.Value.Coordinates[2]);
                maxY = y.Max();
                minY = y.Min();
                z.Add(item.Value.Coordinates[1]);
                maxZ = z.Max();
                minZ = z.Min();
            }
        }

        public void CoordinateSystem(Model3DGroup modelGroup)
        {
            // Point3D as origin of Rect3D, generally the far bottom left corner
            // the positive Y-Axis in 3D-coordinates points upwards,
            // presumably the camera UpDirection is positive

            var meshX = new MeshGeometry3D();
            var coordinateModel = XAxis(meshX);
            modelGroup.Children.Add(coordinateModel);
            coordinates.Add(coordinateModel);

            var meshY = new MeshGeometry3D();
            coordinateModel = YAxis(meshY);
            modelGroup.Children.Add(coordinateModel);
            coordinates.Add(coordinateModel);

            var meshZ = new MeshGeometry3D();
            coordinateModel = ZAxis(meshZ);
            modelGroup.Children.Add(coordinateModel);
            coordinates.Add(coordinateModel);
        }

        private GeometryModel3D XAxis(MeshGeometry3D mesh)
        {
            const double weight = 0.1;
            var vectorLength = 1.0;
            var AxisOvershoot = 0.8;
            // x-Axis
            var start = new Point3D(-AxisOvershoot, 0, 0);
            var end = new Point3D(vectorLength, 0, 0);
            var points = CuboidPointsX(start, end, weight);
            GenerateCuboid(mesh, points);

            var arrowLength = 0.4;
            var width = 2 * weight;
            var arrows = ArrowPointsX(end, arrowLength, width);
            GenerateArrowHead(mesh, arrows);

            var material = new DiffuseMaterial(Brushes.Red);
            var xAxis = new GeometryModel3D(mesh, material) { BackMaterial = material };
            return xAxis;
        }

        private GeometryModel3D YAxis(MeshGeometry3D mesh)
        {
            const double weight = 0.1;
            var vectorLength = 1.0;
            var AxisOvershoot = 0.8;
            // y-Axis
            var start = new Point3D(0, -AxisOvershoot, 0);
            var end = new Point3D(0, vectorLength, 0);
            var points = CuboidPointsY(start, end, weight);
            GenerateCuboid(mesh, points);

            var arrowLength = 0.4;
            var width = 2 * weight;
            var arrows = ArrowPointsY(end, arrowLength, width);
            GenerateArrowHead(mesh, arrows);

            var material = new DiffuseMaterial(Brushes.Green);
            var yAxis = new GeometryModel3D(mesh, material) { BackMaterial = material };
            return yAxis;
        }

        private GeometryModel3D ZAxis(MeshGeometry3D mesh)
        {
            var weight = 0.1;
            var vectorLength = 1.0;
            var axisOvershoot = 0.8;
            // z-Axis
            var start = new Point3D(0, 0, -axisOvershoot);
            var end = new Point3D(0, 0, vectorLength);
            var points = CuboidPointsZ(start, end, weight);
            GenerateCuboid(mesh, points);

            var arrowLength = 0.4;
            var width = 2 * weight;
            var arrows = ArrowPointsZ(end, arrowLength, width);
            GenerateArrowHead(mesh, arrows);

            var material = new DiffuseMaterial(Brushes.Blue);
            var zAxis = new GeometryModel3D(mesh, material) { BackMaterial = material };
            return zAxis;
        }

        public void UndeformedGeometry(Model3DGroup modelGroup, bool volume)
        {
            var mesh = new MeshGeometry3D();
            foreach (var item in model.Elements)
            {
                var points = new Point3DCollection();
                pointDictionary.Clear();
                points.Clear();
                foreach (var nodeId in item.Value.NodeIds)
                    if (model.Nodes.TryGetValue(nodeId, out var node))
                        points.Add(new Point3D(node.Coordinates[0], -node.Coordinates[2], node.Coordinates[1]));

                GenerateCuboid(mesh, points);

                if (volume)
                {
                    // creating surface model
                    // respresentation of mterial for surface model in LightGreen
                    var surfaceMaterial = new DiffuseMaterial(Brushes.LightGreen);
                    surfaceModel = new GeometryModel3D(mesh, surfaceMaterial) { BackMaterial = surfaceMaterial };
                    // visibility of surface from both sides
                    // adding model to ModelGroup
                    modelGroup.Children.Add(surfaceModel);

                    surfaces.Add(surfaceModel);
                }

                // creating wire frame model, thickness (weight of edges) = 0.02
                const double edgeWeight = 0.02;
                var wireframe = mesh.ToWireframe(edgeWeight);
                var wireframeMaterial = new DiffuseMaterial(Brushes.Black);
                wireFrameModel = new GeometryModel3D(wireframe, wireframeMaterial);
                modelGroup.Children.Add(wireFrameModel);

                edges.Add(wireFrameModel);
            }
        }

        public void DeformedGeometry(Model3DGroup modelGroup)
        {
            var mesh = new MeshGeometry3D();
            foreach (var item in model.Elements)
            {
                var points = new Point3DCollection();
                pointDictionary.Clear();
                points.Clear();
                foreach (var nodeId in item.Value.NodeIds)
                    if (model.Nodes.TryGetValue(nodeId, out var node))
                        points.Add(new Point3D(node.Coordinates[0] + node.NodalDof[0] * scalingDeformation,
                            -node.Coordinates[2] - node.NodalDof[2] * scalingDeformation,
                            node.Coordinates[1] + node.NodalDof[1] * scalingDeformation));

                GenerateCuboid(mesh, points);

                // creating wire frame model, thickness (weight of edges) = 0.02
                const double edgeWeight = 0.02;
                var deformation = mesh.ToWireframe(edgeWeight);
                var deformationMaterial = new DiffuseMaterial(Brushes.Red);
                deformationsModel = new GeometryModel3D(deformation, deformationMaterial);
                modelGroup.Children.Add(deformationsModel);

                deformations.Add(deformationsModel);
            }
        }

        public void BoundaryConditions(Model3DGroup modelGroup)
        {
            const double d = 0.1;
            var boundaryConditionsFixedMaterial = new DiffuseMaterial(Brushes.Red);
            var boundaryConditionsPreMaterial = new DiffuseMaterial(Brushes.LightPink);

            foreach (var item in ElasticityParser.parseElasticityBoundaryConditions.faces)
            {
                var points = new Point3DCollection();
                pointDictionary.Clear();
                points.Clear();
                var mesh = new MeshGeometry3D();

                switch (item)
                {
                    case "X0": //left
                        points.Add(new Point3D(minX, -minY, minZ)); //0
                        points.Add(new Point3D(minX, -maxY, minZ)); //1
                        points.Add(new Point3D(minX, -maxY, maxZ)); //2
                        points.Add(new Point3D(minX, -minY, maxZ)); //3
                        points.Add(new Point3D(minX - d, -minY, minZ)); //4
                        points.Add(new Point3D(minX - d, -maxY, minZ)); //5
                        points.Add(new Point3D(minX - d, -maxY, maxZ)); //6
                        points.Add(new Point3D(minX - d, -minY, maxZ)); //7

                        GenerateCuboid(mesh, points);

                        boundaryConditionModel = new GeometryModel3D(mesh, boundaryConditionsFixedMaterial)
                        { BackMaterial = boundaryConditionsFixedMaterial };
                        modelGroup.Children.Add(boundaryConditionModel);

                        boundaryConditionsFixed.Add(boundaryConditionModel);
                        break;

                    case "Y0": // far end
                        points.Add(new Point3D(minX, -minY, minZ)); //0
                        points.Add(new Point3D(minX, -maxY, minZ)); //1
                        points.Add(new Point3D(maxX, -maxY, minZ)); //2
                        points.Add(new Point3D(maxX, -minY, minZ)); //3
                        points.Add(new Point3D(minX, -minY, minZ - d)); //4
                        points.Add(new Point3D(minX, -maxY, minZ - d)); //5
                        points.Add(new Point3D(maxX, -maxY, minZ - d)); //6
                        points.Add(new Point3D(maxX, -minY, minZ - d)); //7

                        GenerateCuboid(mesh, points);

                        boundaryConditionModel = new GeometryModel3D(mesh, boundaryConditionsFixedMaterial)
                        { BackMaterial = boundaryConditionsFixedMaterial };
                        modelGroup.Children.Add(boundaryConditionModel);

                        boundaryConditionsFixed.Add(boundaryConditionModel);
                        break;
                }
            }

            foreach (var item2 in ElasticityParser.parseElasticityBoundaryConditions.faces)
            {
                var points = new Point3DCollection();
                pointDictionary.Clear();
                points.Clear();
                var mesh = new MeshGeometry3D();

                switch (item2)
                {
                    case "XMax": // right
                        points.Add(new Point3D(maxX, -minY, minZ)); //0
                        points.Add(new Point3D(maxX, -maxY, minZ)); //1
                        points.Add(new Point3D(maxX, -maxY, maxZ)); //2
                        points.Add(new Point3D(maxX, -minY, maxZ)); //3
                        points.Add(new Point3D(maxX + d, -minY, minZ)); //4
                        points.Add(new Point3D(maxX + d, -maxY, minZ)); //5
                        points.Add(new Point3D(maxX + d, -maxY, maxZ)); //6
                        points.Add(new Point3D(maxX + d, -minY, maxZ)); //7

                        GenerateCuboid(mesh, points);

                        boundaryConditionBoussinesqModel = new GeometryModel3D(mesh, boundaryConditionsPreMaterial)
                        { BackMaterial = boundaryConditionsPreMaterial };
                        modelGroup.Children.Add(boundaryConditionBoussinesqModel);
                        boundaryConditionsPre.Add(boundaryConditionBoussinesqModel);
                        break;

                    case "YMax": // bottom
                        points.Add(new Point3D(minX, -maxY, minZ)); //0
                        points.Add(new Point3D(minX, -maxY, maxZ)); //1
                        points.Add(new Point3D(maxX, -maxY, maxZ)); //2
                        points.Add(new Point3D(maxX, -maxY, minZ)); //3
                        points.Add(new Point3D(minX, -maxY - d, minZ)); //4
                        points.Add(new Point3D(minX, -maxY - d, maxZ)); //5
                        points.Add(new Point3D(maxX, -maxY - d, maxZ)); //6
                        points.Add(new Point3D(maxX, -maxY - d, minZ)); //7

                        GenerateCuboid(mesh, points);

                        boundaryConditionBoussinesqModel = new GeometryModel3D(mesh, boundaryConditionsPreMaterial)
                        { BackMaterial = boundaryConditionsPreMaterial };
                        modelGroup.Children.Add(boundaryConditionBoussinesqModel);
                        boundaryConditionsPre.Add(boundaryConditionBoussinesqModel);
                        break;

                    case "ZMax": // front
                        points.Add(new Point3D(minX, -minY, maxZ)); //0
                        points.Add(new Point3D(minX, -maxY, maxZ)); //1
                        points.Add(new Point3D(maxX, -maxY, maxZ)); //2
                        points.Add(new Point3D(maxX, -minY, maxZ)); //3  
                        points.Add(new Point3D(minX, -minY, maxZ + d)); //4
                        points.Add(new Point3D(minX, -maxY, maxZ + d)); //5
                        points.Add(new Point3D(maxX, -maxY, maxZ + d)); //6
                        points.Add(new Point3D(maxX, -minY, maxZ + d)); //7

                        GenerateCuboid(mesh, points);

                        boundaryConditionBoussinesqModel = new GeometryModel3D(mesh, boundaryConditionsPreMaterial)
                        { BackMaterial = boundaryConditionsPreMaterial };
                        modelGroup.Children.Add(boundaryConditionBoussinesqModel);
                        boundaryConditionsPre.Add(boundaryConditionBoussinesqModel);
                        break;
                }
            }
        }

        public void NodeLoads(Model3DGroup modelGroup)
        {
            var mesh = new MeshGeometry3D();
            double loadValue = 0;
            var loadPoint = new Point3D();

            foreach (var load in model.Loads)
            {
                var loadDirection = new Vector3D(0, 0, 0);
                const double loadScaling = 10.0;
                var nodeId = load.Value.NodeId;
                if (model.Nodes.TryGetValue(nodeId, out var node))
                {
                    loadPoint.X = node.Coordinates[0];
                    loadPoint.Y = -node.Coordinates[2];
                    loadPoint.Z = node.Coordinates[1];
                }

                if (Math.Abs(load.Value.Intensity[0]) > 0)
                {
                    loadDirection.X = 1;
                    loadValue = loadScaling * load.Value.Intensity[0];
                }
                else if (Math.Abs(load.Value.Intensity[2]) > 0)
                {
                    loadDirection.Y = -1;
                    loadValue = loadScaling * load.Value.Intensity[2];
                }
                else if (Math.Abs(load.Value.Intensity[1]) > 0)
                {
                    loadDirection.Z = 1;
                    loadValue = loadScaling * load.Value.Intensity[1];
                }

                nodeLoadModel = LoadVector(mesh, loadPoint, loadDirection, loadValue);
                modelGroup.Children.Add(nodeLoadModel);
                nodeLoads.Add(nodeLoadModel);
            }
        }

        private GeometryModel3D LoadVector(MeshGeometry3D mesh, Point3D loadPoint,
            Vector3D loadDirection, double loadValue)
        {
            var weight = 0.1;
            var arrowLength = 0.4;
            var start = loadPoint;
            var end = new Point3D(0, 0, 0);
            var loadModel = new GeometryModel3D();

            // horizontal load in x
            if (Math.Abs(loadDirection.X) > 0)
            {
                start.X -= arrowLength;
                end.X = start.X - loadDirection.X * loadValue;
                var points = CuboidPointsX(start, end, weight);
                GenerateCuboid(mesh, points);

                var large = 2 * weight;
                // arrow head
                var next = (Vector3D)loadPoint - loadDirection * arrowLength;
                var cross = Vector3D.CrossProduct(new Vector3D(0, 0, 1), next);
                cross.Normalize();

                var loadArrow = new Point3DCollection
                {
                    loadPoint, // spitze
                    (Point3D)(next + cross * large + new Vector3D(0.0, 0.0, large)), // front-left
                    (Point3D)(next + cross * large + new Vector3D(0.0, 0.0, -large)), // far end-left
                    (Point3D)(next - cross * large + new Vector3D(0.0, 0.0, -large)), // far end-right
                    (Point3D)(next - cross * large + new Vector3D(0.0, 0.0, large)) // front-right
                };
                GenerateArrowHead(mesh, loadArrow);
                var material = new DiffuseMaterial(Brushes.Red);
                loadModel = new GeometryModel3D(mesh, material) { BackMaterial = material };
            }

            // vertikal load in y
            if (Math.Abs(loadDirection.Y) > 0)
            {
                start.Y += arrowLength;
                end.Y = -loadDirection.Y * loadValue;
                var punkte = CuboidPointsY(start, end, weight);
                GenerateCuboid(mesh, punkte);

                var large = 2 * weight;
                // arrow head
                var next = (Vector3D)loadPoint - loadDirection * arrowLength;
                var cross = Vector3D.CrossProduct(new Vector3D(0, 0, 1), next);
                cross.Normalize();

                var loadArrow = new Point3DCollection
                {
                    loadPoint, // head
                    (Point3D)(next + cross * large + new Vector3D(0.0, 0.0, large)), // front-left
                    (Point3D)(next + cross * large + new Vector3D(0.0, 0.0, -large)), // far end-left
                    (Point3D)(next - cross * large + new Vector3D(0.0, 0.0, -large)), // far end-right
                    (Point3D)(next - cross * large + new Vector3D(0.0, 0.0, large)) // front right
                };
                GenerateArrowHead(mesh, loadArrow);
                var material = new DiffuseMaterial(Brushes.Red);
                loadModel = new GeometryModel3D(mesh, material) { BackMaterial = material };
            }

            // horizontal load in z (depth direction)
            if (Math.Abs(loadDirection.Z) > 0)
            {
                start.Z -= arrowLength;
                end.Z = start.Z - loadDirection.Z * loadValue;
                var points = CuboidPointsZ(start, end, weight);
                GenerateCuboid(mesh, points);

                var large = 2 * weight;
                // arrow head
                var next = (Vector3D)loadPoint - loadDirection * arrowLength;
                var cross = Vector3D.CrossProduct(new Vector3D(0, -1, 0), next);
                cross.Normalize();

                var loadArrow = new Point3DCollection
                {
                    loadPoint, // head
                    (Point3D)(next - cross * large + new Vector3D(0.0, -large, 0)), // front-left
                    (Point3D)(next - cross * large + new Vector3D(0.0, large, 0)), // far end-left
                    (Point3D)(next + cross * large + new Vector3D(0.0, large, 0)), // far end-right
                    (Point3D)(next + cross * large + new Vector3D(0.0, -large, 0)) // front-right
                };
                GenerateArrowHead(mesh, loadArrow);
                var material = new DiffuseMaterial(Brushes.Red);
                loadModel = new GeometryModel3D(mesh, material) { BackMaterial = material };
            }

            return loadModel;
        }

        public void ElementStresses_xx(Model3DGroup modelGroup, double maxStress)
        {
            const double weight = 0.04;
            const double vectorLength = 1.0;
            var mesh = new MeshGeometry3D();
            var scaling = vectorLength / maxStress;
            var normalXDirection = new Vector3D(1, 0, 0);

            foreach (var item in model.Elements)
            {
                var element = (Abstract3D)item.Value;
                var elementStresses = new ElementSresses(item.Value.ComputeStateVector());
                var normalXValue = elementStresses.Stresses[0] * scaling;
                var c = element.ComputeCenterOfGravity3D();
                c.Y = -c.Y;

                var start = (Point3D)((Vector3D)c + normalXDirection * normalXValue / 2);
                var end = (Point3D)((Vector3D)c - normalXDirection * normalXValue / 2);

                // normal stresses sigma-xx
                var points = CuboidPointsX(start, end, weight);
                GenerateCuboid(mesh, points);

                var material = normalXValue < 0 ? new DiffuseMaterial(Brushes.Red) : new DiffuseMaterial(Brushes.Blue);
                stressesModel = new GeometryModel3D(mesh, material) { BackMaterial = material };
                modelGroup.Children.Add(stressesModel);
                stressesXx.Add(stressesModel);
            }
        }

        public void ElementStresses_yy(Model3DGroup modelGroup, double maxStress)
        {
            const double weight = 0.04;
            const double vectorLength = 1.0;
            var mesh = new MeshGeometry3D();
            var scaling = vectorLength / maxStress;
            var normalYDirection = new Vector3D(0, 1, 0);

            foreach (var item in model.Elements)
            {
                var element = (Abstract3D)item.Value;
                var elementStresses = new ElementSresses(item.Value.ComputeStateVector());
                var normalYValue = elementStresses.Stresses[1] * scaling;
                var cg = element.ComputeCenterOfGravity3D();
                cg.Y = -cg.Y;

                var start = (Point3D)((Vector3D)cg + normalYDirection * normalYValue / 2);
                var end = (Point3D)((Vector3D)cg - normalYDirection * normalYValue / 2);

                // normal stresses sigma-yy
                var points = CuboidPointsY(start, end, weight);
                GenerateCuboid(mesh, points);

                var material = normalYValue < 0 ? new DiffuseMaterial(Brushes.Red) : new DiffuseMaterial(Brushes.Blue);
                stressesModel = new GeometryModel3D(mesh, material) { BackMaterial = material };
                modelGroup.Children.Add(stressesModel);
                stressesYy.Add(stressesModel);
            }
        }

        public void ElementStresses_xy(Model3DGroup modelGroup, double maxStress)
        {
            const double weight = 0.04;
            const double vectorLength = 1.0;
            var mesh = new MeshGeometry3D();
            var scaling = vectorLength / maxStress;
            var shearXDirection = new Vector3D(1, 0, 0);

            foreach (var item in model.Elements)
            {
                var element = (Abstract3D)item.Value;
                var elementStresses = new ElementSresses(item.Value.ComputeStateVector());
                var shearXValue = elementStresses.Stresses[2] * scaling;
                var cg = element.ComputeCenterOfGravity3D();
                cg.Y = -cg.Y;

                var start = (Point3D)((Vector3D)cg + shearXDirection * shearXValue / 2);
                var end = (Point3D)((Vector3D)cg - shearXDirection * shearXValue / 2);

                // shear stresses tau-xy
                var points = CuboidPointsZ(start, end, weight);
                GenerateCuboid(mesh, points);

                var material = shearXValue < 0 ? new DiffuseMaterial(Brushes.Red) : new DiffuseMaterial(Brushes.Blue);
                stressesModel = new GeometryModel3D(mesh, material) { BackMaterial = material };
                modelGroup.Children.Add(stressesModel);
                stressesXy.Add(stressesModel);
            }
        }

        public void ElementStresses_zz(Model3DGroup modelGroup, double maxStress)
        {
            const double weight = 0.04;
            const double vectorLength = 1.0;
            var mesh = new MeshGeometry3D();
            var scaling = vectorLength / maxStress;
            var normalZDirection = new Vector3D(0, 0, 1);

            foreach (var item in model.Elements)
            {
                var element = (Abstract3D)item.Value;
                var elementStresses = new ElementSresses(item.Value.ComputeStateVector());
                var normalZValue = elementStresses.Stresses[3] * scaling;
                var cg = element.ComputeCenterOfGravity3D();
                cg.Y = -cg.Y;

                var start = (Point3D)((Vector3D)cg + normalZDirection * normalZValue / 2);
                var end = (Point3D)((Vector3D)cg - normalZDirection * normalZValue / 2);

                // normal stresss sigma-yy
                var points = CuboidPointsZ(start, end, weight);
                GenerateCuboid(mesh, points);

                var material = normalZValue < 0 ? new DiffuseMaterial(Brushes.Red) : new DiffuseMaterial(Brushes.Blue);
                stressesModel = new GeometryModel3D(mesh, material) { BackMaterial = material };
                modelGroup.Children.Add(stressesModel);
                stressesZz.Add(stressesModel);
            }
        }

        public void ElementSpannungen_yz(Model3DGroup modelGroup, double maxStress)
        {
            const double weight = 0.04;
            const double vectorLength = 1.0;
            var mesh = new MeshGeometry3D();
            var scaling = vectorLength / maxStress;
            var shearYDirection = new Vector3D(0, 1, 0);

            foreach (var item in model.Elements)
            {
                var element = (Abstract3D)item.Value;
                var elementStresses = new ElementSresses(item.Value.ComputeStateVector());
                var shearYValue = elementStresses.Stresses[4] * scaling;
                var cg = element.ComputeCenterOfGravity3D();
                cg.Y = -cg.Y;

                var start = (Point3D)((Vector3D)cg + shearYDirection * shearYValue / 2);
                var end = (Point3D)((Vector3D)cg - shearYDirection * shearYValue / 2);

                // Schubspannungen tau-yz
                var punkte = CuboidPointsZ(start, end, weight);
                GenerateCuboid(mesh, punkte);

                var material = shearYValue < 0 ? new DiffuseMaterial(Brushes.Red) : new DiffuseMaterial(Brushes.Blue);
                stressesModel = new GeometryModel3D(mesh, material) { BackMaterial = material };
                modelGroup.Children.Add(stressesModel);
                stressesYz.Add(stressesModel);
            }
        }

        public void ElementStresses_zx(Model3DGroup modelGroup, double maxStress)
        {
            const double weight = 0.04;
            const double vectorLength = 1.0;
            var mesh = new MeshGeometry3D();
            var scaling = vectorLength / maxStress;
            var shearZDirection = new Vector3D(0, 0, 1);

            foreach (var item in model.Elements)
            {
                var element = (Abstract3D)item.Value;
                var elementStresses = new ElementSresses(item.Value.ComputeStateVector());
                var shearZValue = elementStresses.Stresses[5] * scaling;
                var cg = element.ComputeCenterOfGravity3D();
                cg.Y = -cg.Y;

                var start = (Point3D)((Vector3D)cg + shearZDirection * shearZValue / 2);
                var end = (Point3D)((Vector3D)cg - shearZDirection * shearZValue / 2);

                // shear stresses tau-zx
                var points = CuboidPointsZ(start, end, weight);
                GenerateCuboid(mesh, points);

                var material = shearZValue < 0 ? new DiffuseMaterial(Brushes.Red) : new DiffuseMaterial(Brushes.Blue);
                stressesModel = new GeometryModel3D(mesh, material) { BackMaterial = material };
                modelGroup.Children.Add(stressesModel);
                stressesZx.Add(stressesModel);
            }
        }

        private static Point3DCollection CuboidPointsX(Point3D start, Point3D end, double weight)
        {
            var points = new Point3DCollection
            {
                new Point3D(start.X, start.Y - weight, start.Z + weight),
                new Point3D(start.X, start.Y + weight, start.Z + weight),
                new Point3D(start.X, start.Y + weight, start.Z - weight),
                new Point3D(start.X, start.Y - weight, start.Z - weight),
                new Point3D(end.X, end.Y - weight, end.Z + weight),
                new Point3D(end.X, end.Y + weight, end.Z + weight),
                new Point3D(end.X, end.Y + weight, end.Z - weight),
                new Point3D(end.X, end.Y - weight, end.Z - weight)
            };
            return points;
        }

        private static Point3DCollection CuboidPointsY(Point3D start, Point3D end, double weight)
        {
            var points = new Point3DCollection
            {
                new Point3D(start.X - weight, start.Y, start.Z + weight),
                new Point3D(start.X - weight, start.Y, start.Z - weight),
                new Point3D(start.X + weight, start.Y, start.Z - weight),
                new Point3D(start.X + weight, start.Y, start.Z + weight),
                new Point3D(end.X - weight, end.Y, end.Z + weight),
                new Point3D(end.X - weight, end.Y, end.Z - weight),
                new Point3D(end.X + weight, end.Y, end.Z - weight),
                new Point3D(end.X + weight, end.Y, end.Z + weight)
            };
            return points;
        }

        private static Point3DCollection CuboidPointsZ(Point3D start, Point3D end, double weight)
        {
            var points = new Point3DCollection
            {
                new Point3D(start.X - weight, start.Y + weight, start.Z),
                new Point3D(start.X - weight, start.Y - weight, start.Z),
                new Point3D(start.X + weight, start.Y - weight, start.Z),
                new Point3D(start.X + weight, start.Y + weight, start.Z),
                new Point3D(end.X - weight, end.Y + weight, end.Z),
                new Point3D(end.X - weight, end.Y - weight, end.Z),
                new Point3D(end.X + weight, end.Y - weight, end.Z),
                new Point3D(end.X + weight, end.Y + weight, end.Z)
            };
            return points;
        }

        private void GenerateCuboid(MeshGeometry3D mesh, Point3DCollection points)
        {
            //top
            AddTriangle(mesh, points[0], points[1], points[2]);
            AddTriangle(mesh, points[0], points[2], points[3]);

            //bottom
            AddTriangle(mesh, points[4], points[6], points[5]);
            AddTriangle(mesh, points[4], points[7], points[6]);

            //front
            AddTriangle(mesh, points[0], points[1], points[5]);
            AddTriangle(mesh, points[0], points[5], points[4]);

            //far end
            AddTriangle(mesh, points[3], points[6], points[2]);
            AddTriangle(mesh, points[3], points[7], points[6]);


            //left
            AddTriangle(mesh, points[0], points[7], points[3]);
            AddTriangle(mesh, points[0], points[4], points[7]);

            //right
            AddTriangle(mesh, points[1], points[2], points[6]);
            AddTriangle(mesh, points[1], points[6], points[5]);
        }

        private static Point3DCollection ArrowPointsX(Point3D end, double length, double width)
        {
            var arrows = new Point3DCollection
            {
                new Point3D(end.X + length, end.Y, end.Z),
                new Point3D(end.X, end.Y - width, end.Z + width),
                new Point3D(end.X, end.Y - width, end.Z - width),
                new Point3D(end.X, end.Y + width, end.Z - width),
                new Point3D(end.X, end.Y + width, end.Z + width)
            };
            return arrows;
        }

        private static Point3DCollection ArrowPointsY(Point3D end, double length, double width)
        {
            var arrows = new Point3DCollection
            {
                new Point3D(end.X, end.Y + length, end.Z),
                new Point3D(end.X - width, end.Y, end.Z + width),
                new Point3D(end.X - width, end.Y, end.Z - width),
                new Point3D(end.X + width, end.Y, end.Z - width),
                new Point3D(end.X + width, end.Y, end.Z + width)
            };
            return arrows;
        }

        private static Point3DCollection ArrowPointsZ(Point3D end, double length, double width)
        {
            var arrows = new Point3DCollection
            {
                new Point3D(end.X, end.Y, end.Z + length),
                new Point3D(end.X - width, end.Y - width, end.Z),
                new Point3D(end.X - width, end.Y - width, end.Z),
                new Point3D(end.X + width, end.Y + width, end.Z),
                new Point3D(end.X + width, end.Y + width, end.Z)
            };
            return arrows;
        }

        private void GenerateArrowHead(MeshGeometry3D mesh, Point3DCollection arrows)
        {
            AddTriangle(mesh, arrows[0], arrows[1], arrows[4]); // far end
            AddTriangle(mesh, arrows[0], arrows[1], arrows[2]); // bottom
            AddTriangle(mesh, arrows[0], arrows[2], arrows[3]); // front
            AddTriangle(mesh, arrows[0], arrows[3], arrows[4]); // top

            AddTriangle(mesh, arrows[1], arrows[2], arrows[3]); // cover
            AddTriangle(mesh, arrows[1], arrows[3], arrows[4]); //
        }

        // add triangle to mesh, reusing triangle points that have been defined before
        private void AddTriangle(MeshGeometry3D mesh, Point3D point1, Point3D point2, Point3D point3)
        {
            // read indices of triangle points in mesh
            var index1 = AddPunkt(mesh.Positions, point1);
            var index2 = AddPunkt(mesh.Positions, point2);
            var index3 = AddPunkt(mesh.Positions, point3);

            // create triangles
            mesh.TriangleIndices.Add(index1);
            mesh.TriangleIndices.Add(index2);
            mesh.TriangleIndices.Add(index3);
        }

        // in case a point has been defined before, read index, otherwise generate point and read new index
        private int AddPunkt(Point3DCollection points, Point3D point)
        {
            // in case point exists in Dictionary, read index stored
            if (pointDictionary.ContainsKey(point))
                return pointDictionary[point];

            // in case point was not found, generate new point
            points.Add(point);
            pointDictionary.Add(point, points.Count - 1);
            return points.Count - 1;
        }
    }

    internal class ElementSresses
    {
        public ElementSresses(double[] stresses)
        {
            Stresses = stresses;
        }

        public double[] Stresses { get; }
    }
}