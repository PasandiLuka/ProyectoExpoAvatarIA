# Graph Report - .  (2026-06-05)

## Corpus Check
- 64 files · ~50,459 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 413 nodes · 462 edges · 55 communities (25 shown, 30 thin omitted)
- Extraction: 85% EXTRACTED · 15% INFERRED · 0% AMBIGUOUS · INFERRED: 71 edges (avg confidence: 0.88)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- [[_COMMUNITY_Avatar Customization Panel|Avatar Customization Panel]]
- [[_COMMUNITY_Camera & Frame Management|Camera & Frame Management]]
- [[_COMMUNITY_Architecture & System Design|Architecture & System Design]]
- [[_COMMUNITY_Customization Catalog Expansion|Customization Catalog Expansion]]
- [[_COMMUNITY_Tracking Service Pipeline|Tracking Service Pipeline]]
- [[_COMMUNITY_Avatar Tracking & Coordinates|Avatar Tracking & Coordinates]]
- [[_COMMUNITY_Manual Scale Slider|Manual Scale Slider]]
- [[_COMMUNITY_Graphify Skill & Rules|Graphify Skill & Rules]]
- [[_COMMUNITY_Deployment & Infrastructure|Deployment & Infrastructure]]
- [[_COMMUNITY_CameraService Implementation|CameraService Implementation]]
- [[_COMMUNITY_Blazor Project Structure|Blazor Project Structure]]
- [[_COMMUNITY_Outline & Visual Effects|Outline & Visual Effects]]
- [[_COMMUNITY_Launch Settings|Launch Settings]]
- [[_COMMUNITY_MediaPipe Server Side|MediaPipe Server Side]]
- [[_COMMUNITY_Index Page & Renderer|Index Page & Renderer]]
- [[_COMMUNITY_Blazor App Bootstrap|Blazor App Bootstrap]]
- [[_COMMUNITY_PWA & Brand Assets|PWA & Brand Assets]]
- [[_COMMUNITY_JS Render & CSS Pipeline|JS Render & CSS Pipeline]]
- [[_COMMUNITY_Web App Manifest|Web App Manifest]]
- [[_COMMUNITY_Openspec Workflow Commands|Openspec Workflow Commands]]
- [[_COMMUNITY_Openspec Artifact Generation|Openspec Artifact Generation]]
- [[_COMMUNITY_Extraction Confidence Rules|Extraction Confidence Rules]]
- [[_COMMUNITY_Navigation Menu|Navigation Menu]]
- [[_COMMUNITY_Opencode Configuration|Opencode Configuration]]
- [[_COMMUNITY_Opencode Package Dependencies|Opencode Package Dependencies]]
- [[_COMMUNITY_Avatar State Singleton|Avatar State Singleton]]
- [[_COMMUNITY_Blazor WASM Deployment|Blazor WASM Deployment]]
- [[_COMMUNITY_Facial Expressions & Gestures|Facial Expressions & Gestures]]
- [[_COMMUNITY_Prototype UI & State|Prototype UI & State]]
- [[_COMMUNITY_Extraction ID Format|Extraction ID Format]]
- [[_COMMUNITY_Manual Scale YAML|Manual Scale YAML]]
- [[_COMMUNITY_Manual Scale Proposal|Manual Scale Proposal]]
- [[_COMMUNITY_Manual Scale Tasks|Manual Scale Tasks]]
- [[_COMMUNITY_Responsive Avatar YAML|Responsive Avatar YAML]]
- [[_COMMUNITY_Responsive Avatar Proposal|Responsive Avatar Proposal]]
- [[_COMMUNITY_Responsive Avatar Tasks|Responsive Avatar Tasks]]
- [[_COMMUNITY_Stay Upright YAML|Stay Upright YAML]]
- [[_COMMUNITY_Stay Upright Design|Stay Upright Design]]
- [[_COMMUNITY_Stay Upright Tasks|Stay Upright Tasks]]
- [[_COMMUNITY_Openspec Archive|Openspec Archive]]
- [[_COMMUNITY_Openspec Explore Stance|Openspec Explore Stance]]
- [[_COMMUNITY_Graphify Honesty Rules|Graphify Honesty Rules]]
- [[_COMMUNITY_OpenCode Considerations PDF|OpenCode Considerations PDF]]
- [[_COMMUNITY_Pokemon Eevee|Pokemon: Eevee]]
- [[_COMMUNITY_Pokemon Lapras|Pokemon: Lapras]]
- [[_COMMUNITY_Pokemon Nidoqueen|Pokemon: Nidoqueen]]
- [[_COMMUNITY_Pokemon Piplup|Pokemon: Piplup]]
- [[_COMMUNITY_Pokemon Rowlet|Pokemon: Rowlet]]
- [[_COMMUNITY_Pokemon Squirtle|Pokemon: Squirtle]]
- [[_COMMUNITY_Pokemon Treecko|Pokemon: Treecko]]
- [[_COMMUNITY_Pokemon Zapdos|Pokemon: Zapdos]]
- [[_COMMUNITY_Hooks & Claude Integration|Hooks & Claude Integration]]

