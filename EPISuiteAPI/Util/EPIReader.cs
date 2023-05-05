using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using EPISuiteAPI.Models;
using EPISuiteAPI.Util;
using System.Data.Entity.Core.Metadata.Edm;


namespace EPISuiteAPI.Util
{
    public class EPIReader
    {
        //string _epiInstallFolder = @"C:\EPISUITE41";
        //string _epiWinExe = "epiwin1.exe";
        //string _epiInput = "epi_inp.txt";

        string _tempFolder { get; set; }

        public EPIReader()
        {
            _tempFolder = CreateTempFolder();
            CopyEPISuiteFiles();
        }

        public void Close()
        {
            if (Directory.Exists(_tempFolder))
                Directory.Delete(_tempFolder, true);
        }


        public ChemicalProperties GetHydrolysisProperty(string smiles, string propertyString, string acid_base = "Kb")
        {
            //string xsmilesFile = WriteHydroNTInput(smiles);
            string epiInputFile = WriteEpiInput(smiles, epiLink: "1");
            //RunExecutable("epiwin1.exe", smiles);
            RunEPIWinExecutable("epiwin1.exe", smiles, epiInputFile);

            ChemicalProperties chemProps = null;
            chemProps = ReadSummaryFileForHydrolysisHalfLife(propertyString, smiles, acid_base);

            return chemProps;

        }

        public ChemicalProperties GetThioPhosphateHydrolysisProperty(string smiles, string propertyString)
        {
            string xsmilesFile = WriteHydroNTInput(smiles);
            RunExecutable("hydront.exe", smiles);

            ChemicalProperties chemProps = null;
            chemProps = ReadSummaryFileForPhosphateThiophosphateHalfLife(propertyString, smiles);

            return chemProps;

        }

        public ChemicalProperties GetAnhydrideHydrolysisProperty(string smiles, string propertyString)
        {
            string xsmilesFile = WriteHydroNTInput(smiles);
            RunExecutable("hydront.exe", smiles);            

            ChemicalProperties chemProps = null;
            chemProps = ReadSummaryFileForAnhydrideHalfLife(propertyString, smiles);

            return chemProps;

        }
        public ChemicalProperty GetEstimatedProperty(string modelExe, string property, string smiles, double? melting_point = null)
        {
            //string tempFolder = CreateTempFolder();
            if (melting_point != null)
            {
                WriteVPBPMPFile(melting_point.ToString());
            }
            RunExecutable(modelExe, smiles);
            ChemicalProperty chemProp = null;
            if (string.Compare(modelExe, "henrynt.exe", true) != 0)
                chemProp = ReadSumbrief(property, smiles);
            else
                chemProp = ReadHenryVal(property, smiles);

            return chemProp;

        }

        public ChemicalProperties GetEstimatedProperties(string modelExe, string smiles, string meltingPoint = "(null)", string logKow = "(null)", string epiLink = "0")
        {
            string epiInputFile = WriteEpiInput(smiles, meltingPoint, logKow, epiLink);
            RunEPIWinExecutable(modelExe, smiles, epiInputFile);
            ChemicalProperties chemProps = null;
            chemProps = ReadSummaryFileForEstimatedValues(smiles);

            return chemProps;
        }

        public ChemicalProperties GetMeasuredProperties(string modelExe, string smiles, string meltingPoint = "(null)", string logKow = "(null)", string epiLink = "0")
        {
            string epiInputFile = WriteEpiInput(smiles, meltingPoint, logKow, epiLink);
            RunEPIWinExecutable(modelExe, smiles, epiInputFile);
            ChemicalProperties chemProps = null;
            chemProps = ReadSummaryFileForMeasuredValues();

            return chemProps;

            //if (chemProps == null)
            //{
            //double mp = 0;
            //string mp = "0";
            //string lKow = "0";
            //ChemicalProperty cp = chemProps.data.Find(i => i.prop == Globals.MELTING_POINT);
            //if (cp != null)
            //{
            //    mp = cp.data;
            //}

            //cp = null;
            //cp = chemProps.data.Find(i => i.prop == Globals.LOG_KOW);
            //if (cp != null)
            //{
            //    lKow = cp.data;
            //}
            //epiInputFile = WriteEpiInput(smiles, mp.ToString(), lKow.ToString());
            //RunEPIWinExecutable(modelExe, smiles, epiInputFile);
            //chemProps = ReadSummaryFile();
            ////}

            //return chemProps;

        }

