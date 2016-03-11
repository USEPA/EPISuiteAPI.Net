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
    [RoutePrefix("epiSuiteCalcs")]
    public class EpiSuiteCalcsController : ApiController
    {
        [Route("boilingPtDegCEstimated")]
        [HttpPost]
        public HttpResponseMessage boilingPtDegCEstimated(Chemical chemical)
        {
            try
            {
                EPIReader epiReader = new EPIReader();
                ChemicalProperty chemProp = epiReader.ReadSumbrief("mpbpnt.exe", "Boiling Pt (deg C)(estimated)", chemical.structure);
                return Request.CreateResponse(System.Net.HttpStatusCode.OK, chemProp);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("meltingPtDegCEstimated")]
        [HttpPost]
        public HttpResponseMessage meltingPtDegCEstimated(Chemical chemical)
        {
            try
            {
                EPIReader epiReader = new EPIReader();
                ChemicalProperty chemProp = epiReader.ReadSumbrief("mpbpnt.exe", "Melting Pt (deg C)(estimated)", chemical.structure);
                return Request.CreateResponse(System.Net.HttpStatusCode.OK, chemProp);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("vaporPressMmHgEstimated")]
        [HttpPost]
        public HttpResponseMessage vaporPressMmHgEstimated(Chemical chemical)
        {
            try
            {
                EPIReader epiReader = new EPIReader();
                ChemicalProperty chemProp = epiReader.ReadSumbrief("mpbpnt.exe", "Vapor Press (mm Hg)(estimated)", chemical.structure);
                return Request.CreateResponse(System.Net.HttpStatusCode.OK, chemProp);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("waterSolMgLEstimated")]
        [HttpPost]
        public HttpResponseMessage waterSolMgLEstimated(Chemical chemical)
        {
            try
            {
                EPIReader epiReader = new EPIReader();
                ChemicalProperty chemProp = epiReader.ReadSumbrief("wskownt.exe", "Water Sol (mg/L)(estimated)", chemical.structure);
                return Request.CreateResponse(System.Net.HttpStatusCode.OK, chemProp);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("henryLcBondAtmM3Mole")]
        [HttpPost]
        public HttpResponseMessage henryLcBondAtmM3Mole(Chemical chemical)
        {
            try
            {
                EPIReader epiReader = new EPIReader();
                ChemicalProperty chemProp = epiReader.ReadHenryVal("henrynt.exe", "Henry's Law Constant (atm-m3/mol)", chemical.structure);
                string tmp = Newtonsoft.Json.JsonConvert.SerializeObject(chemProp);
                return Request.CreateResponse(System.Net.HttpStatusCode.OK, chemProp);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("logKowEstimate")]
        [HttpPost]
        public HttpResponseMessage logKowEstimate(Chemical chemical)
        {
            try
            {
                EPIReader epiReader = new EPIReader();
                ChemicalProperty chemProp = epiReader.ReadSumbrief("kowwinnt.exe", "Log Kow (estimate)", chemical.structure);
                return Request.CreateResponse(System.Net.HttpStatusCode.OK, chemProp);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [Route("soilAdsorptionCoefKoc")]
        [HttpPost]
        public HttpResponseMessage soilAdsorptionCoefKoc(Chemical chemical)
        {
            try
            {
                EPIReader epiReader = new EPIReader();
                ChemicalProperty chemProp = epiReader.ReadSumbrief("pckocnt.exe", "Soil Adsorption Coef (Koc)", chemical.structure);
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
                EPIReader epiReader = new EPIReader();

                ChemicalProperty chemProp = epiReader.ReadSumbrief("mpbpnt.exe", "Boiling Pt (deg C)(estimated)", chemical.structure);
                chemProps.properties.Add(chemProp);

                ChemicalProperty chemProp2 = epiReader.ReadSumbrief("mpbpnt.exe", "Melting Pt (deg C)(estimated)", chemical.structure);
                chemProps.properties.Add(chemProp2);

                ChemicalProperty chemProp3 = epiReader.ReadSumbrief("mpbpnt.exe", "Vapor Press (mm Hg)(estimated)", chemical.structure);
                chemProps.properties.Add(chemProp3);

                ChemicalProperty chemProp4 = epiReader.ReadSumbrief("wskownt.exe", "Water Sol (mg/L)(estimated)", chemical.structure);
                chemProps.properties.Add(chemProp4);

                ChemicalProperty chemProp5 = epiReader.ReadHenryVal("henrynt.exe", "Henry's Law Constant (atm-m3/mol)", chemical.structure);
                chemProps.properties.Add(chemProp5);

                ChemicalProperty chemProp6 = epiReader.ReadSumbrief("kowwinnt.exe", "Log Kow (estimate)", chemical.structure);
                chemProps.properties.Add(chemProp6);

                ChemicalProperty chemProp7 = epiReader.ReadSumbrief("pckocnt.exe", "Soil Adsorption Coef (Koc)", chemical.structure);
                chemProps.properties.Add(chemProp7);

                return Request.CreateResponse(System.Net.HttpStatusCode.OK, chemProps);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }            
        }
    }
}
