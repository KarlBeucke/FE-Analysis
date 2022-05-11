using FE_Analysis.Elasticity.ModelData;
using FEALibrary.Model;
using static System.Globalization.CultureInfo;

namespace FE_Analysis.Elasticity.ModelDataRead
{
    public class LoadParser
    {
        private readonly char[] delimiters = { '\t' };
        private NodeLoad knotenLast;
        private LineLoad linienLast;

        private string loadId;
        private FeModel model;
        private string nodeId;
        private string[] substrings;

        public static double[] NodeLoad { get; set; }

        public void ParseLoads(string[] lines, FeModel feModel)
        {
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] != "NodeLoads") continue;
                model = feModel;
                FeParser.InputFound += "\nNodeLoads";
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    loadId = substrings[0];
                    nodeId = substrings[1];
                    NodeLoad = new double[3];
                    NodeLoad[0] = double.Parse(substrings[2], InvariantCulture);
                    NodeLoad[1] = double.Parse(substrings[3], InvariantCulture);

                    switch (substrings.Length)
                    {
                        case 4:
                            knotenLast = new NodeLoad(nodeId, NodeLoad[0], NodeLoad[1]);
                            break;
                        case 5:
                            {
                                NodeLoad[2] = double.Parse(substrings[4], InvariantCulture);
                                //var p = 4 * NodeLoad[2];
                                knotenLast = new NodeLoad(nodeId, NodeLoad[0], NodeLoad[1], NodeLoad[2]);
                                break;
                            }
                        default:
                            throw new ParseException(i + 1 + ": Knotenlasten erfordert 4 oder 5 Eingabeparameter");
                    }

                    knotenLast.LoadId = loadId;
                    model.Loads.Add(loadId, knotenLast);
                    i++;
                } while (lines[i + 1].Length != 0);

                break;
            }

            for (var i = 0; i < lines.Length; i++)
            {
                model = feModel;
                if (lines[i] != "LineLoads") continue;
                FeParser.InputFound += "\nLineLoads";
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    if (substrings.Length == 7)
                    {
                        loadId = substrings[0];
                        var startNodeId = substrings[1];
                        NodeLoad = new double[4];
                        NodeLoad[0] = double.Parse(substrings[2], InvariantCulture);
                        NodeLoad[1] = double.Parse(substrings[3], InvariantCulture);
                        var endNodeId = substrings[4];
                        NodeLoad[2] = double.Parse(substrings[5], InvariantCulture);
                        NodeLoad[3] = double.Parse(substrings[6], InvariantCulture);

                        linienLast = new LineLoad(startNodeId, NodeLoad[0], NodeLoad[1], endNodeId, NodeLoad[2],
                            NodeLoad[3]);
                        model.LineLoads.Add(loadId, linienLast);
                        i++;
                    }
                    else
                    {
                        throw new ParseException(i + 1 + ": Linienlasten erfordert 7 Eingabeparameter");
                    }
                } while (lines[i + 1].Length != 0);

                break;
            }
        }
    }
}