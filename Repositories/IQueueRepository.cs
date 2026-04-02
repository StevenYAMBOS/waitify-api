
using WaitifyApi.Models;

namespace WaitifyApi.Repositories
{
  public interface IQueueRepository
  {
    Task<(bool Success, IEnumerable<string>? Errors)> JoinQueueAsync(JoinQueueRequest request);
  }
}