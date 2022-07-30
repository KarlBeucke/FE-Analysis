using FE_Analysis.Structural_Analysis.Model_Data;
using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Globalization;

namespace FE_Analysis.Structural_Analysis.ModelDataRead
{
    internal class TransientParser
    {
        private readonly char[] delimiters = { '\t', ';' };
        private string[] substrings;
        public bool timeIntegrationData;

        public void ParseTimeIntegration(string[] lines, FeModel feModel)
        {
            var model = feModel;

            // find "Eigensolutions"
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] != "Eigensolutions") continue;
                FeParser.InputFound += "\nEigensolutions";

                substrings = lines[i + 1].Split(delimiters);
                if (substrings.Length == 2)
                {
                    var id = substrings[0];
                    int numberOfStates = short.Parse(substrings[1]);
                    model.Eigenstate = new Eigenstates(id, numberOfStates);
                    break;
                }

                throw new ParseException(i + 2 + ": Eigensolutions, wrong number of parameters");
            }

            // find "TimeIntegration"
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] != "TimeIntegration") continue;
                FeParser.InputFound += "\nTimeIntegration";
                //id, tmax, dt, method, parameter1, parameter2
                //method=1:beta,gamma  method=2:theta  method=3: alfa
                substrings = lines[i + 1].Split(delimiters);
                switch (substrings.Length)
                {
                    case 5:
                        var tmax = double.Parse(substrings[1], CultureInfo.InvariantCulture);
                        var dt = double.Parse(substrings[2], CultureInfo.InvariantCulture);
                        var method = short.Parse(substrings[3], CultureInfo.InvariantCulture);
                        var parameter1 = double.Parse(substrings[4], CultureInfo.InvariantCulture);
                        model.Timeintegration =
                            new TimeIntegration(tmax, dt, method, parameter1);
                        break;
                    case 6:
                        tmax = double.Parse(substrings[1], CultureInfo.InvariantCulture);
                        dt = double.Parse(substrings[2], CultureInfo.InvariantCulture);
                        method = short.Parse(substrings[3]);
                        parameter1 = double.Parse(substrings[4], CultureInfo.InvariantCulture);
                        var parameter2 = double.Parse(substrings[5], CultureInfo.InvariantCulture);
                        model.Timeintegration =
                            new TimeIntegration(tmax, dt, method, parameter1, parameter2);
                        break;
                    default:
                        throw new ParseException(i + 2 + ": TimeIntegration, wrong number of parameters");
                }

                timeIntegrationData = true;
            }

            // find "Damping"
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] != "Damping") continue;
                FeParser.InputFound += "\nDamping";
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    foreach (var rate in substrings)
                    {
                        model.Eigenstate.DampingRatios.
                            Add(new ModalValues(double.Parse(rate, CultureInfo.InvariantCulture)));
                    }
                    i++;
                } while (lines[i + 1].Length != 0);
                break;
            }

            // find "InitialConditions"
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] != "InitialConditions") continue;
                FeParser.InputFound += "\nInitialConditions";
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    var initialNodeId = substrings[0];
                    // Initial deformations and velocities
                    int nodalDof;
                    switch (substrings.Length)
                    {
                        case 3:
                            nodalDof = 1;
                            break;
                        case 5:
                            nodalDof = 2;
                            break;
                        case 7:
                            nodalDof = 3;
                            break;
                        default:
                            throw new ParseException(i + 2 + ": InitialConditions, wrong number of parameters");
                    }

                    var anfangsWerte = new double[2 * nodalDof];
                    for (var k = 0; k < 2 * nodalDof; k++) anfangsWerte[k] = double.Parse(substrings[k + 1], CultureInfo.InvariantCulture);
                    model.Timeintegration.InitialConditions.Add(new NodalValues(initialNodeId, anfangsWerte));
                    i++;
                } while (lines[i + 1].Length != 0);

                break;
            }

            // find "TimeDependentNodeLoads"
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] != "TimeDependentNodeLoads") continue;
                FeParser.InputFound += "\nTimeDependentNodeLoads";
                var ground = false;
                i++;

                do
                {
                    substrings = lines[i].Split(delimiters);
                    if (substrings.Length != 3)
                        throw new ParseException(i + 2 + ": TimeDependentNodeLoad, wrong number of parameters");

                    var nodeLoadId = substrings[0];
                    var nodeId = substrings[1];
                    if (nodeId == "ground") ground = true;
                    var nodalDof = short.Parse(substrings[2]);

                    substrings = lines[i + 1].Split(delimiters);
                    TimeDependentNodeLoad timeDependentNodeLoads;
                    switch (substrings.Length)
                    {
                        // 1 value: read excitation (load vector) from file, VariationType = 0
                        case 1:
                            {
                                timeDependentNodeLoads =
                                    new TimeDependentNodeLoad(nodeLoadId, nodeId, nodalDof, true, ground)
                                    { VariationType = 0 };
                                var last = (AbstractTimeDependentNodeLoad)timeDependentNodeLoads;
                                model.TimeDependentNodeLoads.Add(nodeLoadId, last);
                                break;
                            }
                        // 3 Werte: harmonische Anregung, Variationstyp = 2
                        case 3:
                            {
                                var amplitude = double.Parse(substrings[0], CultureInfo.InvariantCulture);
                                var circularFrequency = double.Parse(substrings[1], CultureInfo.InvariantCulture);
                                var phaseAngle = double.Parse(substrings[2], CultureInfo.InvariantCulture);
                                timeDependentNodeLoads =
                                    new TimeDependentNodeLoad(nodeLoadId, nodeId, nodalDof, false, ground)
                                    {
                                        Amplitude = amplitude,
                                        Frequency = circularFrequency,
                                        PhaseAngle = phaseAngle,
                                        VariationType = 2
                                    };
                                model.TimeDependentNodeLoads.Add(nodeLoadId, timeDependentNodeLoads);
                                break;
                            }
                        // mehr als 3 Werte: lies Zeit-/Wert-Intervalle der Anregung mit linearer Interpolation, Variationstyp = 1
                        default:
                            {
                                var interval = new double[substrings.Length];
                                for (var j = 0; j < substrings.Length; j += 2)
                                {
                                    interval[j] = double.Parse(substrings[j], CultureInfo.InvariantCulture);
                                    interval[j + 1] = double.Parse(substrings[j + 1], CultureInfo.InvariantCulture);
                                }

                                timeDependentNodeLoads =
                                    new TimeDependentNodeLoad(nodeLoadId, nodeId, nodalDof, false, ground)
                                    { Interval = interval, VariationType = 1 };
                                model.TimeDependentNodeLoads.Add(nodeLoadId, timeDependentNodeLoads);
                                break;
                            }
                    }

                    i += 2;
                } while (lines[i].Length != 0);
            }
        }
    }
}