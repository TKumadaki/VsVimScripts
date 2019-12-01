//EasyMotion.csx

#load "util.csx"

#r "Microsoft.VisualStudio.ComponentModelHost.dll"
#r "Microsoft.VisualStudio.Text.Data.dll"
#r "Microsoft.VisualStudio.Text.Logic.dll"

using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using Vim.Extensions;
using Vim.Interpreter;
using Vim;

//This is the layer used by VsVim.
const string AdornmentLayerName = "BlockCaretAdornmentLayer";

IWpfTextView textView;
EasyMotion easyMotion;

if (!VimBuffer.TryGetWpfTextView(out textView))
{
    VimBuffer.DisplayError("Can not get WpfTextView");
    return;
}

easyMotion = new EasyMotion(textView, VimBuffer);

public class EasyMotion
{
    private IWpfTextView textView;
    private IVimBuffer vimBuffer;
    private string searchKeyword = string.Empty;
    private int defaultSearchKeywordLength = 1;
    private Action<object, KeyInputStartEventArgs> action = null;

    private static readonly string[] navigationKeys =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
    .Select(x => x.ToString())
    .ToArray();

    IComponentModel componentModel;
    IAdornmentLayer adornmentLayer;
    IClassificationFormatMapService classicationFormatMapService;
    IClassificationFormatMap classificationFormatMap;

    private readonly Dictionary<string, SnapshotPoint> navigateMap = new Dictionary<string, SnapshotPoint>();
    private readonly object tag = new object();

    public EasyMotion(IWpfTextView textView, IVimBuffer vimBuffer)
    {
        this.textView = textView;
        this.vimBuffer = vimBuffer;

        this.vimBuffer.KeyInputStart += OnKeyInputStart;
        this.vimBuffer.Closed += OnBufferClosed;

        vimBuffer.DisplayStatus($">{searchKeyword}");

        componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
        classicationFormatMapService = componentModel.GetService<IClassificationFormatMapService>();
        adornmentLayer = textView.GetAdornmentLayer(AdornmentLayerName);
        classificationFormatMap = classicationFormatMapService.GetClassificationFormatMap(textView);

        action = InputSearchKeyword;
    }
    private void OnKeyInputStart(object sender, KeyInputStartEventArgs e)
    {
        action?.Invoke(sender, e);
    }
    private void OnBufferClosed(object sender, EventArgs e)
    {
        vimBuffer.DisplayStatus(string.Empty);
        EndIntercept();
    }
    private void EndIntercept()
    {
        textView.LayoutChanged -= OnLayoutChanged;
        adornmentLayer.RemoveAdornmentsByTag(tag);
        vimBuffer.KeyInputStart -= OnKeyInputStart;
        vimBuffer.Closed -= OnBufferClosed;
    }
    private void InputSearchKeyword(object sender, KeyInputStartEventArgs e)
    {
        e.Handled = true;
        if (e.KeyInput.Key == VimKey.Escape)
        {
            EndIntercept();
            return;
        }
        else if (e.KeyInput.Key == VimKey.Enter)
        {
            vimBuffer.DisplayStatus($">{searchKeyword}");
            if(!string.IsNullOrWhiteSpace(searchKeyword))
            {
                AddAdornments(true);
            }
        }
        else if (e.KeyInput.RawChar.IsSome() && !char.IsControl(e.KeyInput.Char))
        {
            searchKeyword += e.KeyInput.Char.ToString();
            vimBuffer.DisplayStatus($">{searchKeyword}");
            if (defaultSearchKeywordLength <= searchKeyword.Length)
            {
                AddAdornments(true);
            }
        }
    }
    private void InputJumpLetter(object sender, KeyInputStartEventArgs e)
    {
        e.Handled = true;
        if (e.KeyInput.Key == VimKey.Escape || e.KeyInput.Key == VimKey.Enter)
        {
            EndIntercept();
            return;
        }
        else if (e.KeyInput.RawChar.IsSome() && !char.IsControl(e.KeyInput.Char))
        {
            string jumpKeyword = e.KeyInput.Char.ToString();
            SnapshotPoint snapshotPoint;
            if (navigateMap.TryGetValue(jumpKeyword, out snapshotPoint))
            {
                if (snapshotPoint.Snapshot == textView.TextSnapshot)
                {
                    textView.Caret.MoveTo(snapshotPoint);
                }
                EndIntercept();
            }
        }
    }
    private void OnLayoutChanged(object sender, EventArgs e)
    {
        ResetAdornments();
    }
    private void ResetAdornments()
    {
        adornmentLayer.RemoveAdornmentsByTag(tag);
        AddAdornments(false);
    }
    private void AddAdornments(bool isFirst)
    {
        if (textView.InLayout)
        {
            return;
        }

        navigateMap.Clear();

        IWpfTextViewLineCollection lines = textView.TextViewLines;
        int startPosition = lines.FirstVisibleLine.Start.Position;
        int endPosition = lines.LastVisibleLine.End.Position;
        ITextSnapshot snapshot = textView.TextSnapshot;
        SnapshotSpan span;
        int navigateIndex = 0;
        string key;
        int searchKeywordLength = searchKeyword.Length;

        searchKeyword = searchKeyword.ToLower();
        for (int i = startPosition; i < endPosition; i++)
        {
            if (endPosition < (i + searchKeywordLength))
                break;

            span = new SnapshotSpan(snapshot, i, searchKeywordLength);
            if (span.GetText().ToLower() == searchKeyword)
            {
                key = navigationKeys[navigateIndex];
                navigateIndex++;
                AddNavigateToPoint(lines, new SnapshotPoint(snapshot, i), key);
            }
            if (navigationKeys.Length <= navigateIndex)
                break;
        }

        if (navigateIndex == 0)
        {
            vimBuffer.DisplayStatus("not found.");
            EndIntercept();
            return;
        }

        if (isFirst)
        {
            textView.LayoutChanged += OnLayoutChanged;
            action = InputJumpLetter;
        }
    }
    private void AddNavigateToPoint(IWpfTextViewLineCollection textViewLines, SnapshotPoint point, string key)
    {
        navigateMap[key] = point;

        SnapshotSpan span = new SnapshotSpan(point, 1);
        TextBounds bounds = textViewLines.GetCharacterBounds(point);

        TextBox textBox = new TextBox();
        textBox.Text = key;
        textBox.FontFamily = classificationFormatMap.DefaultTextProperties.Typeface.FontFamily;
        textBox.Foreground = Brushes.Black;
        textBox.Background = Brushes.Green;
        textBox.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        Canvas.SetTop(textBox, bounds.TextTop);
        Canvas.SetLeft(textBox, bounds.Left);
        Canvas.SetZIndex(textBox, 10);

        adornmentLayer.AddAdornment(span, tag, textBox);
    }
}
