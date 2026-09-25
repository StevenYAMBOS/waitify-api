
using WaitifyApi.Entities;
using WaitifyApi.Models;

namespace WaitifyApi.Repositories
{
  public interface IQueueRepository
  {
    Task<QueueEntries?> FindQueueByIdAsync(Guid id);
    Task<JoinQueueResponse> JoinQueueAsync(JoinQueueRequest request);
    Task<IEnumerable<QueueEntryResponse>> GetActiveEntriesAsync(Guid qrCodeToken, string ownerId);
    Task<CallNextClientResponse> CallNextClientAsync(Guid qrCodeToken, string ownerId);
    Task<CancelQueueEntryResponse> CancelQueueEntryAsync(Guid id);
    Task<MarkClientAsServedResponse> MarkClientAsServedAsync(Guid id, string ownerId, MarkClientAsServedRequest request);
  }
}