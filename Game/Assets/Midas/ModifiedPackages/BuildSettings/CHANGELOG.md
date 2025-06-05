# Changelog

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/) and [Ascent SDK ChangeLog Process](https://igt-developer-docs.atlassian.net/wiki/spaces/AS/pages/81161431/Ascent+SDK+ChangeLog+Process).

## [9.0.0] - 2024-03-08

### Feature Improvements

- PFR-2240: Copying SystemConfig.xml and CSIConfig.xml to build output for the standard game build type.

## [8.0.0] - 2023-03-16

### New Features

- AS-9570: Report the versions of the packages during game build process.

### Upgrade Requirements

- AS-9627: IgtGameParameters.CreateUcParameters(IgtGameParameters) has been removed. Instead, please use IgtGameParameters.CreateUcParameters(FoundationTarget) to specify the targeted UC Foundation when creating UC-configured parameters.

## [7.1.0] - 2022-06-28

### Feature Improvements

- AS-9387: SDK Build Improvements
  - Moved the Mono AOT Compile component(MonoAOT.cs) to the "com.igt.ascent.assets.build-settings" package.
  - Moved the Commandline Build component(CommandlineBuild.cs) to the "com.igt.ascent.assets.build-settings" package.
  - Mono AOT Compile: SDK will search the system PATH for LLVM 10.0.0 first before falling back to C:\LLVM10.0.0\.
  - Mono AOT Compile: Microsoft Visual Studio VC++ build tool 2017 or later is required on the machine.
    - The environment variables "VS120COMNTOOLS" and "VS140COMNTOOLS" are not required by Mono AOT Compile anymore.
  - Mono AOT Compile: Removed the "MonoAotCompile" option from GameParameters.config

### Bug Fixes

- AS-9556: Disabled AOT compile function since it causes the game to crash on loading.

## [7.0.0] - 2022-01-13

Initial UPM package release.
