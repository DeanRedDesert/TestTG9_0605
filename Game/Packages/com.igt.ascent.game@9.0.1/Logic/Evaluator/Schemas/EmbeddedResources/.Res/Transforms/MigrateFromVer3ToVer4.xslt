<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet
  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
  xmlns:gd="http://www.igt.com/worldgame/common/xml/gamedata"
  exclude-result-prefixes="msxsl"
>
   <xsl:output method="xml" indent="yes"/>

   <!-- Identity transform, preserves everything else. -->
   <xsl:template match="@* | node()">
      <xsl:copy>
         <xsl:apply-templates select="@* | node()"/>
      </xsl:copy>
   </xsl:template>
</xsl:stylesheet>
