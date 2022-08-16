using FE_Analysis.Heat_Transfer.Model_Data;
using FE_Analysis.Structural_Analysis.Model_Data;
using FEALibrary.Model;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;

namespace FE_Analysis
{
    public partial class MainWindow
    {
        private FeParser parse;
        private FeModel model;
        private Analysis modelAnalysis;
        private OpenFileDialog fileDialog;
        private string path;
        public static Structural_Analysis.ModelDataShow.StructuralModelVisualize structuralModel;
        public static Structural_Analysis.Results.StaticResultsVisualize staticResults;
        public static Heat_Transfer.ModelDataShow.HeatDataVisualize heatModel;
        public static Heat_Transfer.Results.StationaryResultsVisualize stationaryResults;


        private string[] lines;
        private bool heatData, structuresData, timeintegrationData;
        public static bool analysed, timeintegrationAnalysed;

        public MainWindow()
        {
            InitializeComponent();
        }
        //********************************************************************
        // Heat Transfer Analysis
        private void HeatDataRead(object sender, RoutedEventArgs e)
        {
            Language = XmlLanguage.GetLanguage("us-US");
            var sb = new StringBuilder();
            fileDialog = new OpenFileDialog
            {
                Filter = "inp files (*.inp)|*.inp|All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal)
            };

            if (Directory.Exists(fileDialog.InitialDirectory + "\\FE-Analysis-App\\input"))
            {
                fileDialog.InitialDirectory += "\\FE-Analysis-App\\input\\HeatTransfer";
                fileDialog.ShowDialog();
            }
            else
            {
                _ = MessageBox.Show("Directory for input file " + fileDialog.InitialDirectory +
                                    " \\FE-Analysis-App\\input not found", "Heat Transfer Analysis");
                fileDialog.ShowDialog();
            }

            path = fileDialog.FileName;

            try
            {
                if (path.Length == 0)
                {
                    _ = MessageBox.Show("Input file is empty", "Heat Transfer Analysis");
                    return;
                }

                lines = File.ReadAllLines(path, Encoding.UTF8);
            }
            catch (ParseException)
            {
                throw new ParseException("Exit: Error when reading input file");
            }

            parse = new FeParser();
            parse.ParseModel(lines);
            model = parse.FeModel;
            parse.ParseNodes(lines);

            var heatElements = new Heat_Transfer.ModelDataRead.ElementParser();
            heatElements.ParseElements(lines, model);

            var heatMaterial = new Heat_Transfer.ModelDataRead.MaterialParser();
            heatMaterial.ParseMaterials(lines, model);

            var heatLoads = new Heat_Transfer.ModelDataRead.LoadParser();
            heatLoads.ParseLoads(lines, model);

            var heatBoundaryCondition = new Heat_Transfer.ModelDataRead.BoundaryConditionParser();
            heatBoundaryCondition.ParseBoundaryConditions(lines, model);

            var heatTransient = new Heat_Transfer.ModelDataRead.TransientParser();
            heatTransient.ParseTimeIntegration(lines, model);

            timeintegrationData = heatTransient.timeIntegrationData;
            heatData = true;
            analysed = false;
            timeintegrationAnalysed = false;

            sb.Append(FeParser.InputFound + "\nHeat Model input data successfully read");
            _ = MessageBox.Show(sb.ToString(), "Heat Transfer Analysis");
            sb.Clear();
        }
        private void HeatDataEdit(object sender, RoutedEventArgs e)
        {
            if (path == null)
            {
                var modelDataEdit = new DataInput.ModelDataEdit();
                modelDataEdit.Show();
            }
            else
            {
                var modelDataEdit = new DataInput.ModelDataEdit(path);
                modelDataEdit.Show();
            }
        }
        private void HeatDataSave(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            var heatFile = new DataInput.NewFileName();
            heatFile.ShowDialog();

            var name = heatFile.fileName;

            if (model == null)
            {
                _ = MessageBox.Show("Model has not yet been defined", "Heat Transfer Analysis");
                return;
            }

            var rows = new List<string>
            {
                "ModelName",
                model.ModelId,
                "\nSpace dimension"
            };
            var numberNodalDof = 1;
            rows.Add(model.SpatialDimension + "\t" + numberNodalDof + "\n");

            // Nodes
            rows.Add("Node");
            if (model.SpatialDimension == 2)
                rows.AddRange(model.Nodes.Select(knoten => knoten.Key
                                                           + "\t" + knoten.Value.Coordinates[0] + "\t" +
                                                           knoten.Value.Coordinates[1]));
            else
                rows.AddRange(model.Nodes.Select(knoten => knoten.Key
                                                           + "\t" + knoten.Value.Coordinates[0] + "\t" +
                                                           knoten.Value.Coordinates[1] + "\t" +
                                                           knoten.Value.Coordinates[2]));

            // Elements
            var allElements2D2 = new List<Element2D2>();
            var allElements2D3 = new List<Element2D3>();
            var allElements2D4 = new List<Element2D4>();
            var allElements3D8 = new List<Element3D8>();
            foreach (var item in model.Elements)
                switch (item.Value)
                {
                    case Element2D2 element2D2:
                        allElements2D2.Add(element2D2);
                        break;
                    case Element2D3 element2D3:
                        allElements2D3.Add(element2D3);
                        break;
                    case Element2D4 element2D4:
                        allElements2D4.Add(element2D4);
                        break;
                    case Element3D8 element3D8:
                        allElements3D8.Add(element3D8);
                        break;
                }

            if (allElements2D2.Count != 0)
            {
                rows.Add("\n" + "Elements2D2Nodes");
                rows.AddRange(allElements2D2.Select(item => item.ElementId + "\t" + item.NodeIds[0] + "\t"
                                                            + item.NodeIds[1] + "\t" + item.ElementMaterialId));
            }

            if (allElements2D3.Count != 0)
            {
                rows.Add("\n" + "Elements2D3Nodes");
                rows.AddRange(allElements2D3.Select(item => item.ElementId + "\t" + item.NodeIds[0] + "\t"
                                                            + item.NodeIds[1] + "\t" + item.NodeIds[2] + "\t" +
                                                            item.ElementMaterialId));
            }

            if (allElements2D4.Count != 0)
            {
                rows.Add("\n" + "Elements2D4Nodes");
                rows.AddRange(allElements2D4.Select(item => item.ElementId + "\t" + item.NodeIds[0] + "\t"
                                                            + item.NodeIds[1] + "\t" + item.NodeIds[2] + "\t" +
                                                            item.NodeIds[3] + "\t" + item.ElementMaterialId));
            }

            if (allElements3D8.Count != 0)
            {
                rows.Add("\n" + "Elements3D8Nodes");
                rows.AddRange(allElements3D8.Select(item => item.ElementId + "\t" + item.NodeIds[0] + "\t"
                                                            + item.NodeIds[1] + "\t" + item.NodeIds[2] + "\t" +
                                                            item.NodeIds[3] + "\t"
                                                            + item.NodeIds[4] + "\t" + item.NodeIds[5] + "\t" +
                                                            item.NodeIds[6] + "\t"
                                                            + item.NodeIds[7] + "\t" + item.ElementMaterialId));
            }

            // Materials
            rows.Add("\n" + "Material");
            foreach (var item in model.Material)
            {
                sb.Clear();
                sb.Append(item.Value.MaterialId + "\t" + item.Value.MaterialValues[0]);
                for (var i = 1; i < item.Value.MaterialValues.Length; i++)
                    sb.Append("\t" + item.Value.MaterialValues[i]);
                rows.Add(sb.ToString());
            }

            // Loads
            foreach (var item in model.Loads)
            {
                sb.Clear();
                sb.Append("\n" + "NodeLoads");
                sb.Append(item.Value.LoadId + "\t" + item.Value.Intensity[0]);
                for (var i = 1; i < item.Value.Intensity.Length; i++) sb.Append("\t" + item.Value.Intensity[i]);
                rows.Add(sb.ToString());
            }

            foreach (var item in model.LineLoads)
            {
                sb.Clear();
                sb.Append("\n" + "LineLoads");
                sb.Append(item.Value.LoadId + "\t" + item.Value.StartNodeId + "\t" + item.Value.EndNodeId + "\t"
                          + item.Value.Intensity[0] + "\t" + item.Value.Intensity[1]);
                rows.Add(sb.ToString());
            }

            var allElementLoads3 = new List<ElementLoad3>();
            var allElementLoads4 = new List<ElementLoad4>();
            foreach (var item in model.ElementLoads)
                switch (item.Value)
                {
                    case ElementLoad3 elementload3:
                        allElementLoads3.Add(elementload3);
                        break;
                    case ElementLoad4 elementload4:
                        allElementLoads4.Add(elementload4);
                        break;
                }

            if (allElementLoads3.Count != 0)
            {
                rows.Add("\n" + "ElementLoad3");
                rows.AddRange(allElementLoads3.Select(item => item.LoadId + "\t" + item.ElementId + "\t"
                                                              + item.Intensity[0] + "\t" + item.Intensity[1] +
                                                              "\t" + item.Intensity[2]));
            }

            if (allElementLoads4.Count != 0)
            {
                rows.Add("\n" + "ElementLoad4");
                rows.AddRange(allElementLoads4.Select(item => item.LoadId + "\t" + item.ElementId + "\t"
                                                              + item.Intensity[0] + "\t" + item.Intensity[1]
                                                              + "\t" + item.Intensity[2] + "\t" +
                                                              item.Intensity[3]));
            }

            // Boundary conditions
            rows.Add("\n" + "BoundaryConditions");
            foreach (var item in model.BoundaryConditions)
            {
                sb.Clear();
                sb.Append(item.Value.SupportId + "\t" + item.Value.NodeId + "\t" + item.Value.Prescribed[0]);
                rows.Add(sb.ToString());
            }

            // Eigensolutions
            if (model.Eigenstate != null)
            {
                rows.Add("\n" + "Eigensolutions");
                rows.Add(model.Eigenstate.Id + "\t" + model.Eigenstate.NumberOfStates);
            }

            // Parameter
            if (model.Timeintegration != null)
            {
                rows.Add("\n" + "TimeIntegration");
                rows.Add(model.Timeintegration.Id + "\t" + model.Timeintegration.Tmax + "\t" +
                         model.Timeintegration.Dt
                         + "\t" + model.Timeintegration.Parameter1);
            }

            // time dependent initial conditions
            if (model.Timeintegration.FromStationary || model.Timeintegration.InitialConditions.Count != 0)
                rows.Add("\n" + "InitialTemperatures");
            if (model.Timeintegration.FromStationary) rows.Add("stationary solution");

            foreach (var item in model.Timeintegration.InitialConditions)
            {
                var knotenwerte = (NodalValues)item;
                rows.Add(knotenwerte.NodeId + "\t" + knotenwerte.Values[0]);
            }

            // time dependent boundary conditions 
            if (model.TimeDependentBoundaryConditions.Count != 0)
                rows.Add("\n" + "Time Dependent Boundary Temperatures");
            foreach (var item in model.TimeDependentBoundaryConditions)
            {
                sb.Clear();
                sb.Append(item.Value.SupportId + "\t" + item.Value.NodeId);
                if (item.Value.VariationType == 0) sb.Append("\tfile");
                if (item.Value.VariationType == 1)
                {
                    sb.Append("\tconstant" + item.Value.ConstantTemperature);
                }
                else if (item.Value.VariationType == 2)
                {
                    sb.Append("\tharmonic\t" + item.Value.Amplitude + "\t" + item.Value.Frequency + "\t" +
                              item.Value.PhaseAngle);
                }
                else if (item.Value.VariationType == 3)
                {
                    sb.Append("\tlinear");
                    var anzahlIntervalle = item.Value.Interval.Length;
                    for (var i = 0; i < anzahlIntervalle; i += 2)
                        sb.Append("\t" + item.Value.Interval[i] + ";" + item.Value.Interval[i + 1]);
                }

                rows.Add(sb.ToString());
            }

            // time dependent nodal temperatures
            if (model.TimeDependentNodeLoads.Count != 0) rows.Add("\n" + "TimeDependent Node Loads");
            foreach (var item in model.TimeDependentNodeLoads)
            {
                sb.Clear();
                sb.Append(item.Value.LoadId + "\t" + item.Value.NodeId);
                if (item.Value.VariationType == 0)
                {
                    sb.Append("\tfile");
                }
                else if (item.Value.VariationType == 2)
                {
                    sb.Append("\tharmonic\t" + item.Value.Amplitude + "\t" + item.Value.Frequency + "\t" +
                              item.Value.PhaseAngle);
                }
                else if (item.Value.VariationType == 3)
                {
                    sb.Append("\tlinear");
                    var anzahlIntervalle = item.Value.Interval.Length;
                    for (var i = 0; i < anzahlIntervalle; i += 2)
                        sb.Append("\t" + item.Value.Interval[i] + ";" + item.Value.Interval[i + 1]);
                }

                rows.Add(sb.ToString());
            }

            // time dependent element temperatures
            if (model.TimeDependentElementLoads.Count != 0) rows.Add("\n" + "time dependent ElementTemperatures");
            foreach (var item in model.TimeDependentElementLoads)
            {
                sb.Clear();
                sb.Append(item.Key + "\t" + item.Value.ElementId);

                if (item.Value.VariationType == 1)
                {
                    sb.Append("\tconstant");
                    for (var i = 0; i < item.Value.P.Length; i++) sb.Append("\t" + item.Value.P[i]);
                }

                rows.Add(sb.ToString());
            }

            // end of File
            rows.Add("\nend");

            // write all rows in a File
            var fileName = "\\" + name + ".inp";
            path = fileDialog.InitialDirectory + fileName;
            File.WriteAllLines(path, rows);
        }
        private void HeatDataShow(object sender, RoutedEventArgs e)
        {
            var heat = new Heat_Transfer.ModelDataShow.HeatDataShow(model);
            heat.Show();
        }
        private void HeatDataVisualize(object sender, RoutedEventArgs e)
        {
            heatModel = new Heat_Transfer.ModelDataShow.HeatDataVisualize(model);
            heatModel.Show();
        }
        private void HeatDataAnalyse(object sender, EventArgs e)
        {
            if (heatData)
            {
                modelAnalysis = new Analysis(model);
                modelAnalysis.ComputeSystemMatrix();
                modelAnalysis.ComputeSystemVector();
                modelAnalysis.SolveEquations();
                analysed = true;
                _ = MessageBox.Show("System Equations successfully solved", "Heat Transfer Analysis");
            }
            else
            {
                _ = MessageBox.Show("Heat Model Data must be defined first", "Heat Transfer Analysis");
            }
        }
        private void HeatTransferAnalysisResultsShow(object sender, EventArgs e)
        {
            if (heatData)
            {
                if (!analysed)
                {
                    modelAnalysis = new Analysis(model);
                    modelAnalysis.ComputeSystemMatrix();
                    modelAnalysis.ComputeSystemVector();
                    modelAnalysis.SolveEquations();
                    analysed = true;
                }

                var results = new Heat_Transfer.Results.StationaryResultsShow(model);
                results.Show();
            }
            else
            {
                _ = MessageBox.Show("Model data for Heat Transfer Analysis not yet defined", "Heat Transfer Analysis");
            }
        }
        private void HeatTransferAnalysisResultsVisualize(object sender, RoutedEventArgs e)
        {
            if (heatData)
            {
                if (!analysed)
                {
                    modelAnalysis = new Analysis(model);
                    modelAnalysis.ComputeSystemMatrix();
                    modelAnalysis.ComputeSystemVector();
                    modelAnalysis.SolveEquations();
                    analysed = true;
                }

                stationaryResults = new Heat_Transfer.Results.StationaryResultsVisualize(model);
                stationaryResults.Show();
            }
            else
            {
                _ = MessageBox.Show("Model Data for Heat Transfer Analysis not defined yet", "Heat Transfer Analysis");
            }
        }
        private void InstationaryData(object sender, RoutedEventArgs e)
        {
            if (model == null)
            {
                _ = MessageBox.Show("Model Data for instationary Heat Transfer Analysis not defined yet", "Heat Transfer Analysis");
            }
            else
            {
                var heat = new Heat_Transfer.ModelDataShow.InstationaryDataShow(model);
                heat.Show();
                timeintegrationAnalysed = false;
            }
        }
        private void EigensolutionHeatAnalyse(object sender, RoutedEventArgs e)
        {
            if (model != null)
            {
                if (!analysed)
                {
                    modelAnalysis = new Analysis(model);
                    modelAnalysis.ComputeSystemMatrix();
                    analysed = true;
                }

                // default = 2 Eigenstates, if not specified otherwise
                if (model.Eigenstate == null) model.Eigenstate = new Eigenstates("default", 2);
                modelAnalysis.Eigenstates();
                _ = MessageBox.Show("Eigensolutions successfully analysed", "Heat Transfer Analysis");
            }
            else
            {
                _ = MessageBox.Show("Heat Model data not defined yet", "Heat Transfer Analysis");
            }
        }
        private void EigensolutionHeatShow(object sender, RoutedEventArgs e)
        {
            if (model != null)
            {
                if (!analysed)
                {
                    modelAnalysis = new Analysis(model);
                    modelAnalysis.ComputeSystemMatrix();
                }

                // default = 2 Eigenstates, if not specified otherwise
                if (model.Eigenstate == null) model.Eigenstate = new Eigenstates("default", 2);
                modelAnalysis.Eigenstates();

                var eigen = new Heat_Transfer.Results.EigensolutionsShow(model);
                eigen.Show();
            }
            else
            {
                _ = MessageBox.Show("Heat Transfer Model data not defined yet", "Heat Transfer Analysis");
            }
        }
        private void EigensolutionHeatVisualize(object sender, RoutedEventArgs e)
        {
            if (model != null)
            {
                if (!timeintegrationAnalysed)
                {
                    modelAnalysis = new Analysis(model);
                    modelAnalysis.ComputeSystemMatrix();
                    // default = 2 Eigenstates, if not specified otherwise
                    if (model.Eigenstate == null) model.Eigenstate = new Eigenstates("default", 2);
                }

                modelAnalysis.Eigenstates();
                var visual = new Heat_Transfer.Results.EigensolutionVisualize(model);
                visual.Show();
            }
            else
            {
                _ = MessageBox.Show("Heat Transfer Model data not defined yet", "Heat Transfer Analysis");
            }
        }
        private void InstationaryHeatTransferAnalysis(object sender, RoutedEventArgs e)
        {
            if (timeintegrationData)
            {
                if (!analysed)
                {
                    modelAnalysis = new Analysis(model);
                    modelAnalysis.ComputeSystemMatrix();
                    modelAnalysis.ComputeSystemVector();
                    modelAnalysis.SolveEquations();
                    analysed = true;
                }

                modelAnalysis.TimeIntegration1StOrder();
                timeintegrationAnalysed = true;
                _ = MessageBox.Show("Time integration successfully completed", "instationary Heat Transfer Analysis");
            }
            else
            {
                _ = MessageBox.Show("Model Data for time history analysis not yet defined", "instationary Heat Transfer Analysis");
                double tmax = 0;
                double dt = 0;
                double alfa = 0;
                model.Timeintegration = new Heat_Transfer.Model_Data.TimeIntegration(tmax, dt, alfa) { FromStationary = false };
                timeintegrationData = true;
                var heat = new Heat_Transfer.ModelDataShow.InstationaryDataShow(model);
                heat.Show();
                timeintegrationAnalysed = false;
            }
        }
        private void InstationaryHeatTransferAnalysisResultsShow(object sender, RoutedEventArgs e)
        {
            if (timeintegrationAnalysed)
                _ = new Heat_Transfer.Results.InstationaryResultsShow(model);
            else
                _ = MessageBox.Show("Time Integration not yet completed!!", "instationary Heat Transfer Analysis");
        }
        private void InstationaryModelStatesVisualize(object sender, RoutedEventArgs e)
        {
            if (timeintegrationAnalysed)
            {
                var wärmeModell = new Heat_Transfer.Results.InstationaryModelStatesVisualize(model);
                wärmeModell.Show();
            }
            else
            {
                _ = MessageBox.Show("Time Integration not yet completed!!", "instationary Heat Transfer Analysis");
            }
        }
        private void NodalTimeHistoryVisualize(object sender, RoutedEventArgs e)
        {
            if (timeintegrationAnalysed)
            {
                var nodalTimeHistoriesVisualize = new Heat_Transfer.Results.NodalTimeHistoriesVisualize(model);
                nodalTimeHistoriesVisualize.Show();
            }
            else
            {
                _ = MessageBox.Show("Time Integration not yet completed!!", "instationary Heat Transfer Analysis");
            }
        }

