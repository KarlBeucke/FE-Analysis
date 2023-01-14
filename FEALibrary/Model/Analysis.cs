using FEALibrary.DynamicSolver;
using FEALibrary.EquationSolver;
using FEALibrary.Model.abstractClasses;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;

namespace FEALibrary.Model
{
    public class Analysis
    {
        private FeModel model;
        private Node node;
        private AbstractElement element;
        private Equations systemEquations;
        private ProfileSolverStatus profileSolver;
        private int dimension;
        private bool decomposed, setDimension, profile, diagonalMatrix;

        public Analysis(FeModel m)
        {
            model = m;
            if (model == null) throw new AnalysisException("Model input data not yet defined");
            // set System Indices
            var k = 0;
            foreach (var item in model.Nodes)
            {
                node = item.Value;
                k = node.SetSystemIndices(k);
            }
            SetReferences(m);
        }

        // object references are initially established only on the basis of unique identifiers, i.e. before object instantiation ****
        // when analyis is started, object references must be established using the unique identifiers ******************************
        private void SetReferences(FeModel m)
        {
            model = m;

            // set references for CrossSection values of 2D Elements
            foreach (var abstractElement in
                     from KeyValuePair<string, AbstractElement> item in model.Elements
                     where item.Value != null
                     where item.Value is Abstract2D
                     let element = item.Value
                     select element)
            {
                var element2D = (Abstract2D)abstractElement;
                element2D.SetCrossSectionReferences(model);
            }

            // set all necessary element references and system indices for all elements
            foreach (var abstractElement in model.Elements.Select(item => item.Value))
            {
                abstractElement.SetReferences(model);
                abstractElement.SetSystemIndicesOfElement();
            }

            foreach (var support in model.BoundaryConditions.Select(item => item.Value)) support.SetReferences(model);
            foreach (var load in model.ElementLoads.Select(item => item.Value)) load.SetReferences(model);
            foreach (var timeDepLoad in model.TimeDependentNodeLoads.Select(item => item.Value))
                timeDepLoad.SetReferences(model);
            foreach (var timeDepLoad in model.TimeDependentElementLoads.Select(item => item.Value))
                timeDepLoad.SetReferences(model);
            foreach (var timeDepBc in model.TimeDependentBoundaryConditions.Select(item => item.Value))
                timeDepBc.SetReferences(model);
        }
        // determine dimension of system matrix *************************************************************************************
        private void DetermineDimension()
        {
            dimension = 0;
            foreach (var item in model.Nodes) dimension += item.Value.NumberOfNodalDof;
            systemEquations = new Equations(dimension);
            setDimension = true;
        }
        // compute and solve system matrix in profile format with status vector *****************************************************
        private void SetProfile()
        {
            foreach (var item in model.Elements)
            {
                element = item.Value;
                systemEquations.SetProfile(element.SystemIndicesOfElement);
            }

            systemEquations.AllocateMatrix();
            profile = true;
        }
        public void ComputeSystemMatrix()
        {
            if (!setDimension) DetermineDimension();
            if (!profile) SetProfile();
            // traverse the elements to assemble the stiffness coefficients
            foreach (var item in model.Elements)
            {
                element = item.Value;
                var elementMatrix = element.ComputeMatrix();
                systemEquations.AddMatrix(element.SystemIndicesOfElement, elementMatrix);
            }

            SetStatusVector();
        }
        private void SetStatusVector()
        {
            // for all fixed Boundary Conditions
            foreach (var item in model.BoundaryConditions) StatusNodes(item.Value);
        }
        private void StatusNodes(AbstractBoundaryCondition boundaryCondition)
        {
            var nodeId = boundaryCondition.NodeId;
            //_ = new double[randbedingung.Node.NumberOfNodalDof];
            //_ = new bool[randbedingung.Node.NumberOfNodalDof];
            if (model.Nodes.TryGetValue(nodeId, out node))
            {
                systemEquations.SetProfile(node.SystemIndices);
                var prescribed = boundaryCondition.Prescribed;
                var restrained = boundaryCondition.Restrained;
                for (var i = 0; i < restrained.Length; i++)
                    if (restrained[i])
                        systemEquations.SetStatus(true, node.SystemIndices[i], prescribed[i]);
            }
            else
            {
                throw new AnalysisException("Boundary Condition Node " + nodeId + " is not found in Model.");
            }
        }
        private void ReComputeSystemMatrix()
        {
            // traverse the elements to assemble the stiffness coefficients
            systemEquations.InitializeMatrix();
            foreach (var item in model.Elements)
            {
                element = item.Value;
                var indices = element.SystemIndicesOfElement;
                var elementMatrix = element.ComputeMatrix();
                systemEquations.AddMatrix(indices, elementMatrix);
            }
        }
        public void ComputeSystemVector()
        {
            int[] indices;
            double[] loadVector;

            // Node Loads
            foreach (var item in model.Loads)
            {
                var nodeLoad = item.Value;
                var nodeId = item.Value.NodeId;
                if (model.Nodes.TryGetValue(nodeId, out var loadNode))
                {
                    var systemIndices = loadNode.SystemIndices;
                    indices = systemIndices;
                    loadVector = nodeLoad.ComputeLoadVector();
                    systemEquations.AddVector(indices, loadVector);
                }
                else
                {
                    throw new AnalysisException("Load Node " + nodeId + " is not found in Model.");
                }
            }

            // LineLoads
            foreach (var item in model.LineLoads)
            {
                var lineLoad = item.Value;
                var startNodeId = item.Value.StartNodeId;
                if (model.Nodes.TryGetValue(startNodeId, out node))
                    lineLoad.StartNode = node;
                else
                    throw new AnalysisException("LineLoad startNode " + startNodeId + " is not found in Model.");
                var endNodeId = item.Value.EndNodeId;
                if (model.Nodes.TryGetValue(endNodeId, out node))
                    lineLoad.EndNode = node;
                else
                    throw new AnalysisException("LineLoad endNode " + endNodeId + " is not found in Model.");
                var start = lineLoad.StartNode.SystemIndices.Length;
                var end = lineLoad.EndNode.SystemIndices.Length;
                indices = new int[start + end];
                for (var i = 0; i < start; i++)
                    indices[i] = lineLoad.StartNode.SystemIndices[i];
                for (var i = 0; i < end; i++)
                    indices[start + i] = lineLoad.EndNode.SystemIndices[i];
                loadVector = lineLoad.ComputeLoadVector();
                systemEquations.AddVector(indices, loadVector);
            }

            //ElementLoads
            foreach (var item in model.ElementLoads)
            {
                var elementLoad = item.Value;
                var elementId = item.Value.ElementId;
                if (model.Elements.TryGetValue(elementId, out element))
                {
                    indices = element.SystemIndicesOfElement;
                    loadVector = elementLoad.ComputeLoadVector();
                    systemEquations.AddVector(indices, loadVector);
                }
                else
                {
                    throw new AnalysisException("Element " + elementId +
                                                " for ElementLoad is not found in Model.");
                }
            }

            foreach (var item in model.PointLoads)
            {
                var pointLoad = item.Value;
                var elementId = item.Value.ElementId;
                if (model.Elements.TryGetValue(elementId, out element))
                {
                    pointLoad.Element = element;
                    indices = element.SystemIndicesOfElement;
                    loadVector = pointLoad.ComputeLoadVector();
                    systemEquations.AddVector(indices, loadVector);
                }
                else
                {
                    throw new AnalysisException("Element " + elementId +
                                                " for PointLoad is not found in Model.");
                }
            }
        }
        public void SolveEquations()
        {
            if (!decomposed)
            {
                profileSolver = new ProfileSolverStatus(
                    systemEquations.Matrix, systemEquations.Vector,
                    systemEquations.Primal, systemEquations.Dual,
                    systemEquations.Status, systemEquations.Profile);
                profileSolver.Decompose();
                decomposed = true;
            }

            profileSolver.Solve();
            // ... save system unknowns (primal values)
            foreach (var item in model.Nodes)
            {
                node = item.Value;
                var index = node.SystemIndices;
                node.NodalDof = new double[node.NumberOfNodalDof];
                for (var i = 0; i < node.NodalDof.Length; i++)
                    node.NodalDof[i] = systemEquations.Primal[index[i]];
            }

            // ... save dual values
            var reactions = systemEquations.Dual;
            foreach (var support in model.BoundaryConditions.Select(item => item.Value))
            {
                node = support.Node;
                var index = node.SystemIndices;
                var supportReaction = new double[node.NumberOfNodalDof];
                for (var i = 0; i < supportReaction.Length; i++)
                    supportReaction[i] = reactions[index[i]];
                node.Reactions = supportReaction;
            }

            model.Solved = true;
        }
        // Eigensolutions ***********************************************************************************************************
        public void Eigenstates()
        {
            var numberOfStates = model.Eigenstate.NumberOfStates;
            var aMatrix = systemEquations.Matrix;
            if (!diagonalMatrix) ComputeDiagonalMatrix();
            var bDiag = systemEquations.DiagonalMatrix;

            // general B-Matrix is expanded to the same structure as A
            var bMatrix = new double[dimension][];
            int row;
            for (row = 0; row < aMatrix.Length; row++)
            {
                bMatrix[row] = new double[aMatrix[row].Length];
                int col;
                for (col = 0; col < bMatrix[row].Length - 1; col++)
                    bMatrix[row][col] = 0;
                bMatrix[row][col] = bDiag[row];
            }

            if (!decomposed)
            {
                profileSolver = new ProfileSolverStatus(
                    systemEquations.Matrix,
                    systemEquations.Status, systemEquations.Profile);
                profileSolver.Decompose();
                decomposed = true;
            }

            var eigenSolver = new Eigensolver(systemEquations.Matrix, bMatrix,
                systemEquations.Profile, systemEquations.Status,
                numberOfStates);
            eigenSolver.SolveEigenstates();

            // save eigenvalues and eigenvectors
            var eigenvalues = new double[numberOfStates];
            var eigenvectors = new double[numberOfStates][];
            for (var i = 0; i < numberOfStates; i++)
            {
                eigenvalues[i] = eigenSolver.GetEigenValue(i);
                eigenvectors[i] = eigenSolver.GetEigenVector(i);
            }

            model.Eigenstate.Eigenvalues = eigenvalues;
            model.Eigenstate.Eigenvectors = eigenvectors;
            model.Eigenstate.Status = systemEquations.Status;
            model.Eigen = true;
        }
        private void ComputeDiagonalMatrix()
        {
            // diagonal specific heat resp. mass matrix
            if (!setDimension) DetermineDimension();

            // traverse the elements to assemble coefficients of the diagonal matrix
            foreach (var item in model.Elements)
            {
                var abstractElement = item.Value;
                var index = abstractElement.SystemIndicesOfElement;
                var elementMatrix = abstractElement.ComputeDiagonalMatrix();
                systemEquations.AddDiagonalMatrix(index, elementMatrix);
            }

            // restrained degrees of freedom do not contribute to inertia forces
            foreach (var supportCondition in model.BoundaryConditions)
            {
                var systemIndices = supportCondition.Value.Node.SystemIndices;
                for (var i = 0; i < supportCondition.Value.Restrained.Length; i++)
                {
                    if (supportCondition.Value.Restrained[i]) systemEquations.DiagonalMatrix[systemIndices[i]] = 0;
                }
            }
            diagonalMatrix = true;
        }
        // 1st order time integration ***********************************************************************************************
        public void TimeIntegration1StOrder()
        {
            // ... Compute specific heat matrix ..............................
            if (!diagonalMatrix) ComputeDiagonalMatrix();
            _ = systemEquations.DiagonalMatrix;


            var dt = model.Timeintegration.Dt;
            if (dt == 0) throw new AnalysisException("Abbruch: Zeitschrittintervall nicht definiert.");
            var tmax = model.Timeintegration.Tmax;
            var alfa = model.Timeintegration.Parameter1;
            var nSteps = (int)(tmax / dt) + 1;
            var forceFunction = new double[nSteps][];
            for (var k = 0; k < nSteps; k++)
                forceFunction[k] = new double[dimension];
            var temperature = new double[nSteps][];
            for (var i = 0; i < nSteps; i++) temperature[i] = new double[dimension];

            Set1StOrderInitialConditions(temperature);
            SetInstationaryStatusVector();

            // ... compute time dependent forcing function and boundary conditions
            Compute1StOrderForcingFunction(dt, forceFunction);
            Compute1StOrderBoundaryConditions(dt, temperature);

            // ... system matrix needs to be recomputed if it is decomposed
            if (decomposed)
            {
                ReComputeSystemMatrix();
                decomposed = false;
            }

            var timeIntegration = new TimeIntegration1StOrderStatus(
                systemEquations, forceFunction, dt, alfa, temperature);
            timeIntegration.Perform();

            // save nodal time histories
            foreach (var item in model.Nodes)
            {
                node = item.Value;
                var index = item.Value.SystemIndices[0];
                node.NodalVariables = new double[1][];
                node.NodalVariables[0] = new double[nSteps];
                node.NodalDerivatives = new double[1][];
                node.NodalDerivatives[0] = new double[nSteps];

                // temperature[nSteps][index], NodalVariables[index][nSteps]
                for (var k = 0; k < nSteps; k++)
                {
                    node.NodalVariables[0][k] = temperature[k][index];
                    node.NodalDerivatives[0][k] = timeIntegration.Tdot[k][index];
                }
            }

            model.TimeIntegration = true;
        }
        private void Set1StOrderInitialConditions(IList<double[]> temperature)
        {
            // set initial conditions to stationary solution
            if (model.Timeintegration.FromStationary) temperature[0] = systemEquations.Primal;
            foreach (var init in model.Timeintegration.InitialConditions.Cast<NodalValues>())
            {
                if (init.NodeId == "all")
                {
                    for (var i = 0; i < dimension; i++) temperature[0][i] = init.Values[0];
                }
                else
                {
                    if (model.Nodes.TryGetValue(init.NodeId, out var anfKnoten))
                    {
                        temperature[0][anfKnoten.SystemIndices[0]] = init.Values[0];
                    }
                }
            }
        }
        private void SetInstationaryStatusVector()
        {
            // for all time dependent boundary conditions
            if (model == null) return;
            foreach (var randbedingung in
                     model.TimeDependentBoundaryConditions.Select(item => item.Value))
                StatusNodes(randbedingung);
        }
        // time dependent nodal and element loads
        private void Compute1StOrderForcingFunction(double dt, double[][] temperature)
        {
            var force = new double[temperature.Length];
            var nSteps = force.Length;

            // find time dependent Node Loads
            foreach (var item in model.TimeDependentNodeLoads)
                if (model.Nodes.TryGetValue(item.Value.NodeId, out node))
                {
                    var lastIndex = node.SystemIndices;

                    switch (item.Value.VariationType)
                    {
                        case 0:
                            {
                                // read from file
                                const string inputDirectory =
                                    "\\FE-Analysis-App\\input\\Heat_Transfer\\instationary\\ExcitationFiles";
                                const int col = 1;
                                FromFile(inputDirectory, col, force);
                                break;
                            }
                        case 1:
                            {
                                // piecewise linear
                                var interval = item.Value.Interval;
                                PiecewiseLinear(dt, interval, force);
                                break;
                            }
                        case 2:
                            {
                                // periodic
                                var amplitude = item.Value.Amplitude;
                                var frequency = item.Value.Frequency;
                                var phaseAngle = item.Value.PhaseAngle;
                                Periodic(dt, amplitude, frequency, phaseAngle, force);
                                break;
                            }
                    }

                    for (var k = 0; k < nSteps; k++)
                        temperature[k][lastIndex[0]] = force[k];
                }
                else
                {
                    throw new AnalysisException("Node " + item.Value.NodeId +
                                                " for time dependent Node Load not found in Model.");
                }

            // find time dependent Node Loads
            foreach (var timeDepElementLoad in model.TimeDependentElementLoads.Select(item => item.Value))
            {
                if (model.Elements.TryGetValue(timeDepElementLoad.ElementId, out var abstractElement))
                {
                    var index = abstractElement.SystemIndicesOfElement;
                    var lastVektor = timeDepElementLoad.ComputeLoadVector();
                    systemEquations.AddVector(index, lastVektor);
                }

                for (var k = 0; k < nSteps; k++)
                    temperature[k] = systemEquations.Vector;
            }
        }
        // time dependent predefined fixed boundary conditions
        private void Compute1StOrderBoundaryConditions(double dt, double[][] temperature)
        {
            var nSteps = temperature.Length;
            var preTemperature = new double[nSteps];

            foreach (var item in model.TimeDependentBoundaryConditions)
                if (model.Nodes.TryGetValue(item.Value.NodeId, out node))
                {
                    var lastIndex = node.SystemIndices;

                    switch (item.Value.VariationType)
                    {
                        case 0:
                            {
                                // read from file
                                _ = MessageBox.Show("Boundary Condition "+item.Key+" data from file", "Heat Transfer Analysis");

                                const string inputDirectory =
                                    "\\FE-Analysis-App\\input\\HeatTransfer\\instationary\\ExcitationFiles";
                                const int col = 1;
                                FromFile(inputDirectory, col, preTemperature);
                                break;
                            }
                        case 1:
                            {
                                // constant
                                for (var k = 0; k < nSteps; k++) preTemperature[k] = item.Value.ConstantTemperature;
                                break;
                            }
                        case 2:
                            {
                                // periodic
                                var amplitude = item.Value.Amplitude;
                                var frequency = item.Value.Frequency;
                                var phaseAngle = item.Value.PhaseAngle;
                                Periodic(dt, amplitude, frequency, phaseAngle, preTemperature);
                                break;
                            }
                        case 3:
                            {
                                // piecewise linear
                                var interval = item.Value.Interval;
                                PiecewiseLinear(dt, interval, preTemperature);
                                break;
                            }
                    }

                    StatusNodes(item.Value);
                    for (var k = 0; k < nSteps; k++)
                        temperature[k][lastIndex[0]] = preTemperature[k];
                }
                else
                {
                    throw new AnalysisException("Node " + item.Value.NodeId +
                                                " for time dependent boundary condition not found in Model.");
                }
        }
        // 2nd order time integration ***********************************************************************************************
        public void TimeIntegration2NdOrder()
        {
            var dt = model.Timeintegration.Dt;
            if (dt == 0) throw new AnalysisException("Zeitschrittintervall nicht definiert");
            var tmax = model.Timeintegration.Tmax;
            var nSteps = (int)(tmax / dt) + 1;
            var method = model.Timeintegration.Method;
            var parameter1 = model.Timeintegration.Parameter1;
            var parameter2 = model.Timeintegration.Parameter2;
            var excitation = new double[nSteps + 1][];
            for (var i = 0; i < nSteps + 1; i++) excitation[i] = new double[dimension];
            // ... Compute diagonal mass matrix ..............................
            if (!diagonalMatrix) ComputeDiagonalMatrix();

            // ... Compute diagonal damping matrix ..............................
            // ... if "damping" contains modal damping ratios, the damping matrix
            // ... may be evaluated by a sum over "n" modes considered C&P p.198, 13-37
            // ... M*(SUM(((2*(xi)n*(omega)n )/(M)n))*phi(n)*(phi)nT)*M
            // ... where M is the mass matrix, (xi)n the modal damping ratio,
            // ... omega(n) eigenfrequency, (M)n modal masses and phi the eigenvectors
            var dampingMatrix = ComputeDampingMatrix();

            // ... compute time dependent forcing function and boundary conditions
            Compute2NdOrderForcingFunction(dt, excitation);

            var displacement = new double[nSteps][];
            for (var k = 0; k < nSteps; k++) displacement[k] = new double[dimension];
            var velocity = new double[2][];
            for (var k = 0; k < 2; k++) velocity[k] = new double[dimension];

            Set2NdOrderInitialConditions(displacement, velocity);
            SetDynamicStatusVector();

            if (decomposed)
            {
                ReComputeSystemMatrix();
                decomposed = false;
            }

            var timeIntegration = new TimeIntegration2NdOrderStatus(systemEquations, dampingMatrix,
                dt, method, parameter1, parameter2,
                displacement, velocity, excitation);
            timeIntegration.Perform();

            // save nodal time histories
            foreach (var item2 in model.Nodes)
            {
                node = item2.Value;
                var index = item2.Value.SystemIndices;
                var ndof = node.NumberOfNodalDof;

                node.NodalVariables = new double[ndof][];
                for (var i = 0; i < ndof; i++) node.NodalVariables[i] = new double[nSteps];
                node.NodalDerivatives = new double[ndof][];
                for (var i = 0; i < ndof; i++) node.NodalDerivatives[i] = new double[nSteps];

                // displacement[nSteps][index], velocity[2][index], NodalVariables[index][nSteps]
                for (var i = 0; i < node.NumberOfNodalDof; i++)
                {
                    if (systemEquations.Status[index[i]]) continue;
                    for (var k = 0; k < nSteps; k++)
                    {
                        node.NodalVariables[i][k] = timeIntegration.displacement[k][index[i]];
                        node.NodalDerivatives[i][k] = timeIntegration.acceleration[k][index[i]];
                    }
                }
            }

            model.TimeIntegration = true;
            _ = MessageBox.Show("Time History Analysis 2nd order successfully completed", "TimeIntegration2ndOrder");
        }
        private double[] ComputeDampingMatrix()
        {
            // ... in case "damping" consists in form of modal damping ratios
            // ... the damping matrix can be evaluated over the sum of all eigenstates considered
            // ... s. Clough & Penzien p. 198, 13-37
            // ... M*(SUM(((2*(xi)n*(omega)n )/(M)n))*phi(n)*(phi)nT)*M
            // ... where M is the mass matrix, (xi)n the modal damping ratio,
            // ... omega(n) eigenfrequency, (M)n modal masses and phi the eigenvectors
            var dampingMatrix = new double[dimension];
            if (model.Eigenstate.DampingRatios.Count == 0)
            {
                _ = MessageBox.Show("undamped System", "ComputeDampingMatrix");
                return dampingMatrix;
            }
            // Eigensolution is required for evaluation of modal damping
            if (model.Eigen == false)
            {
                Eigenstates();
                model.Eigen = true;
            }
            // modal damping ratios from input of Damping
            var modalDamping = new double[model.Eigenstate.NumberOfStates];
            for (var i = 0; i < model.Eigenstate.DampingRatios.Count; i++)
            {
                modalDamping[i] = ((ModalValues)model.Eigenstate.DampingRatios[i]).Damping;
            }
            // if only a single value is given, ALL Eigenstates will be damped with this ratio
            for (var i = model.Eigenstate.DampingRatios.Count; i < model.Eigenstate.NumberOfStates; i++)
            {
                modalDamping[i] = modalDamping[0];
            }
            double factor = 0;
            for (var n = 0; n < model.Eigenstate.NumberOfStates; n++)
            {
                double phinPhinT = 0;
                double mn = 0;
                for (var i = 0; i < systemEquations.DiagonalMatrix.Length; i++)
                {
                    phinPhinT += model.Eigenstate.Eigenvectors[n][i] * model.Eigenstate.Eigenvectors[n][i];
                }

                for (var i = 0; i < systemEquations.DiagonalMatrix.Length; i++)
                {
                    mn += model.Eigenstate.Eigenvectors[n][i] * systemEquations.DiagonalMatrix[i] * model.Eigenstate.Eigenvectors[n][i];
                }

                factor += 2 * modalDamping[n] * Math.Sqrt(model.Eigenstate.Eigenvalues[n]) / 2 / Math.PI * phinPhinT / mn;
            }
            // diagonal damping matrix evaluated from aus m*faktor*m
            for (var i = 0; i < systemEquations.DiagonalMatrix.Length; i++)
            {
                dampingMatrix[i] = systemEquations.DiagonalMatrix[i] * factor * systemEquations.DiagonalMatrix[i];
            }
            return dampingMatrix;
        }
        private void Set2NdOrderInitialConditions(IReadOnlyList<double[]> displ, IReadOnlyList<double[]> veloc)
        {
            // find predefined initial conditions
            foreach (var anf in model.Timeintegration.InitialConditions.Cast<NodalValues>())
            {
                if (!model.Nodes.TryGetValue(anf.NodeId, out var knoten)) continue;
                for (var i = 0; i < anf.Values.Length / 2; i += 2)
                    foreach (var nodeIndexIndex in knoten.SystemIndices)
                    {
                        displ[i][nodeIndexIndex] = anf.Values[i];
                        veloc[i + 1][nodeIndexIndex] = anf.Values[i + 1];
                    }
            }
        }
        private void SetDynamicStatusVector()
        {
            // for all time dependent boundary conditions
            foreach (var randbedingung in
                     model.TimeDependentBoundaryConditions.Select(item => item.Value))
                StatusNodes(randbedingung);
        }
        // time dependent nodal influences
        private void Compute2NdOrderForcingFunction(double dt, IReadOnlyList<double[]> excitation)
        {
            // find time dependent nodal influences
            foreach (var item in model.TimeDependentNodeLoads)
            {
                var force = new double[excitation.Count];
                switch (item.Value.VariationType)
                {
                    case 0:
                        {
                            const string inputDirectory = "\\FE-Analysis-App\\input\\StructuralAnalysis\\Dynamics\\ExcitationFiles";
                            const int col = -1; // ALL values in file
                                                // read ordinate values at time interval dt from file
                            FromFile(inputDirectory, col, force);
                            break;
                        }
                    case 1:
                        {
                            var interval = item.Value.Interval;
                            // lineare Interpolation der abschnittweise linearen Eingabedaten im Zeitintervall dt
                            PiecewiseLinear(dt, interval, force);
                            break;
                        }
                    case 2:
                        {
                            var amplitude = item.Value.Amplitude;
                            var frequency = 2 * Math.PI * item.Value.Frequency;
                            var phaseAngle = Math.PI / 180 * item.Value.PhaseAngle;
                            // periodische Anregung mit Ausgabe "force" im Zeitintervall dt
                            Periodic(dt, amplitude, frequency, phaseAngle, force);
                            break;
                        }
                }

                if (item.Value.GroundExcitation)
                {
                    var nodalDof = item.Value.NodalDof;

                    var mass = systemEquations.DiagonalMatrix;
                    foreach (var item2 in model.Nodes)
                    {
                        var index = item2.Value.SystemIndices;
                        if (systemEquations.Status[index[nodalDof]]) continue;
                        for (var k = 0; k < excitation.Count; k++)
                            excitation[k][index[nodalDof]] = -mass[index[nodalDof]] * force[k];
                    }
                }

                else
                {
                    if (!model.Nodes.TryGetValue(item.Value.NodeId, out node)) continue;
                    var index = node.SystemIndices;
                    var nodalDof = item.Value.NodalDof;

                    for (var k = 0; k < excitation.Count; k++)
                        for (var j = 0; j < excitation[0].Length; j++)
                            excitation[k][index[nodalDof]] = force[k];
                }
            }
        }
        // time varying input data
        public void FromFile(string inputDirectory, int col, IList<double> force)
        {
            string[] lines, substrings;
            var delimiters = new[] { '\t' };

            var file = new OpenFileDialog
            {
                Filter = "All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            file.InitialDirectory += inputDirectory;

            if (file.ShowDialog() != true)
                return;
            var path = file.FileName;

            try
            {
                lines = File.ReadAllLines(path);
            }
            catch (IOException ex)
            {
                _ = MessageBox.Show(ex + " Excitation function could not be read from file!!!",
                    "Analysis.FromFile");
                return;
            }

            // Excitation function[timeSteps]
            // File contains only excitation values at PREDFINED TIME STEPS dt
            var values = new List<double>();
            if (col < 0)
            {
                // read all values from file
                foreach (var line in lines)
                {
                    substrings = line.Split(delimiters);
                    values.AddRange(substrings.Select(item => double.Parse(item, CultureInfo.InvariantCulture)));
                }
            }
            else
            {
                // read all values from a specific column [][0-n]
                foreach (var line in lines)
                {
                    substrings = line.Split(delimiters);
                    values.Add(double.Parse(substrings[col - 1], CultureInfo.InvariantCulture));
                }
            }

            if (values.Count <= force.Count)
            {
                for (var i = 0; i < values.Count; i++) { force[i] = values[i]; }
            }
            else
            {
                for (var i = 0; i < force.Count; i++) { force[i] = values[i]; }
            }
        }
        public List<double> FromFile(string inputDirectory)
        {
            var delimiters = new[] { '\t' };
            var values = new List<double>();

            var file = new OpenFileDialog
            {
                Filter = "All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            file.InitialDirectory += inputDirectory;

            if (file.ShowDialog() != true)
                return values;
            var path = file.FileName;

            try
            {
                var lines = File.ReadAllLines(path);
                foreach (var line in lines)
                {
                    var substrings = line.Split(delimiters);
                    values.AddRange(substrings.Select(item => double.Parse(item, CultureInfo.InvariantCulture)));
                }
            }
            catch (IOException ex)
            {
                _ = MessageBox.Show(ex + " Excitation function could not be read from file!!!", "Analysis.FromFile");
                return values;
            }
            return values;
        }
        private static void PiecewiseLinear(double dt, IReadOnlyList<double> interval, IList<double> force)
        {
            int counter = 0, nSteps = force.Count;
            double endLoad = 0;
            var startTime = interval[0];
            var startLoad = interval[1];
            force[counter] = startLoad;
            for (var j = 2; j < interval.Count; j += 2)
            {
                var endTime = interval[j];
                endLoad = interval[j + 1];
                var stepsPerInterval = (int)Math.Round((endTime - startTime) / dt);
                var increment = (endLoad - startLoad) / stepsPerInterval;
                for (var k = 1; k <= stepsPerInterval; k++)
                {
                    counter++;
                    if (counter == nSteps) return;
                    force[counter] = force[counter - 1] + increment;
                }

                startTime = endTime;
                startLoad = endLoad;
            }

            for (var k = counter + 1; k < nSteps; k++) force[k] = endLoad;
        }
        private static void Periodic(double dt, double amplitude, double frequency, double phaseAngle, double[] force)
        {
            var nSteps = force.GetLength(0);
            double time = 0;
            for (var k = 0; k < nSteps; k++)
            {
                force[k] = amplitude * Math.Sin(frequency * time + phaseAngle);
                time += dt;
            }
        }
    }
}