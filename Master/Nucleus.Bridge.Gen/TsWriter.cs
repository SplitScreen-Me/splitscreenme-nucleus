using System;
using System.Text;

namespace Nucleus.Bridge.Gen;

/// <summary>Writes indented TypeScript with <c>\n</c> line endings.</summary>
internal sealed class TsWriter
{
    private const int MaxWidth = 120;

    private readonly StringBuilder _text = new();
    private int _indent;

    /// <summary>Current indentation width, in characters.</summary>
    public int Indent => _indent * 2;

    /// <summary>Writes a line at the current indentation.</summary>
    public TsWriter Line(string line)
    {
        _text.Append(' ', Indent).Append(line).Append('\n');
        return this;
    }

    /// <summary>Writes an empty line, unless the previous line is already empty.</summary>
    public TsWriter Gap()
    {
        if (_text.Length > 1 && !(_text[_text.Length - 1] == '\n' && _text[_text.Length - 2] == '\n'))
        {
            _text.Append('\n');
        }

        return this;
    }

    /// <summary>Writes <paramref name="open"/>, the body one level deeper, then <paramref name="close"/>.</summary>
    public TsWriter Block(string open, Action body, string close)
    {
        Line(open);
        _indent++;
        body();
        _indent--;
        return Line(close);
    }

    /// <summary>Writes a TSDoc comment, on one line when it fits. Does nothing when <paramref name="doc"/> is empty.</summary>
    public TsWriter Doc(string? doc)
    {
        if (string.IsNullOrWhiteSpace(doc))
        {
            return this;
        }

        var lines = doc!.Replace("*/", "*\\/").Split('\n');
        if (lines.Length == 1 && Indent + lines[0].Length + 7 <= MaxWidth)
        {
            return Line($"/** {lines[0]} */");
        }

        Line("/**");
        foreach (var line in lines)
        {
            Line(line.Length == 0 ? " *" : " * " + line);
        }

        return Line(" */");
    }

    /// <summary>Whether a line of <paramref name="length"/> characters fits at the current indentation.</summary>
    public bool Fits(int length) => Indent + length <= MaxWidth;

    /// <inheritdoc/>
    public override string ToString() => _text.ToString();
}
