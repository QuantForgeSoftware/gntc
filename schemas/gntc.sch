<?xml version="1.0" encoding="utf-8"?>
<schema xmlns="http://purl.oclc.org/dsdl/schematron" queryBinding="xslt2">
  <title>Supplemental GNTC/1 TEI profile assertions</title>
  <ns prefix="tei" uri="http://www.tei-c.org/ns/1.0"/>
  <pattern id="gntc-profile">
    <rule context="tei:TEI">
      <assert test="count(tei:standOff/tei:fs[@xml:id='gntc_metadata']) = 1">Exactly one GNTC metadata feature structure is required.</assert>
      <assert test="count(.//@xml:id) = count(distinct-values(.//@xml:id))">XML IDs must be unique.</assert>
    </rule>
    <rule context="tei:linkGrp[@type='connective']">
      <assert test="@targFunc='marker context scope'">Overt connections require declared marker, context, and scope roles.</assert>
    </rule>
    <rule context="tei:linkGrp[@type='implicit']">
      <assert test="@targFunc='context scope'">Implicit connections have no marker target.</assert>
    </rule>
    <rule context="tei:linkGrp[@type='connective']/tei:link">
      <assert test="count(tokenize(normalize-space(@target), '\s+')) = 3">An overt connection has three targets.</assert>
    </rule>
    <rule context="tei:linkGrp[@type='implicit']/tei:link">
      <assert test="count(tokenize(normalize-space(@target), '\s+')) = 2">An implicit connection has two targets.</assert>
    </rule>
    <rule context="tei:seg[@type='token']">
      <assert test="count(tei:w) = 1">A source-token segment contains exactly one word.</assert>
      <assert test="count(*) = count(tei:w | tei:pc)">Source-token segments contain only a word and punctuation/apparatus signs.</assert>
    </rule>
    <rule context="tei:fs[@type='gntc-annotation']">
      <assert test="tei:f[@name='record']/tei:string=('U','K','Q','C','I','A')">Annotation record kind must be recognized.</assert>
      <assert test="tei:f[@name='id']/tei:string">Annotation identity must be explicit.</assert>
    </rule>
  </pattern>
</schema>
