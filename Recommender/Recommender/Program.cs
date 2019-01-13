using System;

namespace Recommender
{
    class Program
    {
        static void Main(string[] args)
        {  
            KMeans km = new KMeans();

            int nItemsCount = km.LoadItemsFromFile(FileNames.MoviesFile);

            var clusteringCriteria = ReadClusterCriteria();
            switch (clusteringCriteria)
            {
                case ClusteringCriteria.ByGenre:
                    ComputeClustering(km, FileNames.GenresFile, nItemsCount, "Enter the number of movies per genre");
                    break;
                case ClusteringCriteria.ByRanking:
                    ComputeClustering(km, FileNames.RankingFile, nItemsCount, "Enter the number of movies per rank");
                    break;
            }

            Console.Read();
        }

        private static ClusteringCriteria ReadClusterCriteria()
        {
            Console.WriteLine("\nPick the cluster criteria: ");
            Console.WriteLine("1 - By Genre");
            Console.WriteLine("2 - By Ranking");
            return (ClusteringCriteria) int.Parse(Console.ReadLine());
        }

        private static void ComputeClustering(KMeans km, string fileName, int nItemsCount, string userMessage)
        {
            int nUsersCount = km.LoadCentroidsFromFile(fileName);

            int nInitialCent = 0;
            int nItemsPerCluster = 0, nItemsPerClusterMax = 0;
            if (nItemsCount > 0 && nUsersCount > 0)
            {
                do
                {
                    nItemsPerClusterMax = nItemsCount / nUsersCount;
                    Console.Write($"{userMessage} [2-{nItemsPerClusterMax}]: ");
                    nItemsPerCluster = int.Parse(Console.ReadLine());
                } while (nItemsPerCluster < 2 || nItemsPerCluster > nItemsPerClusterMax);

                do
                {
                    Console.Write("\nEnter the number of initial centroids [1-{0}]: ", nUsersCount);
                    nInitialCent = int.Parse(Console.ReadLine());
                } while (nInitialCent < 1 || nInitialCent > nUsersCount);
            }

            km.Compute(nInitialCent, nItemsPerCluster);
        }
    }

    public static class FileNames
    {
        public static string MoviesFile = @"MOVIES.TXT";
        public static string GenresFile = @"GENRES.TXT";
        public static string RankingFile = @"RANKING.TXT";
    }

    public enum ClusteringCriteria
    {
        ByGenre = 1,
        ByRanking = 2
    }
}
