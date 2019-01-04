#load "util.csx"

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
public static int GetSelectedIndex(this Window window)
{
    int index = -1;
    Type t;
    var toolWindow = window.Object;
    if (toolWindow == null)
    {
        return index;
    }
    t = toolWindow.GetType();
    var tableControl = t.InvokeMember("TableControl",
                BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                null,
                toolWindow,
                null);
    if (tableControl == null)
    {
        return index;
    }
    t = tableControl.GetType();
    var controlViewModel = t.InvokeMember("ControlViewModel",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField,
                null,
                tableControl,
                null);
    if (controlViewModel == null)
    {
        return index;
    }
    t = controlViewModel.GetType();
    index = (int)t.InvokeMember("SelectedIndex",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty,
                null,
                controlViewModel,
                null);

    //Vim.DisplayStatus(index.ToString());
    return index;
}
public static void SetSelectedIndex(this Window window, int index)
{
    Type t;
    var windowPane = window.Object;
    if (windowPane == null)
    {
        return;
    }
    t = windowPane.GetType();
    var tableControl = t.InvokeMember("TableControl",
                BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                null,
                windowPane,
                null);
    if (tableControl == null)
    {
        return;
    }
    t = tableControl.GetType();
    var controlViewModel = t.InvokeMember("ControlViewModel",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField,
                null,
                tableControl,
                null);
    if (controlViewModel == null)
    {
        return;
    }
    t = controlViewModel.GetType();
    t.InvokeMember("SelectedIndex",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty,
                null,
                controlViewModel,
                new object[] { index });

    t = tableControl.GetType();
    var wrapListView = t.InvokeMember("Control",
    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty,
    null,
    tableControl,
    null);

    t = wrapListView.GetType();
    t.InvokeMember("TryScrollSelectedItemIntoViewport",
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod,
            null,
            wrapListView,
            new object[] { 0 });
}
public static int GetItemsCount(this Window window)
{
    Type t;
    var windowPane = window.Object;
    if (windowPane == null)
    {
        return 0;
    }
    t = windowPane.GetType();
    var tableControl = t.InvokeMember("TableControl",
                BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                null,
                windowPane,
                null);
    if (tableControl == null)
    {
        return 0;
    }
    t = tableControl.GetType();
    var wrapListView = t.InvokeMember("Control",
    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty,
    null,
    tableControl,
    null);

    t = wrapListView.GetType();
    var tableControlView = t.InvokeMember("TableControlView",
    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField,
    null,
    wrapListView,
    null);

    t = tableControlView.GetType();
    var items = t.InvokeMember("Items",
    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty,
    null,
    tableControlView,
    null);

    t = items.GetType();
    int count = (int)t.InvokeMember("Count",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty,
                null,
                items,
                null);

    return count;
}
public static void NavigateToSelectedEntry(this Window window)
{
    Type t;
    var windowPane = window.Object;
    if (windowPane == null)
    {
        return;
    }
    t = windowPane.GetType();
    var tableControl = t.InvokeMember("TableControl",
                BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                null,
                windowPane,
                null);
    if (tableControl == null)
    {
        return;
    }
    t = tableControl.GetType();
    var controlViewModel = t.InvokeMember("ControlViewModel",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField,
                null,
                tableControl,
                null);
    if (controlViewModel == null)
    {
        return;
    }
    t = controlViewModel.GetType();
    t.InvokeMember("NavigateToSelectedEntry",
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod,
            null,
            controlViewModel,
            null);
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
