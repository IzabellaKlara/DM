using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender
{
    public class IAttributeList : ICloneable, IEnumerable<Attribute>
    {
        public List<Attribute> AttributeList = null;

        public IAttributeList()
        {
            if (AttributeList == null)
                AttributeList = new List<Attribute>();
        }

        public void Add(Attribute attrib_item)
        {
            AttributeList.Add((Attribute)attrib_item.Clone());
        }

        public object Clone()
        {
            IAttributeList TargetAttributeList = new IAttributeList();
            foreach (Attribute attribute in AttributeList)
                TargetAttributeList.Add(attribute);

            return TargetAttributeList.Count() > 0 ?
               (IAttributeList)TargetAttributeList.Clone() : null;
        }

        public Attribute this[int iIndex]
        {
            get { return AttributeList[iIndex]; }
            set { AttributeList[iIndex] = value; }
        }

        public int Count() { return AttributeList.Count(); }
        public IEnumerator GetEnumerator()
        {
            return AttributeList.GetEnumerator();
        }
        IEnumerator<Attribute> IEnumerable<Attribute>.GetEnumerator()
        {
            return (IEnumerator<Attribute>)this.GetEnumerator();
        }
    }
}
