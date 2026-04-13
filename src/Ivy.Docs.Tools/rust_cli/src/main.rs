use clap::{Parser, Subcommand};
use rayon::prelude::*;
use std::fs;
use std::path::{Path, PathBuf};

mod utils;
mod link_converter;
mod converter;
mod llm_markdown;

#[derive(Parser)]
#[command(name = "ivy_docs_cli")]
#[command(about = "High performance Markdown to C# Ivy compiler")]
struct Cli {
    #[command(subcommand)]
    command: Commands,
}

#[derive(Subcommand)]
enum Commands {
    Convert {
        input_folder: String,
        output_folder: String,
        #[arg(long)]
        skip_if_not_changed: bool,
        #[arg(long)]
        api_docs: Option<String>,
    },
}

fn get_root_namespace(project_file: &Path) -> Option<String> {
    if let Ok(content) = std::fs::read_to_string(project_file) {
        if let Some(start) = content.find("<RootNamespace>") {
            let rest = &content[start + 15..];
            if let Some(end) = rest.find("</RootNamespace>") {
                return Some(rest[..end].trim().to_string());
            }
        }
    }
    // Fallback to project file name without .csproj
    if let Some(name) = project_file.file_stem() {
        if let Some(s) = name.to_str() {
            return Some(s.to_string());
        }
    }
    None
}

fn get_project_file(start_folder: &Path) -> Option<PathBuf> {
    let mut current = Some(start_folder);
    while let Some(dir) = current {
        if let Ok(entries) = fs::read_dir(dir) {
            for entry in entries.flatten() {
                let path = entry.path();
                if path.extension().map_or(false, |ext| ext == "csproj") {
                    return Some(path);
                }
            }
        }
        current = dir.parent();
    }
    None
}

fn main() {
    let cli = Cli::parse();
    match cli.command {
        Commands::Convert { input_folder, output_folder, skip_if_not_changed, api_docs } => {
            let api_docs_manifest = llm_markdown::load_api_docs_manifest(
                api_docs.as_ref().map(|p| Path::new(p.as_str()))
            );
            let is_glob = input_folder.contains('*') || input_folder.contains('?');
            let (input_dir, pattern) = if is_glob {
                let p = Path::new(&input_folder);
                let dir = p.parent().unwrap_or(Path::new("")).to_path_buf();
                let pat = p.file_name().unwrap_or_default().to_str().unwrap().to_string();
                (dir, pat)
            } else {
                (PathBuf::from(&input_folder), "*.md".to_string())
            };
            
            let input_dir = fs::canonicalize(&input_dir).unwrap_or(input_dir);
            let out_p = PathBuf::from(&output_folder);
            fs::create_dir_all(&out_p).unwrap();
            let output_dir = fs::canonicalize(&out_p).unwrap_or(out_p);
            
            let proj_file = get_project_file(&input_dir).expect("No .csproj found");
            let root_namespace = get_root_namespace(&proj_file).expect("No <RootNamespace> found");
            
            let mut paths = Vec::new();
            if is_glob {
                let re = glob::Pattern::new(&pattern).unwrap();
                for entry in walkdir::WalkDir::new(&input_dir) {
                    if let Ok(e) = entry {
                        if e.file_type().is_file() {
                            if let Some(name) = e.file_name().to_str() {
                                if re.matches(name) {
                                    paths.push(e.into_path());
                                }
                            }
                        }
                    }
                }
            } else {
                for entry in walkdir::WalkDir::new(&input_dir) {
                    if let Ok(e) = entry {
                        if e.path().extension().map_or(false, |ext| ext == "md") {
                            paths.push(e.into_path());
                        }
                    }
                }
            }
            
            println!("Converting {} files...", paths.len());
            
            let manifest_content = api_docs.as_ref().and_then(|p| fs::read_to_string(p).ok());
            let manifest_hash = manifest_content.as_ref().map(|c| llm_markdown::get_short_hash(c, 8));

            // Parallel processing
            let generated_files: std::collections::HashSet<PathBuf> = paths.par_iter().flat_map(|absolute_input_path| {
                let filename = absolute_input_path.file_name().unwrap().to_str().unwrap();
                let (mut order, name) = utils::get_order_from_file_name(filename);
                
                if name == "_Index" {
                    if let Some(parent) = absolute_input_path.parent() {
                        let parent_name = parent.file_name().unwrap().to_str().unwrap();
                        let (o, _) = utils::get_order_from_file_name(parent_name);
                        order = o;
                    }
                }
                
                let relative_input_path = absolute_input_path.strip_prefix(&input_dir).unwrap_or(absolute_input_path);
                let relative_input_path_str = relative_input_path.to_str().unwrap().replace("\\", "/");
                
                let relative_output_path = utils::get_relative_folder_without_order(&input_dir, absolute_input_path);
                
                let mut folder = output_dir.clone();
                if !relative_output_path.is_empty() {
                    folder.push(&relative_output_path);
                }
                
                fs::create_dir_all(&folder).unwrap();
                
                let mut ivy_output = folder.clone();
                ivy_output.push(format!("{}.g.cs", name));

                let mut md_output = folder.clone();
                md_output.push(format!("{}.md", name));

                let mut namespace_suffix = relative_output_path.replace("/", ".").replace("\\", ".");
                if namespace_suffix.starts_with("Generated.") {
                    namespace_suffix = namespace_suffix["Generated.".len()..].to_string();
                }
                
                let namespace = if namespace_suffix.is_empty() {
                    format!("{}.Apps", root_namespace)
                } else {
                    format!("{}.Apps.{}", root_namespace, namespace_suffix)
                };
                
                if let Err(e) = converter::convert_async(
                    &name,
                    &relative_input_path_str,
                    absolute_input_path,
                    &ivy_output,
                    &namespace,
                    skip_if_not_changed,
                    order
                ) {
                    println!("Error converting {}: {}", name, e);
                }

                if let Err(e) = llm_markdown::generate(
                    absolute_input_path,
                    &md_output,
                    skip_if_not_changed,
                    &api_docs_manifest,
                    manifest_hash.as_deref(),
                ) {
                    println!("Error generating LLM markdown for {}: {}", name, e);
                }
                
                vec![ivy_output, md_output]
            }).collect();
            
            // Prune old files
            for entry in walkdir::WalkDir::new(&output_dir) {
                if let Ok(e) = entry {
                    let p = e.path();
                    if p.is_file() {
                        let file_name = p.file_name().unwrap_or_default().to_string_lossy();
                        if file_name.ends_with(".g.cs") || file_name.ends_with(".md") {
                            if !generated_files.contains(p) {
                                println!("Pruning stale output file: {:?}", p);
                                let _ = fs::remove_file(p);
                            }
                        }
                    }
                }
            }
        }
    }
}
