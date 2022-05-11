namespace FEALibrary.Utils
{
    // public static class MatrixAlgebra
    //
    // public static double[] Clone(double[] vector)
    // public static void Clear(double[] vector)
    // public static void Clear(double[][] matrix)
    // public static void Mult(double[] vector, double scalar) 
    // public static void Add(double[] result, double[] vec1, double[] vec2)
    // public static void Subtract(double[] vec1, double[] vec2)
    // public static void Mult(double[] result, double[][] matrix, double[] vector)
    // public static double[] Mult(double scalar, double[][] matrix, double[] vector)
    // public static double[][] MultTransposed(double scalar, double[] vector)
    // public static double[] MultTransposed( double[][] matrix, double[] vector)
    // public static double[] MultTransposed(double scalar, double[][] matrix, double[] vector)
    // public static double[] Mult(double[][] matrix, double[] vector, int[] profile)
    // public static void Mult(double[] result, double[][] matrix, double[] vector, boolean[] status, int[] profile)
    // public static void MultAdd(double[] result, double[][] matrix, double[] vector, boolean[] status, int[] profile)
    // public static void MultSub(double[] result, double[][] matrix, double[] vector, boolean[] status, int[] profile)
    // public static double[] Mult(double[][] matrix, double[] vector, boolean[] status, int[] profile)
    // public static double[] MultUU(double[][] matrix, double[] vector, boolean[] status, int[] profile) {
    // public static double[] MultUL(double[][] matrix, double[] vector, boolean[] status, int[] profile) {
    // public static double[][] Add(double[][] mat1, double[][] mat2)
    // public static void Subtract(double[][] result, double[][] mat1, double[][] mat2)
    // public static void Mult(double[][] result, double[][] mat1, double[][] mat2)
    // public static double[][] Mult(double[][] mat1, double[][] mat2)
    // public static double[][] Mult(double factor, double[][] mat1, double[][] mat2)
    // public static double[][] MultMatrixTransposed(double scalar, double[][] mat1, double[][] mat2)
    // public static void MultAddMatrixTransposed(double[][] result, double scalar, double[][] mat1, double[][] mat2)
    // public static void MultAddMatrix(double[][] result, double[][] mat1, double[][] mat2)
    // public static double[][] MultTransposedMatrix( double[][] mat1, double[][] mat2)
    // public static double[][] MultTransposedMatrix(double scalar, double[][] mat1, double[][] mat2)
    // public static void ExtractSubMatrix(double[][] matrix, double[][] subMatrix, int[] indices)
    // public static void ExtractSubMatrix(double[][] matrix, double[][] subMatrix, int[] rows, int[] cols)
    // public static void PrintVector(String s, double v[])
    // public static void PrintVector(String s, int v[])
    // public static void PrintVector(String s, boolean v[])
    // public static void PrintVector(String s, String v[])
    // public static void PrintMatrix(String s, double m[][])

    public static class MatrixAlgebra
    {
        public static double[] Clone(double[] vector)
        {
            var nvec = new double[vector.Length];
            for (var i = 0; i < vector.Length; i++)
                nvec[i] = vector[i];
            return nvec;
        }
        public static void Clear(double[] vector)
        {
            for (var i = 0; i < vector.Length; i++) vector[i] = 0;
        }
        public static void Clear(double[][] matrix)
        {
            foreach (var rowRef in matrix)
                for (var j = 0; j < rowRef.Length; j++)
                    rowRef[j] = 0;
        }
        public static void Clear(double[,] matrix)
        {
            var zeilen = matrix.GetLength(0);
            var spalten = matrix.GetLength(1);
            for (var i = 0; i < zeilen; i++)
            for (var j = 0; j < spalten; j++)
                matrix[i, j] = 0;
        }
        
        // vector = vector * scalar
        public static void Mult(double[] vector, double scalar)
        {
            for (var i = 0; i < vector.Length; i++) vector[i] = vector[i] * scalar;
        }
        
        // result = vec1 + vec2
        public static void Add(double[] result, double[] vec1, double[] vec2)
        {
            if (vec1.Length != vec2.Length)
                throw new AlgebraicException("Add: invalid vector dimensions");
            for (var i = 0; i < vec1.Length; i++) result[i] = vec1[i] + vec2[i];
        }
        
        // vec1 = vec1 - vec2
        public static void Subtract(double[] vec1, double[] vec2)
        {
            if (vec1.Length != vec2.Length)
                throw new AlgebraicException("Subtract: invalid vector dimensions");
            for (var i = 0; i < vec1.Length; i++) vec1[i] -= vec2[i];
        }

        // result = matrix * vector (with existing result vector)
        public static void Mult(double[] result, double[][] matrix, double[] vector)
        {
            if (matrix[0].Length != vector.Length)
                throw new AlgebraicException("Mult: invalid matrix and vector dimensions \n\t["
                                             + matrix[0].Length + "]x[" + vector.Length + "]");
            for (var i = 0; i < matrix.Length; i++)
            for (var j = 0; j < matrix[i].Length; j++)
                result[i] += matrix[i][j] * vector[j];
        }

        public static void Mult(double[] result, double[,] matrix, double[] vector)
        {
            if (matrix.GetLength(1) != vector.Length)
                throw new AlgebraicException("Mult: invalid matrix and vector dimensions \n\t["
                                             + matrix.GetLength(1) + "]x[" + vector.Length + "]");
            for (var i = 0; i < matrix.GetLength(0); i++)
            for (var j = 0; j < matrix.GetLength(1); j++)
                result[i] += matrix[i, j] * vector[j];
        }

        // result = matrix * vector (generate and return new result vector)
        public static double[] Mult(double[][] matrix, double[] vector)
        {
            if (matrix[0].Length != vector.Length)
                throw new AlgebraicException("Mult: invalid matrix and vector dimensions \n\t["
                                             + matrix[0].Length + "]x[" + vector.Length + "]");
            var result = new double[matrix.Length];
            for (var i = 0; i < matrix.Length; i++)
            for (var j = 0; j < matrix[i].Length; j++)
                result[i] += matrix[i][j] * vector[j];
            return result;
        }

        public static double[] Mult(double[,] matrix, double[] vector)
        {
            if (matrix.GetLength(1) != vector.Length)
                throw new AlgebraicException("Mult: invalid matrix and vector dimensions \n\t["
                                             + matrix.GetLength(1) + "]x[" + vector.Length + "]");
            var result = new double[matrix.GetLength(0)];
            for (var i = 0; i < matrix.GetLength(0); i++)
            for (var j = 0; j < matrix.GetLength(1); j++)
                result[i] += matrix[i, j] * vector[j];
            return result;
        }

        // result = scalar * matrix * vector (generate and return new result vector)   
        public static double[] Mult(double scalar, double[][] matrix, double[] vector)
        {
            if (vector.Length != matrix[0].Length)
                throw new AlgebraicException
                ("Mult: invalid matrix and vector dimensions \n\t[" + matrix[0].Length + "]x[" + vector.Length +
                 "]");
            var result = new double[matrix.Length];
            for (var row = 0; row < matrix.Length; row++)
            {
                result[row] = 0;
                for (var col = 0; col < vector.Length; col++) result[row] += scalar * matrix[row][col] * vector[col];
            }

            return result;
        }

        // result = scalar * vector(transposed) * vector (generate and return new result vector)   
        public static double[][] MultTransposed(double scalar, double[] vector)
        {
            var result = new double[vector.Length][];
            for (var row = 0; row < vector.Length; row++)
            for (var col = 0; col < vector.Length; col++)
                result[row][col] = scalar * vector[row] * vector[col];

            return result;
        }

        public static double[,] MultTransposedRect(double scalar, double[] vector)
        {
            var result = new double[vector.Length, vector.Length];
            for (var row = 0; row < vector.Length; row++)
            for (var col = 0; col < vector.Length; col++)
                result[row, col] = scalar * vector[row] * vector[col];

            return result;
        }

        // result = matrix(transposed) * vector (generate and return new result vector)   
        public static double[] MultTransposed(double[][] matrix, double[] vector)
        {
            if (vector.Length != matrix.Length)
                throw new AlgebraicException
                ("MultTransposed: invalid matrix and vector dimensions \n\t[" + matrix[0].Length + "]x[" +
                 vector.Length + "]");
            var result = new double[matrix[0].Length];
            for (var row = 0; row < matrix[0].Length; row++)
            {
                result[row] = 0;
                for (var col = 0; col < vector.Length; col++) result[row] += matrix[col][row] * vector[col];
            }

            return result;
        }

        public static double[] MultTransposed(double[,] matrix, double[] vector)
        {
            if (vector.Length != matrix.GetLength(1))
                throw new AlgebraicException
                ("MultTransposed: invalid matrix and vector dimensions \n\t[" + matrix.GetLength(1) + "]x[" +
                 vector.Length + "]");
            var result = new double[matrix.GetLength(1)];
            for (var row = 0; row < matrix.GetLength(1); row++)
            {
                result[row] = 0;
                for (var col = 0; col < vector.Length; col++) result[row] += matrix[col, row] * vector[col];
            }

            return result;
        }

        // result = scalar * matrix(transposed) * vector (generate and return new result vector)   
        public static double[] MultTransposed(double scalar, double[][] matrix, double[] vector)
        {
            if (vector.Length != matrix.Length)
                throw new AlgebraicException
                ("MultTransposed: invalid matrix and vector dimensions \n\t[" + matrix.Length + "]x[" +
                 vector.Length + "]");
            var result = new double[matrix.Length];
            for (var row = 0; row < matrix[0].Length; row++)
            {
                result[row] = 0;
                for (var col = 0; col < vector.Length; col++) result[row] += scalar * matrix[col][row] * vector[col];
            }

            return result;
        }

        public static double[] MultTransposed(double scalar, double[,] matrix, double[] vector)
        {
            if (vector.Length != matrix.GetLength(0))
                throw new AlgebraicException
                ("MultTransposed: invalid matrix and vector dimensions \n\t[" + matrix.GetLength(1) + "][" +
                 matrix.GetLength(0) + "]x[" + vector.Length + "]");
            var result = new double[matrix.GetLength(1)];
            for (var row = 0; row < matrix.GetLength(1); row++)
            {
                result[row] = 0;
                for (var col = 0; col < vector.Length; col++) result[row] += scalar * matrix[col, row] * vector[col];
            }

            return result;
        }

        // result = matrix * vector (with matrix in profile format, generate and return new result vector)
        public static double[] Mult(double[][] matrix, double[] vector, int[] profile)
        {
            var dimension = matrix.Length;
            var result = new double[dimension];
            for (var i = 0; i < dimension; i++)
            {
                double sum = 0;
                for (var k = 0; k <= i - profile[i]; k++)
                    sum += matrix[i][k] * vector[k + profile[i]];
                for (var k = i + 1; k < dimension; k++)
                    if (profile[k] <= i)
                        sum += matrix[k][i - profile[k]] * vector[k];
                result[i] = sum;
            }

            return result;
        }

        // result = matrix * vector (with existing result vector and matrix in profile format and status vector)
        public static void Mult(double[] result, double[][] matrix, double[] vector, bool[] status, int[] profile)
        {
            var dimension = matrix.Length;
            for (var i = 0; i < dimension; i++)
            {
                if (status[i]) continue;
                double sum = 0;
                for (var k = 0; k <= i - profile[i]; k++)
                    sum += matrix[i][k] * vector[k + profile[i]];
                for (var k = i + 1; k < dimension; k++)
                    if (profile[k] <= i)
                        sum += matrix[k][i - profile[k]] * vector[k];
                result[i] = sum;
            }
        }

        // result = result + matrix * vector (with existing result vector and matrix in profile format and status vector)
        public static void MultAdd(double[] result, double[][] matrix, double[] vector, bool[] status, int[] profile)
        {
            var dimension = matrix.Length;
            for (var i = 0; i < dimension; i++)
            {
                if (status[i]) continue;
                double sum = 0;
                for (var k = 0; k <= i - profile[i]; k++)
                    sum += matrix[i][k] * vector[k + profile[i]];
                for (var k = i + 1; k < dimension; k++)
                    if (profile[k] <= i)
                        sum += matrix[k][i - profile[k]] * vector[k];
                result[i] += sum;
            }
        }

        // result = matrix * vector (with matrix in profile format and status vector)
        public static double[] Mult(double[][] matrix, double[] vector, bool[] status, int[] profile)
        {
            var dimension = matrix.Length;
            var result = new double[dimension];
            for (var i = 0; i < dimension; i++)
            {
                if (status[i]) continue;
                double sum = 0;
                for (var k = 0; k <= i - profile[i]; k++)
                    sum += matrix[i][k] * vector[k + profile[i]];
                for (var k = i + 1; k < dimension; k++)
                    if (profile[k] <= i)
                        sum += matrix[k][i - profile[k]] * vector[k];
                result[i] = sum;
            }

            return result;
        }

        // result = matrix * vector (with matrix in profile format and status vector)
        // UpperUpper    	| UU	  ul |   | U |		only UU*U
        //					|			 | * |	 |
        //					| lu	  ll |   | l |
        public static double[] MultUu(double[][] matrix, double[] vector, bool[] status, int[] profile)
        {
            var dimension = matrix.Length;
            var result = new double[dimension];
            for (var i = 0; i < dimension; i++)
            {
                if (status[i]) continue;
                double sum = 0;
                for (var k = 0; k <= i - profile[i]; k++)
                {
                    if (status[k + profile[i]]) continue;
                    sum += matrix[i][k] * vector[k + profile[i]];
                }

                for (var k = i + 1; k < dimension; k++)
                {
                    if (status[k]) continue;
                    if (profile[k] <= i) sum += matrix[k][i - profile[k]] * vector[k];
                }

                result[i] = sum;
            }

            return result;
        }

        // result = matrix * vector (with matrix in profile format and status vector)
        // UpperLower		| uu	  UL |   | u |		only UL*L
        //					|			 | * |	 |
        //					| lu	  ll |   | L |
        public static double[] MultUl(double[][] matrix, double[] vector, bool[] status, int[] profile)
        {
            var dimension = matrix.Length;
            var result = new double[dimension];
            for (var i = 0; i < dimension; i++)
            {
                if (status[i]) continue;
                double sum = 0;
                for (var k = 0; k < i - profile[i]; k++)
                {
                    if (!status[k]) continue;
                    if (k >= profile[i]) sum += matrix[i][k] * vector[k];
                }

                for (var k = i + 1; k < dimension; k++)
                {
                    if (!status[k]) continue;
                    if (profile[k] <= i) sum += matrix[k][i - profile[k]] * vector[k];
                }

                result[i] = sum;
            }

            return result;
        }

        // result = result - matrix * vector (with existing result vector and matrix in profile format and status vector)
        public static void MultSub(double[] result, double[][] matrix, double[] vector, bool[] status, int[] profile)
        {
            var dimension = matrix.Length;
            for (var i = 0; i < dimension; i++)
            {
                if (status[i]) continue;
                double sum = 0;
                for (var k = 0; k <= i - profile[i]; k++)
                    sum += matrix[i][k] * vector[k + profile[i]];
                for (var k = i + 1; k < dimension; k++)
                    if (profile[k] <= i)
                        sum += matrix[k][i - profile[k]] * vector[k];
                result[i] -= sum;
            }
        }

        // result = mat1 + mat2
        public static double[][] Add(double[][] mat1, double[][] mat2)
        {
            if (mat1.Length != mat2.Length || mat1[0].Length != mat2[0].Length)
                throw new AlgebraicException("Add: invalid matrices dimensions \n\t[" + mat1.Length + "," +
                                             mat1[0].Length + "]x[" + mat2.Length + "," + mat2[0].Length + "]");
            var result = new double[mat1.Length][];
            for (var i = 0; i < mat1.Length; i++)
            for (var j = 0; j < mat2[0].Length; j++)
            for (var k = 0; k < mat1[0].Length; k++)
                result[i][j] += mat1[i][k] * mat2[k][j];
            return result;
        }

        // result = mat1 - mat2
        public static void Subtract(double[][] result, double[][] mat1, double[][] mat2)
        {
            for (var i = 0; i < mat1.Length; i++)
            for (var j = 0; j < mat2.Length; j++)
                result[i][j] = mat1[i][j] - mat2[i][j];
        }

        public static void Subtract(double[,] result, double[,] mat1, double[,] mat2)
        {
            for (var i = 0; i < mat1.GetLength(0); i++)
            for (var j = 0; j < mat2.GetLength(0); j++)
                result[i, j] = mat1[i, j] - mat2[i, j];
        }

        // result = mat1 * mat2
        public static void Mult(double[][] result, double[][] mat1, double[][] mat2)
        {
            if (mat1[0].Length != mat2.Length)
                throw new AlgebraicException("Mult: invalid matrices dimensions \n\t["
                                             + mat1.Length + "," + mat1[0].Length + "]x[" + mat2.Length + "," +
                                             mat2[0].Length + "]");
            for (var row = 0; row < mat1.Length; row++)
            for (var col = 0; col < mat2[0].Length; col++)
            {
                double sum = 0;
                for (var k = 0; k < mat1[0].Length; k++)
                    sum += mat1[row][k] * mat2[k][col];
                result[row][col] = sum;
            }
        }

        public static void Mult(double[,] result, double[,] mat1, double[,] mat2)
        {
            if (mat1.GetLength(1) != mat2.Length)
                throw new AlgebraicException("Mult: invalid matrices dimensions \n\t["
                                             + mat1.Length + "," + mat1.GetLength(1) + "]x[" + mat2.Length + "," +
                                             mat2.GetLength(1) + "]");
            for (var row = 0; row < mat1.Length; row++)
            for (var col = 0; col < mat2.GetLength(1); col++)
            {
                double sum = 0;
                for (var k = 0; k < mat1.GetLength(1); k++)
                    sum += mat1[row, k] * mat2[k, col];
                result[row, col] = sum;
            }
        }

        // result = mat1 * mat2
        public static double[][] Mult(double[][] mat1, double[][] mat2)
        {
            if (mat1[0].Length != mat2.Length)
                throw new AlgebraicException("Mult: invalid matrices dimensions \n\t["
                                             + mat1.Length + "," + mat1[0].Length + "]x[" + mat2.Length + "," +
                                             mat2[0].Length + "]");
            var result = new double[mat1.Length][];
            for (var row = 0; row < mat1.Length; row++)
            for (var col = 0; col < mat2[0].Length; col++)
            {
                double sum = 0;
                for (var k = 0; k < mat1[0].Length; k++) sum += mat1[row][k] * mat2[k][col];
                result[row][col] = sum;
            }

            return result;
        }

        public static double[,] Mult(double[,] mat1, double[,] mat2)
        {
            if (mat1.GetLength(1) != mat2.GetLength(0))
                throw new AlgebraicException("Mult: invalid matrices dimensions \n\t["
                                             + mat1.GetLength(0) + "," + mat1.GetLength(1) + "]x[" + mat2.GetLength(0) +
                                             "," + mat2.GetLength(1) + "]");
            var result = new double[mat1.GetLength(0), mat2.GetLength(1)];
            for (var row = 0; row < mat1.GetLength(0); row++)
            for (var col = 0; col < mat2.GetLength(1); col++)
            {
                var sum = 0.0;
                for (var k = 0; k < mat1.GetLength(1); k++)
                    sum += mat1[row, k] * mat2[k, col];
                result[row, col] = sum;
            }

            return result;
        }

        // result = scalar * mat1 * mat2
        public static double[][] Mult(double factor, double[][] mat1, double[][] mat2)
        {
            if (mat1[0].Length != mat2.Length)
                throw new AlgebraicException("Mult: invalid matrices dimensions \n\t["
                                             + mat1.Length + "," + mat1[0].Length + "]x[" + mat2.Length + "," +
                                             mat2[0].Length + "]");
            var result = new double[mat1.Length][];
            for (var row = 0; row < mat1.Length; row++)
            for (var col = 0; col < mat2[0].Length; col++)
            {
                var sum = 0.0;
                for (var k = 0; k < mat1[0].Length; k++)
                    sum += mat1[row][k] * mat2[k][col];
                result[row][col] = factor * sum;
            }

            return result;
        }

        public static double[,] Mult(double factor, double[,] mat1, double[,] mat2)
        {
            if (mat1.GetLength(1) != mat2.Length)
                throw new AlgebraicException("Mult: invalid matrices dimensions \n\t["
                                             + mat1.Length + "," + mat1.GetLength(1) + "]x[" + mat2.Length + "," +
                                             mat2.GetLength(1) + "]");
            var result = new double[mat1.Length, mat2.GetLength(1)];
            for (var row = 0; row < mat1.Length; row++)
            for (var col = 0; col < mat2.GetLength(1); col++)
            {
                var sum = 0.0;
                for (var k = 0; k < mat1.GetLength(1); k++)
                    sum += mat1[row, k] * mat2[k, col];
                result[row, col] = factor * sum;
            }

            return result;
        }

        // result = scalar * mat1 * mat2(transposed)
        public static double[][] MultMatrixTransposed(double scalar, double[][] mat1, double[][] mat2)
        {
            if (mat1[0].Length != mat2[0].Length)
                throw new AlgebraicException("MultMatrixTransposed: invalid matrices dimensions \n\t["
                                             + mat1.Length + "," + mat1[0].Length + "]x[" + mat2[0].Length + "," +
                                             mat2.Length + "]");
            var result = new double[mat1.Length][];

            for (var row = 0; row < mat1.Length; row++)
            {
                result[row] = new double[mat1.Length];
                for (var col = 0; col < mat2.Length; col++)
                {
                    var sum = 0.0;
                    for (var k = 0; k < mat1[0].Length; k++)
                        sum += mat1[row][k] * mat2[col][k];
                    result[row][col] = scalar * sum;
                }
            }

            return result;
        }

        public static double[][] MultMatrixTransposed(double scalar, double[,] mat1, double[,] mat2)
        {
            if (mat1.GetLength(1) != mat2.GetLength(1))
                throw new AlgebraicException("MultMatrixTransposed: invalid matrices dimensions \n\t["
                                             + mat1.GetLength(0) + "," + mat1.GetLength(0) + "]x[" + mat2.GetLength(1) +
                                             "," + mat2.GetLength(0) + "]");
            var result = new double[mat1.Length][];

            for (var row = 0; row < mat1.Length; row++)
            {
                result[row] = new double[mat1.Length];
                for (var col = 0; col < mat2.Length; col++)
                {
                    var sum = 0.0;
                    for (var k = 0; k < mat1.GetLength(1); k++)
                        sum += mat1[row, k] * mat2[col, k];
                    result[row][col] = scalar * sum;
                }
            }

            return result;
        }

        public static double[,] RectMultMatrixTransposed(double scalar, double[,] mat1, double[,] mat2)
        {
            if (mat1.GetLength(1) != mat2.GetLength(1))
                throw new AlgebraicException("RectMultMatrixTransposed: invalid matrices dimensions \n\t["
                                             + mat1.GetLength(0) + "," + mat1.GetLength(1) + "]x[" + mat2.GetLength(1) +
                                             "," + mat2.GetLength(0) + "]");
            var result = new double[mat1.GetLength(0), mat2.GetLength(0)];

            for (var row = 0; row < mat1.GetLength(0); row++)
            for (var col = 0; col < mat2.GetLength(0); col++)
            {
                var sum = 0.0;
                for (var k = 0; k < mat1.GetLength(1); k++)
                    sum += mat1[row, k] * mat2[col, k];
                result[row, col] = scalar * sum;
            }

            return result;
        }

        // result = result + scalar * mat1 * mat2(transposed)
        public static void MultAddMatrixTransposed(double[][] result, double scalar, double[][] mat1, double[][] mat2)
        {
            if (mat1[0].Length != mat2[0].Length)
                throw new AlgebraicException("MultAddMatrixTransposed: invalid matrices dimensions \n\t["
                                             + mat1.Length + "," + mat1[0].Length + "]x[" + mat2[0].Length + "," +
                                             mat2.Length + "]");
            for (var row = 0; row < mat1.Length; row++)
            for (var col = 0; col < mat2.Length; col++)
            {
                var sum = 0.0;
                for (var k = 0; k < mat1[0].Length; k++)
                    sum += mat1[row][k] * mat2[col][k];
                result[row][col] += scalar * sum;
            }
        }

        // result = result + scalar * mat1 * mat2(transposed)
        public static void MultAddMatrixTransposed(double[][] result, double scalar, double[,] mat1, double[,] mat2)
        {
            if (mat1.GetLength(1) != mat2.GetLength(0))
                throw new AlgebraicException("MultAddMatrixTransposed: invalid matrices dimensions \n\t["
                                             + mat1.GetLength(0) + "," + mat1.GetLength(1) + "]x[" + mat2.GetLength(1) +
                                             "," + mat2.GetLength(0) + "]");
            for (var row = 0; row < mat1.Length; row++)
            for (var col = 0; col < mat2.Length; col++)
            {
                var sum = 0.0;
                for (var k = 0; k < mat1.GetLength(1); k++)
                    sum += mat1[row, k] * mat2[col, k];
                result[row][col] += scalar * sum;
            }
        }

        // result = result + scalar * mat1 * mat2(transposed)
        public static void MultAddMatrixTransposed(double[,] result, double scalar, double[,] mat1, double[,] mat2)
        {
            if (mat1.GetLength(1) != mat2.GetLength(1))
                throw new AlgebraicException("MultAddMatrixTransposed: invalid matrices dimensions \n\t["
                                             + mat1.GetLength(0) + "," + mat1.GetLength(1) + "]x[" + mat2.GetLength(1) +
                                             "," + mat2.GetLength(0) + "]");
            for (var row = 0; row < mat1.GetLength(0); row++)
            for (var col = 0; col < mat2.GetLength(0); col++)
            {
                var sum = 0.0;
                for (var k = 0; k < mat1.GetLength(1); k++)
                    sum += mat1[row, k] * mat2[col, k];
                result[row, col] += scalar * sum;
            }
        }

        // result = result + mat1 * mat2
        public static void MultAddMatrix(double[][] result, double[][] mat1, double[][] mat2)
        {
            if (mat1[0].Length != mat2.Length || mat1.Length != result.Length ||
                mat2[0].Length != result[0].Length)
                throw new AlgebraicException("MultAddMatrix: invalid matrices dimensions \n\t[" + result.Length + ","
                                             + result[0].Length + "] = [" + mat1.Length + "," + mat1[0].Length + "]x[" +
                                             mat2.Length + "," + mat2[0].Length + "]");
            for (var row = 0; row < mat1.Length; row++)
            for (var col = 0; col < mat2[0].Length; col++)
            {
                var sum = 0.0;
                for (var k = 0; k < mat1[0].Length; k++)
                    sum += mat1[row][k] * mat2[k][col];
                result[row][col] += sum;
            }
        }

        public static void MultAddMatrix(double[,] result, double[][] mat1, double[][] mat2)
        {
            if (mat1[0].Length != mat2.Length || mat1.Length != result.Length ||
                mat2[0].Length != result.GetLength(1))
                throw new AlgebraicException("MultAddMatrix: invalid matrices dimensions \n\t[" + result.Length + ","
                                             + result.GetLength(1) + "] = [" + mat1.Length + "," + mat1[0].Length +
                                             "]x[" + mat2.Length + "," + mat2[0].Length + "]");
            for (var row = 0; row < mat1.Length; row++)
            for (var col = 0; col < mat2[0].Length; col++)
            {
                var sum = 0.0;
                for (var k = 0; k < mat1[0].Length; k++)
                    sum += mat1[row][k] * mat2[k][col];
                result[row, col] += sum;
            }
        }

        public static void MultAddMatrix(double[,] result, double[,] mat1, double[,] mat2)
        {
            if (mat1.GetLength(1) != mat2.GetLength(0) || mat1.GetLength(0) != result.GetLength(0) ||
                mat2.GetLength(1) != result.GetLength(1))
                throw new AlgebraicException("MultAddMatrix: invalid matrices dimensions \n\t[" + result.Length + ","
                                             + result.GetLength(1) + "] = [" + mat1.GetLength(0) + "," +
                                             mat1.GetLength(1) + "]x[" + mat2.GetLength(0) + "," + mat2.GetLength(1) +
                                             "]");

            for (var row = 0; row < mat1.GetLength(0); row++)
            for (var col = 0; col < mat2.GetLength(1); col++)
            {
                var sum = 0.0;
                for (var k = 0; k < mat1.GetLength(1); k++)
                    sum += mat1[row, k] * mat2[k, col];
                result[row, col] += sum;
            }
        }

        // result = scalar * mat1(transposed) * mat2
        public static double[,] MultTransposedMatrix(double[,] mat1, double[,] mat2)
        {
            if (mat1.GetLength(1) != mat2.GetLength(0))
                throw new AlgebraicException("MultTransposedMatrix: ungültige Matrixdimensionen \n\t["
                                             + mat1.GetLength(1) + "," + mat1.GetLength(0) + "]x[" + mat2.GetLength(0) +
                                             "," + mat2.GetLength(1) + "]");
            var result = new double[mat1.GetLength(1), mat2.GetLength(1)];
            for (var row = 0; row < mat1.GetLength(1); row++)
            for (var col = 0; col < mat2.GetLength(1); col++)
            {
                var sum = 0.0;
                for (var k = 0; k < mat1.GetLength(0); k++)
                    sum += mat1[k, row] * mat2[k, col];
                result[row, col] = sum;
            }

            return result;
        }

        // result = scalar * mat1(transposed) * mat2
        public static double[,] MultTransposedMatrix(double scalar, double[,] mat1, double[,] mat2)
        {
            if (mat1.GetLength(0) != mat2.GetLength(0))
                throw new AlgebraicException("MultTransposedMatrix: invalid matrices dimensions \n\t["
                                             + mat1.GetLength(1) + "," + mat1.GetLength(0) + "]x[" + mat2.GetLength(0) +
                                             "," + mat2.GetLength(1) + "]");
            var result = new double[mat1.GetLength(1), mat2.GetLength(1)];
            for (var row = 0; row < mat1.GetLength(1); row++)
            for (var col = 0; col < mat2.GetLength(1); col++)
            {
                var sum = 0.0;
                for (var k = 0; k < mat1.GetLength(0); k++)
                    sum += mat1[k, row] * mat2[k, col];
                result[row, col] = scalar * sum;
            }

            return result;
        }

        // extract submatrix
        public static void ExtractSubMatrix(double[][] matrix, double[][] subMatrix, int[] indices)
        {
            if (indices.Length != subMatrix.Length || indices.Length != subMatrix[0].Length)
                throw new AlgebraicException("ExtractSubMatrix: index vector not same Length as submatrix");
            try
            {
                for (var i = 0; i < indices.Length; i++)
                for (var j = 0; j < indices.Length; j++)
                    subMatrix[i][j] = matrix[indices[i]][indices[j]];
            }
            catch (AlgebraicException)
            {
                throw new AlgebraicException("ExtractSubMatrix: index value outside range of matrix");
            }
        }

        public static void ExtractSubMatrix(double[,] matrix, double[,] subMatrix, int[] indices)
        {
            if (indices.Length != subMatrix.GetLength(0) || indices.Length != subMatrix.GetLength(1))
                throw new AlgebraicException("ExtractSubMatrix: index vector not same Length as submatrix");
            try
            {
                for (var i = 0; i < indices.Length; i++)
                for (var j = 0; j < indices.Length; j++)
                    subMatrix[i, j] = matrix[indices[i], indices[j]];
            }
            catch (AlgebraicException)
            {
                throw new AlgebraicException("ExtractSubMatrix: index value outside range of matrix");
            }
        }

        public static void ExtractSubMatrix(double[][] matrix, double[][] subMatrix, int[] rows, int[] cols)
        {
            if (rows.Length != subMatrix.Length || cols.Length != subMatrix[0].Length)
                throw new AlgebraicException("ExtractSubMatrix: index vector not same Length as submatrix");
            try
            {
                for (var i = 0; i < rows.Length; i++)
                for (var j = 0; j < cols.Length; j++)
                    subMatrix[i][j] = matrix[rows[i]][cols[j]];
            }
            catch (AlgebraicException)
            {
                throw new AlgebraicException("ExtractSubMatrix: index value outside range of matrix");
            }
        }

        public static void ExtractSubMatrix(double[,] matrix, double[,] subMatrix, int[] rows, int[] cols)
        {
            if (rows.Length != subMatrix.GetLength(0) || cols.Length != subMatrix.GetLength(1))
                throw new AlgebraicException("ExtractSubMatrix: index vector not same Length as submatrix");
            try
            {
                for (var i = 0; i < rows.Length; i++)
                for (var j = 0; j < cols.Length; j++)
                    subMatrix[i, j] = matrix[rows[i], cols[j]];
            }
            catch (AlgebraicException)
            {
                throw new AlgebraicException("ExtractSubMatrix: index value outside range of matrix");
            }
        }

        public static void ExtractSubMatrix(double[,] matrix, double[] subVector, int[] rows, int[] cols)
        {
            if (rows.Length == 1)
                for (var j = 0; j < cols.Length; j++)
                    subVector[j] = matrix[rows[0], cols[j]];
            else if (cols.Length == 1)
                for (var i = 0; i < rows.Length; i++)
                    subVector[i] = matrix[rows[i], cols[0]];
            else if (rows.Length == 1 || cols.Length == 1)
                subVector[0] = matrix[rows[0], cols[0]];
            else
                throw new AlgebraicException(
                    "ExtractSubMatrix: extrahierte Untermatrix muss ein Vektor oder ein Skalar sein");
        }
    }
}