using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender
{
    public class Attribute : ICloneable
    {
        public string Name { get; set; }
        public double Value { get; set; }

        public Attribute(string name, double value)
        {
            Name = name;
            Value = value;
        }
        public object Clone()
        {
            Attribute TargetAttribute = (Attribute)this.MemberwiseClone();
            TargetAttribute.Name = Name; TargetAttribute.Value = Value;
            return TargetAttribute;
        }
    }
}
