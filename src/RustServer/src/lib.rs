use std::ffi::CString;
use std::os::raw::c_char;

// Removed ServerState entirely because C# manages its own view caching perfectly.
// We provide purely memory-safe stateless math computations.

#[unsafe(no_mangle)]
pub extern "C" fn rustserver_free_string(s: *mut c_char) {
    if s.is_null() {
        return;
    }
    unsafe {
        drop(CString::from_raw(s));
    }
}

/// Computes the JSON patch delta array statically.
/// If `out_len` > 0, the returned pointer is a valid UTF-8 CString you must free via `rustserver_free_string`.
#[unsafe(no_mangle)]
pub extern "C" fn rustserver_diff_trees(
    old_json_ptr: *const u8,
    old_json_len: i32,
    new_json_ptr: *const u8,
    new_json_len: i32,
    out_len: *mut i32,
) -> *mut c_char {
    if old_json_ptr.is_null() || new_json_ptr.is_null() || old_json_len == 0 || new_json_len == 0 {
        if !out_len.is_null() {
            unsafe { *out_len = 0; }
        }
        return std::ptr::null_mut();
    }
    
    let old_bytes = unsafe { std::slice::from_raw_parts(old_json_ptr, old_json_len as usize) };
    let new_bytes = unsafe { std::slice::from_raw_parts(new_json_ptr, new_json_len as usize) };
    
    let old_tree: serde_json::Value = match serde_json::from_slice(old_bytes) {
        Ok(t) => t,
        Err(_) => {
            if !out_len.is_null() { unsafe { *out_len = 0; } }
            return std::ptr::null_mut();
        }
    };
    
    let new_tree: serde_json::Value = match serde_json::from_slice(new_bytes) {
        Ok(t) => t,
        Err(_) => {
            if !out_len.is_null() { unsafe { *out_len = 0; } }
            return std::ptr::null_mut();
        }
    };
            
    // Fast RFC6902 JSON Diff mathematically independent of state
    let patch = json_patch::diff(&old_tree, &new_tree);
    
    if patch.is_empty() {
        if !out_len.is_null() {
            unsafe { *out_len = 0; }
        }
        return std::ptr::null_mut();
    }
    
    if let Ok(patch_str) = serde_json::to_string(&patch) {
        if !out_len.is_null() {
            unsafe { *out_len = patch_str.len() as i32; }
        }
        if let Ok(c_str) = CString::new(patch_str) {
            return c_str.into_raw();
        }
        return std::ptr::null_mut();
    }
    
    if !out_len.is_null() {
        unsafe { *out_len = 0; }
    }
    std::ptr::null_mut()
}
