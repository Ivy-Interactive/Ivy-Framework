# Ivy Framework Weekly Notes - Week of 2025-12-12

## Improvements

### React 19 Upgrade

The framework has been updated to use React and ReactDOM 19.2.3, bringing the latest performance improvements and features from the React ecosystem.

### AOT Compilation Support

We've added support for Ahead-of-Time (AOT) compilation with YAML serialization support. This includes:

* Static YAML context for AOT serialization
* Updated project files for AOT compatibility and trimming
* Refactored `FilterParserAgent` to use `FilterYamlContext`

### Metric Views Design

The design of Metric Views in Cards has been updated for better visual hierarchy and consistency. This includes centered headers and improved metric placement.

### Input Widget Improvements

We've improved handling of nullable types in input widgets:

* `IAnyInput` now properly handles nullable values
* Text inputs now support clearing values (setting to null) via a clear button
* Improved styling for invalid states and clear buttons

### Core Improvements

* **Widget Tree Hashing**: Added debug helpers to calculate and verify widget tree hashes to detect duplicate IDs and sync issues.
* **Affixes**: Refactored Affixes (renamed back to PrefixSuffix) and improved API consistency.
* **Grid**: Added `Height` property to `GridView` for better layout control.

## Documentation

Significant documentation updates have been made:

* Updated `HowIvyWorks.md` and installation guides
* Removed outdated diagrams and examples
* Simplified internal documentation links
* Added detailed package component descriptions

## Bug Fixes

* **Charts**: Removed automatic alphabetical sorting in PivotTable.
* **Card**: Fixed nullable Card title handling.
* **Rendering**: Fixed padding rendering for non-Chrome apps.
* **Kanban**: Fixed examples in documentation.
* **CodeInput**: Fixed examples in documentation.

## What's Changed

* deps: Update React and ReactDOM to 19.2.3. (#1865) in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1865>
* (Card): Update Design of Metric Views (#1860) in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1860>
* feat: remove redundant theming system (#1864) in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1864>
* Refactored Affixes (#1802) in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1802>
* [charts]: remove automatic alphabetical sorting in PivotTable (#1871) in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1871>
* Feat/docs updates mikael (#1867) in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1867>
* [Kanban]: fix examples in docs (#1877) in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1877>
* [CodeInput]: fix example in docs (#1878) in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1878>
* [GridView]: add Height (#1876) in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1876>
* feat(filters,aot): add AOT support and YAML serialization (#18xx)
* docs improvements batch 2 (#1882) in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1882>
* refactor(aot): improve trimming/AOT support and simplify config (#18xx)
* (toc): generate compile time instead of frontend runtime (#1883) in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1883>
* fix(card,frontend): handle nullable Card title and clean peer flags (#18xx)
* Simplify bug report template by removing fields (#18xx)
* feat(core): add widget tree hashing and duplicate ID checks (#18xx)
* feat(samples): update Kanban and product demos with interactivity (#18xx)
* Added description for allowing LLMs in for querying (#1899) in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1899>
* (IAnyInput): handle nullable (#1808) in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1808>
* docs: improvements by Mikael vol3 (#1896) in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1896>
* (chrome): fix padding rendering for none-chrome apps (#1873) in <https://github.com/Ivy-Interactive/Ivy-Framework/pull/1873>
* chore: Reduce commit history download to 7 days and update example paths in prompt.
