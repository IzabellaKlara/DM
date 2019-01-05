using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Recommender
{
    public class KMeans
    {
        private IItemsList Items;
        private IItemsList Centroids_Users;
        private IClustersList Clusters;
        private IClustersList mTargetClusters;
        private readonly System.Random random = new System.Random();

        private double MinValue = 0;
        private double MaxValue = 0;

        public KMeans()
        {
            Items = new IItemsList();
            Centroids_Users = new IItemsList();

        }

        public int LoadCentroidsFromFile(string filename)
        {
            // Intializing the file stream object and opening the file with the name being specified
            using (System.IO.FileStream fsFile = new System.IO.FileStream(filename,
              System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
            {
                // Initializing the stream reader object
                using (System.IO.StreamReader fsStream = new System.IO.StreamReader(
                  fsFile, System.Text.Encoding.UTF8, true, 128))
                {
                    int iItem = 0;
                    int nAttrib = 1; string textBuf = "\0";
                    // Retrieving each line from the file until we reach the end-of-file
                    while ((textBuf = fsStream.ReadLine()) != null)
                    {
                        // Performing if the line is not empty and contains the data on a specific centroid
                        if (!String.IsNullOrEmpty(textBuf))
                        {
                            // If so, initializing the array of attributes TargetAttribList for the current centroid
                            string sPattern = " => "; string[] sTokens;
                            IAttributeList TargetAttribList = new IAttributeList();
                            // Tokenizing the string according to the regular expression pattern assigned to the sItemPatern variable
                            if ((sTokens = Regex.Split(textBuf, sPattern)) != null)
                            {
                                // For each particular attribute name token we're performing encoding 
                                // to obtain each attribute value associated with its name
                                foreach (string token in sTokens[1].Split(new char[] { ' ' }))
                                {
                                    // At this point, we're performing a check if the attribute with similar name
                                    // for a specific centroid has not been already indexed into the array of attributes
                                    bool bExists = false; int nToken = 0;
                                    int nIndex = 0; double nCoeff = 0;
                                    // Iterating the array of centroids to find those centroids that have 
                                    // the attribute with the name which is equal to the name of 
                                    // the current attribute attribs[nToken].Name.Equals(token) being processed
                                    while (nIndex < Items.Count() && bExists == false)
                                    {
                                        nToken = 0;
                                        // Iterating through the array of attributes of the current centroid Centroids_Users[nIndex]
                                        // and performing a check if a certain atribute's name of the current centroid is not equal
                                        // the name of the current attributed being retrieved from the file. If so, we're ending
                                        // the loop execution by assinging the the bExists variable value of true
                                        while (nToken < Items[nIndex].AttributeList.Count() && bExists == false)
                                            bExists = (Items[nIndex].AttributeList[nToken++].Name.Equals(token)) ? true : false;

                                        nIndex++;
                                    }

                                    // If the centroids with hat have the attribute with the name which is equal to the name of 
                                    // the current attribute attribs[nToken].Name.Equals(token) don't exists, then we're
                                    // iterating through the array of centroids performing a check if the attribute with similar
                                    // has already been indexed for a particular centroid into the array Centroids_Users.
                                    if (bExists == false)
                                    {
                                        int nItem = iItem - 1;
                                        bool bUserAttrib = false;
                                        // Iterating through the set of previous centroids in the array of centroids Centroids_Users
                                        while (nItem >= 0 && bUserAttrib == false)
                                        {
                                            nToken = 0;
                                            // For each centroid, iterating through the array of attributes, and for each attribute's
                                            // name we're performing a check if the name of the current attribute of the current centroid
                                            // is not equal to the name of the current token retrieved from the file.
                                            while (nToken < Centroids_Users[nItem].AttributeList.Count() && !bUserAttrib)
                                                bUserAttrib = (Centroids_Users[nItem].AttributeList[nToken++].Name.Equals(token)) ? true : false;

                                            nItem--;
                                        }

                                        // Computing the coefficient value for the current attribute retrieved from the file.
                                        // If a centroid with the similar name of attribute has already been indexed into the array
                                        // of centroids we're assigning the its attribute's value to the nCoeff variable, which is
                                        // actually the value of the attribute for the current centroid fetched from the file, otherwise
                                        // we're assigning the actual index value for the current attributes using nAttrib counter 
                                        // variable value
                                        nCoeff = (bUserAttrib == true) ? Centroids_Users[nItem + 1].AttributeList[nToken - 1].Value : nAttrib;
                                    }

                                    // Otherwise, assigning the nCoeff variable to the value of the attribute of a specific centroid that
                                    // has already been indexed into the array of centroids
                                    else nCoeff = Items[nIndex - 1].AttributeList[nToken - 1].Value;

                                    // Inserting the new attribute represented as a pair of two value of either token name 
                                    // or coefficient nCoeff into the array of attributes for the current item being retrieved from the file
                                    TargetAttribList.Add(new Attribute(token, nCoeff));

                                    nAttrib++; // Incrementing the value of the attributes loop counter variable by 1
                                }

                                // Inserting the current centroid retrieved from the file into the array of centroids Centroids_Users
                                Centroids_Users.Add(new Item(textBuf, TargetAttribList, 0, true, false));

                                iItem++; // Incrementing the value of the centroids loop counter variable by 1
                            }
                        }
                    }

                    // Performing normalization of the attributes values for each centroid the array of centroids
                    Normalize(Centroids_Users, nAttrib, true, ref MinValue, ref MaxValue);

                    // Deallocating the stream reader object
                    fsStream.Close();
                }

                // Deallocating the file stream object
                fsFile.Close();
            }

            // Returning the actual value of the number of centroids retrieved from the file
            return Centroids_Users.Count();
        }

        public int LoadItemsFromFile(string filename)
        {
            // Intializing the file stream object and opening the file with the name being specified
            using (System.IO.FileStream fsFile = new System.IO.FileStream(filename,
              System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
            {
                // Initializing the stream reader object
                using (System.IO.StreamReader fsStream = new System.IO.StreamReader(
                  fsFile, System.Text.Encoding.UTF8, true, 128))
                {
                    int iItem = 0;
                    int nAttrib = 1; string textBuf = "\0";
                    // Retrieving each line from the file until we reach the end-of-file
                    while ((textBuf = fsStream.ReadLine()) != null)
                    {
                        // Performing if the line is not empty and contains the data on a specific item
                        if (!String.IsNullOrEmpty(textBuf))
                        {
                            // If so, initializing the array of attributes TargetAttribList for the current item
                            IAttributeList TargetAttribList = new IAttributeList();
                            // Tokenizing the string according to the regular expression pattern assigned to the sItemPatern variable
                            string sItemPattern = " => "; string[] sItemTokens;
                            if ((sItemTokens = Regex.Split(textBuf, sItemPattern)) != null)
                            {
                                // Iterating through the array of tokens and for each string 
                                // perform another tokenization to obtain the set of attributes name for the current item
                                for (int iToken = 0; iToken < 2; iToken++)
                                {
                                    // For each string sItemTokens[iToken] we're performing tokenization to obtain the set of attributes names
                                    string sPattern = " "; string[] sTokens;
                                    if ((sTokens = Regex.Split(sItemTokens[iToken], sPattern)) != null)
                                    {
                                        // For each particular attribute name token we're performing encoding 
                                        // to obtain each attribute value associated with its name
                                        foreach (string token in sTokens)
                                        {
                                            // At this point, we're performing a check if the attribute with similar name
                                            // for a specific item has not been already indexed into the array of attributes
                                            bool bExists = false; int nToken = 0;
                                            int nIndex = iItem; double nCoeff = 0;
                                            // Iterating the array of items to find those items that have 
                                            // the attribute with the name which is equal to the name of 
                                            // the current attribute attribs[nToken].Name.Equals(token) being processed
                                            while (--nIndex >= 0 && bExists == false)
                                            {
                                                nToken = 0;
                                                // Iterating through the array of attributes of the current item m_Items[nIndex]
                                                // and performing a check if a certain atribute's name of the current item is not equal
                                                // the name of the current attributed being retrieved from the file. If so, we're ending
                                                // the loop execution by assinging the the bExists variable value of true
                                                IAttributeList attribs = Items[nIndex].AttributeList;
                                                while (nToken < attribs.Count() && bExists == false)
                                                    bExists = (attribs[nToken++].Name.Equals(token)) ? true : false;
                                            }

                                            // Computing the coefficient value for the current attribute retrieved from the file.
                                            // If an item with the similar name of attribute has already been indexed into the array
                                            // of items we're assigning the its attribute's value to the nCoeff variable, which is
                                            // actually the value of the attribute for the current item fetched from the file, otherwise
                                            // we're assigning the actual index value for the current attributes using nAttrib counter 
                                            // variable value
                                            nCoeff = (bExists == true) ?
                                                Items[nIndex + 1].AttributeList[nToken - 1].Value : nAttrib;

                                            bool bInAttribList = false; int iAttrib = 0;
                                            // Iterating through the array of target attributes and performing a check if the
                                            // attribute with the similar name has not yet been indexed to the following array for current item
                                            while (iAttrib < TargetAttribList.Count() && !bInAttribList)
                                                bInAttribList = (token.Equals(TargetAttribList[iAttrib++].Name)) ? true : false;

                                            // If the current attribute has not yet been indexed, inserting the new attribute
                                            // represented as a pair of two value of either token name or coefficient nCoeff into
                                            // the array of attributes for the current item being retrieved from the file
                                            if (bInAttribList == false)
                                                TargetAttribList.Add(new Attribute(token, nCoeff));

                                            nAttrib++; // Incrementing the value of the attributes loop counter variable by 1
                                        }
                                    }
                                }
                            }

                            // Inserting the current item retrieved from the file into the array of items m_Items
                            Items.Add(new Item(textBuf, TargetAttribList, 0, false, false));

                            iItem++; // Incrementing the value of the items loop counter variable by 1
                        }
                    }

                    // Performing normalization of the attributes values for each item the array of items
                    Normalize(Items, nAttrib, false, ref MinValue, ref MaxValue);

                    // Deallocating the stream reader object
                    fsStream.Close();
                }

                // Deallocating the file stream object
                fsFile.Close();
            }

            // Returning the actual value of the number of items retrieved from the file
            return Items.Count();
        }

        public void Compute(int nInitialCentroids, int nItemsPerCluster)
        {
            // Initializing the array of target clusters for which we'll produce the new clusters
            mTargetClusters = new IClustersList();
            // Performing the iteration until we've obtained the array of target clusters
            while (mTargetClusters.Count() < Centroids_Users.Count())
            {
                // Initializing the array of clusters
                Clusters = new IClustersList();
                // Performing a check if the number of centroids within the initial cluster is not equal to the number of centroids
                if (nInitialCentroids != Centroids_Users.Count())
                {
                    // Obtaining the array of initial centroids based on the values 
                    // retrieved from the array of centroids by performing k-means++ procedure

                    // Initializing the array of centroid indexes
                    List<int> CentroidIndexes = new List<int>();
                    // Randomly generate the index of the first intial centroid
                    int nInitialCentroid = random.Next(0, Centroids_Users.Count());
                    // Performing iteration until we've obtained the n-initial centroids
                    while (CentroidIndexes.Count() < nInitialCentroids)
                    {
                        double nDistance = 0, nDistanceSum = 0;
                        double nDistanceMin = 0; int nCntMin = -1;
                        // Iterating through the array of centroids and for each centroid compute the distance
                        // to the initial centroid being previously selected
                        for (int nItem = 0; nItem < Centroids_Users.Count(); nItem++)
                        {
                            // Performing a check if the index of the current centroid is not equal to
                            // the index of the intial centroid (i.e. centroid) in the array of centroids
                            if (nItem != nInitialCentroid)
                            {
                                // If so, computing the actual distance between the two vectors 
                                // of either the current centroid Centroids_Users[nItem] or initial centroid's Centroids_Users[nInitialCentroid] attributes.
                                if ((nDistance = Math.Pow(Centroids_Users[nItem].EuclDW(Centroids_Users[nInitialCentroid]), 2.0)) >= 0)
                                {
                                    // If the following distance is less than the smallest distance to the initial centroid 
                                    // Centroids_Users[nInitialCentroid], then we're performing a check if the index of the current 
                                    // centroid has not yet been inserted into the array of the centroids indexes.
                                    if (nDistance < nDistanceMin || nCntMin == -1)
                                    {
                                        bool bFound = false; int iCntIndex = 0;
                                        // Iterating through the array of centroids indexes and for each index CentroidIndexes[iCntIndex]
                                        // in the array we're performing a check if it's not equal to the index of the current centroid nItem,
                                        // if so, we're ending the loop execution by assigning the value true to the variable bFound. 
                                        while (iCntIndex < CentroidIndexes.Count() && bFound == false)
                                            bFound = (CentroidIndexes[iCntIndex++] == nItem) ? true : false;

                                        // If the current centroid's index is not in the array of the centroids indexes, then
                                        // we're assigning the variable nDistanceMin the value of the previously computed
                                        // distance nDistance. Also, we're assigning the index value of the current centroid to nCntMin variable
                                        if (bFound == false)
                                        {
                                            nDistanceMin = nDistance; nCntMin = nItem;
                                        }
                                    }

                                    // Computing the sum of the distances to the initial centroid for each centroid
                                    nDistanceSum += nDistance;
                                }
                            }
                        }

                        // Modify the value of nDistanceSum variable multiplying it by the randomly generate number
                        nDistanceSum = random.NextDouble() * nDistanceSum;

                        int nIndex = 0; double nSum = 0;
                        // Iterating through the array of centroids until the sum of distances between the vectors of attributes of
                        // each centroid and the initial centroid doesn't exceed the total value on the sum of distances for all centroids
                        while (nIndex < Centroids_Users.Count() && nSum < nDistanceSum)
                        {
                            int iTargetIndex = 0; bool bFound = false;
                            // For the current centroid Centroids_Users[nIndex] computing the distance 
                            // to the centroids that has been previously selected as an initial centroid
                            double nDist = Math.Pow(Centroids_Users[nIndex++].EuclDW(Centroids_Users[nCntMin]), 2.0);
                            // Performing a check if the index of the current centroid Centroids_Users[nIndex] is not in the array CentroidIndexes. 
                            while (iTargetIndex < CentroidIndexes.Count() && !bFound)
                                bFound = (CentroidIndexes[iTargetIndex++] == nIndex) ? true : false;

                            // If not, summing the distance value for the current centroid nDist with the nSum variable
                            if (bFound == false)
                                nSum += nDist;
                        }

                        // Performing a check if the value of the nCntMin variable representing the actual index
                        // of the centroid with the smallest distance to initial centroid is not equal to -1
                        if (nCntMin != -1)
                            // If not, inserting the index nCntMin to the array of centroids indexes
                            CentroidIndexes.Add(nCntMin);
                    }

                    // Initializing the array of initial centroids
                    IItemsList CentroidItems = new IItemsList();
                    // Iterating through the array of centroids and inserting each centroid 
                    // Centroid_Users[CentroidIndexes[iIndex] with index CentroidIndexes[iIndex] to the array of centroids
                    for (int iIndex = 0; iIndex < CentroidIndexes.Count(); iIndex++)
                        CentroidItems.Add(Centroids_Users[CentroidIndexes[iIndex]]);

                    // Inserting the new current initial cluster to the array of clusters.
                    Clusters.Add(new ICluster(CentroidItems, Items));
                }

                // Inserting the initial cluster into the array of clusters
                else Clusters.Add(new ICluster(Centroids_Users, Items));

                // Iterating through the array of clusters, retrieving each cluster 
                // to obtain the new clusters by performing k-means procedure
                for (int iCluster = 0; iCluster < Clusters.Count(); iCluster++)
                {
                    // Clonning the array of items belonging to the current 
                    // cluster m_Clusters[iCluster] by copying them into array Items
                    IItemsList Items = (IItemsList)Clusters[iCluster].Items.Clone();
                    // Clonning the array of centroids belonging to the current 
                    // cluster m_Clusters[iCluster] by copying them into array Centroids
                    IItemsList Centroids = (IItemsList)Clusters[iCluster].Centroids.Clone();
                    // Iterating through the array of centroids of the current cluster m_Clusters[iCluster]
                    for (int iCentroid = 0; iCentroid < Centroids.Count(); iCentroid++)
                    {
                        // For each centroid Centroids[iCentroid] of the current cluster 
                        // m_Clusters[iCluster] retriving the set of attributes and copy it into array attribsA
                        IAttributeList attribsA = Centroids[iCentroid].AttributeList;
                        // Normalizing the set of attributes of the current centroid Centroids[iCentroid]
                        // Iterating through the array of attributes of the current centroid Centroids[iCentroid]
                        for (int iAttrib = 0; iAttrib < attribsA.Count(); iAttrib++)
                            // For each attribute retrieved from the set of attributes of the current centroid
                            // Centroids[iCentroid], we're performing a linear search in the array of items Items
                            // to find those items that have one or more attributes with similar name (e.g. which name 
                            // is lexicographically equal to the name of the current attribute attribsA[iAttrib] of the 
                            // centroid Centroids[iCentroid]).
                            // Iterating through the array of items and for each item Items[iItem] retrieving a set of attributes
                            for (int iItem = 0; iItem < Items.Count(); iItem++)
                            {
                                // Copying the array of attributes for the current item Items[iItem] into the array attribsB
                                IAttributeList attribsB = Items[iItem].AttributeList;
                                // Iterating through the array of attributes attribB of the current item Items[iItem] and perform a check
                                // if the name of the current attribute attribsB[nAttrib] is lexicographically equal to the name of the
                                // current attribute attribsA[iAttrib] of the current centroid
                                for (int nAttrib = 0; nAttrib < attribsB.Count(); nAttrib++)
                                    // If the name of current item's attribute attribsB[nAttrib] is lexicographically equal to
                                    // the name of the current centroid's attribute attribsA[iAttrib], then we're performing a
                                    // swap to ensure that the particular attributes of either the centroid or item are
                                    // located at the same position in the arrays representing the vectors of either or 
                                    // items attributes, relative to the position of the centroid's of a particular attributes
                                    // in the array of attributes of the current centroid Centroids[iCentroid]
                                    if (attribsB[nAttrib].Name.Equals(attribsA[iAttrib].Name))
                                        if (iAttrib < attribsB.Count()) Swap(ref attribsB, iAttrib, nAttrib);
                            }

                        // Initializing the variable nDistanceAvg to store the value of the average distance
                        // between the current centroid Centroids[iCentroid] and each item within the current cluster m_Clusters[iCluster]
                        double nDistanceAvg = 0;
                        // Initializing the list of indexes of those items that have the smallest distance
                        // to the current centroid Centroids[iCentroid]
                        List<int> ItemsIndexes = new List<int>();
                        int nNeighbors = Items.Count() < nItemsPerCluster ? Items.Count() : nItemsPerCluster;

                        bool bDone = false;
                        // Performing the linear search to find all items in the array Items that have the smallest
                        // distance (e.g. the most similar items) to the current centroid Centroids[iCentroid] until
                        // we've exactly found nNeightbors items that exactly meet the following creteria
                        while (ItemsIndexes.Count() < nNeighbors && !bDone)
                        {
                            // Initializing the variable nDistanceAvg to store the average distance within a cluster
                            nDistanceAvg = 0;
                            // Initializing the nDistanceMin variable used to store the value of the smallest distance
                            // between an item and the current centroid Centroids[iCentroid]
                            // Initializing the nItemMin variable to store the index value of the item from the array Items
                            // having the smallest distance to the current centroid Centroids[iCentroid]
                            double nDistanceMin = 0; int nItemMin = -1;
                            // Iterating through the array of items of the current cluster m_Clusters[iCluster]
                            for (int iItem = 0; iItem < Items.Count(); iItem++)
                            {
                                double nDistance = 0;
                                // For each item being retrieved we're performing a check if the distance for the current item
                                // has not already been computed and the current item is not the item having the smallest distance
                                // to the current centroid Centroids[iCentroid]
                                if (Clusters[iCluster].Items[iItem].Exists == false)
                                {
                                    // If not, computing the distance between two vectors of attributes of either
                                    // the current centroid attribA or the current item Items[iItem]
                                    // Initializing the temporary array of the current centroid's attributes by copying
                                    // the array attribsA to the temporary array item
                                    Item temp = new Item(null, attribsA, 0, false, false);
                                    // Computing the actual distance between the vector of the currentr centroid's attributes
                                    // stored to the array of attributes item, and the vector of attributes for the current item Items[iItem]
                                    if ((nDistance = Items[iItem].EuclDW(temp)) >= 0)
                                    {
                                        // If the value of distance between the either the current centroid's or item's vectors of attributes
                                        // is less than the smallest distance to the current centroid's vector of attributes, then we're performing
                                        // a check if the index of the current item's Items[iItem] has not already been stored into the array ItemsIndexes
                                        if (((nDistance < nDistanceMin) ||
                                             (nItemMin == -1)) && Items[iItem].ItemText != null && nDistance <= 1.0)
                                        {
                                            bool bExists = false;
                                            int nItem = ItemsIndexes.Count() - 1;
                                            // Performing the linear search to check if the index of the current item for which we've previously
                                            // computed the distance value, doesn't exists in the array of indexes.
                                            while (nItem >= 0 && bExists == false)
                                                bExists = (ItemsIndexes[nItem--] == iItem) ? true : false;

                                            // If not, assigning the variable nDistanceMin the distance value between the vector of attributes
                                            // of the current centroid Centroids[iCentroid] and the vector of attributes of the current item Items[iItem]
                                            // Also, we're assigning the index value iItem of the current item Items[iItem] to the nItemMin variable
                                            if (bExists == false)
                                            {
                                                nDistanceMin = nDistance; nItemMin = iItem;
                                            }
                                        }
                                    }
                                }

                                // Computing the avarage distance between the vector of attributes of each item 
                                // Items[iItem] and the vector of attributes of the current centroid Centroids[iCentroid]
                                nDistanceAvg += nDistance / Items.Count();
                            }

                            // If nItemMin variable is not equal to -1 and the appropriate value of the item with the smallest distance
                            // has been found, inserting the value of index nItemMin into the array ItemsIndexes.
                            if (nItemMin > -1)
                                ItemsIndexes.Add(nItemMin);
                            // Otherwise, terminating the process of finding the items with the 
                            // smallest distance to the current centroid Centroids[iCentroid]
                            else bDone = true;
                        }

                        // Iterating through the array of items of the current cluster m_Clusters[iCluster]
                        // and for each item Items[ItemsIndexes[iIndex] with the index value ItemsIndexes[iIndex]
                        // stored in the array ItemsIndexes, we're assigning the variable Exists to true, which
                        // means that the actual distance value for the current item has already been computed
                        // and the following item has already been included into the current the newly built cluster
                        for (int iIndex = 0; iIndex < ItemsIndexes.Count(); iIndex++)
                            Clusters[iCluster].Items[ItemsIndexes[iIndex]].Exists = true;

                        // Updating the value of the variable Centroids[iCentroid].Distance by assigning it
                        // the value of the current averaged distance nDistanceAvg being computed
                        Centroids[iCentroid].Distance = nDistanceAvg;

                        // Initializing the array of target items for the current newly built cluster
                        IItemsList TargetItems = new IItemsList();
                        // Iterating through the array of indexes ItemsIndexes being obtained
                        for (int iItem = 0; iItem < ItemsIndexes.Count(); iItem++)
                        {
                            // For the current cluster we're performing a check if the current cluster
                            // is not the initial cluster in the array of clusters (e.g. the following cluster
                            // has no more than one centroid).
                            if (Clusters[iCluster].Centroids.Count() <= 1)
                                // If not, re-compute the distance between the current item with index ItemsIndexes[iItem]
                                // to the only centroid of the current non-initial cluster and assign this value to the
                                // Items[ItemsIndexes[iItem]].Distance variable for the current item
                                Items[ItemsIndexes[iItem]].Distance =
                                    Items[ItemsIndexes[iItem]].EuclDW(Clusters[iCluster].Centroids[0]);

                            // Resetting the value of the exists variable for the current item in the newly built cluster
                            // by assigning it the variable of false.
                            Items[ItemsIndexes[iItem]].Exists = false;
                            // Inserting the current item with index ItemsIndexes[iItem] into the array of target items for the newly built cluster
                            TargetItems.Add(Items[ItemsIndexes[iItem]]);
                        }

                        int nMinAttribs = -1;
                        // Iterating through the array of target items for the newly built cluster to
                        // find the item with smallest number of attributes
                        for (int iItem = 0; iItem < TargetItems.Count(); iItem++)
                        {
                            // Obtaining the value of the actual number of attributes count of the current item TargetItems[iItem]
                            int nAttribCount = TargetItems[iItem].AttributeList.Count();
                            // If the value of attributes count nAttribCount is less than the smallest value of the attributes count for
                            // the entire array TargetItems, then assigning the value of nAttribCount to the nMinAttribs variable
                            if (nAttribCount < nMinAttribs || nMinAttribs == -1)
                                nMinAttribs = nAttribCount;
                        }

                        // Initializing the attribs list to store the computed values of attributes
                        // for the centroid of the newly built cluster
                        IAttributeList attribs = new IAttributeList();
                        // Computing the new value for each attribute of the centroid for the newly build cluster by iterating 
                        // through each attrbutes in each item, inserting each new value of attribute into the array attrib
                        for (int nAttrib = 0; nAttrib < nMinAttribs &&
                             nAttrib < Centroids[iCentroid].AttributeList.Count(); nAttrib++)
                        {
                            // For each attribute of the current centroid Centroids[iCentroid] of the existing cluster
                            // obtaining the new value by computing the sum of each attribute AttribList[nAttrib] at the
                            // position nAttrib of the vector of attributes AttribList[nAttrib] for each target item being previously obtained
                            double nAttribAvg = 0; int nCount = 0;
                            // Iterating through the array of target items TargetItems
                            for (int iItem = 0; iItem < TargetItems.Count(); iItem++)
                                // For each item performing a check if the value of the attribute 
                                // located at the current position nAttrib is greater than zero
                                if (TargetItems[iItem].AttributeList[nAttrib].Value > 0)
                                {
                                    // If so, adding up the value of the attribute located at the position nAttrib
                                    // with the value of the nAttribAvg variable. Also computing the count of the target
                                    // items that exactly meet the following condition by incrementing the count variable by 1
                                    nAttribAvg += TargetItems[iItem].AttributeList[nAttrib].Value;
                                    nCount++;
                                }

                            // Since we've obtained the new value of attribute AttribList[nAttrib].Value with index nAttrib,
                            // for the current centroid Centroids[iCentroid] we're computing the average value by performing
                            // the division of the nAttribAvg variable's value by the actual number of target items that 
                            // satisfied the criteria commented above. After that, we're compacting the following value by
                            // performing normalization. Finally we're inserting the average value  into the array of
                            // atributes attrib along with the name of the new attribute which value has been obtained
                            attribs.Add(new Attribute(Centroids[iCentroid].AttributeList[nAttrib].Name,
                              ((nAttribAvg / (double)(nCount + 1)) - MinValue) / (MaxValue - MinValue) + 0.01));
                        }

                        bool bDiff = false; int nIndex = 0;
                        // Iterating through the new vector of attributes attribs to determine if the following vector
                        // of attributes being obtained has the different values of attributes than the vector of attributes attribsA 
                        // of the current centroid Centroids[iCentroid]
                        while (nIndex < attribs.Count() && nIndex < attribsA.Count() && bDiff == false)
                            bDiff = (attribs[nIndex].Value != attribsA[nIndex++].Value) ? true : false;

                        if (bDiff == true)
                        {
                            // If so, initializing the array of new centroids
                            IItemsList TargetCentroids = new IItemsList();
                            // Inserting the new centroid with vector of attributes 
                            // attribs into the array of centroids for the new cluster
                            TargetCentroids.Add(new Item(Centroids[iCentroid].ItemText,
                                    attribs, nDistanceAvg, Centroids[iCentroid].IsCentroid, false));

                            // Inserting newly built cluster represented as a pair 
                            // of sets of either TargetCentroids or TargetItems to the array of clusters
                            Clusters.Add(new ICluster(TargetCentroids, TargetItems));
                        }

                        else
                        {
                            // If not, iterating through the array of newly computed target clusters
                            // and for each cluster perfoming a check if there's a cluster with centroid which name
                            // value is equal to the name value of the current centroid Centroids[iCentroid]
                            bool bExists = false; int iTargetCluster = 0;
                            while (iTargetCluster < mTargetClusters.Count() && !bExists)
                                bExists = mTargetClusters[iTargetCluster++].Centroids[0].
                                   ItemText.Equals(Centroids[iCentroid].ItemText) ? true : false;

                            if (bExists == false)
                            {
                                // If so, initializing the array of centroids TargetCentroids and insert the
                                // new centroid into the following array. 
                                IItemsList TargetCentroids = new IItemsList();
                                TargetCentroids.Add(new Item(Centroids[iCentroid].ItemText,
                                    Centroids[iCentroid].AttributeList, Centroids[iCentroid].Distance, true, true));
                                // Inserting the cluster into the array of target clusters for which we'll further
                                // be producing the new clusters during the next iteration of the outermost loop
                                // This fragment of code actually performs filtering to avoid the existance 
                                // of the clusters with similar centroid's vectors of attributes in the array of clusters.
                                mTargetClusters.Add(new ICluster((IItemsList)TargetCentroids.Clone(),
                                   (IItemsList)TargetItems.Clone()));
                            }
                        }
                    }
                }
            }

            double nDiAvg = 0;
            // Computing the sum of the distances values for each target cluster
            for (int iCluster = 1; iCluster < mTargetClusters.Count(); iCluster++)
                nDiAvg += mTargetClusters[iCluster].Centroids[0].Distance / (mTargetClusters.Count() - 1);

            double nD0Avg = 0; int nClustersCount = 0;
            // Computing the average distance value between each newly built cluster
            for (int iCluster = 1; iCluster < mTargetClusters.Count(); iCluster++)
                for (int nCluster = iCluster + 1; nCluster < mTargetClusters.Count(); nCluster++)
                {
                    nD0Avg += mTargetClusters[iCluster].Centroids[0].EuclDW(
                            mTargetClusters[nCluster].Centroids[0]);

                    nClustersCount++;
                }

            nD0Avg /= nClustersCount; // Computing the average distance between clusters
            // Computing the prediction quality value of the array of the newly built clusters.
            double nQ = ((mTargetClusters.Count() - 1) * nDiAvg) / nD0Avg;

            // Performing the k-means clustering results verbose output
            //for (int iCluster = 0; iCluster < mTargetClusters.Count(); iCluster++)
            //{
            //    IItemsList ItemsList = mTargetClusters[iCluster].Items;
            //    IItemsList Centroids = mTargetClusters[iCluster].Centroids;
            //    Console.WriteLine("\nCluster={0}, Centroid=\"{1}\"", iCluster, Centroids[0].ItemText);
            //    Console.WriteLine("-----------------------------------------------------------");

            //    for (int iAttrib = 0; iAttrib < Centroids[0].AttributeList.Count(); iAttrib++)
            //        Console.WriteLine("\"{0,-30}\" => u({1},{2}) = {3}", Centroids[0].AttributeList[iAttrib].Name,
            //            iCluster, iAttrib, Centroids[0].AttributeList[iAttrib].Value);

            //    Console.WriteLine("-----------------------------------------------------------");

            //    for (int iItem = 0; iItem < ItemsList.Count(); iItem++)
            //    {
            //        Console.WriteLine("\n(cluster={0}, item={1}, distance={2})\n{3}",
            //            iCluster, iItem, ItemsList[iItem].Distance, ItemsList[iItem].ItemText);
            //        Console.WriteLine("-----------------------------------------------------------");
            //        for (int iAttrib = 0; iAttrib < ItemsList[iItem].AttributeList.Count(); iAttrib++)
            //            Console.WriteLine("\"{0,-30}\" => i({1},{2}) = {3}", ItemsList[iItem].AttributeList[iAttrib].Name,
            //                iCluster, iAttrib, ItemsList[iItem].AttributeList[iAttrib].Value);

            //        Console.WriteLine("-----------------------------------------------------------");
            //    }
            //}

           // Console.WriteLine("\n===========================================================");
            Console.WriteLine("\nRecommendations:");
            Console.WriteLine("===========================================================\n");

            for (int iCluster = 0; iCluster < mTargetClusters.Count(); iCluster++)
            {
                IItemsList ItemsList = mTargetClusters[iCluster].Items;
                Console.WriteLine("{0}", mTargetClusters[iCluster].Centroids[0].ItemText);
                Console.WriteLine("======================================================");

                for (int iItem = 0; iItem < ItemsList.Count(); iItem++)
                    Console.WriteLine("{0}", ItemsList[iItem].ItemText);

                Console.WriteLine();
            }

            //Console.WriteLine("KMeans Statistics:");
            //Console.WriteLine("===========================================================");
            //Console.WriteLine("The total number of clusters nClustersCount = {0}\n", mTargetClusters.Count());
            //Console.WriteLine("Average distance between clusters nDiAvg = {0}", nDiAvg);
            //Console.WriteLine("Average distance within a cluster nD0Avg = {0}\n", nD0Avg);
            //Console.WriteLine("Average quality of KMeans clustering nQ = {0}\n", nQ);
        }

        private void Swap(ref IAttributeList attributes, int indexA, int indexB)
        {
            Attribute temp = attributes[indexA];
            attributes[indexA] = attributes[indexB];
            attributes[indexB] = temp;
        }

        private void Normalize(IItemsList items_list, int n_attribs,
            bool is_centroids, ref double min_val, ref double max_val)
        {
            // Performing a check if the minimum and maximum value are equal to 0
            if (min_val == 0 && max_val == 0)
            {
                // Assigning the initial values to min_val and max_val variable,
                // which represent the boundaries at which the value of each attribute is normalized
                min_val = (double)1 / n_attribs;
                max_val = (double)n_attribs / (n_attribs + 1);
            }

            // Iterating through the array of items and for each item items_list[iItem]
            // performing normalization by distributing the value of each attribute in
            // the range of [0;1] using local extremum formula
            for (int iItem = 0; iItem < items_list.Count(); iItem++)
            {
                // For the current item items_list[iItem].AttribList retriving the array of attributes
                IAttributeList AttribsTarget = items_list[iItem].AttributeList;
                // Iterating through the array of attributes and for each attribute perform normalization
                // by converting its value into the value from the range [0;1] using the following formula
                for (int iAttrib = 0; iAttrib < AttribsTarget.Count(); iAttrib++)
                    // Performing a check if the value of the current attribute AttribsTarget[iAttrib].Value
                    // exceeding the [0;1] range and this is not the centroid's attribute
                    if (AttribsTarget[iAttrib].Value > 1 || is_centroids == false)
                        // If so, applying the following formula to normalize the current attribute value
                        AttribsTarget[iAttrib].Value = ((AttribsTarget[iAttrib].Value /
                            (n_attribs + 1)) - min_val) / (max_val - min_val) + 0.01;
            }
        }

    }
}
