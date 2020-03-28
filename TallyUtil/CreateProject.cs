using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace TallyUtil
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CreateProject
    {
        public string szCurrPath;
        public string szSharedItemName;
        public string szNewProjectName;
        public string szProjectType;

        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("a9bfeed5-4fa0-478e-984b-1f38f85f3aa3");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateProject"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private CreateProject(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static CreateProject Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        /*private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }*/

        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }
        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in CreateProject's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new CreateProject(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            CreateNewProject();          
        }
        private void CreateNewProject()
        {
            NewProject obj = new NewProject(GetCurrentProjectFileName());

            obj.ShowDialog();

            szCurrPath = obj.currPath;
            szSharedItemName = obj.sharedItemName;
            szNewProjectName = obj.newProjectname;
            szProjectType = obj.projectType;
            
            
            if (szProjectType == "Executable")
            {
                /* logically generate the project for all platforms */
                CreateProjectFileWinExe();
                CreateProjectFileLinExe();
                CreateProjectFileMacExe();
            }
            else if (szProjectType == "Static Library")
            {
                /* logically generate the project for all platforms */
                CreateProjectFileWinStaticLib();
                CreateProjectFileLinStaticLib();
                CreateProjectFileMacStaticLib();
            }
            else if (szProjectType == "Shared Items")
            {
                CreateProjectFileSharedItem();
            }
            else 
            {
                //CreateProjectFileWinSharedLibrary();
            }
            // we have the variables of the dialog

        }
        private string GetCurrentProjectFileName()
        {

            DTE dte = (DTE)ServiceProvider.GetService(typeof(DTE));
            string solutionDir = System.IO.Path.GetDirectoryName(dte.Solution.FullName);
            return solutionDir;

        }

        private void CreateProjectFileWinExe()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            settings.OmitXmlDeclaration = true;
            settings.NewLineOnAttributes = true;

            using (XmlWriter writer = XmlWriter.Create(szCurrPath + "\\" + szNewProjectName + "Win" + ".vcxproj", settings))
            {
                writer.WriteStartElement("Project", "http://schemas.microsoft.com/developer/msbuild/2003");
                writer.WriteAttributeString("DefaultTargets", "Build");

                // {
                writer.WriteStartElement("ItemGroup");
                writer.WriteAttributeString("Label", "ProjectConfigurations");

                WriteProjectConfiguration(writer, "Debug", "x64");
                WriteProjectConfiguration(writer, "Release", "x64");
                WriteProjectConfiguration(writer, "Diagnostics", "x64");
                WriteProjectConfiguration(writer, "Simulation", "x64");
                WriteProjectConfiguration(writer, "TraceOnly", "x64");
                WriteProjectConfiguration(writer, "MetricsOnly", "x64");

                WriteProjectConfiguration(writer, "Debug", "ARM64");
                WriteProjectConfiguration(writer, "Release", "ARM64");
                WriteProjectConfiguration(writer, "Diagnostics", "ARM64");
                WriteProjectConfiguration(writer, "Simulation", "ARM64");
                WriteProjectConfiguration(writer, "TraceOnly", "ARM64");
                WriteProjectConfiguration(writer, "MetricsOnly", "ARM64");

                writer.WriteEndElement();

                // }
                // {
                writer.WriteStartElement("PropertyGroup");
                writer.WriteAttributeString("Label", "Globals");
                writer.WriteElementString("VCProjectVersion", "16.0");
                writer.WriteElementString("ProjectGuid", "{" + Guid.NewGuid().ToString() + "}");
                writer.WriteElementString("Keyword", "Win32Proj");
                writer.WriteElementString("RootNamespace", szNewProjectName + "Win");
                writer.WriteElementString("WindowsTargetPlatformVersion", "10.0");
                writer.WriteEndElement();
                // }
                // {
                writer.WriteStartElement("Import");
                writer.WriteAttributeString("Project", "$(VCTargetsPath)\\Microsoft.Cpp.Default.props");
                writer.WriteEndElement();
                // }
                // {
                
                WritePropertyGroup(writer, "Debug", "x64", "Application", "v142","Unicode","true");
                WritePropertyGroup(writer, "Release", "x64", "Application", "v142", "Unicode", "false");
                WritePropertyGroup(writer, "Diagnostics", "x64", "Application", "v142", "Unicode", "false");
                WritePropertyGroup(writer, "Simulation", "x64", "Application", "v142", "Unicode", "false");
                WritePropertyGroup(writer, "TraceOnly", "x64", "Application", "v142", "Unicode", "false");
                WritePropertyGroup(writer, "MetricsOnly", "x64", "Application", "v142", "Unicode", "false");

                WritePropertyGroup(writer, "Debug", "ARM64", "Application", "v142", "Unicode", "true");
                WritePropertyGroup(writer, "Release", "ARM64", "Application", "v142", "Unicode", "false");
                WritePropertyGroup(writer, "Diagnostics", "ARM64", "Application", "v142", "Unicode", "false");
                WritePropertyGroup(writer, "Simulation", "ARM64", "Application", "v142", "Unicode", "false");
                WritePropertyGroup(writer, "TraceOnly", "ARM64", "Application", "v142", "Unicode", "false");
                WritePropertyGroup(writer, "MetricsOnly", "ARM64", "Application", "v142", "Unicode", "false");

                // }
                // {
                writer.WriteStartElement("Import");
                writer.WriteAttributeString("Project", "$(VCTargetsPath)\\Microsoft.Cpp.props");
                writer.WriteEndElement();
                // }
                // {
                writer.WriteStartElement("ImportGroup");
                writer.WriteAttributeString("Label", "ExtensionSettings");
                writer.WriteEndElement();
                // }
                // {
                writer.WriteStartElement("ImportGroup");
                writer.WriteAttributeString("Label", "Shared");
                writer.WriteStartElement("Import");
                if (szSharedItemName.Length != 0)
                    writer.WriteAttributeString("Project", szSharedItemName);
                writer.WriteAttributeString("Label", "Shared");
                writer.WriteEndElement();
                writer.WriteEndElement();
                // }

                // {
                String output_property = "";
                if (szProjectType == "Executable")
                {
                    output_property = "ExecutableOutput.props";
                }
                else if (szProjectType == "Static Library")
                {
                    output_property = "LibraryOutput.props";
                }
                else if (szProjectType == "Shared Library")
                {
                    output_property = "DllOutput.props";
                }

                WriteImportGroup(writer, "Debug", "x64", output_property, "AdditionalIncludes.props", "WinDebug.props");
                WriteImportGroup(writer, "Release", "x64", output_property, "AdditionalIncludes.props", "WinRelease.props");
                WriteImportGroup(writer, "Diagnostics", "x64", output_property, "AdditionalIncludes.props", "WinRelease.props", "Diagnostics.props");
                WriteImportGroup(writer, "Simulation", "x64", output_property, "AdditionalIncludes.props", "WinRelease.props", "Simulation.props");
                WriteImportGroup(writer, "TraceOnly", "x64", output_property, "AdditionalIncludes.props", "WinRelease.props", "TraceOnly.props");
                WriteImportGroup(writer, "MetricsOnly", "x64", output_property, "AdditionalIncludes.props", "WinRelease.props", "MetricsOnly.props");

                WriteImportGroup(writer, "Debug", "ARM64", output_property, "AdditionalIncludes.props", "WinDebug.props");
                WriteImportGroup(writer, "Release", "ARM64", output_property, "AdditionalIncludes.props", "WinRelease.props");
                WriteImportGroup(writer, "Diagnostics", "ARM64", output_property, "AdditionalIncludes.props", "WinRelease.props", "Diagnostics.props");
                WriteImportGroup(writer, "Simulation", "ARM64", output_property, "AdditionalIncludes.props", "WinRelease.props", "Simulation.props");
                WriteImportGroup(writer, "TraceOnly", "ARM64", output_property, "AdditionalIncludes.props", "WinRelease.props", "TraceOnly.props");
                WriteImportGroup(writer, "MetricsOnly", "ARM64", output_property, "AdditionalIncludes.props", "WinRelease.props", "MetricsOnly.props");

                // }
                // {
                writer.WriteStartElement("PropertyGroup");
                writer.WriteAttributeString("Label", "UserMacros");
                writer.WriteEndElement();
                // }
                // {
                WritePropertyGroupUserMacros(writer, "Debug", "x64","","");
                WritePropertyGroupUserMacros(writer, "Release", "x64","","");
                WritePropertyGroupUserMacros(writer, "Diagnostics", "x64","","");
                WritePropertyGroupUserMacros(writer, "Simulation", "x64","","");
                WritePropertyGroupUserMacros(writer, "TraceOnly", "x64","","");
                WritePropertyGroupUserMacros(writer, "MetricsOnly", "x64","","");

                WritePropertyGroupUserMacros(writer, "Debug", "ARM64","","");
                WritePropertyGroupUserMacros(writer, "Release", "ARM64","","");
                WritePropertyGroupUserMacros(writer, "Diagnostics", "ARM64","","");
                WritePropertyGroupUserMacros(writer, "Simulation", "ARM64","","");
                WritePropertyGroupUserMacros(writer, "TraceOnly", "ARM64","","");
                WritePropertyGroupUserMacros(writer, "MetricsOnly", "ARM64","","");

                // }
                // {
                
                WriteItemDefinitionGroup(writer, "Debug", "x64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log", "$(SolutionDir)lib\\$(Platform)\\$(Configuration)\\",true);
                WriteItemDefinitionGroup(writer, "Release", "x64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log", "$(SolutionDir)lib\\$(Platform)\\$(Configuration)\\", true);
                WriteItemDefinitionGroup(writer, "Diagnostics", "x64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log", "$(SolutionDir)lib\\$(Platform)\\$(Configuration)\\", true);
                WriteItemDefinitionGroup(writer, "Simulation", "x64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log", "$(SolutionDir)lib\\$(Platform)\\$(Configuration)\\", true);
                WriteItemDefinitionGroup(writer, "TraceOnly", "x64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log", "$(SolutionDir)lib\\$(Platform)\\$(Configuration)\\", true);
                WriteItemDefinitionGroup(writer, "MetricsOnly", "x64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log", "$(SolutionDir)lib\\$(Platform)\\$(Configuration)\\", true);

                WriteItemDefinitionGroup(writer, "Debug", "ARM64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log", "$(SolutionDir)lib\\$(Platform)\\$(Configuration)\\", true);
                WriteItemDefinitionGroup(writer, "Release", "ARM64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log", "$(SolutionDir)lib\\$(Platform)\\$(Configuration)\\", true);
                WriteItemDefinitionGroup(writer, "Diagnostics", "ARM64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log", "$(SolutionDir)lib\\$(Platform)\\$(Configuration)\\", true);
                WriteItemDefinitionGroup(writer, "Simulation", "ARM64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log", "$(SolutionDir)lib\\$(Platform)\\$(Configuration)\\", true);
                WriteItemDefinitionGroup(writer, "TraceOnly", "ARM64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log", "$(SolutionDir)lib\\$(Platform)\\$(Configuration)\\", true);
                WriteItemDefinitionGroup(writer, "MetricsOnly", "ARM64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log", "$(SolutionDir)lib\\$(Platform)\\$(Configuration)\\", true);

                // }
                // {
                writer.WriteStartElement("Import");
                writer.WriteAttributeString("Project", "$(VCTargetsPath)\\Microsoft.Cpp.targets");
                writer.WriteEndElement();
                // }
                // {
                writer.WriteStartElement("ImportGroup");
                writer.WriteAttributeString("Label", "ExtensionTargets");
                writer.WriteEndElement();
                // }

                writer.WriteEndElement(); // project
                writer.Flush();
            }
        }
        private void CreateProjectFileWinStaticLib()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            settings.OmitXmlDeclaration = true;
            settings.NewLineOnAttributes = true;

            using (XmlWriter writer = XmlWriter.Create(szCurrPath + "\\" + szNewProjectName + "Win" + ".vcxproj", settings))
            {
                writer.WriteStartElement("Project", "http://schemas.microsoft.com/developer/msbuild/2003");
                writer.WriteAttributeString("DefaultTargets", "Build");

                // {
                writer.WriteStartElement("ItemGroup");
                writer.WriteAttributeString("Label", "ProjectConfigurations");

                WriteProjectConfiguration(writer, "Debug", "x64");
                WriteProjectConfiguration(writer, "Release", "x64");
                WriteProjectConfiguration(writer, "Diagnostics", "x64");
                WriteProjectConfiguration(writer, "Simulation", "x64");
                WriteProjectConfiguration(writer, "TraceOnly", "x64");
                WriteProjectConfiguration(writer, "MetricsOnly", "x64");

                WriteProjectConfiguration(writer, "Debug", "ARM64");
                WriteProjectConfiguration(writer, "Release", "ARM64");
                WriteProjectConfiguration(writer, "Diagnostics", "ARM64");
                WriteProjectConfiguration(writer, "Simulation", "ARM64");
                WriteProjectConfiguration(writer, "TraceOnly", "ARM64");
                WriteProjectConfiguration(writer, "MetricsOnly", "ARM64");

                writer.WriteEndElement();

                // }
                // {
                writer.WriteStartElement("PropertyGroup");
                writer.WriteAttributeString("Label", "Globals");
                writer.WriteElementString("VCProjectVersion", "16.0");
                writer.WriteElementString("ProjectGuid", "{" + Guid.NewGuid().ToString() + "}");
                writer.WriteElementString("Keyword", "Win32Proj");
                writer.WriteElementString("RootNamespace", szNewProjectName);
                writer.WriteElementString("WindowsTargetPlatformVersion", "10.0");
                writer.WriteEndElement();
                // }
                // {
                writer.WriteStartElement("Import");
                writer.WriteAttributeString("Project", "$(VCTargetsPath)\\Microsoft.Cpp.Default.props");
                writer.WriteEndElement();
                // }
                // {
                WritePropertyGroup(writer, "Debug", "x64", "StaticLibrary", "v142", "Unicode", "true");
                WritePropertyGroup(writer, "Release", "x64", "StaticLibrary", "v142", "Unicode", "false");
                WritePropertyGroup(writer, "Diagnostics", "x64", "StaticLibrary", "v142", "Unicode", "false");
                WritePropertyGroup(writer, "Simulation", "x64", "StaticLibrary", "v142", "Unicode", "false");
                WritePropertyGroup(writer, "TraceOnly", "x64", "StaticLibrary", "v142", "Unicode", "false");
                WritePropertyGroup(writer, "MetricsOnly", "x64", "StaticLibrary", "v142", "Unicode", "false");

                WritePropertyGroup(writer, "Debug", "ARM64", "StaticLibrary", "v142", "Unicode", "true");
                WritePropertyGroup(writer, "Release", "ARM64", "StaticLibrary", "v142", "Unicode", "false");
                WritePropertyGroup(writer, "Diagnostics", "ARM64", "StaticLibrary", "v142", "Unicode", "false");
                WritePropertyGroup(writer, "Simulation", "ARM64", "StaticLibrary", "v142", "Unicode", "false");
                WritePropertyGroup(writer, "TraceOnly", "ARM64", "StaticLibrary", "v142", "Unicode", "false");
                WritePropertyGroup(writer, "MetricsOnly", "ARM64", "StaticLibrary", "v142", "Unicode", "false");

                // }
                // {
                writer.WriteStartElement("Import");
                writer.WriteAttributeString("Project", "$(VCTargetsPath)\\Microsoft.Cpp.props");
                writer.WriteEndElement();
                // }
                // {
                writer.WriteStartElement("ImportGroup");
                writer.WriteAttributeString("Label", "ExtensionSettings");
                writer.WriteEndElement();
                // }
                // {
                writer.WriteStartElement("ImportGroup");
                writer.WriteAttributeString("Label", "Shared");
                writer.WriteStartElement("Import");
                if (szSharedItemName.Length != 0)
                    writer.WriteAttributeString("Project", szSharedItemName);
                writer.WriteAttributeString("Label", "Shared");
                writer.WriteEndElement();
                writer.WriteEndElement();
                // }

                // {
                String output_property = "";
                if (szProjectType == "Executable")
                {
                    output_property = "ExecutableOutput.props";
                }
                else if (szProjectType == "Static Library")
                {
                    output_property = "LibraryOutput.props";
                }
                else if (szProjectType == "Shared Library")
                {
                    output_property = "DllOutput.props";
                }

                WriteImportGroup(writer, "Debug", "x64", output_property, "AdditionalIncludes.props", "WinDebug.props");
                WriteImportGroup(writer, "Release", "x64", output_property, "AdditionalIncludes.props", "WinRelease.props");
                WriteImportGroup(writer, "Diagnostics", "x64", output_property, "AdditionalIncludes.props", "WinRelease.props", "Diagnostics.props");
                WriteImportGroup(writer, "Simulation", "x64", output_property, "AdditionalIncludes.props", "WinRelease.props", "Simulation.props");
                WriteImportGroup(writer, "TraceOnly", "x64", output_property, "AdditionalIncludes.props", "WinRelease.props", "TraceOnly.props");
                WriteImportGroup(writer, "MetricsOnly", "x64", output_property, "AdditionalIncludes.props", "WinRelease.props", "MetricsOnly.props");

                WriteImportGroup(writer, "Debug", "ARM64", output_property, "AdditionalIncludes.props", "WinDebug.props");
                WriteImportGroup(writer, "Release", "ARM64", output_property, "AdditionalIncludes.props", "WinRelease.props");
                WriteImportGroup(writer, "Diagnostics", "ARM64", output_property, "AdditionalIncludes.props", "WinRelease.props", "Diagnostics.props");
                WriteImportGroup(writer, "Simulation", "ARM64", output_property, "AdditionalIncludes.props", "WinRelease.props", "Simulation.props");
                WriteImportGroup(writer, "TraceOnly", "ARM64", output_property, "AdditionalIncludes.props", "WinRelease.props", "TraceOnly.props");
                WriteImportGroup(writer, "MetricsOnly", "ARM64", output_property, "AdditionalIncludes.props", "WinRelease.props", "MetricsOnly.props");

                // }
                // {
                writer.WriteStartElement("PropertyGroup");
                writer.WriteAttributeString("Label", "UserMacros");
                writer.WriteEndElement();
                // }
                // {
                WritePropertyGroupUserMacros(writer, "Debug", "x64","","");
                WritePropertyGroupUserMacros(writer, "Release", "x64", "", "");
                WritePropertyGroupUserMacros(writer, "Diagnostics", "x64", "", "");
                WritePropertyGroupUserMacros(writer, "Simulation", "x64", "", "");
                WritePropertyGroupUserMacros(writer, "TraceOnly", "x64", "", "");
                WritePropertyGroupUserMacros(writer, "MetricsOnly", "x64", "", "");

                WritePropertyGroupUserMacros(writer, "Debug", "ARM64", "", "");
                WritePropertyGroupUserMacros(writer, "Release", "ARM64", "", "");
                WritePropertyGroupUserMacros(writer, "Diagnostics", "ARM64", "", "");
                WritePropertyGroupUserMacros(writer, "Simulation", "ARM64", "", "");
                WritePropertyGroupUserMacros(writer, "TraceOnly", "ARM64", "", "");
                WritePropertyGroupUserMacros(writer, "MetricsOnly", "ARM64", "", "");

                // }
                // {

                WriteItemDefinitionGroup(writer, "Debug", "x64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log", "$(SolutionDir)lib\\$(Platform)\\$(Configuration)\\", true);
                WriteItemDefinitionGroup(writer, "Release", "x64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log", "$(SolutionDir)lib\\$(Platform)\\$(Configuration)\\", true);
                WriteItemDefinitionGroup(writer, "Diagnostics", "x64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log", "$(SolutionDir)lib\\$(Platform)\\$(Configuration)\\", true);
                WriteItemDefinitionGroup(writer, "Simulation", "x64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log", "$(SolutionDir)lib\\$(Platform)\\$(Configuration)\\", true);
                WriteItemDefinitionGroup(writer, "TraceOnly", "x64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log", "$(SolutionDir)lib\\$(Platform)\\$(Configuration)\\", true);
                WriteItemDefinitionGroup(writer, "MetricsOnly", "x64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log", "$(SolutionDir)lib\\$(Platform)\\$(Configuration)\\", true);

                WriteItemDefinitionGroup(writer, "Debug", "ARM64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log", "$(SolutionDir)lib\\$(Platform)\\$(Configuration)\\", true);
                WriteItemDefinitionGroup(writer, "Release", "ARM64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log", "$(SolutionDir)lib\\$(Platform)\\$(Configuration)\\", true);
                WriteItemDefinitionGroup(writer, "Diagnostics", "ARM64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log", "$(SolutionDir)lib\\$(Platform)\\$(Configuration)\\", true);
                WriteItemDefinitionGroup(writer, "Simulation", "ARM64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log", "$(SolutionDir)lib\\$(Platform)\\$(Configuration)\\", true);
                WriteItemDefinitionGroup(writer, "TraceOnly", "ARM64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log", "$(SolutionDir)lib\\$(Platform)\\$(Configuration)\\", true);
                WriteItemDefinitionGroup(writer, "MetricsOnly", "ARM64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log", "$(SolutionDir)lib\\$(Platform)\\$(Configuration)\\", true);

                // }
                // {
                writer.WriteStartElement("Import");
                writer.WriteAttributeString("Project", "$(VCTargetsPath)\\Microsoft.Cpp.targets");
                writer.WriteEndElement();
                // }
                // {
                writer.WriteStartElement("ImportGroup");
                writer.WriteAttributeString("Label", "ExtensionTargets");
                writer.WriteEndElement();
                // }

                writer.WriteEndElement(); // project
                writer.Flush();
            }
        }
        private void CreateProjectFileSharedItem()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            settings.OmitXmlDeclaration = true;
            settings.NewLineOnAttributes = true;

            using (XmlWriter writer = XmlWriter.Create(szCurrPath + "\\" + szNewProjectName + ".vcxitems", settings))
            {
                writer.WriteStartElement("Project", "http://schemas.microsoft.com/developer/msbuild/2003");

                // {
                writer.WriteStartElement("PropertyGroup");
                writer.WriteAttributeString("Label", "Globals");
                writer.WriteElementString("MSBuildAllProjects", "$(MSBuildAllProjects);$(MSBuildThisFileFullPath)");
                writer.WriteElementString("HasSharedItems", "true");
                writer.WriteElementString("ItemsProjectGuid", "{" + Guid.NewGuid().ToString() + "}");
                writer.WriteEndElement(); //PropertyGroup
                // }
                // {
                writer.WriteStartElement("ItemDefinitionGroup");
                writer.WriteStartElement("ClCompile");
                writer.WriteElementString("AdditionalIncludeDirectories", "%(AdditionalIncludeDirectories);$(MSBuildThisFileDirectory)");                
                writer.WriteEndElement(); // ClCompile
                writer.WriteEndElement(); // ItemDefinitionGroup
                // }
                // {
                writer.WriteStartElement("ItemGroup");
                writer.WriteStartElement("ProjectCapability");
                writer.WriteAttributeString("Include", "SourceItemsFromImports");
                writer.WriteEndElement();//ProjectCapability
                writer.WriteEndElement();//ItemGroup
                // }


                writer.WriteEndElement(); // project
                writer.Flush();
            }
        }

        private void CreateProjectFileLinExe()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            settings.OmitXmlDeclaration = true;
            settings.NewLineOnAttributes = true;

            using (XmlWriter writer = XmlWriter.Create(szCurrPath + "\\" + szNewProjectName + "Lin" + ".vcxproj", settings))
            {
                writer.WriteStartElement("Project", "http://schemas.microsoft.com/developer/msbuild/2003");
                writer.WriteAttributeString("DefaultTargets", "Build");

                // {
                writer.WriteStartElement("ItemGroup");
                writer.WriteAttributeString("Label", "ProjectConfigurations");

                WriteProjectConfiguration(writer, "Debug", "x64");
                WriteProjectConfiguration(writer, "Release", "x64");
                WriteProjectConfiguration(writer, "Diagnostics", "x64");
                WriteProjectConfiguration(writer, "Simulation", "x64");
                WriteProjectConfiguration(writer, "TraceOnly", "x64");
                WriteProjectConfiguration(writer, "MetricsOnly", "x64");

                WriteProjectConfiguration(writer, "Debug", "ARM64");
                WriteProjectConfiguration(writer, "Release", "ARM64");
                WriteProjectConfiguration(writer, "Diagnostics", "ARM64");
                WriteProjectConfiguration(writer, "Simulation", "ARM64");
                WriteProjectConfiguration(writer, "TraceOnly", "ARM64");
                WriteProjectConfiguration(writer, "MetricsOnly", "ARM64");

                writer.WriteEndElement();

                // }
                // {
                writer.WriteStartElement("PropertyGroup");
                writer.WriteAttributeString("Label", "Globals");                
                writer.WriteElementString("ProjectGuid", "{" + Guid.NewGuid().ToString() + "}");
                writer.WriteElementString("Keyword", "Linux");
                writer.WriteElementString("RootNamespace", szNewProjectName + "Lin");
                writer.WriteElementString("MinimumVisualStudioVersion", "15.0");
                writer.WriteElementString("ApplicationType", "Linux");
                writer.WriteElementString("ApplicationTypeRevision", "1.0");
                writer.WriteElementString("TargetLinuxPlatform", "Generic");
                writer.WriteElementString("LinuxProjectType", "{" + Guid.NewGuid().ToString() + "}");
                writer.WriteEndElement();
                // }
                // {
                writer.WriteStartElement("Import");
                writer.WriteAttributeString("Project", "$(VCTargetsPath)\\Microsoft.Cpp.Default.props");
                writer.WriteEndElement();
                // }
                // {
                WritePropertyGroup(writer, "Debug", "x64", "Application", "Remote_GCC_1_0", "", "true");
                WritePropertyGroup(writer, "Release", "x64", "Application", "Remote_GCC_1_0", "", "false");
                WritePropertyGroup(writer, "Diagnostics", "x64", "Application", "Remote_GCC_1_0", "", "false");
                WritePropertyGroup(writer, "Simulation", "x64", "Application", "Remote_GCC_1_0", "", "false");
                WritePropertyGroup(writer, "TraceOnly", "x64", "Application", "Remote_GCC_1_0", "", "false");
                WritePropertyGroup(writer, "MetricsOnly", "x64", "Application", "Remote_GCC_1_0", "", "false");

                WritePropertyGroup(writer, "Debug", "ARM64", "Application", "Remote_GCC_1_0", "", "true");
                WritePropertyGroup(writer, "Release", "ARM64", "Application", "Remote_GCC_1_0", "", "false");
                WritePropertyGroup(writer, "Diagnostics", "ARM64", "Application", "Remote_GCC_1_0", "", "false");
                WritePropertyGroup(writer, "Simulation", "ARM64", "Application", "Remote_GCC_1_0", "", "false");
                WritePropertyGroup(writer, "TraceOnly", "ARM64", "Application", "Remote_GCC_1_0", "", "false");
                WritePropertyGroup(writer, "MetricsOnly", "ARM64", "Application", "Remote_GCC_1_0", "", "false");

                // }
                // {
                writer.WriteStartElement("Import");
                writer.WriteAttributeString("Project", "$(VCTargetsPath)\\Microsoft.Cpp.props");
                writer.WriteEndElement();
                // }
                // {
                writer.WriteStartElement("ImportGroup");
                writer.WriteAttributeString("Label", "ExtensionSettings");
                writer.WriteEndElement();
                // }
                // {
                writer.WriteStartElement("ImportGroup");
                writer.WriteAttributeString("Label", "Shared");
                writer.WriteStartElement("Import");
                if (szSharedItemName.Length != 0)
                    writer.WriteAttributeString("Project", szSharedItemName);
                writer.WriteAttributeString("Label", "Shared");
                writer.WriteEndElement();
                writer.WriteEndElement();
                // }

                // {
                String output_property = "";
                if (szProjectType == "Executable")
                {
                    output_property = "ExecutableOutput.props";
                }
                else if (szProjectType == "Static Library")
                {
                    output_property = "LibraryOutput.props";
                }
                else if (szProjectType == "Shared Library")
                {
                    output_property = "DllOutput.props";
                }

                WriteImportGroup(writer, "Debug", "x64", output_property, "AdditionalIncludes.props", "LinDebug.props");
                WriteImportGroup(writer, "Release", "x64", output_property, "AdditionalIncludes.props", "LinRelease.props");
                WriteImportGroup(writer, "Diagnostics", "x64", output_property, "AdditionalIncludes.props", "LinRelease.props", "Diagnostics.props");
                WriteImportGroup(writer, "Simulation", "x64", output_property, "AdditionalIncludes.props", "LinRelease.props", "Simulation.props");
                WriteImportGroup(writer, "TraceOnly", "x64", output_property, "AdditionalIncludes.props", "LinRelease.props", "TraceOnly.props");
                WriteImportGroup(writer, "MetricsOnly", "x64", output_property, "AdditionalIncludes.props", "LinRelease.props", "MetricsOnly.props");

                WriteImportGroup(writer, "Debug", "ARM64", output_property, "AdditionalIncludes.props", "LinDebug.props");
                WriteImportGroup(writer, "Release", "ARM64", output_property, "AdditionalIncludes.props", "LinRelease.props");
                WriteImportGroup(writer, "Diagnostics", "ARM64", output_property, "AdditionalIncludes.props", "LinRelease.props", "Diagnostics.props");
                WriteImportGroup(writer, "Simulation", "ARM64", output_property, "AdditionalIncludes.props", "LinRelease.props", "Simulation.props");
                WriteImportGroup(writer, "TraceOnly", "ARM64", output_property, "AdditionalIncludes.props", "LinRelease.props", "TraceOnly.props");
                WriteImportGroup(writer, "MetricsOnly", "ARM64", output_property, "AdditionalIncludes.props", "LinRelease.props", "MetricsOnly.props");

                // }
                // {
                writer.WriteStartElement("PropertyGroup");
                writer.WriteAttributeString("Label", "UserMacros");
                writer.WriteEndElement();
                // }
                // {
                WritePropertyGroupUserMacros(writer, "Debug", "x64","$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "Release", "x64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "Diagnostics", "x64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "Simulation", "x64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "TraceOnly", "x64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "MetricsOnly", "x64", "$(RemoteRootDir)", "$(IncludePath)");

                WritePropertyGroupUserMacros(writer, "Debug", "ARM64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "Release", "ARM64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "Diagnostics", "ARM64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "Simulation", "ARM64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "TraceOnly", "ARM64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "MetricsOnly", "ARM64", "$(RemoteRootDir)", "$(IncludePath)");

                // }
                // {

                WriteItemDefinitionGroup(writer, "Debug", "x64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibLin.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibLin.a;", "","",false);
                WriteItemDefinitionGroup(writer, "Release", "x64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibLin.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibLin.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "Diagnostics", "x64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibLin.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibLin.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "Simulation", "x64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibLin.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibLin.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "TraceOnly", "x64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibLin.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibLin.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "MetricsOnly", "x64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibLin.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibLin.a;", "", "", false);

                WriteItemDefinitionGroup(writer, "Debug", "ARM64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibLin.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibLin.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "Release", "ARM64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibLin.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibLin.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "Diagnostics", "ARM64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibLin.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibLin.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "Simulation", "ARM64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibLin.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibLin.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "TraceOnly", "ARM64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibLin.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibLin.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "MetricsOnly", "ARM64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibLin.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibLin.a;", "", "", false);

                // }
                // {
                writer.WriteStartElement("Import");
                writer.WriteAttributeString("Project", "$(VCTargetsPath)\\Microsoft.Cpp.targets");
                writer.WriteEndElement();
                // }
                // {
                writer.WriteStartElement("ImportGroup");
                writer.WriteAttributeString("Label", "ExtensionTargets");
                writer.WriteEndElement();
                // }

                writer.WriteEndElement(); // project
                writer.Flush();
            }
        }

        private void CreateProjectFileLinStaticLib()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            settings.OmitXmlDeclaration = true;
            settings.NewLineOnAttributes = true;

            using (XmlWriter writer = XmlWriter.Create(szCurrPath + "\\" + szNewProjectName + "Lin" + ".vcxproj", settings))
            {
                writer.WriteStartElement("Project", "http://schemas.microsoft.com/developer/msbuild/2003");
                writer.WriteAttributeString("DefaultTargets", "Build");

                // {
                writer.WriteStartElement("ItemGroup");
                writer.WriteAttributeString("Label", "ProjectConfigurations");

                WriteProjectConfiguration(writer, "Debug", "x64");
                WriteProjectConfiguration(writer, "Release", "x64");
                WriteProjectConfiguration(writer, "Diagnostics", "x64");
                WriteProjectConfiguration(writer, "Simulation", "x64");
                WriteProjectConfiguration(writer, "TraceOnly", "x64");
                WriteProjectConfiguration(writer, "MetricsOnly", "x64");

                WriteProjectConfiguration(writer, "Debug", "ARM64");
                WriteProjectConfiguration(writer, "Release", "ARM64");
                WriteProjectConfiguration(writer, "Diagnostics", "ARM64");
                WriteProjectConfiguration(writer, "Simulation", "ARM64");
                WriteProjectConfiguration(writer, "TraceOnly", "ARM64");
                WriteProjectConfiguration(writer, "MetricsOnly", "ARM64");

                writer.WriteEndElement();

                // }
                // {
                writer.WriteStartElement("PropertyGroup");
                writer.WriteAttributeString("Label", "Globals");
                writer.WriteElementString("ProjectGuid", "{" + Guid.NewGuid().ToString() + "}");
                writer.WriteElementString("Keyword", "Linux");
                writer.WriteElementString("RootNamespace", szNewProjectName + "Lin");
                writer.WriteElementString("MinimumVisualStudioVersion", "15.0");
                writer.WriteElementString("ApplicationType", "Linux");
                writer.WriteElementString("ApplicationTypeRevision", "1.0");
                writer.WriteElementString("TargetLinuxPlatform", "Generic");
                writer.WriteElementString("LinuxProjectType", "{" + Guid.NewGuid().ToString() + "}");
                writer.WriteEndElement();
                // }
                // {
                writer.WriteStartElement("Import");
                writer.WriteAttributeString("Project", "$(VCTargetsPath)\\Microsoft.Cpp.Default.props");
                writer.WriteEndElement();
                // }
                // {
                WritePropertyGroup(writer, "Debug", "x64", "Application", "Remote_GCC_1_0", "", "true");
                WritePropertyGroup(writer, "Release", "x64", "Application", "Remote_GCC_1_0", "", "false");
                WritePropertyGroup(writer, "Diagnostics", "x64", "Application", "Remote_GCC_1_0", "", "false");
                WritePropertyGroup(writer, "Simulation", "x64", "Application", "Remote_GCC_1_0", "", "false");
                WritePropertyGroup(writer, "TraceOnly", "x64", "Application", "Remote_GCC_1_0", "", "false");
                WritePropertyGroup(writer, "MetricsOnly", "x64", "Application", "Remote_GCC_1_0", "", "false");

                WritePropertyGroup(writer, "Debug", "ARM64", "Application", "Remote_GCC_1_0", "", "true");
                WritePropertyGroup(writer, "Release", "ARM64", "Application", "Remote_GCC_1_0", "", "false");
                WritePropertyGroup(writer, "Diagnostics", "ARM64", "Application", "Remote_GCC_1_0", "", "false");
                WritePropertyGroup(writer, "Simulation", "ARM64", "Application", "Remote_GCC_1_0", "", "false");
                WritePropertyGroup(writer, "TraceOnly", "ARM64", "Application", "Remote_GCC_1_0", "", "false");
                WritePropertyGroup(writer, "MetricsOnly", "ARM64", "Application", "Remote_GCC_1_0", "", "false");

                // }
                // {
                writer.WriteStartElement("Import");
                writer.WriteAttributeString("Project", "$(VCTargetsPath)\\Microsoft.Cpp.props");
                writer.WriteEndElement();
                // }
                // {
                writer.WriteStartElement("ImportGroup");
                writer.WriteAttributeString("Label", "ExtensionSettings");
                writer.WriteEndElement();
                // }
                // {
                writer.WriteStartElement("ImportGroup");
                writer.WriteAttributeString("Label", "Shared");
                writer.WriteStartElement("Import");
                if (szSharedItemName.Length != 0)
                    writer.WriteAttributeString("Project", szSharedItemName);
                writer.WriteAttributeString("Label", "Shared");
                writer.WriteEndElement();
                writer.WriteEndElement();
                // }

                // {
                String output_property = "";
                if (szProjectType == "Executable")
                {
                    output_property = "ExecutableOutput.props";
                }
                else if (szProjectType == "Static Library")
                {
                    output_property = "LibraryOutput.props";
                }
                else if (szProjectType == "Shared Library")
                {
                    output_property = "DllOutput.props";
                }

                WriteImportGroup(writer, "Debug", "x64", output_property, "AdditionalIncludes.props", "LinDebug.props");
                WriteImportGroup(writer, "Release", "x64", output_property, "AdditionalIncludes.props", "LinRelease.props");
                WriteImportGroup(writer, "Diagnostics", "x64", output_property, "AdditionalIncludes.props", "LinRelease.props", "Diagnostics.props");
                WriteImportGroup(writer, "Simulation", "x64", output_property, "AdditionalIncludes.props", "LinRelease.props", "Simulation.props");
                WriteImportGroup(writer, "TraceOnly", "x64", output_property, "AdditionalIncludes.props", "LinRelease.props", "TraceOnly.props");
                WriteImportGroup(writer, "MetricsOnly", "x64", output_property, "AdditionalIncludes.props", "LinRelease.props", "MetricsOnly.props");

                WriteImportGroup(writer, "Debug", "ARM64", output_property, "AdditionalIncludes.props", "LinDebug.props");
                WriteImportGroup(writer, "Release", "ARM64", output_property, "AdditionalIncludes.props", "LinRelease.props");
                WriteImportGroup(writer, "Diagnostics", "ARM64", output_property, "AdditionalIncludes.props", "LinRelease.props", "Diagnostics.props");
                WriteImportGroup(writer, "Simulation", "ARM64", output_property, "AdditionalIncludes.props", "LinRelease.props", "Simulation.props");
                WriteImportGroup(writer, "TraceOnly", "ARM64", output_property, "AdditionalIncludes.props", "LinRelease.props", "TraceOnly.props");
                WriteImportGroup(writer, "MetricsOnly", "ARM64", output_property, "AdditionalIncludes.props", "LinRelease.props", "MetricsOnly.props");

                // }
                // {
                writer.WriteStartElement("PropertyGroup");
                writer.WriteAttributeString("Label", "UserMacros");
                writer.WriteEndElement();
                // }
                // {
                WritePropertyGroupUserMacros(writer, "Debug", "x64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "Release", "x64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "Diagnostics", "x64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "Simulation", "x64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "TraceOnly", "x64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "MetricsOnly", "x64", "$(RemoteRootDir)", "$(IncludePath)");

                WritePropertyGroupUserMacros(writer, "Debug", "ARM64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "Release", "ARM64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "Diagnostics", "ARM64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "Simulation", "ARM64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "TraceOnly", "ARM64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "MetricsOnly", "ARM64", "$(RemoteRootDir)", "$(IncludePath)");

                // }
                // {

                WriteItemDefinitionGroup(writer, "Debug", "x64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibLin.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibLin.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "Release", "x64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibLin.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibLin.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "Diagnostics", "x64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibLin.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibLin.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "Simulation", "x64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibLin.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibLin.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "TraceOnly", "x64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibLin.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibLin.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "MetricsOnly", "x64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibLin.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibLin.a;", "", "", false);

                WriteItemDefinitionGroup(writer, "Debug", "ARM64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibLin.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibLin.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "Release", "ARM64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibLin.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibLin.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "Diagnostics", "ARM64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibLin.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibLin.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "Simulation", "ARM64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibLin.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibLin.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "TraceOnly", "ARM64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibLin.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibLin.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "MetricsOnly", "ARM64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibLin.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibLin.a;", "", "", false);

                // }
                // {
                writer.WriteStartElement("Import");
                writer.WriteAttributeString("Project", "$(VCTargetsPath)\\Microsoft.Cpp.targets");
                writer.WriteEndElement();
                // }
                // {
                writer.WriteStartElement("ImportGroup");
                writer.WriteAttributeString("Label", "ExtensionTargets");
                writer.WriteEndElement();
                // }

                writer.WriteEndElement(); // project
                writer.Flush();
            }
        }

        private void CreateProjectFileMacExe()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            settings.OmitXmlDeclaration = true;
            settings.NewLineOnAttributes = true;

            using (XmlWriter writer = XmlWriter.Create(szCurrPath + "\\" + szNewProjectName + "Mac" + ".vcxproj", settings))
            {
                writer.WriteStartElement("Project", "http://schemas.microsoft.com/developer/msbuild/2003");
                writer.WriteAttributeString("DefaultTargets", "Build");

                // {
                writer.WriteStartElement("ItemGroup");
                writer.WriteAttributeString("Label", "ProjectConfigurations");

                WriteProjectConfiguration(writer, "Debug", "x64");
                WriteProjectConfiguration(writer, "Release", "x64");
                WriteProjectConfiguration(writer, "Diagnostics", "x64");
                WriteProjectConfiguration(writer, "Simulation", "x64");
                WriteProjectConfiguration(writer, "TraceOnly", "x64");
                WriteProjectConfiguration(writer, "MetricsOnly", "x64");

                WriteProjectConfiguration(writer, "Debug", "ARM64");
                WriteProjectConfiguration(writer, "Release", "ARM64");
                WriteProjectConfiguration(writer, "Diagnostics", "ARM64");
                WriteProjectConfiguration(writer, "Simulation", "ARM64");
                WriteProjectConfiguration(writer, "TraceOnly", "ARM64");
                WriteProjectConfiguration(writer, "MetricsOnly", "ARM64");

                writer.WriteEndElement();

                // }
                // {
                writer.WriteStartElement("PropertyGroup");
                writer.WriteAttributeString("Label", "Globals");
                writer.WriteElementString("ProjectGuid", "{" + Guid.NewGuid().ToString() + "}");
                writer.WriteElementString("Keyword", "Linux");
                writer.WriteElementString("RootNamespace", szNewProjectName + "Lin");
                writer.WriteElementString("MinimumVisualStudioVersion", "15.0");
                writer.WriteElementString("ApplicationType", "Linux");
                writer.WriteElementString("ApplicationTypeRevision", "1.0");
                writer.WriteElementString("TargetLinuxPlatform", "Generic");
                writer.WriteElementString("LinuxProjectType", "{" + Guid.NewGuid().ToString() + "}");
                writer.WriteEndElement();
                // }
                // {
                writer.WriteStartElement("Import");
                writer.WriteAttributeString("Project", "$(VCTargetsPath)\\Microsoft.Cpp.Default.props");
                writer.WriteEndElement();
                // }
                // {
                WritePropertyGroup(writer, "Debug", "x64", "Application", "Remote_Clang_1_0", "", "true");
                WritePropertyGroup(writer, "Release", "x64", "Application", "Remote_Clang_1_0", "", "false");
                WritePropertyGroup(writer, "Diagnostics", "x64", "Application", "Remote_Clang_1_0", "", "false");
                WritePropertyGroup(writer, "Simulation", "x64", "Application", "Remote_Clang_1_0", "", "false");
                WritePropertyGroup(writer, "TraceOnly", "x64", "Application", "Remote_Clang_1_0", "", "false");
                WritePropertyGroup(writer, "MetricsOnly", "x64", "Application", "Remote_Clang_1_0", "", "false");

                WritePropertyGroup(writer, "Debug", "ARM64", "Application", "Remote_Clang_1_0", "", "true");
                WritePropertyGroup(writer, "Release", "ARM64", "Application", "Remote_Clang_1_0", "", "false");
                WritePropertyGroup(writer, "Diagnostics", "ARM64", "Application", "Remote_Clang_1_0", "", "false");
                WritePropertyGroup(writer, "Simulation", "ARM64", "Application", "Remote_Clang_1_0", "", "false");
                WritePropertyGroup(writer, "TraceOnly", "ARM64", "Application", "Remote_Clang_1_0", "", "false");
                WritePropertyGroup(writer, "MetricsOnly", "ARM64", "Application", "Remote_Clang_1_0", "", "false");

                // }
                // {
                writer.WriteStartElement("Import");
                writer.WriteAttributeString("Project", "$(VCTargetsPath)\\Microsoft.Cpp.props");
                writer.WriteEndElement();
                // }
                // {
                writer.WriteStartElement("ImportGroup");
                writer.WriteAttributeString("Label", "ExtensionSettings");
                writer.WriteEndElement();
                // }
                // {
                writer.WriteStartElement("ImportGroup");
                writer.WriteAttributeString("Label", "Shared");
                writer.WriteStartElement("Import");
                if (szSharedItemName.Length != 0)
                    writer.WriteAttributeString("Project", szSharedItemName);
                writer.WriteAttributeString("Label", "Shared");
                writer.WriteEndElement();
                writer.WriteEndElement();
                // }

                // {
                String output_property = "";
                if (szProjectType == "Executable")
                {
                    output_property = "ExecutableOutput.props";
                }
                else if (szProjectType == "Static Library")
                {
                    output_property = "LibraryOutput.props";
                }
                else if (szProjectType == "Shared Library")
                {
                    output_property = "DllOutput.props";
                }

                WriteImportGroup(writer, "Debug", "x64", output_property, "AdditionalIncludes.props", "MacDebug.props");
                WriteImportGroup(writer, "Release", "x64", output_property, "AdditionalIncludes.props", "MacRelease.props");
                WriteImportGroup(writer, "Diagnostics", "x64", output_property, "AdditionalIncludes.props", "MacRelease.props", "Diagnostics.props");
                WriteImportGroup(writer, "Simulation", "x64", output_property, "AdditionalIncludes.props", "MacRelease.props", "Simulation.props");
                WriteImportGroup(writer, "TraceOnly", "x64", output_property, "AdditionalIncludes.props", "MacRelease.props", "TraceOnly.props");
                WriteImportGroup(writer, "MetricsOnly", "x64", output_property, "AdditionalIncludes.props", "MacRelease.props", "MetricsOnly.props");

                WriteImportGroup(writer, "Debug", "ARM64", output_property, "AdditionalIncludes.props", "MacDebug.props");
                WriteImportGroup(writer, "Release", "ARM64", output_property, "AdditionalIncludes.props", "MacRelease.props");
                WriteImportGroup(writer, "Diagnostics", "ARM64", output_property, "AdditionalIncludes.props", "MacRelease.props", "Diagnostics.props");
                WriteImportGroup(writer, "Simulation", "ARM64", output_property, "AdditionalIncludes.props", "MacRelease.props", "Simulation.props");
                WriteImportGroup(writer, "TraceOnly", "ARM64", output_property, "AdditionalIncludes.props", "MacRelease.props", "TraceOnly.props");
                WriteImportGroup(writer, "MetricsOnly", "ARM64", output_property, "AdditionalIncludes.props", "MacRelease.props", "MetricsOnly.props");

                // }
                // {
                writer.WriteStartElement("PropertyGroup");
                writer.WriteAttributeString("Label", "UserMacros");
                writer.WriteEndElement();
                // }
                // {
                WritePropertyGroupUserMacros(writer, "Debug", "x64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "Release", "x64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "Diagnostics", "x64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "Simulation", "x64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "TraceOnly", "x64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "MetricsOnly", "x64", "$(RemoteRootDir)", "$(IncludePath)");

                WritePropertyGroupUserMacros(writer, "Debug", "ARM64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "Release", "ARM64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "Diagnostics", "ARM64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "Simulation", "ARM64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "TraceOnly", "ARM64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "MetricsOnly", "ARM64", "$(RemoteRootDir)", "$(IncludePath)");

                // }
                // {

                WriteItemDefinitionGroup(writer, "Debug", "x64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibMac.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibMac.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "Release", "x64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibMac.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibMac.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "Diagnostics", "x64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibMac.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibMac.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "Simulation", "x64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibMac.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibMac.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "TraceOnly", "x64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibMac.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibMac.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "MetricsOnly", "x64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibMac.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibMac.a;", "", "", false);

                WriteItemDefinitionGroup(writer, "Debug", "ARM64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibMac.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibMac.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "Release", "ARM64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibMac.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibMac.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "Diagnostics", "ARM64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibMac.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibMac.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "Simulation", "ARM64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibMac.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibMac.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "TraceOnly", "ARM64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibMac.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibMac.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "MetricsOnly", "ARM64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibMac.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibMac.a;", "", "", false);

                // }
                // {
                writer.WriteStartElement("Import");
                writer.WriteAttributeString("Project", "$(VCTargetsPath)\\Microsoft.Cpp.targets");
                writer.WriteEndElement();
                // }
                // {
                writer.WriteStartElement("ImportGroup");
                writer.WriteAttributeString("Label", "ExtensionTargets");
                writer.WriteEndElement();
                // }

                writer.WriteEndElement(); // project
                writer.Flush();
            }
        }

        private void CreateProjectFileMacStaticLib()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            settings.OmitXmlDeclaration = true;
            settings.NewLineOnAttributes = true;

            using (XmlWriter writer = XmlWriter.Create(szCurrPath + "\\" + szNewProjectName + "Mac" + ".vcxproj", settings))
            {
                writer.WriteStartElement("Project", "http://schemas.microsoft.com/developer/msbuild/2003");
                writer.WriteAttributeString("DefaultTargets", "Build");

                // {
                writer.WriteStartElement("ItemGroup");
                writer.WriteAttributeString("Label", "ProjectConfigurations");

                WriteProjectConfiguration(writer, "Debug", "x64");
                WriteProjectConfiguration(writer, "Release", "x64");
                WriteProjectConfiguration(writer, "Diagnostics", "x64");
                WriteProjectConfiguration(writer, "Simulation", "x64");
                WriteProjectConfiguration(writer, "TraceOnly", "x64");
                WriteProjectConfiguration(writer, "MetricsOnly", "x64");

                WriteProjectConfiguration(writer, "Debug", "ARM64");
                WriteProjectConfiguration(writer, "Release", "ARM64");
                WriteProjectConfiguration(writer, "Diagnostics", "ARM64");
                WriteProjectConfiguration(writer, "Simulation", "ARM64");
                WriteProjectConfiguration(writer, "TraceOnly", "ARM64");
                WriteProjectConfiguration(writer, "MetricsOnly", "ARM64");

                writer.WriteEndElement();

                // }
                // {
                writer.WriteStartElement("PropertyGroup");
                writer.WriteAttributeString("Label", "Globals");
                writer.WriteElementString("ProjectGuid", "{" + Guid.NewGuid().ToString() + "}");
                writer.WriteElementString("Keyword", "Linux");
                writer.WriteElementString("RootNamespace", szNewProjectName + "Mac");
                writer.WriteElementString("MinimumVisualStudioVersion", "15.0");
                writer.WriteElementString("ApplicationType", "Linux");
                writer.WriteElementString("ApplicationTypeRevision", "1.0");
                writer.WriteElementString("TargetLinuxPlatform", "Generic");
                writer.WriteElementString("LinuxProjectType", "{" + Guid.NewGuid().ToString() + "}");
                writer.WriteEndElement();
                // }
                // {
                writer.WriteStartElement("Import");
                writer.WriteAttributeString("Project", "$(VCTargetsPath)\\Microsoft.Cpp.Default.props");
                writer.WriteEndElement();
                // }
                // {
                WritePropertyGroup(writer, "Debug", "x64", "Application", "Remote_Clang_1_0", "", "true");
                WritePropertyGroup(writer, "Release", "x64", "Application", "Remote_Clang_1_0", "", "false");
                WritePropertyGroup(writer, "Diagnostics", "x64", "Application", "Remote_Clang_1_0", "", "false");
                WritePropertyGroup(writer, "Simulation", "x64", "Application", "Remote_Clang_1_0", "", "false");
                WritePropertyGroup(writer, "TraceOnly", "x64", "Application", "Remote_Clang_1_0", "", "false");
                WritePropertyGroup(writer, "MetricsOnly", "x64", "Application", "Remote_Clang_1_0", "", "false");

                WritePropertyGroup(writer, "Debug", "ARM64", "Application", "Remote_Clang_1_0", "", "true");
                WritePropertyGroup(writer, "Release", "ARM64", "Application", "Remote_Clang_1_0", "", "false");
                WritePropertyGroup(writer, "Diagnostics", "ARM64", "Application", "Remote_Clang_1_0", "", "false");
                WritePropertyGroup(writer, "Simulation", "ARM64", "Application", "Remote_Clang_1_0", "", "false");
                WritePropertyGroup(writer, "TraceOnly", "ARM64", "Application", "Remote_Clang_1_0", "", "false");
                WritePropertyGroup(writer, "MetricsOnly", "ARM64", "Application", "Remote_Clang_1_0", "", "false");

                // }
                // {
                writer.WriteStartElement("Import");
                writer.WriteAttributeString("Project", "$(VCTargetsPath)\\Microsoft.Cpp.props");
                writer.WriteEndElement();
                // }
                // {
                writer.WriteStartElement("ImportGroup");
                writer.WriteAttributeString("Label", "ExtensionSettings");
                writer.WriteEndElement();
                // }
                // {
                writer.WriteStartElement("ImportGroup");
                writer.WriteAttributeString("Label", "Shared");
                writer.WriteStartElement("Import");
                if (szSharedItemName.Length != 0)
                    writer.WriteAttributeString("Project", szSharedItemName);
                writer.WriteAttributeString("Label", "Shared");
                writer.WriteEndElement();
                writer.WriteEndElement();
                // }

                // {
                String output_property = "";
                if (szProjectType == "Executable")
                {
                    output_property = "ExecutableOutput.props";
                }
                else if (szProjectType == "Static Library")
                {
                    output_property = "LibraryOutput.props";
                }
                else if (szProjectType == "Shared Library")
                {
                    output_property = "DllOutput.props";
                }

                WriteImportGroup(writer, "Debug", "x64", output_property, "AdditionalIncludes.props", "MacDebug.props");
                WriteImportGroup(writer, "Release", "x64", output_property, "AdditionalIncludes.props", "MacRelease.props");
                WriteImportGroup(writer, "Diagnostics", "x64", output_property, "AdditionalIncludes.props", "MacRelease.props", "Diagnostics.props");
                WriteImportGroup(writer, "Simulation", "x64", output_property, "AdditionalIncludes.props", "MacRelease.props", "Simulation.props");
                WriteImportGroup(writer, "TraceOnly", "x64", output_property, "AdditionalIncludes.props", "MacRelease.props", "TraceOnly.props");
                WriteImportGroup(writer, "MetricsOnly", "x64", output_property, "AdditionalIncludes.props", "MacRelease.props", "MetricsOnly.props");

                WriteImportGroup(writer, "Debug", "ARM64", output_property, "AdditionalIncludes.props", "MacDebug.props");
                WriteImportGroup(writer, "Release", "ARM64", output_property, "AdditionalIncludes.props", "MacRelease.props");
                WriteImportGroup(writer, "Diagnostics", "ARM64", output_property, "AdditionalIncludes.props", "MacRelease.props", "Diagnostics.props");
                WriteImportGroup(writer, "Simulation", "ARM64", output_property, "AdditionalIncludes.props", "MacRelease.props", "Simulation.props");
                WriteImportGroup(writer, "TraceOnly", "ARM64", output_property, "AdditionalIncludes.props", "MacRelease.props", "TraceOnly.props");
                WriteImportGroup(writer, "MetricsOnly", "ARM64", output_property, "AdditionalIncludes.props", "MacRelease.props", "MetricsOnly.props");

                // }
                // {
                writer.WriteStartElement("PropertyGroup");
                writer.WriteAttributeString("Label", "UserMacros");
                writer.WriteEndElement();
                // }
                // {
                WritePropertyGroupUserMacros(writer, "Debug", "x64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "Release", "x64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "Diagnostics", "x64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "Simulation", "x64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "TraceOnly", "x64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "MetricsOnly", "x64", "$(RemoteRootDir)", "$(IncludePath)");

                WritePropertyGroupUserMacros(writer, "Debug", "ARM64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "Release", "ARM64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "Diagnostics", "ARM64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "Simulation", "ARM64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "TraceOnly", "ARM64", "$(RemoteRootDir)", "$(IncludePath)");
                WritePropertyGroupUserMacros(writer, "MetricsOnly", "ARM64", "$(RemoteRootDir)", "$(IncludePath)");

                // }
                // {

                WriteItemDefinitionGroup(writer, "Debug", "x64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibMac.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibMac.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "Release", "x64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibMac.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibMac.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "Diagnostics", "x64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibMac.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibMac.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "Simulation", "x64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibMac.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibMac.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "TraceOnly", "x64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibMac.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibMac.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "MetricsOnly", "x64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibMac.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibMac.a;", "", "", false);

                WriteItemDefinitionGroup(writer, "Debug", "ARM64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibMac.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibMac.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "Release", "ARM64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibMac.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibMac.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "Diagnostics", "ARM64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibMac.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibMac.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "Simulation", "ARM64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibMac.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibMac.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "TraceOnly", "ARM64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibMac.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibMac.a;", "", "", false);
                WriteItemDefinitionGroup(writer, "MetricsOnly", "ARM64", "/DTWMETRICS_ENABLE", "", "$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWServerLibMac.a;$(RemoteRootDir)/$(SolutionName)/lib/$(Platform)/$(Configuration)/libTWCoreLibMac.a;", "", "", false);

                // }
                // {
                writer.WriteStartElement("Import");
                writer.WriteAttributeString("Project", "$(VCTargetsPath)\\Microsoft.Cpp.targets");
                writer.WriteEndElement();
                // }
                // {
                writer.WriteStartElement("ImportGroup");
                writer.WriteAttributeString("Label", "ExtensionTargets");
                writer.WriteEndElement();
                // }

                writer.WriteEndElement(); // project
                writer.Flush();
            }
        }

        private void WriteProjectConfiguration(XmlWriter writer, String configuration, String platform)
        {
            writer.WriteStartElement("ProjectConfiguration");
            writer.WriteAttributeString("Include", configuration + "|" + platform);
            writer.WriteElementString("Configuration", configuration);
            writer.WriteElementString("Platform", platform);
            writer.WriteEndElement();
        }
        private void WritePropertyGroup(XmlWriter writer, String configuration, String platform, String configtype, String pltformtoolset, String charset, String use_debug_libraries)
        {
            writer.WriteStartElement("PropertyGroup");
            writer.WriteAttributeString("Condition", "'$(Configuration)|$(Platform)'=='" + configuration + "|" + platform + "'");
            writer.WriteAttributeString("Label", "Configuration");
            writer.WriteElementString("ConfigurationType", configtype);
            writer.WriteElementString("UseDebugLibraries", use_debug_libraries);
            writer.WriteElementString("PlatformToolset", pltformtoolset);
            if(charset.Length != 0)
                writer.WriteElementString("CharacterSet", charset);
            writer.WriteEndElement();
        }
        private void WriteImportGroup(XmlWriter writer, String configuration, String platform, String prop_1, String prop_2, String prop_3, String prop_4 = "")
        {
            writer.WriteStartElement("ImportGroup");
            writer.WriteAttributeString("Label", "PropertySheets");
            writer.WriteAttributeString("Condition", "'$(Configuration)|$(Platform)'=='" + configuration + "|" + platform + "'");

            writer.WriteStartElement("Import");
            writer.WriteAttributeString("Project", "$(UserRootDir)\\Microsoft.Cpp.$(Platform).user.props");
            writer.WriteAttributeString("Condition", "exists('$(UserRootDir)\\Microsoft.Cpp.$(Platform).user.props')");
            writer.WriteAttributeString("Label", "LocalAppDataPlatform");
            writer.WriteEndElement();

            writer.WriteStartElement("Import");
            writer.WriteAttributeString("Project", prop_1);
            writer.WriteEndElement();

            writer.WriteStartElement("Import");
            writer.WriteAttributeString("Project", prop_2);
            writer.WriteEndElement();

            writer.WriteStartElement("Import");
            writer.WriteAttributeString("Project", prop_3);
            writer.WriteEndElement();

            if (prop_4.Length != 0)
            {
                writer.WriteStartElement("Import");
                writer.WriteAttributeString("Project", prop_4);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }
        private void WritePropertyGroupUserMacros(XmlWriter writer, String configuration, String platform, String remoterootdir, String includepath)
        {
            writer.WriteStartElement("PropertyGroup");
            writer.WriteAttributeString("Condition", "'$(Configuration)|$(Platform)'=='" + configuration + "|" + platform + "'");
            if(remoterootdir.Length != 0)
                writer.WriteElementString("RemoteProjectDir", remoterootdir);//$(RemoteRootDir)
            if(includepath.Length != 0)
                writer.WriteElementString("IncludePath", includepath);//$(IncludePath)
            writer.WriteEndElement();
        }
        private void WriteItemDefinitionGroup(XmlWriter writer,String configuration, String platform , String preprocessor, String sub_system, String libs, String buildlogpath, String addlibdir, Boolean iscompile)
        { 
            writer.WriteStartElement("ItemDefinitionGroup");
            writer.WriteAttributeString("Condition", "'$(Configuration)|$(Platform)'=='" + configuration + "|" + platform + "'");

            
                writer.WriteStartElement("ClCompile");
                if (iscompile == true)
                {
                    writer.WriteElementString("AdditionalOptions", preprocessor + "%(AdditionalOptions)");
                    writer.WriteElementString("AssemblerListingLocation", "$(IntDir)");
                    writer.WriteElementString("ObjectFileName", "$(IntDir)");
                    writer.WriteElementString("ProgramDataBaseFileName", "$(IntDir)vc$(PlatformToolsetVersion).pdb");
                }
            writer.WriteEndElement();

            writer.WriteStartElement("Link");
            if(sub_system.Length != 0)
                writer.WriteElementString("SubSystem", sub_system);

            writer.WriteElementString("AdditionalDependencies", libs + "%(AdditionalDependencies)");
            
            writer.WriteElementString("AdditionalLibraryDirectories", addlibdir); // "$(SolutionDir)lib\\$(Platform)\\$(Configuration)\\"
            writer.WriteEndElement();

            if (buildlogpath.Length != 0)
            {
                writer.WriteStartElement("BuildLog");
                writer.WriteElementString("Path", buildlogpath);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            }
    }
}
