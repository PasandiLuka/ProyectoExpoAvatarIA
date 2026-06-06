## ADDED Requirements

### Requirement: Screenshot capture from avatar
The system SHALL capture a PNG screenshot of the avatar wrapper element using html2canvas at 2x scale with ET12 watermark overlay.

#### Scenario: Successful capture
- **WHEN** user clicks "Sacar Foto", countdown completes, and html2canvas renders successfully
- **THEN** a base64-encoded PNG data URL is returned to the caller

#### Scenario: Capture failure
- **WHEN** html2canvas throws an error (e.g., avatar-wrapper not found)
- **THEN** the UI displays an error message and re-enables the "Sacar Foto" button

### Requirement: Configurable countdown before capture
The system SHALL display a configurable countdown (default 5 seconds) between clicking "Sacar Foto" and the actual screenshot capture.

#### Scenario: Countdown with default delay
- **WHEN** user clicks "Sacar Foto" with delay set to 5 seconds
- **THEN** the button shows a countdown "Sacar Foto (5)... (4)... (1)" and captures after 5 seconds

#### Scenario: Countdown with zero delay
- **WHEN** user clicks "Sacar Foto" with delay set to 0 seconds
- **THEN** the screenshot is captured immediately with no visible countdown

#### Scenario: Cancel countdown with second click
- **WHEN** user clicks "Sacar Foto" a second time during the countdown
- **THEN** the countdown is cancelled, the button returns to "Sacar Foto", and the UI is re-enabled

#### Scenario: Delay control in hamburger menu
- **WHEN** user opens the hamburger menu (scale/delay panel)
- **THEN** a slider labeled "Delay foto" allows setting the countdown from 0 to 10 seconds in 1-second increments

#### Scenario: Delay control disabled in demo mode
- **WHEN** tracking is not active (demo mode)
- **THEN** the delay slider is disabled and shows the current delay value as read-only

### Requirement: WebSocket always connected
The system SHALL maintain a persistent WebSocket connection from app startup, independent of camera tracking state.

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

### Requirement: Drive upload feedback
The system SHALL provide clear feedback for screenshot upload results.

#### Scenario: Successful upload
- **WHEN** the server returns `screenshot_result` with `success: true`
- **THEN** the UI displays "Foto subida" in green and the Drive URL is available

#### Scenario: Upload failure from server
- **WHEN** the server returns `screenshot_result` with `success: false`
- **THEN** the UI displays "Error al subir la foto" in red and re-enables the button

### Requirement: UI timeout protection
The system SHALL prevent indefinite UI freeze with a 30-second safety timeout when waiting for Drive upload response.

#### Scenario: Server never responds
- **WHEN** 30 seconds elapse after sending screenshot without receiving `screenshot_result`
- **THEN** the UI displays "Timeout al subir la foto" in red and re-enables the button

#### Scenario: Server responds within timeout
- **WHEN** `screenshot_result` is received before the timeout expires
- **THEN** the timeout is cancelled and the result is processed normally

### Requirement: Error resilience
The system SHALL ensure the "Sacar Foto" button is always re-enabled after any error condition.

#### Scenario: Any error during capture or upload
- **WHEN** any error occurs (capture, connection, upload, timeout)
- **THEN** a red error message is shown below the button and the button is re-enabled

#### Scenario: SendScreenshot returns false
- **WHEN** `SendScreenshot` returns `false` (WebSocket not connected)
- **THEN** the UI displays "Error de conexion" in red and re-enables the button
