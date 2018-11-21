#load "util.csx"
#load "findresultswindow.csx"

using EnvDTE;
using Microsoft.VisualStudio.Text;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Vim;
using Vim.Extensions;

var frw = new FindResultsWindow(Vim);
frw.Display();
