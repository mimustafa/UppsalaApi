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
        Task<BookingResource> GetBookingAsync(Guid bookingId, CancellationToken ct);

        Task<Guid> CreateBookingAsync(
            Guid userId,
            Guid roomId,
            DateTimeOffset startAt,
            DateTimeOffset endAt,
            CancellationToken ct);


        Task DeleteBookingAsync(Guid bookingId, CancellationToken ct);

        Task<PagedResults<BookingResource>> GetBookingsAsync(
            PagingOptions pagingOptions,
            SortOptions<BookingResource, BookingEntity> sortOptions,
            SearchOptions<BookingResource, BookingEntity> searchOptions,
            CancellationToken ct);

        Task<BookingResource> GetBookingForUserIdAsync(
            Guid bookingId,
            Guid userId,
            CancellationToken ct);

        Task<PagedResults<BookingResource>> GetBookingsForUserIdAsync(
            Guid userId,
            PagingOptions pagingOptions,
            SortOptions<BookingResource, BookingEntity> sortOptions,
            SearchOptions<BookingResource, BookingEntity> searchOptions,
            CancellationToken ct);

    }
}
