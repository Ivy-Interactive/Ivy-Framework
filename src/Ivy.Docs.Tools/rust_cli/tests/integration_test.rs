use std::fs;
use std::process::Command;
use std::path::PathBuf;

fn get_bin_path() -> PathBuf {
    let mut exe = std::env::current_exe().unwrap();
    // Path is usually target/debug/deps/integration_test-HASH
    exe.pop();
    if exe.ends_with("deps") {
        exe.pop();
    }
    exe.push("rust_cli");
    if std::env::consts::OS == "windows" {
        exe.set_extension("exe");
    }
    exe
}

#[test]
fn test_stale_files_are_pruned() {
    let bin_path = get_bin_path();
    
    // Create temp input and output dirs
    let temp_dir = std::env::temp_dir().join("ivy_docs_cli_test_prune");
    let input_dir = temp_dir.join("input");
    let output_dir = temp_dir.join("output");
    
    let _ = fs::remove_dir_all(&temp_dir);
    fs::create_dir_all(&input_dir).unwrap();
    fs::create_dir_all(&output_dir).unwrap();
    
    // Create dummy project file to satisfy cli
    fs::write(input_dir.join("TestProj.csproj"), "<RootNamespace>TestApp</RootNamespace>").unwrap();
    
    // Write an initial .md file
    let md_path_1 = input_dir.join("Page1.md");
    fs::write(&md_path_1, "# Page 1").unwrap();
    
    // Also simulate a stale generated file existing in output_dir
    let stale_g_cs = output_dir.join("StalePage.g.cs");
    let stale_md = output_dir.join("StalePage.md");
    fs::write(&stale_g_cs, "// Stale Code").unwrap();
    fs::write(&stale_md, "# Stale Content").unwrap();

    // Run cli
    let status = Command::new(&bin_path)
        .arg("convert")
        .arg(input_dir.to_str().unwrap())
        .arg(output_dir.to_str().unwrap())
        .status()
        .expect("Failed to execute cli");
        
    assert!(status.success());
    
    // Check old files are pruned
    assert!(!stale_g_cs.exists(), "Stale .g.cs file should have been pruned!");
    assert!(!stale_md.exists(), "Stale .md file should have been pruned!");
    
    // Check new files generated
    let new_g_cs = output_dir.join("Page1.g.cs");
    let new_md = output_dir.join("Page1.md");
    assert!(new_g_cs.exists(), "Expected new .g.cs file to be generated");
    assert!(new_md.exists(), "Expected new .md file to be generated");
    
    // Provide a second run to ensure file deletion triggers properly
    // Remove Page1.md from input, expect it to be pruned from output
    fs::remove_file(&md_path_1).unwrap();
    
    let status2 = Command::new(&bin_path)
        .arg("convert")
        .arg(input_dir.to_str().unwrap())
        .arg(output_dir.to_str().unwrap())
        .status()
        .expect("Failed to execute cli");
        
    assert!(status2.success());
    
    // Page1 files should now be gone from output
    assert!(!new_g_cs.exists(), "Page1.g.cs should have been pruned after source md removed!");
    assert!(!new_md.exists(), "Page1.md should have been pruned after source md removed!");
    
    let _ = fs::remove_dir_all(&temp_dir);
}
