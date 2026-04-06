use std::path::Path;
use regex::Regex;
use std::process::Command;
use std::sync::Mutex;
use lazy_static::lazy_static;

pub fn get_order_from_file_name(filename: &str) -> (Option<i32>, String) {
    let name_without_ext = Path::new(filename).file_stem().unwrap_or_default().to_str().unwrap_or_default().to_string();
    let parts: Vec<&str> = name_without_ext.split('_').collect();
    if parts.len() > 1 {
        if let Ok(order) = parts[0].parse::<i32>() {
            return (Some(order), parts[1..].join("_"));
        }
    }
    (None, name_without_ext)
}

pub fn get_relative_folder_without_order(input_folder: &Path, input_file: &Path) -> String {
    let normalized_input = input_folder.to_str().unwrap_or("").replace("\\", "/");
    let normalized_file = input_file.parent().unwrap_or(Path::new("")).to_str().unwrap_or("").replace("\\", "/");
    
    let rel = if normalized_file.starts_with(&normalized_input) {
        let mut trim = normalized_input.len();
        if trim < normalized_file.len() && normalized_file[trim..].starts_with('/') { trim += 1; }
        normalized_file[trim..].to_string()
    } else { String::new() };

    if rel.is_empty() || rel == "." { return String::new(); }

    let re = Regex::new(r"^\d+_").unwrap();
    let parts: Vec<String> = rel.split('/').map(|p| re.replace(p, "").to_string()).filter(|p| !p.is_empty()).collect();
    parts.join("/")
}

pub fn format_literal(literal: &str) -> String {
    let escaped = literal.replace("\\", "\\\\").replace("\"", "\\\"").replace("\n", "\\n").replace("\r", "\\r");
    format!("\"{escaped}\"")
}

pub fn is_view(code: &str) -> Option<String> {
    let class_re = Regex::new(r"class\s+([A-Za-z0-9_]+)\s*:\s*(?:[^\{]*?)ViewBase").unwrap();
    let build_re = Regex::new(r"override[\s\w\?<>\.]+Build\s*\(").unwrap();
    if let Some(caps) = class_re.captures(code) {
        if build_re.is_match(code) { return Some(caps[1].to_string()); }
    }
    None
}

pub fn rename_class(code: &str, old_class: &str, new_class: &str) -> String {
    let re = Regex::new(&format!(r"class\s+{}", old_class)).unwrap();
    let code = re.replace(code, format!("class {}", new_class));
    let ctor_re = Regex::new(&format!(r"public\s+{}\s*\(", old_class)).unwrap();
    ctor_re.replace(&code, format!("public {new_class}(")).to_string()
}


lazy_static! { static ref GIT_INFO: Mutex<Option<(String, String, String)>> = Mutex::new(None); }

pub fn get_git_file_url(local_file_path: &Path) -> Option<String> {
    if !local_file_path.exists() { return None; }
    let mut cache = GIT_INFO.lock().unwrap();
    if cache.is_none() {
        let dir = local_file_path.parent().unwrap();
        if let Ok(url) = run_git_command("config --get remote.origin.url", dir) {
            let remote_url = convert_to_https_url(url.trim());
            if let Ok(root) = run_git_command("rev-parse --show-toplevel", dir) {
                if let Ok(branch) = run_git_command("rev-parse --abbrev-ref HEAD", dir) {
                    *cache = Some((remote_url, root.trim().to_string(), branch.trim().to_string()));
                }
            }
        }
    }
    if let Some((remote_url, repo_root, branch)) = cache.as_ref() {
        if let Ok(rel_path) = local_file_path.strip_prefix(repo_root) {
            let rel_str = rel_path.to_str().unwrap().replace("\\", "/");
            return Some(format!("{}/blob/{}/{}", remote_url, branch, rel_str));
        }
    }
    None
}

