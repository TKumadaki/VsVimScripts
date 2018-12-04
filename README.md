VsVim Scripts
===

VsVim Scripts is a C # script file that runs on VsVim.  

## Execution method

Official VsVim does not support script file execution.  
Compile and install the following branches.  
https://github.com/TKumadaki/VsVim/tree/execute-csharp-script-ver2  
This makes the csx, csxe commands usable.  
Next, place the csx file in the "C: \ Users \ user \ VsVimScripts" folder.  

The command to execute the csx file is as follows.  

csx <script file name>  
csxe <script file name>  

The script file name does not need an extension.  

csx reuses compiled objects once.  
You can use the static variable to hold information that was last executed.  

csxe compiles every time a command is executed.  
It is assumed to be used for debugging.  

## Caution

Only Visual Studio 2017 works.  
This function is not official, so if code encounter any problems, I may delete the repository without notice.  

## License

All code in this project is covered under the Apache 2 license a copy of which is available in the same directory under the name License.txt.  
VsVim is a work of Jared Parsons.  

For Japanese Version: [日本語版](README.ja.md)
