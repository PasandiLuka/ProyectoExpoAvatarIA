## MODIFIED Requirements

### Requirement: Screenshot capture from avatar
The system SHALL temporarily disable CSS 3D properties (`perspective`, `transform-style`) before capturing with html2canvas and restore them after capture, producing a flat 2D image with correct colors, opacity, gradients, and SVG rendering.

#### Scenario: Avatar captured with full opacity
- **WHEN** html2canvas captures `.avatar-container` with 3D disabled
- **THEN** the resulting image shows the avatar with 100% opacity (not translucent)

#### Scenario: Accessories render correctly in flat mode
- **WHEN** the avatar has CSS-gradient handheld items (pochoclos) and SVG items (sable laser) equipped
- **THEN** all items render with correct colors and structures in the flat screenshot

#### Scenario: No UI elements in capture
- **WHEN** html2canvas captures `.avatar-container`
- **THEN** the "Avatar Render" label and other UI elements are not present in the image

#### Scenario: 3D restored after capture
- **WHEN** html2canvas completes (success or error)
- **THEN** the CSS 3D properties are restored to their original values
