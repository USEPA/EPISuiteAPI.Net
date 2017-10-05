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
        //boilingPtDegCEstimated
        [Route("boilingPoint/estimated")]
        [HttpPost]
        public HttpResponseMessage boilingPtDegCEstimated(Chemical chemical)
        {
            try
            {
                EPIReader epiReader = new EPIReader();
                ChemicalProperty chemProp = epiReader.GetEstimatedProperty("mpbpnt.exe", "Boiling Pt (deg C)(estimated)", chemical.structure);
                epiReader.Close();
                return Request.CreateResponse(System.Net.HttpStatusCode.OK, chemProp);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        //meltingPtDegCEstimated
        [Route("meltingPoint/estimated")]
        [HttpPost]
        public HttpResponseMessage meltingPtDegCEstimated(Chemical chemical)
        {
            try
            {
                EPIReader epiReader = new EPIReader();
                ChemicalProperty chemProp = epiReader.GetEstimatedProperty("mpbpnt.exe", "Melting Pt (deg C)(estimated)", chemical.structure);
                epiReader.Close();
                return Request.CreateResponse(System.Net.HttpStatusCode.OK, chemProp);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        //vaporPressMmHgEstimated
        [Route("vaporPressure/estimated")]
        [HttpPost]
        public HttpResponseMessage vaporPressMmHgEstimated(Chemical chemical)
        {
            try
            {
                EPIReader epiReader = new EPIReader();
                ChemicalProperty chemProp = epiReader.GetEstimatedProperty("mpbpnt.exe", "Vapor Press (mm Hg)(estimated)", chemical.structure);
                epiReader.Close();
                return Request.CreateResponse(System.Net.HttpStatusCode.OK, chemProp);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        //waterSolMgLEstimated
        [Route("waterSolubility/estimated")]
        [HttpPost]
        public HttpResponseMessage waterSolMgLEstimated(Chemical chemical)
        {
            try
            {
                EPIReader epiReader = new EPIReader();
                ChemicalProperty chemProp = epiReader.GetEstimatedProperty("wskownt.exe", "Water Sol (mg/L)(estimated)", chemical.structure);
                epiReader.Close();
                return Request.CreateResponse(System.Net.HttpStatusCode.OK, chemProp);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        //henryLcBondAtmM3Mole
        [Route("henrysLawConstant/estimated")]
        [HttpPost]
        public HttpResponseMessage henryLcBondAtmM3Mole(Chemical chemical)
        {
            try
            {
                EPIReader epiReader = new EPIReader();
                ChemicalProperty chemProp = epiReader.GetEstimatedProperty("henrynt.exe", "Henry's Law Constant (atm-m3/mol)", chemical.structure);
                epiReader.Close();
                string tmp = Newtonsoft.Json.JsonConvert.SerializeObject(chemProp);
                return Request.CreateResponse(System.Net.HttpStatusCode.OK, chemProp);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        //logKowEstimate
        [Route("logKow/estimated")]
        [HttpPost]
        public HttpResponseMessage logKowEstimate(Chemical chemical)
        {
            try
            {
                EPIReader epiReader = new EPIReader();
                ChemicalProperty chemProp = epiReader.GetEstimatedProperty("kowwinnt.exe", "Log Kow (estimate)", chemical.structure);
                epiReader.Close();
                return Request.CreateResponse(System.Net.HttpStatusCode.OK, chemProp);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        //soilAdsorptionCoefKoc
        [Route("soilAdsorptionCoefficientKoc/estimated")]
        [HttpPost]
        public HttpResponseMessage soilAdsorptionCoefKoc(Chemical chemical)
        {
            try
            {
                EPIReader epiReader = new EPIReader();
                ChemicalProperty chemProp = epiReader.GetEstimatedProperty("pckocnt.exe", "Soil Adsorption Coef (Koc)", chemical.structure);
                epiReader.Close();
                return Request.CreateResponse(System.Net.HttpStatusCode.OK, chemProp);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("calculators")]
        [HttpPost]
        public HttpResponseMessage all(Chemical chemical)
        {
            ChemicalProperties chemProps = new ChemicalProperties();
            try
            {
                string smiles = chemical.structure;
                EPIReader epiReader = new EPIReader();                
                               
                epiReader.RunExecutable("mpbpnt.exe",smiles);
                ChemicalProperty chemProp = epiReader.ReadSumbrief("Boiling Pt (deg C)(estimated)", smiles);
                chemProps.properties.Add(Globals.BOILING_POINT, chemProp);

                ChemicalProperty chemProp2 = epiReader.ReadSumbrief("Melting Pt (deg C)(estimated)", smiles);
                chemProps.properties.Add(Globals.MELTING_POINT, chemProp2);

                ChemicalProperty chemProp3 = epiReader.ReadSumbrief("Vapor Press (mm Hg)(estimated)", smiles);
                chemProps.properties.Add(Globals.VAPOR_PRESSURE, chemProp3);

                epiReader.RunExecutable("wskownt.exe", smiles);
                ChemicalProperty chemProp4 = epiReader.ReadSumbrief("Water Sol (mg/L)(estimated)", smiles);
                chemProps.properties.Add(Globals.WATER_SOLUBILITY, chemProp4);

                epiReader.RunExecutable("henrynt.exe", smiles);
                ChemicalProperty chemProp5 = epiReader.ReadHenryVal("Henry's Law Constant (atm-m3/mol)", smiles);
                chemProps.properties.Add(Globals.HENRY_LAW, chemProp5);

                epiReader.RunExecutable("kowwinnt.exe", smiles);
                ChemicalProperty chemProp6 = epiReader.ReadSumbrief("Log Kow (estimate)", smiles);
                chemProps.properties.Add(Globals.LOG_KOW, chemProp6);

                epiReader.RunExecutable("pckocnt.exe", smiles);
                ChemicalProperty chemProp7 = epiReader.ReadSumbrief("Soil Adsorption Coef (Koc)", smiles);
                chemProps.properties.Add(Globals.LOG_KOC, chemProp7);

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
    }
}
