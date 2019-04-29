#load "Util.csx"
#load "WindowUtil.csx"

using EnvDTE80;
using EnvDTE;
using Microsoft.VisualStudio.Text;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Reflection;
using Vim;
using Vim.Extensions;


public class FindAllReferencesWindow
{
    //This guid is not documented
    private const string guidFindAllReferencesWindow = "{A80FEBB4-E7E0-4147-B476-21AAF2453969}";

    private Window findAllReferencesWindow;
    private bool autoHides = true;
    private IVimBuffer vimBuffer;
    private DTE2 dte;

    public FindAllReferencesWindow(IVimBuffer vimBuffer)
    {
        this.vimBuffer = vimBuffer;
        this.dte = Util.GetDTE2();
    }

    public void Display()
    {
        findAllReferencesWindow = dte.Windows.Item(guidFindAllReferencesWindow);
        autoHides = true;

        //Action messageAction = null;

        vimBuffer.KeyInputStart += OnKeyInputStart;
        vimBuffer.Closed += OnBufferClosed;

        autoHides = findAllReferencesWindow.AutoHides;

        findAllReferencesWindow.AutoHides = false;
        findAllReferencesWindow.Activate();

        dte.ActiveDocument.Activate();
    }

    public void OnKeyInputStart(object sender, KeyInputStartEventArgs e)
    {
        e.Handled = true;

        int index;
        if (e.KeyInput.Char == 'j')
        {
            int count = findAllReferencesWindow.GetItemsCount();
            if (0 < count)
            {
                index = findAllReferencesWindow.GetSelectedIndex();
                if (index < count)
                {
                    index++;
                    findAllReferencesWindow.SetSelectedIndex(index);
                }
            }
        }
        else if (e.KeyInput.Char == 'k')
        {
            if (0 < findAllReferencesWindow.GetItemsCount())
            {
                index = findAllReferencesWindow.GetSelectedIndex();
                if (0 < index)
                {
                    index--;
                    findAllReferencesWindow.SetSelectedIndex(index);
                }
            }
        }
        else if (e.KeyInput.Key == VimKey.Enter)
        {
            EndIntercept();
            //findAllReferencesWindow.Activate();
            findAllReferencesWindow.NavigateToSelectedEntry();
            dte.ActiveDocument.Activate();
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
        findAllReferencesWindow.AutoHides = autoHides;
    }
}
