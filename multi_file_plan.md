Goal
- Add a pluggable, reusable multi-file upload handler that reuses the single-file streaming logic (MemoryStreamUploadHandler) without changing existing public contracts (IUploadHandler, UseUpload).

Approach
- Keep IUploadHandler as-is (called once per file by UploadService).
- Extract the core stream-processing from MemoryStreamUploadHandler into a small, stateless processor:
  - Interface: IFileUploadStreamProcessor<T>
    - Task<(T? Content, IEnumerable<float> ProgressUpdates)> ProcessAsync(FileUpload file, Stream stream, CancellationToken ct)
  - Implementation: MemoryStreamProcessor : IFileUploadStreamProcessor<byte[]>
- Re-implement MemoryStreamUploadHandler using MemoryStreamProcessor to preserve current behavior for single-file state.
- Add MultiFileUploadHandler<T> : IUploadHandler
  - ctor: (IState<ImmutableArray<FileUpload<T>>> state, IFileUploadStreamProcessor<T> processor)
  - On HandleUploadAsync:
    - Add file to collection (Status=Loading, Progress=0).
    - Consume processor.ProcessAsync; apply progress updates to the matching item; set Content + Status=Finished on completion.
    - Handle OperationCanceledException → Status=Aborted; Exception → Status=Failed.

Usage
- Single: new MemoryStreamUploadHandler(singleState)
- Multiple: new MultiFileUploadHandler<byte[]>(filesState, new MemoryStreamProcessor())

Notes
- No interface changes required; only add optional IFileUploadStreamProcessor<T> and new handlers.
- Thread-safe updates use state.Set(files => files.Replace(old, updated)).
