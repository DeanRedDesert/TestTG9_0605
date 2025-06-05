<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet
  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
  xmlns:gd="http://www.igt.com/worldgame/common/xml/gamedata"
  exclude-result-prefixes="msxsl"
>
  <xsl:output method="xml" indent="yes"/>

  <xsl:variable name="defaultMultiplyConsolationPayByDenom">false</xsl:variable>

  <!-- Add the default MultiplyConsolationPayByDenom element after the ConsolationPay element. -->
  <xsl:template match="gd:ConsolationPay">
    <xsl:copy-of select="."/>
    <xsl:text>&#xa;      </xsl:text>
    <xsl:element name="MultiplyConsolationPayByDenom" namespace="http://www.igt.com/worldgame/common/xml/gamedata">
      <xsl:value-of select="$defaultMultiplyConsolationPayByDenom"/>
    </xsl:element>
  </xsl:template>

  <!-- Identity transform, preserves everything else. -->
  <xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()"/>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>