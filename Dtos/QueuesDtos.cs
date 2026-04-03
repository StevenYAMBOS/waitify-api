using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using WaitifyApi.Enums;

namespace WaitifyApi.Models;

public class JoinQueueRequest
{
  [Required]
  public Guid BusinessId { get; set; }
  [Required]
  [Phone]
  public string? Phone { get; set; }
  public string? ClientName { get; set; }
  public string? Status { get; set; }
}
