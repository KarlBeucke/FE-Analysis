using System.Globalization;
using FE_Analysis.Heat_Transfer.Model_Data;
using FEALibrary.Model;

namespace FE_Analysis.Heat_Transfer.ModelDataRead
{
    public class MaterialParser
    {
        private double densityConductivity;
        private double[] conductivity = new double[3];
        private Material material;
        private string materialId;
        private FeModel model;
        private string[] substrings;

        public void ParseMaterials(string[] lines, FeModel feModel)
        {
            model = feModel;
            var delimiters = new[] { '\t' };

            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] != "Material") continue;
                FeParser.InputFound += "\nMaterial";
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    materialId = substrings[0];
                    switch (substrings.Length)
                    {
                        case 2:
                           conductivity[0] = double.Parse(substrings[1], CultureInfo.InvariantCulture);
                            material = new Material(materialId, conductivity);
                            break;
                        case 3:
                            conductivity[0] = double.Parse(substrings[1], CultureInfo.InvariantCulture);
                            densityConductivity = double.Parse(substrings[2], CultureInfo.InvariantCulture);
                            material = new Material(materialId, conductivity, densityConductivity);
                            break;
                        case 4:
                            conductivity[0] = double.Parse(substrings[1], CultureInfo.InvariantCulture);
                            conductivity[1] = double.Parse(substrings[2], CultureInfo.InvariantCulture);
                            conductivity[2] = double.Parse(substrings[3], CultureInfo.InvariantCulture);
                            material = new Material(materialId, conductivity);
                            break;
                        case 5:
                            conductivity[0] = double.Parse(substrings[1], CultureInfo.InvariantCulture);
                            conductivity[1] = double.Parse(substrings[2], CultureInfo.InvariantCulture);
                            conductivity[2] = double.Parse(substrings[3], CultureInfo.InvariantCulture);
                            densityConductivity = double.Parse(substrings[4], CultureInfo.InvariantCulture);
                            material = new Material(materialId, conductivity, densityConductivity);
                            break;
                        default:
                            throw new ParseException(i + 2 + ": Material, wrong number of parameters");
                    }

                    model.Material.Add(materialId, material);
                    i++;
                } while (lines[i + 1].Length != 0);

                break;
            }
        }
    }
}