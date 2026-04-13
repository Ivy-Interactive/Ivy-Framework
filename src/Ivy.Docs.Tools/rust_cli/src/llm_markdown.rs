use lazy_static::lazy_static;
use regex::Regex;
use std::collections::HashMap;
use std::fs;
use std::path::Path;
#[cfg(any(target_os = "macos", target_os = "linux"))]
use std::process::Command;

lazy_static! {
    static ref DETAILS_BLOCK: Regex = Regex::new(r"(?s)<Details>.*?</Details>").unwrap();
    static ref SUMMARY_START: Regex = Regex::new(r"<Summary[^>]*>").unwrap();
    static ref CODE_BLOCK_DEMO: Regex =
        Regex::new(r"```(\w+)\s+demo-\w+(?:\s+demo-\w+)*").unwrap();
    static ref INGRESS_BLOCK: Regex = Regex::new(r"(?s)<Ingress>\s*([\s\S]*?)\s*</Ingress>").unwrap();
    static ref CALLOUT_BLOCK: Regex = Regex::new(r"(?s)<Callout[^>]*>\s*([\s\S]*?)\s*</Callout>").unwrap();
    static ref EMBED_BLOCK: Regex = Regex::new(r"<Embed\s+[^>]*/>").unwrap();
    static ref WIDGET_DOCS_BLOCK: Regex = Regex::new(r"<WidgetDocs\s+[^>]*/>").unwrap();
    static ref ATTR_TYPE: Regex =
        Regex::new(r#"(?i)Type\s*=\s*["']([^"']*)["']"#).unwrap();
    static ref ATTR_EXTENSION_TYPES: Regex =
        Regex::new(r#"(?i)ExtensionTypes\s*=\s*["']([^"']*)["']"#).unwrap();
    static ref ATTR_SOURCE_URL: Regex =
        Regex::new(r#"(?i)SourceUrl\s*=\s*["']([^"']*)["']"#).unwrap();
    static ref ATTR_URL: Regex =
        Regex::new(r#"(?i)Url\s*=\s*["']([^"']*)["']"#).unwrap();
    static ref CALLOUT_TYPE_ATTR: Regex =
        Regex::new(r#"(?i)<Callout[^>]*Type\s*=\s*["']([^"']*)["']"#).unwrap();
}

/// Load an API docs manifest from a JSON file.
/// Returns an empty HashMap if the path is None or the file doesn't exist.
pub fn load_api_docs_manifest(path: Option<&Path>) -> HashMap<String, String> {
    let Some(path) = path else {
        return HashMap::new();
    };
    if !path.exists() {
        return HashMap::new();
    }
    match fs::read_to_string(path) {
        Ok(content) => serde_json::from_str(&content).unwrap_or_default(),
        Err(_) => HashMap::new(),
    }
}

/// Build a manifest lookup key matching the C# GenerateApiDocsCommand.BuildKey format.
fn build_manifest_key(type_name: &str, extension_types: &str, source_url: &str) -> String {
    format!("{}|{}|{}", type_name, extension_types, source_url)
}

/// Generate a clean LLM-friendly markdown file from a source markdown file.
/// Mirrors the C# LlmMarkdownGenerator behavior.
pub fn generate(
    input_path: &Path,
    output_path: &Path,
    skip_if_not_changed: bool,
    api_docs: &HashMap<String, String>,
    manifest_hash: Option<&str>,
) -> Result<(), String> {
    let markdown_content =
        fs::read_to_string(input_path).map_err(|e| format!("Failed to read {}: {}", input_path.display(), e))?;

    let mut combined_content = markdown_content.clone();
    if let Some(h) = manifest_hash {
        combined_content.push_str(h);
    }
    let hash = get_short_hash(&combined_content, 8);

    if output_path.exists() && skip_if_not_changed {
        if let Some(old_hash) = read_hash(output_path) {
            if old_hash == hash {
                return Ok(());
            }
        }
    }

    let output_content = generate_markdown(&markdown_content, api_docs);

    fs::write(output_path, &output_content)
        .map_err(|e| format!("Failed to write {}: {}", output_path.display(), e))?;

    write_hash(output_path, &hash);

    Ok(())
}

/// Core transformation: strip frontmatter and process custom blocks.
fn generate_markdown(source: &str, api_docs: &HashMap<String, String>) -> String {
    let content = strip_frontmatter(source);
    let content = expand_details_blocks(&content);
    let content = process_widget_docs_blocks(&content, api_docs);
    let content = clean_code_block_arguments(&content);
    let content = process_custom_blocks(&content);
    content.trim().to_string()
}

/// Strip YAML frontmatter (--- ... ---) from the beginning.
fn strip_frontmatter(source: &str) -> String {
    let trimmed = source.trim_start();
    if !trimmed.starts_with("---") {
        return source.to_string();
    }

    // Find the closing ---
    if let Some(end_pos) = trimmed[3..].find("\n---") {
        let after = &trimmed[3 + end_pos + 4..]; // skip past \n---
        // Skip leading newlines after frontmatter
        let content = after.trim_start_matches(|c| c == '\n' || c == '\r');
        return content.to_string();
    }

    source.to_string()
}

/// Expand <Details><Summary>...</Summary><Body>...</Body></Details> blocks
fn expand_details_blocks(markdown: &str) -> String {
    let mut result = markdown.to_string();
    let max_iterations = 100;
    let mut iteration = 0;

    while DETAILS_BLOCK.is_match(&result) && iteration < max_iterations {
        result = DETAILS_BLOCK
            .replace_all(&result, |caps: &regex::Captures| {
                expand_single_details_block(&caps[0])
            })
            .to_string();
        iteration += 1;
    }

    result
}

fn expand_single_details_block(details_html: &str) -> String {
    let summary_match = SUMMARY_START.find(details_html);
    if summary_match.is_none() {
        return details_html
            .replace("<Details>", "")
            .replace("</Details>", "");
    }

    let summary_match = summary_match.unwrap();
    let summary_content_start = summary_match.end();
    let summary_end = match details_html[summary_content_start..].find("</Summary>") {
        Some(pos) => summary_content_start + pos,
        None => return details_html.to_string(),
    };

    let summary = details_html[summary_content_start..summary_end].trim();

    let body_match = Regex::new(r"<Body[^>]*>").unwrap().find(details_html);
    if body_match.is_none() {
        return format!("\n### {}\n\n", summary);
    }

    let body_match = body_match.unwrap();
    let body_content_start = body_match.end();
    let body_end = match details_html.rfind("</Body>") {
        Some(pos) => pos,
        None => return format!("\n### {}\n\n", summary),
    };

    let body_content = details_html[body_content_start..body_end].trim();

    format!("\n### {}\n\n{}\n\n", summary, body_content)
}

/// Replace <WidgetDocs Type="X" /> with API docs from manifest or a placeholder
fn process_widget_docs_blocks(markdown: &str, api_docs: &HashMap<String, String>) -> String {
    WIDGET_DOCS_BLOCK
        .replace_all(markdown, |caps: &regex::Captures| {
            let tag = &caps[0];
            if let Some(type_caps) = ATTR_TYPE.captures(tag) {
                let type_name = &type_caps[1];
                let extension_types = ATTR_EXTENSION_TYPES
                    .captures(tag)
                    .map(|c| c[1].to_string())
                    .unwrap_or_default();
                let source_url = ATTR_SOURCE_URL
                    .captures(tag)
                    .map(|c| c[1].to_string())
                    .unwrap_or_default();

                let key = build_manifest_key(type_name, &extension_types, &source_url);
                if let Some(api_doc) = api_docs.get(&key) {
                    api_doc.clone()
                } else {
                    format!("## API\n\n(See widget documentation for {})", type_name)
                }
            } else {
                String::new()
            }
        })
        .to_string()
}

/// Strip demo-* annotations from code fences
fn clean_code_block_arguments(markdown: &str) -> String {
    CODE_BLOCK_DEMO
        .replace_all(markdown, |caps: &regex::Captures| format!("```{}", &caps[1]))
        .to_string()
}

/// Process Ingress, Callout, and Embed blocks
fn process_custom_blocks(markdown: &str) -> String {
    let mut result = markdown.to_string();

    // Ingress -> italic
    result = INGRESS_BLOCK
        .replace_all(&result, |caps: &regex::Captures| {
            let content = caps[1].trim();
            format!("*{}*", content)
        })
        .to_string();

    // Callout -> blockquote
    result = CALLOUT_BLOCK
        .replace_all(&result, |caps: &regex::Captures| {
            let full_match = &caps[0];
            let content = caps[1].trim();
            let callout_type = CALLOUT_TYPE_ATTR
                .captures(full_match)
                .map(|c| c[1].to_string())
                .unwrap_or_else(|| "Note".to_string());
            format!("> **{}:** {}", callout_type, content)
        })
        .to_string();

    // Embed -> link
    result = EMBED_BLOCK
        .replace_all(&result, |caps: &regex::Captures| {
            let tag = &caps[0];
            if let Some(url_caps) = ATTR_URL.captures(tag) {
                let url = &url_caps[1];
                format!("[View: {}]({})", url, url)
            } else {
                String::new()
            }
        })
        .to_string();

    result
}

/// Compute a short hash matching the C# Utils.GetShortHash behavior:
/// SHA256 -> Base64 -> replace +/- _/ -> lowercase -> filter alphanumeric -> take first N chars
pub fn get_short_hash(input: &str, length: usize) -> String {
    use sha2::{Digest, Sha256};

    let mut hasher = Sha256::new();
    hasher.update(input.as_bytes());
    let hash = hasher.finalize();

    use base64::Engine;
    let b64 = base64::engine::general_purpose::STANDARD.encode(&hash);

    let transformed: String = b64
        .replace('+', "-")
        .replace('/', "_")
        .to_lowercase()
        .chars()
        .filter(|c| c.is_alphanumeric())
        .take(length)
        .collect();

    transformed
}

/// Write hash as extended attribute (macOS xattr, Linux setfattr, Windows ADS)
fn write_hash(file_path: &Path, hash: &str) {
    let path_str = file_path.to_str().unwrap_or("");

    #[cfg(target_os = "macos")]
    {
        let _ = Command::new("xattr")
            .args(["-w", "hash", hash, path_str])
            .output();
    }

    #[cfg(target_os = "linux")]
    {
        let _ = Command::new("setfattr")
            .args(["-n", "hash", "-v", hash, path_str])
            .output();
    }

    #[cfg(target_os = "windows")]
    {
        let ads_path = format!("{}:hash", path_str);
        let _ = fs::write(&ads_path, hash);
    }
}

/// Read hash from extended attribute
fn read_hash(file_path: &Path) -> Option<String> {
    let path_str = file_path.to_str().unwrap_or("");

    #[cfg(target_os = "macos")]
    {
        let output = Command::new("xattr")
            .args(["-p", "hash", path_str])
            .output()
            .ok()?;
        if output.status.success() {
            return Some(String::from_utf8_lossy(&output.stdout).trim().to_string());
        }
        return None;
    }

    #[cfg(target_os = "linux")]
    {
        let output = Command::new("getfattr")
            .args(["-n", "hash", "--only-values", path_str])
            .output()
            .ok()?;
        if output.status.success() {
            return Some(String::from_utf8_lossy(&output.stdout).trim().to_string());
        }
        return None;
    }

    #[cfg(target_os = "windows")]
    {
        let ads_path = format!("{}:hash", path_str);
        if let Ok(content) = fs::read_to_string(&ads_path) {
            return Some(content.trim().to_string());
        }
        return None;
    }

    #[allow(unreachable_code)]
    None
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_strip_frontmatter() {
        let input = "---\ntitle: Hello\norder: 1\n---\n\n# Content here";
        let result = strip_frontmatter(input);
        assert_eq!(result, "# Content here");
    }

    #[test]
    fn test_strip_frontmatter_no_frontmatter() {
        let input = "# Just content\n\nSome text";
        let result = strip_frontmatter(input);
        assert_eq!(result, input);
    }

    #[test]
    fn test_expand_details_block() {
        let input =
            "<Details><Summary>My Title</Summary><Body>Some content here</Body></Details>";
        let result = expand_details_blocks(input);
        assert!(result.contains("### My Title"));
        assert!(result.contains("Some content here"));
        assert!(!result.contains("<Details>"));
    }

    #[test]
    fn test_expand_details_no_body() {
        let input = "<Details><Summary>Title Only</Summary></Details>";
        let result = expand_details_blocks(input);
        assert!(result.contains("### Title Only"));
    }

    #[test]
    fn test_clean_code_block_arguments() {
        let input = "```csharp demo-tabs\nvar x = 1;\n```";
        let result = clean_code_block_arguments(input);
        assert_eq!(result, "```csharp\nvar x = 1;\n```");
    }

    #[test]
    fn test_clean_code_block_multiple_demo_args() {
        let input = "```csharp demo-tabs demo-right\nvar x = 1;\n```";
        let result = clean_code_block_arguments(input);
        assert_eq!(result, "```csharp\nvar x = 1;\n```");
    }

    #[test]
    fn test_clean_code_block_no_demo() {
        let input = "```csharp\nvar x = 1;\n```";
        let result = clean_code_block_arguments(input);
        assert_eq!(result, input);
    }

    #[test]
    fn test_process_ingress() {
        let input = "<Ingress>This is important text</Ingress>";
        let result = process_custom_blocks(input);
        assert_eq!(result, "*This is important text*");
    }

    #[test]
    fn test_process_callout() {
        let input = "<Callout Type=\"Warning\">Be careful here</Callout>";
        let result = process_custom_blocks(input);
        assert_eq!(result, "> **Warning:** Be careful here");
    }

    #[test]
    fn test_process_callout_default_type() {
        let input = "<Callout>Some note</Callout>";
        let result = process_custom_blocks(input);
        assert_eq!(result, "> **Note:** Some note");
    }

    #[test]
    fn test_process_embed() {
        let input = "<Embed Url=\"https://example.com/video\" />";
        let result = process_custom_blocks(input);
        assert_eq!(
            result,
            "[View: https://example.com/video](https://example.com/video)"
        );
    }

    #[test]
    fn test_widget_docs_block_placeholder() {
        let input = "<WidgetDocs Type=\"Button\" />";
        let empty_manifest = HashMap::new();
        let result = process_widget_docs_blocks(input, &empty_manifest);
        assert_eq!(result, "## API\n\n(See widget documentation for Button)");
    }

    #[test]
    fn test_widget_docs_block_with_manifest() {
        let input = "<WidgetDocs Type=\"Ivy.Button\" ExtensionTypes=\"Ivy.ButtonExtensions\" SourceUrl=\"https://example.com/Button.cs\"/>";
        let mut manifest = HashMap::new();
        manifest.insert(
            "Ivy.Button|Ivy.ButtonExtensions|https://example.com/Button.cs".to_string(),
            "## API\n\n### Constructors\n\n| Signature |\n|---|\n| `new Button()` |".to_string(),
        );
        let result = process_widget_docs_blocks(input, &manifest);
        assert!(result.contains("### Constructors"));
        assert!(result.contains("new Button()"));
        assert!(!result.contains("(See widget documentation"));
    }

    #[test]
    fn test_widget_docs_block_manifest_fallback() {
        let input = "<WidgetDocs Type=\"Ivy.Unknown\" />";
        let mut manifest = HashMap::new();
        manifest.insert(
            "Ivy.Button||".to_string(),
            "## API\n\nButton docs".to_string(),
        );
        let result = process_widget_docs_blocks(input, &manifest);
        assert_eq!(result, "## API\n\n(See widget documentation for Ivy.Unknown)");
    }

    #[test]
    fn test_full_pipeline() {
        let input = r#"---
title: Test
order: 1
---

<Ingress>Welcome to the docs</Ingress>

# Hello World

```csharp demo-tabs
var x = 1;
```

<Callout Type="Tip">This is a tip</Callout>

<WidgetDocs Type="Button" />
"#;
        let empty_manifest = HashMap::new();
        let result = generate_markdown(input, &empty_manifest);
        assert!(result.contains("*Welcome to the docs*"));
        assert!(result.contains("# Hello World"));
        assert!(result.contains("```csharp\n"));
        assert!(!result.contains("demo-tabs"));
        assert!(result.contains("> **Tip:** This is a tip"));
        assert!(result.contains("## API"));
        assert!(result.contains("(See widget documentation for Button)"));
        assert!(!result.contains("title: Test"));
    }

    #[test]
    fn test_skip_if_not_changed() {
        let dir = std::env::temp_dir().join("llm_markdown_test");
        let _ = fs::create_dir_all(&dir);

        let input_path = dir.join("test_input.md");
        let output_path = dir.join("test_output.md");
        let empty_manifest = HashMap::new();

        fs::write(&input_path, "# Hello\n\nWorld").unwrap();

        // First generate
        generate(&input_path, &output_path, true, &empty_manifest, None).unwrap();
        assert!(output_path.exists());
        let _content1 = fs::read_to_string(&output_path).unwrap();

        // Modify output to detect if it gets regenerated
        fs::write(&output_path, "MODIFIED").unwrap();
        // Re-write the hash so it thinks it's current
        let hash = get_short_hash("# Hello\n\nWorld", 8);
        write_hash(&output_path, &hash);

        // Should skip since hash matches
        generate(&input_path, &output_path, true, &empty_manifest, None).unwrap();
        let content2 = fs::read_to_string(&output_path).unwrap();
        assert_eq!(content2, "MODIFIED"); // Not regenerated

        // Change source, should regenerate
        fs::write(&input_path, "# Changed\n\nContent").unwrap();
        generate(&input_path, &output_path, true, &empty_manifest, None).unwrap();
        let content3 = fs::read_to_string(&output_path).unwrap();
        assert_ne!(content3, "MODIFIED"); // Was regenerated

        // Cleanup
        let _ = fs::remove_dir_all(&dir);
    }
}
