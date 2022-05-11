using FE_Analysis.Heat_Transfer.Model_Data;
using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Collections.Generic;

namespace FE_Analysis.Heat_Transfer.ModelDataRead
{
    public class ElementParser : FeParser
    {
        private AbstractElement element;
        private string elementId;
        private string materialId;
        private FeModel model;
        private string[] nodeIds = new string[8];
        private int nodesPerElement;
        private string[] substrings;

        // parsing a new model to be read from file
        public void ParseElements(string[] lines, FeModel feModel)
        {
            model = feModel;
            ParseElement2D2(lines);
            ParseElement2D3(lines);
            ParseElement2D4(lines);
            ParseElement3D8(lines);
        }

        private void ParseElement2D2(IReadOnlyList<string> lines)
        {
            nodesPerElement = 2;
            var delimiters = new[] { '\t' };

            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i] != "Elements2D2Nodes") continue;
                InputFound += "\nElements2D2Nodes";
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    switch (substrings.Length)
                    {
                        case 4:
                            {
                                elementId = substrings[0];
                                nodeIds = new string[8];
                                for (var k = 0; k < nodesPerElement; k++) nodeIds[k] = substrings[k + 1];

                                materialId = substrings[3];
                                element = new Element2D2(elementId, nodeIds, materialId, model);
                                model.Elements.Add(elementId, element);
                                i++;
                                break;
                            }
                        default:
                            throw new ParseException(i + 2 + ": Elements2D2Nodes, wrong number of parameters");
                    }
                } while (lines[i + 1].Length != 0);

                break;
            }
        }

        private void ParseElement2D3(IReadOnlyList<string> lines)
        {
            nodesPerElement = 3;
            var delimiters = new[] { '\t' };

            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i] != "Elements2D3Nodes") continue;
                InputFound += "\nElements2D3Nodes";
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    switch (substrings.Length)
                    {
                        case 5:
                            {
                                elementId = substrings[0];
                                nodeIds = new string[8];
                                for (var k = 0; k < nodesPerElement; k++) nodeIds[k] = substrings[k + 1];

                                materialId = substrings[4];

                                element = new Element2D3(elementId, nodeIds, materialId, model);
                                model.Elements.Add(elementId, element);
                                i++;
                                break;
                            }
                        default:
                            throw new ParseException(i + 2 + ": Elements2D3Nodes, wrong number of parameters");
                    }
                } while (lines[i + 1].Length != 0);

                break;
            }
        }

        private void ParseElement2D4(IReadOnlyList<string> lines)
        {
            nodesPerElement = 4;
            var delimiters = new[] { '\t' };

            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i] != "Elements2D4Nodes") continue;
                InputFound += "\nElements2D4Nodes";
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    switch (substrings.Length)
                    {
                        case 6:
                            {
                                elementId = substrings[0];
                                nodeIds = new string[8];
                                for (var k = 0; k < nodesPerElement; k++) nodeIds[k] = substrings[k + 1];

                                materialId = substrings[5];

                                element = new Element2D4(elementId, nodeIds, materialId, model);
                                model.Elements.Add(elementId, element);
                                i++;
                                break;
                            }
                        default:
                            throw new ParseException(i + 2 + ": Elements2D4Nodes, wrong number of parameters");
                    }
                } while (lines[i + 1].Length != 0);

                break;
            }
        }

        private void ParseElement3D8(IReadOnlyList<string> lines)
        {
            nodesPerElement = 8;
            var delimiters = new[] { '\t' };

            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i] != "Elements3D8Nodes") continue;
                InputFound += "\nElements3D8Nodes";
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    switch (substrings.Length)
                    {
                        case 10:
                            {
                                elementId = substrings[0];
                                nodeIds = new string[8];
                                for (var k = 0; k < nodesPerElement; k++) nodeIds[k] = substrings[k + 1];

                                materialId = substrings[9];

                                element = new Element3D8(elementId, nodeIds, materialId, model);
                                model.Elements.Add(elementId, element);
                                i++;
                                break;
                            }
                        default:
                            throw new ParseException(i + 2 + ": Elements3D8Nodes, wrong number of parameters");
                    }
                } while (lines[i + 1].Length != 0);

                break;
            }
        }
    }
}