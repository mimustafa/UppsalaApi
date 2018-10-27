using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UppsalaApi.Models;

namespace UppsalaApi.Services
{
    public interface IRoomService
    {

        Task<RoomResource> GetRoomAsync(
            Guid id, 
            CancellationToken ct);

        Task<IEnumerable<RoomResource>> GetRoomsAsync(
            CancellationToken ct);
    }
}
