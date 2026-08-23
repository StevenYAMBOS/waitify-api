using WaitifyApi.Entities;

namespace WaitifyApi.Dtos;

public record SendContactInfoRequest
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? Subject { get; set; }
    public string? Content { get; set; }
    public IFormFile? File { get; set; }
}

public record AdminGetAllWaitifyContactsResponse
{
    public int Count { get; set; } = 0;
    public required IEnumerable<Contact?>? Contacts { get; set; } = null;
}

public record AdminDeleteContactResponse
{
    public bool Success { get; set; } = false;
    public string Message { get; set; } = string.Empty;
}
