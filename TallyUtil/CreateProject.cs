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
                CreateProjectFileWinExe();
            }
            else if (szProjectType == "Static Library")
            {
                CreateProjectFileWinStaticLib();
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

            using (XmlWriter writer = XmlWriter.Create(szCurrPath + "\\" + szNewProjectName + ".vcxproj", settings))
            {
                writer.WriteStartElement("Project", "http://schemas.microsoft.com/developer/msbuild/2003");
                writer.WriteAttributeString("DefaultTargets", "Build");

                // {
                writer.WriteStartElement("ItemGroup");
                writer.WriteAttributeString("Label", "ProjectConfigurations");

                WriteProjectConfiguration(writer, "Debug", "x64");
                WriteProjectConfiguration(writer, "Release", "x64");
                WriteProjectConfiguration(writer, "Diagnostics", "x64");
                WriteProjectConfiguration(writer, "SimulationOnly", "x64");
                WriteProjectConfiguration(writer, "TraceOnly", "x64");
                WriteProjectConfiguration(writer, "MetricsOnly", "x64");

                WriteProjectConfiguration(writer, "Debug", "ARM64");
                WriteProjectConfiguration(writer, "Release", "ARM64");
                WriteProjectConfiguration(writer, "Diagnostics", "ARM64");
                WriteProjectConfiguration(writer, "SimulationOnly", "ARM64");
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
                WritePropertyGroup(writer, "Debug", "x64", "Application", "true");
                WritePropertyGroup(writer, "Release", "x64", "Application","false");
                WritePropertyGroup(writer, "Diagnostics", "x64", "Application", "false");
                WritePropertyGroup(writer, "SimulationOnly", "x64", "Application", "false");
                WritePropertyGroup(writer, "TraceOnly", "x64", "Application", "false");
                WritePropertyGroup(writer, "MetricsOnly", "x64", "Application", "false");

                WritePropertyGroup(writer, "Debug", "ARM64", "Application", "true");
                WritePropertyGroup(writer, "Release", "ARM64", "Application", "false");
                WritePropertyGroup(writer, "Diagnostics", "ARM64", "Application", "false");
                WritePropertyGroup(writer, "SimulationOnly", "ARM64", "Application", "false");
                WritePropertyGroup(writer, "TraceOnly", "ARM64", "Application", "false");
                WritePropertyGroup(writer, "MetricsOnly", "ARM64", "Application", "false");

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
                WriteImportGroup(writer, "SimulationOnly", "x64", output_property, "AdditionalIncludes.props", "WinRelease.props", "Simulation.props");
                WriteImportGroup(writer, "TraceOnly", "x64", output_property, "AdditionalIncludes.props", "WinRelease.props", "TraceOnly.props");
                WriteImportGroup(writer, "MetricsOnly", "x64", output_property, "AdditionalIncludes.props", "WinRelease.props", "MetricsOnly.props");

                WriteImportGroup(writer, "Debug", "ARM64", output_property, "AdditionalIncludes.props", "WinDebug.props");
                WriteImportGroup(writer, "Release", "ARM64", output_property, "AdditionalIncludes.props", "WinRelease.props");
                WriteImportGroup(writer, "Diagnostics", "ARM64", output_property, "AdditionalIncludes.props", "WinRelease.props", "Diagnostics.props");
                WriteImportGroup(writer, "SimulationOnly", "ARM64", output_property, "AdditionalIncludes.props", "WinRelease.props", "Simulation.props");
                WriteImportGroup(writer, "TraceOnly", "ARM64", output_property, "AdditionalIncludes.props", "WinRelease.props", "TraceOnly.props");
                WriteImportGroup(writer, "MetricsOnly", "ARM64", output_property, "AdditionalIncludes.props", "WinRelease.props", "MetricsOnly.props");

                // }
                // {
                writer.WriteStartElement("PropertyGroup");
                writer.WriteAttributeString("Label", "UserMacros");
                writer.WriteEndElement();
                // }
                // {
                WritePropertyGroupUserMacros(writer, "Debug", "x64");
                WritePropertyGroupUserMacros(writer, "Release", "x64");
                WritePropertyGroupUserMacros(writer, "Diagnostics", "x64");
                WritePropertyGroupUserMacros(writer, "SimulationOnly", "x64");
                WritePropertyGroupUserMacros(writer, "TraceOnly", "x64");
                WritePropertyGroupUserMacros(writer, "MetricsOnly", "x64");

                WritePropertyGroupUserMacros(writer, "Debug", "ARM64");
                WritePropertyGroupUserMacros(writer, "Release", "ARM64");
                WritePropertyGroupUserMacros(writer, "Diagnostics", "ARM64");
                WritePropertyGroupUserMacros(writer, "SimulationOnly", "ARM64");
                WritePropertyGroupUserMacros(writer, "TraceOnly", "ARM64");
                WritePropertyGroupUserMacros(writer, "MetricsOnly", "ARM64");

                // }
                // {
                
                WriteItemDefinitionGroup(writer, "Debug", "x64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log");
                WriteItemDefinitionGroup(writer, "Release", "x64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log");
                WriteItemDefinitionGroup(writer, "Diagnostics", "x64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log");
                WriteItemDefinitionGroup(writer, "SimulationOnly", "x64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log");
                WriteItemDefinitionGroup(writer, "TraceOnly", "x64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log");
                WriteItemDefinitionGroup(writer, "MetricsOnly", "x64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log");

                WriteItemDefinitionGroup(writer, "Debug", "ARM64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log");
                WriteItemDefinitionGroup(writer, "Release", "ARM64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log");
                WriteItemDefinitionGroup(writer, "Diagnostics", "ARM64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log");
                WriteItemDefinitionGroup(writer, "SimulationOnly", "ARM64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log");
                WriteItemDefinitionGroup(writer, "TraceOnly", "ARM64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log");
                WriteItemDefinitionGroup(writer, "MetricsOnly", "ARM64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log");

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

            using (XmlWriter writer = XmlWriter.Create(szCurrPath + "\\" + szNewProjectName + ".vcxproj", settings))
            {
                writer.WriteStartElement("Project", "http://schemas.microsoft.com/developer/msbuild/2003");
                writer.WriteAttributeString("DefaultTargets", "Build");

                // {
                writer.WriteStartElement("ItemGroup");
                writer.WriteAttributeString("Label", "ProjectConfigurations");

                WriteProjectConfiguration(writer, "Debug", "x64");
                WriteProjectConfiguration(writer, "Release", "x64");
                WriteProjectConfiguration(writer, "Diagnostics", "x64");
                WriteProjectConfiguration(writer, "SimulationOnly", "x64");
                WriteProjectConfiguration(writer, "TraceOnly", "x64");
                WriteProjectConfiguration(writer, "MetricsOnly", "x64");

                WriteProjectConfiguration(writer, "Debug", "ARM64");
                WriteProjectConfiguration(writer, "Release", "ARM64");
                WriteProjectConfiguration(writer, "Diagnostics", "ARM64");
                WriteProjectConfiguration(writer, "SimulationOnly", "ARM64");
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
                WritePropertyGroup(writer, "Debug", "x64", "StaticLibrary", "true");
                WritePropertyGroup(writer, "Release", "x64", "StaticLibrary", "false");
                WritePropertyGroup(writer, "Diagnostics", "x64", "StaticLibrary", "false");
                WritePropertyGroup(writer, "SimulationOnly", "x64", "StaticLibrary", "false");
                WritePropertyGroup(writer, "TraceOnly", "x64", "StaticLibrary", "false");
                WritePropertyGroup(writer, "MetricsOnly", "x64", "StaticLibrary", "false");

                WritePropertyGroup(writer, "Debug", "ARM64", "StaticLibrary", "true");
                WritePropertyGroup(writer, "Release", "ARM64", "StaticLibrary", "false");
                WritePropertyGroup(writer, "Diagnostics", "ARM64", "StaticLibrary", "false");
                WritePropertyGroup(writer, "SimulationOnly", "ARM64", "StaticLibrary", "false");
                WritePropertyGroup(writer, "TraceOnly", "ARM64", "StaticLibrary", "false");
                WritePropertyGroup(writer, "MetricsOnly", "ARM64", "StaticLibrary", "false");

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
                WriteImportGroup(writer, "SimulationOnly", "x64", output_property, "AdditionalIncludes.props", "WinRelease.props", "Simulation.props");
                WriteImportGroup(writer, "TraceOnly", "x64", output_property, "AdditionalIncludes.props", "WinRelease.props", "TraceOnly.props");
                WriteImportGroup(writer, "MetricsOnly", "x64", output_property, "AdditionalIncludes.props", "WinRelease.props", "MetricsOnly.props");

                WriteImportGroup(writer, "Debug", "ARM64", output_property, "AdditionalIncludes.props", "WinDebug.props");
                WriteImportGroup(writer, "Release", "ARM64", output_property, "AdditionalIncludes.props", "WinRelease.props");
                WriteImportGroup(writer, "Diagnostics", "ARM64", output_property, "AdditionalIncludes.props", "WinRelease.props", "Diagnostics.props");
                WriteImportGroup(writer, "SimulationOnly", "ARM64", output_property, "AdditionalIncludes.props", "WinRelease.props", "Simulation.props");
                WriteImportGroup(writer, "TraceOnly", "ARM64", output_property, "AdditionalIncludes.props", "WinRelease.props", "TraceOnly.props");
                WriteImportGroup(writer, "MetricsOnly", "ARM64", output_property, "AdditionalIncludes.props", "WinRelease.props", "MetricsOnly.props");

                // }
                // {
                writer.WriteStartElement("PropertyGroup");
                writer.WriteAttributeString("Label", "UserMacros");
                writer.WriteEndElement();
                // }
                // {
                WritePropertyGroupUserMacros(writer, "Debug", "x64");
                WritePropertyGroupUserMacros(writer, "Release", "x64");
                WritePropertyGroupUserMacros(writer, "Diagnostics", "x64");
                WritePropertyGroupUserMacros(writer, "SimulationOnly", "x64");
                WritePropertyGroupUserMacros(writer, "TraceOnly", "x64");
                WritePropertyGroupUserMacros(writer, "MetricsOnly", "x64");

                WritePropertyGroupUserMacros(writer, "Debug", "ARM64");
                WritePropertyGroupUserMacros(writer, "Release", "ARM64");
                WritePropertyGroupUserMacros(writer, "Diagnostics", "ARM64");
                WritePropertyGroupUserMacros(writer, "SimulationOnly", "ARM64");
                WritePropertyGroupUserMacros(writer, "TraceOnly", "ARM64");
                WritePropertyGroupUserMacros(writer, "MetricsOnly", "ARM64");

                // }
                // {

                WriteItemDefinitionGroup(writer, "Debug", "x64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log");
                WriteItemDefinitionGroup(writer, "Release", "x64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log");
                WriteItemDefinitionGroup(writer, "Diagnostics", "x64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log");
                WriteItemDefinitionGroup(writer, "SimulationOnly", "x64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log");
                WriteItemDefinitionGroup(writer, "TraceOnly", "x64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log");
                WriteItemDefinitionGroup(writer, "MetricsOnly", "x64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log");

                WriteItemDefinitionGroup(writer, "Debug", "ARM64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log");
                WriteItemDefinitionGroup(writer, "Release", "ARM64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log");
                WriteItemDefinitionGroup(writer, "Diagnostics", "ARM64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log");
                WriteItemDefinitionGroup(writer, "SimulationOnly", "ARM64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log");
                WriteItemDefinitionGroup(writer, "TraceOnly", "ARM64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log");
                WriteItemDefinitionGroup(writer, "MetricsOnly", "ARM64", "/DTWMETRICS_ENABLE", "Console", "TWCoreLibWin.lib;TWServerLibWin.lib;kernel32.lib;user32.lib;gdi32.lib;winspool.lib;comdlg32.lib;advapi32.lib;shell32.lib;ole32.lib;oleaut32.lib;uuid.lib;odbc32.lib;odbccp32.lib;", "$(SolutionDir)build\\$(ProjectName)\\$(Platform)\\$(Configuration)\\$(MSBuildProjectName).log");

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


        private void WriteProjectConfiguration(XmlWriter writer, String configuration, String platform)
        {
            writer.WriteStartElement("ProjectConfiguration");
            writer.WriteAttributeString("Include", configuration + "|" + platform);
            writer.WriteElementString("Configuration", configuration);
            writer.WriteElementString("Platform", platform);
            writer.WriteEndElement();
        }

        private void WritePropertyGroup(XmlWriter writer, String configuration, String platform, String configtype, String use_debug_libraries)
        {
            writer.WriteStartElement("PropertyGroup");
            writer.WriteAttributeString("Condition", "'$(Configuration)|$(Platform)'=='" + configuration + "|" + platform + "'");
            writer.WriteAttributeString("Label", "Configuration");
            writer.WriteElementString("ConfigurationType", configtype);
            writer.WriteElementString("UseDebugLibraries", use_debug_libraries);
            writer.WriteElementString("PlatformToolset", "v142");
            writer.WriteElementString("CharacterSet", "Unicode");
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

        private void WritePropertyGroupUserMacros(XmlWriter writer, String configuration, String platform)
        {
            writer.WriteStartElement("PropertyGroup");
            writer.WriteAttributeString("Condition", "'$(Configuration)|$(Platform)'=='" + configuration + "|" + platform + "'");
            writer.WriteEndElement();
        }

        private void WriteItemDefinitionGroup(XmlWriter writer,String configuration, String platform , String preprocessor, String sub_system, String libs, String buildlogpath)
        { 
            writer.WriteStartElement("ItemDefinitionGroup");
            writer.WriteAttributeString("Condition", "'$(Configuration)|$(Platform)'=='" + configuration + "|" + platform + "'");

            writer.WriteStartElement("ClCompile");
            writer.WriteElementString("AdditionalOptions", preprocessor + "%(AdditionalOptions)");
            writer.WriteElementString("AssemblerListingLocation", "$(IntDir)");
            writer.WriteElementString("ObjectFileName", "$(IntDir)");
            writer.WriteElementString("ProgramDataBaseFileName", "$(IntDir)vc$(PlatformToolsetVersion).pdb");      
            writer.WriteEndElement();

            writer.WriteStartElement("Link");
            writer.WriteElementString("SubSystem", sub_system);
            writer.WriteElementString("AdditionalDependencies", libs + "% (AdditionalDependencies)");
            writer.WriteElementString("AdditionalLibraryDirectories", "$(SolutionDir)lib\\$(Platform)\\$(Configuration)\\");
            writer.WriteEndElement();

            writer.WriteStartElement("BuildLog");
            writer.WriteElementString("Path", buildlogpath);
            writer.WriteEndElement();

            writer.WriteEndElement();
            }
    }
}
