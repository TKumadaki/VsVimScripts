#load "util.csx"

using EnvDTE;
using Microsoft.VisualStudio.Text;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Vim;

const string PhysicalFile = "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}";   //GUID_ItemType_PhysicalFile
const string SolutionItem = "{66A26722-8FB5-11D2-AA7E-00C04F688DDE}";   //VsProjectItemKindSolutionItem
const string VirtualFolder = "{6BB5F8F0-4483-11D3-8BCF-00C04F8EC28C}";  //GUID_ItemType_VirtualFolder

IVimBuffer vimBuffer;

if (!TryGetActiveVimBuffer(out vimBuffer))
{
    DisplayError("Can not get VimBuffer");
    return;
}

var DTE = GetDTE2();

vimBuffer.KeyInputStart += OnKeyInputStart;
vimBuffer.Closed += OnBufferClosed;

//not work
//Vim.VimHost.RunHostCommand(vimBuffer.TextView, "View.SolutionExplorer", string.Empty);
//DTE.ExecuteCommand("View.SolutionExplorer");
//DTE.ActiveDocument.Activate();

UIHierarchy solutionExplorer = DTE.ToolWindows.SolutionExplorer;
Action messageAction = null;
Action<object, KeyInputStartEventArgs> modeAction = null;
string buffer = string.Empty;
string lastSearchKeyword = string.Empty;
bool lastSearchForward = true;
bool currentSearchForward = true;

UIHierarchyItem item = GetUIHierarchyItem();
item.Select(vsUISelectionType.vsUISelectionTypeSelect);

SwitchNormalMode(swichMessage: true);

