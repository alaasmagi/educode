using Contracts;
using Oci.Common;
using Oci.Common.Auth;
using Oci.Common.Model;
using Oci.ObjectstorageService;
using Oci.ObjectstorageService.Requests;

namespace App.BLL;

public class OciPhotoService : IPhotoService
{
    private readonly ObjectStorageClient _client;
    private readonly string _namespace;
    private readonly string _bucketName;
    
    public OciPhotoService(EnvInitializer env)
    {
        var provider = new SimpleAuthenticationDetailsProvider
        {
            TenantId = env.OciTenancyId,
            UserId = env.OciUserId,
            Fingerprint = env.OciFingerprint,
            Region = Region.FromRegionCodeOrId(env.OciRegion),
            PrivateKeySupplier = new PrivateKeySupplier(env.OciKey.Trim().Replace("\\n", "\n"))
        };
        
        _client = new ObjectStorageClient(provider);
        _namespace = _client.GetNamespace(new GetNamespaceRequest()).GetAwaiter().GetResult().Value;
        _bucketName = env.OciBucketName;
    }

    public async Task<string?> UploadPhotoAsync(string folderName, Guid ownerId, Stream photoStream, string contentType)
    {
        string prefix = folderName.EndsWith("/") ? folderName : folderName + "/";
        string extension = Helpers.GetExtensionFromContentType(contentType);
        
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
    
    public async Task<bool> RemovePhotoAsync(string photoPath)
    {
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
    }
}