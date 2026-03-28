use std::fs;
use std::path::Path;

pub fn write_hash(file_path: &Path, hash: &str) {
    if let Some(path_str) = file_path.to_str() {
        let hash_file = format!("{}.hash", path_str);
        let _ = fs::write(&hash_file, hash);
    }
}

pub fn read_hash(file_path: &Path) -> Option<String> {
    if let Some(path_str) = file_path.to_str() {
        let hash_file = format!("{}.hash", path_str);
        if Path::new(&hash_file).exists() {
            return fs::read_to_string(&hash_file).ok();
        }
    }
    None
}
