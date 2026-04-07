
using WaitifyApi.Entities;
using WaitifyApi.Models;

namespace WaitifyApi.Repositories
{
  public interface IQueueRepository
  {
    Task<QueueEntries?> FindQueueByIdAsync(Guid id);
    Task<JoinQueueResponse> JoinQueueAsync(JoinQueueRequest request);
    Task<CallNextClientResponse> CallNextClientAsync(Guid businessId);
    Task<CancelQueueEntryResponse> CancelQueueEntryAsync(Guid id);
  }
}