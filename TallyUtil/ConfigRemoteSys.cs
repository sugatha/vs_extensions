using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace TallyUtil
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ConfigRemoteSys
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("7d59e09d-41b1-4deb-a8ab-b565dcae2db9");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigRemoteSys"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private ConfigRemoteSys(AsyncPackage package, OleMenuCommandService commandService)
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
        public static ConfigRemoteSys Instance
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
            // Switch to the main thread - the call to AddCommand in ConfigRemoteSys's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new ConfigRemoteSys(package, commandService);
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
            ConfigRemoteBuildSys();
            /*ThreadHelper.ThrowIfNotOnUIThread();
            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            string title = "ConfigRemoteSys";

            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                this.package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);*/
        }

        private string GetCurrentProjectFileName()
        {

            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            string title = "ItemContextCommand";

            IntPtr hierarchyPointer, selectionContainerPointer;
            Object selectedObject = null;
            IVsMultiItemSelect multiItemSelect;
            uint projectItemId;

            IVsMonitorSelection monitorSelection =
                    (IVsMonitorSelection)Package.GetGlobalService(
                    typeof(SVsShellMonitorSelection));

            monitorSelection.GetCurrentSelection(out hierarchyPointer,
                                                 out projectItemId,
                                                 out multiItemSelect,
                                                 out selectionContainerPointer);

            IVsHierarchy selectedHierarchy = Marshal.GetTypedObjectForIUnknown(
                                                 hierarchyPointer,
                                                 typeof(IVsHierarchy)) as IVsHierarchy;

            if (selectedHierarchy != null)
            {
                ErrorHandler.ThrowOnFailure(selectedHierarchy.GetProperty(
                                                  projectItemId,
                                                  (int)__VSHPROPID.VSHPROPID_ExtObject,
                                                  out selectedObject));
            }

            Project selectedProject = selectedObject as Project;

            string projectPath = selectedProject.FullName;

            return selectedProject.FullName;

        }
        private void ConfigRemoteBuildSys()
        {
            string userFileName = GetCurrentProjectFileName() + ".user";

            //string path = @"%%LOCALAPPDATA%%\\Microsoft\\Linux\\User Data\\3.0\\store.xml";
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\\Linux\\User Data\\3.0\\store.xml");

            var doc = XDocument.Load(path);

            List<tHosts> hostList = new List<tHosts>();

            IEnumerable<XElement> entries = doc.Descendants("entry");
            foreach (XElement entry in entries)
            {
                tHosts host = new tHosts();
                host.id = entry.Attribute("id").Value;
                host.hostname = entry.Element("hostname").Value;
                host.username = entry.Element("credentials").Element("username").Value;
                host.port = entry.Element("port").Value;

                hostList.Add(host);
            }


            RemoteBuildSysDiag obj = new RemoteBuildSysDiag(hostList);

            obj.ShowDialog();

            var myIndex = obj.selectedIndex;

            if (myIndex == 0)
                return;
            XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";

            XDocument newprofiledoc =
            new XDocument(
                new XComment("This .user file is generated by VS2019 extension"),
                new XElement(ns + "Project", 
                    new XAttribute("ToolsVersion", "Current"),
                    new XElement(ns + "PropertyGroup", 
                        new XAttribute("Condition", "'$(Configuration)|$(Platform)'=='Debug|x64'"),
                        new XElement(ns + "RemoteTarget", hostList[myIndex].id + ";" + hostList[myIndex].hostname)),
                    new XElement(ns + "PropertyGroup",
                            new XAttribute("Condition", "'$(Configuration)|$(Platform)'=='Release|x64'"),
                            new XElement(ns + "RemoteTarget", hostList[myIndex].id + ";" + hostList[myIndex].hostname)),
                    new XElement(ns + "PropertyGroup",
                            new XAttribute("Condition", "'$(Configuration)|$(Platform)'=='Diagnostics|x64'"),
                            new XElement(ns + "RemoteTarget", hostList[myIndex].id + ";" + hostList[myIndex].hostname)),
                    new XElement(ns + "PropertyGroup",
                            new XAttribute("Condition", "'$(Configuration)|$(Platform)'=='MetricsOnly|x64'"),
                            new XElement(ns + "RemoteTarget", hostList[myIndex].id + ";" + hostList[myIndex].hostname)),
                    new XElement(ns + "PropertyGroup",
                            new XAttribute("Condition", "'$(Configuration)|$(Platform)'=='Simulation|x64'"),
                            new XElement(ns + "RemoteTarget", hostList[myIndex].id + ";" + hostList[myIndex].hostname)),
                    new XElement(ns + "PropertyGroup",
                            new XAttribute("Condition", "'$(Configuration)|$(Platform)'=='TraceOnly|x64'"),
                            new XElement(ns + "RemoteTarget", hostList[myIndex].id + ";" + hostList[myIndex].hostname)))
            );

            newprofiledoc.Save(userFileName);




        }
    }
}
