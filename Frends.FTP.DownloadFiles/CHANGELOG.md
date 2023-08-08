# Changelog

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
