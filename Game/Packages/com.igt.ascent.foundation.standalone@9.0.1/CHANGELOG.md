# Changelog

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/) and [Ascent SDK ChangeLog Process](https://igt-developer-docs.atlassian.net/wiki/spaces/AS/pages/81161431/Ascent+SDK+ChangeLog+Process).

## [9.0.0] - 2024-03-09

### New Features

- PFR-1459: Added Foundation simulator functions to start and stop Host Initiated AutoPlay.
- PFR-2205: Added UIs to allow configuring return values for IService methods.
- PFR-2326: Added Foundation simulator functions to request enforced failures for some game actions.

### Feature Improvements

- AS-9879: Reworked Standalone Disk Store management so that the critical data written by "current" game theme and paytable could be read by the Report object.
- Decoupled the dependency to "com.igt.ascent.game" package.

## [8.0.2] - 2024-01-16

### New Features

- AS-9893: Add additional languages to standalone LocalizationInformation.

### Bug Fixes

- AS-9870: Fixed a crash involving the Progressive Simulator when switching to GameModes of type Invalid or History.

## [8.0.0] - 2023-03-16

### New Features

- PFR-1460: Initial addition of the in Unity editor progressive simulator.
- PFR-2207: Added support for making contributions to event based progressives in Standalone mode.
- PFR-2259: Added support for a PENN requirement that higher total bets must return a higher RTP than a lesser bet.
- AS-9676: Added Support for Video Wall monitor role.

### Feature Improvements

- SS-1475: Removed the dependency to MSBuilld in Editor play.

## [7.1.0] - 2022-06-28

### New Features

- PFR-2152: Added the AvailableDenominationsWithProgressivesBroadcastEvent to IGameLib implementation.
- PFR-1600: Allowed the user to set the show environment for Standalone GameLib.
- PFR-2061: Added support for reporting the Min Playable Credit Balance.

### Bug Fixes

- PFR-2188: Fixes the issue where Standalone ConfigurationRead does not use the correct theme registry info to use as ThemeIdentifier.
- AS-9526: Fixed issue where Configuration Extension Interfaces were not working as intended in Standalone.

## [7.0.0] - 2022-01-13

Initial UPM package release.
