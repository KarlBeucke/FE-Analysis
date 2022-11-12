using FE_Analysis.Elasticity.ModelData;
using FEALibrary.Model;
using System;
using System.Collections.Generic;
using static System.Globalization.CultureInfo;

namespace FE_Analysis.Elasticity.ModelDataRead
{
    public class BoundaryConditionParser : FeParser
    {
        public readonly List<string> faces = new();
        private FeModel model;
        private string nodeId;
        private string[] substrings;
        private Support support;
        private string supportId;

        public void ParseBoundaryConditions(string[] lines, FeModel feModel)
        {
            model = feModel;
            ParseBoundaryConditionsNodes(lines);
            ParseBoundaryConditionsFaces(lines);
            ParseBoundaryConditionBoussinesq(lines);
        }

        private void ParseBoundaryConditionsNodes(IReadOnlyList<string> lines)
        {
            char[] delimiters = { '\t' };

            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i] != "BoundaryCondition Nodes") continue;
                InputFound += "\nBoundaryCondition Nodes";
                double[] prescribed = { 0, 0, 0 };
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    if (substrings.Length == 5 || substrings.Length == 6)
                    {
                        supportId = substrings[0];
                        nodeId = substrings[1];
                        var conditions = 0;
                        var type = substrings[2];
                        for (var k = 0; k < type.Length; k++)
                        {
                            var subType = type.Substring(k, 1);
                            switch (subType)
                            {
                                case "x":
                                    conditions += Support.XFixed;
                                    break;
                                case "y":
                                    conditions += Support.YFixed;
                                    break;
                                case "z":
                                    conditions += Support.ZFixed;
                                    break;
                            }
                        }

                        if (substrings.Length > 3) prescribed[0] = double.Parse(substrings[3], InvariantCulture);
                        if (substrings.Length > 4) prescribed[1] = double.Parse(substrings[4], InvariantCulture);
                        if (substrings.Length > 5) prescribed[2] = double.Parse(substrings[5], InvariantCulture);
                        support = new Support(nodeId, "0", conditions, prescribed, model);
                        model.BoundaryConditions.Add(supportId, support);
                        i++;
                    }
                    else
                    {
                        throw new ParseException(i + 1 + ": Boundary Condition requires 5 or 6 input parameter");
                    }
                } while (lines[i + 1].Length != 0);

                break;
            }
        }

        private void ParseBoundaryConditionsFaces(IReadOnlyList<string> lines)
        {
            char[] delimiters = { '\t' };
            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i] != "BoundaryCondition Faces") continue;
                InputFound += "\nBoundaryCondition Faces";
                var prescribed = new double[3];
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    var supportInitial = substrings[0];
                    var face = substrings[1];
                    faces.Add(face);
                    var nodeInitial = substrings[2];
                    int nNodes = short.Parse(substrings[3]);
                    var type = substrings[4];
                    var conditions = 0;
                    for (var count = 0; count < type.Length; count++)
                    {
                        var subType = type.Substring(count, 1).ToLower();
                        conditions += subType switch
                        {
                            "x" => Support.XFixed,
                            "y" => Support.YFixed,
                            "z" => Support.ZFixed,
                            _ => throw new ParseException("Support Condition for x, y and/or z must be defined")
                        };
                    }

                    var j = 0;
                    for (var k = 5; k < substrings.Length; k++)
                    {
                        prescribed[j] = double.Parse(substrings[k]);
                        j++;
                    }

                    for (var m = 0; m < nNodes; m++)
                    {
                        var id1 = m.ToString().PadLeft(2, '0');
                        for (var k = 0; k < nNodes; k++)
                        {
                            var id2 = k.ToString().PadLeft(2, '0');
                            var supportName = supportInitial + face + id1 + id2;
                            if (model.BoundaryConditions.TryGetValue(supportName, out _))
                                throw new ParseException($"Support Condition \"{supportName}\" already exists.");
                            string nodeName;
                            const string faceNode = "00";
                            nodeName = face.Substring(0, 1) switch
                            {
                                "X" => nodeInitial + faceNode + id1 + id2,
                                "Y" => nodeInitial + id1 + faceNode + id2,
                                "Z" => nodeInitial + id1 + id2 + faceNode,
                                _ => throw new ParseException(
                                    $"wrong FaceId = {face.Substring(0, 1)}, mut be:\n X, Y or Z")
                            };

                            support = new Support(nodeName, face, conditions, prescribed, model);
                            model.BoundaryConditions.Add(supportName, support);
                        }
                    }

                    i++;
                } while (lines[i + 1].Length != 0);

                break;
            }
        }

        private void ParseBoundaryConditionBoussinesq(IReadOnlyList<string> lines)
        {
            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i] != "BoundaryCondition Boussinesq") continue;
                var gModulus = MaterialParser.GModul;
                var poisson = MaterialParser.Poisson;
                if (LoadParser.NodeLoad == null)
                    throw new ParseException("Node Load for Boussinesq boundary condition not defined");
                var p = 4.0 * LoadParser.NodeLoad[2];
                char[] delimiters = { '\t' };

                // 1. Zeile: Feld mit Offsets
                // 2. Zeile: supportInitial, face, nodeInitial, type
                InputFound += "\nBoundaryCondition Boussinesq";
                substrings = lines[i + 1].Split(delimiters);
                var offset = new double[substrings.Length];
                for (var k = 0; k < substrings.Length; k++)
                    offset[k] = double.Parse(substrings[k]);

                var prescribed = new double[3];
                i += 2;
                do
                {
                    var conditions = 0;
                    string subType;
                    substrings = lines[i].Split(delimiters);

                    var supportInitial = substrings[0];
                    var face = substrings[1];
                    faces.Add(face);
                    var nodeInitial = substrings[2];
                    //int nNodes = short.Parse(substrings[3]);
                    var nNodes = offset.Length;
                    face = $"{face.Substring(0, 1)}0{(nNodes - 1)}";
                    var type = substrings[3];
                    for (var count = 0; count < type.Length; count++)
                    {
                        subType = type.Substring(count, 1).ToLower();
                        conditions += subType switch
                        {
                            "x" => Support.XFixed,
                            "y" => Support.YFixed,
                            "z" => Support.ZFixed,
                            _ => throw new ParseException("5. Parameter must be x and/or y and/or z")
                        };
                    }

                    for (var m = 0; m < nNodes; m++)
                    {
                        var id1 = m.ToString().PadLeft(2, '0');
                        for (var k = 0; k < nNodes; k++)
                        {
                            var id2 = k.ToString().PadLeft(2, '0');
                            var supportName = supportInitial + face + id1 + id2;
                            if (model.BoundaryConditions.TryGetValue(supportName, out _))
                                throw new ParseException($"Randbedingung \"{supportName}\" bereits vorhanden.");
                            string nodeName;
                            var faceNode = $"0{(offset.Length - 1)}";
                            switch (face.Substring(0, 1))
                            {
                                case "X":
                                    nodeName = nodeInitial + faceNode + id1 + id2;
                                    break;
                                case "Y":
                                    nodeName = nodeInitial + id1 + faceNode + id2;
                                    break;
                                case "Z":
                                    nodeName = nodeInitial + id1 + id2 + faceNode;
                                    break;
                                default:
                                    throw new ParseException(
                                        $"wrong Face Id = {face.Substring(0, 1)}, must be:\n X, Y or Z");
                            }

                            for (var count = 0; count < type.Length; count++)
                            {
                                subType = type.Substring(count, 1).ToLower();
                                double x, y, z, r, a, factor;
                                switch (subType)
                                {
                                    case "x":
                                        x = offset[nNodes - 1];
                                        y = offset[m];
                                        z = offset[k];
                                        r = Math.Sqrt(x * x + y * y);
                                        a = Math.Sqrt(z * z + r * r);
                                        factor = p / (4 * Math.PI * gModulus * a);
                                        prescribed[0] = x / r * (r * z / (a * a) - (1 - 2 * poisson) * r / (a + z)) *
                                                        factor;
                                        break;
                                    case "y":
                                        x = offset[m];
                                        y = offset[nNodes - 1];
                                        z = offset[k];
                                        r = Math.Sqrt(x * x + y * y);
                                        a = Math.Sqrt(z * z + r * r);
                                        factor = p / (4 * Math.PI * gModulus * a);
                                        prescribed[1] = y / r * (r * z / (a * a) - (1 - 2 * poisson) * r / (a + z)) *
                                                        factor;
                                        break;
                                    case "z":
                                        x = offset[m];
                                        y = offset[k];
                                        z = offset[nNodes - 1];
                                        r = Math.Sqrt(x * x + y * y);
                                        a = Math.Sqrt(z * z + r * r);
                                        factor = p / (4 * Math.PI * gModulus * a);
                                        prescribed[2] = (z * z / (a * a) + 2 * (1 - poisson)) * factor;
                                        break;
                                    default:
                                        throw new ParseException(
                                            "wrong number of parameters in BoundaryConditionsBoussinesq, must be:\n"
                                            + "4 for supportInitial, area, nodeInitial, Type\n");
                                }
                            }

                            support = new Support(nodeName, face, conditions, prescribed, model);
                            model.BoundaryConditions.Add(supportName, support);
                        }
                    }

                    i++;
                } while (lines[i].Length != 0);
            }
        }
    }
}