        private ChemicalProperties ReadSummaryFileForEstimatedValues(string smiles)
        {
            ChemicalProperties chemProps = new ChemicalProperties();

            //string summary = ReadFile(Path.Combine(Globals.EPI_SUITE_PATH, "summary"));
            string summary = ReadFile(Path.Combine(_tempFolder, "summary"));
            if (string.IsNullOrWhiteSpace(summary))
                return null;

            int idx = 0;
            //bool bLogKow = false;
            //bool bMP = false;


            //Code for log kow
            //Parsing line like this:     Log Kow (KOWWIN v1.68 estimate) = 4.40            
            string logKowSearchText = "Log Kow (KOWWIN v1.68 estimate) =";
            idx = summary.IndexOf(logKowSearchText);
            //if (idx < 0)
            //bLogKow = false;
            //else
            if (idx >= 0)
            {
                //bLogKow = true;
                ChemicalProperty chemProp = GetPropertyFromSummaryString(summary, logKowSearchText, Globals.LOG_KOW, "=");
                chemProp.chemical = smiles;
                chemProp.prop = Globals.LOG_KOW;
                chemProps.data.Add(chemProp);
            }
            //End code log kow


            //Code for melting point
            //Parsing line like this:     Melting Pt (deg C):  53.61
            string MPSearchTest = "Melting Pt (deg C):";
            idx = summary.IndexOf(MPSearchTest);
            //if (idx < 0)
            //bMP = false;
            //else
            if (idx >= 0)
            {
                //bMP = true;
                ChemicalProperty chemProp = GetPropertyFromSummaryString(summary, MPSearchTest, Globals.MELTING_POINT, ":");
                chemProp.chemical = smiles;
                chemProp.prop = Globals.MELTING_POINT;
                chemProp.units = "C";
                chemProps.data.Add(chemProp);
            }
            //End code for melting point


            //Code for vapor pressure
            //Parsing line like this:     VP(mm Hg,25 deg C):  0.00443
            string VPSearchText = "VP(mm Hg,25 deg C):";
            idx = summary.IndexOf(VPSearchText);
            if (idx > 0)
            {
                ChemicalProperty chemProp = GetPropertyFromSummaryString(summary, VPSearchText, Globals.VAPOR_PRESSURE, ":");
                chemProp.chemical = smiles;
                chemProp.prop = Globals.VAPOR_PRESSURE;
                chemProp.units = "mm Hg";
                chemProps.data.Add(chemProp);
            }


            ////Code for boiling point
            //Parsing line like this:  Boiling Pt (deg C):  297.60
            string BPSearchText = "Boiling Pt (deg C):";
            idx = summary.IndexOf(BPSearchText);
            if (idx > 0)
            {
                ChemicalProperty chemProp = GetPropertyFromSummaryString(summary, BPSearchText, Globals.BOILING_POINT, ":");
                chemProp.chemical = smiles;
                chemProp.prop = Globals.BOILING_POINT;
                chemProp.units = "C";
                chemProps.data.Add(chemProp);
            }


            //Code for water solubility - WSKOW
            //Parsing line like this:     Water Solubility at 25 deg C (mg/L):  6.122
            string WSSearchText = "Water Solubility at 25 deg C (mg/L):";
            idx = summary.IndexOf(WSSearchText);
            if (idx > 0)
            {
                ChemicalProperty chemProp = GetPropertyFromSummaryString(summary, WSSearchText, Globals.WATER_SOLUBILITY, ":");
                chemProp.chemical = smiles;
                chemProp.prop = Globals.WATER_SOLUBILITY;
                chemProp.method = "WSKOW";
                chemProp.units = "mg/L";
                chemProps.data.Add(chemProp);
            }

            //Code for water solubility - WATERNT
            //Parsing line like this:     Wat Sol (v1.01 est) =  2.48 mg/L
            string WS2SearchText = "Wat Sol (v1.01 est) =";
            idx = summary.IndexOf(WS2SearchText);
            if (idx > 0)
            {
                ChemicalProperty chemProp = GetPropertyFromSummaryString(summary, WS2SearchText, Globals.WATER_SOLUBILITY, "=");
                chemProp.chemical = smiles;
                chemProp.prop = Globals.WATER_SOLUBILITY;
                chemProp.method = "WATERNT";
                chemProp.units = "mg/L";
                chemProps.data.Add(chemProp);
            }

            //Code for henrys law constant
            //Parsing line like this:     Bond Method :   3.07E-004  atm-m3/mole
            string henrysLawSearchText = "Bond Method :";
            idx = summary.IndexOf(henrysLawSearchText);
            if (idx > 0)
            {
                ChemicalProperty chemProp = GetPropertyFromSummaryString(summary, henrysLawSearchText, Globals.HENRY_LAW, ":");
                chemProp.chemical = smiles;
                chemProp.prop = Globals.HENRY_LAW;
                chemProps.data.Add(chemProp);
            }


            //We are getting two values for log kow MCI method, and Kow method
            //Code for experimental log koc
            //Parsing line like this:     Log Koc:  3.695       (MCI method)
            string logKocSearchText = "Log Koc: ";
            idx = summary.IndexOf(logKocSearchText);
            if (idx > 0)
            {
                ChemicalProperty chemProp = GetPropertyFromSummaryString(summary, logKocSearchText, Globals.LOG_KOC, ":");
                chemProp.chemical = smiles;
                chemProp.prop = Globals.LOG_KOC;
                chemProp.method = "MCI";
                chemProps.data.Add(chemProp);

                chemProp = GetPropertyFromSummaryString(summary, logKocSearchText, Globals.LOG_KOC, ":", idx + logKocSearchText.Length);
                chemProp.chemical = smiles;
                chemProp.prop = Globals.LOG_KOC;
                chemProp.method = "Kow";
                chemProps.data.Add(chemProp);
            }

            //Code for estimated Bio Concentration Factor regression based
            //Parsing line like this:     Log BCF from regression-based method = 0.500 (BCF = 3.162 L/kg wet-wt)
            string logBCFReg = "Log BCF from regression-based method";
            idx = summary.IndexOf(logBCFReg);
            if (idx > 0)
            {
                ChemicalProperty chemProp = GetPropertyFromSummaryString(summary, logBCFReg, Globals.LOG_BCF, "=");
                chemProp.chemical = smiles;
                chemProp.prop = Globals.LOG_BCF;
                chemProp.method = "regression";
                chemProps.data.Add(chemProp);
            }

            //Code for estimated Bio Concentration Factor Arnot-Gobas method
            //Parsing line like this:     Log BCF Arnot-Gobas method (upper trophic) = 0.243 (BCF = 1.749)
            string logBCFAG = "Log BCF Arnot-Gobas method (upper trophic)";
            idx = summary.IndexOf(logBCFAG);
            if (idx > 0)
            {
                ChemicalProperty chemProp = GetPropertyFromSummaryString(summary, logBCFAG, Globals.LOG_BCF, "=");
                chemProp.chemical = smiles;
                chemProp.prop = Globals.LOG_BCF;
                chemProp.method = "Arnot-Gobas";
                chemProps.data.Add(chemProp);
            }

            //Code for estimated Bio Accumulation Factor Arnot-Gobas method
            //Parsing line like this:     Log BAF Arnot-Gobas method (upper trophic) = 0.243 (BAF = 1.749)
            string logBAFAG = "Log BAF Arnot-Gobas method (upper trophic)";
            idx = summary.IndexOf(logBAFAG);
            if (idx > 0)
            {
                ChemicalProperty chemProp = GetPropertyFromSummaryString(summary, logBAFAG, Globals.LOG_BAF, "=");
                chemProp.chemical = smiles;
                chemProp.prop = Globals.LOG_BAF;
                chemProp.method = "Arnot-Gobas";
                chemProps.data.Add(chemProp);
            }

            return chemProps;
        }

