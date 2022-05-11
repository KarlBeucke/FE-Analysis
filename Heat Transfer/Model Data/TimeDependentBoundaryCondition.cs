using FEALibrary.Model.abstractClasses;

namespace FE_Analysis.Heat_Transfer.Model_Data
{
    public class TimeDependentBoundaryCondition : AbstractTimeDependentBoundaryCondition
    {
        public TimeDependentBoundaryCondition(string nodeId, bool file)
        {
            NodeId = nodeId;
            Restrained = new bool[1];
            Restrained[0] = true;
            File = file;
            VariationType = 0;
        }

        public TimeDependentBoundaryCondition(string nodeId, double constantTemperature)
        {
            NodeId = nodeId;
            Restrained = new bool[1];
            Restrained[0] = true;
            ConstantTemperature = constantTemperature;
            VariationType = 3;
        }

        public TimeDependentBoundaryCondition(string nodeId, double[] interval)
        {
            NodeId = nodeId;
            Restrained = new bool[1];
            Restrained[0] = true;
            Interval = interval;
            VariationType = 1;
        }

        public TimeDependentBoundaryCondition(string nodeId,
            double amplitude, double frequency, double phaseAngle)
        {
            NodeId = nodeId;
            Restrained = new bool[1];
            Restrained[0] = true;
            Amplitude = amplitude;
            Frequency = frequency;
            PhaseAngle = phaseAngle;
            VariationType = 2;
        }
    }
}