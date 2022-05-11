using FEALibrary.Model.abstractClasses;

namespace FE_Analysis.Heat_Transfer.Model_Data
{
    public class TimeDependentNodeLoad : AbstractTimeDependentNodeLoad
    {
        public TimeDependentNodeLoad(string nodeId, bool file)
        {
            NodeId = nodeId;
            File = file;
            VariationType = 0;
        }

        public TimeDependentNodeLoad(string nodeId, double constantTemperature)
        {
            NodeId = nodeId;
            ConstantTemperature = constantTemperature;
            VariationType = 1;
        }

        public TimeDependentNodeLoad(string nodeId,
            double amplitude, double frequency, double phaseAngle)
        {
            NodeId = nodeId;
            Amplitude = amplitude;
            Frequency = frequency;
            PhaseAngle = phaseAngle;
            VariationType = 2;
        }

        public TimeDependentNodeLoad(string nodeId, double[] interval)
        {
            NodeId = nodeId;
            Interval = interval;
            VariationType = 3;
        }
    }
}