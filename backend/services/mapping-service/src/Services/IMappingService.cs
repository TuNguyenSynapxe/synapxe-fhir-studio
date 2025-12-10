using MappingService.DTOs;

namespace MappingService.Services;

public interface IMappingService
{
    Task<IEnumerable<MappingResponse>> GetAllMappingsAsync(string? projectId = null);
    Task<MappingResponse?> GetMappingByIdAsync(string id);
    Task<MappingResponse> CreateMappingAsync(MappingRequest request, string createdBy = "system");
    Task<MappingResponse?> UpdateMappingAsync(string id, MappingRequest request, string updatedBy = "system");
    Task<MappingResponse?> PublishMappingAsync(string id, string updatedBy = "system");
    Task<bool> DeleteMappingAsync(string id);
}
