## MODIFIED Requirements

### Requirement: Screenshot capture from avatar
The system SHALL capture a PNG screenshot of the full avatar including torso, head, arms, and handheld items using html2canvas at 2x scale with ET12 watermark overlay, from the `.avatar-container` element excluding UI elements.

#### Scenario: Full avatar visible without UI
- **WHEN** html2canvas captures `.avatar-container`
- **THEN** the resulting image includes torso, head, both arms, and equipped items, and does NOT include the "Avatar Render" label or any UI elements

#### Scenario: Watermark centered
- **WHEN** the ET12 watermark is applied to the screenshot
- **THEN** the watermark is centered both horizontally and vertically on the canvas

#### Scenario: Accessories render correctly
- **WHEN** the avatar has handheld items with CSS gradients (e.g., pochoclos) and SVG items (e.g., sable laser)
- **THEN** all items render with their correct colors, structures, and gradients in the screenshot
