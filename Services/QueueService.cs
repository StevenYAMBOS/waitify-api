using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using WaitifyApi.Constants;
using WaitifyApi.Data;
using WaitifyApi.Entities;
using WaitifyApi.Helpers;
using WaitifyApi.Models;
using WaitifyApi.Repositories;

namespace WaitifyApi.Services;

public class QueueService(AppDbContext context, IApplicationUserRepository userService, IBusinessRepository businessRepository, ILogger<QueueService> logger) : IQueueRepository
{
  public Task JoinQueueAsync(JoinQueueRequest request)
  {
    throw new NotImplementedException();
  }
}