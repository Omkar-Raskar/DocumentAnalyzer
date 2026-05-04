public static class SimilarityHelper
{

    public static double CosineSimilarity(float[] v1, float[] v2)
    {
        if (v1 == null || v2 == null)
            return 0;

        int length = Math.Min(v1.Length, v2.Length);

        double dotProduct = 0;
        double mag1 = 0;
        double mag2 = 0;

        for (int i = 0; i < length; i++)
        {
            dotProduct += v1[i] * v2[i];
            mag1 += v1[i] * v1[i];
            mag2 += v2[i] * v2[i];
        }

        return dotProduct / (Math.Sqrt(mag1) * Math.Sqrt(mag2) + 1e-10);
    }
    //public static double CosineSimilarity(float[] v1, float[] v2)
    //{
    //    if (v1.Length != v2.Length)
    //        return 0; // skip invalid comparison

    //    double dotProduct = 0;
    //    double norm1 = 0;
    //    double norm2 = 0;

    //    for (int i = 0; i < v1.Length; i++)
    //    {
    //        dotProduct += v1[i] * v2[i];
    //        norm1 += Math.Pow(v1[i], 2);
    //        norm2 += Math.Pow(v2[i], 2);
    //    }

    //    return dotProduct / (Math.Sqrt(norm1) * Math.Sqrt(norm2));
    //}
}