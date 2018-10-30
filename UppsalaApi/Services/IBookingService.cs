using UppsalaApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UppsalaApi.Services
{
    public interface IBookingService
    {
        Task<BookingResoure> GetBookingAsync(Guid bookingId, CancellationToken ct);

        Task<Guid> CreateBookingAsync(
            Guid userId,
            Guid roomId,
            DateTimeOffset startAt,
            DateTimeOffset endAt,
            CancellationToken ct);
    }
}
