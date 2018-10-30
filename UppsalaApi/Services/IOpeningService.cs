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
        Task<PagedResults<OpeningResource>> GetOpeningsAsync(PagingOptions pagingOptions, CancellationToken ct);

        Task<IEnumerable<BookingRange>> GetConflictingSlots(
            Guid roomId,
            DateTimeOffset start,
            DateTimeOffset end,
            CancellationToken ct);
    }
}
