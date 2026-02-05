using App.Application.Initializers;
using App.Contracts.Services;
using Oci.Common;
using Oci.Common.Auth;
using Oci.Common.Model;
using Oci.ObjectstorageService;
using Oci.ObjectstorageService.Requests;

namespace App.Infrastructure.Oracle;

public class OciPhotoService : IPhotoService, IDisposable
{
    private readonly ObjectStorageClient _client;
    private readonly string _namespace;
    private readonly string _bucketName;
    private bool _disposed;
    
    private static readonly SemaphoreSlim PhotoUploadSemaphore = new(8, 8);
    
    public OciPhotoService(EnvInitializer envInitializer)
    {
        var provider = new SimpleAuthenticationDetailsProvider
        {
            TenantId = envInitializer.OciTenancyId,
            UserId = envInitializer.OciUserId,
            Fingerprint = envInitializer.OciFingerprint,
            Region = Region.FromRegionCodeOrId(envInitializer.OciRegion),
            PrivateKeySupplier = new PrivateKeySupplier(envInitializer.OciKey.Trim().Replace("\\n", "\n"))
        };
        
        _client = new ObjectStorageClient(provider);
        _namespace = _client.GetNamespace(new GetNamespaceRequest()).GetAwaiter().GetResult().Value;
        _bucketName = envInitializer.OciBucketName;
    }

    public async Task<string?> UploadPhotoAsync(string folderName, Guid ownerId, Stream photoStream, string contentType)
    {
        await PhotoUploadSemaphore.WaitAsync();
        try
        {
            string prefix = folderName.EndsWith("/") ? folderName : folderName + "/";
            string extension = GetExtensionFromContentType(contentType);
            
            string objectName = $"{prefix}{ownerId:N}{extension}"; 

            if (photoStream.CanSeek)
            {
                photoStream.Seek(0, SeekOrigin.Begin);
            }
            else if (photoStream.Length <= 0)
            {
                return null;
            }
            
            var request = new PutObjectRequest
            {
                NamespaceName = _namespace,
                BucketName = _bucketName,
                ObjectName = objectName,
                PutObjectBody = photoStream,
                ContentLength = photoStream.Length,
                ContentType = contentType,
                OpcMeta = new Dictionary<string, string>
                {
                    { "uploaded-at", DateTime.UtcNow.ToString("o") }
                }
            };
            
            await _client.PutObject(request);
            return objectName; 
        }
        finally
        {
            PhotoUploadSemaphore.Release();
        }
    }
    
    public async Task<bool> RemovePhotoAsync(string photoPath)
    {
        await PhotoUploadSemaphore.WaitAsync();
        try
        {
            var request = new DeleteObjectRequest
            {
                NamespaceName = _namespace,
                BucketName = _bucketName,
                ObjectName = photoPath
            };

            await _client.DeleteObject(request);
            return true;
        }
        catch (OciException ex)
        {
            return false;
        }
        finally
        {
            PhotoUploadSemaphore.Release();
        }
    }
    
    private string GetExtensionFromContentType(string contentType)
    {
        return contentType?.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/gif" => ".gif",
            _ => string.Empty,
        };
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        _client.Dispose();
        _disposed = true;
    }
}