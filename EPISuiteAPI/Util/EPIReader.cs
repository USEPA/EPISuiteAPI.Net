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
        public EPIReader()
        {

        }

        //Create a temp folder
        private string CreateTempFolder()
        {
            string serverPath = System.Web.HttpContext.Current.Server.MapPath("Temp");

            //Create a temp folder for EPI Suite I/0
            string randomFolder = Path.Combine(serverPath, Path.GetRandomFileName());
            if (!Directory.Exists(randomFolder))
                Directory.CreateDirectory(randomFolder);

            return randomFolder;
        }

        private string WriteXsmile(string tempFolder, string fileContents)
        {
            //Input file that ES reads
            string xsmiles = Path.Combine(tempFolder, Globals.XSMILES);
            using (StreamWriter writer = new StreamWriter(xsmiles))
            {
                writer.Write(fileContents);
            }

            return xsmiles;
        }

        private int RunProcess(string command, string args, string workingFolder)
        {
            int exitCode;
            ProcessStartInfo processInfo = CreateProcessStartInfo(command, args, workingFolder);
            Process process = Process.Start(processInfo);
            process.WaitForExit();
            exitCode = process.ExitCode;
            return exitCode;
        }


        public ChemicalProperty ReadHenryVal(string modelExe, string property, string smiles, string tempFolder = null)
        {
            string randomFolder = null;
            try
            {
                //Create a temp folder for EPI Suite I/0
                if (string.IsNullOrWhiteSpace(tempFolder))
                    randomFolder = CreateTempFolder();

                string xsmilesFile = WriteXsmile(randomFolder, smiles);

                //Path to the ES executable
                string command = Path.Combine(Globals.EPI_SUITE_PATH, modelExe);
                string args = "episrc " + xsmilesFile;

                int exitCode = RunProcess(command, args, randomFolder);
                if (exitCode != 0)
                    throw new Exception(string.Format("Error calculating {0} with argument {1} .", property, smiles));

                //Ouput file that ES produces
                string outFile = Path.Combine(randomFolder, Globals.HENRYVAL);
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
                if (double.TryParse(sval, out val))
                {
                    chemProp = new ChemicalProperty();
                    chemProp.propertyvalue = val;
                    chemProp.propertyname = property;
                    chemProp.structure = smiles;
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
                if (Directory.Exists(randomFolder))
                    Directory.Delete(randomFolder, true);
            }
        }


        //This works for all the properties except Henry's Law
        public ChemicalProperty ReadSumbrief(string modelExe, string property, string smiles, string tempFolder = null)
        {
            string randomFolder = "";
            try
            {
                //Create a temp folder for EPI Suite I/0
                if (string.IsNullOrWhiteSpace(tempFolder))
                    randomFolder = CreateTempFolder();

                string xsmilesFile = WriteXsmile(randomFolder, smiles);

                //Path to the ES executable
                string command = Path.Combine(Globals.EPI_SUITE_PATH, modelExe);
                string args = "episrc " + xsmilesFile;

                int exitCode = RunProcess(command, args, randomFolder);
                if (exitCode != 0)
                    throw new Exception(string.Format("Error calculating {0} with argument {1} .", property, smiles));

                //Ouput file that ES produces
                string outFile = Path.Combine(randomFolder, Globals.SUMBRIEF);
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
                    double val;
                    if (double.TryParse(dctPropVals[property], out val))
                    {
                        chemProp = new ChemicalProperty();
                        chemProp.propertyvalue = val;
                        chemProp.propertyname = property;
                        chemProp.structure = smiles;
                    }
                }

                return chemProp;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                //Delete the temporary directory
                if (Directory.Exists(randomFolder))
                    Directory.Delete(randomFolder, true);
            }
        }

        private static ProcessStartInfo CreateProcessStartInfo(string command, string args, string workingFolder)
        {           
            ProcessStartInfo processInfo;            
            processInfo = new ProcessStartInfo(command);
            processInfo.Arguments = args;
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.WorkingDirectory = workingFolder;

            return processInfo;
        }
    }
}