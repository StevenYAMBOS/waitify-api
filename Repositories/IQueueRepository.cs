
using WaitifyApi.Models;

namespace WaitifyApi.Repositories
{
  public interface IQueueRepository
  {
    Task<JoinQueueResponse> JoinQueueAsync(JoinQueueRequest request);
  }
}