        private ChemicalProperties ReadSummaryFileForMeasuredValues()
        {
            ChemicalProperties chemProps = new ChemicalProperties();

            //string summary = ReadFile(Path.Combine(Globals.EPI_SUITE_PATH, "summary"));
            string summary = ReadFile(Path.Combine(_tempFolder, "summary"));
            if (string.IsNullOrWhiteSpace(summary))
                return null;

            int idx = 0;
            //bool bLogKow = false;
            //bool bMP = false;


            //Code for log kow
            //Parsing line like this:     Log Kow (Exper. database match) =  4.96
            string logKowSearchText = "Log Kow (Exper. database match) =";
            idx = summary.IndexOf(logKowSearchText);
            //if (idx < 0)
            //    bLogKow = false;
            //else
            if (idx >= 0)
            {
                //bLogKow = true;
                ChemicalProperty chemProp = GetPropertyFromSummaryString(summary, logKowSearchText, Globals.LOG_KOW, "=");
                chemProp.prop = Globals.LOG_KOW;
                chemProp.method = "measured";
                chemProps.data.Add(chemProp);
            }
            //End code log kow


            //Code for melting point
            //Parsing line like this:     MP  (exp database):  42 deg C
            string MPSearchTest = "MP  (exp database):";
            idx = summary.IndexOf(MPSearchTest);
            //if (idx < 0)
            //    bMP = false;
            //else
            if (idx >= 0)
            {
                //bMP = true;
                ChemicalProperty chemProp = GetPropertyFromSummaryString(summary, MPSearchTest, Globals.MELTING_POINT, ":");
                chemProp.prop = Globals.MELTING_POINT;
                chemProp.method = "measured";
                chemProps.data.Add(chemProp);
            }
            //End code for melting point


            //Code for vapor pressure
            //Parsing line like this:     VP  (exp database):  2.03E-05 mm Hg (2.71E-003 Pa) at 25 deg C
            string VPSearchText = "VP  (exp database):";
            idx = summary.IndexOf(VPSearchText);
            if (idx > 0)
            {
                ChemicalProperty chemProp = GetPropertyFromSummaryString(summary, VPSearchText, Globals.VAPOR_PRESSURE, ":");
                chemProp.prop = Globals.VAPOR_PRESSURE;
                chemProp.method = "measured";
                chemProps.data.Add(chemProp);
            }


            ////Code for boiling point
            string BPSearchText = "BP  (exp database):";
            idx = summary.IndexOf(BPSearchText);
            if (idx > 0)
            {
                ChemicalProperty chemProp = GetPropertyFromSummaryString(summary, BPSearchText, Globals.BOILING_POINT, ":");
                chemProp.prop = Globals.BOILING_POINT;
                chemProp.method = "measured";
                chemProps.data.Add(chemProp);
            }


            //Code for water solubility
            //Parsing line like this:     Water Sol (Exper. database match) =  1.12 mg/L (24 deg C)
            string WSSearchText = "Water Sol (Exper. database match) =";
            idx = summary.IndexOf(WSSearchText);
            if (idx > 0)
            {
                ChemicalProperty chemProp = GetPropertyFromSummaryString(summary, WSSearchText, Globals.WATER_SOLUBILITY, "=");
                chemProp.prop = Globals.WATER_SOLUBILITY;
                chemProp.method = "measured";
                chemProps.data.Add(chemProp);
            }

            //Code for henrys law constant
            //Parsing line like this:     Exper Database: 2.93E-06  atm-m3/mole  (2.97E-001 Pa-m3/mole)
            string henrysLawSearchText = "Exper Database:";
            idx = summary.IndexOf(henrysLawSearchText);
            if (idx > 0)
            {
                ChemicalProperty chemProp = GetPropertyFromSummaryString(summary, henrysLawSearchText, Globals.HENRY_LAW, ":");
                chemProp.prop = Globals.HENRY_LAW;
                chemProp.method = "measured";
                chemProps.data.Add(chemProp);
            }


            //Code for experimental log koc
            //Parsing line like this:     Experimental Log Koc:  3.7  (database)
            string logKocSearchText = "Experimental Log Koc:";
            idx = summary.IndexOf(logKocSearchText);
            if (idx > 0)
            {
                ChemicalProperty chemProp = GetPropertyFromSummaryString(summary, logKocSearchText, Globals.LOG_KOC, ":");
                chemProp.prop = Globals.LOG_KOC;
                chemProp.method = "measured";
                chemProps.data.Add(chemProp);
            }

            return chemProps;
        }

