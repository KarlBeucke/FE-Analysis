using FE_Analysis.Structural_Analysis.Model_Data;
using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Collections.Generic;
using System.Globalization;

namespace FE_Analysis.Structural_Analysis.ModelDataRead;

internal class LoadParser
{
    private readonly char[] delimiters = { '\t' };
    private string elementId;
    private bool inElementCoordinateSystem;

    private string loadId;
    private FeModel model;
    private string nodeId;
    private NodeLoad nodeLoad;
    private double offset;
    private double[] p;
    private PointLoad pointLoad;
    private string[] substrings;

    public void ParseLoads(string[] lines, FeModel feModel)
    {
        model = feModel;

        ParseNodeLoads(lines);
        ParsePointLoads(lines);
        ParseLineLoads(lines);
    }

    private void ParseNodeLoads(IReadOnlyList<string> lines)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "NodeLoad") continue;
            FeParser.InputFound += "\nNodeLoad";
            do
            {
                substrings = lines[i + 1].Split(delimiters);

                p = new double[3];
                switch (substrings.Length)
                {
                    case 4:
                        loadId = substrings[0];
                        nodeId = substrings[1];
                        p[0] = double.Parse(substrings[2], CultureInfo.InvariantCulture);
                        p[1] = double.Parse(substrings[3], CultureInfo.InvariantCulture);
                        nodeLoad = new NodeLoad(nodeId, p[0], p[1]);
                        break;
                    case 5:
                        loadId = substrings[0];
                        nodeId = substrings[1];
                        p[0] = double.Parse(substrings[2], CultureInfo.InvariantCulture);
                        p[1] = double.Parse(substrings[3], CultureInfo.InvariantCulture);
                        p[2] = double.Parse(substrings[4], CultureInfo.InvariantCulture);
                        nodeLoad = new NodeLoad(nodeId, p[0], p[1], p[2])
                        {
                            LoadId = loadId
                        };
                        break;
                    default:
                        {
                            throw new ParseException(i + 2 + ": Truss, wrong number of parameters");
                        }
                }

                model.Loads.Add(loadId, nodeLoad);
                i++;
            } while (lines[i + 1].Length != 0);

            break;
        }
    }

    private void ParsePointLoads(IReadOnlyList<string> lines)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "PointLoad") continue;
            FeParser.InputFound += "\nPointLoad";
            do
            {
                // PointLoad defined by Axial-, Shear Force on beam and offset by percent from beam start
                // e.g. Axial Force pN=0, Shear Force pQ=2 acting on center of element offset=0.5
                substrings = lines[i + 1].Split(delimiters);
                switch (substrings.Length)
                {
                    case 5:
                        loadId = substrings[0];
                        elementId = substrings[1];
                        p = new double[3];
                        p[0] = double.Parse(substrings[2], CultureInfo.InvariantCulture);
                        p[1] = double.Parse(substrings[3], CultureInfo.InvariantCulture);
                        offset = double.Parse(substrings[4], CultureInfo.InvariantCulture);

                        pointLoad = new PointLoad(elementId, p[0], p[1], offset)
                        {
                            LoadId = loadId
                        };
                        model.PointLoads.Add(loadId, pointLoad);
                        i++;
                        break;
                    default:
                        throw new ParseException(i + 2 + ": PointLoad");
                }
            } while (lines[i + 1].Length != 0);

            break;
        }
    }

    private void ParseLineLoads(IReadOnlyList<string> lines)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "LineLoad") continue;
            FeParser.InputFound += "\nLineLoad";
            do
            {
                // LineLoad defined by p0, p1, p2, p3 with optional inElementCoordinateSystem: default= true
                // with local coordinates p0N, p0Q, p1N, p1Q   for inElementCoordinateSystem = true
                // with global coordinates p0x, p0y, p1x, p1y, inElementCoordinateSystem = false
                substrings = lines[i + 1].Split(delimiters);

                p = new double[4];
                AbstractLineLoad lineLoad;
                switch (substrings.Length)
                {
                    case 6:
                        loadId = substrings[0];
                        elementId = substrings[1];
                        p[0] = double.Parse(substrings[2], CultureInfo.InvariantCulture);
                        p[1] = double.Parse(substrings[3], CultureInfo.InvariantCulture);
                        p[2] = double.Parse(substrings[4], CultureInfo.InvariantCulture);
                        p[3] = double.Parse(substrings[5], CultureInfo.InvariantCulture);
                        lineLoad =
                            new LineLoad(elementId, p[0], p[1], p[2], p[3]); // inElementCoordinateSystem = true
                        lineLoad.LoadId = loadId;
                        model.ElementLoads.Add(loadId, lineLoad);
                        i++;
                        break;
                    case 7:
                        loadId = substrings[0];
                        elementId = substrings[1];
                        p[0] = double.Parse(substrings[2], CultureInfo.InvariantCulture);
                        p[1] = double.Parse(substrings[3], CultureInfo.InvariantCulture);
                        p[2] = double.Parse(substrings[4], CultureInfo.InvariantCulture);
                        p[3] = double.Parse(substrings[5], CultureInfo.InvariantCulture);
                        inElementCoordinateSystem = bool.Parse(substrings[6]);
                        lineLoad = new LineLoad(elementId, p[0], p[1], p[2], p[3],
                            inElementCoordinateSystem); //inElementCoordinateSystem = input
                        lineLoad.LoadId = loadId;
                        model.ElementLoads.Add(loadId, lineLoad);
                        i++;
                        break;
                    default:
                        throw new ParseException(i + 2 + ": LineLoad, wrong number of parameters");
                }
            } while (lines[i + 1].Length != 0);

            break;
        }
    }
}