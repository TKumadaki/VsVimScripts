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
    return;

var findResultItems = new ObservableCollection<IFindResultItem>();
MethodItem mi;

foreach (CodeElement element in fcm.CodeElements)
{
    if (element is CodeNamespace)
    {
        CodeNamespace nsp = element as CodeNamespace;

        foreach (CodeElement subElement in nsp.Children)
        {
            if (subElement is CodeClass)
            {
                CodeClass c2 = subElement as CodeClass;
                foreach (CodeElement item in c2.Children)
                {
                    if (item is CodeFunction)
                    {
                        CodeFunction cf = item as CodeFunction;
                        mi = new MethodItem();
                        mi.Name = cf.Name;
                        mi.NavigateItem = cf.StartPoint.Line;
                        findResultItems.Add(mi);
                    }
                }
            }
        }
    }
}

if (findResultItems.Count == 0)
    return;

vimBuffer.KeyInputStart += OnKeyInputStart;
vimBuffer.Closed += OnBufferClosed;

var toolWindow = new VsVimFindResultsWindow();

toolWindow.SetFindResultItems(findResultItems);
toolWindow.Show();
DTE.ActiveDocument.Activate();

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

