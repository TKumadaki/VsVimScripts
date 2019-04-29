#r "EnvDTE80.dll"
#r "EnvDTE.dll"
#r "Microsoft.VisualStudio.CoreUtility.dll"
#r "Microsoft.VisualStudio.Text.UI.dll"
#r "Microsoft.VisualStudio.Text.UI.Wpf.dll"
#r "Microsoft.VisualStudio.Shell.11.0.dll"

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

public static void DisplayError(this IVimBuffer vimBuffer, string input)
{
    vimBuffer.VimBufferData.StatusUtil.OnError(input);
}
public static void DisplayStatus(this IVimBuffer vimBuffer, string input)
{
    vimBuffer.VimBufferData.StatusUtil.OnStatus(input);
}
public static void DisplayStatusLong(this IVimBuffer vimBuffer, IEnumerable<string> value)
{
    vimBuffer.VimBufferData.StatusUtil.OnStatusLong(value);
}
public static void DisplayWarning(this IVimBuffer vimBuffer, string input)
{
    vimBuffer.VimBufferData.StatusUtil.OnWarning(input);
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
public static void Process(this IVimBuffer vimBuffer, string input, bool enter = false)
{
    foreach (var c in input)
    {
        var i = KeyInputUtil.CharToKeyInput(c);
        vimBuffer.Process(i);
    }

    if (enter)
    {
        vimBuffer.Process(KeyInputUtil.EnterKey);
    }
}
