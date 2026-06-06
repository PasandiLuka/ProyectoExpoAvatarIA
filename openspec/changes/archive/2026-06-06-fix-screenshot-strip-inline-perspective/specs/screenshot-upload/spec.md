## MODIFIED Requirements

### Requirement: Screenshot capture from avatar
The system SHALL temporarily strip the `perspective()` function from the `#avatar-wrapper` inline transform, disable CSS perspective properties, capture with html2canvas, and restore all values after capture. The avatar pose (rotateY, rotateZ, scale) SHALL be preserved in the flat 2D capture.

#### Scenario: Inline perspective stripped during capture
- **WHEN** html2canvas captures `.avatar-container`
- **THEN** the `perspective(800px)` function is removed from the wrapper's inline transform before capture and restored after

#### Scenario: Avatar pose preserved in flat mode
- **WHEN** the avatar has arms raised and torso rotated
- **THEN** the captured image shows the correct arm positions and torso rotation (flat, without 3D depth)

#### Scenario: 3D fully restored after capture
- **WHEN** html2canvas completes (success or error)
- **THEN** the inline transform, CSS perspective, and transform-style are all restored to their original values
