using System.ComponentModel.DataAnnotations;

namespace WaitifyApi.Models;

public record JoinQueueRequest
{
  [Required]
  public Guid BusinessQrCodeToken { get; set; }
  [Required]
  [Phone]
  public string? Phone { get; set; }
  public string? ClientName { get; set; }
}

public record FindQueueEntriesCountRequest
{
  [Required]
  public Guid Id { get; set; }
  [Required]
  public Guid BusinessQrCodeToken { get; set; }
}

public record JoinQueueResponse
{
  public Guid Id { get; set; }
  public Guid BusinessQrCodeToken { get; set; }
  public string? BusinessName { get; set; }
  public int Position { get; set; }
  public int EstimatedWaitTime { get; set; }
  public string? Phone { get; set; }
  public string? ClientName { get; set; }
  public string? Status { get; set; }
  public DateTime CreatedAt { get; set; }
}

public record CallNextClientResponse
{
  public Guid Id { get; set; }
  public Guid BusinessQrCodeToken { get; set; }
  public string? Phone { get; set; }
  public string? ClientName { get; set; }
  public int Position { get; set; }
  public string? Status { get; set; }
  public DateTime? CalledAt { get; set; }
}

public record CancelQueueEntryResponse
{
  public Guid Id { get; set; }
  public Guid BusinessQrCodeToken { get; set; }
  public string? Phone { get; set; }
  public string? ClientName { get; set; }
  public string? Status { get; set; }
  public DateTime UpdatedAt { get; set; }
}

public record MarkClientAsServedRequest
{
  public int? ActualServiceTime { get; set; }
}

public record MarkClientAsServedResponse
{
  public Guid Id { get; set; }
  public Guid BusinessQrCodeToken { get; set; }
  public string? Phone { get; set; }
  public string? ClientName { get; set; }
  public string? Status { get; set; }
  public DateTime? CalledAt { get; set; }
  public DateTime? ServedAt { get; set; }
  public int? ActualServiceTime { get; set; }
}

public record QueueEntriesCountForBusinessResponse
{
  public int Count { get; set; } = 0;
}