## God Nodes (most connected - your core abstractions)
1. `CustomizationPanel` - 61 edges
2. `CameraPanel` - 27 edges
3. `TrackingService` - 27 edges
4. `CameraService` - 13 edges
5. `Graphify Skill` - 9 edges
6. `Task` - 9 edges
7. `handle_connection()` - 8 edges
8. `Proposal: Fix Avatar Tracking and Add Outline` - 8 edges
9. `http` - 7 edges
10. `https` - 7 edges

## Surprising Connections (you probably didn't know these)
- `Query-First Rule for Codebase Questions` --semantically_similar_to--> `Graphify Query Command (BFS/DFS Traversal)`  [INFERRED] [semantically similar]
  AGENTS.md → .opencode/skills/graphify/references/query.md
- `Expand Customization Catalog` --references--> `CustomizationPanel`  [EXTRACTED]
  openspec/changes/archive/2026-06-05-expand-customization-catalog/tasks.md → AvatarExpo/Pages/Index.razor
- `Customization Catalog Spec` --references--> `CustomizationPanel`  [EXTRACTED]
  openspec/changes/archive/2026-06-05-expand-customization-catalog/specs/customization-catalog/spec.md → AvatarExpo/Pages/Index.razor
- `Accordion UI Pattern` --rationale_for--> `CustomizationPanel`  [EXTRACTED]
  openspec/changes/archive/2026-06-05-expand-customization-catalog/design.md → AvatarExpo/Pages/Index.razor
- `LandmarkParser` --semantically_similar_to--> `extract_pose()`  [INFERRED] [semantically similar]
  AvatarExpo/Services/Models.cs → server/server.py

## Import Cycles
- None detected.

## Hyperedges (group relationships)
- **Avatar Composite View Hierarchy** — components_avatarrenderer, components_avatartorso, components_avatararm, components_avatarhead [INFERRED 0.85]
- **Pose Landmark Processing Pipeline** — server_server_extract_pose, server_server_detect_expression, server_server_extract_skeleton, services_models_landmarkparser, services_models_processedlandmarks [INFERRED 0.75]
- **Hand Item Customization System** — components_avatararm, js_avataranim, components_customizationpanel, customization_catalog_spec, pokemon_hand_items_spec [INFERRED 0.75]
- **Head Tilt Computation Approaches (Shoulders vs Nose)** — 2026_06_05_fix_avatar_tracking_and_add_outline_design_head_tilt_shoulders, 2026_06_05_fix_head_tracking_from_nose_design_nose_head_tilt, 2026_06_05_fix_head_tracking_from_nose_design_nosex_nosey [INFERRED 0.85]
- **CSS Transition Smoothing Strategy Evolution (Remove then Reinstate)** — 2026_06_05_harden_tracking_pipeline_design_css_transition_removal, 2026_06_05_js_direct_avatar_render_design_css_transition_reinstate, 2026_06_05_js_direct_avatar_render_design_js_direct_dom [INFERRED 0.85]
- **Avatar animation update() method enhancements across three openspec changes** — 2026_06_05_manual_avatar_scale_slider_design_manualscale_null_override, 2026_06_05_responsive_avatar_and_dynamic_ws_design_dynamic_scale_calculation, 2026_06_05_stay_upright_hand_items_design_rotation_compensation, 2026_06_05_manual_avatar_scale_slider_design_js_interop_direct_slider, 2026_06_05_stay_upright_hand_items_design_queryselector_loop_per_frame [INFERRED 0.95]
- **Pokemon character sprites used as avatar image options** — pokemon_eevee_png, pokemon_lapras_png, pokemon_nidoqueen_png, pokemon_piplup_png, pokemon_rowlet_png, pokemon_squirtle_png, pokemon_treecko_png, pokemon_zapdos_png [INFERRED 0.85]

