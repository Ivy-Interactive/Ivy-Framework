use std::collections::HashSet;
use std::fs;
use std::path::Path;
use markdown::{to_mdast, ParseOptions};
use markdown::mdast::Node;
use serde::Deserialize;
use regex::Regex;
use lazy_static::lazy_static;

use crate::utils;
use crate::link_converter::LinkConverter;
use roxmltree::Document as XmlDocument;

#[derive(Debug, Deserialize, Default)]
#[serde(rename_all = "camelCase")]
pub struct AppMeta {
    pub icon: Option<String>,
    #[serde(default)]
    pub order: i32,
    pub title: Option<String>,
    #[serde(default = "default_view_base")]
    pub view_base: String,
    #[serde(default)]
    pub prepare: Option<String>,
    #[serde(default)]
    pub group_expanded: bool,
    pub search_hints: Option<Vec<String>>,
    pub imports: Option<Vec<String>>,
    #[serde(default)]
    pub hidden: bool,
}

fn default_view_base() -> String { "ViewBase".to_string() }

lazy_static! {
    static ref DETAILS_BLOCK: Regex = Regex::new(r"(?s)<Details>.*?</Details>").unwrap();
    static ref SUMMARY_START: Regex = Regex::new(r"<Summary[^>]*>").unwrap();
    static ref BODY_START: Regex = Regex::new(r"<Body[^>]*>").unwrap();
}

