using System.Globalization;
using FE_Analysis.Elasticity.ModelData;
using FEALibrary.Model;
using static System.Globalization.CultureInfo;

namespace FE_Analysis.Elasticity.ModelDataRead
{
    public class MaterialParser
    {
        private double eModul;
        private Material material;
        private string materialId;
        private FeModel model;
        private string[] substrings;

        public static double GModul { get; set; }
        public static double Poisson { get; set; }

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
                        case 3:
                            eModul = double.Parse(substrings[1], InvariantCulture);
                            Poisson = double.Parse(substrings[2], InvariantCulture);
                            material = new Material(eModul, Poisson);
                            break;
                        case 4:
                            eModul = double.Parse(substrings[1], InvariantCulture);
                            Poisson = double.Parse(substrings[2], InvariantCulture);
                            GModul = double.Parse(substrings[3], InvariantCulture);
                            material = new Material(eModul, Poisson, GModul);
                            break;
                        default:
                            throw new ParseException(i + 1 + ": Material erfordert 3 oder 4 Eingabeparameter");
                    }

                    material.MaterialId = materialId;
                    model.Material.Add(materialId, material);
                    i++;
                } while (lines[i + 1].Length != 0);

                break;
            }
        }
    }
}