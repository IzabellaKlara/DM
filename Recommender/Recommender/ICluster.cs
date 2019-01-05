using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender
{
    public class ICluster : ICloneable
    {
        public IItemsList Items { get; set; }
        public IItemsList Centroids { get; set; }
        public ICluster(IItemsList cnts_list, IItemsList items_list)
        {
            Items = items_list;
            Centroids = cnts_list;
        }
        public object Clone()
        {
            IItemsList TargetItemsList = new IItemsList();
            IItemsList TargetCentroidsList = new IItemsList();
            ICluster TargetCluster = (ICluster)this.MemberwiseClone();

            foreach (Item centroid in Centroids)
                TargetCentroidsList.Add(centroid);

            foreach (Item item in Items)
                TargetItemsList.Add(item);

            TargetCluster.Items = TargetItemsList;
            TargetCluster.Centroids = TargetCentroidsList;

            return TargetCluster;
        }
    }
}
