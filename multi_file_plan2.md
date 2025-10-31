Goal
- One clean MemoryStreamUploadHandler that supports both single and multiple file states without duplicating buffer code.

Design
- Keep existing public interfaces: IUploadHandler and UseUpload remain unchanged.
- Encapsulate sink abstraction inside the handler only (not public):
  - private interface IFileUploadSink<TContent> inside MemoryStreamUploadHandler
  - private SingleFileSink and private ImmutableArraySink implementations
- Centralize stream reading in one private helper to avoid duplication:
  - private static Task<byte[]> ReadAllWithProgressAsync(Stream s, int chunkSize, long length, Action<float> onProgress, CancellationToken ct)

Handler API
- class MemoryStreamUploadHandler : IUploadHandler
  - static factory methods (preferred; keep ctor internal):
    - Create(IState<FileUpload<byte[]>?> single, int chunkSize = 8192) : IUploadHandler
    - Create(IState<System.Collections.Immutable.ImmutableArray<FileUpload<byte[]>>> many, int chunkSize = 8192) : IUploadHandler
    - Optional generic for other collections if needed later:
      - Create<TCollection>(IState<TCollection> many, Func<TCollection, FileUpload, TCollection> add, Func<TCollection, Guid, Func<FileUpload, FileUpload>, TCollection> updateById, int chunkSize = 8192) : IUploadHandler

Flow
1) On HandleUploadAsync(file, stream, ct):
   - var key = sink.Start(file with { Status = Loading, Progress = 0 });
   - var bytes = await ReadAllWithProgressAsync(stream, chunkSize, file.Length, p => sink.Progress(key, p), ct);
   - sink.Complete(key, bytes);
2) catch (OperationCanceledException): sink.Aborted(key)
3) catch (Exception): sink.Failed(key); throw;

Notes
- No changes to IUploadHandler or UseUpload contracts.
- Factory methods align with desired usage: MemoryStreamUploadHandler.Create(singleState) and MemoryStreamUploadHandler.Create(immutableArrayState).
- All buffer/stream logic lives in a single helper to avoid duplication.
