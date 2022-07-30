using FE_Analysis.Structural_Analysis.Model_Data;
using FEALibrary.Model;
using System.Globalization;

namespace FE_Analysis.Structural_Analysis.ModelDataRead
{
    public class BoundaryConditionParser
    {
        private FeModel model;
        private readonly char[] delimiters = { '\t' };
        private string[] substrings;
        private string supportId;
        private string nodeId;
        private Support support;

        public void ParseBoundaryConditions(string[] lines, FeModel feModel)
        {
            model = feModel;

            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] != "Support") continue;
                FeParser.InputFound += "\nSupport";
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    if (substrings.Length < 7)
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
                                    conditions += Support.X_FIXED;
                                    break;
                                case "y":
                                    conditions += Support.Y_FIXED;
                                    break;
                                case "r":
                                    conditions += Support.R_FIXED;
                                    break;
                            }
                        }
                        var prescribed = new double[3];
                        if (substrings.Length > 3) prescribed[0] = double.Parse(substrings[3], CultureInfo.InvariantCulture);
                        if (substrings.Length > 4) prescribed[1] = double.Parse(substrings[4], CultureInfo.InvariantCulture);
                        if (substrings.Length > 5) prescribed[2] = double.Parse(substrings[5], CultureInfo.InvariantCulture);
                        support = new Support(nodeId, conditions, prescribed, model) { SupportId = supportId };
                        model.BoundaryConditions.Add(supportId, support);
                        i++;
                    }
                    else
                    {
                        throw new ParseException((i + 2) + ": Support" + supportId);
                    }
                } while (lines[i + 1].Length != 0);
                break;
            }
        }
    }
}