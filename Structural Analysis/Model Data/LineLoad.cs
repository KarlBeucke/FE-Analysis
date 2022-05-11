﻿using FEALibrary.Model;
using FEALibrary.Model.abstractClasses;

namespace FE_Analysis.Structural_Analysis.Model_Data
{
    public class LineLoad : AbstractLineLoad
    {
        // ... Constructors ........................................................
        public LineLoad(string elementId, double p1X, double p2X, double p1Y, double p2Y)
        {
            ElementId = elementId;
            Intensity = new double[4]; // 2 nodes, 2 dimensions
            Intensity[0] = p1X;
            Intensity[1] = p2X;
            Intensity[2] = p1Y;
            Intensity[3] = p2Y;
        }

        public LineLoad(string elementId, double p1X, double p2X, double p1Y, double p2Y,
            bool inElementCoordinateSystem)
        {
            ElementId = elementId;
            Intensity = new double[4]; // 2 nodes, 2 dimensions
            Intensity[0] = p1X;
            Intensity[1] = p2X;
            Intensity[2] = p1Y;
            Intensity[3] = p2Y;
            InElementCoordinateSystem = inElementCoordinateSystem;
        }

        public override double[] ComputeLoadVector()
        {
            var balken = (Beam)Element;
            // inElementCoordinateSystem is false
            return balken.ComputeLoadVector(this, false);
        }

        public double[] ComputeLocalLoadVector()
        {
            var balken = (Beam)Element;
            // inElementCoordinateSystem is true
            return balken.ComputeLoadVector(this, true);
        }

        // useful for GAUSS integration
        public double GetXIntensity(double z)
        {
            if (z < 0 || z > 1)
                throw new ModelException("LineLoad on element:" + ElementId + "out of coordinate range 0 <= z <= 1");
            return Intensity[0] * (1 - z) + Intensity[2] * z;
        }

        public double GetYIntensity(double z)
        {
            if (z < 0 || z > 1)
                throw new ModelException("LineLoad on element:" + ElementId + "out of coordinate range 0 <= z <= 1");
            return Intensity[1] * (1 - z) + Intensity[3] * z;
        }
    }
}