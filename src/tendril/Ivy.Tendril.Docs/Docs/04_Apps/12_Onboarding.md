---
searchHints:
  - onboarding
  - start
  - setup
icon: Compass
---

# Onboarding

<Ingress>
The Onboarding App is a critical unskippable sequence launched when Tendril observes an empty or corrupted `TENDRIL_HOME` system directory.
</Ingress>

## Guided Initialization

Starting Tendril for the very first time directs the graphical interface payload directly to the Onboarding pipeline instead of the Dashboard. This avoids frustrating command-line errors concerning missing YAML keys.

You will be prompted interactively to assign:

1. **Working Context** (`TENDRIL_HOME`) — Specifies the dedicated absolute storage directory for your Plans, costs, and trash bins.
2. **AI Credentials** — Secure linkage for your Anthropic (Claude) configuration required to spawn programmatic Promptwares seamlessly.
3. **Primary Git Tracking** — Binding your first physical software Project/Repository to begin analysis.

Once validated, the app saves and generates a strictly compliant `config.yaml` file into your Root storage silently.

## Skippable Behavior

If you pre-provision a valid `config.yaml` script (common in enterprise shared infrastructure or dotfiles integration), Tendril completely bypasses the Onboarding application and establishes the Dashboard instantaneously.
