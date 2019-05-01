#load "util.csx"
#load "VsVimFindResultsWindow.csx"

using EnvDTE;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using Vim;

Type t = VimBuffer.Vim.VimHost.GetType();
var fieldInfo = t.GetField("_sharedService", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance);
var sharedService = fieldInfo.GetValue(VimBuffer.Vim.VimHost);
t = sharedService.GetType();
MethodInfo GetActiveViews = t.GetMethod("GetActiveViews", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod);
MethodInfo GoToTab = t.GetMethod("GoToTab", BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Instance);

IList views = (IList)GetActiveViews.Invoke(null, null);
if (views.Count == 0)
{
    VimBuffer.DisplayError("There was no tabs");
    return;
}

t = null;
TabItem ti;
var findResultItems = new ObservableCollection<IFindResultItem>();
for (var i = 0; i < views.Count; i++)
{
    var view = views[i];
    if (t == null)
    {
        t = view.GetType();
    }
    ti = new TabItem();
    var title = t.InvokeMember("Title",
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty,
            null,
            view,
            null);

    ti.Name = title.ToString();
    ti.NavigateItem = i;
    findResultItems.Add(ti);
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
            var index = (int)t.InvokeMember("NavigateItem",
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty,
                    null,
                    selectedItem,
                    null);
            GoToTab.Invoke(sharedService, new object[] { index });
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

public class TabItem : IFindResultItem
{
    public string Name { get; set; } = string.Empty;
    public object NavigateItem { get; set; } = null;
}

