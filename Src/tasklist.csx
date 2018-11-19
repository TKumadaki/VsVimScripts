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

const string vsWindowKindTaskList = "{4A9B7E51-AA16-11D0-A8C5-00A0C921A4D2}";

IVimBuffer vimBuffer;

if (!Vim.TryGetActiveVimBuffer(out vimBuffer))
{
    Vim.DisplayError("Can not get VimBuffer");
    return;
}

var DTE = Util.GetDTE2();

Window taskListWindow = DTE.Windows.Item(vsWindowKindTaskList);
TaskList taskList = taskListWindow.Object as TaskList;
bool autoHides = true;
int selectedIndex = 1;

//Action messageAction = null;

vimBuffer.KeyInputStart += OnKeyInputStart;
vimBuffer.Closed += OnBufferClosed;

autoHides = taskListWindow.AutoHides;

taskListWindow.AutoHides = false;
taskListWindow.Activate();

if (taskList != null && 0 < taskList.TaskItems.Count)
{
    taskList.TaskItems.Item(selectedIndex).Select();
}
DTE.ActiveDocument.Activate();

public void OnKeyInputStart(object sender, KeyInputStartEventArgs e)
{
    e.Handled = true;

    if (e.KeyInput.Char == 'j')
    {
        //if (taskList != null && selectedIndex < taskList.TaskItems.Count)
        //{
        //    selectedIndex++;
        //    taskList.TaskItems.Item(selectedIndex).Select();
        //}
        if (taskList != null && selectedIndex < taskList.TaskItems.Count)
        {
            selectedIndex++;
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
        //if (taskList != null && 1 < selectedIndex)
        //{
        //    selectedIndex--;
        //    taskList.TaskItems.Item(selectedIndex).Select();
        //}

        if (taskList != null && 1 < selectedIndex)
        {
            selectedIndex--;
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
        InterceptEnd();
        taskListWindow.Activate();
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
    Vim.DisplayStatus(string.Empty);
    taskListWindow.AutoHides = autoHides;
}