pub fn convert_async(
    name: &str,
    relative_path: &str,
    absolute_path: &Path,
    output_file: &Path,
    namespace: &str,
    skip_if_not_changed: bool,
    order: Option<i32>
) -> Result<(), String> {
    let class_name = format!("{}App", name);
    let markdown_content = fs::read_to_string(absolute_path).expect("Failed to read");
    let document_source = utils::get_git_file_url(absolute_path);

    
    let mut app_meta = AppMeta {
        view_base: "ViewBase".to_string(),
        ..Default::default()
    };
    
    // The markdown crate natively parses YAML into a Node::Yaml object, but it's easier
    // to manually slice it out just like C# does for parsing AppMeta.
    let mut parse_options = ParseOptions::gfm();
    parse_options.constructs.frontmatter = true;
    
    let ast_result = to_mdast(&markdown_content, &parse_options);
    if let Err(e) = ast_result {
        return Err(format!("Failed to parse markdown: {:?} - {}", absolute_path, e));
    }
    let ast = ast_result.unwrap();

    if let Node::Root(root) = &ast {
        if let Some(Node::Yaml(yaml)) = root.children.first() {
            if let Ok(meta) = serde_yaml_ng::from_str::<AppMeta>(&yaml.value) {
                app_meta = meta;
            }
        }
    }

    if app_meta.view_base.is_empty() {
        app_meta.view_base = "ViewBase".to_string();
    }

    if let Some(o) = order { app_meta.order = o; }

    let mut code_builder = String::new();
    let mut view_builder = String::new();
    let mut used_class_names = HashSet::new();
    let mut referenced_apps = HashSet::new();
    let link_converter = LinkConverter::new(relative_path);

    code_builder.push_str("using System;\nusing Ivy;\nusing static Ivy.Layout;\nusing static Ivy.Text;\n");
    if let Some(ref imports) = app_meta.imports {
        for import in imports { code_builder.push_str(&format!("using {};\n", import)); }
    }
    
    code_builder.push_str(&format!("\nnamespace {};\n\n", namespace));
    
    code_builder.push_str(&format!("[App(order:{}", app_meta.order));
    if let Some(ref icon) = app_meta.icon { code_builder.push_str(&format!(", icon:Icons.{}", icon)); }
    if let Some(ref title) = app_meta.title { code_builder.push_str(&format!(", title:{}", utils::format_literal(title))); }
    if app_meta.group_expanded { code_builder.push_str(", groupExpanded:true"); }
    if app_meta.hidden { code_builder.push_str(", isVisible:false"); }
    if let Some(ref ds) = document_source { code_builder.push_str(&format!(", documentSource:{}", utils::format_literal(ds))); }
    if let Some(ref hints) = app_meta.search_hints {
        if !hints.is_empty() {
            let hints_str = hints.iter().map(|h| utils::format_literal(h)).collect::<Vec<_>>().join(", ");
            code_builder.push_str(&format!(", searchHints: [{}]", hints_str));
        }
    }
    code_builder.push_str(")]\n");
    
    code_builder.push_str(&format!("public class {}(bool onlyBody = false) : {}\n{{\n", class_name, app_meta.view_base));
    code_builder.push_str(&format!("    public {}() : this(false)\n    {{\n    }}\n", class_name));
    code_builder.push_str("    public override object? Build()\n    {\n");
    
    if let Some(ref prep) = app_meta.prepare {
        for line in prep.lines() { code_builder.push_str(&format!("        {}\n", line.trim())); }
    }
    
    // Check if there are non-YAML nodes
    let has_content = if let Node::Root(root) = &ast {
        root.children.iter().any(|n| !matches!(n, Node::Yaml(_)))
    } else { false };

    if has_content {
        code_builder.push_str("        var appDescriptor = this.UseService<AppDescriptor>();\n");
        code_builder.push_str("        var onLinkClick = this.UseLinks();\n");

        let mut content_builder = String::new();
        let mut headings = Vec::new();
        
        handle_blocks(
            &ast, &markdown_content, &mut content_builder, &mut view_builder, &mut used_class_names, 
            &mut referenced_apps, &link_converter, false, 3, Some(&mut headings)
        );

        let mut headings_code = String::from("new List<ArticleHeading> { ");
        for (id, text, level) in headings {
            headings_code.push_str(&format!("new ArticleHeading({}, {}, {}), ", utils::format_literal(&id), utils::format_literal(&text), level));
        }
        headings_code.push_str("}");

        code_builder.push_str("        var article = new Article().ShowToc(!onlyBody).ShowFooter(!onlyBody).Previous(appDescriptor.Previous).Next(appDescriptor.Next).DocumentSource(appDescriptor.DocumentSource).OnLinkClick(onLinkClick)\n");
        code_builder.push_str(&format!("            .Headings({})\n", headings_code));
        code_builder.push_str("            | new global::Ivy.Docs.Shared.Internal.SmartSearchView()\n");
        code_builder.push_str(&content_builder);
        code_builder.push_str("            ;\n");

        if !referenced_apps.is_empty() {
            code_builder.push_str("        // Build errors here indicates that one or more referenced apps don't exist. Check markdown links.\n");
            let mut refs: Vec<_> = referenced_apps.into_iter().collect();
            refs.sort(); // For deterministic output
            let refs_str = refs.iter().map(|e| format!("typeof({})", e)).collect::<Vec<_>>().join(", ");
            code_builder.push_str(&format!("        Type[] _ = [{}]; \n", refs_str));
        }
        
        code_builder.push_str("        return article;\n");
    } else {
        code_builder.push_str("        return null;\n");
    }

    code_builder.push_str("    }\n}\n");
    code_builder.push_str(&view_builder);

    if output_file.exists() && skip_if_not_changed {
        if let Ok(existing_content) = fs::read_to_string(output_file) {
            if existing_content == code_builder {
                return Ok(());
            }
        }
    }

    fs::write(output_file, code_builder).expect("Failed to write to file");
    Ok(())
}

