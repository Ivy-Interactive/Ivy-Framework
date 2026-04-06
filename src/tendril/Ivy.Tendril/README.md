# Tendril

## Quick Start

### First-Time Setup

1. **Run Tendril:**
   ```bash
   dotnet run
   ```

2. **Onboarding Wizard** will guide you through:
   - Setting up `TENDRIL_HOME` (where Tendril stores data)
   - Optional: Setting up `REPOS_HOME` (for shared team configs)

## Configuration

Tendril looks for `config.yaml` in this order:
1. `$TENDRIL_HOME/config.yaml` (if TENDRIL_HOME is set)
2. Application directory `config.yaml`

See `example.config.yaml` for configuration options.

