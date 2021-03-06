﻿using System;
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
    internal sealed class FuncPrev
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("8ac24453-aaa9-49ac-af9c-0003e3101278");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="FuncPrev"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private FuncPrev(AsyncPackage package, OleMenuCommandService commandService)
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
        public static FuncPrev Instance
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
            // Switch to the main thread - the call to AddCommand in FuncPrev's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new FuncPrev(package, commandService);
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
            DoFuncPrev(ServiceProvider);
        }
        private void DoFuncPrev(IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetService(typeof(SVsTextManager));
            var textManager = service as IVsTextManager2;
            IVsTextView view;

            int result = textManager.GetActiveView2(1, null, (uint)_VIEWFRAMETYPE.vftCodeWindow, out view);

            IVsTextLines textLines;
            view.GetCaretPos(out int startLine, out int startColumn);
            view.GetBuffer(out textLines);

            textLines.GetLineCount(out int maxLineCount);
            bool movecursor = true;            
            int i = 0;
            for (i = startLine - 1; i > 0; i--)
            {
                bool readNextLine = true;
                textLines.GetLengthOfLine(i, out int maxColumn);

                textLines.GetLineText(i, 0, i, maxColumn, out string buffer);

                var camelCaseExpr = new Regex("^{");

                MatchCollection matches = camelCaseExpr.Matches(buffer);

                foreach (Match match in matches)
                {
                    view.SetCaretPos(i, (match.Index));
                    movecursor = false;
                    view.SetTopLine(i-2);
                    readNextLine = false;
                    break;
                }
                if (readNextLine == false)
                    break;
            }
            /* this is to set focus to 2 lines above the "{" when we reach the top-of-the-page */
           /* if(movecursor == true)
            {
                textLines.GetLineText(startLine, 0, startLine, 2, out string szbuffer);
                if (szbuffer.StartsWith("{"))
                {
                    view.SetTopLine(startLine - 2);
                }
            }*/

        }
    }
}