fn handle_blocks(
    node: &Node,
    markdown_content: &str,
    code_builder: &mut String,
    view_builder: &mut String,
    used_class_names: &mut HashSet<String>,
    referenced_apps: &mut HashSet<String>,
    link_converter: &LinkConverter,
    is_nested_content: bool,
    base_indent: usize,
    mut headings: Option<&mut Vec<(String, String, i32)>>,
) {
    let mut section_builder = String::new();
    
    let write_section = |section: &mut String, code_builder: &mut String, referenced_apps: &mut HashSet<String>, _remove_bottom_margin: bool| {
        let trimmed = section.trim();
        if !trimmed.is_empty() {
            let (types, converted) = link_converter.convert(trimmed);
            for t in types { referenced_apps.insert(t); }
            let prepend = if is_nested_content { ", new Markdown(" } else { "| new Markdown(" };
            append_multiline(base_indent, &converted, code_builder, prepend, ").OnLinkClick(onLinkClick)");
            section.clear();
        }
    };

    let details_matches: Vec<(usize, usize, String)> = DETAILS_BLOCK.find_iter(markdown_content)
        .map(|m| (m.start(), m.end(), m.as_str().to_string()))
        .collect();

    if let Node::Root(root) = node {
        for child in &root.children {
            let pos = child.position().unwrap();
            let start = pos.start.offset;
            let end = pos.end.offset;
            
            // Skip if inside Details block entirely
            if details_matches.iter().any(|(s, e, _)| start > *s && start < *e) {
                continue;
            }

            match child {
                Node::Html(html) => {
                    write_section(&mut section_builder, code_builder, referenced_apps, false);
                    let html_str = html.value.trim();
                    
                    if html_str.starts_with("<Details>") {
                        if let Some((_, _, full_html)) = details_matches.iter().find(|(s, _, _)| *s == start) {
                            handle_details_direct(code_builder, full_html, view_builder, used_class_names, referenced_apps, link_converter, base_indent);
                            continue;
                        }
                    }
                    
                    if html_str.starts_with("<details>") || html_str.to_lowercase().starts_with("</") {
                        continue;
                    }
                    
                    if html_str.starts_with("<Callout") || html_str.starts_with("<Embed") || html_str.starts_with("<WidgetDocs") || html_str.starts_with("<Ingress") {
                        if let Ok(xml) = XmlDocument::parse(html_str) {
                            if let Some(root_xml) = xml.root_element().into() {
                                let node_xml: roxmltree::Node = root_xml;
                                match node_xml.tag_name().name() {
                                    "Callout" => handle_callout_block(code_builder, &node_xml, link_converter, referenced_apps),
                                    "Embed" => handle_embed_block(code_builder, &node_xml),
                                    "WidgetDocs" => handle_widget_docs_block(code_builder, &node_xml, headings.as_deref_mut()),
                                    "Ingress" => handle_ingress_block(code_builder, &node_xml, link_converter, referenced_apps),
                                    _ => println!("Unknown XML block: {}", node_xml.tag_name().name()),
                                }
                            }
                        }
                        continue;
                    }
                    
                    // Unknown HTML block, just append raw text
                    section_builder.push_str("\n");
                    section_builder.push_str(&markdown_content[start..end].trim());
                },
                Node::Code(code) => {
                    write_section(&mut section_builder, code_builder, referenced_apps, true);
                    handle_code_block(code, markdown_content, code_builder, view_builder, used_class_names, is_nested_content, base_indent);
                },
                Node::Heading(heading) => {
                    let mut text = String::new();
                    extract_text(child, &mut text);
                    
                    section_builder.push_str("\n");
                    section_builder.push_str(&format!("{} {}\n", "#".repeat(heading.depth as usize), text.trim()));
                    
                    if let Some(ref mut heads) = headings {
                        heads.push((generate_heading_id(&text), text.trim().to_string(), heading.depth as i32));
                    }
                },
                Node::Yaml(_) => { /* Ignore frontmatter */ },
                _ => {
                    section_builder.push_str("\n\n");
                    section_builder.push_str(&markdown_content[start..end].trim());
                }
            }
        }
    }
    write_section(&mut section_builder, code_builder, referenced_apps, false);
}

fn extract_text(node: &Node, buf: &mut String) {
    if let Some(children) = node.children() {
        for child in children {
            if let Node::Text(t) = child {
                buf.push_str(&t.value);
            } else if let Node::InlineCode(c) = child {
                buf.push_str(&c.value);
            } else {
                extract_text(child, buf);
            }
        }
    }
}

fn append_multiline(tabs: usize, raw: &str, sb: &mut String, prepend: &str, append: &str) {
    if raw.contains('\n') || raw.contains('"') {
        sb.push_str(&format!("{}{}\n", "    ".repeat(tabs), prepend));
        sb.push_str(&format!("{}\"\"\"\"\n", "    ".repeat(tabs + 1)));
        for line in raw.lines() {
            sb.push_str(&format!("{}\n", if line.is_empty() { String::new() } else { format!("{}{}", "    ".repeat(tabs + 1), line.trim_end()) }));
        }
        sb.push_str(&format!("{}\"\"\"\"{}\n", "    ".repeat(tabs + 1), append));
    } else {
        sb.push_str(&format!("{}{}{}{}\n", "    ".repeat(tabs), prepend, utils::format_literal(raw), append));
    }
}

