# Changelog

## [2.3.0] - 2026-10-26
### Changed
- Aligned the Task with the current Frends harmonization standards. No changes to Task behavior.

## [2.2.0] - 2026-10-15
### Changed
- Updated the Task to target .NET 8.
- The Task result now includes Success and Error properties.

### Added
- Added Options with ThrowErrorOnFailure and ErrorMessageOnFailure. When ThrowErrorOnFailure is false, errors are returned in the result instead of being thrown.

## [2.1.0] - 2024-08-23
### Changed
- Updated the Newtonsoft.Json package to version 13.0.3.

## [2.0.0] - 2022-12-20
### Fixed
- Fixed documentation
- Fixed Tests and change the Tests to use docker container.
- Fixed error handling to throw specific errors from the library and not custom.

### Added
- Created Connection class for connection parameters.
- Added FTPS options.
- Added FtpMode and FtpsSslMode
- Added feature which enables the Task to get files, directories or both.
- Added feature which enables the Task to get files from subdirectories.
- Added possibility to use certificates.

### Changed
- Changed return object to a List of FileItems.
- Renamed most of the parameters.
- Changed the target framework to .net6.
- Changed the Task to async method.

## [1.0.0] - 2022-05-18
### Added
- Initial implementation