private void SwitchNormalMode(bool swichMessage)
{
    if (swichMessage)
    {
        messageAction = () => DisplayStatus("solution explorer operation mode");
    }
    messageAction?.Invoke();
    modeAction = NormalMode;
}
private void NormalMode(object sender, KeyInputStartEventArgs e)
{
    if (e.KeyInput.Char == 'a')
    {
        InterceptEnd();
        DTE.ExecuteCommand("View.SolutionExplorer");
        DTE.ExecuteCommand("Project.AddNewItem");
    }
    else if (e.KeyInput.Char == 'c')
    {
        CollapseProject(solutionExplorer, expanded: false);
    }
    else if (e.KeyInput.Char == 'C')
    {
        CollapseProject(solutionExplorer, expanded: true);
    }
    else if (e.KeyInput.Char == 'd')
    {
        ProjectItem pi = GetProjectItem();
        SwitchDeleteMode(pi?.Name);
    }
    else if (e.KeyInput.Char == 'f')
    {
        SwitchSearchMode(forward: true);
    }
    else if (e.KeyInput.Char == 'F')
    {
        SwitchSearchMode(forward: false);
    }
    else if (e.KeyInput.Char == '/')
    {
        SwitchIncrementalSearchMode(forward: true);
    }
    else if (e.KeyInput.Char == '?')
    {
        SwitchIncrementalSearchMode(forward: false);
    }
    else if (e.KeyInput.Char == 'j')
    {
        solutionExplorer.SelectDown(vsUISelectionType.vsUISelectionTypeSelect, 1);
    }
    else if (e.KeyInput.Char == 'k')
    {
        solutionExplorer.SelectUp(vsUISelectionType.vsUISelectionTypeSelect, 1);
    }
    else if (e.KeyInput.Char == 'h')
    {
        UIHierarchyItem item = GetUIHierarchyItem();
        UIHierarchyItem parent = item?.Collection.Parent as UIHierarchyItem;

        if (parent == null)
            return;

        parent.Select(vsUISelectionType.vsUISelectionTypeSelect);
        parent.UIHierarchyItems.Expanded = false;
    }
    else if (e.KeyInput.Char == 'l')
    {
        UIHierarchyItem item = GetUIHierarchyItem();
        if (0 < item.UIHierarchyItems.CountEx())
        {
            item.UIHierarchyItems.Expanded = true;
        }
    }
    else if (e.KeyInput.Char == 'n')
    {
        Search(lastSearchKeyword, solutionExplorer, startItem: GetUIHierarchyItem(), forward: lastSearchForward, switchNormalMode:false);
    }
    else if (e.KeyInput.Char == 'N')
    {
        Search(lastSearchKeyword, solutionExplorer, startItem: GetUIHierarchyItem(), forward: !lastSearchForward, switchNormalMode:false);
    }
    else if (e.KeyInput.Char == 'r')
    {
        InterceptEnd();
        DTE.ExecuteCommand("View.SolutionExplorer");
        DTE.ExecuteCommand("File.Rename");
    }
    else if (e.KeyInput.Key == VimKey.Enter)
    {
        UIHierarchyItem item = GetUIHierarchyItem();
        ProjectItem pi = GetProjectItem();
        if (pi != null && (pi.Kind == PhysicalFile || pi.Kind == SolutionItem))
        {
            InterceptEnd();
        }
        solutionExplorer.DoDefaultAction();
    }
    else if (e.KeyInput.Key == VimKey.Escape)
    {
        SwitchExitMode();
    }
}
private void SwitchDeleteMode(string fileName)
{
    messageAction = () => DisplayStatus($"delete {fileName} (y/n)?");
    messageAction.Invoke();
    modeAction = DeleteMode;
}
private void DeleteMode(object sender, KeyInputStartEventArgs e)
{
    if (e.KeyInput.Char == 'y')
    {
        ProjectItem pi = GetProjectItem();
        //pi?.Remove(); //Removes the project item from the collection.
        pi?.Delete(); //Removes the item from its project and its storage.
    }

    SwitchNormalMode(swichMessage: true);
}
private void SwitchExitMode()
{
    messageAction = () => DisplayStatus($"Do you want to exit? (y/n)?");
    messageAction.Invoke();
    modeAction = ExitMode;
}
private void ExitMode(object sender, KeyInputStartEventArgs e)
{
    if (e.KeyInput.Char == 'y')
    {
        InterceptEnd();
        return;
    }

    SwitchNormalMode(swichMessage: true);
}
private void SwitchSearchMode(bool forward)
{
    buffer = string.Empty;
    messageAction = () => DisplayStatus($":{(forward ? "/" : "?")}{buffer}");
    messageAction.Invoke();

    currentSearchForward = forward;
    modeAction = SearchMode;
}
private void SearchMode(object sender, KeyInputStartEventArgs e)
{
    if (e.KeyInput.Key == VimKey.Escape)
    {
        SwitchNormalMode(swichMessage: true);
    }
    else if (e.KeyInput.Key == VimKey.Enter)
    {
        Search(buffer, solutionExplorer, startItem: GetUIHierarchyItem(), forward: currentSearchForward, switchNormalMode:true);
    }
    else if (e.KeyInput.Key == VimKey.Back)
    {
        if (1 < buffer.Length)
        {
            buffer = buffer.Substring(0, buffer.Length - 1);
        }
        else if (buffer.Length == 1)
        {
            buffer = string.Empty;
        }
    }
    else
    {
        buffer += e.KeyInput.Char;
    }
}
private void SwitchIncrementalSearchMode(bool forward)
{
    buffer = string.Empty;
    messageAction = () => DisplayStatus($"{(forward ? "/" : "?")}{buffer}");
    messageAction.Invoke();

    currentSearchForward = forward;
    modeAction = IncrementalSearchMode;
}
private void IncrementalSearchMode(object sender, KeyInputStartEventArgs e)
{
    if (e.KeyInput.Key == VimKey.Escape)
    {
        SwitchNormalMode(swichMessage: true);
        return;
    }

    if (e.KeyInput.Key == VimKey.Enter)
    {
        Search(buffer, solutionExplorer, startItem: GetUIHierarchyItem(), forward: currentSearchForward, switchNormalMode:true);
        return;
    }

    if (e.KeyInput.Key == VimKey.Back)
    {
        if (1 < buffer.Length)
        {
            buffer = buffer.Substring(0, buffer.Length - 1);
        }
        else if (buffer.Length == 1)
        {
            buffer = string.Empty;
        }
    }
    else
    {
        buffer += e.KeyInput.Char;
    }
    Search(buffer, solutionExplorer, startItem: GetUIHierarchyItem(), forward: currentSearchForward, switchNormalMode:false);
}
private void Search(string keyword, UIHierarchy solutionExplorer, object startItem, bool forward, bool switchNormalMode)
{
    if (string.IsNullOrWhiteSpace(keyword))
    {
        messageAction = () => DisplayStatus("nothing keyword.");
        SwitchNormalMode(swichMessage: false);
        return;
    }

    if (solutionExplorer.UIHierarchyItems.CountEx() == 0)
    {
        messageAction = () => DisplayStatus("solution item 0.");
        SwitchNormalMode(swichMessage: false);
        return;
    }

    var searchList = new List<SearchItem>();
    UIHierarchyItem rootNode = solutionExplorer.UIHierarchyItems.Item(1);

    GetSearchList(ref searchList, solutionExplorer, rootNode);

    if (searchList.Count == 0)
    {
        messageAction = () => DisplayStatus("search item 0.");
        SwitchNormalMode(swichMessage: false);
        return;
    }

    keyword = keyword.ToLower();

    if (!forward)
        searchList.Reverse();

    int startIndex = -1;
    if (startItem != null)
    {
        startIndex = searchList.FindIndex((x) => object.ReferenceEquals(x.Item, startItem));
    }
    startIndex++;

    int index = -1;
    for (int i = startIndex; i < searchList.Count(); i++)
    {
        if (0 <= searchList[i].Name.IndexOf(keyword))
        {
            index = i;
            break;
        }
    }
    if (index < 0)
    {
        for (int i = 0; i < startIndex; i++)
        {
            if (searchList[i].Name.IndexOf(keyword) == 0)
            {
                index = i;
                break;
            }
        }
    }

    if (index < 0)
    {
        messageAction = () => DisplayStatus("not found.");
    }
    else
    {
        searchList[index].Item.Select(vsUISelectionType.vsUISelectionTypeSelect);
        lastSearchKeyword = keyword;
        lastSearchForward = currentSearchForward;
    }

    if (!switchNormalMode)
        return;

    SwitchNormalMode(swichMessage: 0 <= index);
}
private void OnKeyInputStart(object sender, KeyInputStartEventArgs e)
{
    e.Handled = true;

    modeAction?.Invoke(sender, e);
    messageAction?.Invoke();
}
private void OnBufferClosed(object sender, EventArgs e)
{
    InterceptEnd();
}
private void InterceptEnd()
{
    vimBuffer.KeyInputStart -= OnKeyInputStart;
    vimBuffer.Closed -= OnBufferClosed;
    messageAction = null;
    modeAction = null;
    DisplayStatus(string.Empty);
}
private void CollapseProject(UIHierarchy solutionExplorer, bool expanded)
{
    if (solutionExplorer.UIHierarchyItems.CountEx() == 0)
    {
        return;
    }
    UIHierarchyItem rootNode = solutionExplorer.UIHierarchyItems.Item(1);

    CollapseProject(solutionExplorer, rootNode, expanded);

    rootNode.Select(vsUISelectionType.vsUISelectionTypeSelect);
}
private void CollapseProject(UIHierarchy solutionExplorer, UIHierarchyItem item, bool expanded)
{
    foreach (UIHierarchyItem innerItem in item.UIHierarchyItems)
    {
        ProjectItem pi = innerItem.Object as ProjectItem;
        if (pi != null && (pi.Kind == VirtualFolder || pi.Kind == PhysicalFile || pi.Kind == SolutionItem))
        {
            continue;
        }

        if (0 < innerItem.UIHierarchyItems.CountEx())
        {
            CollapseProject(solutionExplorer, innerItem, expanded);

            innerItem.UIHierarchyItems.Expanded = expanded;
        }
    }
}
private UIHierarchyItem GetUIHierarchyItem()
{
    var selectedItems = solutionExplorer.SelectedItems as UIHierarchyItem[];
    return selectedItems?.FirstOrDefault();
}
private ProjectItem GetProjectItem()
{
    return GetUIHierarchyItem()?.Object as ProjectItem;
}
private class SearchItem
{
    public string Name { get; set; } = string.Empty;
    public UIHierarchyItem Item { get; set; } = null;
}
private void GetSearchList(ref List<SearchItem> searchList, UIHierarchy solutionExplorer, UIHierarchyItem item)
{
    foreach (UIHierarchyItem innerItem in item.UIHierarchyItems)
    {
        ProjectItem pi = innerItem.Object as ProjectItem;
        if (pi != null && pi.Kind == VirtualFolder)
        {
            continue;
        }

        searchList.Add(new SearchItem() { Name = innerItem.Name.ToLower(), Item = innerItem });

        if (pi != null && (pi.Kind == PhysicalFile || pi.Kind == SolutionItem))
        {
            continue;
        }

        if (0 < innerItem.UIHierarchyItems.CountEx())
        {
            GetSearchList(ref searchList, solutionExplorer, innerItem);
        }
    }
}
private static int CountEx(this UIHierarchyItems items)
{
    //UIHierarchyItems can not get count unless expanded.
    if (items.Expanded == false)
    {
        items.Expanded = true;
        items.Expanded = false;
    }
    return items.Count;
}
