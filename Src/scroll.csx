#load "util.csx"
#r "Microsoft.VisualStudio.Text.Data.dll"

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using Vim;

IWpfTextView textView;

if (!VimBuffer.TryGetWpfTextView(out textView))
{
    VimBuffer.DisplayError("Can not get WpfTextView");
    return;
}


VimBuffer.KeyInputStart += OnKeyInputStart;
VimBuffer.Closed += OnBufferClosed;

private void OnKeyInputStart(object sender, KeyInputStartEventArgs e)
{
    e.Handled = true;

    if (e.KeyInput.Char == 'j')
    {
        textView.ViewScroller.ScrollViewportVerticallyByPixels(-10);
    }
    else if (e.KeyInput.Char == 'k')
    {
        textView.ViewScroller.ScrollViewportVerticallyByPixels(10);
    }
    else if (e.KeyInput.Char == 'd')
    {
        textView.ViewScroller.ScrollViewportVerticallyByPixels(-50);
    }
    else if (e.KeyInput.Char == 'u')
    {
        textView.ViewScroller.ScrollViewportVerticallyByPixels(50);
    }
    else
    {
        var count = textView.TextViewLines.Count;
        var index = count / 2;
        if (textView.TextViewLines.Count <= index)
        {
            index = 0;
        }
        var line = textView.TextViewLines[index];

        var lineNumber = line.Start.GetContainingLine().LineNumber;
        var snapshotLine = textView.TextSnapshot.GetLineFromLineNumber(lineNumber);
        var point = new SnapshotPoint(textView.TextSnapshot, snapshotLine.Start.Position);
        textView.Caret.MoveTo(new SnapshotPoint(textView.TextSnapshot, point));

        EndIntercept();
        return;
    }
}
private void EndIntercept()
{
    VimBuffer.KeyInputStart -= OnKeyInputStart;
    VimBuffer.Closed -= OnBufferClosed;
}
private void OnBufferClosed(object sender, EventArgs e)
{
    EndIntercept();
}

