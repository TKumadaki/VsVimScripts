#load "util.csx"
#load "WindowUtil.csx"

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


const string vsWindowKindTaskList = "{4A9B7E51-AA16-11D0-A8C5-00A0C921A4D2}";

IVimBuffer vimBuffer;

if (!Vim.TryGetActiveVimBuffer(out vimBuffer))
{
    Vim.DisplayError("Can not get VimBuffer");
    return;
}

var DTE = Util.GetDTE2();

Window taskListWindow = DTE.Windows.Item(vsWindowKindTaskList);
bool autoHides = true;

vimBuffer.KeyInputStart += OnKeyInputStart;
vimBuffer.Closed += OnBufferClosed;

autoHides = taskListWindow.AutoHides;

taskListWindow.AutoHides = false;
taskListWindow.Activate();

if (0 < taskListWindow.GetItemsCount())
{
    int index = taskListWindow.GetSelectedIndex();
    index = Math.Max(index, 0);
    taskListWindow.SetSelectedIndex(index);
}
DTE.ActiveDocument.Activate();

public void OnKeyInputStart(object sender, KeyInputStartEventArgs e)
{
    e.Handled = true;

    int index;
    if (e.KeyInput.Char == 'j')
    {
        int count = taskListWindow.GetItemsCount();
        if (0 < count)
        {
            index = taskListWindow.GetSelectedIndex();
            if (index < count)
            {
                index++;
                taskListWindow.SetSelectedIndex(index);
            }
        }
    }
    else if (e.KeyInput.Char == 'k')
    {
        if (0 < taskListWindow.GetItemsCount())
        {
            index = taskListWindow.GetSelectedIndex();
            if (0 < index)
            {
                index--;
                taskListWindow.SetSelectedIndex(index);
            }
        }
    }
    else if (e.KeyInput.Key == VimKey.Enter)
    {
        EndIntercept();
        //taskListWindow.Activate();
        taskListWindow.NavigateToSelectedEntry();
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
    Vim.DisplayStatus(string.Empty);
    taskListWindow.AutoHides = autoHides;
}
