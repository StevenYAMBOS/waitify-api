namespace WaitifyApi.Dtos;

public class SendContactInfoDto
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? Subject { get; set; }
    public string? Content { get; set; }
    public IFormFile? File { get; set; }
}