fn generate_heading_id(text: &str) -> String {
    let mut id = text.to_lowercase().trim().to_string();
    let re1 = Regex::new(r"\s+").unwrap();
    id = re1.replace_all(&id, "-").to_string();
    let re2 = Regex::new(r"[^\w\u00C0-\u024F\u1E00-\u1EFF-]").unwrap();
    id = re2.replace_all(&id, "").to_string();
    id
}

fn handle_details_direct(
    code_builder: &mut String,
    html_content: &str,
    view_builder: &mut String,
    used_class_names: &mut HashSet<String>,
    referenced_apps: &mut HashSet<String>,
    _link_converter: &LinkConverter,
    base_indent: usize,
) {
    let summary_match = SUMMARY_START.find(html_content).expect("Details block missing <Summary>");
    let summary_start = summary_match.start() + summary_match.len();
    let summary_end = html_content[summary_start..].find("</Summary>").expect("Details missing closing </Summary>") + summary_start;
    let summary = html_content[summary_start..summary_end].trim();

    let body_match = BODY_START.find(html_content).expect("Details block missing <Body>");
    let body_start = body_match.start() + body_match.len();
    let body_end = html_content.rfind("</Body>").expect("Details missing closing </Body>");
    let body_content = html_content[body_start..body_end].trim();

    let mut options = ParseOptions::gfm();
    options.constructs.frontmatter = true;
    let ast = to_mdast(body_content, &options).unwrap();
    
    let mut body_code_builder = String::new();
    let mut body_referenced_apps = HashSet::new();
    let body_link_converter = LinkConverter::new("");
    
    handle_blocks(
        &ast, body_content, &mut body_code_builder, view_builder, used_class_names,
        &mut body_referenced_apps, &body_link_converter, false, 4, None
    );
    
    for t in body_referenced_apps {
        referenced_apps.insert(t);
    }
    
    let body_output = body_code_builder.trim_end();
    
    if !body_output.is_empty() {
        let lines: Vec<&str> = body_output.lines().collect();
        let multiple_items = lines.iter().filter(|l| l.trim_start().starts_with("| ")).count() > 1;
        
        let lead = if base_indent > 3 { ", " } else { "| " };
        if multiple_items {
            code_builder.push_str(&format!("{}{}new Expandable(\"{}\",\n", "    ".repeat(base_indent), lead, summary));
            code_builder.push_str(&format!("{}Vertical().Gap(4)\n", "    ".repeat(base_indent + 1)));
            code_builder.push_str(body_output);
            code_builder.push('\n');
            code_builder.push_str(&format!("{})\n", "    ".repeat(base_indent)));
        } else {
            let mut single_item = body_output.trim_start().to_string();
            if single_item.starts_with("| ") {
                single_item = single_item[2..].to_string();
            }
            code_builder.push_str(&format!("{}{}new Expandable(\"{}\",\n", "    ".repeat(base_indent), lead, summary));
            code_builder.push_str(&format!("{}\n", single_item));
            code_builder.push_str(&format!("{})\n", "    ".repeat(base_indent)));
        }
    } else {
        let lead = if base_indent > 3 { ", " } else { "| " };
        code_builder.push_str(&format!("{}{}new Expandable(\"{}\", new Markdown(\"No content\"))\n", "    ".repeat(base_indent), lead, summary));
    }
}

fn get_unused_class_name(class_name: &str, used_class_names: &mut HashSet<String>) -> String {
    if used_class_names.contains(class_name) {
        let mut i = 1;
        while used_class_names.contains(&format!("{}{}", class_name, i)) {
            i += 1;
        }
        format!("{}{}", class_name, i)
    } else {
        class_name.to_string()
    }
}

