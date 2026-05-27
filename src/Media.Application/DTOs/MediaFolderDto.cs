namespace Media.Application.DTOs;

public record MediaFolderDto(Guid Id, string Name, Guid? ParentFolderId, int ItemCount, DateTime CreatedAt);
