use axum::{
    Router,
    extract::{
        State,
        ws::{Message, WebSocket, WebSocketUpgrade},
    },
    response::IntoResponse,
    routing::get,
};
use futures_util::StreamExt;
use std::net::SocketAddr;
use std::os::raw::c_char;
use std::sync::Mutex;
use tokio::runtime::Runtime;
use tokio::sync::broadcast;

// Function pointer typedef for Rust -> C# callbacks
pub type CSharpEventCallback = unsafe extern "C" fn(*const u8, i32);

#[repr(C)]
pub struct CServerArgs {
    pub port: i32,
    pub verbose: i32,
}

#[repr(C)]
pub struct FfiWidgetProps {
    pub keys: *const *const c_char,
    pub values: *const *const c_char,
    pub count: i32,
}

#[repr(C)]
pub struct FfiWidget {
    pub id: *const c_char,
    pub component_type: *const c_char,
    pub parent_index: i32,
    pub props: FfiWidgetProps,
}

pub struct ServerState {
    args: CServerArgs,
    rt: Runtime,
    vdom: Mutex<serde_json::Value>,
    tx: broadcast::Sender<String>,
    // Store the global C# function pointer to invoke when frontend events arrive
    c_event_callback: Mutex<Option<CSharpEventCallback>>,
}

#[unsafe(no_mangle)]
pub extern "C" fn rustserver_create(args: *const CServerArgs) -> *mut ServerState {
    if args.is_null() {
        return std::ptr::null_mut();
    }
    let args_val = unsafe { std::ptr::read(args) };

    let rt = tokio::runtime::Builder::new_multi_thread()
        .enable_all()
        .build()
        .unwrap();

    let (tx, _rx) = broadcast::channel(100);

    let state = Box::new(ServerState {
        args: args_val,
        rt,
        vdom: Mutex::new(serde_json::json!({})),
        tx,
        c_event_callback: Mutex::new(None),
    });
    Box::into_raw(state)
}

#[unsafe(no_mangle)]
pub extern "C" fn rustserver_register_callback(
    state_ptr: *mut ServerState,
    callback: Option<CSharpEventCallback>,
) {
    if state_ptr.is_null() {
        return;
    }
    let state = unsafe { &*state_ptr };
    if let Ok(mut guard) = state.c_event_callback.lock() {
        *guard = callback;
        println!("[RustyServer Core] Secured C# Native Callback Pointer!");
    }
}

#[unsafe(no_mangle)]
pub extern "C" fn rustserver_render_json_tree(
    state_ptr: *mut ServerState,
    json_utf8_ptr: *const u8,
    json_len: i32,
) {
    if state_ptr.is_null() || json_utf8_ptr.is_null() || json_len == 0 {
        return;
    }

    let state = unsafe { &*state_ptr };
    let json_bytes = unsafe { std::slice::from_raw_parts(json_utf8_ptr, json_len as usize) };

    match serde_json::from_slice::<serde_json::Value>(json_bytes) {
        Ok(new_tree) => {
            let mut vdom_guard = state.vdom.lock().unwrap();
            let old_tree = &*vdom_guard;

            // Core: Lightning Fast Rust JSON Differ
            let patch = json_patch::diff(old_tree, &new_tree);

            // Only broadcast if there is a difference or if it's the first tree
            let patch_payload = serde_json::to_string(&patch).unwrap();

            if !patch.is_empty() {
                // Broadcast binary MessagePack or plain text JSON patch to all Axum WebSocket listeners
                // For optimal speed, JS `diffpatch` logic requires the `patch` RFC array
                println!(
                    "[RustyServer Differ] Diff calculated exactly! Emitting {} ops.",
                    patch.len()
                );
                let _ = state.tx.send(patch_payload);
            }

            // Update the stored Virtual DOM state
            *vdom_guard = new_tree;
        }
        Err(e) => eprintln!("[RustyServer Core] Failed to fast-parse UI payload: {}", e),
    }
}

#[unsafe(no_mangle)]
pub extern "C" fn rustserver_run(state_ptr: *mut ServerState) {
    if state_ptr.is_null() {
        return;
    }
    let state = unsafe { &mut *state_ptr };

    let port = state.args.port as u16;
    let verbose = state.args.verbose != 0;
    
    // Clone the broadcast sender securely before relinquishing the borrow
    let tx = state.tx.clone();
    
    // We snapshot the current C# callback pointer into an Arc, so we can move it
    let callback_guard = state.c_event_callback.lock().unwrap();
    let c_callback_arc = std::sync::Arc::new(callback_guard.clone());

    state.rt.block_on(async move {
        // Build the Axum router with a WebSockets route for real-time frontend connection
        // We inject a tuple of (tx, callback) as App State so handlers can access both
        let app_state = (tx, c_callback_arc);
        
        let app = Router::new()
            .route("/", get(|| async { "Hello from Rusty Axum Protocol!" }))
            .route("/ws", get(ws_handler))
            .with_state(app_state);

        let addr = SocketAddr::from(([0, 0, 0, 0], port));
        if verbose {
            println!("[RustyServer] Axum WebSocket streaming online at {}", addr);
        }

        if let Ok(listener) = tokio::net::TcpListener::bind(addr).await {
            if let Err(e) = axum::serve(listener, app).await {
                eprintln!("[RustyServer] Axum server error: {}", e);
            }
        } else {
            eprintln!("[RustyServer] Failed to bind to port {}", port);
        }
    });
}

// Axum WebSocket Connection Upgrade
async fn ws_handler(
    ws: WebSocketUpgrade,
    State(app_state): State<(broadcast::Sender<String>, std::sync::Arc<Option<CSharpEventCallback>>)>,
) -> impl IntoResponse {
    ws.on_upgrade(move |socket| handle_socket(socket, app_state.0, app_state.1))
}

// Websocket logic
async fn handle_socket(
    mut socket: WebSocket, 
    tx: broadcast::Sender<String>, 
    c_callback: std::sync::Arc<Option<CSharpEventCallback>>
) {
    let mut rx = tx.subscribe();
    
    println!("[RustyServer WS] New Web Client Connected!");
    
    loop {
        tokio::select! {
            Ok(patch) = rx.recv() => {
                let _ = socket.send(Message::Text(patch.into())).await;
            }
            Some(Ok(msg)) = socket.next() => {
                if let Message::Text(text) = msg {
                    // Instantly bounce the incoming frontend JSON event back to C#
                    if let Some(callback) = *c_callback {
                        let bytes = text.as_bytes();
                        unsafe {
                            callback(bytes.as_ptr(), bytes.len() as i32);
                        }
                    } else {
                        println!("[RustyServer WS] Warning: Received '{}' but no C# Callback pointer is registered!", text);
                    }
                }
            }
            else => break,
        }
    }
}

#[unsafe(no_mangle)]
pub extern "C" fn rustserver_free(state_ptr: *mut ServerState) {
    if !state_ptr.is_null() {
        unsafe {
            let _ = Box::from_raw(state_ptr);
        }
    }
}