## Communities (55 total, 30 thin omitted)

### Community 0 - "Avatar Customization Panel"
Cohesion: 0.04
Nodes (57): AvatarState, Notify, OnHairColorInput, OnHairHexInput, readonly, SetColorCastano, SetColorGris, SetColorNegro (+49 more)

### Community 1 - "Camera & Frame Management"
Cohesion: 0.07
Nodes (35): Frame Backpressure Fix - Design, Frame Backpressure Fix - Tasks, AvatarState, IJSRuntime, TrackingService, CalibrationOffset, CameraPanel, Calibrate (+27 more)

### Community 2 - "Architecture & System Design"
Cohesion: 0.09
Nodes (33): AvatarState Singleton with OnChange Event Pattern, Client-Side Calibration Offset System, Client-Side Transform Pipeline (Mirror + Angles + Lerp), 3-Column Grid Blazor Component Layout, CSS 3D Torso Rotation with Clamping, Fallback Demo Mode Without Camera, Heartbeat + Exponential Backoff Reconnection Protocol, JPEG Binary Upload + JSON Minified Response Format (+25 more)

### Community 3 - "Customization Catalog Expansion"
Cohesion: 0.09
Nodes (24): Expand Customization Catalog, Accordion UI Pattern for Customization Categories, Pokemon as Hand Items, stay-upright Hand Item Rotation Compensation, Fix Shirt CSS and Scale Hand Items, Hand Item Scaling 1.5x, Lightsaber Glow Effect (15x220px), Shirt Flag CSS with linear-gradient (+16 more)

### Community 4 - "Tracking Service Pipeline"
Cohesion: 0.12
Nodes (15): CameraService, IJSRuntime, CancellationToken, CancellationTokenSource, ClientWebSocket, double, int, JsonSerializerOptions (+7 more)

### Community 5 - "Avatar Tracking & Coordinates"
Cohesion: 0.13
Nodes (23): Avatar Tracking Fix and Outline Addition Change Spec, Atan2 Coordinate System, Avatar 2D, AvatarState, CSS Transform Coordinate System, Demo Mode, Contorno Negro del Avatar con filter: drop-shadow(), Sliders Demo con Handlers @oninput Explícitos (+15 more)

