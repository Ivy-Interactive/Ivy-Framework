# Slack Block Kit JSON

The `notify slack <profile> --json` flag for Block Kit payloads fails with `no_text` error, even when a `text` fallback field is included in the JSON. 

## Workaround

Use `--message` with plain text instead. The `--json` flag appears to not properly pass the payload to the Slack API.

**Last tested:** 2026-03-31
