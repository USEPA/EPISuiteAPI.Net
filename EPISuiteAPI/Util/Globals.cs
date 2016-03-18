using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPISuiteAPI.Util
{
    public class Globals
    {
        public const string EPI_SUITE_PATH = @"c:\episuite41\";
        public const string XSMILES = @"xsmiles";
        public const string SUMBRIEF = @"sumbrief.epi";
        public const string HENRYVAL = @"henryval";

        public const string MP_BP_EXE           = "mpbpnt.exe";
        public const string BP_EST              = "Boiling Pt (deg C)(estimated)";
        public const string MP_EST              = "Melting Pt (deg C)(estimated)";
        public const string VP_MMHG_EST         = "Vapor Press (mm Hg)(estimated)";
        //public const string MP_SUPERCOOL_EST    = "Vapor Press (supercool)(mm Hg)";

        public const string WATER_SOL_EXE       = "wskownt.exe";
        public const string WATER_SOL_EST       = "Water Sol (mg/L)(estimated)";

        public const string HENRY_LAW_EXE       = "henrynt.exe";
        public const string HENRY_LAW_DESC      = "Henry LC (bond)(atm-m3/mole)";

        public const string LOG_KOW_EXE         = "kowwinnt.exe";
        public const string LOG_KOW_EST         = "Log Kow (estimate)";
        //public const string LOG_KOW_DB_MATCH    = "Log Kow (database match)";

        public const string KOC_EXE             = "pckocnt.exe";
        public const string KOC_EST             = "Soil Adsorption Coef (Koc)";

        public const string BOILING_POINT       = "boiling_point";
        public const string MELTING_POINT       = "melting_point";
        public const string VAPOR_PRESSURE      = "vapor_pressure";
        public const string WATER_SOLUBILITY    = "water_solubility";
        public const string HENRY_LAW           = "henrys_law_constant";
        public const string LOG_KOW             = "log_kow";
        public const string LOG_KOC             = "log_koc";


    }
}