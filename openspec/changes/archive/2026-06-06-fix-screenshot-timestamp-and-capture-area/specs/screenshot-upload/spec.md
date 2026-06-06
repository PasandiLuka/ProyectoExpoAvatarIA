## MODIFIED Requirements

### Requirement: Screenshot capture from avatar
The system SHALL capture a PNG screenshot of the full avatar including torso, head, arms, and handheld items using html2canvas at 2x scale with ET12 watermark overlay.

#### Scenario: Successful capture
- **WHEN** user clicks "Sacar Foto", countdown completes, and html2canvas renders successfully
- **THEN** a base64-encoded PNG data URL containing the complete avatar is returned

#### Scenario: Full avatar visible
- **WHEN** html2canvas captures the avatar panel
- **THEN** the resulting image includes the torso, head, both arms, and any equipped handheld items

#### Scenario: Capture failure
- **WHEN** html2canvas throws an error (e.g., panel element not found)
- **THEN** the UI displays an error message and re-enables the "Sacar Foto" button

### Requirement: Argentina timezone in filename
The system SHALL use Argentina Standard Time (UTC-3) when generating screenshot filenames.

#### Scenario: Filename uses Argentina time
- **WHEN** a screenshot is taken at 23:00 Argentina time on June 5, 2026
- **THEN** the filename is `avatar-20260605-230000.png`
