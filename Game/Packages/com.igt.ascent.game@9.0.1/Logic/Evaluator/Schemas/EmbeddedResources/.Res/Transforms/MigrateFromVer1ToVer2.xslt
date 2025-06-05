<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet
  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
  xmlns:gd="http://www.igt.com/worldgame/common/xml/gamedata"
  exclude-result-prefixes="msxsl"
  >

  <!--Define default values.-->
  <xsl:variable name="defaultMinPayback" select="0"/>
  <xsl:variable name="defaultMaxPayback" select="0"/>

  <xsl:output method="xml" indent="yes"/>

  <!--Match the root paytable element, and add the new paytable abstract.-->
  <xsl:template match="/gd:Paytable">
    <!--Copy the element, but not the attributes (namespaces are not considered elements, and are copied by the default template below.)-->
    <xsl:copy>
      <xsl:element name="Abstract" namespace="http://www.igt.com/worldgame/common/xml/gamedata">
        <!--This moves the gameID attribute onto the Abstract element.-->
        <xsl:copy-of select="@gameID"/>
        <!--If there is a min payback percentage attribute use its value, otherwise use the default value.-->
        <xsl:element name="MinPaybackPercentage" namespace="http://www.igt.com/worldgame/common/xml/gamedata">
          <xsl:choose>
            <xsl:when test="@minPaybackPercentage">
              <xsl:value-of select="@minPaybackPercentage"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$defaultMinPayback"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:element>
        <!--If there is a max payback percentage attribute use its value, otherwise use the default value.-->
        <xsl:element name="MaxPaybackPercentage" namespace="http://www.igt.com/worldgame/common/xml/gamedata">
          <xsl:choose>
            <xsl:when test="@maxPaybackPercentage">
              <xsl:value-of select="@maxPaybackPercentage"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$defaultMaxPayback"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:element>
      </xsl:element>
      <xsl:copy-of select="*"/>
    </xsl:copy>
  </xsl:template>

  <!--This is the default template, which matches all unmatched elements and attributes.  It copies them over intact.-->
  <xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()"/>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>