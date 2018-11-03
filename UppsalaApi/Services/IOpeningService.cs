using UppsalaApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UppsalaApi.Services
{
    public interface IOpeningService
    {
        Task<PagedResults<OpeningResource>> GetOpeningsAsync(
            PagingOptions pagingOptions,
            SortOptions<OpeningResource, OpeningEntity> sortOptions,
            SearchOptions<OpeningResource, OpeningEntity> searchOptions,
            CancellationToken ct);

        Task<PagedResults<OpeningResource>> GetOpeningsByRoomIdAsync(
            Guid roomId,
            PagingOptions pagingOptions,
            SortOptions<OpeningResource, OpeningEntity> sortOptions,
            SearchOptions<OpeningResource, OpeningEntity> searchOptions,
            CancellationToken ct);

        Task<IEnumerable<BookingRange>> GetConflictingSlots(
            Guid roomId,
            DateTimeOffset start,
            DateTimeOffset end,
            CancellationToken ct);


    }
}