        private ChemicalProperty GetPropertyFromSummaryString(string summary, string propText, string propName, string delimeter, int startPosition = 0)
        {
            int idx;
            int idxNewLine;
            idx = summary.IndexOf(propText, startPosition);
            ChemicalProperty chemProp = null;
            if (idx > 0)
            {
                idxNewLine = summary.IndexOf(Environment.NewLine, idx);
                //string line = summary.Substring(idx, (idxNewLine - idx) - 1);
                string line = summary.Substring(idx, (idxNewLine - idx));
                string[] tokens = line.Split(delimeter.ToCharArray());

                //Not sure why this changed - somehow the summary
                string[] tokens2 = tokens[1].Trim().Split(" ".ToCharArray());
                //string[] tokens2 = tokens[1].Trim().Split(":".ToCharArray());

                chemProp = new ChemicalProperty();
                chemProp.prop = propName;
                //chemProp.propertyvalue = Convert.ToDouble(tokens2[0].Trim());                
                chemProp.data = tokens2[0].Trim();

                if (tokens2.Length > 2)
                    chemProp.units = tokens2[2].Trim();
            }

            return chemProp;
        }

        private ChemicalProperties ReadSummaryFileForHydrolysisHalfLife(string propName, string smiles, string acid_base)
        {
            ChemicalProperties chemProps = null;

            string summary = ReadFile(Path.Combine(_tempFolder, "summary"));
            if (string.IsNullOrWhiteSpace(summary))
                return null;
            //throw new Exception("Unable to estimate rate constants for this structure: " + smiles);

            //HYDROWIN Program(v2.00) Results:
            //================================
            //SMILES : CNC(=O)Oc1c2ccccc2ccc1
            //CHEM   : 
            //MOL FOR: C12 H11 N1 O2 
            //MOL WT : 201.23
            //--------------------------- HYDROWIN v2.00 Results ---------------------------


            //            R1
            //CARBAMATE:     >N-C(=O)-O-R3            R1: -CH3                
            //            R2                         R2: -H                  
            //                            R3: -Naphthyl           
            //Kb hydrolysis at atom #  3:  4.680E+000  L/mol-sec

            //Total Kb for pH > 8 at 25 deg C :  4.680E+000  L/mol-sec
            //Kb Half-Life at pH 8:       1.714 days   
            //Kb Half-Life at pH 7:      17.143  days   


            //In the case above, 'CARBAMATE' would be the property name.
            //We are looking for the line:
            // Kb hydrolysis at atom #  3:  4.680E+000  L/mol-sec



            int idx = 0;
            idx = summary.IndexOf(propName, StringComparison.InvariantCultureIgnoreCase);
            //if (idx >= 0)
            while (idx > 0)
            {
                string searchStr = acid_base + " hydrolysis at ";
                int idx2 = summary.IndexOf(searchStr, idx, StringComparison.InvariantCultureIgnoreCase);
                if (idx2 < 0)
                    break;

                ChemicalProperty chemProp = new ChemicalProperty();
                chemProp.prop = acid_base;
                chemProp.units = "days";

                string summary2 = summary.Substring(idx2);
                string[] lines = summary2.Split(Environment.NewLine.ToCharArray());
                string s1 = lines[0];
                string[] tokens = s1.Split(':');
                string[] tokens2 = Regex.Split(tokens[1].Trim(), @"\s+");

                string data = tokens2[0].Trim();
                double dval = 0.0;
                if (double.TryParse(data, out dval))
                {
                    dval = 0.6931 / (dval * 1.0e-7);
                    //Convert from seconds to day 60*60*24=86400
                    dval = dval / 86400.0;
                }
                else
                    dval = double.NaN;


                //chemProp.prop = "Kb";
                chemProp.data = dval.ToString();
                //chemProp.units = "days";
                if (chemProps == null)
                    chemProps = new ChemicalProperties();

                chemProps.data.Add(chemProp);
                idx = idx2 + lines[0].Length;
            }
            //else
            //    return null;
            //throw new Exception("Unable to estimate rate constants for this structure: " + smiles);            
            return chemProps;

        }

