using WaitifyApi.Entities;

namespace WaitifyApi.Dtos;

public record SendContactInfoDto
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? Subject { get; set; }
    public string? Content { get; set; }
    public IFormFile? File { get; set; }
}

public record AdminGetAllWaitifyContactsResponse
{
    public int Count { get; set; }
    public required IEnumerable<Contact?>? Contacts { get; set; } = null;
}