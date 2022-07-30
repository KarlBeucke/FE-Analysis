using FE_Analysis.Heat_Transfer.Model_Data;
using FEALibrary.Model;
using System.Globalization;

namespace FE_Analysis.Heat_Transfer.ModelDataRead
{
    internal class BoundaryConditionParser
    {
        private FeModel model;
        private string[] substrings;
        private string supportId;
        private string nodeId;
        private BoundaryCondition boundaryCondition;

        public void ParseBoundaryConditions(string[] lines, FeModel feModel)
        {
            model = feModel;
            var delimiters = new[] { '\t' };

            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] != "BoundaryConditions") continue;
                FeParser.InputFound += "\nBoundaryConditions";
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    switch (substrings.Length)
                    {
                        case 3:
                            {
                                supportId = substrings[0];
                                nodeId = substrings[1];
                                var pre = double.Parse(substrings[2], CultureInfo.InvariantCulture);
                                boundaryCondition = new BoundaryCondition(supportId, nodeId, pre);
                                model.BoundaryConditions.Add(supportId, boundaryCondition);
                                i++;
                                break;
                            }
                        default:
                            throw new ParseException((i + 2) + ": BoundaryCondition, wrong number of parameters");
                    }
                } while (lines[i + 1].Length != 0);
                break;
            }
        }
    }
}
