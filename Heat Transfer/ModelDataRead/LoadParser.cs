using System.Collections.Generic;
using FE_Analysis.Heat_Transfer.Model_Data;
using FEALibrary.Model;
using static System.Globalization.CultureInfo;

namespace FE_Analysis.Heat_Transfer.ModelDataRead
{
    public class LoadParser
    {
        private string elementId;
        private ElementLoad3 elementLoad3;
        private ElementLoad4 elementLoad4;
        private LineLoad lineLoad;
        private string loadId;
        private FeModel model;
        private string nodeId, startNodeId, endNodeId;
        private NodeLoad nodeLoad;
        private double[] p = new double[8];
        private string[] substrings;

        public void ParseLoads(string[] lines, FeModel feModel)
        {
            model = feModel;
            ParseNodeLoads(lines);
            ParseLineLoads(lines);
            ParseElementLoads3(lines);
            ParseElementLoads4(lines);
        }

        private void ParseNodeLoads(IReadOnlyList<string> lines)
        {
            var delimiters = new[] { '\t' };

            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i] != "NodeLoads") continue;
                FeParser.InputFound += "\nNodeLoads";
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    switch (substrings.Length)
                    {
                        case 3:
                            {
                                loadId = substrings[0];
                                nodeId = substrings[1];
                                p[0] = double.Parse(substrings[2], InvariantCulture);
                                nodeLoad = new NodeLoad(loadId, nodeId, p);
                                model.Loads.Add(loadId, nodeLoad);
                                i++;
                                break;
                            }
                        default:
                            throw new ParseException(i + 2 + ": NodeLoads, wrong number of parameters");
                    }
                } while (lines[i + 1].Length != 0);

                break;
            }
        }

        private void ParseLineLoads(string[] lines)
        {
            var delimiters = new[] { '\t' };

            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] != "LineLoads") continue;
                FeParser.InputFound += "\nLineLoads";
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    switch (substrings.Length)
                    {
                        case 5:
                            {
                                loadId = substrings[0];
                                startNodeId = substrings[1];
                                endNodeId = substrings[2];
                                p[0] = double.Parse(substrings[3], InvariantCulture);
                                p[1] = double.Parse(substrings[4], InvariantCulture);
                                lineLoad = new LineLoad(loadId, startNodeId, endNodeId, p);
                                model.LineLoads.Add(loadId, lineLoad);
                                i++;
                                break;
                            }
                        default:
                            throw new ParseException(i + 2 + ": LineLoads, wrong number of parameters");
                    }
                } while (lines[i + 1].Length != 0);

                break;
            }
        }

        private void ParseElementLoads3(string[] lines)
        {
            var delimiters = new[] { '\t' };

            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] != "ElementLoads3") continue;
                FeParser.InputFound += "\nElementLoads3";
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    switch (substrings.Length)
                    {
                        case 5:
                            {
                                loadId = substrings[0];
                                elementId = substrings[1];
                                p[0] = double.Parse(substrings[2], InvariantCulture);
                                p[1] = double.Parse(substrings[3], InvariantCulture);
                                p[2] = double.Parse(substrings[4], InvariantCulture);
                                elementLoad3 = new ElementLoad3(loadId, elementId, p);
                                model.ElementLoads.Add(loadId, elementLoad3);
                                i++;
                                break;
                            }
                        default:
                            throw new ParseException(i + 2 + ": ElementLoad3, wrong number of parameters");
                    }
                } while (lines[i + 1].Length != 0);

                break;
            }
        }

        private void ParseElementLoads4(string[] lines)
        {
            var delimiters = new[] { '\t' };

            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] != "ElementLoads4") continue;
                FeParser.InputFound += "\nElementLoads4";
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    switch (substrings.Length)
                    {
                        case 6:
                            {
                                loadId = substrings[0];
                                elementId = substrings[1];
                                p[0] = double.Parse(substrings[2], InvariantCulture);
                                p[1] = double.Parse(substrings[3], InvariantCulture);
                                p[2] = double.Parse(substrings[4], InvariantCulture);
                                p[3] = double.Parse(substrings[5], InvariantCulture);
                                elementLoad4 = new ElementLoad4(loadId, elementId, p);
                                model.ElementLoads.Add(loadId, elementLoad4);
                                i++;
                                break;
                            }
                        default:
                            throw new ParseException(i + 2 + ": ElementLoads4, wrong number of parameters");
                    }
                } while (lines[i + 1].Length != 0);

                break;
            }
        }
    }
}