fn handle_demo_code_block(
    code_builder: &mut String,
    view_builder: &mut String,
    code_content: &str,
    language: &str,
    arguments: &str,
    used_class_names: &mut HashSet<String>,
    is_nested_content: bool,
    base_indent: usize,
) {
    let mut insert_code = code_content.to_string();
    if let Some(mut class_name) = utils::is_view(code_content) {
        let unused_class_name = get_unused_class_name(&class_name, used_class_names);
        if unused_class_name != class_name {
            insert_code = utils::rename_class(code_content, &class_name, &unused_class_name);
            class_name = unused_class_name;
        }
        used_class_names.insert(class_name.clone());
        view_builder.push_str("\n\n");
        view_builder.push_str(&insert_code);
        insert_code = format!("new {}()", class_name);
    }

    let parts: Vec<&str> = arguments.split_whitespace().collect();
    let layout = if parts.is_empty() { "demo" } else { parts[0] };

    let append_demo_content = |cb: &mut String, tabs: usize, insert: &str| {
        cb.push_str(&format!("{}{}new Box().Content({})\n", "    ".repeat(tabs), if is_nested_content { ", " } else { "| " }, insert));
    };

    let prepend_container = if is_nested_content { ", " } else { "| " };

    match layout {
        "demo-tabs" => {
            code_builder.push_str(&format!("{}{}Tabs( \n", "    ".repeat(base_indent), prepend_container));
            code_builder.push_str(&format!("{}new Tab(\"Demo\", new Box().Content({})),\n", "    ".repeat(base_indent + 1), insert_code));
            append_multiline(base_indent + 1, code_content, code_builder, "new Tab(\"Code\", new CodeBlock(", &format!(",{}))", map_language_to_enum(language)));
            code_builder.push_str(&format!("{}).Height(Size.Fit()).Variant(TabsVariant.Content)\n", "    ".repeat(base_indent)));
        },
        "demo-below" => {
            code_builder.push_str(&format!("{}{}(Vertical() \n", "    ".repeat(base_indent), prepend_container));
            append_multiline(base_indent + 1, code_content, code_builder, "| new CodeBlock(", &format!(",{})", map_language_to_enum(language)));
            append_demo_content(code_builder, base_indent + 1, &insert_code);
            code_builder.push_str(&format!("{})\n", "    ".repeat(base_indent)));
        },
        "demo-above" => {
            code_builder.push_str(&format!("{}{}(Vertical() \n", "    ".repeat(base_indent), prepend_container));
            append_demo_content(code_builder, base_indent + 1, &insert_code);
            append_multiline(base_indent + 1, code_content, code_builder, "| new CodeBlock(", &format!(",{})", map_language_to_enum(language)));
            code_builder.push_str(&format!("{})\n", "    ".repeat(base_indent)));
        },
        "demo-right" => {
            code_builder.push_str(&format!("{}{}(Grid().Columns(2) \n", "    ".repeat(base_indent), prepend_container));
            append_multiline(base_indent + 1, code_content, code_builder, "| new CodeBlock(", &format!(",{})", map_language_to_enum(language)));
            append_demo_content(code_builder, base_indent + 1, &insert_code);
            code_builder.push_str(&format!("{})\n", "    ".repeat(base_indent)));
        },
        "demo-left" => {
            code_builder.push_str(&format!("{}{}(Grid().Columns(2) \n", "    ".repeat(base_indent), prepend_container));
            append_demo_content(code_builder, base_indent + 1, &insert_code);
            append_multiline(base_indent + 1, code_content, code_builder, "| new CodeBlock(", &format!(",{})", map_language_to_enum(language)));
            code_builder.push_str(&format!("{})\n", "    ".repeat(base_indent)));
        },
        _ => { append_demo_content(code_builder, base_indent, &insert_code); }
    }
}
fn get_xml_text(node: &roxmltree::Node) -> String {
    let mut s = String::new();
    for descendant in node.descendants() {
        if descendant.is_text() {
            s.push_str(descendant.text().unwrap_or(""));
        }
    }
    s.trim().to_string()
}

fn handle_callout_block(code_builder: &mut String, xml: &roxmltree::Node, link_converter: &LinkConverter, referenced_apps: &mut HashSet<String>) {
    let t = xml.attribute("Type").unwrap_or("Info");
    let icon = xml.attribute("Icon").unwrap_or_else(|| {
        match t.to_lowercase().as_str() {
            "tip" | "info" => "Info",
            "warning" | "error" => "CircleAlert",
            "success" => "CircleCheck",
            _ => "Info"
        }
    });

    let content = get_xml_text(xml);
    let (types, converted) = link_converter.convert(&content);
    for t in types { referenced_apps.insert(t); }

    append_multiline(3, &converted, code_builder, "| new Callout(", &format!(", icon:Icons.{}).OnLinkClick(onLinkClick)", icon));
}

fn handle_embed_block(code_builder: &mut String, xml: &roxmltree::Node) {
    let url = xml.attribute("Url").expect("Embed block must have Url");
    code_builder.push_str(&format!("            | new Embed(\"{}\")\n", url));
}

