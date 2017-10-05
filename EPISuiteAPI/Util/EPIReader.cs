using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Diagnostics;
using EPISuiteAPI.Models;
using EPISuiteAPI.Util;


namespace EPISuiteAPI.Util
{
    public class EPIReader
    {
        string _epiInstallFolder = @"C:\EPISUITE41";
        string _epiWinExe = "epiwin1.exe";
        string _epiInput = "epi_inp.txt";

        string _tempFolder { get; set;}

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

        public ChemicalProperty GetEstimatedProperty(string modelExe, string property, string smiles)
        {
            //string tempFolder = CreateTempFolder();
            RunExecutable(modelExe, smiles);
            ChemicalProperty chemProp = null;
            if (string.Compare(modelExe, "henrynt.exe", true) != 0)
                chemProp = ReadSumbrief(property, smiles);
            else
                chemProp = ReadHenryVal(property, smiles);

            return chemProp;

        }


        public ChemicalProperties GetMeasuredProperties(string modelExe, string smiles, string meltingPoint = "(null)", string logKow = "(null)")
        {
            string epiInputFile = WriteEpiInput(smiles, meltingPoint, logKow);
            RunEPIWinExecutable(modelExe, smiles, epiInputFile);
            ChemicalProperties chemProps = null;
            chemProps = ReadSummaryFile();
            //if (chemProps == null)
            //{
            //double mp = 0;
            string mp = "0";
            string lKow = "0";
            if (chemProps.properties.Keys.Contains(Globals.MELTING_POINT))
            {
                mp = chemProps.properties[Globals.MELTING_POINT].propertyvalue;
            }
            if (chemProps.properties.Keys.Contains(Globals.LOG_KOW))
            {
                lKow = chemProps.properties[Globals.LOG_KOW].propertyvalue;
            }
            epiInputFile = WriteEpiInput(smiles, mp.ToString(), lKow.ToString());
            RunEPIWinExecutable(modelExe, smiles, epiInputFile);
            chemProps = ReadSummaryFile();
            //}

            return chemProps;

        }

        private ChemicalProperties ReadSummaryFile()
        {
            ChemicalProperties chemProps = new ChemicalProperties();

            //string summary = ReadFile(Path.Combine(Globals.EPI_SUITE_PATH, "summary"));
            string summary = ReadFile(Path.Combine(_tempFolder, "summary"));
            if (string.IsNullOrWhiteSpace(summary))
                return null;

            int idx = 0;
            bool bLogKow = false;
            bool bMP = false;


            //Code for log kow
            //Parsing line like this:     Log Kow (Exper. database match) =  4.96
            string logKowSearchText = "Log Kow (Exper. database match) =";
            idx = summary.IndexOf(logKowSearchText);
            if (idx < 0)
                bLogKow = false;
            else
            {
                bLogKow = true;
                ChemicalProperty chemProp = GetPropertyFromSummaryString(summary, logKowSearchText, Globals.LOG_KOW, "=");
                chemProps.properties.Add(Globals.LOG_KOW, chemProp);
            }
            //End code log kow


            //Code for melting point
            //Parsing line like this:     MP  (exp database):  42 deg C
            string MPSearchTest = "MP  (exp database):";
            idx = summary.IndexOf(MPSearchTest);
            if (idx < 0)
                bMP = false;
            else
            {
                bMP = true;
                ChemicalProperty chemProp = GetPropertyFromSummaryString(summary, MPSearchTest, Globals.MELTING_POINT, ":");
                chemProps.properties.Add(Globals.MELTING_POINT, chemProp);                
            }
            //End code for melting point


            //Code for vapor pressure
            //Parsing line like this:     VP  (exp database):  2.03E-05 mm Hg (2.71E-003 Pa) at 25 deg C
            string VPSearchText = "VP  (exp database):";
            idx = summary.IndexOf(VPSearchText);
            if (idx > 0)
            {
                ChemicalProperty chemProp = GetPropertyFromSummaryString(summary, VPSearchText, Globals.VAPOR_PRESSURE, ":");
                chemProps.properties.Add(Globals.VAPOR_PRESSURE, chemProp);
            }


            ////Code for boiling point
            string BPSearchText = "BP  (exp database):";
            idx = summary.IndexOf(BPSearchText);
            if (idx > 0)
            {
                ChemicalProperty chemProp = GetPropertyFromSummaryString(summary, BPSearchText, Globals.BOILING_POINT, ":");
                chemProps.properties.Add(Globals.BOILING_POINT, chemProp);
            }


            //Code for water solubility
            //Parsing line like this:     Water Sol (Exper. database match) =  1.12 mg/L (24 deg C)
            string WSSearchText = "Water Sol (Exper. database match) =";
            idx = summary.IndexOf(WSSearchText);
            if (idx > 0)
            {
                ChemicalProperty chemProp = GetPropertyFromSummaryString(summary, WSSearchText, Globals.WATER_SOLUBILITY, "=");
                chemProps.properties.Add(Globals.WATER_SOLUBILITY, chemProp);
            }

            //Code for henrys law constant
            //Parsing line like this:     Exper Database: 2.93E-06  atm-m3/mole  (2.97E-001 Pa-m3/mole)
            string henrysLawSearchText = "Exper Database:";
            idx = summary.IndexOf(henrysLawSearchText);
            if (idx > 0)
            {
                ChemicalProperty chemProp = GetPropertyFromSummaryString(summary, henrysLawSearchText, Globals.HENRY_LAW, ":");
                chemProps.properties.Add(Globals.HENRY_LAW, chemProp);
            }


            //Code for experimental log koc
            //Parsing line like this:     Experimental Log Koc:  3.7  (database)
            string logKocSearchText = "Experimental Log Koc:";
            idx = summary.IndexOf(logKocSearchText);
            if (idx > 0)
            {
                ChemicalProperty chemProp = GetPropertyFromSummaryString(summary, logKocSearchText, Globals.LOG_KOC, ":");
                chemProps.properties.Add(Globals.LOG_KOC,chemProp);
            }

            return chemProps;
        }

