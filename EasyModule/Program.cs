using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Xml;
using FilesContents;

namespace EasyModule
{
    class MyModule
    {
        public const int MAX_MOD_NAME_LEN = 12;
        private string ThisDirectory;
        private string PackageDirectory;
        private string ProgramDirectory;
        private string ModuleName;

        private const string MOD_NAME_KEYWORD = "__MODNAME__";
        private const string MODCAPITAL_NAME_KEYWORD = "__MODNAMECAPITAL__";
    
        public MyModule(string thisDirectory, string moduleName)
        {
            ThisDirectory = thisDirectory;
            ModuleName = moduleName;
            PackageDirectory = Path.Combine(ThisDirectory,ModuleName);
            ProgramDirectory = Path.Combine(PackageDirectory,ModuleName + "Program");

            if (!Directory.Exists(ThisDirectory)) throw new Exception("Exception: This directory not found.");
        }
        public void Create()
        {
            string FileContent;
            string? FileName;

            // Package directory
            Directory.CreateDirectory(PackageDirectory);

            // Program directory
            Directory.CreateDirectory(ProgramDirectory);

            // Main file
            FileContent = Module.MAIN_FILE_CONTENT.Replace(MOD_NAME_KEYWORD,ModuleName).Replace(MODCAPITAL_NAME_KEYWORD,ModuleName.ToUpper()).TrimStart();
            CreateFile(ProgramDirectory,Module.MAIN_FILE_NAME,FileContent);

            // Actions file
            FileContent = Module.ACTIONS_FILE_CONTENT.Replace(MOD_NAME_KEYWORD,ModuleName).Replace(MODCAPITAL_NAME_KEYWORD,ModuleName.ToUpper()).TrimStart();
            CreateFile(ProgramDirectory,Module.ACTIONS_FILE_NAME,FileContent);
            
            // Local types file
            FileContent = Module.LOC_TYPES_FILE_CONTENT.Replace(MOD_NAME_KEYWORD,ModuleName).TrimStart();
            CreateFile(ProgramDirectory,Module.LOC_TYPES_FILE_NAME,FileContent);
            
            // Local variables file
            FileContent = Module.LOC_VARIABLES_FILE_CONTENT.Replace(MOD_NAME_KEYWORD,ModuleName).TrimStart();
            CreateFile(ProgramDirectory,Module.LOC_VARIABLES_FILE_NAME,FileContent);
            
            // IEC file
            FileContent = Module.IEC_FILE_CONTENT.Replace(MOD_NAME_KEYWORD,ModuleName).TrimStart();
            CreateFile(ProgramDirectory,Module.IEC_FILE_NAME,FileContent);
            
            // Alarms text file
            FileContent = Module.ALARMS_TXT_FILE_CONTENT.Replace(MOD_NAME_KEYWORD,ModuleName).TrimStart();
            FileName = Module.ALARMS_TXT_FILE_NAME.Replace(MOD_NAME_KEYWORD,ModuleName);
            CreateFile(PackageDirectory,FileName,FileContent);
            
            // Global types file
            FileContent = Module.GLB_TYPES_FILE_CONTENT.Replace(MOD_NAME_KEYWORD,ModuleName).Replace(MODCAPITAL_NAME_KEYWORD,ModuleName.ToUpper()).TrimStart();
            FileName = Module.GLB_TYPES_FILE_NAME.Replace(MOD_NAME_KEYWORD,ModuleName);
            CreateFile(PackageDirectory,FileName,FileContent);
            
            // Global variables file
            FileContent = Module.GLB_VARIABLES_FILE_CONTENT.Replace(MOD_NAME_KEYWORD,ModuleName).TrimStart();
            FileName = Module.GLB_VARIABLES_FILE_NAME.Replace(MOD_NAME_KEYWORD,ModuleName);
            CreateFile(PackageDirectory,FileName,FileContent);
            
            // This package file
            FileContent = Module.THIS_PKG_FILE_CONTENT.Replace(MOD_NAME_KEYWORD,ModuleName).TrimStart();
            FileName = Module.THIS_PKG_FILE_NAME.Replace(MOD_NAME_KEYWORD,ModuleName);
            CreateFile(PackageDirectory,FileName,FileContent);
            
            // Parent package file
            FileContent = Module.PARENT_PKG_FILE_CONTENT.Replace(MOD_NAME_KEYWORD,ModuleName).TrimStart();
            FileName = Directory.GetFiles(ThisDirectory,"Package.pkg").FirstOrDefault();
            if (FileName != null) MergePackageFiles(FileName,FileContent);
            else 
            {                
                FileName = Module.PARENT_PKG_FILE_NAME.Replace(MOD_NAME_KEYWORD,ModuleName);
                CreateFile(ThisDirectory,FileName,FileContent);
            }
        }
        private void CreateFile(string path, string name, string content)
        {
            string pathAndName = Path.Combine(path,name);

            var a = File.Create(pathAndName);
            a.Close();
            
            using (StreamWriter writer = new StreamWriter(pathAndName,true)) 
                writer.Write(content);
        }
        private void MergePackageFiles(string destPath, string content)
        {
            XmlDocument TemplateXmlFile = new XmlDocument();
            XmlDocument ThisXmlFile = new XmlDocument();
            XmlNode? ThisFilesNode;
            XmlNodeList? TemplateFileNodes;

            ThisXmlFile.Load(destPath);
            ThisFilesNode = ThisXmlFile.SelectSingleNode("//*[local-name()='Objects']");
            if (ThisFilesNode == null) throw new Exception("Cannot find Objects node in this package file.");
            
            TemplateXmlFile.LoadXml(content);
            TemplateFileNodes = TemplateXmlFile.SelectNodes("//*[local-name()='Object']");         
            if (TemplateFileNodes == null) throw new Exception("Cannot find Object nodes in template package file.");

            foreach(XmlNode node in TemplateFileNodes) ThisFilesNode.AppendChild(ThisXmlFile.ImportNode(node,true));                  

            // Check for duplicates
            foreach(XmlNode node1 in ThisFilesNode)
            {
                var cnt = 0;
                foreach(XmlNode node2 in ThisFilesNode)
                {
                    if (node1.InnerText == node2.InnerText) cnt = cnt + 1;
                    if (cnt>1) ThisFilesNode.RemoveChild(node2);
                }
            }

            ThisXmlFile.Save(destPath);
        }
    }

