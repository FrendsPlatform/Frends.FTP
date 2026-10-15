# Changelog

## [2.0.0] - 2026-10-15
### Changed
- [Breaking] The Task now targets .NET 8 only.
- [Breaking] Task parameters are now in the order Input, Connection, Options. Existing Task usages must be re-configured.
- [Breaking] The result now includes `Success` and `Error` properties.
### Added
- Added `Options` with `ThrowErrorOnFailure` and `ErrorMessageOnFailure` to control whether failures throw an error or are returned in the result.

## [1.1.0] - 2024-08-23
### Changed
- Updated the Newtonsoft.Json package to version 13.0.3.

## [1.0.0] - 2023-08-03
### Added
- Initial implementation