        private ChemicalProperty GetPropertyFromSummaryString(string summary, string propText, string propName, string delimeter)
        {
            int idx;
            int idxNewLine;
            idx = summary.IndexOf(propText);
            ChemicalProperty chemProp = null;
            if (idx > 0)
            {
                idxNewLine = summary.IndexOf(Environment.NewLine, idx);
                string line = summary.Substring(idx, (idxNewLine - idx) - 1);
                string[] tokens = line.Split(delimeter.ToCharArray());
                string[] tokens2 = tokens[1].Trim().Split(" ".ToCharArray());
                chemProp = new ChemicalProperty();
                chemProp.propertyname = propName;
                //chemProp.propertyvalue = Convert.ToDouble(tokens2[0].Trim());                
                chemProp.propertyvalue = tokens2[0].Trim();
            }

            return chemProp;
        }


        private string WriteXsmile(string fileContents)
        {
            //Input file that Epi Suite models read
            string xsmiles = Path.Combine(_tempFolder, Globals.XSMILES);
            WriteFile(xsmiles, fileContents);

            return xsmiles;
        }

        /// <summary>
        /// Reads the epi_inp template and write out file with input data
        /// </summary>
        /// <param name="smiles">The structure to process</param>
        /// <param name="meltingPoint">Optional input melting point value</param>
        /// <param name="logKow">Optional input log kow value</param>
        /// <returns></returns>
        private string WriteEpiInput(string smiles, string meltingPoint = "(null)", string logKow = "(null)")
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
            //string epiInputFile = Path.Combine(Globals.EPI_SUITE_PATH, "epi_inp.txt");
            string epiInputFile = Path.Combine(_tempFolder, "epi_inp.txt");
            WriteFile(epiInputFile, fileContents);

            return epiInputFile;
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
                double val;
                //if (double.TryParse(sval, out val))
                //{
                    chemProp = new ChemicalProperty();
                    //chemProp.propertyvalue = val;
                    chemProp.propertyvalue = sval;
                    chemProp.propertyname = property;
                    chemProp.structure = smiles;
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
            string xsmilesFile = WriteXsmile(smiles);
            //string command = Path.Combine(Globals.EPI_SUITE_PATH, modelExe);
            string command = Path.Combine(_tempFolder, modelExe);
            string args = "episrc " + xsmilesFile;

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
                    chemProp.propertyvalue = sval;
                    chemProp.propertyname = property;
                    chemProp.structure = smiles;
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