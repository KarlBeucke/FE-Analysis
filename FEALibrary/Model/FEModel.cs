using FEALibrary.Model.abstractClasses;
using System.Collections.Generic;

namespace FEALibrary.Model
{
    public class FeModel
    {
        public string ModelId { get; set; }
        public int SpatialDimension { get; set; }
        public bool Solved { get; set; } = false;
        public bool Eigen { get; set; } = false;
        public bool TimeIntegration { get; set; } = false;

        public Dictionary<string, Node> Nodes { get; set; }
        public Dictionary<string, AbstractElement> Elements { get; set; }
        public Dictionary<string, AbstractMaterial> Material { get; set; }
        public Dictionary<string, CrossSection> CrossSection { get; set; }
        public Dictionary<string, AbstractLoad> Loads { get; set; }
        public Dictionary<string, AbstractLineLoad> LineLoads { get; set; }
        public Dictionary<string, AbstractElementLoad> ElementLoads { get; set; }
        public Dictionary<string, AbstractElementLoad> PointLoads { get; set; }
        public Dictionary<string, AbstractBoundaryCondition> BoundaryConditions { get; set; }
        public Eigenstates Eigenstate { get; set; }
        public AbstractTimeintegration Timeintegration { get; set; }
        public Dictionary<string, AbstractTimeDependentNodeLoad> TimeDependentNodeLoads { get; set; }
        public Dictionary<string, AbstractTimeDependentElementLoad> TimeDependentElementLoads { get; set; }
        public Dictionary<string, AbstractTimeDependentBoundaryCondition> TimeDependentBoundaryConditions { get; set; }

        public FeModel(string modelId, int spatialDimension)
        {
            SpatialDimension = spatialDimension;
            ModelId = modelId;

            Nodes = new Dictionary<string, Node>();
            Elements = new Dictionary<string, AbstractElement>();
            Material = new Dictionary<string, AbstractMaterial>();
            CrossSection = new Dictionary<string, CrossSection>();
            Loads = new Dictionary<string, AbstractLoad>();
            LineLoads = new Dictionary<string, AbstractLineLoad>();
            ElementLoads = new Dictionary<string, AbstractElementLoad>();
            PointLoads = new Dictionary<string, AbstractElementLoad>();
            BoundaryConditions = new Dictionary<string, AbstractBoundaryCondition>();
            TimeDependentNodeLoads = new Dictionary<string, AbstractTimeDependentNodeLoad>();
            TimeDependentElementLoads = new Dictionary<string, AbstractTimeDependentElementLoad>();
            TimeDependentBoundaryConditions = new Dictionary<string, AbstractTimeDependentBoundaryCondition>();
        }
    }
}