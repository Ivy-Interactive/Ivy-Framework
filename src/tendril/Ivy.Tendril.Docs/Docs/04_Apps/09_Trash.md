---
searchHints:
  - trash
  - delete
  - removed
  - force plan
  - duplicate
icon: Trash2
---

# Trash

<Ingress>
The Trash App securely tracks deleted plans, pruned AI ideas, and automatically tracked duplicate submissions, safeguarding your system from accidental deletion of architectural context.
</Ingress>

## Purpose

Tapping 'Delete' on an experimental Draft or a failed execution does not permanently wipe the associated technical writeups from your machine. Instead, they are relegated to the Trash view, allowing you to restore them subsequently or parse them for logic.

## The Trash Interface

The application parses an aggregate list from your configured `TENDRIL_HOME/Trash` directory.

The dashboard allows:
- **Comprehensive Filtering**: Search via filename, the literal original request parameter, targeted project, or standard file contents.
- **Reading Revisions**: By clicking on a trashed file, the Markdown viewer safely parses the historical AI-generated content locally, preventing you from losing hours of iteration due to a misclick.

## Force Plan Integration

Sometimes an initial Draft is rejected because a similar Plan (`DuplicateOf`) existed beforehand. However, after further investigation, you might identify critical differences that warrant a separate AI execution.

From the Trash interface, you can select any discarded element and click **Force Plan**. This completely circumvents Standard AI duplicate safety bounds and injects the discarded prompt securely back into an active `MakePlan` generative sequence locally linked against your repository.
