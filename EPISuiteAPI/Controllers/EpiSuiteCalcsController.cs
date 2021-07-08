using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Net.Http;
using EPISuiteAPI.Models;
using EPISuiteAPI.Util;

namespace EPISuiteAPI.Controllers
{
    [RoutePrefix("rest/episuite")]
    public class EpiSuiteCalcsController : ApiController
    {
        [Route("test")]
        [HttpGet]
        public HttpResponseMessage Test()
        {
            return Request.CreateResponse(System.Net.HttpStatusCode.OK, "You successfully called EPISuiteAPI");           
        }

        [Route("testcalc")]
        [HttpGet]
        public HttpResponseMessage TestCalc()
        {
            string smiles = "c1ccccc1";
            string meltingPoint = "20.0";
            try
            {
                EPIReader epiReader = new EPIReader();
                ChemicalProperties chemProps = epiReader.GetEstimatedProperties("epiwin1.exe", smiles, meltingPoint);
                epiReader.Close();
                return Request.CreateResponse(System.Net.HttpStatusCode.OK, chemProps);
            }
            catch(Exception ex)
            {
                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("estimated")]
        [HttpPost]
        public HttpResponseMessage Estimated(Chemical chemical)
        {
            try
            {
                string smiles = chemical.structure;
                string meltingPoint = "";
                if (chemical.melting_point == null)
                    meltingPoint = "(null)";
                else
                    meltingPoint = chemical.melting_point.ToString();
                EPIReader epiReader = new EPIReader();
                ChemicalProperties chemProps = epiReader.GetEstimatedProperties("epiwin1.exe", smiles, meltingPoint);
                epiReader.Close();
                return Request.CreateResponse(System.Net.HttpStatusCode.OK, chemProps);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("measured")]
        [HttpPost]
        public HttpResponseMessage Measured(Chemical chemical)
        {
            try
            {
                string smiles = chemical.structure;
                string meltingPoint = "";
                if (chemical.melting_point == null)
                    meltingPoint = "(null)";
                else
                    meltingPoint = chemical.melting_point.ToString();
                EPIReader epiReader = new EPIReader();
                ChemicalProperties chemProps = epiReader.GetMeasuredProperties("epiwin1.exe", smiles, meltingPoint);
                epiReader.Close();
                return Request.CreateResponse(System.Net.HttpStatusCode.OK, chemProps);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        ////boilingPtDegCEstimated
        //[Route("boilingPoint/estimated")]
        //[HttpPost]
        //public HttpResponseMessage boilingPtDegCEstimated(Chemical chemical)
        //{
        //    try
        //    {
        //        EPIReader epiReader = new EPIReader();
        //        ChemicalProperty chemProp = epiReader.GetEstimatedProperty("mpbpnt.exe", "Boiling Pt (deg C)(estimated)", chemical.structure);
        //        epiReader.Close();
        //        ChemicalProperties chemProps = new ChemicalProperties();
        //        chemProp.prop = Globals.BOILING_POINT;
        //        chemProps.data.Add(chemProp);                
        //        return Request.CreateResponse(System.Net.HttpStatusCode.OK, chemProps);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //}

        ////meltingPtDegCEstimated
        //[Route("meltingPoint/estimated")]
        //[HttpPost]
        //public HttpResponseMessage meltingPtDegCEstimated(Chemical chemical)
        //{
        //    try
        //    {
        //        EPIReader epiReader = new EPIReader();
        //        ChemicalProperty chemProp = epiReader.GetEstimatedProperty("mpbpnt.exe", "Melting Pt (deg C)(estimated)", chemical.structure);
        //        epiReader.Close();

        //        ChemicalProperties chemProps = new ChemicalProperties();
        //        chemProp.prop = Globals.MELTING_POINT;
        //        chemProps.data.Add(chemProp);

        //        return Request.CreateResponse(System.Net.HttpStatusCode.OK, chemProps);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //}

        ////vaporPressMmHgEstimated
        //[Route("vaporPressure/estimated")]
        //[HttpPost]
        //public HttpResponseMessage vaporPressMmHgEstimated(Chemical chemical)
        //{
        //    try
        //    {
        //        EPIReader epiReader = new EPIReader();
        //        ChemicalProperty chemProp = epiReader.GetEstimatedProperty("mpbpnt.exe", "Vapor Press (mm Hg)(estimated)", chemical.structure);
        //        epiReader.Close();

        //        ChemicalProperties chemProps = new ChemicalProperties();
        //        chemProp.prop = Globals.VAPOR_PRESSURE;
        //        chemProps.data.Add(chemProp);

        //        return Request.CreateResponse(System.Net.HttpStatusCode.OK, chemProps);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //}

        ////waterSolMgLEstimated
        //[Route("waterSolubility/estimated")]
        //[HttpPost]
        //public HttpResponseMessage waterSolMgLEstimated(Chemical chemical)
        //{
        //    try
        //    {
        //        ChemicalProperties chemProps = new ChemicalProperties();
        //        EPIReader epiReader = new EPIReader();
        //        ChemicalProperty chemProp = epiReader.GetEstimatedProperty("wskownt.exe", "Water Sol (mg/L)(estimated)", chemical.structure, chemical.melting_point);
        //        //epiReader.Close();
        //        chemProp.prop = Globals.WATER_SOLUBILITY;
        //        chemProp.method = "wskownt";
        //        chemProps.data.Add(chemProp);

        //        //epiReader = new EPIReader();
        //        ChemicalProperty chemProp2 = epiReader.GetEstimatedProperty("waternt.exe", "WSol (mg/L frag est)", chemical.structure, chemical.melting_point);
        //        chemProp2.prop = Globals.WATER_SOLUBILITY;
        //        chemProp2.method = "waternt";
        //        chemProps.data.Add(chemProp2);
        //        epiReader.Close();


        //        return Request.CreateResponse(System.Net.HttpStatusCode.OK, chemProps);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //}

        ////henryLcBondAtmM3Mole
        //[Route("henrysLawConstant/estimated")]
        //[HttpPost]
        //public HttpResponseMessage henryLcBondAtmM3Mole(Chemical chemical)
        //{
        //    try
        //    {
        //        EPIReader epiReader = new EPIReader();
        //        ChemicalProperty chemProp = epiReader.GetEstimatedProperty("henrynt.exe", "Henry's Law Constant (atm-m3/mol)", chemical.structure);
        //        epiReader.Close();
        //        ChemicalProperties chemProps = new ChemicalProperties();
        //        chemProp.prop = Globals.HENRY_LAW;
        //        chemProps.data.Add(chemProp);
        //        //string tmp = Newtonsoft.Json.JsonConvert.SerializeObject(chemProp);
        //        return Request.CreateResponse(System.Net.HttpStatusCode.OK, chemProps);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //}

        ////logKowEstimate
        //[Route("logKow/estimated")]
        //[HttpPost]
        //public HttpResponseMessage logKowEstimate(Chemical chemical)
        //{
        //    try
        //    {
        //        EPIReader epiReader = new EPIReader();
        //        ChemicalProperty chemProp = epiReader.GetEstimatedProperty("kowwinnt.exe", "Log Kow (estimate)", chemical.structure);
        //        epiReader.Close();
        //        ChemicalProperties chemProps = new ChemicalProperties();
        //        chemProp.prop = Globals.LOG_KOW;
        //        chemProps.data.Add(chemProp);
        //        return Request.CreateResponse(System.Net.HttpStatusCode.OK, chemProps);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //}

        ////soilAdsorptionCoefKoc
        //[Route("soilAdsorptionCoefficientKoc/estimated")]
        //[HttpPost]
        //public HttpResponseMessage soilAdsorptionCoefKoc(Chemical chemical)
        //{
        //    try
        //    {
        //        EPIReader epiReader = new EPIReader();
        //        ChemicalProperty chemProp = epiReader.GetEstimatedProperty("pckocnt.exe", "Soil Adsorption Coef (Koc)", chemical.structure);
        //        epiReader.Close();
        //        ChemicalProperties chemProps = new ChemicalProperties();
        //        chemProp.prop = Globals.LOG_KOC;
        //        chemProps.data.Add(chemProp);
        //        return Request.CreateResponse(System.Net.HttpStatusCode.OK, chemProps);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //}

        //[Route("calculators")]
        //[HttpPost]
        //public HttpResponseMessage all(Chemical chemical)
        //{
        //    ChemicalProperties chemProps = new ChemicalProperties();
        //    try
        //    {
        //        string smiles = chemical.structure;
        //        EPIReader epiReader = new EPIReader();                

        //        epiReader.RunExecutable("mpbpnt.exe",smiles);
        //        ChemicalProperty chemProp = epiReader.ReadSumbrief("Boiling Pt (deg C)(estimated)", smiles);
        //        chemProp.prop = Globals.BOILING_POINT;
        //        chemProps.data.Add(chemProp);

        //        ChemicalProperty chemProp2 = epiReader.ReadSumbrief("Melting Pt (deg C)(estimated)", smiles);
        //        chemProp2.prop = Globals.MELTING_POINT;
        //        chemProps.data.Add(chemProp2);

        //        ChemicalProperty chemProp3 = epiReader.ReadSumbrief("Vapor Press (mm Hg)(estimated)", smiles);
        //        chemProp3.prop = Globals.VAPOR_PRESSURE;
        //        chemProps.data.Add(chemProp3);

        //        epiReader.RunExecutable("wskownt.exe", smiles);
        //        ChemicalProperty chemProp4 = epiReader.ReadSumbrief("Water Sol (mg/L)(estimated)", smiles);
        //        chemProp4.prop = Globals.WATER_SOLUBILITY;
        //        chemProps.data.Add(chemProp4);

        //        epiReader.RunExecutable("henrynt.exe", smiles);
        //        ChemicalProperty chemProp5 = epiReader.ReadHenryVal("Henry's Law Constant (atm-m3/mol)", smiles);
        //        chemProp5.prop = Globals.HENRY_LAW;
        //        chemProps.data.Add(chemProp5);

        //        epiReader.RunExecutable("kowwinnt.exe", smiles);
        //        ChemicalProperty chemProp6 = epiReader.ReadSumbrief("Log Kow (estimate)", smiles);
        //        chemProp6.prop = Globals.LOG_KOW;
        //        chemProps.data.Add(chemProp6);

        //        epiReader.RunExecutable("pckocnt.exe", smiles);
        //        ChemicalProperty chemProp7 = epiReader.ReadSumbrief("Soil Adsorption Coef (Koc)", smiles);
        //        chemProp7.prop = Globals.LOG_KOC;
        //        chemProps.data.Add(chemProp7);

        //        epiReader.Close();

        //        return Request.CreateResponse(System.Net.HttpStatusCode.OK, chemProps);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
        //    }            
        //}


    }
}
