using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Text;
using EnvDTE;
using EnvDTE80;

namespace TallyUtil
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class TraceFunc
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("72b42dd0-baf5-4139-843a-602e2afa39df");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceFunc"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private TraceFunc(AsyncPackage package, OleMenuCommandService commandService)
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
        public static TraceFunc Instance
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
            // Switch to the main thread - the call to AddCommand in TraceFunc's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new TraceFunc(package, commandService);
        }

        private IVsEditorAdaptersFactoryService GetEditorAdaptersFactoryService()
        {
            IComponentModel componentModel = (IComponentModel)this.ServiceProvider.GetService(typeof(SComponentModel));

            return componentModel.GetService<IVsEditorAdaptersFactoryService>();
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
            DoTraceFunc(ServiceProvider);
            /*ThreadHelper.ThrowIfNotOnUIThread();
            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            string title = "TraceFunc";

            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                this.package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);*/
        }
        private void DoTraceFunc(IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetService(typeof(SVsTextManager));
            var textManager = service as IVsTextManager2;
            IVsTextView textView;

            int result = textManager.GetActiveView2(1, null, (uint)_VIEWFRAMETYPE.vftCodeWindow, out textView);
            
            Microsoft.VisualStudio.Text.Editor.IWpfTextView wpfTextView = GetEditorAdaptersFactoryService().GetWpfTextView(textView);                        

            string entireBuffer = wpfTextView.TextBuffer.CurrentSnapshot.GetText();

            string currentFileName = GetCurrentFilenameFromEditor();
            if (currentFileName.Contains(".hxx") || currentFileName.Contains(".hpp"))
            {
                /* we need to remove the tracefunc and traceadd */
                RemoveTraceFunc(wpfTextView);
            }
            else
            {
                /* check if there is a comment to remove the tracefunc*/
                if(CheckIfTraceFunc(entireBuffer))
                {
                    RemoveTraceFunc(wpfTextView);
                }
                else 
                {
                    AddTraceFunc(wpfTextView, textView);
                }

            }

        }

        bool CheckIfTraceFunc(string entireBuffer)
        {
            return entireBuffer.Contains("/*NoTrace*/");
        }

        void AddTraceFunc(Microsoft.VisualStudio.Text.Editor.IWpfTextView pWpfTextView, IVsTextView textView)
        {
            int totalLineCount = pWpfTextView.TextSnapshot.LineCount;

            Microsoft.VisualStudio.Text.ITextEdit edit = pWpfTextView.TextBuffer.CreateEdit();

            ITextSnapshot snapshot = edit.Snapshot;

            int lineNumber = 0;
            //Regex startFuncExpr = new Regex("^{");
            //Regex endFuncExpr = new Regex("^}");
                        
            int startFuncIterCount = 0;
            while (lineNumber < snapshot.LineCount)
            {
                int startFuncLine = GetFuncStartLineNumber(snapshot, lineNumber,out int index);
                int endFuncLine = 0;
                bool isTraceFuncPresent = false;

                if (startFuncLine != 0)
                {
                    lineNumber = startFuncLine;
                    endFuncLine = GetFuncEndLineNumber(snapshot, startFuncLine + 1, out isTraceFuncPresent);
                    if (endFuncLine != 0)
                    {
                        lineNumber = endFuncLine;
                        if (isTraceFuncPresent == false)
                        {                            
                            // add tracefunc
                            InsertTraceFunc(startFuncLine, edit, textView);
                            //break;
                        }
                        else
                        {
                            //continue looking for the start of the next func.
                        }
                    }
                }
                else
                {
                    if (startFuncIterCount > 1)
                        break;
                    else
                        startFuncIterCount++;
                }

            }
            edit.Apply();
            /*foreach (ITextSnapshotLine line in snapshot.Lines)
            {
                string lineText = line.GetText();
                Regex camelCaseExpr;
                if (foundFuncStart == false)
                    camelCaseExpr = new Regex("^{");
                else
                    camelCaseExpr = new Regex("^}");

                MatchCollection matches = camelCaseExpr.Matches(lineText);

                int funcStart = 0;
                int funcEnd = 0;
                if (matches.Count != 0)
                {
                    funcStart = line.LineNumber;
                    //loop through other lines till i get ^}
                    foundFuncStart = true;
                    continue;
                }
                if(foundFuncStart == true)
                {


                }
            }*/

        }

        void InsertTraceFunc(int posToAddTraceFunc, Microsoft.VisualStudio.Text.ITextEdit edit, IVsTextView textView)
        {
            string stub = "TRACEFUNC;";
            addPrePaddingAndLineBreak(2*GetIndentSize(), ref stub);
            textView.GetBuffer(out IVsTextLines textLines);
            textLines.GetPositionOfLineIndex(posToAddTraceFunc+1, 0, out int newPosition);
            edit.Insert(newPosition, stub);
        }

        int GetFuncStartLineNumber (ITextSnapshot snapshot, int startFrom, out int index)
        {
            index = 0;
            Regex startFuncExpr = new Regex("^{");
            for (int i = startFrom; i < snapshot.LineCount; i++)
            {
                string line = snapshot.GetLineFromLineNumber(i).GetText();


                MatchCollection matches = startFuncExpr.Matches(line);

                if (matches.Count != 0)
                {
                    index = matches[0].Index;
                    return i;                    
                }
            }
            return 0;
        }

        int GetFuncEndLineNumber(ITextSnapshot snapshot, int startFrom, out bool isTraceFuncPresent)
        {
            isTraceFuncPresent = false;
            Regex endFuncExpr = new Regex("^}");
            for (int i = startFrom; i < snapshot.LineCount; i++)
            {
                string line = snapshot.GetLineFromLineNumber(i).GetText();

                if(line.Contains("TRACEFUNC"))
                {
                    isTraceFuncPresent = true;
                }
                MatchCollection matches = endFuncExpr.Matches(line);

                if (matches.Count != 0)
                {
                    return i;
                }
            }
            return 0;
        }

        /*bool GetLineNumber (ITextSnapshot snapshot, Regex findText, int startFrom, out int lineNumber)
        {
            bool foundTrace = false;

            startFrom++;

            lineNumber = 0;

            for (int i = startFrom; i < snapshot.LineCount; i++)
            {
                string line = snapshot.GetLineFromLineNumber(i).GetText();

                if (line.Contains("TRACEFUNC") || line.Contains("TRACEADD"))
                {
                    foundTrace = true;
                    break;
                }

                MatchCollection matches = findText.Matches(line);

                if (matches.Count != 0)
                {
                    lineNumber = i;
                    break;
                }
            }

            return foundTrace;
        }*/

        void RemoveTraceFunc( Microsoft.VisualStudio.Text.Editor.IWpfTextView pWpfTextView)
        {
            int totalLineCount = pWpfTextView.TextSnapshot.LineCount;
            
            Microsoft.VisualStudio.Text.ITextEdit edit = pWpfTextView.TextBuffer.CreateEdit();


            ITextSnapshot snapshot = edit.Snapshot;

            foreach (ITextSnapshotLine line in snapshot.Lines)
            {
                string lineText = line.GetText();
                if (lineText.Contains("TRACEFUNC") || lineText.Contains("TRACEADD"))
                {
                    string newline = "\n";
                    Span span = new Span(line.Start,line.Length+ newline.Length);
                    edit.Delete(span);
                }
            }
            edit.Apply();
        }
        private string GetCurrentFilenameFromEditor()
        {
            var textManager = this.ServiceProvider.GetService(typeof(SVsTextManager)) as IVsTextManager;
            int mustHaveFocus = 1;
            textManager.GetActiveView(mustHaveFocus, null, out IVsTextView textView);

            var userData = textView as IVsUserData;
            if (userData == null)
            {
                // no text view is currently open
                return String.Empty;
            }
            else
            {
                Guid guidViewHost = Microsoft.VisualStudio.Editor.DefGuidList.guidIWpfTextViewHost;
                userData.GetData(ref guidViewHost, out object holder);
                Microsoft.VisualStudio.Text.Editor.IWpfTextViewHost viewHost = (Microsoft.VisualStudio.Text.Editor.IWpfTextViewHost)holder;

                viewHost.TextView.TextDataModel.DocumentBuffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument doc);
                return doc.FilePath;
            }
        }
        private int GetIndentSize()
        {
            DTE2 dte = this.ServiceProvider.GetService(typeof(DTE)) as DTE2;

            return dte.ActiveDocument.IndentSize;
        }
        private void addPrePaddingAndLineBreak(int padSize, ref string szPad)
        {
            string temp = "";

            for (int i = 0; i < padSize; i++)
            {
                temp += " ";
            }

            temp += szPad + "\n";

            szPad = temp;
        }
    }
}
