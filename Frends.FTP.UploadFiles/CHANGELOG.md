# Changelog

## [2.0.0] - 2026-10-22
### Changed
- **BREAKING:** Target framework changed to net8.0 (net6.0, netstandard2.0 and net471 are no longer supported).
- **BREAKING:** Task signature changed to `UploadFiles(Input input, Connection connection, Options options, CancellationToken cancellationToken)`. The former `Source`, `Destination` and `Info` parameters are now properties of the new `Input` class (`Input.Source`, `Input.Destination`, `Input.Info`).
- **BREAKING:** `Options.ThrowErrorOnFail` renamed to `Options.ThrowErrorOnFailure`. It now defaults to `true`.
- **BREAKING:** Thrown error message prefix changed from "SFTP transfer failed" to "FTP transfer failed" (or the custom `ErrorMessageOnFailure`).
- Fixed project metadata (Company, Product, PackageProjectUrl) and added missing XML documentation.

### Added
- `Options.ErrorMessageOnFailure` for a custom error message on failure.
- `Result.Error` (`Message`, `AdditionalInfo`), set when the transfer fails and `ThrowErrorOnFailure` is false.

## [1.3.0] - 2024-12-11
### Fixed
- Fixed issue with Task not throwing even when retry failed.

## [1.2.0]
### Added
- Added check for file checksum and size.
- Added retry attempts parameter to retry the transfer if checksum or file size differs.

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
