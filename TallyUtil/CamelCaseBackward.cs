using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Task = System.Threading.Tasks.Task;


namespace TallyUtil
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CamelCaseBackward
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("71ddcfaa-0a3d-40e0-878e-d8e4b0da360f");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="CamelCaseBackward"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private CamelCaseBackward(AsyncPackage package, OleMenuCommandService commandService)
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
        public static CamelCaseBackward Instance
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
            // Switch to the main thread - the call to AddCommand in CamelCaseBackward's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new CamelCaseBackward(package, commandService);
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
            DoCamelCaseBackward(ServiceProvider);           
        }
        private void DoCamelCaseBackward(IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetService(typeof(SVsTextManager));
            var textManager = service as IVsTextManager2;
            IVsTextView view;

            int result = textManager.GetActiveView2(1, null, (uint)_VIEWFRAMETYPE.vftCodeWindow, out view);

            IVsTextLines textLines;
            view.GetCaretPos(out int startLine, out int startColumn);
            view.GetBuffer(out textLines);

            textLines.GetLineCount(out int maxLineCount);
            int i = startLine;


            bool readNextLine = true;
            textLines.GetLengthOfLine(i, out int maxColumn);

            textLines.GetLineText(i, 0, i, startColumn, out string buffer);

            var camelCaseExpr = new Regex("[A-Z]+|[0-9]+|\\s+|[_*=!||&();{}<>]+", RegexOptions.RightToLeft);

            MatchCollection matches = camelCaseExpr.Matches(buffer);

            foreach (Match match in matches)
            {
                int nindex = match.Index;
                if (nindex != 0)
                {
                    if (match.Value == " ")
                        view.SetCaretPos(i, (nindex));
                    else
                        view.SetCaretPos(i, (nindex));

                    readNextLine = false;
                    break;
                }
            }
            if (readNextLine == false) { }
            //break;
            else
            {
                /*new...to move*/

                i--;
                for (int j = i; j >= 0; j--)
                {
                    bool fetchNextLine = true;
                    startColumn = 0;


                    textLines.GetLengthOfLine(j, out maxColumn);
                    if (maxColumn == 0)
                        continue;
                    textLines.GetLineText(j, startColumn, j, maxColumn, out buffer);
                    var _camelCaseExpr = new Regex("[a-zA-Z]+|[0-9]+|[_*=!||&();{}<>]+|[\\n\\r]+", RegexOptions.RightToLeft);
                    MatchCollection _matches = _camelCaseExpr.Matches(buffer);
                    foreach (Match _match in _matches)
                    {
                        view.SetCaretPos(j, (_match.Index));
                        fetchNextLine = false;
                        break;
                    }
                    if (fetchNextLine == false)
                        break;
                }

            }

        }
    }
}
