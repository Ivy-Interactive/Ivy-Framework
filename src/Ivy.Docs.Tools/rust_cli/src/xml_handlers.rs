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

fn remove_first_last_line(input: &str) -> String {
    let lines: Vec<&str> = input.lines().collect();
    if lines.len() <= 2 {
        return String::new();
    }
    lines[1..lines.len()-1].join("\n")
}

fn handle_code_block(
    code_node: &markdown::mdast::Code,
    markdown_content: &str,
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
    let mut code_content = code_node.value.trim().to_string();

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
