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
    private IVim vim;
    private IWpfTextView wpfTextView;

    public FindResultsWindow(IVim vim)
    {
        this.vim = vim;
    }

    public void Display()
    {
        if (!vim.TryGetActiveVimBuffer(out vimBuffer))
        {
            vim.DisplayError("Can not get VimBuffer");
            return;
        }

        var DTE = Util.GetDTE2();

        findResultsWindow = DTE.Windows.Item(vsWindowKindFindResults1);

        var uiShell = (IVsUIShell)Package.GetGlobalService(typeof(SVsUIShell));
        IVsWindowFrame windowFrame = null;
        var gi = new Guid(vsWindowKindFindResults1);
        uiShell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fFindFirst, ref gi, out windowFrame);

        object docView;
        windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out docView);

        var textView = docView as IVsTextView;

        IComponentModel componentModel = Package.GetGlobalService(typeof(SComponentModel)) as IComponentModel;
        var factory = componentModel.GetService<IVsEditorAdaptersFactoryService>();

        wpfTextView = factory.GetWpfTextView(textView);

        //Action messageAction = null;

        vimBuffer.KeyInputStart += OnKeyInputStart;
        vimBuffer.Closed += OnBufferClosed;

        autoHides = findResultsWindow.AutoHides;

        findResultsWindow.AutoHides = false;
        findResultsWindow.Activate();
        DTE.ActiveDocument.Activate();
    }

    public void OnKeyInputStart(object sender, KeyInputStartEventArgs e)
    {
        e.Handled = true;
        EnvDTE.TextSelection selection;

        if (e.KeyInput.Char == 'j')
        {
            selection = findResultsWindow.Selection as EnvDTE.TextSelection;
            if (selection != null && (selection.CurrentLine < (wpfTextView.TextSnapshot.LineCount - 2)))
            {
                selection.GotoLine(selection.CurrentLine + 1, true);
            }
        }
        else if (e.KeyInput.Char == 'k')
        {
            selection = findResultsWindow.Selection as EnvDTE.TextSelection;
            if (selection != null && 2 < selection.CurrentLine)
            {
                selection.GotoLine(selection.CurrentLine - 1, true);
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
        vim.DisplayStatus(string.Empty);
        findResultsWindow.AutoHides = autoHides;
    }
}