        //Phosphate and Thiophosphate reactions produce a different summary file than other reactions
        //It will produce both Kn and Kb values
        private ChemicalProperties ReadSummaryFileForPhosphateThiophosphateHalfLife(string propName, string smiles)
        {
            ChemicalProperties chemProps = new ChemicalProperties();

            string summary = ReadFile(Path.Combine(_tempFolder, "summary"));
            if (string.IsNullOrWhiteSpace(summary))
                return null;
            //throw new Exception("Unable to estimate rate constants for this structure: " + smiles);

            //HYDROWIN Program(v2.00) Results:
            //================================
            //SMILES : COP(=S)(OC)Oc1cc(C)c(cc1)N(=O)(=O)
            //CHEM   : 
            //MOL FOR: C9 H12 N1 O5 P1 S1 
            //MOL WT : 277.23
            //--------------------------- HYDROWIN v2.00 Results ---------------------------
            //
            //Thiophosphate: S=P-({O,S}-R)3           
            //R1{-O-}: -CH3                
            //R1{-O-}: -Phenyl [2 frags]   
            //R1{-O-}: -CH3                

            //Log Kn (/sec) = -6.8193
            //Kn = 1.516e-007/sec = 9.097e-006/min
            //= 0.0005458/hr = 0.0131/day

            //Log Kb (/M-sec) = -2.0617
            //Kb = 0.008675/M-sec = 0.5205/M-min
            //= 31.23/M-hr = 749.5/M-day

            //Log Ka : ** Acid-catalyzed rates can NOT be estimated at this time.


            //In the case above, 'Thiophosphate' would be the property name.
            //We are looking for the lines:
            // Kn = 1.516e-007/sec = 9.097e-006/min
            // Kb = 0.008675/M-sec = 0.5205/M-min

            string[] lines = summary.Split(Environment.NewLine.ToCharArray());
            string[] propTypes = new string[] { "Kn", "Kb" };
            foreach (string line in lines)
            {
                string tmpLine = line.Trim();
                foreach (string propType in propTypes)
                {
                    Match match = Regex.Match(tmpLine, "^" + propType);
                    if (match.Success)
                    {
                        string[] tokens = tmpLine.Split(new char[] { '=', '/' });
                        string data = tokens[1].Trim();
                        double dval;
                        if (double.TryParse(data, out dval))
                        {
                            if (propType == "Kn")
                                dval = 0.6931 / (dval);
                            else
                                dval = 0.6931 / (dval * 1.0e-7);
                            //Convert from seconds to day 60*60*24=86400
                            dval = dval / 86400.0;
                        }
                        else
                            dval = double.NaN;

                        ChemicalProperty chemProp = new ChemicalProperty();
                        chemProp.prop = propType;
                        chemProp.data = dval.ToString();
                        chemProp.units = "days";
                        chemProp.chemical = smiles;
                        chemProps.data.Add(chemProp);
                    }
                }
            }

            return chemProps;

        }
        //Anhydride reactions produce a different summary file than other reactions
        //It will produce two Kb values
        private ChemicalProperties ReadSummaryFileForAnhydrideHalfLife(string propName, string smiles)
        {
            ChemicalProperties chemProps = new ChemicalProperties();

            string summary = ReadFile(Path.Combine(_tempFolder, "summary"));
            if (string.IsNullOrWhiteSpace(summary))
                return null;

            
            //HYDROWIN Program (v2.00) Results:
            //================================
            //SMILES : O=C(C)OC(=O)(C)
            //CHEM   : 
            //MOL FOR: C4 H6 O3 
            //MOL WT : 102.09
            //--------------------------- HYDROWIN v2.00 Results ---------------------------
            //Hydrolyzable Function detected:  Anhydrides        

            //-C-C(=O)-O-C(=O)-C-

            //Acid anhydrides react with water to form the corresponding acid.
            //Neutral hydrolysis half-lives of some Anhydrides (25 deg C,
            //Bunton et al, 1963; Bunton & Fenndler, 1965, Hawkins, 1975):
            //Acetic:   4.3 min          Phthalic:  1.5 minutes
            //Succinic: 4.3 min          Benzoic :  27.8 minutes
            //Glutaric: 4.4 minutes      Trimethylacetic anhydride: 24.2 hours

            //Branching may slow the hydrolysis rate.
            //Hydrolysis rate increases with pH.

            //ESTER:  R1-C(=O)-O-R2                   R1: -CH3                
            //                                        R2: -CO-CH3             
            //Kb hydrolysis at atom #  2:  1.285E+003  L/mol-sec

            //ESTER:  R1-C(=O)-O-R2                   R1: -CH3                
            //                                        R2: -CO-CH3             
            //Kb hydrolysis at atom #  5:  1.285E+003  L/mol-sec
            
            int count = 0;
            //For this calculator we are using the following:
            //  anhydride half-life = ((0.6931 / ((kb #2 + kb #5 )* 1.0E-7)))
            string[] lines = summary.Split(Environment.NewLine.ToCharArray());
            double dval = 0.0;
            foreach (string line in lines)
            {
                if (count >= 2)
                    break;

                string tmpLine = line.Trim();
                //Match match = Regex.Match(tmpLine, "^" + "Kb");
                Match match = Regex.Match(tmpLine, "^" + "Total Kb for pH");                
                if (match.Success)
                {
                    string[] tokens = tmpLine.Split(new char[] { ':' });
                    if (tokens == null || tokens.Length < 2)
                        return null;

                    string data = tokens[1].Trim();
                    tokens = data.Split();
                    data = tokens[0].Trim();
                    
                    if (double.TryParse(data, out dval))
                    {
  
                        dval = 0.6931 / (dval * 1.0e-7);
                        //Convert from seconds to day 60*60*24=86400
                        dval = dval / 86400.0;
                    }
                    else
                        dval = double.NaN;
                    
                    count++;
                    break;
                }
            }

            //double val  = (0.6931 / ((Kb1 + Kb2) * 1.0e-7));
            ChemicalProperty chemProp = new ChemicalProperty();
            chemProp.prop = "Kb";
            chemProp.data = dval.ToString();
            chemProp.units = "days";
            chemProp.chemical = smiles;
            chemProps.data.Add(chemProp);

            return chemProps;

        }



