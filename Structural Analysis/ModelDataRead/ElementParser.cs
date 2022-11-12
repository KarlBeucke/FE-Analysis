using FE_Analysis.Structural_Analysis.Model_Data;
using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;
using System.Collections.Generic;
using System.Globalization;

namespace FE_Analysis.Structural_Analysis.ModelDataRead;

public class ElementParser
{
    private readonly char[] delimiters = { '\t' };
    private AbstractElement element;
    private string elementId;
    private FeModel model;
    private string[] nodeIds;
    private int nodesPerElement;
    private string[] substrings;

    // parsing a new model to be read from file
    public void ParseElements(string[] lines, FeModel feModel)
    {
        model = feModel;
        ParseTrusses(lines);
        ParseBeams(lines);
        ParseSpringElements(lines);
        ParseBeamsHinged(lines);
        ParseCrossSections(lines);
    }

    private void ParseTrusses(IReadOnlyList<string> lines)
    {
        nodesPerElement = 2;
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "Truss") continue;
            FeParser.InputFound += "\nTruss";
            do
            {
                substrings = lines[i + 1].Split(delimiters);
                switch (substrings.Length)
                {
                    case 5:
                        {
                            elementId = substrings[0];
                            nodeIds = new string[nodesPerElement];
                            for (var k = 0; k < nodesPerElement; k++) nodeIds[k] = substrings[k + 1];
                            var querschnittId = substrings[3];
                            var materialId = substrings[4];
                            element = new Truss(nodeIds, querschnittId, materialId, model)
                            {
                                ElementId = elementId
                            };
                            model.Elements.Add(elementId, element);
                            i++;
                            break;
                        }
                    default:
                        throw new ParseException(i + 2 + ": Truss, wrong number of parameters");
                }
            } while (lines[i + 1].Length != 0);

            break;
        }
    }

    private void ParseBeams(IReadOnlyList<string> lines)
    {
        nodesPerElement = 2;
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "Beam") continue;
            FeParser.InputFound += "\nBeam";
            do
            {
                substrings = lines[i + 1].Split(delimiters);
                switch (substrings.Length)
                {
                    case 5:
                        {
                            elementId = substrings[0];
                            nodeIds = new string[nodesPerElement];
                            for (var k = 0; k < nodesPerElement; k++) nodeIds[k] = substrings[k + 1];
                            var querschnittId = substrings[3];
                            var materialId = substrings[4];
                            element = new Beam(nodeIds, querschnittId, materialId, model)
                            {
                                ElementId = elementId
                            };
                            model.Elements.Add(elementId, element);
                            i++;
                            break;
                        }
                    default:
                        throw new ParseException(i + 2 + ": Beam, wrong number of parameters");
                }
            } while (lines[i + 1].Length != 0);

            break;
        }
    }

    private void ParseBeamsHinged(IReadOnlyList<string> lines)
    {
        nodesPerElement = 2;
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "BeamHinged") continue;
            FeParser.InputFound += "\nBeamHinged";
            do
            {
                substrings = lines[i + 1].Split(delimiters);

                switch (substrings.Length)
                {
                    case 6:
                        {
                            elementId = substrings[0];
                            nodeIds = new string[nodesPerElement];
                            for (var k = 0; k < nodesPerElement; k++) nodeIds[k] = substrings[k + 1];
                            var querschnittId = substrings[3];
                            var materialId = substrings[4];
                            int type;
                            switch (short.Parse(substrings[5]))
                            {
                                //if (string.Equals(gelenk, "F", StringComparison.OrdinalIgnoreCase)) type = 1;
                                //else if (string.Equals(gelenk, "S", StringComparison.OrdinalIgnoreCase)) type = 2;
                                case 1:
                                    type = 1;
                                    break;
                                case 2:
                                    type = 2;
                                    break;
                                default:
                                    throw new ParseException(i + 2 + ": BeamHinged, wrong hinge type");
                            }

                            element = new BeamHinged(nodeIds, materialId, querschnittId, model, type)
                            {
                                ElementId = elementId
                            };
                            model.Elements.Add(elementId, element);
                            i++;
                            break;
                        }
                    default:
                        throw new ParseException(i + 2 + ": BeamHinged, wrong number of parameters");
                }
            } while (lines[i + 1].Length != 0);

            break;
        }
    }

    private void ParseSpringElements(IReadOnlyList<string> lines)
    {
        nodesPerElement = 1;
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "SpringElement") continue;
            FeParser.InputFound += "\nSpringElement";
            do
            {
                substrings = lines[i + 1].Split(delimiters);
                switch (substrings.Length)
                {
                    case 3:
                        {
                            elementId = substrings[0];
                            nodeIds = new string[nodesPerElement];
                            nodeIds[0] = substrings[1];
                            var materialId = substrings[2];
                            var federLager = new SpringElement(nodeIds, materialId, model)
                            {
                                ElementId = elementId
                            };
                            model.Elements.Add(elementId, federLager);
                            i++;
                            break;
                        }
                    default:
                        throw new ParseException(i + 2 + ": SpringElement, wrong number of parameters");
                }
            } while (lines[i + 1].Length != 0);

            break;
        }
    }

    private void ParseCrossSections(IReadOnlyList<string> lines)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "CrossSection") continue;
            FeParser.InputFound += "\nCrossSection";
            do
            {
                substrings = lines[i + 1].Split(delimiters);
                switch (substrings.Length)
                {
                    case 2:
                        {
                            var querschnittId = substrings[0];
                            var fläche = double.Parse(substrings[1], CultureInfo.InvariantCulture);
                            var querschnitt = new CrossSection(fläche) { CrossSectionId = querschnittId };
                            model.CrossSection.Add(querschnittId, querschnitt);
                            i++;
                            break;
                        }
                    case 3:
                        {
                            var querschnittId = substrings[0];
                            var flaeche = double.Parse(substrings[1], CultureInfo.InvariantCulture);
                            var ixx = double.Parse(substrings[2], CultureInfo.InvariantCulture);
                            var querschnitt = new CrossSection(flaeche, ixx) { CrossSectionId = querschnittId };
                            model.CrossSection.Add(querschnittId, querschnitt);
                            i++;
                            break;
                        }
                    default:
                        throw new ParseException(i + 2 + ": CrossSection, wrong number of parameters");
                }
            } while (lines[i + 1].Length != 0);

            break;
        }
    }
}