## MODIFIED Requirements

### Requirement: Expression detection via scoring system
The system SHALL detect facial expressions using a weighted multi-signal scoring system where each expression (surprise, smile, angry) receives a score [0,1] based on independent facial signals, and the highest-scoring expression above a 0.4 confidence threshold is selected.

#### Scenario: Surprise detected correctly
- **WHEN** mouth is wide open AND eyebrows are raised
- **THEN** surprise score exceeds 0.4 and is selected over neutral

#### Scenario: Smile detected correctly  
- **WHEN** mouth corners are pulled wide AND lip corners rise above mouth center
- **THEN** smile score exceeds 0.4 and is selected

#### Scenario: Angry detected correctly
- **WHEN** eyebrows are close together AND mouth is tightened
- **THEN** angry score exceeds 0.4 and is selected

#### Scenario: Neutral when no expression is clear
- **WHEN** no expression score exceeds 0.4
- **THEN** the system returns "neutral"

#### Scenario: Multiple expressions score above threshold
- **WHEN** more than one expression scores above 0.4
- **THEN** the expression with the highest score is selected
