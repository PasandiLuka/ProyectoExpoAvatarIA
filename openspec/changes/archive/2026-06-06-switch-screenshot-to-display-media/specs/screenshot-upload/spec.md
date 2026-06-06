## MODIFIED Requirements

### Requirement: Screenshot capture from avatar
The system SHALL capture a pixel-perfect screenshot of the avatar using `getDisplayMedia` API, cropping to the avatar wrapper bounding box at 2x scale, with ET12 watermark centered on the cropped area. If `getDisplayMedia` is unavailable, the system SHALL fall back to html2canvas with explicit dimensions calculated from the avatar's bounding box.

#### Scenario: Pixel-perfect capture with getDisplayMedia
- **WHEN** `getDisplayMedia` is available and user clicks "Sacar Foto"
- **THEN** the captured image matches the on-screen avatar exactly (3D, gradients, SVG, opacity correct)

#### Scenario: Fallback to html2canvas
- **WHEN** `getDisplayMedia` is unavailable (Firefox, permission denied)
- **THEN** the system uses html2canvas with width/height matching the avatar's bounding box

#### Scenario: Stream initialized once
- **WHEN** the app starts and capture is first requested
- **THEN** the display media permission dialog appears once and the stream is reused for all subsequent captures

#### Scenario: No UI in capture
- **WHEN** the screenshot is captured and cropped to the avatar wrapper
- **THEN** the "Avatar Render" label and other UI elements are not present

#### Scenario: Watermark correctly positioned
- **WHEN** the watermark is applied
- **THEN** it is centered on the cropped avatar image