        //********************************************************************
        // Structural Analysis
        private void StructuralModelDataRead(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            fileDialog = new OpenFileDialog
            {
                Filter = "inp files (*.inp)|*.inp|All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal)
                //InitialDirectory = Directory.GetCurrentDirectory()
            };

            if (Directory.Exists(fileDialog.InitialDirectory + "\\FE-Analysis-App\\input"))
            {
                fileDialog.InitialDirectory += "\\FE-Analysis-App\\input\\StructuralAnalysis";
                fileDialog.ShowDialog();
            }
            else
            {
                _ = MessageBox.Show("Directory for Input Files " + fileDialog.InitialDirectory +
                                    " \\FE-Analysis-App\\input\\StructuralAnalysis not found", "Structural Analysis");
                fileDialog.ShowDialog();
            }

            path = fileDialog.FileName;

            try
            {
                if (path.Length == 0)
                {
                    _ = MessageBox.Show("Input File is empty", "Structural Analysis");
                    return;
                }

                lines = File.ReadAllLines(path, Encoding.UTF8);
            }
            catch (ParseException)
            {
                throw new ParseException("Exit: Error when reading input file ");
            }

            parse = new FeParser();
            parse.ParseModel(lines);
            model = parse.FeModel;
            parse.ParseNodes(lines);

            var structuralElements = new Structural_Analysis.ModelDataRead.ElementParser();
            structuralElements.ParseElements(lines, model);

            var tragwerksMaterial = new Structural_Analysis.ModelDataRead.MaterialParser();
            tragwerksMaterial.ParseMaterials(lines, model);

            var tragwerksLasten = new Structural_Analysis.ModelDataRead.LoadParser();
            tragwerksLasten.ParseLoads(lines, model);

            var tragwerksRandbedingungen = new Structural_Analysis.ModelDataRead.BoundaryConditionParser();
            tragwerksRandbedingungen.ParseBoundaryConditions(lines, model);

            var structureTransient = new Structural_Analysis.ModelDataRead.TransientParser();
            structureTransient.ParseTimeIntegration(lines, model);

            timeintegrationData = structureTransient.timeIntegrationData;
            structuresData = true;
            analysed = false;
            timeintegrationAnalysed = false;

            sb.Append(FeParser.InputFound + "\nStructural Model data successfully read");
            _ = MessageBox.Show(sb.ToString(), "Structural Analysis");
            sb.Clear();
        }
        private void StructuralModelDataEdit(object sender, RoutedEventArgs e)
        {
            if (path == null)
            {
                var structureData = new DataInput.ModelDataEdit();
                structureData.Show();
            }
            else
            {
                var structureData = new DataInput.ModelDataEdit(path);
                structureData.Show();
            }
        }
        private void StructuralModelDataSave(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            var structureFile = new DataInput.NewFileName();
            structureFile.ShowDialog();

            var name = structureFile.fileName;

            var rows = new List<string>
            {
                "Model Name",
                model.ModelId,
                "\nSpatial Dimension"
            };
            rows.Add(model.SpatialDimension + "\t" + model.NumberNodalDof);

            // Nodes
            rows.Add("\nNodes");
            switch (model.SpatialDimension)
            {
                case 1:
                    rows.AddRange(model.Nodes.Select(node => node.Key
                                                   + "\t" + node.Value.Coordinates[0]));
                    break;
                case 2:
                    rows.AddRange(model.Nodes.Select(node => node.Key
                                                   + "\t" + node.Value.Coordinates[0] 
                                                   + "\t" + node.Value.Coordinates[1]));
                    break;
                case 3:
                    rows.AddRange(model.Nodes.Select(node => node.Key
                                                   + "\t" + node.Value.Coordinates[0] 
                                                   + "\t" + node.Value.Coordinates[1] 
                                                   + "\t" + node.Value.Coordinates[2]));
                    break;
                default:
                    _ = MessageBox.Show("wrong spatial dimension, must be 1, 2 or 3", "Structural Analysis");
                    return;
            }

            // Elements
            var allTrussElements = new List<Truss>();
            var allBeamElements = new List<Beam>();
            var allBeamHingedElements = new List<BeamHinged>();
            var allSpringElements = new List<SpringElement>();
            var allCrossSections = new List<CrossSection>();
            foreach (var item in model.Elements)
                switch (item.Value)
                {
                    case Truss truss:
                        allTrussElements.Add(truss);
                        break;
                    case Beam beam:
                        allBeamElements.Add(beam);
                        break;
                    case BeamHinged beamHinged:
                        allBeamHingedElements.Add(beamHinged);
                        break;
                    case SpringElement springElement:
                        allSpringElements.Add(springElement);
                        break;
                }

            foreach (var item in model.CrossSection) allCrossSections.Add(item.Value);

            if (allTrussElements.Count != 0)
            {
                rows.Add("\nTruss");
                rows.AddRange(allTrussElements.Select(item => item.ElementId + "\t" + item.NodeIds[0] + "\t"
                                                                    + item.NodeIds[1] + "\t" +
                                                                    item.ElementCrossSectionId + "\t" +
                                                                    item.ElementMaterialId));
            }

            if (allBeamElements.Count != 0)
            {
                rows.Add("\nBeam");
                rows.AddRange(allBeamElements.Select(item => item.ElementId + "\t" + item.NodeIds[0] + "\t"
                                                               + item.NodeIds[1] + "\t" + item.ElementCrossSectionId +
                                                               "\t" + item.ElementMaterialId));
            }

            if (allBeamHingedElements.Count != 0)
            {
                rows.Add("\nBeamHinged");
                rows.AddRange(allBeamHingedElements.Select(item => item.ElementId + "\t" + item.NodeIds[0] + "\t"
                                                                     + item.NodeIds[1] + "\t" +
                                                                     item.ElementCrossSectionId + "\t" +
                                                                     item.ElementMaterialId + "\t" + item.Type));
            }

            if (allSpringElements.Count != 0)
            {
                rows.Add("\nSpringElement");
                rows.AddRange(allSpringElements.Select(item => item.ElementId + "\t" + item.NodeIds[0] + "\t"
                                                                 + item.ElementMaterialId));
            }

            if (allCrossSections.Count != 0)
            {
                rows.Add("\nCrossSection");
                rows.AddRange(allCrossSections.Select(item => item.CrossSectionId + "\t"
                    + item.CrossSectionValues[0] + "\t" + item.CrossSectionValues[1]));
            }

            // Materials
            rows.Add("\nMaterial");
            foreach (var item in model.Material)
            {
                sb.Clear();
                sb.Append(item.Value.MaterialId + "\t" + item.Value.MaterialValues[0]);
                for (var i = 1; i < item.Value.MaterialValues.Length; i++) sb.Append("\t" + item.Value.MaterialValues[i]);
                rows.Add(sb.ToString());
            }

            // Loads
            foreach (var item in model.Loads)
            {
                rows.Add("\nNodeLoad");
                sb.Clear();
                sb.Append(item.Value.LoadId + "\t" + item.Value.NodeId + "\t" + item.Value.Intensity[0]);
                for (var i = 1; i < item.Value.Intensity.Length; i++) sb.Append("\t" + item.Value.Intensity[i]);
                rows.Add(sb.ToString());
            }

            foreach (var item in model.PointLoads)
            {
                var pointLoad = (PointLoad)item.Value;
                sb.Clear();
                rows.Add("\nPointLoad");
                rows.Add(pointLoad.LoadId + "\t" + pointLoad.ElementId
                           + "\t" + pointLoad.Intensity[0] + "\t" + pointLoad.Intensity[1] + "\t" + pointLoad.Offset);
            }

            foreach (var item in model.ElementLoads)
            {
                sb.Clear();
                rows.Add("\nLineLoad");
                rows.Add(item.Value.LoadId + "\t" + item.Value.ElementId
                           + "\t" + item.Value.Intensity[0] + "\t" + item.Value.Intensity[1]
                           + "\t" + item.Value.Intensity[2] + "\t" + item.Value.Intensity[3]
                           + "\t" + item.Value.InElementCoordinateSystem);
            }

            // Boundary Conditions
            var fix = string.Empty;
            rows.Add("\nSupport");
            foreach (var item in model.BoundaryConditions)
            {
                if (item.Value.Type == 1) fix = "x";
                else if (item.Value.Type == 2) fix = "y";
                else if (item.Value.Type == 3) fix = "xy";
                else if (item.Value.Type == 7) fix = "xyr";
                rows.Add(item.Value.SupportId + "\t" + item.Value.NodeId + "\t" + fix);
            }

            // end of file
            rows.Add("\nend");

            // write all lines to file
            var fileName = "\\" + name + ".inp";
            path = fileDialog.InitialDirectory + fileName;
            File.WriteAllLines(path, rows);
        }
        private void StructuralModelDataShow(object sender, EventArgs e)
        {
            var tragwerk = new Structural_Analysis.ModelDataShow.StructuralModelDataShow(model);
            tragwerk.Show();
        }
        private void StructuralModelDataVisualize(object sender, RoutedEventArgs e)
        {
            structuralModel = new Structural_Analysis.ModelDataShow.StructuralModelVisualize(model);
            structuralModel.Show();
        }
        private void StructuralModelStaticAnalysis(object sender, EventArgs e)
        {
            if (structuresData)
            {
                modelAnalysis = new Analysis(model);
                modelAnalysis.ComputeSystemMatrix();
                modelAnalysis.ComputeSystemVector();
                modelAnalysis.SolveEquations();
                analysed = true;
                _ = MessageBox.Show("System equations successfully solved", "static Structural Analysis");
            }
            else
            {
                _ = MessageBox.Show("Structural Model Data must be defined first", "static Structural Analysis");
            }
        }
        private void StructuralModelStaticResultsShow(object sender, EventArgs e)
        {
            if (!analysed)
            {
                modelAnalysis = new Analysis(model);
                modelAnalysis.ComputeSystemMatrix();
                modelAnalysis.ComputeSystemVector();
                modelAnalysis.SolveEquations();
                analysed = true;
            }

            var results = new Structural_Analysis.Results.StaticResultsShow(model);
            results.Show();
        }
        private void StructuralModelStaticResultsVisualize(object sender, RoutedEventArgs e)
        {
            if (!analysed)
            {
                modelAnalysis = new Analysis(model);
                modelAnalysis.ComputeSystemMatrix();
                modelAnalysis.ComputeSystemVector();
                modelAnalysis.SolveEquations();
                analysed = true;
            }

            staticResults = new Structural_Analysis.Results.StaticResultsVisualize(model);
            staticResults.Show();
        }
        private void StructuralModelEigensolutionAnalysis(object sender, RoutedEventArgs e)
        {
            if (model != null)
            {
                if (!analysed)
                {
                    modelAnalysis = new Analysis(model);
                    modelAnalysis.ComputeSystemMatrix();
                }

                // default = 2 Eigenstates, if not specified otherwise
                if (model.Eigenstate == null) model.Eigenstate = new Eigenstates("default", 2);
                modelAnalysis.Eigenstates();
                _ = MessageBox.Show("Eigenfrequencies successfully analysed", "dynamic Structural Analysis");
            }
            else
            {
                _ = MessageBox.Show("Structural Model Data not defined yet", "dynamic Structural Analysis");
            }
        }
        private void StructuralModelEigensolutionShow(object sender, RoutedEventArgs e)
        {
            if (model != null)
            {
                if (!analysed)
                {
                    modelAnalysis = new Analysis(model);
                    modelAnalysis.ComputeSystemMatrix();
                    // default = 2 Eigenstates, if not specified otherwise
                    if (model.Eigenstate == null) model.Eigenstate = new Eigenstates("default", 2);
                }

                modelAnalysis.Eigenstates();
                var eigen = new Structural_Analysis.Results.EigensolutionShow(model);
                eigen.Show();
            }
            else
            {
                _ = MessageBox.Show("Structural Model data not specified yet", "dynamic Structural Analysis");
            }
        }
        private void StructuralModelEigensolutionVisualize(object sender, RoutedEventArgs e)
        {
            if (model != null)
            {
                if (!analysed)
                {
                    modelAnalysis = new Analysis(model);
                    modelAnalysis.ComputeSystemMatrix();
                    // default = 2 Eigenstates, falls nicht anders spezifiziert
                    if (model.Eigenstate == null) model.Eigenstate = new Eigenstates("default", 2);
                }

                modelAnalysis.Eigenstates();
                var visual = new Structural_Analysis.Results.EigensolutionVisualize(model);
                visual.Show();
            }
            else
            {
                _ = MessageBox.Show("Structural Model data not defined yet", "dynamic Structural Analysis");
            }
        }
        private void StructuralModelDynamicDataShow(object sender, EventArgs e)
        {
            if (timeintegrationData)
            {
                var tragwerk = new Structural_Analysis.ModelDataShow.DynamicModelDataShow(model);
                tragwerk.Show();
            }
            else
            {
                _ = MessageBox.Show("Model Data for time history analysis have not been specified yet", "dynamic Structural Analysis");
            }
        }
        private void StructuralModelDynamicAnalysis(object sender, EventArgs e)
        {
            if (timeintegrationData)
            {
                if (!analysed)
                {
                    modelAnalysis = new Analysis(model);
                    modelAnalysis.ComputeSystemMatrix();
                    modelAnalysis.ComputeSystemVector();
                    modelAnalysis.SolveEquations();
                    analysed = true;
                }

                modelAnalysis.TimeIntegration2NdOrder();
                timeintegrationAnalysed = true;
            }
            else
            {
                _ = MessageBox.Show("Model data for time history analysis have not been specified yet", "dynamic Structural Analysis");
            }
        }
        private void StructuralModelDynamicResultsShow(object sender, RoutedEventArgs e)
        {
            if (timeintegrationAnalysed)
                _ = new Structural_Analysis.Results.DynamicResultsShow(model);
            else
                _ = MessageBox.Show("Time Integration has not yet been completed!!", "dynamic Structural Analysis");
        }
        private void StructuralModelDynamicModelStatesVisualize(object sender, RoutedEventArgs e)
        {
            if (timeintegrationAnalysed)
            {
                var dynamicResults = new Structural_Analysis.Results.DynamicModelStatesVisualize(model);
                dynamicResults.Show();
            }
            else
            {
                _ = MessageBox.Show("Time Integration has not yet been completed!!", "dynamic Structural Analysis");
            }
        }
        private void StructuralModelNodalTimeHistoriesVisualize(object sender, RoutedEventArgs e)
        {
            if (timeintegrationAnalysed)
            {
                var nodalTimeHistories = new Structural_Analysis.Results.NodalTimeHistoriesVisualize(model);
                nodalTimeHistories.Show();
            }
            else
            {
                _ = MessageBox.Show("Time Integration has not yet been completed!!", "dynamic Structural Analysis");
            }
        }

