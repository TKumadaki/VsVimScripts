#load "Util.csx"

using EnvDTE80;
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

public class FindAllReferencesWindow
{
    //This guid is not documented
    private const string guidFindAllReferencesWindow = "{A80FEBB4-E7E0-4147-B476-21AAF2453969}";

    private IVimBuffer vimBuffer;
    private Window findAllReferencesWindow;
    private bool autoHides = true;
    private IVim vim;
    private DTE2 dte;

    public FindAllReferencesWindow(IVim vim)
    {
        this.vim = vim;
        this.dte = Util.GetDTE2();
    }

    public void Display()
    {
        if (!vim.TryGetActiveVimBuffer(out vimBuffer))
        {
            vim.DisplayError("Can not get VimBuffer");
            return;
        }

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

        if (e.KeyInput.Char == 'j')
        {
            var args = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Down)
            {
                RoutedEvent = Keyboard.KeyDownEvent
            };
            findAllReferencesWindow.Activate();
            InputManager.Current.ProcessInput(args);
            dte.ActiveDocument.Activate();
        }
        else if (e.KeyInput.Char == 'k')
        {
            var args = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Up)
            {
                RoutedEvent = Keyboard.KeyDownEvent
            };
            findAllReferencesWindow.Activate();
            InputManager.Current.ProcessInput(args);
            dte.ActiveDocument.Activate();
        }
        else if (e.KeyInput.Key == VimKey.Enter)
        {
            EndIntercept();
            findAllReferencesWindow.Activate();
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
        findAllReferencesWindow.AutoHides = autoHides;
    }
}
