using FE_Analysis.Elasticity.ModelData;
using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Collections.Generic;

namespace FE_Analysis.Elasticity.ModelDataRead
{
    public class ElementParser
    {
        private AbstractElement element;
        private string elementId;
        private FeModel model;
        private string[] nodeIds;
        private string[] substrings;

        // parsing a new model to be read from file
        public void ParseElements(string[] lines, FeModel feModel)
        {
            model = feModel;
            ParseElement2D3(lines);
            ParseElement3D8(lines);
            ParseElement3D8Mesh(lines);
            ParseCrossSections(lines);
        }

        private void ParseElement2D3(IReadOnlyList<string> lines)
        {
            const int nodesPerElement = 3;
            var delimiters = new[] { '\t' };

            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i] != "Element2D3") continue;
                FeParser.InputFound += "\nElement2D3";
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    if (substrings.Length == 6)
                    {
                        elementId = substrings[0];
                        nodeIds = new string[nodesPerElement];
                        for (var k = 0; k < nodesPerElement; k++) nodeIds[k] = substrings[k + 1];

                        var crossSectionId = substrings[4];
                        var materialId = substrings[5];
                        element = new Element2D3(nodeIds, crossSectionId, materialId, model) { ElementId = elementId };
                        model.Elements.Add(elementId, element);
                        i++;
                    }
                    else
                    {
                        throw new ParseException(i + 1 + ": Element2D3 requires 6 input parameters");
                    }
                } while (lines[i + 1].Length != 0);

                break;
            }
        }

        private void ParseElement3D8(IReadOnlyList<string> lines)
        {
            const int nodesPerElement = 8;
            var delimiters = new[] { '\t' };

            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i] != "Element3D8") continue;
                FeParser.InputFound += "\nElement3D8";
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    if (substrings.Length == 10)
                    {
                        elementId = substrings[0];
                        nodeIds = new string[nodesPerElement];
                        for (var k = 0; k < nodesPerElement; k++) nodeIds[k] = substrings[k + 1];
                        var materialId = substrings[9];
                        element = new Element3D8(nodeIds, materialId, model) { ElementId = elementId };
                        model.Elements.Add(elementId, element);
                        i++;
                    }
                    else
                    {
                        throw new ParseException(i + 1 + ": Element3D8 requires 10 input parameters");
                    }
                } while (lines[i + 1].Length != 0);

                break;
            }
        }

        private void ParseElement3D8Mesh(IReadOnlyList<string> lines)
        {
            const int nodesPerElement = 8;
            var delimiters = new[] { '\t' };

            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i] != "3D8ElementMesh") continue;
                FeParser.InputFound += "\n3D8ElementMesh";
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    if (substrings.Length != 4)
                        throw new ParseException("wrong number of parameters for element input:\n"
                                                 + "must be equal to 4 for element name, node mesh name,"
                                                 + "number of intervals and element material");
                    var initial = substrings[0];
                    var eNodeName = substrings[1];
                    int nIntervals = short.Parse(substrings[2]);
                    var eMaterial = substrings[3];


                    for (var n = 0; n < nIntervals; n++)
                    {
                        var idX = n.ToString().PadLeft(2, '0');
                        var idXp = (n + 1).ToString().PadLeft(2, '0');
                        for (var m = 0; m < nIntervals; m++)
                        {
                            var idY = m.ToString().PadLeft(2, '0');
                            var idYp = (m + 1).ToString().PadLeft(2, '0');
                            for (var k = 0; k < nIntervals; k++)
                            {
                                var idZ = k.ToString().PadLeft(2, '0');
                                var idZp = (k + 1).ToString().PadLeft(2, '0');
                                var eNode = new string[nodesPerElement];
                                var elementName = initial + idX + idY + idZ;
                                if (model.Elements.TryGetValue(elementName, out element))
                                    throw new ParseException("Element \"" + elementName + "\" already exists.");
                                eNode[0] = eNodeName + idX + idY + idZ;
                                eNode[1] = eNodeName + idXp + idY + idZ;
                                eNode[2] = eNodeName + idXp + idYp + idZ;
                                eNode[3] = eNodeName + idX + idYp + idZ;
                                eNode[4] = eNodeName + idX + idY + idZp;
                                eNode[5] = eNodeName + idXp + idY + idZp;
                                eNode[6] = eNodeName + idXp + idYp + idZp;
                                eNode[7] = eNodeName + idX + idYp + idZp;
                                element = new Element3D8(eNode, eMaterial, model) { ElementId = elementName };
                                model.Elements.Add(elementName, element);
                            }
                        }
                    }
                } while (lines[i + 2].Length != 0);

                break;
            }
        }

        private void ParseCrossSections(IReadOnlyList<string> lines)
        {
            var delimiters = new[] { '\t' };

            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i] != "CrossSections") continue;
                FeParser.InputFound += "\nCrossSections";
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    var crossSectionId = substrings[0];
                    var thickness = double.Parse(substrings[1]);

                    var crossSection = new CrossSection(thickness) { CrossSectionId = crossSectionId };
                    model.CrossSection.Add(crossSectionId, crossSection);
                    i++;
                } while (lines[i + 1].Length != 0);

                break;
            }
        }
    }
}