        //********************************************************************
        // Elasticity Analysis
        private void ElasticityDataRead(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            fileDialog = new OpenFileDialog
            {
                Filter = "inp files (*.inp)|*.inp|All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal)
            };

            if (Directory.Exists(fileDialog.InitialDirectory + "\\FE-Analysis-App\\input"))
            {
                fileDialog.InitialDirectory += "\\FE-Analysis-App\\input\\Elasticity";
                fileDialog.ShowDialog();
            }
            else
            {
                _ = MessageBox.Show("Directory for input file " + fileDialog.InitialDirectory +
                                    " \\FE-Analysis-App\\input\\Elasticity not found",
                    "Elasticity Analysis");
                fileDialog.ShowDialog();
            }

            path = fileDialog.FileName;

            try
            {
                if (path.Length == 0)
                {
                    _ = MessageBox.Show("Input file is empty", "Elasticity Analysis");
                    return;
                }

                lines = File.ReadAllLines(path, Encoding.UTF8);
            }
            catch (ParseException)
            {
                throw new ParseException("Exit: Error when reading input file ");
            }

            parse = new FeParser();
            parse.ParseModel(lines);
            model = parse.FeModel;
            parse.ParseNodes(lines);

            var parseElasticity = new Elasticity.ModelDataRead.ElasticityParser();
            parseElasticity.ParseElasticity(lines, model);

            analysed = false;

            sb.Clear();
            sb.Append(FeParser.InputFound + "\n\nModel Data for elasticity analysis successfully read");
            _ = MessageBox.Show(sb.ToString(), "Elasticity Analysis");
            sb.Clear();
        }
        private void ElasticityDataEdit(object sender, RoutedEventArgs e)
        {
            if (path == null)
            {
                var elasticityData = new DataInput.ModelDataEdit();
                elasticityData.Show();
            }
            else
            {
                var elasticityData = new DataInput.ModelDataEdit(path);
                elasticityData.Show();
            }
        }
        private void ElasticityDataShow(object sender, EventArgs e)
        {
            if (model == null)
            {
                _ = MessageBox.Show("Model data not yet specified", "Elasticity Analysis");
                return;
            }

            var structure = new Elasticity.ModelDataShow.ElasticityDataShow(model);
            structure.Show();
        }
        private void ElasticityDataSave(object sender, RoutedEventArgs e)
        {
            var elasticityFile = new DataInput.NewFileName();
            elasticityFile.ShowDialog();

            var name = elasticityFile.fileName;

            var found = new List<string>
            {
                "Model Name",
                model.ModelId,
                "\nSpatial Dimension"
            };
            var nodalDof = 3;
            found.Add(model.SpatialDimension + "\t" + nodalDof);

            // Node
            found.Add("\nNodes");
            if (model.SpatialDimension == 2)
                found.AddRange(model.Nodes.Select(knoten => knoten.Key
                                                            + "\t" + knoten.Value.Coordinates[0] + "\t" +
                                                            knoten.Value.Coordinates[1]));
            else
                found.AddRange(model.Nodes.Select(knoten => knoten.Key
                                                            + "\t" + knoten.Value.Coordinates[0] + "\t" +
                                                            knoten.Value.Coordinates[1] + "\t" +
                                                            knoten.Value.Coordinates[2]));

            // Elements
            var alleElemente2D3 = new List<Elasticity.ModelData.Element2D3>();
            var alleElemente3D8 = new List<Elasticity.ModelData.Element3D8>();
            var allCrossSections = new List<CrossSection>();
            foreach (var item in model.Elements)
                switch (item.Value)
                {
                    case Elasticity.ModelData.Element2D3 element2D3:
                        alleElemente2D3.Add(element2D3);
                        break;
                    case Elasticity.ModelData.Element3D8 element3D8:
                        alleElemente3D8.Add(element3D8);
                        break;
                }

            foreach (var item in model.CrossSection) allCrossSections.Add(item.Value);

            if (alleElemente2D3.Count != 0)
            {
                found.Add("\nElement2D3");
                found.AddRange(alleElemente2D3.Select(item => item.ElementId + "\t" + item.NodeIds[0] + "\t"
                                                              + item.NodeIds[1] + "\t" + item.NodeIds[2] + "\t"
                                                              + item.ElementCrossSectionId + "\t" +
                                                              item.ElementMaterialId));
            }

            if (alleElemente3D8.Count != 0)
            {
                found.Add("\nElement3D8");
                found.AddRange(alleElemente3D8.Select(item => item.ElementId + "\t" + item.NodeIds[0] + "\t"
                                                              + item.NodeIds[1] + "\t" + item.NodeIds[2] + "\t" +
                                                              item.NodeIds[3] + "\t"
                                                              + item.NodeIds[4] + "\t" + item.NodeIds[5] + "\t" +
                                                              item.NodeIds[6] + "\t"
                                                              + item.NodeIds[7] + "\t" + item.ElementMaterialId));
            }

            if (allCrossSections.Count != 0)
            {
                found.Add("\nCrossSection");
                found.AddRange(allCrossSections.Select(item => item.CrossSectionId + "\t"
                    + item.CrossSectionValues[0]));
            }

            // Materialien
            found.Add("\n" + "Material");
            var sb = new StringBuilder();
            foreach (var item in model.Material)
            {
                sb.Clear();
                sb.Append(item.Value.MaterialId + "\t" + item.Value.MaterialValues[0]);
                for (var i = 1; i < item.Value.MaterialValues.Length; i++) sb.Append("\t" + item.Value.MaterialValues[i]);
                found.Add(sb.ToString());
            }

            // Lasten
            if (model.Loads.Count > 0) found.Add("\nNodeLoads");
            foreach (var item in model.Loads)
            {
                sb.Clear();
                sb.Append(item.Value.LoadId + "\t" + item.Value.NodeId + "\t" + item.Value.Intensity[0]);
                for (var i = 1; i < item.Value.Intensity.Length; i++) sb.Append("\t" + item.Value.Intensity[i]);
                found.Add(sb.ToString());
            }

            if (model.LineLoads.Count > 0) found.Add("\nLineLoads");
            foreach (var item in model.LineLoads)
                found.Add(item.Value.LoadId + "\t" + item.Value.StartNodeId
                          + "\t" + item.Value.Intensity[0] + "\t" + item.Value.Intensity[1]
                          + "\t" + item.Value.EndNodeId
                          + "\t" + item.Value.Intensity[2] + "\t" + item.Value.Intensity[3]);

            // Randbedingungen
            var fest = string.Empty;
            found.Add("\nBoundaryConditions");
            foreach (var item in model.BoundaryConditions)
            {
                sb.Clear();
                if (model.SpatialDimension == 2)
                {
                    if (item.Value.Type == 1) fest = "x";
                    else if (item.Value.Type == 2) fest = "y";
                    else if (item.Value.Type == 3) fest = "xy";
                    else if (item.Value.Type == 7) fest = "xyr";
                }
                else if (model.SpatialDimension == 3)
                {
                    if (item.Value.Type == 1) fest = "x";
                    else if (item.Value.Type == 2) fest = "y";
                    else if (item.Value.Type == 3) fest = "xy";
                    else if (item.Value.Type == 4) fest = "z";
                    else if (item.Value.Type == 5) fest = "xz";
                    else if (item.Value.Type == 6) fest = "yz";
                    else if (item.Value.Type == 7) fest = "xyz";
                }

                sb.Append(item.Key + "\t" + item.Value.NodeId + "\t" + fest);
                foreach (var wert in item.Value.Prescribed) sb.Append("\t" + wert);
                found.Add(sb.ToString());
            }

            // Dateiende
            found.Add("\nend");

            // alle Zeilen in Datei schreiben
            var dateiName = "\\" + name + ".inp";
            path = fileDialog.InitialDirectory + dateiName;
            File.WriteAllLines(path, found);
        }
        private void ElasticityDataVisualize(object sender, RoutedEventArgs e)
        {
            if (model == null)
            {
                _ = MessageBox.Show("Modelldaten sind noch nicht spezifiziert", "Elastizitätsberechnung");
                return;
            }

            switch (model.SpatialDimension)
            {
                case 2:
                    {
                        var tragwerk = new Elasticity.ModelDataShow.ElasticityModelVisualize(model);
                        tragwerk.Show();
                        break;
                    }
                case 3:
                    {
                        var tragwerk = new Elasticity.ModelDataShow.ElasticityModel3DVisualize(model);
                        tragwerk.Show();
                        break;
                    }
            }
        }

