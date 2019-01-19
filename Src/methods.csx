#load "util.csx"
#load "VsVimFindResultsWindow.csx"

using EnvDTE;
using System;
using System.Collections.ObjectModel;
using System.Reflection;
using Vim;

IVimBuffer vimBuffer;

if (!Vim.TryGetActiveVimBuffer(out vimBuffer))
{
    Vim.DisplayError("Can not get VimBuffer");
    return;
}

var DTE = Util.GetDTE2();

FileCodeModel fcm = DTE.ActiveDocument.ProjectItem.FileCodeModel as FileCodeModel;
if (fcm == null)
{
    Vim.DisplayError("Can not get FileCodeModel");
    return;
}

var findResultItems = new ObservableCollection<IFindResultItem>();
MethodItem mi;

try
{
    foreach (CodeElement element in fcm.CodeElements)
    {
        MakeFindResultItem(ref findResultItems, element);
    }
}
catch (Exception ex)
{
    Vim.DisplayError(ex.Message);
    return;
}

if (findResultItems.Count == 0)
{
    Vim.DisplayError("There was no item");
    return;
}

vimBuffer.KeyInputStart += OnKeyInputStart;
vimBuffer.Closed += OnBufferClosed;

var toolWindow = new VsVimFindResultsWindow();

toolWindow.SetFindResultItems(findResultItems);
toolWindow.Show();
DTE.ActiveDocument.Activate();

public void MakeFindResultItem(ref ObservableCollection<IFindResultItem> items, CodeElement element)
{
    try
    {
        if (element is CodeFunction || element is CodeProperty)
        {
            var t = element.GetType();
            mi = new MethodItem();
            mi.Name = (string)t.InvokeMember("Name",
                                            BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty,
                                            null,
                                            element,
                                            null);
            var tp = (TextPoint)t.InvokeMember("StartPoint",
                                            BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty,
                                            null,
                                            element,
                                            null);
            mi.NavigateItem = tp.Line;
            items.Add(mi);
            return;
        }
        foreach (var child in element.Children)
        {
            if (child is CodeElement)
            {
                MakeFindResultItem(ref items, (CodeElement)child);
            }
        }
    }
    catch (Exception ex)
    {
        Vim.DisplayError(ex.Message);
    }
}
public void OnKeyInputStart(object sender, KeyInputStartEventArgs e)
{
    e.Handled = true;

    if (e.KeyInput.Char == 'j')
    {
        toolWindow.MoveNextItem();
    }
    else if (e.KeyInput.Char == 'k')
    {
        toolWindow.MovePreviousItem();
    }
    else if (e.KeyInput.Key == VimKey.Enter)
    {
        var selectedItem = toolWindow.GetSelectedItem();
        if (selectedItem != null)
        {
            var t = selectedItem.GetType();
            var lineNumber = (int)t.InvokeMember("NavigateItem",
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty,
                    null,
                    selectedItem,
                    null);
            vimBuffer.SwitchMode(ModeKind.Normal, ModeArgument.None);
            vimBuffer.Process(lineNumber.ToString() + "G", enter: false);
        }
        EndIntercept();
    }
    else if (e.KeyInput.Key == VimKey.Escape)
    {
        EndIntercept();
    }
}
public void OnBufferClosed(object sender, EventArgs e)
{
    EndIntercept();
}
public void EndIntercept()
{
    vimBuffer.KeyInputStart -= OnKeyInputStart;
    vimBuffer.Closed -= OnBufferClosed;
    Vim.DisplayStatus(string.Empty);
    toolWindow.ClearItems();
    toolWindow.CloseFrame();
}

public class MethodItem : IFindResultItem
{
    public string Name { get; set; } = string.Empty;
    public object NavigateItem { get; set; } = null;
}

