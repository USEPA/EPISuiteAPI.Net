using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPISuiteAPI.Models
{    

    public class ChemicalProperty
    {
        public string chemical { get; set; }
        public string prop { get; set; }
        public string calc { get; set; }
        public string method { get; set; }
        public string data { get; set; }

        public ChemicalProperty()
        {
            chemical = null;
            prop = null;
            calc = "epi";
            method = null;
            data = null;
        }
    }

    public class ChemicalProperties
    {
        public ChemicalProperties()
        {
            data = new List<ChemicalProperty>();
        }
        public List<ChemicalProperty> data { get; set; }

    }

    

    //public class ChemicalProperty
    //{
    //    public string structure { get; set; }
    //    public string propertyname { get; set; }
    //    //public double propertyvalue { get; set; }
    //    public string propertyvalue { get; set; }
    //}

    //public class ChemicalProperties
    //{
    //    public ChemicalProperties()
    //    {
    //        //properties = new List<ChemicalProperty>();
    //        properties = new Dictionary<string, ChemicalProperty>();
    //    }
    //    //public List<ChemicalProperty> properties{get; set;}
    //    public Dictionary<string,ChemicalProperty> properties { get; set; }
    //}
}