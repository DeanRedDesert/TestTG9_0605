<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet 
  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
  xmlns:gd="http://www.igt.com/worldgame/common/xml/gamedata"
  exclude-result-prefixes="msxsl"
>
  <xsl:output method="xml" indent="yes"/>

  <!--Match the Paytable element so that the gameID, minPaybackPercentage, and maxPaybackPercentage attributes can be added to it. -->
  <xsl:template match="/gd:Paytable">
    <xsl:copy>
      <!--Copy the gameID attribute from the Abstract element to the Paytable element.-->
      <xsl:copy-of select="gd:Abstract/@gameID"/>
      
      <!--Create the minPaybackPercentage attribute and copy the value of the MinPaybackPercentage element.-->
      <xsl:attribute name="minPaybackPercentage">
        <xsl:value-of select="gd:Abstract/gd:MinPaybackPercentage"/>
      </xsl:attribute>
      
      <!--Create the maxPaybackPercentage attribute and copy the value of the MaxPaybackPercentage element.-->
      <xsl:attribute name="maxPaybackPercentage">
        <xsl:value-of select="gd:Abstract/gd:MaxPaybackPercentage"/>
      </xsl:attribute>
      
      <!--Apply the defined templates to the rest of the Paytable element's contents.-->
      <xsl:apply-templates select="*"/>
    </xsl:copy>
  </xsl:template>
  
  <!--Remove any optional pick strategy attributes from PickPaytableSection elements.-->
  <xsl:template match="gd:PickPaytableSection">
    <xsl:copy>
      <xsl:copy-of select="@name"/>
      <xsl:copy-of select="*"/>
    </xsl:copy>
  </xsl:template>
  
  <!--This template will effectively remove any Abstract elements from the source document.-->
  <xsl:template match="gd:Abstract"/>

  <!--Anything that isn't matched above will be handled by this template.-->
  <xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()"/>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>
