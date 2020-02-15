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
            /*ThreadHelper.ThrowIfNotOnUIThread();
            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            string title = "CreateProject";

            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                this.package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);*/
        }
        private void CreateNewProject()
        {
            NewProject obj = new NewProject(GetCurrentProjectFileName());

            obj.ShowDialog();

            szCurrPath = obj.currPath;
            szSharedItemName = obj.sharedItemName;
            szNewProjectName = obj.newProjectname;
            szProjectType = obj.projectType;

            CreateProjectFile();
            // we have the variables of the dialog

        }
        private string GetCurrentProjectFileName()
        {

            DTE dte = (DTE)ServiceProvider.GetService(typeof(DTE));
            string solutionDir = System.IO.Path.GetDirectoryName(dte.Solution.FullName);
            return solutionDir;

        }

        private void CreateProjectFile()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            settings.OmitXmlDeclaration = true;
            settings.NewLineOnAttributes = true;

            using (XmlWriter writer = XmlWriter.Create(szCurrPath + "\\" + szNewProjectName + ".vcxproj", settings))
            {
                // {
                writer.WriteStartElement("Project", "http://schemas.microsoft.com/developer/msbuild/2003");
                writer.WriteAttributeString("DefaultTargets", "Build");
                writer.WriteStartElement("ItemGroup");
                writer.WriteAttributeString("Label", "ProjectConfigurations");
                writer.WriteStartElement("ProjectConfiguration");
                writer.WriteAttributeString("Include", "Debug|x64");
                writer.WriteElementString("Configuration", "Debug");
                writer.WriteEndElement();
                writer.WriteEndElement();
                // }
                // {
                writer.WriteStartElement("PropertyGroup");
                writer.WriteAttributeString("Label", "Globals");
                writer.WriteElementString("VCProjectVersion", "16.0");
                writer.WriteElementString("ProjectGuid", "{F1498C1E-C4C5-4FFB-AAFF-137C4C53BAB7}");
                writer.WriteElementString("Keyword", "Win32Proj");
                writer.WriteElementString("RootNamespace", "TWCoreLibWin");
                writer.WriteElementString("WindowsTargetPlatformVersion", "10.0");
                writer.WriteEndElement();
                // }
                // {
                writer.WriteStartElement("Import");
                writer.WriteAttributeString("Project", "$(VCTargetsPath)\\Microsoft.Cpp.Default.props");
                writer.WriteEndElement();
                // }
                // {
                writer.WriteStartElement("PropertyGroup");
                writer.WriteAttributeString("Condition", "'$(Configuration)|$(Platform)'=='Debug|x64'");
                writer.WriteAttributeString("Label", "Configuration");
                writer.WriteElementString("ConfigurationType", "StaticLibrary");
                writer.WriteElementString("UseDebugLibraries", "true");
                writer.WriteElementString("PlatformToolset", "v142");
                writer.WriteElementString("CharacterSet", "Unicode");
                writer.WriteEndElement();
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
                if(szSharedItemName.Length != 0)
                writer.WriteAttributeString("Project", szSharedItemName);
                writer.WriteAttributeString("Label", "Shared");
                writer.WriteEndElement();
                writer.WriteEndElement();
                // }

                // {
                writer.WriteStartElement("ImportGroup");
                writer.WriteAttributeString("Label", "PropertySheets");
                writer.WriteAttributeString("Condition", "'$(Configuration)|$(Platform)'=='Debug|x64'");

                writer.WriteStartElement("Import");
                writer.WriteAttributeString("Project", "$(UserRootDir)\\Microsoft.Cpp.$(Platform).user.props");
                writer.WriteAttributeString("Condition", "exists('$(UserRootDir)\\Microsoft.Cpp.$(Platform).user.props')");
                writer.WriteAttributeString("Label", "LocalAppDataPlatform");
                writer.WriteEndElement();

                writer.WriteStartElement("Import");
                writer.WriteAttributeString("Project", "LibraryOutput.props");
                writer.WriteEndElement();

                writer.WriteStartElement("Import");
                writer.WriteAttributeString("Project", "AdditionalIncludes.props");
                writer.WriteEndElement();

                writer.WriteStartElement("Import");
                writer.WriteAttributeString("Project", "WinDebug.props");
                writer.WriteEndElement();

                writer.WriteEndElement();
                // }
                // {
                writer.WriteStartElement("PropertyGroup");
                writer.WriteAttributeString("Label", "UserMacros");
                writer.WriteEndElement();
                // }
                // {
                writer.WriteStartElement("PropertyGroup");
                writer.WriteAttributeString("Condition", "'$(Configuration)|$(Platform)'=='Debug|x64'");
                writer.WriteElementString("LinkIncremental", "true");
                writer.WriteElementString("RunCodeAnalysis", "false");
                writer.WriteElementString("CodeAnalysisRuleSet", "AllRules.ruleset");
                writer.WriteEndElement();
                // }
                // {
                writer.WriteStartElement("ItemDefinitionGroup");
                writer.WriteAttributeString("Condition", "'$(Configuration)|$(Platform)'=='Debug|x64'");

                writer.WriteStartElement("ClCompile");
                writer.WriteElementString("PrecompiledHeader", "NotUsing");
                writer.WriteElementString("Optimization", "Disabled");
                writer.WriteElementString("SDLCheck", "true");
                writer.WriteElementString("ConformanceMode", "true");
                writer.WriteElementString("PrecompiledHeaderFile", "pch.h");
                writer.WriteElementString("AdditionalOptions", "/DTRACE /DTWMETRICS_ENABLE %(AdditionalOptions)");
                writer.WriteEndElement();

                writer.WriteStartElement("Link");
                writer.WriteElementString("SubSystem", "Windows");
                writer.WriteElementString("GenerateDebugInformation", "true");
                writer.WriteEndElement();

                writer.WriteStartElement("BuildLog");
                writer.WriteEndElement();

                writer.WriteEndElement();
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

    }
}
