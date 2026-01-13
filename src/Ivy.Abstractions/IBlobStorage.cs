using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ivy.Abstractions;

public interface IBlobStorage
{
    Task<bool> CreateContainerAsync(string containerName, CancellationToken cancellationToken = default);

    Task<bool> DeleteContainerAsync(string containerName, CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> ListContainersAsync(CancellationToken cancellationToken = default);

    Task<bool> ContainerExistsAsync(string containerName, CancellationToken cancellationToken = default);

    Task UploadAsync(string containerName, string blobName, Stream data, string? contentType = null,
        CancellationToken cancellationToken = default);

    Task<Stream> DownloadAsync(string containerName, string blobName,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string containerName, string blobName,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string containerName, string blobName,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<BlobInfo>> ListBlobsAsync(string containerName, string? prefix = null,
        CancellationToken cancellationToken = default);
}

public sealed record BlobInfo(string Name, string ContentType, long Size, DateTime LastModified);
