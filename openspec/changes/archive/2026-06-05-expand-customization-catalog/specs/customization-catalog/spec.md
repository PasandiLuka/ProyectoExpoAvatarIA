## ADDED Requirements

### Requirement: Accordion category menu
The customization panel SHALL organize items into collapsible categories. Only one category SHALL be expanded at a time. Opening a category SHALL close the previously open one.

#### Scenario: Open a category
- **WHEN** user clicks a category header button
- **THEN** that category expands showing its items and all other categories collapse

#### Scenario: Category with many items scrolls
- **WHEN** a hand items category has more than 8 items displayed
- **THEN** the category content SHALL show a vertical scrollbar with max-height of 300px

### Requirement: Shirt catalog
The system SHALL support 12 shirt styles selectable via the customization panel. Styles SHALL include: manga corta, manga larga, Boca Juniors, River Plate, Argentina, Brasil, España, Alemania, C#, Python, JavaScript, and Matrix. Each SHALL render as a CSS class on the torso element.

#### Scenario: Select C# shirt
- **WHEN** user clicks the "C#" shirt button
- **THEN** the avatar torso renders with purple background and "C#" text overlay

### Requirement: Hat catalog
The system SHALL support 10 hat styles including: none, cowboy, chef, cap, Argentina cap, viking, Mexican hat, space helmet, VR headband, and crown. Each SHALL render as a positioned element above the head.

#### Scenario: Select space helmet
- **WHEN** user clicks the "Casco Espacial" button
- **THEN** a sphere-shaped helmet appears over the avatar head

### Requirement: Glasses catalog
The system SHALL support 7 glasses styles: none, sun, googly, nerd, futurista, 3D, and monocle. The "2026" style SHALL be removed. Each SHALL render as CSS elements positioned over the eyes.

#### Scenario: Select 3D glasses
- **WHEN** user clicks the "3D" glasses button
- **THEN** one red lens and one blue lens appear over the avatar eyes

### Requirement: Hand items catalog
Each hand SHALL support 20 selectable items: none, fernet, water, cup, mate, pelota, bandera, trofeo, laptop, teclado, mouse, microfono, guitarra, sable, celular, libro, pizza, hamburguesa, pochoclos, and pokebola. Items with class `stay-upright` SHALL compensate arm rotation to remain pointing upward.

#### Scenario: Select soccer ball for left hand
- **WHEN** user selects "Pelota" for the left hand
- **THEN** a soccer ball item appears in the left hand, staying upright regardless of arm angle

### Requirement: Item rotation compensation
Hand items with CSS class `stay-upright` SHALL have their rotation dynamically compensated by JavaScript so they always point approximately 90° outward from the body, independent of shoulder and elbow angles.

#### Scenario: Upright item during arm movement
- **WHEN** tracking is active and arm angle changes
- **THEN** `stay-upright` items maintain their outward-pointing orientation
