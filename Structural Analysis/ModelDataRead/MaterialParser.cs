using FE_Analysis.Structural_Analysis.Model_Data;
using FEALibrary.Model;
using static System.Globalization.CultureInfo;

namespace FE_Analysis.Structural_Analysis.ModelDataRead;

internal class MaterialParser
{
    private readonly char[] delimiters = { '\t' };
    private double eModulus;
    private double kx, ky, kphi;
    private double mass;
    private Material material;
    private string materialId;
    private FeModel model;
    private double poisson;
    private string[] substrings;

    public void ParseMaterials(string[] lines, FeModel feModel)
    {
        model = feModel;

        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i] != "Material") continue;
            FeParser.InputFound += "\nMaterial";
            do
            {
                substrings = lines[i + 1].Split(delimiters);
                if (substrings.Length > 1 && substrings.Length < 6)
                {
                    materialId = substrings[0];
                    switch (substrings.Length)
                    {
                        case 2:
                            eModulus = double.Parse(substrings[1], InvariantCulture);
                            material = new Material(eModulus)
                            {
                                MaterialId = materialId
                            };
                            model.Material.Add(materialId, material);
                            break;
                        case 3:
                            eModulus = double.Parse(substrings[1], InvariantCulture);
                            poisson = double.Parse(substrings[2], InvariantCulture);
                            material = new Material(eModulus, poisson)
                            {
                                MaterialId = materialId
                            };
                            model.Material.Add(materialId, material);
                            break;
                        case 4:
                            eModulus = double.Parse(substrings[1], InvariantCulture);
                            poisson = double.Parse(substrings[2], InvariantCulture);
                            mass = double.Parse(substrings[3], InvariantCulture);
                            material = new Material(eModulus, poisson, mass)
                            {
                                MaterialId = materialId
                            };
                            model.Material.Add(materialId, material);
                            break;
                        case 5:
                            {
                                kx = double.Parse(substrings[2], InvariantCulture);
                                ky = double.Parse(substrings[3], InvariantCulture);
                                kphi = double.Parse(substrings[4], InvariantCulture);
                                material = new Material(true, kx, ky, kphi)
                                {
                                    MaterialId = materialId
                                };
                                model.Material.Add(materialId, material);
                                break;
                            }
                    }

                    i++;
                }
                else
                {
                    throw new ParseException(i + 2 + ": Material " + materialId);
                }
            } while (lines[i + 1].Length != 0);

            break;
        }
    }
}