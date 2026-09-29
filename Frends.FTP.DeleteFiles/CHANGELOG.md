# Changelog

## [2.0.0] - 2026-09-29
### Changed
- Target framework changed to .NET 8.
- Added `Options` parameter with `ThrowErrorOnFailure` and `ErrorMessageOnFailure` properties for controlling error handling behavior.
- `Result` now includes `Success` and `Error` properties. On failure, if `ThrowErrorOnFailure` is set to `false`, the Task returns a failed `Result` instead of throwing an exception.

## [1.0.0] - 2024-02-20
### Changed
- Initial implementation
