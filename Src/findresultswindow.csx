#r "Microsoft.VisualStudio.ComponentModelHost.dll"
#r "Microsoft.VisualStudio.Editor.dll"
#r "Microsoft.VisualStudio.Shell.Interop.dll"
#r "Microsoft.VisualStudio.Text.Data.dll"
#r "Microsoft.VisualStudio.TextManager.Interop.dll"

#load "util.csx"

using EnvDTE;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.ComponentModelHost;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Vim;
using Vim.Extensions;

public class FindResultsWindow
{

    private const string vsWindowKindFindResults1 = "{0F887920-C2B6-11D2-9375-0080C747D9A0}";

    private IVimBuffer vimBuffer;
    private Window findResultsWindow;
    private bool autoHides = true;
    private IVsTextView textView;
    private IWpfTextView wpfTextView;

    public FindResultsWindow(IVimBuffer vimBuffer)
    {
        this.vimBuffer = vimBuffer;
    }

    public void Display()
    {
        var DTE = Util.GetDTE2();

        findResultsWindow = DTE.Windows.Item(vsWindowKindFindResults1);

        var uiShell = (IVsUIShell)Package.GetGlobalService(typeof(SVsUIShell));
        IVsWindowFrame windowFrame = null;
        var gi = new Guid(vsWindowKindFindResults1);
        uiShell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fFindFirst, ref gi, out windowFrame);

        object docView;
        windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out docView);

        textView = docView as IVsTextView;

        IComponentModel componentModel = Package.GetGlobalService(typeof(SComponentModel)) as IComponentModel;
        var factory = componentModel.GetService<IVsEditorAdaptersFactoryService>();

        wpfTextView = factory.GetWpfTextView(textView);

        //Action messageAction = null;

        vimBuffer.KeyInputStart += OnKeyInputStart;
        vimBuffer.Closed += OnBufferClosed;

        autoHides = findResultsWindow.AutoHides;

        findResultsWindow.AutoHides = false;
        findResultsWindow.Activate();

        Select();
        DTE.ActiveDocument.Activate();
    }
    public void Select()
    {
        ITextSnapshotLine line = wpfTextView.Caret.Position.BufferPosition.GetContainingLine();
        if (line.LineNumber <= 1 && 3 <= wpfTextView.TextSnapshot.LineCount)
        {
            line = wpfTextView.TextSnapshot.GetLineFromLineNumber(1);
            textView.SetSelection(1, line.End, 1, 0);
        }
    }
    public void OnKeyInputStart(object sender, KeyInputStartEventArgs e)
    {
        e.Handled = true;
        ITextSnapshotLine line;
        int lineNumber;

        if (e.KeyInput.Char == 'j')
        {
            line = wpfTextView.Selection.SelectedSpans[0].Start.GetContainingLine();
            if (line.LineNumber < (wpfTextView.TextSnapshot.LineCount - 3))
            {
                lineNumber = line.LineNumber + 1;
                //Start from the end.
                //Do not move horizontally.
                line = wpfTextView.TextSnapshot.GetLineFromLineNumber(lineNumber);
                textView.SetSelection(lineNumber, line.End, lineNumber, 0);
            }
        }
        else if (e.KeyInput.Char == 'k')
        {
            line = wpfTextView.Selection.SelectedSpans[0].Start.GetContainingLine();
            if (1 < line.LineNumber)
            {
                lineNumber = line.LineNumber - 1;
                //Start from the end.
                //Do not move horizontally.
                line = wpfTextView.TextSnapshot.GetLineFromLineNumber(lineNumber);
                textView.SetSelection(lineNumber, line.End, lineNumber, 0);
            }
        }
        else if (e.KeyInput.Key == VimKey.Enter)
        {
            EndIntercept();
            findResultsWindow.Activate();
        }
        else if (e.KeyInput.Key == VimKey.Escape)
        {
            EndIntercept();
        }
        //messageAction?.Invoke();
    }
    public void OnBufferClosed(object sender, EventArgs e)
    {
        EndIntercept();
    }
    public void EndIntercept()
    {
        vimBuffer.KeyInputStart -= OnKeyInputStart;
        vimBuffer.Closed -= OnBufferClosed;
        //messageAction = null;
        vimBuffer.DisplayStatus(string.Empty);
        findResultsWindow.AutoHides = autoHides;
    }
}
