﻿#load "util.csx"

using EnvDTE;
using Microsoft.VisualStudio.Text;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Vim;
using Vim.Extensions;

const string vsWindowKindFindResults1 = "{0F887920-C2B6-11D2-9375-0080C747D9A0}";

IVimBuffer vimBuffer;

if (!TryGetActiveVimBuffer(out vimBuffer))
{
    DisplayError("Can not get VimBuffer");
    return;
}

var DTE = GetDTE2();

Window findResultsWindow = DTE.Windows.Item(vsWindowKindFindResults1);

bool autoHides = true;

//Action messageAction = null;

vimBuffer.KeyInputStart += OnKeyInputStart;
vimBuffer.Closed += OnBufferClosed;

autoHides = findResultsWindow.AutoHides;

findResultsWindow.AutoHides = false;
findResultsWindow.Activate();
DTE.ActiveDocument.Activate();

public void OnKeyInputStart(object sender, KeyInputStartEventArgs e)
{
    e.Handled = true;
    EnvDTE.TextSelection selection;
    int moveLine;

    if (e.KeyInput.Char == 'j')
    {
        selection = findResultsWindow.Selection as EnvDTE.TextSelection;
        if (selection != null)
        {
            selection.GotoLine(selection.CurrentLine + 1, true);
        }
    }
    else if (e.KeyInput.Char == 'k')
    {
        selection = findResultsWindow.Selection as EnvDTE.TextSelection;
        if (selection != null)
        {
            selection.GotoLine(selection.CurrentLine - 1, true);
        }
    }
    else if (e.KeyInput.Key == VimKey.Enter)
    {
        InterceptEnd();
        findResultsWindow.Activate();
    }
    else if (e.KeyInput.Key == VimKey.Escape)
    {
        InterceptEnd();
    }
    //messageAction?.Invoke();
}
public void OnBufferClosed(object sender, EventArgs e)
{
    InterceptEnd();
}
public void InterceptEnd()
{
    vimBuffer.KeyInputStart -= OnKeyInputStart;
    vimBuffer.Closed -= OnBufferClosed;
    //messageAction = null;
    DisplayStatus(string.Empty);
    findResultsWindow.AutoHides = autoHides;
}