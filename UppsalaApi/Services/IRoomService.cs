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

        Task<PagedResults<RoomResource>> GetRoomsAsync(
            PagingOptions pagingOptions,
            SortOptions<RoomResource, RoomEntity> sortOptions,
            SearchOptions<RoomResource, RoomEntity> searchOptions,
            CancellationToken ct);
    }
}