        private void ElasticityDataAnalyze(object sender, EventArgs e)
        {
            if (model == null)
            {
                _ = MessageBox.Show("Model data for elasticity analysis not yet specified",
                    "Elasticity Analysis");
                return;
            }

            try
            {
                modelAnalysis = new Analysis(model);
                modelAnalysis.ComputeSystemMatrix();
                modelAnalysis.ComputeSystemVector();
                modelAnalysis.SolveEquations();
                analysed = true;

                _ = MessageBox.Show("Systemgleichungen erfolgreich gelöst", "Elasticity Analysis");
            }

            catch (AnalysisException)
            {
                throw new AnalysisException("Exit: Error when solving system equations");
            }
        }
        private void ElasticityAnalysisResults(object sender, EventArgs e)
        {
            if (!analysed)
            {
                if (model == null)
                {
                    _ = MessageBox.Show("Model data for Elasticity Analysis not yet specified",
                        "Elasticity Analysis");
                    return;
                }

                modelAnalysis = new Analysis(model);
                modelAnalysis.ComputeSystemMatrix();
                modelAnalysis.ComputeSystemVector();
                modelAnalysis.SolveEquations();
                analysed = true;
            }

            var results = new Elasticity.Results.StaticResultsShow(model);
            results.Show();
        }
        private void ElasticityResultsVisualize(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            if (!analysed)
            {
                if (model == null)
                {
                    _ = MessageBox.Show("Model data for Elasticity Analysis not yet specified",
                        "Elasticity Analysis");
                    return;
                }

                modelAnalysis = new Analysis(model);
                modelAnalysis.ComputeSystemMatrix();
                modelAnalysis.ComputeSystemVector();
                modelAnalysis.SolveEquations();
                analysed = true;
            }

            if (model.SpatialDimension == 2)
            {
                var physicalStructure = new Elasticity.Results.StaticResultsVisualize(model);
                physicalStructure.Show();
            }
            else if (model.SpatialDimension == 3)
            {
                var physicalStructure = new Elasticity.Results.StaticResults3DVisualize(model);
                physicalStructure.Show();
            }
            else
            {
                _ = MessageBox.Show(sb.ToString(), "wrong spatial dimension, must be 2 or 3");
            }
        }
    }
}