using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.TextManager.Interop;
using EnvDTE80;
using EnvDTE;

namespace TallyUtil
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class TabOverCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("b0b04262-44b2-40cd-9159-c39e6e7d99a1");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="TabOverCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private TabOverCommand(AsyncPackage package, OleMenuCommandService commandService)
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
        public static TabOverCommand Instance
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
            // Switch to the main thread - the call to AddCommand in TabOverCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new TabOverCommand(package, commandService);
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
            MoveCursor(ServiceProvider);
        }

        private void MoveCursor(IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetService(typeof(SVsTextManager));
            var textManager = service as IVsTextManager2;
            int tabSize = GetTabSize();
            IVsTextLines textLines;
            IVsTextView view;

            int result = textManager.GetActiveView2(1, null, (uint)_VIEWFRAMETYPE.vftCodeWindow, out view);

            view.GetCaretPos(out int startLine, out int startColumn);

            view.GetBuffer(out textLines);

            int maxColumn;
            textLines.GetLengthOfLine(startLine, out maxColumn);
            if (maxColumn == 0)
            {
                string szPad = "";
                addPadding(startColumn, ref szPad);
                addPadding(tabSize, ref szPad);
                IntPtr pText = System.Runtime.InteropServices.Marshal.StringToCoTaskMemAuto(szPad);

                textLines.ReplaceLines(startLine, 0, startLine, startColumn+tabSize, pText, szPad.Length, new TextSpan[szPad.Length]);
                view.SetCaretPos(startLine, (szPad.Length));
            }
            else if (startColumn == maxColumn) //if cursor is at the last character,insert a tab
            {
                textLines.GetLineText(startLine, 0, startLine, maxColumn, out string buffer);
                string szPad = buffer;

                //addPadding(tabSize, ref szPad);
                int tabSpace = CursorPos(startColumn, tabSize);
                addPadding((tabSpace- startColumn), ref szPad);

                IntPtr pText = System.Runtime.InteropServices.Marshal.StringToCoTaskMemAuto(szPad);
                textLines.ReplaceLines(startLine, 0, startLine, maxColumn, pText, szPad.Length, new TextSpan[szPad.Length]);
                view.SetCaretPos(startLine, (szPad.Length));
            }
            else if ((startColumn + tabSize) >= maxColumn) //if cursor's new position is at or beyond last character, move to end of line
            {
                view.SetCaretPos(startLine, maxColumn);
            }
            else
            {
                view.SetCaretPos(startLine, CursorPos(startColumn, tabSize));
            }
        }

        int CursorPos(int pos, int tabSize)
        {
            if (pos == 0)
                return tabSize;

            for (int i = pos+1; i <= pos+ tabSize; i++)
            {
                if (i % tabSize == 0)
                    return i;
            }

            return 0;
        }

        private int GetTabSize()
        {
            DTE2 dte = this.ServiceProvider.GetService(typeof(DTE)) as DTE2;

            return dte.ActiveDocument.TabSize;
        }

        private void addPadding(int tabSize, ref string szPad)
        {
            for (int i = 0; i < tabSize; i++)
            {
                szPad += " ";
            }
        }
    }
}
