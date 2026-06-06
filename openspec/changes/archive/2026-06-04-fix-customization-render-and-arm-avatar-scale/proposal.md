## Why

The avatar customization panel renders customization changes (hair, shirts, hats, glasses, hand items) only when combined with unrelated interactions — expression buttons or sliders — due to Blazor's `ShouldRender` optimization skipping child components with unchanged parameters. Additionally, the arm upper segment appears transparent and the avatar is disproportionately small within the central panel.

## What Changes

- **Refactor rendering data flow**: Customization properties (hair style/color, shirt, hat, glasses, hand items) flow as explicit `[Parameter]` values from `AvatarRenderer` to child components, replacing `@inject AvatarState` reads in `AvatarTorso`, `AvatarHead`, and `AvatarArm`
- **Fix arm transparency**: Remove `background: inherit` from `.arm-upper` and set explicit `background-color: var(--skin-color)` with shirt-specific CSS overrides for long-sleeve and team jerseys
- **Scale avatar 2×**: Apply `scale(2)` to the body-wrapper transform with `transform-origin: center bottom` so the avatar occupies more of the central panel

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `avatar-rendering`: Child components (`AvatarTorso`, `AvatarHead`, `AvatarArm`) now receive all customization state as explicit `[Parameter]` values instead of reading from `@inject AvatarState`. Arm upper segment uses explicit background color instead of inheriting. Avatar body-wrapper applies `scale(2)` + `transform-origin: center bottom` for larger display.
- `avatar-customization`: Customization changes are now guaranteed to trigger re-render regardless of whether other parameters (angles, expression) also change, because customization data is passed as Blazor parameters.

## Impact

- **Components**: `AvatarRenderer.razor`, `AvatarTorso.razor`, `AvatarHead.razor`, `AvatarArm.razor` — all modified to pass/receive parameters
- **CSS**: `avatar.css` — arm upper styling, body-wrapper transform-origin
- **No breaking changes** to API, WebSocket protocol, or service layer
