#load "util.csx"

static int callCount = 0;

callCount++;

Vim.DisplayStatus($"Called Count {callCount}");