        private string WriteXsmile(string fileContents)
        {
            //Input file that Epi Suite models read
            string xsmiles = Path.Combine(_tempFolder, Globals.XSMILES);
            WriteFile(xsmiles, fileContents);

            return xsmiles;
        }

        /// <summary>
        /// THis is the text file that contains a melting point value
        /// First line is  -999.00
        /// Second line is melting point
        /// </summary>
        /// <returns></returns>
        private string WriteVPBPMPFile(string melting_point)
        {
            string vpbpmp = Path.Combine(_tempFolder, Globals.VPBPMP);
            string fileContents = "-999.00" + Environment.NewLine + melting_point;
            WriteFile(vpbpmp, fileContents);
            return vpbpmp;
        }

        /// <summary>
        /// Reads the epi_inp template and write out file with input data
        /// </summary>
        /// <param name="smiles">The structure to process</param>
        /// <param name="meltingPoint">Optional input melting point value</param>
        /// <param name="logKow">Optional input log kow value</param>
        /// <returns></returns>
        private string WriteEpiInput(string smiles, string meltingPoint = "(null)", string logKow = "(null)", string epiLink = "0")
        {
            //Read the input template file.
            //Replace tags with values
            //Write out modified epi suite input file
            string serverPath = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data");
            string epiInputTemplate = Path.Combine(serverPath, "epi_inp_template.txt");

            string fileContents = ReadFile(epiInputTemplate);

            fileContents = fileContents.Replace("(smiles)", smiles);
            fileContents = fileContents.Replace("(melting_point)", meltingPoint);
            fileContents = fileContents.Replace("(log_kow)", logKow);
            fileContents = fileContents.Replace("(epi_links)", epiLink);
            string epiInputFile = Path.Combine(_tempFolder, "epi_inp.txt");
            WriteFile(epiInputFile, fileContents);

            return epiInputFile;
        }