fn run_git_command(args: &str, dir: &Path) -> Result<String, String> {
    let args_vec: Vec<&str> = args.split_whitespace().collect();
    let output = Command::new("git").args(args_vec).current_dir(dir).output().map_err(|e| e.to_string())?;
    if output.status.success() { Ok(String::from_utf8_lossy(&output.stdout).to_string()) } else { Err(String::from_utf8_lossy(&output.stderr).to_string()) }
}

fn convert_to_https_url(mut git_url: &str) -> String {
    if git_url.starts_with("git@") {
        let re = Regex::new(r"git@([^:]+):([^\.]+)\.git").unwrap();
        if let Some(caps) = re.captures(git_url) { return format!("https://{}/{}", &caps[1], &caps[2]); }
    }
    if git_url.ends_with(".git") { git_url = &git_url[..git_url.len()-4]; }
    git_url.to_string()
}

// ---- PATH & LINK MAPPERS ---- //

pub fn get_path_for_link(source: &str, link: &str) -> String {
    if link.starts_with("http://") || link.starts_with("https://") || link.starts_with("app://") {
        return link.to_string();
    }
    
    let source_normalized = source.replace("\\", "/");
    let source_dir = if let Some(idx) = source_normalized.rfind('/') { source_normalized[..idx].to_string() } else { "".to_string() };
    
    let mut link_str = link.to_string();
    if link_str.starts_with("./") { link_str = link_str[2..].to_string(); } 
    else if link_str.starts_with("../") {
        let mut source_parts: Vec<&str> = source_dir.split('/').filter(|p| !p.is_empty()).collect();
        let link_parts: Vec<&str> = link_str.split('/').collect();
        let up_count = link_parts.iter().take_while(|&&p| p == "..").count();
        let actual_link_parts: Vec<&str> = link_parts.into_iter().skip(up_count).collect();
        for _ in 0..up_count { source_parts.pop(); }
        source_parts.extend(actual_link_parts);
        return source_parts.join("/");
    }
    
    if source_dir.is_empty() { link_str.replace("\\", "/") } else { format!("{}/{}", source_dir, link_str).replace("\\", "/") }
}

pub fn get_type_name_from_path(path: &str) -> String {
    let mut p = path.to_string();
    if p.ends_with(".md") { p = p[..p.len()-3].to_string(); }
    let re = Regex::new(r"^\d+_").unwrap();
    let parts: Vec<String> = p.split(|c| c == '/' || c == '\\').map(|part| re.replace(part, "").to_string()).filter(|part| !part.is_empty()).collect();
    if parts.is_empty() { return String::new(); }
    let mut ns_parts = parts[..parts.len()-1].to_vec();
    ns_parts.push(format!("{}App", parts.last().unwrap()));
    ns_parts.join(".")
}

pub fn title_case_to_friendly_url(mut input: String) -> String {
    if input.is_empty() { return input; }
    if input.to_lowercase().ends_with("app") { input = input[..input.len()-3].to_string(); }
    let had_underscore = input.starts_with('_');
    if had_underscore { input = input[1..].to_string(); }
    let re1 = Regex::new(r"([A-Z]+)([A-Z][a-z])").unwrap();
    let out1 = re1.replace_all(&input, "$1-$2");
    let re2 = Regex::new(r"([a-z0-9])([A-Z])").unwrap();
    let out2 = re2.replace_all(&out1, "$1-$2");
    let with_boundaries = out2.replace("_", "-").replace(" ", "-");
    let re3 = Regex::new(r"-{2,}").unwrap();
    let mut normalized = re3.replace_all(&with_boundaries, "-").trim_matches('-').to_lowercase();
    if had_underscore { normalized = format!("_{}", normalized); }
    normalized
}

pub fn get_app_id_from_type_name(type_name: &str) -> String {
    let mut ns: Vec<&str> = type_name.split('.').collect();
    if let Some(pos) = ns.iter().position(|&x| x == "Apps") { ns = ns[pos+1..].to_vec(); }
    let friendly: Vec<String> = ns.iter().map(|&s| title_case_to_friendly_url(s.to_string())).collect();
    friendly.join("/")
}
