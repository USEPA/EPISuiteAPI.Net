using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPISuiteAPI.Models
{
    public class ChemicalProperty
    {
        public string structure { get; set; }
        public string propertyname { get; set; }
        public double propertyvalue { get; set; }
    }

    public class ChemicalProperties
    {
        public ChemicalProperties()
        {
            properties = new List<ChemicalProperty>();
        }
        public List<ChemicalProperty> properties{get; set;}
    }
}