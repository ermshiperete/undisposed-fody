# Change Log

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

<!-- Available types of changes:
### Added
### Changed
### Fixed
### Deprecated
### Removed
### Security
-->

## [Unreleased]

### Changed

- deal with objects that are different but produce the same hash
- update to Fody 6 (#6)

### Fixed

- crash trying to unregister an object

## [3.0.0] - 2018-06-06

### Changed

- Adjust to Fody version 3
- Move to .NET 4.6 (instead of 4.5)

### Removed

- Support for .NET 4.0