        /// <summary>
        /// Reads the xsmiles.txt template and write out file with input data
        /// </summary>
        /// <param name="smiles">The structure to process</param>
        /// <returns></returns>
        private string WriteHydroNTInput(string smiles)
        {
            //Read the xsmiles template file.
            //Replace tags with values
            //Write out modified xsmiles file
            string serverPath = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data");
            string xsmilesTemplate = Path.Combine(serverPath, "xsmiles_template.txt");

            string fileContents = ReadFile(xsmilesTemplate);

            fileContents = fileContents.Replace("(smiles)", smiles);
            //fileContents = fileContents.Replace("(melting_point)", meltingPoint);
            //fileContents = fileContents.Replace("(log_kow)", logKow);
            string xsmilesFile = Path.Combine(_tempFolder, "xsmiles");
            WriteFile(xsmilesFile, fileContents);

            return xsmilesFile;
        }

        private int RunProcess(ProcessStartInfo processStartInfo)
        {
            int exitCode;
            //ProcessStartInfo processInfo = CreateProcessStartInfo(command, args);
            Process process = Process.Start(processStartInfo);

            process.WaitForExit();
            exitCode = process.ExitCode;
            return exitCode;
        }


        public ChemicalProperty ReadHenryVal(string property, string smiles)
        {
            //string randomFolder = null;
            try
            {

                //Ouput file that Epi Suite produces
                string outFile = Path.Combine(_tempFolder, Globals.HENRYVAL);
                if (!File.Exists(outFile))
                    throw new Exception(string.Format("Error calculating {0} with argument {1} .", property, smiles));

                string sval = null;
                using (StreamReader sr = new StreamReader(outFile))
                {
                    string line = sr.ReadLine();
                    sval = line.Trim();
                }

                ChemicalProperty chemProp = null;
                //double val;
                //if (double.TryParse(sval, out val))
                //{
                chemProp = new ChemicalProperty();
                //chemProp.propertyvalue = val;
                chemProp.data = sval;
                chemProp.prop = property;
                chemProp.chemical = smiles;
                //}

                return chemProp;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                //Delete the temporary directory
                //DeleteFolder(tempFolder);
            }
        }

        public void RunExecutable(string modelExe, string smiles)
        {
            //string xsmilesFile = WriteXsmile(smiles);
            //string command = Path.Combine(Globals.EPI_SUITE_PATH, modelExe);
            string command = Path.Combine(_tempFolder, modelExe);
            string args = "episrc xsmiles";

            ProcessStartInfo psi = CreateProcessStartInfo(command, args);
            int exitCode = RunProcess(psi);
            if (exitCode != 0)
                throw new Exception(string.Format("Error executing {0} with argument {1} .", modelExe, smiles));

        }