### Community 6 - "Manual Scale Slider"
Cohesion: 0.10
Nodes (21): Auto button to restore dynamic scale calculation, Proportional scaling of full body-wrapper (not individual parts), Hamburger menu collapse pattern for scale slider UI, Direct JS interop for slider (C# to JS, bypassing Blazor state), manualScale null-override pattern (null=auto, number=manual), Avatar scale slider range 0.5-3 with step 0.1, avatarAnim.setManualScale(value) method concept, CameraPanel scale menu with hamburger toggle (+13 more)

### Community 7 - "Graphify Skill & Rules"
Cohesion: 0.10
Nodes (20): Graphify Usage Rules in AGENTS.md, Query-First Rule for Codebase Questions, Update Graph After Code Changes, Graphify Skill, AST Extraction (Code Files, Deterministic), Graphify Full Pipeline (Detect → Extract → Build → Cluster → Analyze → Report → Viz), Semantic Extraction (LLM Subagents for Docs/Papers/Images), Graphify Add URL Subcommand (+12 more)

### Community 8 - "Deployment & Infrastructure"
Cohesion: 0.12
Nodes (18): Python MediaPipe WebSocket Server, WebSocket Server on ws://0.0.0.0:8765, Client-Server WebSocket Architecture (Phase 1), JSON Pose/Face Payload Format, MediaPipe Heavy Model, Arm Kinematics (Atan2 Angle Calculation), Mirror Effect (X Coordinate Inversion), Rest Position and Arm Rigidity Rule (+10 more)

### Community 9 - "CameraService Implementation"
Cohesion: 0.16
Nodes (9): IJSRuntime, Task, TrackingService, ValueTask, bool, DotNetObjectReference, IAsyncDisposable, JSInvokable (+1 more)

### Community 10 - "Blazor Project Structure"
Cohesion: 0.14
Nodes (11): AvatarExpo, AvatarExpo, AvatarExpo.Components, AvatarExpo.Layout, AvatarExpo.Services, net10.0, Microsoft.AspNetCore.Components.Web, Microsoft.AspNetCore.Components.Web.Virtualization (+3 more)

### Community 11 - "Outline & Visual Effects"
Cohesion: 0.18
Nodes (12): Fix Avatar Tracking and Add Outline, Avatar Black Outline via drop-shadow, Head Tilt from Shoulder Height (RSy-LSy), Explicit oninput Slider Handlers (Replacing bind:after), Shoulder Angle -90 Degree Offset, Fix Head Tracking from Nose, Head Tilt from Nose Position (Landmark 0), NoseX/NoseY Fields in ProcessedLandmarks (+4 more)

### Community 12 - "Launch Settings"
Cohesion: 0.26
Nodes (11): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, inspectUri, launchBrowser, profiles (+3 more)

### Community 13 - "MediaPipe Server Side"
Cohesion: 0.22
Nodes (8): Blazor WASM, MediaPipe, WebSocket, mediapipe 0.10.13, numpy >=1.24.0, opencv-python >=4.8.0, WebSocket, Url

### Community 14 - "Index Page & Renderer"
Cohesion: 0.33
Nodes (5): AvatarState, AvatarRenderer, Dispose, OnInitialized, route:/

### Community 15 - "Blazor App Bootstrap"
Cohesion: 0.40
Nodes (4): FocusOnNavigate, Found, Router, RouteView

### Community 16 - "PWA & Brand Assets"
Cohesion: 0.70
Nodes (5): Avatar Expo IA (Blazor WebAssembly virtual avatar exhibition app), Favicon (32x32 PNG, purple-on-silver brand icon for AvatarExpo web app), icon-192.png (PWA launcher icon), index.html (Blazor WASM host page), Tailwind color theme (principal #990066, dark backgrounds, beige text)

### Community 17 - "JS Render & CSS Pipeline"
Cohesion: 0.50
Nodes (4): CSS Transition Removal for Double Smoothing Fix, avatarAnim.js Module, CSS Transition Reactivation at 0.05s, JS Direct DOM Rendering Bypass

### Community 18 - "Web App Manifest"
Cohesion: 0.67
Nodes (4): Avatar Expo IA Brand Identity, AvatarExpo Blazor WASM PWA, favicon.png - Favicon (Browser Tab Icon), icon-192.png - PWA App Icon (192x192 PNG, 8-bit colormap)

### Community 19 - "Openspec Workflow Commands"
Cohesion: 0.67
Nodes (3): OpenSpec Apply Command, OpenSpec Explore Command, OpenSpec Propose Command

### Community 20 - "Openspec Artifact Generation"
Cohesion: 0.67
Nodes (3): Task Implementation Loop (Apply), Delta Spec Sync Assessment, Artifact Generation Flow (Propose)

### Community 21 - "Extraction Confidence Rules"
Cohesion: 0.67
Nodes (3): EXTRACTED/INFERRED/AMBIGUOUS Confidence System, Confidence Score Rubric (0.55-0.95 discrete), Semantic Similarity Edges

## Knowledge Gaps
- **202 isolated node(s):** `$schema`, `plugin`, `@opencode-ai/plugin`, `Router`, `Found` (+197 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **30 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `CustomizationPanel` connect `Avatar Customization Panel` to `Customization Catalog Expansion`, `Index Page & Renderer`?**
  _High betweenness centrality (0.109) - this node is a cross-community bridge._
- **Why does `CameraPanel` connect `Camera & Frame Management` to `CameraService Implementation`, `Customization Catalog Expansion`, `Index Page & Renderer`?**
  _High betweenness centrality (0.096) - this node is a cross-community bridge._
- **Why does `TrackingService` connect `Tracking Service Pipeline` to `Camera & Frame Management`, `CameraService Implementation`?**
  _High betweenness centrality (0.060) - this node is a cross-community bridge._
- **Are the 2 inferred relationships involving `CustomizationPanel` (e.g. with `AvatarArm.razor` and `AvatarHead.razor`) actually correct?**
  _`CustomizationPanel` has 2 INFERRED edges - model-reasoned connections that need verification._
- **Are the 2 inferred relationships involving `CameraPanel` (e.g. with `avatarAnim.js` and `camera.js`) actually correct?**
  _`CameraPanel` has 2 INFERRED edges - model-reasoned connections that need verification._
- **What connects `$schema`, `plugin`, `@opencode-ai/plugin` to the rest of the system?**
  _226 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Avatar Customization Panel` be split into smaller, more focused modules?**
  _Cohesion score 0.03508771929824561 - nodes in this community are weakly interconnected._