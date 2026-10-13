using System;
using System.Collections.Generic;
using System.IO;
using Nucleus.Gaming.Localization;

namespace Nucleus.Coop.Services.Tests.Localization;

/// <summary>A temporary i18n folder with language files, and a translator on it that records its log.</summary>
internal sealed class LanguageFolder : IDisposable
{
    public const string English = @"{
  ""_meta"": { ""locale"": ""en"", ""englishName"": ""English"", ""nativeName"": ""English"" },
  ""common"": { ""ok"": ""OK"", ""cancel"": ""Cancel"" },
  ""library"": {
    ""title"": ""Library"",
    ""gameCount_zero"": ""No games"",
    ""gameCount_one"": ""{count} game"",
    ""gameCount_other"": ""{count} games"",
    ""hello"": ""Hello {name}"",
    ""lockHint"": ""Press <kbd>{key}</kbd> to lock""
  }
}";

    public const string French = @"{
  ""_meta"": { ""locale"": ""fr"", ""englishName"": ""French"", ""nativeName"": ""Français"" },
  ""_note"": ""Translator notes are ignored."",
  ""common"": { ""ok"": ""D'accord"" },
  ""library"": {
    ""gameCount_one"": ""{count} jeu"",
    ""gameCount_other"": ""{count} jeux"",
    ""hello"": ""Bonjour {name}""
  }
}";

    public LanguageFolder(params (string Name, string Content)[] files)
    {
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "nucleus-i18n-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path);
        foreach ((string name, string content) in files)
        {
            Write(name, content);
        }
    }

    public string Path { get; }

    public List<string> Log { get; } = new List<string>();

    public static LanguageFolder WithEnglishAndFrench() => new(("en.json", English), ("fr.json", French));

    public void Write(string name, string content) => File.WriteAllText(System.IO.Path.Combine(Path, name), content);

    public Translator Translator(string locale = "en", Func<string?>? builtInEnglish = null) =>
        new(Path, locale, message => { lock (Log) Log.Add(message); }, builtInEnglish == null ? null : () => builtInEnglish()!);

    public void Dispose()
    {
        try
        {
            Directory.Delete(Path, true);
        }
        catch (IOException)
        {
        }
    }
}

/// <summary>A language store in memory.</summary>
internal sealed class MemoryLanguageStore : ILanguageStore
{
    public string Value { get; set; } = "";

    public string Load() => Value;

    public void Save(string locale) => Value = locale;
}
