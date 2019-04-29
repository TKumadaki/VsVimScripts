#load "util.csx"

static int callCount = 0;

callCount++;

VimBuffer.DisplayStatus($"Called Count {callCount}");

