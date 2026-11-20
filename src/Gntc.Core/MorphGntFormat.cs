using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Gntc.Core;

public static partial class MorphGntFormat
{
    public static Corpus Read(byte[] bytes, string book)
    {
        if (!Regex.IsMatch(book, "^[1-3]?[A-Za-z]+$")) throw new FormatException("Invalid book abbreviation.");
        var result = new Corpus();
        result.Metadata["book"] = book;
        result.Metadata["sourceSha256"] = Convert.ToHexStringLower(SHA256.HashData(bytes));
        result.Metadata["tokenization"] = "morphgnt-row-v1";
        result.Metadata["profile"] = Profile.Id;
        result.Metadata["textCoverage"] = "full";
        result.Metadata["analysisCoverage"] = "partial";
        result.Metadata["morphologyCoverage"] = "full";
        var previousBcv = "";
        var position = 0;
        var index = 0;
        foreach (var line in new UTF8Encoding(false, true).GetString(bytes).TrimStart('\uFEFF').Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 7 || !BcvPattern().IsMatch(parts[0]) || !Regex.IsMatch(parts[1], "^[A-Z-]{2}$") || parts[2].Length != 8)
                throw new FormatException($"Invalid MorphGNT row {index + 1}.");
            if (previousBcv.Length > 0 && (string.CompareOrdinal(parts[0], previousBcv) < 0 || parts[0][..2] != previousBcv[..2]))
                throw new FormatException("Input must be one book in canonical verse order.");
            position = previousBcv == parts[0] ? position + 1 : 1;
            previousBcv = parts[0];
            var chapter = int.Parse(parts[0][2..4]);
            var verse = int.Parse(parts[0][4..6]);
            if (chapter < 1 || verse < 1) throw new FormatException("Invalid chapter or verse.");
            result.Words.Add(new Word
            {
                Id = ++index, Verse = $"{book}.{chapter}.{verse}", Position = position, SourceBcv = parts[0],
                Pos = parts[1], Parsing = parts[2], Surface = parts[3], Form = parts[4], Normalized = parts[5], Lemma = parts[6]
            });
        }
        if (result.Words.Count == 0) throw new FormatException("No MorphGNT rows.");
        return result;
    }

    public static string Write(Corpus corpus)
    {
        if (corpus.Words.Any(w => !w.HasMorphology)) throw new FormatException("MorphGNT export requires full morphology.");
        if (corpus.Words.Any(w => new[] { w.SourceBcv, w.Pos!, w.Parsing!, w.Surface, w.Form!, w.Normalized!, w.Lemma! }
            .Any(value => value.Any(char.IsWhiteSpace)))) throw new FormatException("MorphGNT fields cannot contain whitespace.");
        return string.Concat(corpus.Words.OrderBy(w => w.Id).Select(w =>
            $"{w.SourceBcv} {w.Pos} {w.Parsing} {w.Surface} {w.Form} {w.Normalized} {w.Lemma}\n"));
    }

    public static void VerifySource(Corpus corpus, byte[] bytes)
    {
        var book = corpus.Metadata.GetValueOrDefault("book") ?? throw new FormatException("Missing book metadata.");
        var source = Read(bytes, book);
        if (source.Metadata["sourceSha256"] != corpus.Metadata.GetValueOrDefault("sourceSha256"))
            throw new FormatException("Source checksum does not match.");
        if (corpus.Metadata.GetValueOrDefault("textCoverage") == "full" && corpus.Words.Count != source.Words.Count)
            throw new FormatException("Full text has a different word count from the source.");
        var byReference = source.Words.ToDictionary(w => w.Reference);
        foreach (var word in corpus.Words)
        {
            if (!byReference.TryGetValue(word.Reference, out var expected) || expected.Id != word.Id ||
                expected.SourceBcv != word.SourceBcv || expected.Surface != word.Surface ||
                word.HasMorphology && (expected.Lemma != word.Lemma || expected.Pos != word.Pos || expected.Parsing != word.Parsing ||
                expected.Form != word.Form || expected.Normalized != word.Normalized))
                throw new FormatException($"Source mismatch at {word.Reference}.");
        }
    }

    [GeneratedRegex("^[0-9]{6}$")] private static partial Regex BcvPattern();
}
