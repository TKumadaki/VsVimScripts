
using EnvDTE80;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using System.Collections.Generic;
using Vim;
using Vim.Extensions;

public class Util
{
    public static DTE2 GetDTE2()
    {
        return Package.GetGlobalService(typeof(DTE)) as DTE2;
    }
}

public static void DisplayError(this IVim vim, string input)
{
    vim.ActiveStatusUtil.OnError(input);
}
public static void DisplayStatus(this IVim vim, string input)
{
    vim.ActiveStatusUtil.OnStatus(input);
}
public static void DisplayStatusLong(this IVim vim, IEnumerable<string> value)
{
    vim.ActiveStatusUtil.OnStatusLong(value);
}
public static void DisplayWarning(this IVim vim, string input)
{
    vim.ActiveStatusUtil.OnWarning(input);
}
public static bool TryGetActiveVimBuffer(this IVim vim, out IVimBuffer vimBuffer)
{
    var activeVimBuffer = vim.ActiveBuffer;
    if (activeVimBuffer.IsNone())
    {
        vimBuffer = null;
        return false;
    }
    vimBuffer = activeVimBuffer.Value;
    return true;
}
public static bool TryGetWpfTextView(this IVimBuffer vimBuffer, out IWpfTextView textView)
{
    textView = vimBuffer?.TextView as IWpfTextView;
    if (textView == null)
    {
        return false;
    }
    return true;
}
public static void Process(this IVimBuffer vimBuffer, KeyInput keyInput)
{
    vimBuffer.ProcessFromScript(keyInput);
}
public static void Process(this IVimBuffer vimBuffer, string input, bool enter = false)
{
    foreach (var c in input)
    {
        var i = KeyInputUtil.CharToKeyInput(c);
        Process(vimBuffer, i);
    }

    if (enter)
    {
        Process(vimBuffer, KeyInputUtil.EnterKey);
    }
}
