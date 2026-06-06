## ADDED Requirements

### Requirement: Pokemon image hand items
The system SHALL support hand items rendered from PNG/SVG image files stored in `wwwroot/images/pokemon/`. Each file SHALL generate a corresponding hand item button. Images SHALL be constrained to 50×50 pixels with `object-fit: contain`.

#### Scenario: Load Pokemon image
- **WHEN** user selects a Pokemon hand item
- **THEN** the image renders in the hand at 50×50px, centered within the item bounding box

### Requirement: Pokemon item upright behavior
Pokemon image items SHALL include the `stay-upright` CSS class so they compensate arm rotation identically to other hand items.

#### Scenario: Pokemon stays upright
- **WHEN** arm moves during tracking
- **THEN** the Pokemon image maintains its upward orientation

### Requirement: Pokemon folder auto-discovery
The system SHALL scan the `wwwroot/images/pokemon/` folder at build time and generate one button per image file found. If the folder is empty or missing, no Pokemon buttons SHALL appear.

#### Scenario: No Pokemon images present
- **WHEN** `wwwroot/images/pokemon/` contains no images
- **THEN** the Pokemon section in the hand items category renders empty (no empty buttons)
