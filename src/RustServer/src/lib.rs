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
pub struct FfiWidget {
    pub id: *const c_char,
    pub type_id: i32,
    pub parent_index: i32,
    pub text_val: *const c_char,
    pub number_val: f64,
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
pub extern "C" fn rustserver_render_tree(
    _state_ptr: *mut ServerState,
    _widgets_ptr: *const FfiWidget,
    widgets_len: i32,
) {
    // In the future: Read the pointer length, reconstruct the `FfiWidget` slice.
    // Box the elements and process the virtual DOM diffing logic.
    println!(
        "[RustyServer Core] Received C# Tree Sync: {} nodes.",
        widgets_len
    );
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
