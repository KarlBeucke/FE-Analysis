using FEALibrary.Model;

namespace FE_Analysis.Elasticity.ModelDataRead
{
    public class ElasticityParser : FeParser
    {
        public static BoundaryConditionParser parseElasticityBoundaryConditions;
        private FeModel model;
        private ElementParser parseElasticityElements;
        private LoadParser parseElasticityLoads;
        private MaterialParser parseElasticityMaterial;

        // Eingabedaten für eine Elastizitätsberechnung aus Detei lesen
        public void ParseElasticity(string[] lines, FeModel feModell)
        {
            model = feModell;
            parseElasticityElements = new ElementParser();
            parseElasticityElements.ParseElements(lines, model);

            parseElasticityMaterial = new MaterialParser();
            parseElasticityMaterial.ParseMaterials(lines, model);

            parseElasticityLoads = new LoadParser();
            parseElasticityLoads.ParseLoads(lines, model);

            parseElasticityBoundaryConditions = new BoundaryConditionParser();
            parseElasticityBoundaryConditions.ParseBoundaryConditions(lines, model);
        }
    }
}