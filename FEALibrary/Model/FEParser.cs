using System.Globalization;

namespace FEALibrary.Model
{
    public class FeParser
    {
        private string nodeId, nodePrefix;
        private string[] substrings;
        private readonly char[] delimiters = { '\t' };
        private double[] crds;
        private int numberNodalDof, counter;
        private double xInterval, yInterval, zInterval;
        private int nNodesX, nNodesY;

        public string ModelId { get; set; }
        public FeModel FeModel { get; set; }
        public int SpatialDimension { get; set; }
        public static string InputFound { get; set; }

        // parsing a new model to be read from file
        public void ParseModel(string[] lines)
        {
            for (var i = 0; i < lines.Length; i++)
            {
                InputFound = "";
                if (lines[i] != "Model Name") continue;
                ModelId = lines[i + 1];
                InputFound = "Model Name = " + ModelId;
                break;
            }

            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] != "Spatial Dimension") continue;
                substrings = lines[i + 1].Split(delimiters);
                SpatialDimension = int.Parse(substrings[0]);
                numberNodalDof = int.Parse(substrings[1]);
                InputFound += "\nSpatial Dimension = " + SpatialDimension + ", Nodal DOF = " + numberNodalDof;
                break;
            }

            FeModel = new FeModel(ModelId, SpatialDimension);
        }

        // NodeId, Nodal Coordinates
        public void ParseNodes(string[] lines)
        {
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] == "Nodes")
                {
                    InputFound += "\nNodes";
                    do
                    {
                        substrings = lines[i + 1].Split(delimiters);
                        Node knoten;
                        crds = new double[3];
                        var dimension = FeModel.SpatialDimension;
                        switch (substrings.Length)
                        {
                            case 1:
                                numberNodalDof = int.Parse(substrings[0]);
                                break;
                            case 2:
                                nodeId = substrings[0];
                                crds[0] = double.Parse(substrings[1], CultureInfo.InvariantCulture);
                                knoten = new Node(nodeId, crds, numberNodalDof, dimension);
                                FeModel.Nodes.Add(nodeId, knoten);
                                break;
                            case 3:
                                nodeId = substrings[0];
                                crds[0] = double.Parse(substrings[1], CultureInfo.InvariantCulture);
                                crds[1] = double.Parse(substrings[2], CultureInfo.InvariantCulture);
                                knoten = new Node(nodeId, crds, numberNodalDof, dimension);
                                FeModel.Nodes.Add(nodeId, knoten);
                                break;
                            case 4:
                                nodeId = substrings[0];
                                crds[0] = double.Parse(substrings[1], CultureInfo.InvariantCulture);
                                crds[1] = double.Parse(substrings[2], CultureInfo.InvariantCulture);
                                crds[2] = double.Parse(substrings[3], CultureInfo.InvariantCulture);
                                knoten = new Node(nodeId, crds, numberNodalDof, dimension);
                                FeModel.Nodes.Add(nodeId, knoten);
                                break;
                            default:
                            {
                                throw new ParseException(i + 2 + ": Knoten " + nodeId + " falsche Anzahl Parameter");
                            }
                        }

                        i++;
                    } while (lines[i + 1].Length != 0);
                }

                //Nodes Group
                if (lines[i] == "Nodes Group")
                {
                    InputFound += "\nNodes Group";
                    substrings = lines[i + 1].Split(delimiters);
                    if (substrings.Length == 1) nodePrefix = substrings[0];
                    else
                        throw new ParseException(i + 2 + ": Nodes Group");
                    counter = 0;
                    i += 2;
                    do
                    {
                        substrings = lines[i].Split(delimiters);
                        crds = new double[3];
                        if (substrings.Length == SpatialDimension)
                            for (var k = 0; k < SpatialDimension; k++)
                                crds[k] = double.Parse(substrings[k], CultureInfo.InvariantCulture);
                        else
                            throw new ParseException(i + ": Nodes Group");

                        //spatialDimension += numberNodalDOF;
                        nodeId = nodePrefix + counter.ToString().PadLeft(6, '0');
                        var node = new Node(nodeId, crds, numberNodalDof, SpatialDimension);
                        FeModel.Nodes.Add(nodeId, node);
                        i++;
                        counter++;
                    } while (lines[i].Length != 0);
                }

                //Equidistant mesh of Nodes in 1D
                if (lines[i] == "Equidistant Node Mesh")
                    do
                    {
                        substrings = lines[i + 1].Split(delimiters);
                        nodePrefix = substrings[0];

                        //Aequidistantes Knotennetz in 1D
                        double[] nodeCrds;
                        switch (substrings.Length)
                        {
                            case 4:
                            {
                                InputFound += "\nEquidistant Node Mesh in 1D";
                                crds[0] = double.Parse(substrings[1], CultureInfo.InvariantCulture);
                                xInterval = double.Parse(substrings[2], CultureInfo.InvariantCulture);
                                nNodesX = short.Parse(substrings[3], CultureInfo.InvariantCulture);

                                for (var k = 0; k < nNodesX; k++)
                                {
                                    nodeId = nodePrefix + "0000" + k.ToString().PadLeft(2, '0');
                                    nodeCrds = new[] { crds[0], 0 };
                                    var node = new Node(nodeId, nodeCrds, numberNodalDof, SpatialDimension);
                                    FeModel.Nodes.Add(nodeId, node);
                                    crds[0] += xInterval;
                                }

                                break;
                            }
                            //Equidistant Node Mesh in 2D
                            case 7:
                            {
                                InputFound += "\nEquidistant Node Mesh in 2D";
                                crds = new double[3];
                                crds[0] = double.Parse(substrings[1], CultureInfo.InvariantCulture);
                                xInterval = double.Parse(substrings[2], CultureInfo.InvariantCulture);
                                nNodesX = short.Parse(substrings[3], CultureInfo.InvariantCulture);
                                crds[1] = double.Parse(substrings[4], CultureInfo.InvariantCulture);
                                yInterval = double.Parse(substrings[5], CultureInfo.InvariantCulture);
                                nNodesY = short.Parse(substrings[6], CultureInfo.InvariantCulture);

                                var idZ = "00";
                                for (var k = 0; k < nNodesX; k++)
                                {
                                    var temp = crds[1];
                                    var idY = k.ToString().PadLeft(2, '0');
                                    for (var l = 0; l < nNodesY; l++)
                                    {
                                        var idX = l.ToString().PadLeft(2, '0');
                                        nodeId = nodePrefix + idZ +idY + idX;
                                        nodeCrds = new[] { crds[0], crds[1] };
                                        var node = new Node(nodeId, nodeCrds, numberNodalDof, SpatialDimension);
                                        FeModel.Nodes.Add(nodeId, node);
                                        crds[1] += yInterval;
                                    }

                                    crds[0] += xInterval;
                                    crds[1] = temp;
                                }

                                break;
                            }
                            //Equidistant Node Mesh in 3D
                            case 10:
                            {
                                InputFound += "\nEquidistant Node Mesh in 3D";
                                crds = new double[3];
                                crds[0] = double.Parse(substrings[1], CultureInfo.InvariantCulture);
                                xInterval = double.Parse(substrings[2], CultureInfo.InvariantCulture);
                                nNodesX = short.Parse(substrings[3], CultureInfo.InvariantCulture);
                                crds[1] = double.Parse(substrings[4], CultureInfo.InvariantCulture);
                                yInterval = double.Parse(substrings[5], CultureInfo.InvariantCulture);
                                nNodesY = short.Parse(substrings[6], CultureInfo.InvariantCulture);
                                crds[2] = double.Parse(substrings[7], CultureInfo.InvariantCulture);
                                zInterval = double.Parse(substrings[8], CultureInfo.InvariantCulture);

                                for (var k = 0; k < nNodesX; k++)
                                {
                                    var temp1 = crds[1];
                                    var idX = k.ToString().PadLeft(2, '0');
                                    for (var l = 0; l < nNodesY; l++)
                                    {
                                        var temp2 = crds[2];
                                        var idY = l.ToString().PadLeft(2, '0');
                                        nodeId = nodePrefix + idX + idY;
                                        for (var m = 0; m < nNodesY; m++)
                                        {
                                            var idZ = m.ToString().PadLeft(2, '0');
                                            nodeId = nodePrefix + idX + idY + idZ;
                                            nodeCrds = new[] { crds[0], crds[1], crds[2] };
                                            var node = new Node(nodeId, nodeCrds, numberNodalDof, SpatialDimension);
                                            FeModel.Nodes.Add(nodeId, node);
                                            crds[2] += zInterval;
                                        }

                                        crds[2] = temp2;
                                        crds[1] += yInterval;
                                    }

                                    crds[1] = temp1;
                                    crds[0] += xInterval;
                                }

                                break;
                            }
                            default:
                            {
                                throw new ParseException(i + 3 + ": Equidistant Node Mesh");
                            }
                        }

                        i++;
                    } while (lines[i + 1].Length != 0);

                //Variable Node Mesh
                if (lines[i] != "Variable Node Mesh") continue;
                {
                    do
                    {
                        substrings = lines[i + 1].Split(delimiters);
                        InputFound += "\nVariable Node Mesh";
                        substrings = lines[i + 1].Split(delimiters);
                        string idX, idY;
                        crds = new double[3];

                        double coord0, coord1;
                        var offset = new double[substrings.Length];
                        for (var k = 0; k < substrings.Length; k++)
                            offset[k] = double.Parse(substrings[k], CultureInfo.InvariantCulture);

                        substrings = lines[i + 2].Split(delimiters);
                        double[] nodeCrds;
                        switch (substrings.Length)
                        {
                            case 2:
                            {
                                nodePrefix = substrings[0];
                                coord0 = double.Parse(substrings[1], CultureInfo.InvariantCulture);
                                for (var n = 0; n < offset.Length; n++)
                                {
                                    crds[0] = coord0 + offset[n];
                                    nodeId = nodePrefix + "0000" + n.ToString().PadLeft(2, '0');
                                    nodeCrds = new[] { crds[0], 0 };
                                    var node = new Node(nodeId, nodeCrds, numberNodalDof, SpatialDimension);
                                    FeModel.Nodes.Add(nodeId, node);
                                }

                                break;
                            }
                            case 3:
                            {
                                nodePrefix = substrings[0];
                                var idZ = "00";
                                coord0 = double.Parse(substrings[1], CultureInfo.InvariantCulture);
                                coord1 = double.Parse(substrings[2], CultureInfo.InvariantCulture);
                                for (var n = 0; n < offset.Length; n++)
                                {
                                    idX = n.ToString().PadLeft(2, '0');
                                    crds[0] = coord0 + offset[n];
                                    for (var m = 0; m < offset.Length; m++)
                                    {
                                        idY = m.ToString().PadLeft(2, '0');
                                        crds[1] = coord1 + offset[m];
                                        nodeId = nodePrefix + idX + idY + idZ;
                                        nodeCrds = new[] { crds[0], crds[1] };
                                        var node = new Node(nodeId, nodeCrds, numberNodalDof, SpatialDimension);
                                        FeModel.Nodes.Add(nodeId, node);
                                    }
                                }

                                break;
                            }
                            case 4:
                            {
                                nodePrefix = substrings[0];
                                coord0 = double.Parse(substrings[1], CultureInfo.InvariantCulture);
                                coord1 = double.Parse(substrings[2], CultureInfo.InvariantCulture);
                                var coord2 = double.Parse(substrings[3], CultureInfo.InvariantCulture);
                                for (var n = 0; n < offset.Length; n++)
                                {
                                    idX = n.ToString().PadLeft(2, '0');
                                    var inkrement0 = coord0 + offset[n];
                                    for (var m = 0; m < offset.Length; m++)
                                    {
                                        idY = m.ToString().PadLeft(2, '0');
                                        var inkrement1 = coord1 + offset[m];
                                        for (var k = 0; k < offset.Length; k++)
                                        {
                                            crds = new double[3];
                                            crds[0] = inkrement0;
                                            crds[1] = inkrement1;
                                            var idZ = k.ToString().PadLeft(2, '0');
                                            crds[2] = coord2 + offset[k];
                                            nodeId = nodePrefix + idX + idY + idZ;
                                            nodeCrds = new[] { crds[0], crds[1], crds[2] };
                                            var node = new Node(nodeId, nodeCrds, numberNodalDof, SpatialDimension);
                                            FeModel.Nodes.Add(nodeId, node);
                                        }
                                    }
                                }

                                break;
                            }
                            default:
                            {
                                throw new ParseException(i + 3 + ": Variable Node Mesh");
                            }
                        }
                    } while (lines[i + 3].Length != 0);
                }
            }
        }
    }
}