fn handle_widget_docs_block(code_builder: &mut String, xml: &roxmltree::Node, headings: Option<&mut Vec<(String, String, i32)>>) {
    let type_name = xml.attribute("Type").expect("WidgetDocs block must have Type");
    let ext_types = xml.attribute("ExtensionTypes");
    let src_url = xml.attribute("SourceUrl");

    let e = if let Some(x) = ext_types { utils::format_literal(x) } else { "null".to_string() };
    let s = if let Some(x) = src_url { utils::format_literal(x) } else { "null".to_string() };

    code_builder.push_str(&format!("            | new WidgetDocsView(\"{}\", {}, {})\n", type_name, e, s));

    if let Some(h) = headings {
        h.push(("api".to_string(), "API".to_string(), 2));
    }
}

fn handle_ingress_block(code_builder: &mut String, xml: &roxmltree::Node, link_converter: &LinkConverter, referenced_apps: &mut HashSet<String>) {
    let content = get_xml_text(xml);
    if content.is_empty() {
        panic!("Ingress block must have content.");
    }
    let (types, converted) = link_converter.convert(&content);
    for t in types { referenced_apps.insert(t); }

    append_multiline(3, &converted, code_builder, "| Lead(", ")");
}

fn map_language_to_enum(lang: &str) -> &'static str {
    match lang.to_lowercase().as_str() {
        "csharp" | "cs" => "Languages.Csharp",
        "javascript" | "js" => "Languages.Javascript",
        "typescript" | "ts" => "Languages.Typescript",
        "python" => "Languages.Python",
        "sql" => "Languages.Sql",
        "html" => "Languages.Html",
        "css" => "Languages.Css",
        "json" => "Languages.Json",
        "dbml" => "Languages.Dbml",
        "xml" => "Languages.Xml",
        "text" => "Languages.Text",
        _ => "Languages.Text",
    }
}


fn handle_code_block(
    code_node: &markdown::mdast::Code,
    _markdown_content: &str,
    code_builder: &mut String,
    view_builder: &mut String,
    used_class_names: &mut HashSet<String>,
    is_nested_content: bool,
    base_indent: usize,
) {
    let language = code_node.lang.as_deref().unwrap_or("csharp").to_lowercase();
    // In Markdig, code block span contains the markdown ` ```... ` delimiters.
    // In markdown crate, code_node.value is just the parsed string context WITHOUT the delimiters!
    // But `MarkdownConverter.cs` executes `RemoveFirstAndLastLine()` because Markdig gave the raw ```!
    // So with `markdown` crate, we DO NOT DO `RemoveFirstAndLastLine()` !!
    let code_content = code_node.value.trim().to_string();

    let meta = code_node.meta.as_deref().unwrap_or("").trim().to_lowercase();

    if language == "csharp" && meta.starts_with("demo") {
        handle_demo_code_block(code_builder, view_builder, &code_content, &language, &meta, used_class_names, is_nested_content, base_indent);
    } else if language == "terminal" {
        code_builder.push_str(&format!("{}{}new Terminal()\n", "    ".repeat(base_indent), if is_nested_content { ", " } else { "| " }));
        for line in code_content.lines() {
            if line.starts_with('>') {
                code_builder.push_str(&format!("{}.AddCommand({})\n", "    ".repeat(base_indent + 1), utils::format_literal(line.trim_start_matches('>').trim())));
            } else {
                code_builder.push_str(&format!("{}.AddOutput({})\n", "    ".repeat(base_indent + 1), utils::format_literal(line.trim())));
            }
        }
        code_builder.push_str(&format!("{}\n", "    ".repeat(base_indent + 1)));
    } else if language == "mermaid" {
        let mermaid_block = format!("```mermaid\n{}\n```", code_content);
        let prepend = if is_nested_content { ", new Markdown(" } else { "| new Markdown(" };
        append_multiline(base_indent, &mermaid_block, code_builder, prepend, ").OnLinkClick(onLinkClick)");
    } else {
        let prepend = if is_nested_content { ", new CodeBlock(" } else { "| new CodeBlock(" };
        let append = format!(",{})", map_language_to_enum(&language));
        append_multiline(base_indent, &code_content, code_builder, prepend, &append);
    }
}
