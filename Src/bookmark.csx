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

const string guidBookmarkWindow = "{A0C5197D-0AC7-4B63-97CD-8872A789D233}";

IVimBuffer vimBuffer;

if (!TryGetActiveVimBuffer(out vimBuffer))
{
    DisplayError("Can not get VimBuffer");
    return;
}

var DTE = GetDTE2();

Window bookmarkWindow = DTE.Windows.Item(guidBookmarkWindow);
bool autoHides = true;
int selectedIndex = 1;

//Action messageAction = null;

vimBuffer.KeyInputStart += OnKeyInputStart;
vimBuffer.Closed += OnBufferClosed;

autoHides = bookmarkWindow.AutoHides;

bookmarkWindow.AutoHides = false;
bookmarkWindow.Activate();

DTE.ActiveDocument.Activate();

public void OnKeyInputStart(object sender, KeyInputStartEventArgs e)
{
    e.Handled = true;
    //EnvDTE.TextSelection selection;

    if (e.KeyInput.Char == 'j')
    {
        //not work
        //selection = bookmarkWindow.Selection as EnvDTE.TextSelection;
        //if (selection != null)
        //{
        //    selection.GotoLine(selection.CurrentLine + 1, true);
        //}

        //not work
        //var args = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Down)
        //{
        //    RoutedEvent = Keyboard.KeyDownEvent
        //};
        //bookmarkWindow.Activate();
        //InputManager.Current.ProcessInput(args);
        //DTE.ActiveDocument.Activate();
    }
    else if (e.KeyInput.Char == 'k')
    {
        //not work
        //selection = bookmarkWindow.Selection as EnvDTE.TextSelection;
        //if (selection != null)
        //{
        //    selection.GotoLine(selection.CurrentLine - 1, true);
        //}

        //not work
        //var args = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Up)
        //{
        //    RoutedEvent = Keyboard.KeyDownEvent
        //};
        //bookmarkWindow.Activate();
        //InputManager.Current.ProcessInput(args);
        //DTE.ActiveDocument.Activate();
    }
    else if (e.KeyInput.Key == VimKey.Enter)
    {
        InterceptEnd();
        bookmarkWindow.Activate();
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
    bookmarkWindow.AutoHides = autoHides;
}
