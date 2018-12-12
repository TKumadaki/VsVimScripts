#load "util.csx"

using EnvDTE;
using Microsoft.VisualStudio.Text;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Vim;
using Vim.Extensions;
using System.Windows.Input;
using System.Reflection;

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

var taskList = taskListWindow.Object as TaskList;
if (taskList != null && 0 < taskList.TaskItems.Count && GetSelectedIndex() == -1)
{
    taskList.TaskItems.Item(1).Select();
}
DTE.ActiveDocument.Activate();

public void OnKeyInputStart(object sender, KeyInputStartEventArgs e)
{
    e.Handled = true;

    var taskList = taskListWindow.Object as TaskList;
    if (e.KeyInput.Char == 'j')
    {
        if (taskList != null && 0 < taskList.TaskItems.Count && GetSelectedIndex() < taskList.TaskItems.Count)
        {
            var args = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Down)
            {
                RoutedEvent = Keyboard.KeyDownEvent
            };
            taskListWindow.Activate();
            InputManager.Current.ProcessInput(args);
            DTE.ActiveDocument.Activate();
        }
    }
    else if (e.KeyInput.Char == 'k')
    {
        if (taskList != null && 0 < taskList.TaskItems.Count && 0 < GetSelectedIndex())
        {
            var args = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Up)
            {
                RoutedEvent = Keyboard.KeyDownEvent
            };
            taskListWindow.Activate();
            InputManager.Current.ProcessInput(args);
            DTE.ActiveDocument.Activate();
        }
    }
    else if (e.KeyInput.Key == VimKey.Enter)
    {
        EndIntercept();
        taskListWindow.Activate();
    }
    else if (e.KeyInput.Key == VimKey.Escape)
    {
        EndIntercept();
    }
    //messageAction?.Invoke();
}
public int GetSelectedIndex()
{
    int index = -1;
    Type t;
    var taskList = taskListWindow.Object as TaskList;
    if (taskList == null)
    {
        return index;
    }
    t = taskList.GetType();
    var tableControl = t.InvokeMember("TableControl",
                BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                null,
                taskList,
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
