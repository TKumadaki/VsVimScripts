#load "util.csx"
#r "Microsoft.VisualStudio.ComponentModelHost.dll"
#r "Microsoft.VisualStudio.Text.Data.dll"

using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Formatting;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Vim;

//This is the layer used by VsVim.
const string AdornmentLayerName = "BlockCaretAdornmentLayer";

IAdornmentLayer adornmentLayer;
IWpfTextView textView;
IClassificationFormatMapService classicationFormatMapService;
IClassificationFormatMap classificationFormatMap;
IComponentModel componentModel;
TextBox showCommandText;
MethodInfo GetShowCommandText;
bool isAdornmentPresent = false;
bool isUpdating = false;
object tag = new object();

if (!VimBuffer.TryGetWpfTextView(out textView))
{
    VimBuffer.DisplayError("Can not get WpfTextView");
    return;
}

var asm = typeof(Vim.UI.Wpf.IBlockCaret).Assembly;
var commandMarginUtil = asm.GetType("Vim.UI.Wpf.Implementation.CommandMargin.CommandMarginUtil");
GetShowCommandText = commandMarginUtil.GetMethod("GetShowCommandText");

componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
classicationFormatMapService = componentModel.GetService<IClassificationFormatMapService>();


//textView.GotAggregateFocus += OnCaretEvent;
textView.Caret.PositionChanged += OnCaretEvent;

adornmentLayer = textView.GetAdornmentLayer(AdornmentLayerName);
classificationFormatMap = classicationFormatMapService.GetClassificationFormatMap(textView);

showCommandText = new TextBox();
showCommandText.Text = string.Empty;
showCommandText.FontFamily = classificationFormatMap.DefaultTextProperties.Typeface.FontFamily;
showCommandText.Foreground = Brushes.White;
showCommandText.Background = Brushes.Black;
showCommandText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

VimBuffer.KeyInputEnd += OnKeyInputEnd;
VimBuffer.Closed += OnBufferClosed;

//UpdateShowCommand();

private bool IsRealCaretVisible
{
    get
    {
        try
        {
            var caret = textView.Caret;
            var line = caret.ContainingTextViewLine;
            return line.VisibilityState != VisibilityState.Unattached && textView.HasAggregateFocus;
        }
        catch (InvalidOperationException)
        {
            // InvalidOperationException is thrown when we ask for ContainingTextViewLine and the view
            // is not yet completely rendered.  It's safe to say at this point that the caret is not 
            // visible
            return false;
        }
    }
}
private void UpdateShowCommand()
{
    if (!IsRealCaretVisible)
        return;

    if (isUpdating)
        return;

    try
    {
        isUpdating = true;
        if (!isAdornmentPresent)
        {
            adornmentLayer.AddAdornment(AdornmentPositioningBehavior.TextRelative,
                                        new SnapshotSpan(textView.Caret.Position.BufferPosition, 0),
                                        tag,
                                        showCommandText,
                                        OnAdornmentRemoved);
            isAdornmentPresent = true;
        }
        Canvas.SetLeft(showCommandText, textView.Caret.Left);
        Canvas.SetTop(showCommandText, textView.Caret.Bottom);

        var commandText = (string)GetShowCommandText.Invoke(null, new object[] { VimBuffer });
        commandText = commandText.Replace(" ", "<Space>");
        if (string.IsNullOrEmpty(commandText))
        {
            showCommandText.Visibility = Visibility.Hidden;
            return;
        }
        showCommandText.Visibility = Visibility.Visible;
        showCommandText.Text = commandText;
    }
    finally
    {
        isUpdating = false;
    }
}
private void OnCaretEvent(object sender, EventArgs e)
{
    UpdateShowCommand();
}
private void OnAdornmentRemoved(object sender, UIElement element)
{
    isAdornmentPresent = false;
}
public void OnKeyInputEnd(object sender, KeyInputEventArgs e)
{
    UpdateShowCommand();
}
public void OnBufferClosed(object sender, EventArgs e)
{
    if (isAdornmentPresent)
    {
        adornmentLayer.RemoveAdornmentsByTag(tag);
    }
    if (!textView.IsClosed)
    {
        //textView.GotAggregateFocus -= OnCaretEvent;
        textView.Caret.PositionChanged -= OnCaretEvent;
    }
    VimBuffer.KeyInputEnd -= OnKeyInputEnd;
    VimBuffer.Closed -= OnBufferClosed;
}

