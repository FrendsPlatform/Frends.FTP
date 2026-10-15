# Changelog

## [2.0.0] - 2026-10-15
### Changed
- The Task now targets .NET 8.
- Parameter order changed to Input, Connection, Options and CancellationToken. Existing Task usages need to be reconfigured.
- The Task result now includes `Success` and `Error` properties.

### Added
- New `Options` parameter with `ThrowErrorOnFailure` and `ErrorMessageOnFailure` for controlling error handling. By default the Task still throws on failure.

## [1.1.0] - 2024-08-23
### Changed
- Updated the Newtonsoft.Json package to version 13.0.3.

## [1.0.0] - 2023-08-04
### Added
- Initial implementation