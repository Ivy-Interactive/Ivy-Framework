use axum::{
    Router,
    extract::ws::{Message, WebSocket, WebSocketUpgrade},
    response::IntoResponse,
    routing::get,
};
use futures_util::{SinkExt, StreamExt};
use std::net::SocketAddr;
use std::os::raw::c_char;
use tokio::runtime::Runtime;

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

    let state = Box::new(ServerState { args: args_val, rt });
    Box::into_raw(state)
}

#[unsafe(no_mangle)]
pub extern "C" fn rustserver_render_json_tree(
    _state_ptr: *mut ServerState,
    json_utf8_ptr: *const u8,
    json_len: i32,
) {
    if json_utf8_ptr.is_null() || json_len == 0 {
        return;
    }
    
    // Safely convert the pointer buffer to a Rust slice without copying
    let json_bytes = unsafe { std::slice::from_raw_parts(json_utf8_ptr, json_len as usize) };
    
    // Parse the high-speed JSON buffer coming from C#
    match serde_json::from_slice::<serde_json::Value>(json_bytes) {
        Ok(tree) => {
            println!("[RustyServer Core] Fast-Parsed C# Tree! Size: {} bytes. Commencing Virtual DOM diffing...", json_len);
            // TODO: Execute Virtual DOM Diff and queue MessagePack broadcast to Axum WebSockets
        }
        Err(e) => eprintln!("[RustyServer] Failed to parse UI payload from C#: {}", e),
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

    state.rt.block_on(async move {
        // Build the Axum router with a WebSockets route for real-time frontend connection
        let app = Router::new()
            .route("/", get(|| async { "Hello from Rusty Axum!" }))
            .route("/ws", get(ws_handler));

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
async fn ws_handler(ws: WebSocketUpgrade) -> impl IntoResponse {
    ws.on_upgrade(handle_socket)
}

// Websocket logic
async fn handle_socket(mut socket: WebSocket) {
    // We can stream MessagePack binary diff patches to `socket.send` here based on Virtual DOM reconciliation.
    while let Some(Ok(msg)) = socket.next().await {
        if let Message::Text(text) = msg {
            println!("[RustyServer WS] Frontend JS Says: {}", text);
            let _ = socket.send(Message::Text("Ack from Rust".into())).await;
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
