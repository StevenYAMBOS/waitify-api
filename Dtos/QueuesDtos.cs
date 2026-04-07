using System.ComponentModel.DataAnnotations;

namespace WaitifyApi.Models;

public class JoinQueueRequest
{
  [Required]
  public Guid QrCodeToken { get; set; }
  [Required]
  [Phone]
  public string? Phone { get; set; }
  public string? ClientName { get; set; }
}

public class JoinQueueResponse
{
  public Guid Id { get; set; }
  public Guid BusinessId { get; set; }
  public string? BusinessName { get; set; }
  public int Position { get; set; }
  public int EstimatedWaitTime { get; set; }
  public string? Phone { get; set; }
  public string? ClientName { get; set; }
  public string? Status { get; set; }
  public DateTime CreatedAt { get; set; }
}

public class CallNextClientResponse
{
  public Guid Id { get; set; }
  public Guid BusinessId { get; set; }
  public string? Phone { get; set; }
  public string? ClientName { get; set; }
  public int Position { get; set; }
  public string? Status { get; set; }
  public DateTime? CalledAt { get; set; }
}

public class CancelQueueEntryResponse
{
  public Guid Id { get; set; }
  public Guid BusinessId { get; set; }
  public string? Phone { get; set; }
  public string? ClientName { get; set; }
  public string? Status { get; set; }
  public DateTime UpdatedAt { get; set; }
}
