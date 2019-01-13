using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Recommender
{
    class Program
    {
        static void Main(string[] args)
        {
            KMeans km = new KMeans();

            int nItemsCount = km.LoadItemsFromFile(FileNames.MoviesFile);

            Dictionary<string, List<string>> usersAndTheirSeenMovies = new Dictionary<string, List<string>>();
            usersAndTheirSeenMovies = FileNames.LoadUserDataFromFile(FileNames.UsersMovies);

            var user = ReadUserForRecommendation(usersAndTheirSeenMovies);

            var seenMovies = usersAndTheirSeenMovies[user];

            var clusteringCriteria = ReadClusterCriteria();

            IClustersList obtainedClusters = new IClustersList();


            switch (clusteringCriteria)
            {
                case ClusteringCriteria.ByGenre:
                    obtainedClusters = ComputeClustering(km, FileNames.GenresFile, nItemsCount, "Enter the number of movies per genre");
                    break;
                case ClusteringCriteria.ByRanking:
                    obtainedClusters = ComputeClustering(km, FileNames.RankingFile, nItemsCount, "Enter the number of movies per rank");
                    break;
            }

            RecommendMoviesBasedOnClusters(seenMovies, obtainedClusters);

            Console.Read();
        }

        private static void RecommendMoviesBasedOnClusters(List<string> seenMovies, IClustersList obtainedClusters)
        {
            Dictionary<int, List<KeyValuePair<ICluster, List<string>>>> ocurrencesAndClustersMapping = new Dictionary<int, List<KeyValuePair<ICluster, List<string>>>>();
            foreach (ICluster cluster in obtainedClusters.ClustersList)
            {
                int countPerCluster = 0;
                List<string> seenMovieItems = new List<string>();
                foreach (var item in cluster.Items.ItemsList)
                {
                    foreach (string movieName in seenMovies)
                    {
                        if (item.ItemText.Contains(movieName))
                        {
                            countPerCluster += 1;
                            seenMovieItems.Add(item.ItemText);
                        }
                    }

                }
                if (ocurrencesAndClustersMapping.ContainsKey(countPerCluster))
                {
                    ocurrencesAndClustersMapping[countPerCluster].Add(new KeyValuePair<ICluster, List<string>>(cluster, seenMovieItems));
                }
                else
                {
                    ocurrencesAndClustersMapping.Add(countPerCluster, new List<KeyValuePair<ICluster, List<string>>> { new KeyValuePair<ICluster, List<string>>(cluster, seenMovieItems) });
                }
            }

            var maxKey = ocurrencesAndClustersMapping.Keys.Max();

            var chosenClusterData = ocurrencesAndClustersMapping[maxKey];

            var recommendations = new List<string>();

            foreach (var data in chosenClusterData)
            {
                foreach (var item in data.Key.Items.ItemsList)
                {
                    recommendations.Add(item.ItemText);
                }
                foreach (var seen in data.Value)
                {
                    recommendations.Remove(seen);
                }

            }

            Console.WriteLine("Future recommendations");
            Console.WriteLine("_____________________");
            foreach (var recomm in recommendations)
            {
                Console.WriteLine(recomm);
            }
        }

        private static ClusteringCriteria ReadClusterCriteria()
        {
            Console.WriteLine("\nPick the cluster criteria: ");
            Console.WriteLine("1 - By Genre");
            Console.WriteLine("2 - By Ranking\n");
            return (ClusteringCriteria) int.Parse(Console.ReadLine());
        }

        private static string ReadUserForRecommendation(Dictionary<string, List<string>> pairs)
        {
            Console.WriteLine("Pick the user: \n");
            foreach(var user in pairs.Keys)
            {
                Console.WriteLine(user);
            }
            return (string)Console.ReadLine();
        }

        private static IClustersList ComputeClustering(KMeans km, string fileName, int nItemsCount, string userMessage)
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

            return km.Compute(nInitialCent, nItemsPerCluster);
        }
    }

    public static class FileNames
    {
        public static string MoviesFile = @"MOVIES.TXT";
        public static string GenresFile = @"GENRES.TXT";
        public static string RankingFile = @"RANKING.TXT";
        public static string UsersMovies = @"USERS_MOVIES.TXT";

        public static Dictionary<string, List<string>> LoadUserDataFromFile(string filename)
        {
            Dictionary<string, List<string>> usersWithSeenMovies = new Dictionary<string, List<string>>();
            using (System.IO.FileStream fsFile = new System.IO.FileStream(filename,
              System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
            {
                using (System.IO.StreamReader fsStream = new System.IO.StreamReader(
                  fsFile, System.Text.Encoding.UTF8, true, 128))
                {
                    string textBuf = "\0";
                    while ((textBuf = fsStream.ReadLine()) != null)
                    {
                        if (!String.IsNullOrEmpty(textBuf))
                        {
                            IAttributeList TargetAttribList = new IAttributeList();
                            string sItemPattern = " => "; string[] sItemTokens;
                            if ((sItemTokens = Regex.Split(textBuf, sItemPattern)) != null)
                            {
                                if (!usersWithSeenMovies.ContainsKey(sItemTokens[0]))
                                {
                                    usersWithSeenMovies.Add(sItemTokens[0], new List<string> { sItemTokens[1] });
                                }
                                else
                                {
                                    usersWithSeenMovies[sItemTokens[0]].Add(sItemTokens[1]);
                                }
                            }
                        }
                    }
                    fsStream.Close();
                }
                fsFile.Close();
            }
            return usersWithSeenMovies;
        }
    }

    public enum ClusteringCriteria
    {
        ByGenre = 1,
        ByRanking = 2
    }
}
