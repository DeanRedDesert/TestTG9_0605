# Changelog

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/) and [Ascent SDK ChangeLog Process](https://igt-developer-docs.atlassian.net/wiki/spaces/AS/pages/81161431/Ascent+SDK+ChangeLog+Process).

## [8.0.2] - 2024-01-16

### Feature Improvements

- AS-9899: Removed the trimming '\0' character in the received F2X/CSI messages from Foundation.
- AS-9911: Preserve the original callstacks when the exception is rethrown from a different thread.
- AS-9920: Make some EventArgs types serializable.
- Added ProgressiveController Interfaces to decouple the dependencies from standalone Foundation package to the Ascent Game package.

### Bug Fixes

- GMSCPL-14041: Fixed an edging-case bug that the cached game cycle state is out-of-sync with the Foundation state when the game is recovered in the 'EvaluatePending' state.

## [8.0.0] - 2023-03-16

### New Features

- PFR-2259: Added support for a PENN requirement that higher total bets must return a higher RTP than a lesser bet.

### Bug Fixes

- AS-9643: Fixed an issue where config data of Link level categories in Extensions are not reset when entering and exiting the operator menu.

## [7.1.0] - 2022-06-28

### New Features

- PFR-2152: Added AvailableDenominationsWithProgressivesBroadcastEvent to IGameLib.
- PFR-2061: Added the F2XReportGameDataInspection category version 1.3.
- PFR-2034: Added the F2XEventBasedProgressive category version 1.1.
- PFR-1600: Allowed the user to set the show environment thru IGameLibDemo.
- PFR-2061: Added support for reporting the Min Playable Credit Balance.

### Feature Improvements

- AS-9560: Moved some Standard and Standalone specific types in Communication.Platform namespace to Platform.Standard and Platform.Standalone, respectively. This does not affect game code.
- AS-9561: Added a Fill method to ICriticalDataStore to read data and set it in the given critical data block.
- AS-9413: Improved the ease of use for CriticalDataBlock and SingleCriticalData.

### Bug Fixes

- AS-9575: In order to fix a potential deadlock, added the functionality in the event queue implementations to allow the blocking PostEvent call to be ineterrupted.

## [7.0.0] - 2022-01-13

Initial UPM package release.
