use std::os::raw::c_char;
use std::ffi::CString;

#[unsafe(no_mangle)]
pub extern "C" fn rustserver_say_hello() -> *mut c_char {
    let s = CString::new("Hello from RustyServer!").unwrap();
    s.into_raw()
}

#[unsafe(no_mangle)]
pub extern "C" fn rustserver_free_string(s: *mut c_char) {
    if s.is_null() { return; }
    unsafe {
        let _ = CString::from_raw(s);
    }
}
