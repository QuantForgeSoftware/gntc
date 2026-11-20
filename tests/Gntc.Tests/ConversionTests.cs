using System.Text;
using System.Xml.Linq;
using Gntc.Core;

namespace Gntc.Tests;

public class ConversionTests
{
    public static string Root
    {
        get
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Gntc.sln"))) directory = directory.Parent;
            return directory?.FullName ?? throw new InvalidOperationException("Cannot find repository root.");
        }
    }
    public static string At(string relative) => Path.Combine(Root, relative);
    public static Corpus Full() => CorpusFiles.Read(At("data/1tim.gntc"));
    private static Corpus RoundTrip(Corpus corpus, string format) => format switch
    {
        "compact" => CompactFormat.Read(CompactFormat.Write(corpus)),
        "json" => JsonFormat.Read(JsonFormat.Write(corpus)),
        "tei" => TeiFormat.Read(TeiFormat.Write(corpus)),
        _ => throw new ArgumentException("Unknown test format.")
    };

    [Theory]
    [InlineData("compact", "compact")]
    [InlineData("compact", "json")]
    [InlineData("compact", "tei")]
    [InlineData("json", "compact")]
    [InlineData("json", "json")]
    [InlineData("json", "tei")]
    [InlineData("tei", "compact")]
    [InlineData("tei", "json")]
    [InlineData("tei", "tei")]
    public void WholeBookGraphSurvivesEveryFormatPair(string first, string second)
    {
        var original = Full();
        var actual = RoundTrip(RoundTrip(original, first), second);
        Validator.RequireValid(actual);
        Assert.Equal(JsonFormat.Write(original), JsonFormat.Write(actual));
        MorphGntFormat.VerifySource(actual, File.ReadAllBytes(At("data/source/75-1Ti-morphgnt.txt")));
    }

    [Fact]
    public void OriginalSevenColumnsAndApparatusMarksArePreserved()
    {
        var raw = File.ReadAllBytes(At("data/source/75-1Ti-morphgnt.txt"));
        var corpus = MorphGntFormat.Read(raw, "1Ti");
        Assert.Equal(raw, Encoding.UTF8.GetBytes(MorphGntFormat.Write(corpus)));
        Assert.Equal(1591, corpus.Words.Count);
        Assert.Equal(113, corpus.Words.Select(w => w.Verse).Distinct().Count());
        Assert.Equal("⸂Χριστοῦ", corpus.Words[2].Surface);
        Assert.Equal("ὀνειδιζόμεθα", corpus.Words.Single(w => w.Id == 829).Form);
    }

    [Theory]
    [InlineData("compact")]
    [InlineData("json")]
    [InlineData("tei")]
    public void UnicodeEscapingAndExtensionAttributesSurvive(string format)
    {
        var corpus = Full();
        corpus.Metadata["annotationNote"] = "α\u0313 | \\ \"quoted\"\nsecond line\t= equals\rcarriage\r\nCRLF";
        corpus.Metadata["empty"] = "";
        corpus.Metadata["spaces"] = "   ";
        corpus.Annotations.Single(a => a.Id == "c795").Attributes["custom.evidence"] = "a|b = c\nd";
        Assert.Equal(JsonFormat.Write(corpus), JsonFormat.Write(RoundTrip(corpus, format)));
    }

    [Fact]
    public void FragmentedMarkersAndAlternativesSurviveTei()
    {
        var corpus = RoundTrip(Full(), "tei");
        Assert.Equal("1074-1075,1078-1079", corpus.Annotations.Single(a => a.Id == "c1074").Args[0]);
        Assert.Equal("c826a,c826b", corpus.Annotations.Single(a => a.Id == "a826").Args[1]);
    }

    [Fact]
    public void ContextNodesUnknownEndpointsAndImplicitRelationsSurvive()
    {
        var corpus = Full();
        corpus.Annotations.Add(new Annotation { Kind = "K", Id = "kExpected", Args = ["assumption", "A contextual expectation; hypothesis only."] });
        corpus.Annotations.Add(new Annotation { Kind = "C", Id = "cTest", Args = ["?", "kExpected", "?", "?", "?", "?"] });
        Validator.RequireValid(corpus);
        Assert.Equal(JsonFormat.Write(corpus), JsonFormat.Write(RoundTrip(corpus, "tei")));
        var document = XDocument.Parse(TeiFormat.Write(corpus));
        XNamespace n = "http://www.tei-c.org/ns/1.0";
        var group = document.Descendants(n + "linkGrp").Single(g => (string?)g.Attribute("type") == "implicit");
        Assert.Equal("context scope", (string?)group.Attribute("targFunc"));
        Assert.All(group.Elements(), link => Assert.Equal(2, link.Attribute("target")!.Value.Split(' ').Length));
    }

    [Fact]
    public void ExtractionIncludesReferencedScopeAndCompetingAttachments()
    {
        var excerpt = CorpusOperations.Extract(Full(), ["1Ti.4.10"]);
        Assert.Contains(excerpt.Annotations, a => a.Id == "a826");
        Assert.Contains(excerpt.Annotations, a => a.Id == "c826a");
        Assert.Contains(excerpt.Annotations, a => a.Id == "c826b");
        Assert.Contains(excerpt.Words, w => w.Id == 789 && w.Reference == "1Ti.4.7.w8");
        Assert.Contains(excerpt.Words, w => w.Id == 817);
        Validator.RequireValid(excerpt);
        MorphGntFormat.VerifySource(excerpt, File.ReadAllBytes(At("data/source/75-1Ti-morphgnt.txt")));
    }

    [Fact]
    public void PromptOmitsUnneededMorphologyAndPreservesMarkers()
    {
        var prompt = CorpusOperations.Prompt(CorpusOperations.Extract(Full(), ["1Ti.4.7", "1Ti.4.8"]));
        var payload = prompt.Split("```text\n")[1].Split("```")[0];
        var corpus = CompactFormat.Read(payload);
        Assert.True(corpus.Words.Single(w => w.Id == 795).HasMorphology);
        Assert.False(corpus.Words.Single(w => w.Id == 789).HasMorphology);
        Assert.Contains("Proposed is not reviewed", prompt);
        Assert.Equal("partial", corpus.Metadata["morphologyCoverage"]);
        Validator.RequireValid(corpus);
    }

    [Fact]
    public void CandidateScanIsExplicitlyOverinclusiveAndNeverAssignsMeaning()
    {
        var source = MorphGntFormat.Read(File.ReadAllBytes(At("data/source/75-1Ti-morphgnt.txt")), "1Ti");
        CorpusOperations.ScanCandidates(source);
        Assert.Equal(288, source.Annotations.Count);
        Assert.All(source.Annotations, a => Assert.Equal("Q", a.Kind));
        Assert.Contains(source.Annotations, a => a.Id == "qx1158" && a.Args[0] == "1158-1160");
        Assert.Contains(source.Annotations, a => a.Id == "q33");
    }

    [Fact]
    public void OverlayFailureDoesNotMutateTheExistingGraph()
    {
        var corpus = Full();
        var before = JsonFormat.Write(corpus);
        var overlay = new Corpus();
        overlay.Metadata["sourceSha256"] = corpus.Metadata["sourceSha256"];
        overlay.Metadata["book"] = "WrongBook";
        overlay.Annotations.Add(new Annotation { Kind = "U", Id = "uBad", Args = ["1", "word"] });
        Assert.Throws<FormatException>(() => CorpusOperations.Apply(corpus, overlay));
        Assert.Equal(before, JsonFormat.Write(corpus));
    }

    [Fact]
    public void TheCentralExampleHasWideScopeAndSeparateInternalContrast()
    {
        var corpus = Full();
        var gar = corpus.Annotations.Single(a => a.Id == "c795");
        Assert.Equal(new[] { "795", "u789_793", "u794_816", "support", "ground", "act" }, gar.Args);
        Assert.Equal("802-816", corpus.Annotations.Single(a => a.Id == "u802_816").Args[0]);
        Assert.Equal("contrast", corpus.Annotations.Single(a => a.Id == "c803").Args[4]);
        Assert.All(corpus.Annotations.Where(a => a.Kind == "C"), a => Assert.Equal("proposed", corpus.Attribute(a, "status")));
    }
}
