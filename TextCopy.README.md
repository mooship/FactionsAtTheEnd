# TextCopy clipboard utility for .NET
TextCopy is a cross-platform clipboard library for .NET. It allows you to copy and paste text to and from the system clipboard in a simple, platform-agnostic way.

## Installation

Install via NuGet:

```
dotnet add package TextCopy
```

## Usage

```
using TextCopy;

// Copy text to clipboard
ClipboardService.SetText("Hello, clipboard!");

// Get text from clipboard
string text = ClipboardService.GetText();
```

- `ClipboardService.SetText(string)` copies the provided string to the clipboard.
- `ClipboardService.GetText()` retrieves the current clipboard contents as a string.

TextCopy works on Windows, macOS, and Linux.

## Reference
- [TextCopy GitHub](https://github.com/CopyText/TextCopy)
- [NuGet package](https://www.nuget.org/packages/TextCopy/)
