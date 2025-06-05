<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet
  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
  xmlns:gd="http://www.igt.com/worldgame/common/xml/gamedata"
  exclude-result-prefixes="msxsl"
>
  <xsl:output method="xml" indent="yes"/>

  <!-- These templates remove ProgressiveLevel and Trigger elements from within the PokerWinCategory only. -->
  <xsl:template match="/gd:Paytable/gd:PokerPaytableSection/gd:PokerWinCategory/gd:Win/gd:Trigger" />
  <xsl:template match="/gd:Paytable/gd:PokerPaytableSection/gd:PokerWinCategory/gd:Win/gd:ProgressiveLevel" />

  <!-- This template removes RequiredBetOnPatternMax from WinAmounts. -->
  <xsl:template match="*/gd:RequiredBetOnPatternMax" />

  <!-- This template reduces any BetAmountRequired that contains a range to just the lower bounds of the range. -->
  <xsl:template match="gd:BetAmountRequired">
    <xsl:element name="BetAmountRequired" namespace="http://www.igt.com/worldgame/common/xml/gamedata">
      <xsl:value-of select="substring-before(concat(.,'-'), '-')"/>
    </xsl:element>
  </xsl:template>

  <!-- This template reduces any requiredTotalBet that contains a range to just the lower bounds of the range. -->
  <xsl:template match="@requiredTotalBet" >
    <xsl:attribute name="requiredTotalBet">
      <xsl:value-of select="substring-before(concat(.,'-'), '-')"/>
    </xsl:attribute>
  </xsl:template>

  <!-- Identity transform, preserves everything else. -->
   <xsl:template match="@* | node()">
      <xsl:copy>
         <xsl:apply-templates select="@* | node()"/>
      </xsl:copy>
   </xsl:template>

</xsl:stylesheet>
