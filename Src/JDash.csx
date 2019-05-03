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
var scroller = new Scroller(VimBuffer, textView);

public class Scroller
{
    private enum Direction
    {
        None,
        Up,
        Down
    }

    private DateTime _lastKeyPressedTime = DateTime.MinValue;
    private Direction _lastKeyPressedDirection = Direction.None;
    private double _lastKeyPressedDistanceFromTop = 0;
    private int _keyPressedCount = 0;
    private bool _scrollMode = false;
    private IVimBuffer _vimBuffer;
    private IWpfTextView _textView = null;
    private double _distanceToScroll = 0;
    private int _caretPositionIndex = 0;

    private const double ValidInterval = 100;
    private const int LimitKeyPressedCount = 100;
    private const double DefaultDistanceToScroll = 10;
    private const double MaxDistanceToScroll = 500;
    private const double IncrementDistance = 1;

    public Scroller(IVimBuffer vimBuffer, IWpfTextView textView)
    {
        _vimBuffer = vimBuffer;
        _vimBuffer.KeyInputStart += OnKeyInputStart;
        _vimBuffer.Closed += OnBufferClosed;
        _textView = textView;
        Initilize();
    }

    private void Initilize()
    {
        _lastKeyPressedTime = DateTime.MinValue;
        _lastKeyPressedDirection = Direction.None;
        _lastKeyPressedDistanceFromTop = 0;
        _keyPressedCount = 0;
    }

    private bool CanSwitchScrollMode(char letter)
    {
        if (_vimBuffer.ModeKind != ModeKind.Normal)
            return false;

        var direction = GetDirection(letter);
        if (direction == Direction.None || _lastKeyPressedDirection != direction)
        {
            _lastKeyPressedDirection = direction;
            _keyPressedCount = 0;
            return false;
        }
        DateTime keyPressedTime = DateTime.Now;
        if (1 < _keyPressedCount)
        {
            TimeSpan timeSpan = keyPressedTime.Subtract(_lastKeyPressedTime);
            if (ValidInterval < timeSpan.TotalMilliseconds)
            {
                _lastKeyPressedDirection = direction;
                _keyPressedCount = 0;
                return false;
            }
        }
        _lastKeyPressedTime = keyPressedTime;
        var KeyPressedDistanceFromTop = _textView.Caret.Top - _textView.ViewportTop;
        if (_lastKeyPressedDistanceFromTop == KeyPressedDistanceFromTop)
        {
            _keyPressedCount++;
            if (LimitKeyPressedCount < _keyPressedCount)
            {
                Initilize();
                return true;
            }
        }
        _lastKeyPressedDistanceFromTop = KeyPressedDistanceFromTop;
        return false;
    }

    private void SwitchNormalMode()
    {
        Initilize();
        _vimBuffer.DisplayStatus(string.Empty);
        _scrollMode = false;
    }

    private void SwitchScrollMode()
    {
        _distanceToScroll = DefaultDistanceToScroll;
        _scrollMode = true;

        _caretPositionIndex = 0;
        var textViewLines = _textView.TextViewLines;
        if (textViewLines.Count == 0)
            return;

        var firstLineNumber = textViewLines[0].Start.GetContainingLine().LineNumber;
        var caretLineNumber = _textView.Caret.Position.BufferPosition.GetContainingLine().LineNumber;
        _caretPositionIndex = caretLineNumber - firstLineNumber;
    }

    private Direction GetDirection(char letter)
    {
        switch (letter)
        {
            case 'j':
                return Direction.Down;
            case 'k':
                return Direction.Up;
            default:
                return Direction.None;
        }
    }

    private void Scroll(object sender, KeyInputStartEventArgs e)
    {
        var letter = e.KeyInput.Char;
        var direction = GetDirection(letter);
        e.Handled = true;

        if (direction == Direction.None)
        {
            SwitchNormalMode();
            return;
        }

        DateTime keyPressedTime = DateTime.Now;
        if (_lastKeyPressedDirection != direction)
        {
            _lastKeyPressedDirection = direction;
            _distanceToScroll = DefaultDistanceToScroll;
        }
        else
        {
            TimeSpan timeSpan = keyPressedTime.Subtract(_lastKeyPressedTime);
            if (ValidInterval < timeSpan.TotalMilliseconds)
            {
                SwitchNormalMode();
                return;
            }
            _distanceToScroll += IncrementDistance;
            _distanceToScroll = Math.Min(_distanceToScroll, MaxDistanceToScroll);
        }
        _lastKeyPressedTime = keyPressedTime;

        _textView.ViewScroller.ScrollViewportVerticallyByPixels((direction == Direction.Down ? -1 : 1) * _distanceToScroll);
        var index = Math.Min(_textView.TextViewLines.Count - 1, _caretPositionIndex);
        if (index < 0)
            return;

        var line = _textView.TextViewLines[index];

        var lineNumber = line.Start.GetContainingLine().LineNumber;
        var snapshotLine = _textView.TextSnapshot.GetLineFromLineNumber(lineNumber);
        var point = new SnapshotPoint(_textView.TextSnapshot, snapshotLine.Start.Position);
        _textView.Caret.MoveTo(new SnapshotPoint(_textView.TextSnapshot, point));
    }

    private void OnKeyInputStart(object sender, KeyInputStartEventArgs e)
    {
        if (!_scrollMode)
        {
            if (!CanSwitchScrollMode(e.KeyInput.Char))
                return;

            SwitchScrollMode();
        }
        Scroll(sender, e);
    }

    private void EndIntercept()
    {
        _vimBuffer.KeyInputStart -= OnKeyInputStart;
        _vimBuffer.Closed -= OnBufferClosed;
    }

    private void OnBufferClosed(object sender, EventArgs e)
    {
        EndIntercept();
    }
}
