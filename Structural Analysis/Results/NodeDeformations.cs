namespace FE_Analysis.Structural_Analysis.Results
{
    public class NodeDeformations
    {
        public NodeDeformations(double time, double deformationX, double deformationY, double rotation,
            double accelerationX, double accelerationY, double accelerationPhi)
        {
            Time = time;
            DeformationX = deformationX;
            DeformationY = deformationY;
            Rotation = rotation;
            AccelerationX = accelerationX;
            AccelerationY = accelerationY;
            AccelerationPhi = accelerationPhi;
        }

        public NodeDeformations(double time, double deformationX, double deformationY,
            double accelerationX, double accelerationY)
        {
            Time = time;
            DeformationX = deformationX;
            DeformationY = deformationY;
            AccelerationX = accelerationX;
            AccelerationY = accelerationY;
        }

        public NodeDeformations(string node, double deformationX, double deformationY, double rotation,
            double accelerationX, double accelerationY, double accelerationPhi)
        {
            Node = node;
            DeformationX = deformationX;
            DeformationY = deformationY;
            Rotation = rotation;
            AccelerationX = accelerationX;
            AccelerationY = accelerationY;
            AccelerationPhi = accelerationPhi;
        }

        public NodeDeformations(string node, double deformationX, double deformationY,
            double accelerationX, double accelerationY)
        {
            Node = node;
            DeformationX = deformationX;
            DeformationY = deformationY;
            AccelerationX = accelerationX;
            AccelerationY = accelerationY;
        }

        public double Time { get; set; }
        public string Node { get; set; }
        public double DeformationX { get; set; }
        public double DeformationY { get; set; }
        public double Rotation { get; set; }
        public double AccelerationX { get; set; }
        public double AccelerationY { get; set; }
        public double AccelerationPhi { get; set; }
    }
}