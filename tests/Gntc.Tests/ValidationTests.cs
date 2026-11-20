using System.Text;
using System.Xml.Linq;
using Gntc.Core;

namespace Gntc.Tests;

public class ValidationTests
{
    [Fact]
    public void InvalidXmlCharactersCannotEnterTheCrossFormatGraph()
    {
        var corpus = ConversionTests.Full();
        corpus.Metadata["bad"] = "invalid\u0001";
        Assert.Contains(Validator.Errors(corpus), e => e.Contains("XML 1.0"));
    }

    [Fact]
    public void SourceExportRejectsFieldsThatWouldCreateExtraColumns()
    {
        var corpus = ConversionTests.Full();
        corpus.Words[0].Lemma = "two words";
        Assert.Throws<FormatException>(() => MorphGntFormat.Write(corpus));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("5-2")]
    [InlineData("1-3,3-5")]
    [InlineData("3,1")]
    [InlineData("1-")]
    [InlineData("1-2000000")]
    public void InvalidExtentsAreRejected(string value) => Assert.Throws<FormatException>(() => Extent.Parse(value));

    [Theory]
    [InlineData("@GNTC/2\n")]
    [InlineData("@GNTC/1\nZ unknown\n")]
    [InlineData("@GNTC/1\n@meta x=a\n@meta x=b\n")]
    [InlineData("@GNTC/1\nT 1 a\n")]
    [InlineData("@GNTC/1\nR 1 1Ti.1.1 1 150101\nT 1 a|\n")]
    [InlineData("@GNTC/1\nR 1 1Ti.1.1 1 150101\nT 1 a\nT 1 b\n")]
    [InlineData("@GNTC/1\nK k1 assumption \"unclosed\n")]
    [InlineData("@GNTC/1\nM 1 x N- ----NSM- x x\n")]
    public void MalformedCompactInputIsRejected(string text) => Assert.Throws<FormatException>(() => CompactFormat.Read(text));

    [Fact]
    public void MissingTargetsAndUnknownVocabulariesAreRejected()
    {
        var corpus = ConversionTests.Full();
        var c = corpus.Annotations.Single(a => a.Id == "c795");
        c.Args[1] = "missing"; c.Args[3] = "magic";
        var errors = Validator.Errors(corpus);
        Assert.Contains(errors, e => e.Contains("invalid reference"));
        Assert.Contains(errors, e => e.Contains("unknown operation"));
    }

    [Fact]
    public void DuplicateAnnotationIdsAreRejected()
    {
        var corpus = ConversionTests.Full();
        corpus.Annotations.Add(corpus.Annotations[0]);
        Assert.Contains(Validator.Errors(corpus), e => e.Contains("Duplicate annotation"));
    }

    [Fact]
    public void SourceChecksumAndSurfaceChangesAreDetected()
    {
        var corpus = ConversionTests.Full();
        var raw = File.ReadAllBytes(ConversionTests.At("data/source/75-1Ti-morphgnt.txt"));
        Assert.Throws<FormatException>(() => MorphGntFormat.VerifySource(corpus, raw.Concat(Encoding.UTF8.GetBytes("\n")).ToArray()));
        corpus.Words[0].Surface = "wrong";
        Assert.Throws<FormatException>(() => MorphGntFormat.VerifySource(corpus, raw));
    }

    [Fact]
    public void TeiStructuralDisagreementIsRejected()
    {
        var xml = XDocument.Parse(TeiFormat.Write(ConversionTests.Full()));
        XNamespace n = "http://www.tei-c.org/ns/1.0";
        xml.Descendants(n + "link").Single(e => (string?)e.Attribute(XNamespace.Xml + "id") == "c795")
            .SetAttributeValue("target", "#w795 #u789_793 #u794_801");
        Assert.Throws<FormatException>(() => TeiFormat.Read(xml.ToString()));
    }

    [Fact]
    public void UnrecognizedTeiContentIsNotSilentlyDropped()
    {
        var xml = XDocument.Parse(TeiFormat.Write(ConversionTests.Full()));
        XNamespace n = "http://www.tei-c.org/ns/1.0";
        xml.Descendants(n + "body").Single().Add(new XElement(n + "note", "An external editorial note."));
        Assert.Throws<FormatException>(() => TeiFormat.Read(xml.ToString()));
    }

    [Fact]
    public void ExternalEntitiesAreDisabled()
    {
        var xml = "<!DOCTYPE TEI [<!ENTITY x SYSTEM 'file:///nonexistent'>]><TEI xmlns='http://www.tei-c.org/ns/1.0'>&x;</TEI>";
        Assert.Throws<System.Xml.XmlException>(() => TeiFormat.Read(xml));
    }

    [Theory]
    [InlineData("{\"format\":\"GNTC/1\",\"format\":\"GNTC/2\"}")]
    [InlineData("{\"metadata\":{},\"words\":[],\"annotations\":[]}")]
    public void DuplicateJsonFieldsAndMissingVersionsAreRejected(string json) => Assert.Throws<FormatException>(() => JsonFormat.Read(json));

    [Fact]
    public void ReferenceMapsCannotChangeTheVerseIdentity()
    {
        var corpus = ConversionTests.Full();
        corpus.Words[0].Verse = "1Ti.9.9";
        Assert.Contains(Validator.Errors(corpus), e => e.Contains("BCV disagree"));
    }

    [Fact]
    public void ImplicitConnectionsCannotClaimAnOvertMarkerInstruction()
    {
        var corpus = ConversionTests.Full();
        corpus.Annotations.Single(a => a.Id == "cImplicit4_11").Args[3] = "support";
        Assert.Contains(Validator.Errors(corpus), e => e.Contains("implicit relation"));
    }
}
