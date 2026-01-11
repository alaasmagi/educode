namespace Contracts;

public interface IPhotoService
{
    Task<string?> UploadPhotoAsync(string folderName, Guid ownerId, Stream photoStream, string contentType);
    Task<bool> RemovePhotoAsync(string photoPath);
}