#load "util.csx"

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using Vim;

IVimBuffer vimBuffer;
IWpfTextView textView;

if (!TryGetActiveVimBuffer(out vimBuffer))
{
    DisplayError("Can not get VimBuffer");
    return;
}

if (!TryGetWpfTextView(vimBuffer, out textView))
{
    DisplayError("Can not get WpfTextView");
    return;
}


vimBuffer.KeyInputStart += OnKeyInputStart;
vimBuffer.Closed += OnBufferClosed;

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
        count = count / 2;
        var line = textView.TextViewLines[count];

        var lineNumber = line.Start.GetContainingLine().LineNumber;
        var snapshotLine = textView.TextSnapshot.GetLineFromLineNumber(lineNumber);
        var point = new SnapshotPoint(textView.TextSnapshot, snapshotLine.Start.Position);
        textView.Caret.MoveTo(new SnapshotPoint(textView.TextSnapshot, point));

        InterceptEnd();
        return;
    }
}
private void InterceptEnd()
{
    vimBuffer.KeyInputStart -= OnKeyInputStart;
    vimBuffer.Closed -= OnBufferClosed;
}
private void OnBufferClosed(object sender, EventArgs e)
{
    InterceptEnd();
}

