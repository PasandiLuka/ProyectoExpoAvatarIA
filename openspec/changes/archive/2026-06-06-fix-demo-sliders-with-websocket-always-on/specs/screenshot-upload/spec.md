## MODIFIED Requirements

### Requirement: WebSocket always connected
The system SHALL maintain a persistent WebSocket connection from app startup, independent of camera tracking state. Demo slider values SHALL be applied whenever real tracking landmarks are not being actively received, regardless of WebSocket connection state.

#### Scenario: Screenshot upload in demo mode
- **WHEN** tracking is not active (demo mode) and user takes a screenshot
- **THEN** the screenshot is uploaded to Drive via the existing WebSocket connection

#### Scenario: Screenshot upload with tracking active
- **WHEN** tracking is active and user takes a screenshot
- **THEN** the screenshot is uploaded to Drive via the existing WebSocket connection

#### Scenario: Button disabled when WebSocket is not connected
- **WHEN** the WebSocket connection is lost (server down, network error)
- **THEN** the "Sacar Foto" button is disabled and shows "Sin conexion"

#### Scenario: Button re-enabled on reconnection
- **WHEN** the WebSocket reconnects after being disconnected
- **THEN** the "Sacar Foto" button is re-enabled automatically

#### Scenario: Demo sliders work with WebSocket connected but tracking off
- **WHEN** WebSocket is connected and tracking is not active (no landmarks being received)
- **THEN** moving demo sliders updates the avatar position in real time

#### Scenario: Demo sliders ignored when landmarks are active
- **WHEN** tracking is active and landmarks are being received
- **THEN** demo slider values are ignored and real tracking positions are used instead
