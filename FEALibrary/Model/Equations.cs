using System.Windows;

namespace FEALibrary.Model
{
    public class Equations
    {
        private readonly int dimension;

        // false : load prescribed
        private double[][] matrix; // system matrix A
        private int row;


        // ... Constructor ........................................................
        public Equations(int n)
        {
            dimension = n;
            Status = new bool[dimension];
            Profile = new int[dimension];
            Primal = new double[dimension];
            Dual = new double[dimension];
            Vector = new double[dimension];
            matrix = new double[dimension][];
            DiagonalMatrix = new double[dimension];
            for (row = 0; row < dimension; row++) Profile[row] = row;
        }

        // Properties
        public double[][] Matrix
        {
            get
            {
                if (matrix != null) return matrix;

                var systemgleichungen = MessageBox.Show("Systemgleichungen wurden noch nicht berechnet");
                _ = systemgleichungen;
                return null;
            }
            set => matrix = value;
        }

        public double[] DiagonalMatrix { get; set; }
        public double[] Primal { get; set; }
        public double[] Dual { get; set; }
        public double[] Vector { get; set; }
        public bool[] Status { get; set; }
        public int[] Profile { get; set; }

        // ... Set the profile vector for one element..............................
        public void SetProfile(int[] index)
        {
            foreach (var entry in index)
                foreach (var value in index)
                    if (Profile[entry] > value)
                        Profile[entry] = value;
        }

        // ... Set the status vector for one node .................................
        public void SetStatus(bool status, int index, double value)
        {
            Status[index] = status;
            if (status) Primal[index] = value;
        }

        // ... Allocate the row vectors of the system matrix .......................
        public void AllocateMatrix()
        {
            for (row = 0; row < dimension; row++) matrix[row] = new double[row - Profile[row] + 1];
        }

        // ... initialize system matrix ............................................
        public void InitializeMatrix()
        {
            foreach (var rowRef in matrix)
                for (var col = 0; col < rowRef.Length; col++)
                    rowRef[col] = 0;
        }

        // ... Read/write a coefficient in the system matrix ......................
        public double GetValue(int i, int m)
        {
            return matrix[i][m - Profile[i]];
        }

        public void SetValue(int i, int m, double value)
        {
            matrix[i][m - Profile[i]] = value;
        }

        public void AddValue(int i, int m, double value)
        {
            matrix[i][m - Profile[i]] += value;
        }

        // ... addSubmatrix() .....................................................
        public void AddMatrix(int[] index, double[,] elementMatrix)
        {
            for (var k = 0; k < index.Length; k++)
                for (var m = 0; m < index.Length; m++)
                    if (index[m] <= index[k])
                        AddValue(index[k], index[m], elementMatrix[k, m]);
        }

        // ... addDiagonalSubmatrix() ...............................................
        public void AddDiagonalMatrix(int[] index, double[] elementMatrix)
        {
            for (var k = 0; k < index.Length; k++)
                DiagonalMatrix[index[k]] += elementMatrix[k];
        }

        // ... addVector() .....................................................
        public void AddVector(int[] index, double[] subvector)
        {
            for (var k = 0; k < subvector.Length; k++)
                Vector[index[k]] += subvector[k];
        }
    }
}