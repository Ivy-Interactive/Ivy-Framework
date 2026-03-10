# Review Checklist: Scale to Density Rename

## Objectives
- [x] Rename the `Scale` enum to `Density` to avoid conceptual ambiguity.
- [x] Update all APIs, properties, and extension methods in Ivy-Framework from `Scale` to `Density`.

## Areas Touched
- [x] **Core Enum:** `src/Ivy/Shared/Density.cs`
- [x] **Widgets & Builders:** 
  - `WidgetBase.cs`
  - `Sheet.cs`, `Dialog.cs`
  - `FormBuilder.cs`, `FormView.cs`
  - `AsyncSelectInput.cs`, `Field.cs`
- [x] **Views:** 
  - `Text.cs`, `RichText.cs`
  - `TableBuilder.cs`, `DetailsBuilder.cs`
- [x] **Apps/Samples:** Applied script to replace all variants in `src/Ivy.Samples.Shared/Apps/`.
- [x] **Documentation:** Replaced `Scale` with `Density` in `src/Ivy.Docs.Shared/` properties and code examples.
- [x] **General Docs:** Updated `AGENTS.md` terminology.
- [x] **Refactor Prompt:** Created migration guide at `src/.releases/Refactors/Upcoming/Scale-to-Density.md`.

## Manual Verification Required
- [ ] Review Ivy Framework's `FormBuilder` and `FormView` integration. Ensure all input densities render at the correct sizes.
- [ ] Spin up `Ivy.Samples.Shared/Apps` locally and verify the UI for `ScaleDemo` (now `DensityDemo`) renders correctly.
- [ ] Double-check that chart `Scales` (AxisScales, etc.) still function as expected on line charts, as they were excluded from the rename. 
- [ ] Review documentation outputs built from `src/Ivy.Docs.Shared` for any lingering references to old `Scale` APIs.