        public void RunEPIWinExecutable(string modelExe, string smiles, string inputFile)
        {
            //string epiInpFile = WriteEpiInput(smiles, meltingPoint, logKow);
            //string command = Path.Combine(Globals.EPI_SUITE_PATH, modelExe);
            string command = Path.Combine(_tempFolder, modelExe);
            string args = inputFile;

            ProcessStartInfo psi = CreateProcessStartInfo(command, args);
            psi.WorkingDirectory = _tempFolder;
            //psi.WorkingDirectory = Globals.EPI_SUITE_PATH;
            int exitCode = RunProcess(psi);
            if (exitCode != 0)
                throw new Exception(string.Format("Error executing {0} with argument {1} .", modelExe, smiles));

        }


        //This works for all the properties except Henry's Law
        public ChemicalProperty ReadSumbrief(string property, string smiles)
        {
            try
            {
                //Create a temp folder for EPI Suite I/0
                //if (string.IsNullOrWhiteSpace(tempFolder))
                //    randomFolder = CreateTempFolder();

                //string xsmilesFile = WriteXsmile(randomFolder, smiles);

                //Path to the ES executable
                //string command = Path.Combine(Globals.EPI_SUITE_PATH, modelExe);
                //string args = "episrc " + xsmilesFile;

                //int exitCode = RunProcess(command, args, randomFolder);
                //if (exitCode != 0)
                //    throw new Exception(string.Format("Error calculating {0} with argument {1} .", property, smiles));

                ////Ouput file that ES produces
                string outFile = Path.Combine(_tempFolder, Globals.SUMBRIEF);
                if (!File.Exists(outFile))
                    throw new Exception(string.Format("Error calculating {0} with argument {1} .", property, smiles));

                //File contents should look something like this:
                //Log Kow (estimate):  1.99                        
                Dictionary<string, string> dctPropVals = new Dictionary<string, string>();
                using (StreamReader sr = new StreamReader(outFile))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        line = line.Trim();
                        string[] tokens = line.Split(":".ToCharArray());
                        dctPropVals.Add(tokens[0].Trim(), tokens[1].Trim());
                    }
                }

                //Make sure we got some data
                if (dctPropVals.Count < 1)
                    throw new Exception(string.Format("Error calculating {0} with argument {1} .", property, smiles));

                ChemicalProperty chemProp = null;
                if (dctPropVals.ContainsKey(property))
                {
                    //double val;
                    string sval = dctPropVals[property];
                    //if (double.TryParse(dctPropVals[property], out val))
                    //{
                    chemProp = new ChemicalProperty();
                    //chemProp.propertyvalue = val;
                    chemProp.data = sval;
                    chemProp.prop = property;
                    chemProp.chemical = smiles;
                    //}
                }

                return chemProp;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                //Delete the temporary directory                
                //DeleteFolder(tempFolder);
            }
        }


        private ProcessStartInfo CreateProcessStartInfo(string command, string args)
        {
            ProcessStartInfo processInfo;
            processInfo = new ProcessStartInfo(command);
            processInfo.Arguments = args;
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.WorkingDirectory = _tempFolder;

            return processInfo;
        }

        //Create a temp folder
        public string CreateTempFolder()
        {
            string serverPath = System.Web.HttpContext.Current.Server.MapPath("Temp");

            //Create a temp folder for EPI Suite I/0
            string randomFolder = Path.Combine(serverPath, Path.GetRandomFileName());
            if (!Directory.Exists(randomFolder))
                Directory.CreateDirectory(randomFolder);

            return randomFolder;
        }

        public void CopyEPISuiteFiles()
        {
            string epi_suite_files = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/episuitefiles");
            //string epi_suite_files = Path.Combine(serverPath, "epi_inp_template.txt");

            if (Directory.Exists(epi_suite_files))
            {
                string[] files = Directory.GetFiles(epi_suite_files);

                // Copy the files and overwrite destination files if they already exist.
                foreach (string file in files)
                {
                    // Use static Path methods to extract only the file name from the path.
                    string src = Path.Combine(epi_suite_files, file);
                    string dest = Path.Combine(_tempFolder, Path.GetFileName(file));
                    System.IO.File.Copy(src, dest, true);
                }
            }
        }


        public void DeleteFolder(string folder)
        {
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
        }

        /// <summary>
        /// Read a file and return contents
        /// </summary>
        /// <param name="file">File path</param>
        /// <returns></returns>
        private string ReadFile(string file)
        {
            string contents = null;
            using (StreamReader sr = new StreamReader(file))
            {
                contents = sr.ReadToEnd();
                contents = contents.Trim();
            }
            return contents;
        }


        private void WriteFile(string file, string contents)
        {
            using (StreamWriter sw = new StreamWriter(file))
            {
                sw.Write(contents);
            }
        }
    }
}