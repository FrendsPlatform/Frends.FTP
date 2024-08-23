# Changelog

## [1.2.0] - 2024-08-23
### Changed
- Updated the Newtonsoft.Json package to version 13.0.3.

## [1.1.3] - 2024-01-30
### Fixed
- Fixed bug when using wildcard filemask and no source files were found.

## [1.1.2] - 2024-01-16
### Improved
- Improved Operations log by adding more logging steps.

## [1.1.1] - 2024-01-04
### Added
- Added setup for FtpClient.ReadTimeout, FtpClient.DataConnectionConnectTimeout and FtpClient.DataConnectionReadTimeout which were all defaulting to 15 seconds.

## [1.1.0] - 2023-09-12
### Added
- Added search for local certificates from machine certification store.
- [Breaking] Added parameters ClientCertificationName and ClientCertificationThumbprint for exclusive search of client certification.

## [1.0.4] - 2023-08-08
### Changed
- Moved client.EncryptionMode setting to be done if UseFTPS is enabled.

## [1.0.3] - 2022-11-22
### Changed
- Connection.BufferSize documentation update. Value is set as bytes instead of KBs.
- Removed Connection.ClientCertificatePath's default value.

## [1.0.2] - 2022-08-25
### Fixed
- Fixed RestoreModified method to use the whole path of the destination file and not just the name.
- Fixed logging of successful transfer to use the name of the file and not reference of FileItem class.

## [1.0.1] - 2022-06-15
### Fixed
- Added DisplayFormat annotation to the Connection ClientCertificatePath input field.

## [1.0.0] - 2022-06-06
### Changed
- Initial implementation
