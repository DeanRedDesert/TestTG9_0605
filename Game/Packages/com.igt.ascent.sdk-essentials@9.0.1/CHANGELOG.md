# Changelog

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/) and [Ascent SDK ChangeLog Process](https://igt-developer-docs.atlassian.net/wiki/spaces/AS/pages/81161431/Ascent+SDK+ChangeLog+Process).

## [9.0.0] - 2024-03-08

### New Features

- PFR-2135: Chooser metrics support

### Feature Improvements

- AS-9907: Added the Foundation Target for S1 Series.
- AS-9911: Preserve the original callstacks when the exception is rethrown from a different thread.

## [8.0.0] - 2023-03-16

### New Features

- AS-9709: Added the Foundation Target for R2 Series.

## [7.1.0] - 2022-06-28

### New Features

- PFR-2034: Added the Foundation Target for R1 Series.
- PFR-1600: Added a RunMode enum to note whether the game is running FastPlay, with or without Foundation etc.

### Feature Improvements

- AS-9575: In order to fix a potential deadlock, added the functionality in EventQueueManagement to allow the blocking PostEvent call to be ineterrupted.
- AS-9638: Make TimeSpanWatch.Now and TimeSpanWatch.TimeStampMilliseconds comparable with timestamps from the Foundation and other processes.

## [7.0.0] - 2022-01-13

Initial UPM package release.
