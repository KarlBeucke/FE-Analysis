using System.Globalization;

namespace FEALibrary.Model
{
    public class FeParser
    {
        private string nodeId, nodePrefix;
        private string[] substrings;
        private readonly char[] delimiters = { '\t' };
        private double[] crds;
        private int counter;
        private double xInterval, yInterval, zInterval;
        private int nNodesX, nNodesY, nNodesZ;

        private string ModelId { get; set; }
        public FeModel FeModel { get; private set; }
        private int SpatialDimension { get; set; }
        private int NumberNodalDof { get; set; }
        public static string InputFound { get; set; }

        // parsing a new model to be read from file
        public void ParseModel(string[] lines)
        {
            InputFound = "";
            for (var i = 0; i < lines.Length; i++)
            {
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
                NumberNodalDof = int.Parse(substrings[1]);
                InputFound += "\nSpatial Dimension = " + SpatialDimension + ", Nodal DOF = " + NumberNodalDof;
                break;
            }

            FeModel = new FeModel(ModelId, SpatialDimension, NumberNodalDof);
        }

        // NodeId, Nodal Coordinates
        public void ParseNodes(string[] lines)
        {
            for (var i = 0; i < lines.Length; i++)
            {
                double[] nodalCrds;
                if (lines[i] == "Nodes")
                {
                    InputFound += "\nNodes";
                    while (i + 1 <= lines.Length)
                    {
                        if (lines[i + 1] == string.Empty) break;
                        substrings = lines[i + 1].Split(delimiters);
                        Node node;
                        crds = new double[3];
                        var dimension = FeModel.SpatialDimension;
                        switch (substrings.Length)
                        {
                            case 1:
                                NumberNodalDof = int.Parse(substrings[0]);
                                break;
                            case 2:
                                nodeId = substrings[0];
                                crds[0] = double.Parse(substrings[1], CultureInfo.InvariantCulture);
                                nodalCrds = new[] { crds[0] };
                                node = new Node(nodeId, nodalCrds, NumberNodalDof, dimension);
                                FeModel.Nodes.Add(nodeId, node);
                                break;
                            case 3:
                                nodeId = substrings[0];
                                crds[0] = double.Parse(substrings[1], CultureInfo.InvariantCulture);
                                crds[1] = double.Parse(substrings[2], CultureInfo.InvariantCulture);
                                nodalCrds = new[] { crds[0], crds[1] };
                                node = new Node(nodeId, nodalCrds, NumberNodalDof, dimension);
                                FeModel.Nodes.Add(nodeId, node);
                                break;
                            case 4:
                                nodeId = substrings[0];
                                crds[0] = double.Parse(substrings[1], CultureInfo.InvariantCulture);
                                crds[1] = double.Parse(substrings[2], CultureInfo.InvariantCulture);
                                crds[2] = double.Parse(substrings[3], CultureInfo.InvariantCulture);
                                nodalCrds = new[] { crds[0], crds[1], crds[2] };
                                node = new Node(nodeId, nodalCrds, NumberNodalDof, dimension);
                                FeModel.Nodes.Add(nodeId, node);
                                break;
                            default:
                                throw new ParseException(i + 2 + ": Node " + nodeId + " wrong number of prameters");
                        }
                        i++;
                    }
                }

                //Nodes Group
                if (lines[i] == "Nodes Group")
                {
                    InputFound += "\nNodes Group";
                    i++;
                    while (i <= lines.Length)
                    {
                        if (lines[i] == string.Empty) break;
                        substrings = lines[i].Split(delimiters);
                        if (substrings.Length == 1) nodePrefix = substrings[0];
                        else
                            throw new ParseException(i + 2 + ": Nodes Group wrong Prefix");
                        counter = 0;
                        while (lines[i + 1].Length > 1)
                        {
                            substrings = lines[i + 1].Split(delimiters);
                            nodalCrds = new double[substrings.Length];
                            for (var k = 0; k < substrings.Length; k++)
                                nodalCrds[k] = double.Parse(substrings[k]);

                            nodeId = nodePrefix + counter.ToString().PadLeft(substrings.Length, '0');
                            var node = new Node(nodeId, nodalCrds, NumberNodalDof, SpatialDimension);
                            FeModel.Nodes.Add(nodeId, node);
                            counter++;
                            i++;
                        }
                        i++;
                    }
                }

                //Equidistant mesh of Nodes
                if (lines[i] == "Equidistant Node Mesh")
                {
                    i++;
                    while (i < lines.Length)
                    {
                        if (lines[i] == string.Empty) break;
                        substrings = lines[i].Split(delimiters);
                        nodePrefix = substrings[0];

                        switch (substrings.Length)
                        {
                            //Aequidistantes Knotennetz in 1D
                            case 4:
                                InputFound += "\nEquidistant Node Mesh in 1D";
                                crds[0] = double.Parse(substrings[1], CultureInfo.InvariantCulture);
                                xInterval = double.Parse(substrings[2], CultureInfo.InvariantCulture);
                                nNodesX = short.Parse(substrings[3], CultureInfo.InvariantCulture);

                                for (var k = 0; k < nNodesX; k++)
                                {
                                    nodeId = nodePrefix + k.ToString().PadLeft(2, '0');
                                    nodalCrds = new[] { crds[0] };
                                    var node = new Node(nodeId, nodalCrds, NumberNodalDof, SpatialDimension);
                                    FeModel.Nodes.Add(nodeId, node);
                                    crds[0] += xInterval;
                                }

                                i++;
                                break;
                            //Equidistant Node Mesh in 2D
                            case 7:
                                InputFound += "\nEquidistant Node Mesh in 2D";
                                crds = new double[3];
                                crds[0] = double.Parse(substrings[1], CultureInfo.InvariantCulture);
                                xInterval = double.Parse(substrings[2], CultureInfo.InvariantCulture);
                                nNodesX = short.Parse(substrings[3], CultureInfo.InvariantCulture);
                                crds[1] = double.Parse(substrings[4], CultureInfo.InvariantCulture);
                                yInterval = double.Parse(substrings[5], CultureInfo.InvariantCulture);
                                nNodesY = short.Parse(substrings[6], CultureInfo.InvariantCulture);

                                for (var k = 0; k < nNodesX; k++)
                                {
                                    var temp = crds[0];
                                    var idY = k.ToString().PadLeft(2, '0');
                                    for (var l = 0; l < nNodesY; l++)
                                    {
                                        var idX = l.ToString().PadLeft(2, '0');
                                        nodeId = nodePrefix + idX + idY;
                                        nodalCrds = new[] { crds[0], crds[1] };
                                        var node = new Node(nodeId, nodalCrds, NumberNodalDof, SpatialDimension);
                                        FeModel.Nodes.Add(nodeId, node);
                                        crds[0] += xInterval;
                                    }

                                    crds[1] += yInterval;
                                    crds[0] = temp;
                                }
                                i++;
                                break;
                            //Equidistant Node Mesh in 3D
                            case 10:
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
                                nNodesZ = short.Parse(substrings[9], CultureInfo.InvariantCulture);

                                for (var k = 0; k < nNodesZ; k++)
                                {
                                    var temp1 = crds[1];
                                    var idZ = k.ToString().PadLeft(2, '0');
                                    for (var l = 0; l < nNodesY; l++)
                                    {
                                        var temp0 = crds[0];
                                        var idY = l.ToString().PadLeft(2, '0');
                                        for (var m = 0; m < nNodesX; m++)
                                        {
                                            var idX = m.ToString().PadLeft(2, '0');
                                            nodeId = nodePrefix + idX + idY + idZ;
                                            nodalCrds = new[] { crds[0], crds[1], crds[2] };
                                            var node = new Node(nodeId, nodalCrds, NumberNodalDof, SpatialDimension);
                                            FeModel.Nodes.Add(nodeId, node);
                                            crds[0] += xInterval;
                                        }

                                        crds[0] = temp0;
                                        crds[1] += yInterval;
                                    }

                                    crds[1] = temp1;
                                    crds[2] += zInterval;
                                }

                                i++;
                                break;
                            default:
                                throw new ParseException(i + 3 + ": Equidistant Node Mesh");
                        }
                    }
                }

                //Variable Node Mesh
                if (lines[i] != "Variable Node Mesh") continue;
                {
                    if (lines[i] == string.Empty) break;
                    InputFound += "\nVariable Node Mesh";

                    i++;
                    while (i < lines.Length)
                    {
                        substrings = lines[i].Split(delimiters);
                        crds = new double[3];

                        var offset = new double[substrings.Length];
                        for (var k = 0; k < substrings.Length; k++)
                            offset[k] = double.Parse(substrings[k], CultureInfo.InvariantCulture);

                        substrings = lines[i + 1].Split(delimiters);
                        string idX, idY;
                        double coord0, coord1;
                        switch (substrings.Length)
                        {
                            case 2:
                                nodePrefix = substrings[0];
                                coord0 = double.Parse(substrings[1], CultureInfo.InvariantCulture);
                                for (var n = 0; n < offset.Length; n++)
                                {
                                    crds[0] = coord0 + offset[n];
                                    nodeId = nodePrefix + n.ToString().PadLeft(2, '0');
                                    nodalCrds = new[] { crds[0] };
                                    var node = new Node(nodeId, nodalCrds, NumberNodalDof, SpatialDimension);
                                    FeModel.Nodes.Add(nodeId, node);
                                }
                                break;
                            case 3:
                                nodePrefix = substrings[0];
                                coord0 = double.Parse(substrings[1], CultureInfo.InvariantCulture);
                                coord1 = double.Parse(substrings[2], CultureInfo.InvariantCulture);
                                for (var n = 0; n < offset.Length; n++)
                                {
                                    idY = n.ToString().PadLeft(2, '0');
                                    crds[1] = coord1 + offset[n];
                                    for (var m = 0; m < offset.Length; m++)
                                    {
                                        idX = m.ToString().PadLeft(2, '0');
                                        crds[0] = coord0 + offset[m];
                                        nodeId = nodePrefix + idX + idY;
                                        nodalCrds = new[] { crds[0], crds[1] };
                                        var node = new Node(nodeId, nodalCrds, NumberNodalDof, SpatialDimension);
                                        FeModel.Nodes.Add(nodeId, node);
                                    }
                                }
                                break;
                            case 4:
                                nodePrefix = substrings[0];
                                coord0 = double.Parse(substrings[1], CultureInfo.InvariantCulture);
                                coord1 = double.Parse(substrings[2], CultureInfo.InvariantCulture);
                                var coord2 = double.Parse(substrings[3], CultureInfo.InvariantCulture);
                                for (var n = 0; n < offset.Length; n++)
                                {
                                    var idZ = n.ToString().PadLeft(2, '0');
                                    var inkrement2 = coord2 + offset[n];
                                    for (var m = 0; m < offset.Length; m++)
                                    {
                                        idY = m.ToString().PadLeft(2, '0');
                                        var inkrement1 = coord1 + offset[m];
                                        for (var k = 0; k < offset.Length; k++)
                                        {
                                            crds = new double[3];
                                            crds[1] = inkrement1;
                                            crds[2] = inkrement2;
                                            idX = k.ToString().PadLeft(2, '0');
                                            crds[0] = coord0 + offset[k];
                                            nodeId = nodePrefix + idX + idY + idZ;
                                            nodalCrds = new[] { crds[0], crds[1], crds[2] };
                                            var node = new Node(nodeId, nodalCrds, NumberNodalDof, SpatialDimension);
                                            FeModel.Nodes.Add(nodeId, node);
                                        }
                                    }
                                }
                                break;
                            default:
                                throw new ParseException(i + 3 + ": Variable Node Mesh");
                        }
                        i += 2;
                        if (lines[i] == string.Empty) break;
                    }
                    break;
                }
            }
        }
    }
}