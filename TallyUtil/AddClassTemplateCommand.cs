using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;

namespace TallyUtil
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class AddClassTemplateCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("6001978e-fd37-4a83-b58a-5ed762f582c9");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddClassTemplateCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private AddClassTemplateCommand(AsyncPackage package, OleMenuCommandService commandService)
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
        public static AddClassTemplateCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
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
            // Switch to the main thread - the call to AddCommand in AddClassTemplateCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new AddClassTemplateCommand(package, commandService);
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
            GenerateClass(ServiceProvider);

            /*
            ThreadHelper.ThrowIfNotOnUIThread();
            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            string title = "AddClassTemplateCommand";

            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                this.package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            */
        }

        private Microsoft.VisualStudio.Editor.IVsEditorAdaptersFactoryService GetEditorAdaptersFactoryService()
        {
            Microsoft.VisualStudio.ComponentModelHost.IComponentModel componentModel =
                (Microsoft.VisualStudio.ComponentModelHost.IComponentModel)this.ServiceProvider.GetService(
                    typeof(Microsoft.VisualStudio.ComponentModelHost.SComponentModel));
            return componentModel.GetService<Microsoft.VisualStudio.Editor.IVsEditorAdaptersFactoryService>();
        }

        private void GenerateClass(IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetService(typeof(SVsTextManager));
            var textManager = service as IVsTextManager2;
            IVsTextView view;

            int result = textManager.GetActiveView2(1, null, (uint)_VIEWFRAMETYPE.vftCodeWindow, out view);

            view.GetCaretPos(out int startLine, out int startColumn);

            IWpfTextView v = GetEditorAdaptersFactoryService().GetWpfTextView(view);

            string strClass = formClassTemplate();

            ITextEdit edit = v.TextBuffer.CreateEdit();
            edit.Insert(v.Caret.Position.BufferPosition, strClass);
            edit.Apply();
        }

        private string formClassTemplate()
        {
            var sb = new System.Text.StringBuilder();

            // insert an empty line
            // begin of line            
            sb.Append("\n");

            // put public
            sb.Append("\tpublic:");
            sb.Append("\n");

            // put comment for constructor
            sb.Append("\t\t// constructors");
            sb.Append("\n");

            // put comment for destructor
            sb.Append("\t\t// destructors");
            sb.Append("\n");

            // put comment for modifiers
            sb.Append("\t\t// modifiers");
            sb.Append("\n");

            // put comment for selectors
            sb.Append("\t\t// selectors");
            sb.Append("\n");

            // put comment for iterators
            sb.Append("\t\t// iterators");
            sb.Append("\n");

            // put comment for friends
            sb.Append("\t\t// friends");
            sb.Append("");

            // put protected
            sb.Append("\n");
            sb.Append("\tprotected:");
            sb.Append("\n");

            // put private
            sb.Append("\tprivate:");
            sb.Append("\n");

            return sb.ToString();
        }
    }
}
