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
        public static Analysis modelAnalysis;
        private OpenFileDialog fileDialog;
        private string path;
        private FeModel structuresModel;
        public static Structural_Analysis.ModelDataShow.StructuralModelVisualize structuresModelVisual;
        public static Structural_Analysis.Results.StaticResultsVisualize staticResults;
        private FeModel heatModel;
        public static Heat_Transfer.ModelDataShow.HeatDataVisualize heatModelVisual;
        public static Heat_Transfer.Results.StationaryResultsVisualize stationaryResults;
        private FeModel elasticityModel;

        private string[] lines;
        public static bool heatData, structuresData, timeintegrationData;
        public static bool analysed, eigenAnalysed, timeintegrationAnalysed;

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
            heatModel = parse.FeModel;
            parse.ParseNodes(lines);

            var heatElements = new Heat_Transfer.ModelDataRead.ElementParser();
            heatElements.ParseElements(lines, heatModel);

            var heatMaterial = new Heat_Transfer.ModelDataRead.MaterialParser();
            heatMaterial.ParseMaterials(lines, heatModel);

            var heatLoads = new Heat_Transfer.ModelDataRead.LoadParser();
            heatLoads.ParseLoads(lines, heatModel);

            var heatBoundaryCondition = new Heat_Transfer.ModelDataRead.BoundaryConditionParser();
            heatBoundaryCondition.ParseBoundaryConditions(lines, heatModel);

            var heatTransient = new Heat_Transfer.ModelDataRead.TransientParser();
            heatTransient.ParseTimeIntegration(lines, heatModel);

            timeintegrationData = heatTransient.timeIntegrationData;
            heatData = true;
            analysed = false;
            timeintegrationAnalysed = false;

            sb.Append(FeParser.InputFound + "\nHeat Model input data successfully read");
            _ = MessageBox.Show(sb.ToString(), "Heat Transfer Analysis");
            sb.Clear();

            heatModelVisual = new Heat_Transfer.ModelDataShow.HeatDataVisualize(heatModel);
            heatModelVisual.Show();
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

            if (heatModel == null)
            {
                _ = MessageBox.Show("Model has not yet been defined", "Heat Transfer Analysis");
                return;
            }

            var rows = new List<string>
            {
                "ModelName",
                heatModel.ModelId,
                "\nSpace dimension"
            };
            var numberNodalDof = 1;
            rows.Add(heatModel.SpatialDimension + "\t" + numberNodalDof + "\n");

            // Nodes
            rows.Add("Node");
            if (heatModel.SpatialDimension == 2)
                rows.AddRange(heatModel.Nodes.Select(knoten => knoten.Key
                                                               + "\t" + knoten.Value.Coordinates[0] + "\t" +
                                                               knoten.Value.Coordinates[1]));
            else
                rows.AddRange(heatModel.Nodes.Select(knoten => knoten.Key
                                                               + "\t" + knoten.Value.Coordinates[0] + "\t" +
                                                               knoten.Value.Coordinates[1] + "\t" +
                                                               knoten.Value.Coordinates[2]));

            // Elements
            var allElements2D2 = new List<Element2D2>();
            var allElements2D3 = new List<Element2D3>();
            var allElements2D4 = new List<Element2D4>();
            var allElements3D8 = new List<Element3D8>();
            foreach (var item in heatModel.Elements)
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
            foreach (var item in heatModel.Material)
            {
                sb.Clear();
                sb.Append(item.Value.MaterialId + "\t" + item.Value.MaterialValues[0]);
                for (var i = 1; i < item.Value.MaterialValues.Length; i++)
                    sb.Append("\t" + item.Value.MaterialValues[i]);
                rows.Add(sb.ToString());
            }

            // Loads
            foreach (var item in heatModel.Loads)
            {
                sb.Clear();
                sb.Append("\n" + "NodeLoads");
                sb.Append(item.Value.LoadId + "\t" + item.Value.Loadvalues[0]);
                for (var i = 1; i < item.Value.Loadvalues.Length; i++) sb.Append("\t" + item.Value.Loadvalues[i]);
                rows.Add(sb.ToString());
            }

            foreach (var item in heatModel.LineLoads)
            {
                sb.Clear();
                sb.Append("\n" + "LineLoads");
                sb.Append(item.Value.LoadId + "\t" + item.Value.StartNodeId + "\t" + item.Value.EndNodeId + "\t"
                          + item.Value.Loadvalues[0] + "\t" + item.Value.Loadvalues[1]);
                rows.Add(sb.ToString());
            }

            var allElementLoads3 = new List<ElementLoad3>();
            var allElementLoads4 = new List<ElementLoad4>();
            foreach (var item in heatModel.ElementLoads)
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
                                                              + item.Loadvalues[0] + "\t" + item.Loadvalues[1] +
                                                              "\t" + item.Loadvalues[2]));
            }

            if (allElementLoads4.Count != 0)
            {
                rows.Add("\n" + "ElementLoad4");
                rows.AddRange(allElementLoads4.Select(item => item.LoadId + "\t" + item.ElementId + "\t"
                                                              + item.Loadvalues[0] + "\t" + item.Loadvalues[1]
                                                              + "\t" + item.Loadvalues[2] + "\t" +
                                                              item.Loadvalues[3]));
            }

            // Boundary conditions
            rows.Add("\n" + "BoundaryConditions");
            foreach (var item in heatModel.BoundaryConditions)
            {
                sb.Clear();
                sb.Append(item.Value.SupportId + "\t" + item.Value.NodeId + "\t" + item.Value.Prescribed[0]);
                rows.Add(sb.ToString());
            }

            // Eigensolutions
            if (heatModel.Eigenstate != null)
            {
                rows.Add("\n" + "Eigensolutions");
                rows.Add(heatModel.Eigenstate.Id + "\t" + heatModel.Eigenstate.NumberOfStates);
            }

            // Parameter
            if (heatModel.Timeintegration != null)
            {
                rows.Add("\n" + "TimeIntegration");
                rows.Add(heatModel.Timeintegration.Id + "\t" + heatModel.Timeintegration.Tmax + "\t" +
                         heatModel.Timeintegration.Dt
                         + "\t" + heatModel.Timeintegration.Parameter1);
            }

            // time dependent initial conditions
            if (heatModel.Timeintegration.FromStationary || heatModel.Timeintegration.InitialConditions.Count != 0)
                rows.Add("\n" + "InitialTemperatures");
            if (heatModel.Timeintegration.FromStationary) rows.Add("stationary solution");

            foreach (var item in heatModel.Timeintegration.InitialConditions)
            {
                var knotenwerte = (NodalValues)item;
                rows.Add(knotenwerte.NodeId + "\t" + knotenwerte.Values[0]);
            }

            // time dependent boundary conditions 
            if (heatModel.TimeDependentBoundaryConditions.Count != 0)
                rows.Add("\n" + "Time Dependent Boundary Temperatures");
            foreach (var item in heatModel.TimeDependentBoundaryConditions)
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
            if (heatModel.TimeDependentNodeLoads.Count != 0) rows.Add("\n" + "TimeDependent Node Loads");
            foreach (var item in heatModel.TimeDependentNodeLoads)
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
            if (heatModel.TimeDependentElementLoads.Count != 0) rows.Add("\n" + "time dependent ElementTemperatures");
            foreach (var item in heatModel.TimeDependentElementLoads)
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
            if (heatModel == null)
            {
                var heat = new Heat_Transfer.ModelDataShow.HeatDataShow(heatModel);
                heat.Show();
            }
            else
            {
                _ = MessageBox.Show("Heat Model Data must be defined first", "Heat Transfer Analysis");
            }
        }
        private void HeatDataVisualize(object sender, RoutedEventArgs e)
        {
            if (heatModel == null)
            {
                heatModelVisual = new Heat_Transfer.ModelDataShow.HeatDataVisualize(heatModel);
                heatModelVisual.Show();
            }
            else
            {
                _ = MessageBox.Show("Heat Model Data must be defined first", "Heat Transfer Analysis");
            }
        }
        private void HeatDataAnalyse(object sender, EventArgs e)
        {
            if (heatData && heatModel!= null)
            {
                modelAnalysis = new Analysis(heatModel);
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
            if (heatData && heatModel != null)
            {
                if (!analysed)
                {
                    modelAnalysis = new Analysis(heatModel);
                    modelAnalysis.ComputeSystemMatrix();
                    modelAnalysis.ComputeSystemVector();
                    modelAnalysis.SolveEquations();
                    analysed = true;
                }

                var results = new Heat_Transfer.Results.StationaryResultsShow(heatModel);
                results.Show();
            }
            else
            {
                _ = MessageBox.Show("Model data for Heat Transfer Analysis not yet defined", "Heat Transfer Analysis");
            }
        }
        private void HeatTransferAnalysisResultsVisualize(object sender, RoutedEventArgs e)
        {
            if (heatData && heatModel != null)
            {
                if (!analysed)
                {
                    modelAnalysis = new Analysis(heatModel);
                    modelAnalysis.ComputeSystemMatrix();
                    modelAnalysis.ComputeSystemVector();
                    modelAnalysis.SolveEquations();
                    analysed = true;
                }

                stationaryResults = new Heat_Transfer.Results.StationaryResultsVisualize(heatModel);
                stationaryResults.Show();
            }
            else
            {
                _ = MessageBox.Show("Model Data for Heat Transfer Analysis not defined yet", "Heat Transfer Analysis");
            }
        }
        private void InstationaryData(object sender, RoutedEventArgs e)
        {
            if (heatModel == null)
            {
                _ = MessageBox.Show("Model Data for instationary Heat Transfer Analysis not defined yet", "Heat Transfer Analysis");
            }
            else
            {
                var heat = new Heat_Transfer.ModelDataShow.InstationaryDataShow(heatModel);
                heat.Show();
                timeintegrationAnalysed = false;
            }
        }
        private void HeatExcitationVisualize(object sender, RoutedEventArgs e)
        {
            if (heatModel == null) return;
            modelAnalysis ??= new Analysis(heatModel);
            var excitation = new Heat_Transfer.ModelDataShow.HeatExcitationVisualize(heatModel);
            excitation.Show();
        }
        private void EigensolutionHeatAnalyse(object sender, RoutedEventArgs e)
        {
            if (heatModel != null)
            {
                modelAnalysis ??= new Analysis(heatModel);
                if (!analysed)
                {
                    modelAnalysis.ComputeSystemMatrix();
                    analysed = true;
                }
                // default = 2 Eigenstates, if not specified otherwise
                heatModel.Eigenstate ??= new Eigenstates("default", 2);
                if (heatModel.Eigenstate.Eigenvalues != null) return;
                modelAnalysis.Eigenstates();
                eigenAnalysed = true;
                _ = MessageBox.Show("Eigensolutions successfully analysed", "Heat Transfer Analysis");
            }
            else
            {
                _ = MessageBox.Show("Heat Model data not defined yet", "Heat Transfer Analysis");
            }
        }
        private void EigensolutionHeatShow(object sender, RoutedEventArgs e)
        {
            if (heatModel != null)
            {
                modelAnalysis ??= new Analysis(heatModel);
                if (!analysed)
                {
                    modelAnalysis.ComputeSystemMatrix();
                    analysed = true;
                }

                // default = 2 Eigenstates, if not specified otherwise
                heatModel.Eigenstate ??= new Eigenstates("default", 2);
                if (heatModel.Eigenstate.Eigenvalues == null) modelAnalysis.Eigenstates();
                var eigen = new Heat_Transfer.Results.EigensolutionsShow(heatModel);
                eigen.Show();
            }
            else
            {
                _ = MessageBox.Show("Heat Transfer Model data not defined yet", "Heat Transfer Analysis");
            }
        }
        private void EigensolutionHeatVisualize(object sender, RoutedEventArgs e)
        {
            if (heatModel != null)
            {
                modelAnalysis ??= new Analysis(heatModel);
                if (!timeintegrationAnalysed)
                {
                    modelAnalysis.ComputeSystemMatrix();
                    // default = 2 Eigenstates, if not specified otherwise
                    heatModel.Eigenstate ??= new Eigenstates("default", 2);
                }
                // default = 2 Eigenstates, if not specified otherwise
                heatModel.Eigenstate ??= new Eigenstates("default", 2);
                if (heatModel.Eigenstate.Eigenvalues == null) modelAnalysis.Eigenstates();
                var visual = new Heat_Transfer.Results.EigensolutionVisualize(heatModel);
                visual.Show();
            }
            else
            {
                _ = MessageBox.Show("Heat Transfer Model data not defined yet", "Heat Transfer Analysis");
            }
        }
        private void InstationaryHeatTransferAnalysis(object sender, RoutedEventArgs e)
        {
            if (timeintegrationData && heatModel != null)
            {
                if (!analysed)
                {
                    modelAnalysis = new Analysis(heatModel);
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
                const double tmax = 0;
                const double dt = 0;
                const double alfa = 0;
                if (heatModel != null)
                {
                    heatModel.Timeintegration = new Heat_Transfer.Model_Data.TimeIntegration(tmax, dt, alfa) { FromStationary = false };
                    timeintegrationData = true;
                    var heat = new Heat_Transfer.ModelDataShow.InstationaryDataShow(heatModel);
                    heat.Show();
                }

                timeintegrationAnalysed = false;
            }
        }
        private void InstationaryHeatTransferAnalysisResultsShow(object sender, RoutedEventArgs e)
        {
            if (timeintegrationAnalysed && heatModel != null)
                _ = new Heat_Transfer.Results.InstationaryResultsShow(heatModel);
            else
                _ = MessageBox.Show("Time Integration not yet completed!!", "instationary Heat Transfer Analysis");
        }
        private void InstationaryModelStatesVisualize(object sender, RoutedEventArgs e)
        {
            if (timeintegrationAnalysed && heatModel != null)
            {
                var wärmeModell = new Heat_Transfer.Results.InstationaryModelStatesVisualize(heatModel);
                wärmeModell.Show();
            }
            else
            {
                _ = MessageBox.Show("Time Integration not yet completed!!", "instationary Heat Transfer Analysis");
            }
        }
        private void NodalTimeHistoryVisualize(object sender, RoutedEventArgs e)
        {
            if (timeintegrationAnalysed && heatModel != null)
            {
                var nodalTimeHistoriesVisualize = new Heat_Transfer.Results.NodalTimeHistoriesVisualize(heatModel);
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
            structuresModel = parse.FeModel;
            parse.ParseNodes(lines);

            var structuralElements = new Structural_Analysis.ModelDataRead.ElementParser();
            structuralElements.ParseElements(lines, structuresModel);

            var tragwerksMaterial = new Structural_Analysis.ModelDataRead.MaterialParser();
            tragwerksMaterial.ParseMaterials(lines, structuresModel);

            var tragwerksLasten = new Structural_Analysis.ModelDataRead.LoadParser();
            tragwerksLasten.ParseLoads(lines, structuresModel);

            var tragwerksRandbedingungen = new Structural_Analysis.ModelDataRead.BoundaryConditionParser();
            tragwerksRandbedingungen.ParseBoundaryConditions(lines, structuresModel);

            var structureTransient = new Structural_Analysis.ModelDataRead.TransientParser();
            structureTransient.ParseTimeIntegration(lines, structuresModel);

            timeintegrationData = structureTransient.timeIntegrationData;
            structuresData = true;
            analysed = false;
            timeintegrationAnalysed = false;

            sb.Append(FeParser.InputFound + "\nStructural Model data successfully read");
            _ = MessageBox.Show(sb.ToString(), "Structural Analysis");
            sb.Clear();

            structuresModelVisual = new Structural_Analysis.ModelDataShow.StructuralModelVisualize(structuresModel);
            structuresModelVisual.Show();
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
                structuresModel.ModelId,
                "\nSpatial Dimension",
                structuresModel.SpatialDimension + "\t" + structuresModel.NumberNodalDof,
                // Nodes
                "\nNodes"
            };

            switch (structuresModel.SpatialDimension)
            {
                case 1:
                    rows.AddRange(structuresModel.Nodes.Select(node => node.Key
                                                   + "\t" + node.Value.Coordinates[0]));
                    break;
                case 2:
                    rows.AddRange(structuresModel.Nodes.Select(node => node.Key
                                                   + "\t" + node.Value.Coordinates[0]
                                                   + "\t" + node.Value.Coordinates[1]));
                    break;
                case 3:
                    rows.AddRange(structuresModel.Nodes.Select(node => node.Key
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
            foreach (var item in structuresModel.Elements)
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

            var allCrossSections = structuresModel.CrossSection.Select(item => item.Value).ToList();

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
            foreach (var item in structuresModel.Material)
            {
                sb.Clear();
                sb.Append(item.Value.MaterialId + "\t" + item.Value.MaterialValues[0]);
                for (var i = 1; i < item.Value.MaterialValues.Length; i++) sb.Append("\t" + item.Value.MaterialValues[i]);
                rows.Add(sb.ToString());
            }

            // Loads
            foreach (var item in structuresModel.Loads)
            {
                rows.Add("\nNodeLoad");
                sb.Clear();
                sb.Append(item.Value.LoadId + "\t" + item.Value.NodeId + "\t" + item.Value.Loadvalues[0]);
                for (var i = 1; i < item.Value.Loadvalues.Length; i++) sb.Append("\t" + item.Value.Loadvalues[i]);
                rows.Add(sb.ToString());
            }

            foreach (var item in structuresModel.PointLoads)
            {
                var pointLoad = (PointLoad)item.Value;
                sb.Clear();
                rows.Add("\nPointLoad");
                rows.Add(pointLoad.LoadId + "\t" + pointLoad.ElementId
                           + "\t" + pointLoad.Loadvalues[0] + "\t" + pointLoad.Loadvalues[1] + "\t" + pointLoad.Offset);
            }

            foreach (var item in structuresModel.ElementLoads)
            {
                sb.Clear();
                rows.Add("\nLineLoad");
                rows.Add(item.Value.LoadId + "\t" + item.Value.ElementId
                           + "\t" + item.Value.Loadvalues[0] + "\t" + item.Value.Loadvalues[1]
                           + "\t" + item.Value.Loadvalues[2] + "\t" + item.Value.Loadvalues[3]
                           + "\t" + item.Value.InElementCoordinateSystem);
            }

            // Boundary Conditions
            var fix = string.Empty;
            rows.Add("\nSupport");
            foreach (var item in structuresModel.BoundaryConditions)
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
            if (structuresData && structuresModel != null)
            {
                var tragwerk = new Structural_Analysis.ModelDataShow.StructuralModelDataShow(structuresModel);
                tragwerk.Show();
            }
            else
            {
                _ = MessageBox.Show("Structural Model Data must be defined first", "static Structural Analysis");
            }
        }
        private void StructuralModelDataVisualize(object sender, RoutedEventArgs e)
        {
            if (structuresData && structuresModel != null)
            {
                structuresModelVisual = new Structural_Analysis.ModelDataShow.StructuralModelVisualize(structuresModel);
                structuresModelVisual.Show();
            }
            else
            {
                _ = MessageBox.Show("Structural Model Data must be defined first", "static Structural Analysis");
            }
        }
        private void StructuralModelStaticAnalysis(object sender, EventArgs e)
        {
            if (structuresData && structuresModel != null)
            {
                modelAnalysis = new Analysis(structuresModel);
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
            if (!structuresData && structuresModel != null)
            {
                if (!analysed)
                {
                    modelAnalysis = new Analysis(structuresModel);
                    modelAnalysis.ComputeSystemMatrix();
                    modelAnalysis.ComputeSystemVector();
                    modelAnalysis.SolveEquations();
                    analysed = true;
                }

                var results = new Structural_Analysis.Results.StaticResultsShow(structuresModel);
                results.Show();
            }
            else
            {
                _ = MessageBox.Show("Structural Model Data must be defined first", "static Structural Analysis");
            }
        }
        private void StructuralModelStaticResultsVisualize(object sender, RoutedEventArgs e)
        {
            if (structuresData && structuresModel != null)
            {
                if (!analysed)
                {
                    modelAnalysis = new Analysis(structuresModel);
                    modelAnalysis.ComputeSystemMatrix();
                    modelAnalysis.ComputeSystemVector();
                    modelAnalysis.SolveEquations();
                    analysed = true;
                }
                staticResults = new Structural_Analysis.Results.StaticResultsVisualize(structuresModel);
                staticResults.Show();
            }
            else
            {
                _ = MessageBox.Show("Structural Model Data must be defined first", "static Structural Analysis");
            }
        }
        private void StructuralModelEigensolutionAnalysis(object sender, RoutedEventArgs e)
        {
            if (structuresModel != null)
            {
                modelAnalysis ??= new Analysis(structuresModel);
                if (!analysed)
                {
                    modelAnalysis.ComputeSystemMatrix();
                    analysed = true;
                }
                // default = 2 Eigenstates, if not specified otherwise
                structuresModel.Eigenstate ??= new Eigenstates("default", 2);
                if (structuresModel.Eigenstate.Eigenvalues != null) return;
                modelAnalysis.Eigenstates();
                eigenAnalysed = true;
                _ = MessageBox.Show("Eigenfrequencies successfully analysed", "dynamic Structural Analysis");
            }
            else
            {
                _ = MessageBox.Show("Structural Model Data not defined yet", "dynamic Structural Analysis");
            }
        }
        private void StructuralModelEigensolutionShow(object sender, RoutedEventArgs e)
        {
            if (structuresModel != null)
            {
                modelAnalysis ??= new Analysis(structuresModel);
                if (!analysed)
                {
                    modelAnalysis.ComputeSystemMatrix();
                    analysed = true;
                }
                // default = 2 Eigenstates, if not specified otherwise
                structuresModel.Eigenstate ??= new Eigenstates("default", 2);
                if (structuresModel.Eigenstate.Eigenvalues == null) modelAnalysis.Eigenstates();
                var eigen = new Structural_Analysis.Results.EigensolutionShow(structuresModel);
                eigen.Show();
            }
            else
            {
                _ = MessageBox.Show("Structural Model data not specified yet", "dynamic Structural Analysis");
            }
        }
        private void StructuralModelEigensolutionVisualize(object sender, RoutedEventArgs e)
        {
            if (structuresModel != null)
            {
                modelAnalysis ??= new Analysis(structuresModel);
                if (!analysed)
                {
                    modelAnalysis.ComputeSystemMatrix();
                    analysed = true;
                }
                // default = 2 Eigenstates, falls nicht anders spezifiziert
                structuresModel.Eigenstate ??= new Eigenstates("default", 2);
                if (structuresModel.Eigenstate.Eigenvalues == null) modelAnalysis.Eigenstates();
                var visual = new Structural_Analysis.Results.EigensolutionVisualize(structuresModel);
                visual.Show();
            }
            else
            {
                _ = MessageBox.Show("Structural Model data not defined yet", "dynamic Structural Analysis");
            }
        }
        private void StructuralModelDynamicDataShow(object sender, EventArgs e)
        {
            if (timeintegrationData && structuresModel != null)
            {
                var tragwerk = new Structural_Analysis.ModelDataShow.DynamicModelDataShow(structuresModel);
                tragwerk.Show();
            }
            else
            {
                _ = MessageBox.Show("Model Data for time history analysis have not been specified yet", "dynamic Structural Analysis");
            }
        }
        private void ExcitationVisualize(object sender, RoutedEventArgs e)
        {
            if (timeintegrationData && structuresModel != null)
            {
                modelAnalysis ??= new Analysis(structuresModel);
                var excitation = new Structural_Analysis.ModelDataShow.ExcitationVisualize(structuresModel);
                excitation.Show();
            }
            else
            {
                _ = MessageBox.Show("Model Data for time history analysis have not been specified yet", "dynamic Structural Analysis");
            }
        }

        private void StructuralModelDynamicAnalysis(object sender, EventArgs e)
        {
            if (timeintegrationData && structuresModel != null)
            {
                if (!analysed)
                {
                    modelAnalysis = new Analysis(structuresModel);
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
            if (timeintegrationAnalysed && structuresModel != null)
                _ = new Structural_Analysis.Results.DynamicResultsShow(structuresModel);
            else
                _ = MessageBox.Show("Time Integration has not yet been completed!!", "dynamic Structural Analysis");
        }
        private void StructuralModelDynamicModelStatesVisualize(object sender, RoutedEventArgs e)
        {
            if (timeintegrationAnalysed && structuresModel != null)
            {
                var dynamicResults = new Structural_Analysis.Results.DynamicModelStatesVisualize(structuresModel);
                dynamicResults.Show();
            }
            else
            {
                _ = MessageBox.Show("Time Integration has not yet been completed!!", "dynamic Structural Analysis");
            }
        }
        private void StructuralModelNodalTimeHistoriesVisualize(object sender, RoutedEventArgs e)
        {
            if (timeintegrationAnalysed && structuresModel != null)
            {
                var nodalTimeHistories = new Structural_Analysis.Results.NodalTimeHistoriesVisualize(structuresModel);
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
            elasticityModel = parse.FeModel;
            parse.ParseNodes(lines);

            var parseElasticity = new Elasticity.ModelDataRead.ElasticityParser();
            parseElasticity.ParseElasticity(lines, elasticityModel);

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
            if (elasticityModel == null)
            {
                _ = MessageBox.Show("Model data not yet specified", "Elasticity Analysis");
                return;
            }

            var structure = new Elasticity.ModelDataShow.ElasticityDataShow(elasticityModel);
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
                elasticityModel.ModelId,
                "\nSpatial Dimension"
            };
            var nodalDof = 3;
            found.Add(elasticityModel.SpatialDimension + "\t" + nodalDof);

            // Node
            found.Add("\nNodes");
            if (elasticityModel.SpatialDimension == 2)
                found.AddRange(elasticityModel.Nodes.Select(knoten => knoten.Key
                                                                      + "\t" + knoten.Value.Coordinates[0] + "\t" +
                                                                      knoten.Value.Coordinates[1]));
            else
                found.AddRange(elasticityModel.Nodes.Select(knoten => knoten.Key
                                                                      + "\t" + knoten.Value.Coordinates[0] + "\t" +
                                                                      knoten.Value.Coordinates[1] + "\t" +
                                                                      knoten.Value.Coordinates[2]));

            // Elements
            var alleElemente2D3 = new List<Elasticity.ModelData.Element2D3>();
            var alleElemente3D8 = new List<Elasticity.ModelData.Element3D8>();
            var allCrossSections = new List<CrossSection>();
            foreach (var item in elasticityModel.Elements)
                switch (item.Value)
                {
                    case Elasticity.ModelData.Element2D3 element2D3:
                        alleElemente2D3.Add(element2D3);
                        break;
                    case Elasticity.ModelData.Element3D8 element3D8:
                        alleElemente3D8.Add(element3D8);
                        break;
                }

            foreach (var item in elasticityModel.CrossSection) allCrossSections.Add(item.Value);

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
            foreach (var item in elasticityModel.Material)
            {
                sb.Clear();
                sb.Append(item.Value.MaterialId + "\t" + item.Value.MaterialValues[0]);
                for (var i = 1; i < item.Value.MaterialValues.Length; i++) sb.Append("\t" + item.Value.MaterialValues[i]);
                found.Add(sb.ToString());
            }

            // Lasten
            if (elasticityModel.Loads.Count > 0) found.Add("\nNodeLoads");
            foreach (var item in elasticityModel.Loads)
            {
                sb.Clear();
                sb.Append(item.Value.LoadId + "\t" + item.Value.NodeId + "\t" + item.Value.Loadvalues[0]);
                for (var i = 1; i < item.Value.Loadvalues.Length; i++) sb.Append("\t" + item.Value.Loadvalues[i]);
                found.Add(sb.ToString());
            }

            if (elasticityModel.LineLoads.Count > 0) found.Add("\nLineLoads");
            foreach (var item in elasticityModel.LineLoads)
                found.Add(item.Value.LoadId + "\t" + item.Value.StartNodeId
                          + "\t" + item.Value.Loadvalues[0] + "\t" + item.Value.Loadvalues[1]
                          + "\t" + item.Value.EndNodeId
                          + "\t" + item.Value.Loadvalues[2] + "\t" + item.Value.Loadvalues[3]);

            // Randbedingungen
            var fest = string.Empty;
            found.Add("\nBoundaryConditions");
            foreach (var item in elasticityModel.BoundaryConditions)
            {
                sb.Clear();
                if (elasticityModel.SpatialDimension == 2)
                {
                    if (item.Value.Type == 1) fest = "x";
                    else if (item.Value.Type == 2) fest = "y";
                    else if (item.Value.Type == 3) fest = "xy";
                    else if (item.Value.Type == 7) fest = "xyr";
                }
                else if (elasticityModel.SpatialDimension == 3)
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
            if (elasticityModel == null)
            {
                _ = MessageBox.Show("Modelldaten sind noch nicht spezifiziert", "Elastizitätsberechnung");
                return;
            }

            switch (elasticityModel.SpatialDimension)
            {
                case 2:
                    {
                        var tragwerk = new Elasticity.ModelDataShow.ElasticityModelVisualize(elasticityModel);
                        tragwerk.Show();
                        break;
                    }
                case 3:
                    {
                        var tragwerk = new Elasticity.ModelDataShow.ElasticityModel3DVisualize(elasticityModel);
                        tragwerk.Show();
                        break;
                    }
            }
        }

        private void ElasticityDataAnalyze(object sender, EventArgs e)
        {
            if (elasticityModel == null)
            {
                _ = MessageBox.Show("Model data for elasticity analysis not yet specified",
                    "Elasticity Analysis");
                return;
            }

            try
            {
                modelAnalysis = new Analysis(elasticityModel);
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
                if (elasticityModel == null)
                {
                    _ = MessageBox.Show("Model data for Elasticity Analysis not yet specified",
                        "Elasticity Analysis");
                    return;
                }

                modelAnalysis = new Analysis(elasticityModel);
                modelAnalysis.ComputeSystemMatrix();
                modelAnalysis.ComputeSystemVector();
                modelAnalysis.SolveEquations();
                analysed = true;
            }

            var results = new Elasticity.Results.StaticResultsShow(elasticityModel);
            results.Show();
        }
        private void ElasticityResultsVisualize(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            if (!analysed)
            {
                if (elasticityModel == null)
                {
                    _ = MessageBox.Show("Model data for Elasticity Analysis not yet specified",
                        "Elasticity Analysis");
                    return;
                }

                modelAnalysis = new Analysis(elasticityModel);
                modelAnalysis.ComputeSystemMatrix();
                modelAnalysis.ComputeSystemVector();
                modelAnalysis.SolveEquations();
                analysed = true;
            }

            if (elasticityModel.SpatialDimension == 2)
            {
                var physicalStructure = new Elasticity.Results.StaticResultsVisualize(elasticityModel);
                physicalStructure.Show();
            }
            else if (elasticityModel.SpatialDimension == 3)
            {
                var physicalStructure = new Elasticity.Results.StaticResults3DVisualize(elasticityModel);
                physicalStructure.Show();
            }
            else
            {
                _ = MessageBox.Show(sb.ToString(), "wrong spatial dimension, must be 2 or 3");
            }
        }
    }
}