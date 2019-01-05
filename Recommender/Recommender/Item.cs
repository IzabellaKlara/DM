using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender
{
    public class Item : ICloneable
    {
        public bool IsCentroid { get; set; }
        public bool Exists { get; set; }
        public string ItemText { get; set; }
        public double Distance { get; set; }

        public IAttributeList AttributeList = null;

        public Item(string item_text, IAttributeList attributes,
            double distance, bool is_user, bool exists)
        {
            if (AttributeList == null)
                AttributeList = (IAttributeList)attributes;

            IsCentroid = is_user;
            Exists = exists;
            ItemText = item_text;
            Distance = distance;
        }

        public IAttributeList GetAttributeList()
        {
            return AttributeList;
        }

        public object Clone()
        {
            Item TargetItem = (Item)this.MemberwiseClone();
            IAttributeList TargetAttributeList = new IAttributeList();
            foreach (Attribute attribute in this.AttributeList)
                TargetAttributeList.Add(attribute);

            if (TargetAttributeList.Count() > 0)
                TargetItem.AttributeList = (IAttributeList)TargetAttributeList.Clone();

            TargetItem.IsCentroid = this.IsCentroid;
            TargetItem.Exists = this.Exists;
            TargetItem.ItemText = this.ItemText;
            TargetItem.Distance = this.Distance;

            return TargetItem;
        }

        private double Relevance(string firstAttribute, string secondAttribute)
        {
            double nRelevance = 0;
            // Assigning the nLength variable the value of the smallest string length
            int nLength = firstAttribute.Length < secondAttribute.Length ? firstAttribute.Length : secondAttribute.Length;
            // Iterating through the two strings of character, comparing the pairs of items
            // from either firstAttribute and secondAttribute. If the two characters are lexicographically equal
            // we're adding the value 1 / nLength to the nRelevance variable
            for (int iIndex = 0; iIndex < nLength; iIndex++)
                nRelevance += (firstAttribute[iIndex] == secondAttribute[iIndex]) ? (double)1 / nLength : 0;

            return nRelevance;
        }

        public double EuclDW(Item item)
        {
            int nCount = 0;
            int iIndex = 0; double nDistance = 0;
            // Iterating through the array of attributes and for each pair of either users or items
            // attributes computing the distance between those attributes. Then, each distance values
            // is added to the nDistance variable. During the computation we're also obtaining the
            // value of releavance between the lexicographical representations of those attributes
            while (iIndex < item.AttributeList.Count() && iIndex < AttributeList.Count())
            {
                // Compute the relevance between names of the pair of attributes
                double nRel = Relevance(item.AttributeList[iIndex].Name, AttributeList[iIndex].Name);
                if (nRel == 1) nCount++;

                // Computing the Eucledean distance between the pair of current attributes
                nDistance += Math.Pow(item.AttributeList[iIndex].Value - AttributeList[iIndex].Value, 2.0) *
                    ((double)((nRel > 0) ? nRel : 1));

                iIndex++;
            }

            // Returning the value of the distance between two vectors of attributes
            return Math.Sqrt(nDistance) * ((double)1 / ((nCount > 0) ? nCount : 0.01));
        }
    }
}
