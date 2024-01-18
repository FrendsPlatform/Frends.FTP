# Changelog

## [1.1.1] - 2024-01-18
### Added
- Added setup for FtpClient.ReadTimeout, FtpClient.DataConnectionConnectTimeout and FtpClient.DataConnectionReadTimeout which were all defaulting to 15 seconds.

## [1.1.0] - 2023-09-12
### Added
- Added search for local certificates from machine certification store.
- [Breaking] Added parameters ClientCertificationName and ClientCertificationThumbprint for exclusive search of client certification.

## [1.0.2] - 2023-08-03
### Added
- Operations log improvements.

## [1.0.1] - 2022-11-25
### Changed
- Connection.BufferSize documentation update. Value is set as bytes instead of KBs.
- Removed Connection.ClientCertificatePath's default value.
- Updated Task's return fields documentation.

## [1.0.0] - 2022-05-29
### Changed
- Initial implementation
