#load "util.csx"
#load "VsVimFindResultsWindow.csx"

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using Vim;
using Vim.Extensions;

var findResultItems = new ObservableCollection<IFindResultItem>();
MarkItem mi;
string format;
var marks = Vim.MarkMap;
foreach (var letter in Letter.All)
{
    var markInfo = marks.GetMarkInfo(Mark.NewGlobalMark(letter), VimBuffer.VimBufferData);
    if (markInfo.IsNone())
        continue;

    var miValue = markInfo.Value;
    format = $" {miValue.Ident}  {(miValue.Line + 1),5}{miValue.Column,5} {Path.GetFileNameWithoutExtension(miValue.Name)}";
    mi = new MarkItem();

    mi.Name = format;
    mi.NavigateItem = miValue.Ident;
    findResultItems.Add(mi);
}

if (findResultItems.Count == 0)
{
    VimBuffer.DisplayError("There was no global marks");
    return;
}


VimBuffer.KeyInputStart += OnKeyInputStart;
VimBuffer.Closed += OnBufferClosed;

var toolWindow = new VsVimFindResultsWindow();

toolWindow.SetFindResultItems(findResultItems);
toolWindow.Show();

var DTE = Util.GetDTE2();
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
            var mark = (char)t.InvokeMember("NavigateItem",
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty,
                    null,
                    selectedItem,
                    null);

            VimBuffer.SwitchMode(ModeKind.Normal, ModeArgument.None);
            VimBuffer.KeyInputStart -= OnKeyInputStart;
            VimBuffer.Process($"`{mark}", enter: false);
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
    VimBuffer.KeyInputStart -= OnKeyInputStart;
    VimBuffer.Closed -= OnBufferClosed;
    VimBuffer.DisplayStatus(string.Empty);
    toolWindow.ClearItems();
    toolWindow.CloseFrame();
}
public class MarkItem : IFindResultItem
{
    public string Name { get; set; } = string.Empty;
    public object NavigateItem { get; set; } = null;
}

