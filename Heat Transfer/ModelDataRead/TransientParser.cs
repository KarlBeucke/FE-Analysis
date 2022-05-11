using FE_Analysis.Heat_Transfer.Model_Data;
using FEALibrary.Model;
using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace FE_Analysis.Heat_Transfer.ModelDataRead
{
    public class TransientParser
    {
        private string[] substrings;
        public bool timeIntegrationData;

        public void ParseTimeIntegration(string[] lines, FeModel feModel)
        {
            var delimiters = new[] { '\t' };

            //find "Eigensolutions"
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] != "Eigensolutions") continue;
                FeParser.InputFound += "\nEigensolutions";
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    switch (substrings.Length)
                    {
                        case 2:
                            {
                                var id = substrings[0];
                                int numberOfStates = short.Parse(substrings[1]);
                                feModel.Eigenstate = new Eigenstates(id, numberOfStates);
                                i++;
                                break;
                            }
                        default:
                            throw new ParseException(i + 2 + ": Eigenstates: wrong number of parameters");
                    }
                } while (lines[i + 1].Length != 0);

                break;
            }

            // find "TimeIntegration"
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] != "TimeIntegration") continue;
                FeParser.InputFound += "\nTimeIntegration";
                var teilStrings = lines[i + 1].Split(delimiters);
                var tmax = double.Parse(teilStrings[1], CultureInfo.InvariantCulture);
                var dt = double.Parse(teilStrings[2], CultureInfo.InvariantCulture);
                var alfa = double.Parse(teilStrings[3], CultureInfo.InvariantCulture);
                feModel.Timeintegration = new TimeIntegration(tmax, dt, alfa)
                { Id = teilStrings[0], FromStationary = false };
                timeIntegrationData = true;
                break;
            }

            // find InitialTemperatures
            for (var i = 0; i < lines.Length; i++)
            {
                // stationäre Loesung oder nodeId (incl. "alle")
                if (lines[i] != "Initial Temperatures") continue;
                FeParser.InputFound += "\nInitial Temperatures";

                var teilStrings = lines[i + 1].Split(delimiters);
                if (teilStrings[0] == "stationary solution")
                    feModel.Timeintegration.FromStationary = true;
                else if (teilStrings.Length == 2)
                    do
                    {
                        // nodeId incl. all
                        var nodeId = teilStrings[0];
                        var t0 = double.Parse(teilStrings[1], CultureInfo.InvariantCulture);
                        var initial = new double[1];
                        initial[0] = t0;
                        feModel.Timeintegration.InitialConditions.Add(new NodalValues(nodeId, initial));
                        i++;
                        teilStrings = lines[i + 1].Split(delimiters);
                    } while (lines[i + 1].Length != 0);
                else
                    break;
            }

            // find time dependent boundary temperatures, induced temperature at boundary
            //  5:Name,NodeId,Amplitude,Frequency,Phase 
            // >7:Name,NodeId,pairs of values for piecewise linear distribution
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] != "TimeDependent BoundaryConditions") continue;
                FeParser.InputFound += "\nTimeDependent BoundaryConditions";
                do
                {
                    var teilStrings = lines[i + 1].Split(delimiters);
                    var supportId = teilStrings[0];
                    var nodeId = teilStrings[1];

                    TimeDependentBoundaryCondition timeDependentBoundaryCondition = null;
                    if (teilStrings.Length < 3)
                        throw new FEALibrary.Model.ParseException(i + 2 +
                                                                  ": TimeDependent Boundary Condition, wrong number of parameters");
                    switch (teilStrings[2])
                    {
                        case "file":
                            {
                                const bool file = true;
                                timeDependentBoundaryCondition =
                                    new TimeDependentBoundaryCondition(nodeId, file)
                                    { SupportId = supportId, VariationType = 0, Prescribed = new double[1] };
                                break;
                            }
                        case "constant":
                            {
                                if (teilStrings.Length != 4)
                                    throw new FEALibrary.Model.ParseException(i + 2 +
                                                                              ": TimeDependent BoundaryCondition constant, wrong number of parameters");
                                var constantTemperature = double.Parse(teilStrings[3], CultureInfo.InvariantCulture);
                                timeDependentBoundaryCondition =
                                    new TimeDependentBoundaryCondition(nodeId, constantTemperature)
                                    { SupportId = supportId, VariationType = 1, Prescribed = new double[1] };
                                break;
                            }
                        case "harmonic":
                            {
                                if (teilStrings.Length != 6)
                                    throw new FEALibrary.Model.ParseException(i + 2 +
                                                                              ": TimeDependent BoundaryCondition harmonic, wrong number of parameters");
                                var amplitude = double.Parse(teilStrings[3], CultureInfo.InvariantCulture);
                                var frequency = double.Parse(teilStrings[4], CultureInfo.InvariantCulture);
                                var phaseAngle = double.Parse(teilStrings[5], CultureInfo.InvariantCulture);
                                timeDependentBoundaryCondition =
                                    new TimeDependentBoundaryCondition(nodeId, amplitude, frequency, phaseAngle)
                                    { SupportId = supportId, VariationType = 2, Prescribed = new double[1] };
                                break;
                            }
                        case "linear":
                            {
                                if (teilStrings.Length < 5)
                                    throw new FEALibrary.Model.ParseException(i + 2 +
                                                                              ": TimeDependent BoundaryCondition linear, wrong number of parameters");
                                var k = 0;
                                char[] paarDelimiter = { ';' };
                                var interval = new double[2 * (teilStrings.Length - 3)];

                                for (var j = 3; j < teilStrings.Length; j++)
                                {
                                    var wertePaar = teilStrings[j].Split(paarDelimiter);
                                    interval[k] = double.Parse(wertePaar[0], CultureInfo.InvariantCulture);
                                    interval[k + 1] = double.Parse(wertePaar[1], CultureInfo.InvariantCulture);
                                    k += 2;
                                }

                                timeDependentBoundaryCondition = new TimeDependentBoundaryCondition(nodeId, interval)
                                { SupportId = supportId, VariationType = 3, Prescribed = new double[1] };
                                break;
                            }
                    }

                    feModel.TimeDependentBoundaryConditions.Add(supportId, timeDependentBoundaryCondition);
                    i++;
                } while (lines[i + 1].Length != 0);

                break;
            }

            // find time dependent node loads (temperatures) nodal temperatures
            //  5:Name,NodeId,Amplitude,Frequency,Phase 
            // >7:Name,NodeId,pairs of values for piecewise linear distribution
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] != "TimeDependent NodeLoads") continue;
                FeParser.InputFound += "\nTimeDependentNodeLoad";

                do
                {
                    var teilStrings = lines[i + 1].Split(delimiters);
                    if (teilStrings.Length < 3)
                        throw new FEALibrary.Model.ParseException(i + 2 +
                                                                  ": TimeDependent NodeLoad, wrong number of parameters");
                    var loadId = teilStrings[0];
                    var nodeId = teilStrings[1];

                    TimeDependentNodeLoad timeDependentNodeLoad = null;

                    switch (teilStrings[2])
                    {
                        case "datei":
                            {
                                timeDependentNodeLoad =
                                    new TimeDependentNodeLoad(nodeId, true) { LoadId = loadId };
                                break;
                            }
                        case "harmonic":
                            {
                                if (teilStrings.Length != 6)
                                    throw new FEALibrary.Model.ParseException(i + 2 +
                                                                              ": TimeDependent NodeLoad harmonic, wrong number of parameters");
                                var amplitude = double.Parse(teilStrings[3], CultureInfo.InvariantCulture);
                                var frequency = double.Parse(teilStrings[4], CultureInfo.InvariantCulture);
                                var phaseAngle = double.Parse(teilStrings[5], CultureInfo.InvariantCulture);
                                timeDependentNodeLoad =
                                    new TimeDependentNodeLoad(nodeId, amplitude, frequency, phaseAngle)
                                    { LoadId = loadId };
                                break;
                            }
                        case "linear":
                            {
                                if (teilStrings.Length < 5)
                                    throw new FEALibrary.Model.ParseException(i + 2 +
                                                                              ": TimeDependent NodeLoad linear, wrong number of parameters");
                                var k = 0;
                                char[] paarDelimiter = { ';' };
                                var interval = new double[2 * (teilStrings.Length - 3)];
                                for (var j = 3; j < teilStrings.Length; j++)
                                {
                                    var wertePaar = teilStrings[j].Split(paarDelimiter);
                                    interval[k] = double.Parse(wertePaar[0], CultureInfo.InvariantCulture);
                                    interval[k + 1] = double.Parse(wertePaar[1], CultureInfo.InvariantCulture);
                                    k += 2;
                                }

                                timeDependentNodeLoad =
                                    new TimeDependentNodeLoad(nodeId, interval) { LoadId = loadId };
                                break;
                            }
                    }

                    feModel.TimeDependentNodeLoads.Add(loadId, timeDependentNodeLoad);
                    i++;
                } while (lines[i + 1].Length != 0);

                break;
            }

            // suche zeitabhängigeElementLast auf Dreieckselementen
            //  5:Name,ElementId,Knotenwert1, Knotenwert2, Knotenwert3 
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] != "TimeDependent ElementLoads") continue;
                FeParser.InputFound += "\nTimeDependent ElementLoads";
                var knotenWerte = new double[3];
                do
                {
                    var teilStrings = lines[i + 1].Split(delimiters);
                    var loadId = teilStrings[0];
                    var elementId = teilStrings[1];
                    TimeDependentElementLoad zeitabhängigeElementLast = null;
                    switch (teilStrings[2])
                    {
                        case "constant":
                            {
                                for (var k = 3; k < teilStrings.Length; k++)
                                    knotenWerte[k - 3] =
                                        double.Parse(teilStrings[k], CultureInfo.InvariantCulture);
                                zeitabhängigeElementLast =
                                    new TimeDependentElementLoad(elementId, knotenWerte)
                                    { LoadId = loadId, VariationType = 1 };
                                break;
                            }
                    }

                    feModel.TimeDependentElementLoads.Add(loadId, zeitabhängigeElementLast);
                    i++;
                } while (lines[i + 1].Length != 0);

                break;
            }
        }

        [Serializable]
        private class ParseException : Exception
        {
            public ParseException()
            {
            }

            public ParseException(string message) : base(message)
            {
            }

            public ParseException(string message, Exception innerException) : base(message, innerException)
            {
            }

            protected ParseException(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }
        }
    }
}