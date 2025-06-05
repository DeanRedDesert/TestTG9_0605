**Change Type**: Enhancement<br/>
**Required**: Yes<br/>
**Description**: <br/>
This patch adds a new hybrid game identity to the template. This new game identity borrows features both from the ANZ and Global game identities.<br/>
We have tried to minimise the changes in the existing game identities but it was impossible to prevent all changes due to localisation.

**Additional Information:**
* All uses of Myriad PRO font have been replaced by Franklin Gothic
* Default chinese font is now NOTO Sans CJKsc Bold
* Entirely new AnzHybrid game identity content in Assets\Game\GameIdentity\AnzHybrid
* New prefab instances at GameIdentity\GiMain\GiAnzHybridMain and GameIdentity\GiDpp\GiAnzHybridDpp

The new ANZ Hybrid GI has been selected as default when running on the Ascent foundation. To change what GI your game uses in its global market you must open the Startup scene, find the GamingSystem object and change the preferred global GI:
![](0016-%20StartupSceneGlobalGiConfig.png)