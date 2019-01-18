#load "util.csx"

#r "Microsoft.VisualStudio.Shell.Interop.dll"
#r "Microsoft.VisualStudio.OLE.Interop.dll"

using Microsoft.CSharp;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Collections.ObjectModel;

public class VsVimFindResultsWindow
{
    public const string ToolWindowGuid = "5433925F-6DF5-4A5B-B51F-0A20EBA30481";
    private IVsWindowFrame windowFrame = null;
    private ListBox listBox = null;
    private object docView = null;
    private MethodInfo setFindResultItems = null;
    private MethodInfo clearItems = null;

    public VsVimFindResultsWindow()
    {

        Guid guidEmpty = Guid.Empty;
        Guid toolWindowGuid;
        int result;

        var shell = (IVsUIShell)Package.GetGlobalService(typeof(SVsUIShell));

        toolWindowGuid = new Guid(VsVimFindResultsWindow.ToolWindowGuid);

        shell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fFindFirst, ref toolWindowGuid, out windowFrame);
        if (windowFrame == null)
        {
            VsVimFindResultsWindowControl control = new VsVimFindResultsWindowControl();
            result = shell.CreateToolWindow((uint)__VSCREATETOOLWIN.CTW_fInitNew,
                0, control,
                ref guidEmpty, ref toolWindowGuid, ref guidEmpty, null, "VsVimFindResultsWindow", null, out windowFrame);

            ErrorHandler.ThrowOnFailure(result);
            windowFrame.SetProperty((int)__VSFPROPID.VSFPROPID_FrameMode, VSFRAMEMODE.VSFM_Dock);
        }
        windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out docView);
        var t = docView.GetType();
        setFindResultItems = t.GetMethod("SetFindResultItems");
        clearItems = t.GetMethod("ClearItems");
    }
    public void Show()
    {
        windowFrame.Show();

        //ListBox will be null if you do not do it here
        var t = docView.GetType();
        listBox = (ListBox)t.InvokeMember("FindControl",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod,
                null,
                docView,
                new object[] { "FindResultsListBox" });
    }
    public void CloseFrame()
    {
        windowFrame.CloseFrame((uint)__FRAMECLOSE.FRAMECLOSE_NoSave);
    }
    public void MoveNextItem()
    {
        if (listBox.Items.Count == 0)
            return;

        int index = listBox.SelectedIndex;

        if (listBox.Items.Count <= index + 1)
            return;

        listBox.SelectedIndex = index + 1;
        listBox.ScrollIntoView(listBox.Items[index + 1]);

    }
    public void MovePreviousItem()
    {
        if (listBox.Items.Count == 0)
            return;

        int index = listBox.SelectedIndex;

        if (index <= 0)
            return;

        listBox.SelectedIndex = index - 1;
        listBox.ScrollIntoView(listBox.Items[index - 1]);
    }
    public object GetSelectedItem()
    {
        int index = listBox.SelectedIndex;
        if (index < 0)
            return null;

        return listBox.Items[index];
    }
    public void ClearItems()
    {
        clearItems.Invoke(docView, null);
    }
    public void SetFindResultItems(ObservableCollection<IFindResultItem> findResultItems)
    {
        setFindResultItems.Invoke(docView, new object[] { findResultItems });
    }
}
public class VsVimFindResultsWindowControl : UserControl
{
    public VsVimFindResultsWindowControl()
    {
        try
        {
            string xamlPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"VsVimScripts\VsVimFindResultsWindow.xaml");

            using (var sm = new StreamReader(xamlPath, Encoding.UTF8))
            {
                Grid grid = (Grid)XamlReader.Load(sm.BaseStream);
                this.Content = grid;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
            throw ex;
        }
    }
    public DependencyObject FindControl(string name)
    {
        return FindControl(this, name);
    }
    private DependencyObject FindControl(DependencyObject obj, string name)
    {
        if (obj != null)
        {
            if (obj is FrameworkElement && ((FrameworkElement)obj).Name == name)
            {
                return obj;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject childReturn = FindControl(VisualTreeHelper.GetChild(obj, i), name);
                if (childReturn != null)
                {
                    return childReturn;
                }
            }
        }
        return null;
    }
    public void SetFindResultItems(object findResultItems)
    {
        this.DataContext = findResultItems;
    }
    public void ClearItems()
    {
        this.DataContext = null;
    }
}
public interface IFindResultItem
{
    string Name { get; set; }
    object NavigateItem { get; set; }
}
