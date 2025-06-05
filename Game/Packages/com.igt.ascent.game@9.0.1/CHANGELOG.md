# Changelog

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/) and [Ascent SDK ChangeLog Process](https://igt-developer-docs.atlassian.net/wiki/spaces/AS/pages/81161431/Ascent+SDK+ChangeLog+Process).

## [9.0.0] - 2024-03-08

### Feature Improvements

- Moved ProgressiveController interfaces to Ascent Foundation package.

## [8.0.0] - 2023-03-16

### New Features

- PFR-2207: Added support for making contributions to event based progressives in Standalone mode.

### Feature Improvements

- SS-1475: Removed the dependency to MSBuilld in Editor play.

## [7.1.0] - 2022-06-28

### New Features

- PFR-2152: The service of EnabledDenominationsWithProgressives in ProgressiveProvider can be Asynchronous now. For performance, the async service is turned off by default. The game state machine could call the following APIs to turn it on and off:
  - TurnOnAsyncUpdateForEnabledDenominationsWithProgressives
  - TurnOffAsyncUpdateForEnabledDenominationsWithProgressives
- PFR-1600: Provided a RunMode value to game logic code in the State Machine Framework so that the game logic code knows whether the game is running FastPlay etc.

### Feature Improvements

- AS-9560: Moved some Standard and Standalone specific types in the Communication.Platform namespace to Platform.Standard and Platform.Standalone, respectively. This does not affect game code.

### Bug Fixes

- AS-9575: Fixed a potential deadlock issue in concurrent games that happens when the shell is posting an event (e.g., display control state, shell tilt, transactional parcel comm) to a coplayer while that player is sending a transactional parcel comm or tilt request to the shell at the same time.

## [7.0.0] - 2022-01-13

Initial UPM package release.
