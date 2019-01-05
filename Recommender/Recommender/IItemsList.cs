using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender
{
    public class IItemsList : ICloneable, IEnumerable<Item>
    {
        private List<Item> ItemsList = null;
        public IItemsList()
        {
            if (ItemsList == null)
                ItemsList = new List<Item>();
        }
        public void Add(Item item)
        {
            ItemsList.Add(item);
        }
        public object Clone()
        {
            IItemsList TargetItems = new IItemsList();
            foreach (Item item in ItemsList)
            {
                IAttributeList TargetAttributeList = new IAttributeList();
                foreach (Attribute attrib in item.GetAttributeList())
                    TargetAttributeList.Add(new Attribute(attrib.Name, attrib.Value));

                if (TargetAttributeList.Count() > 0)
                    TargetItems.Add(new Item(item.ItemText, TargetAttributeList,
                        item.Distance, item.IsCentroid, item.Exists));
            }

            return TargetItems;
        }
        public Item this[int iIndex]
        {
            get { return ItemsList[iIndex]; }
            set { ItemsList[iIndex] = value; }
        }
        public int Count() { return ItemsList.Count(); }
        public void RemoveAt(int iIndex) { ItemsList.RemoveAt(iIndex); }
        public IEnumerator GetEnumerator()
        {
            return ItemsList.GetEnumerator();
        }
        IEnumerator<Item> IEnumerable<Item>.GetEnumerator()
        {
            return (IEnumerator<Item>)this.GetEnumerator();
        }
    }
}
