# Changelog

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/) and [Ascent SDK ChangeLog Process](https://igt-developer-docs.atlassian.net/wiki/spaces/AS/pages/81161431/Ascent+SDK+ChangeLog+Process).

## [7.1.0] - 2022-06-28

### New Features

- PFR-2061: Added support for reporting Min Playable Credit Balance.

### End of Life Support

- AS-9553 The interface **IXPaytableGameDataInspectionServiceHandler** and its implementation **XPaytableGameDataInspectionServiceHandlerBase** have been removed, as the interface is not used in any of the game reporting services. If a game uses this interface or base class for any reason, it could switch to use IGameDataInspectionServiceHandler and GameDataInspectionServiceHandlerBase instead.

## [7.0.0] - 2022-01-13

Initial UPM package release.
