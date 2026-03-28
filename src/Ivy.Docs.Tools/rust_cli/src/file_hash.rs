use std::fs;
use std::path::Path;

#[cfg(unix)]
pub fn write_hash(file_path: &Path, hash: &str) {
    // macOS/Linux
    let _ = xattr::set(file_path, "hash", hash.as_bytes());
}

#[cfg(windows)]
pub fn write_hash(file_path: &Path, hash: &str) {
    // Windows ADS
    if let Some(path_str) = file_path.to_str() {
        let ads_path = format!("{}:hash", path_str);
        let _ = fs::write(&ads_path, hash);
    }
}

#[cfg(unix)]
pub fn read_hash(file_path: &Path) -> Option<String> {
    if let Ok(Some(bytes)) = xattr::get(file_path, "hash") {
        String::from_utf8(bytes).ok()
    } else {
        None
    }
}

#[cfg(windows)]
pub fn read_hash(file_path: &Path) -> Option<String> {
    if let Some(path_str) = file_path.to_str() {
        let ads_path = format!("{}:hash", path_str);
        if Path::new(&ads_path).exists() {
            return fs::read_to_string(&ads_path).ok()
        }
    }
    None
}