    class Program
    {
        static void Main(string[] argv)
        {
            //Console.Clear();
            
            // Get This folder
            string? ThisDirectory;
            if (argv.Count()>=1) 
            {  
                ThisDirectory = argv[1];
                if (ThisDirectory == null || !Directory.Exists(ThisDirectory)) throw new Exception("Exception: Cannot retrieve This directory.");
            }
            else
            {
                try
                {
                    ThisDirectory = Directory.GetCurrentDirectory();
                    if (ThisDirectory == null) throw new Exception("Exception: Cannot retrieve This directory.");         
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.ReadLine();
                    return;
                }
                Console.WriteLine("Working directory: " + ThisDirectory);
            }

            // Get module name
            string? ModuleName;
            try
            {
                if (argv.Count()>=2) 
                {
                    if (argv[2].Length < MyModule.MAX_MOD_NAME_LEN) ModuleName = argv[2];
                    else throw new Exception("Exception: Module name too long. Max " + MyModule.MAX_MOD_NAME_LEN.ToString() + " chars!");
                }
                else
                { 
                    var InputOk = false;
                    do
                    {
                        Console.WriteLine("Write the module name: ");
                        ModuleName = Console.ReadLine();
                        if (ModuleName == null) throw new Exception("Exception: Empty module name");
                        else if (ModuleName.Length > MyModule.MAX_MOD_NAME_LEN ) Console.WriteLine("Module name too long. Max " + MyModule.MAX_MOD_NAME_LEN.ToString() + " chars!");
                        else if (!Regex.IsMatch(ModuleName,"^[a-zA-Z][a-zA-Z0-9]*$")) Console.WriteLine("Module name invalid!");
                        else InputOk = true;
                    } while (!InputOk); 
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.ReadLine();
                return;
            }

            // Create MyFunctionBlock object
            MyModule myModule;            
            try
            {
                myModule = new MyModule(ThisDirectory,ModuleName);

                myModule.Create();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Done. Bye Bye");
        }

    }

}