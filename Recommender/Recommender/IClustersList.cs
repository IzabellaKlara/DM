using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender
{
    public class IClustersList : ICloneable, IEnumerable<ICluster>
    {
        public List<ICluster> ClustersList = null;
        public IClustersList()
        {
            if (ClustersList == null)
                ClustersList = new List<ICluster>();
        }
        public void Add(ICluster cluster)
        {
            ClustersList.Add(cluster);
        }
        public object Clone()
        {
            IClustersList TargetClustersList = new IClustersList();
            foreach (ICluster cluster in ClustersList)
            {
                IItemsList TargetCentroidsList = new IItemsList();
                foreach (Item centroid in (IItemsList)cluster.Centroids.Clone())
                    TargetCentroidsList.Add(new Item(centroid.ItemText, (IAttributeList)centroid.AttributeList.Clone(),
                        centroid.Distance, centroid.IsCentroid, centroid.Exists));

                IItemsList TargetItemsList = new IItemsList();
                foreach (Item item in (IItemsList)cluster.Items.Clone())
                    TargetItemsList.Add(new Item(item.ItemText, (IAttributeList)item.AttributeList.Clone(),
                        item.Distance, item.IsCentroid, item.Exists));

                TargetClustersList.Add(new ICluster((IItemsList)TargetCentroidsList.Clone(),
                    (IItemsList)TargetItemsList.Clone()));
            }

            return TargetClustersList;
        }
        public ICluster this[int iIndex]
        {
            get { return ClustersList[iIndex]; }
        }
        public int Count() { return ClustersList.Count(); }
        public IEnumerator GetEnumerator()
        {
            return ClustersList.GetEnumerator();
        }
        IEnumerator<ICluster> IEnumerable<ICluster>.GetEnumerator()
        {
            return (IEnumerator<ICluster>)this.GetEnumerator();
        }
    }
}
