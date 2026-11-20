using System.Text;

namespace Gntc.Core;

public static class CorpusFiles
{
    public static Corpus Read(string path)
    {
        var text = File.ReadAllText(path, new UTF8Encoding(false, true));
        var corpus = Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".gntc" => CompactFormat.Read(text),
            ".json" => JsonFormat.Read(text),
            ".xml" => TeiFormat.Read(text),
            _ => throw new FormatException("Expected .gntc, .json, or .xml input.")
        };
        Validator.RequireValid(corpus);
        return corpus;
    }

    public static void Write(Corpus corpus, string path)
    {
        Validator.RequireValid(corpus);
        var text = Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".gntc" => CompactFormat.Write(corpus),
            ".json" => JsonFormat.Write(corpus),
            ".xml" => TeiFormat.Write(corpus),
            _ => throw new FormatException("Expected .gntc, .json, or .xml output.")
        };
        WriteText(path, text);
    }

    public static void WriteText(string path, string text)
    {
        var absolute = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(absolute)!);
        var temporary = absolute + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            File.WriteAllText(temporary, text, new UTF8Encoding(false));
            File.Move(temporary, absolute, true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
}
