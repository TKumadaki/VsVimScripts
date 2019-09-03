#r "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\CommonExtensions\Microsoft\ManagedLanguages\VBCSharp\InteractiveComponents\Microsoft.CodeAnalysis.dll"
#r "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\CommonExtensions\Microsoft\ManagedLanguages\VBCSharp\InteractiveComponents\Microsoft.CodeAnalysis.CSharp.dll"
#r "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\Microsoft\Microsoft.NET.Build.Extensions\net461\lib\System.Threading.Tasks.dll"

#r "Microsoft.VisualStudio.Text.Data.dll"
#r "Microsoft.VisualStudio.Text.Logic.dll"

#load "util.csx"

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vim;

// Run the following two processes in the script
// Then, unintended behavior was caused.
// 
// VimBuffer.SwitchMode(ModeKind.VisualCharacter, ModeArgument.None);
// textView.Selection.Select(virtualStartPoint, virtualEndPoint);
//
// So I decided to do the processing in the OnSwitchedMode event.
// However, VsVim executes scripts asynchronously.
// Depending on your environment, this script may not work as expected.


VimBuffer.SwitchedMode += OnSwitchedMode;

public void OnSwitchedMode(object sender, SwitchModeEventArgs e)
{
    VimBuffer.SwitchedMode -= OnSwitchedMode;

    IWpfTextView textView;

    if (!VimBuffer.TryGetWpfTextView(out textView))
    {
        VimBuffer.DisplayError("Can not get WpfTextView");
        return;
    }

    ITextSnapshot snapshot;
    ITextEdit edit;
    SnapshotPoint caretBufferPosition;

    caretBufferPosition = textView.Caret.Position.BufferPosition;
    snapshot = textView.TextSnapshot;
    string text = snapshot.GetText();
    SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(text, CSharpParseOptions.Default);

    var nodes = syntaxTree.GetRoot().DescendantNodes();

    var methodSyntaxArray = nodes.OfType<MethodDeclarationSyntax>();
    if (methodSyntaxArray.Count() == 0)
    {
        VimBuffer.DisplayError("Can not find method");
        return;
    }

    MethodDeclarationSyntax syntax = methodSyntaxArray.FirstOrDefault((x) => x.FullSpan.Start <= caretBufferPosition.Position && caretBufferPosition.Position <= x.FullSpan.End);
    if (syntax == null)
    {
        VimBuffer.DisplayError("Can not find method");
        return;
    }

    string textKind = Arguments.Trim().Substring(1, 1);

    int startPoint;
    int endPoint;

    if (textKind == "i")
    {
        //Inner
        startPoint = syntax.Body.OpenBraceToken.FullSpan.End;
        endPoint = syntax.Body.CloseBraceToken.FullSpan.Start;
    }
    else
    {
        //a Text
        startPoint = syntax.FullSpan.Start;
        endPoint = syntax.FullSpan.End;
    }

    string procesKind = Arguments.Trim().Substring(0, 1);

    VirtualSnapshotPoint virtualStartPoint;
    VirtualSnapshotPoint virtualEndPoint;

    switch (procesKind)
    {
        case "d":
            //Delete
            edit = textView.TextBuffer.CreateEdit();
            edit.Delete(startPoint, endPoint - startPoint);
            edit.Apply();

            break;
        case "v":
        case "y":
            //Visual
            if (procesKind == "y")
            {
                //Yank
                VimBuffer.SwitchedMode += OnSwitchedModeForYank;
            }

            textView.Caret.MoveTo(new SnapshotPoint(textView.TextSnapshot, startPoint));

            //When the script does the following, VsVim switches to Visual Mode.

            virtualStartPoint = new VirtualSnapshotPoint(snapshot, startPoint);
            virtualEndPoint = new VirtualSnapshotPoint(snapshot, endPoint);

            textView.Selection.Select(virtualStartPoint, virtualEndPoint);
            break;
    }

}
public void OnSwitchedModeForYank(object sender, SwitchModeEventArgs e)
{
    VimBuffer.SwitchedMode -= OnSwitchedModeForYank;
    VimBuffer.Process("y", enter: false);
}

