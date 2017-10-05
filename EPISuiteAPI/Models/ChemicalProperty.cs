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
        //public double propertyvalue { get; set; }
        public string propertyvalue { get; set; }
    }

    public class ChemicalProperties
    {
        public ChemicalProperties()
        {
            //properties = new List<ChemicalProperty>();
            properties = new Dictionary<string, ChemicalProperty>();
        }
        //public List<ChemicalProperty> properties{get; set;}
        public Dictionary<string,ChemicalProperty> properties { get; set; }
    }
}