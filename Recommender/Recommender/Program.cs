using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] filenames = new string[2] { @"MOVIES.TXT", @"GENRES.TXT" };
           
            KMeans km = new KMeans();

            int nItemsCount = km.LoadItemsFromFile(filenames[0]);
            int nUsersCount = km.LoadCentroidsFromFile(filenames[1]);

            int nInitialCent = 0;
            int nItemsPerCluster = 0, nItemsPerClusterMax = 0;
            if (nItemsCount > 0 && nUsersCount > 0)
            {
                do
                {
                    nItemsPerClusterMax = nItemsCount / nUsersCount;
                    Console.Write("Enter the number of movies per genre [2-{0}]: ", nItemsPerClusterMax);
                    nItemsPerCluster = int.Parse(Console.ReadLine());
                } while (nItemsPerCluster < 2 || nItemsPerCluster > nItemsPerClusterMax);

                do
                {
                    Console.Write("\nEnter the number of initial centroids [1-{0}]: ", nUsersCount);
                    nInitialCent = int.Parse(Console.ReadLine());
                } while (nInitialCent < 1 || nInitialCent > nUsersCount);
            }

            km.Compute(nInitialCent, nItemsPerCluster);

            Console.Read();
        }
    }
}
