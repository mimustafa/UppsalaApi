using AutoMapper;
using AutoMapper.QueryableExtensions;
using UppsalaApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UppsalaApi.Services
{
    public class DefaultOpeningService : IOpeningService
    {
        private readonly UppsalaApiContext _context;
        private readonly IDateLogicService _dateLogicService;

        public DefaultOpeningService(UppsalaApiContext context, IDateLogicService dateLogicService)
        {
            _context = context;
            _dateLogicService = dateLogicService;
        }

        public async Task<PagedResults<OpeningResource>> GetOpeningsAsync(
            PagingOptions pagingOptions,
            SortOptions<OpeningResource, OpeningEntity> sortOptions,
            SearchOptions<OpeningResource, OpeningEntity> searchOptions,
            CancellationToken ct)
        {
            var rooms = await _context.Rooms.ToArrayAsync();

            return await GetOpeningsForRoomsAsync(
                  rooms, pagingOptions, sortOptions, searchOptions, ct);
        }



        public async Task<PagedResults<OpeningResource>> GetOpeningsByRoomIdAsync(
            Guid roomId,
            PagingOptions pagingOptions,
            SortOptions<OpeningResource, OpeningEntity> sortOptions,
            SearchOptions<OpeningResource, OpeningEntity> searchOptions,
            CancellationToken ct)
        {
            var room = await _context.Rooms.SingleOrDefaultAsync(r => r.Id == roomId, ct);
            if (room == null) throw new ArgumentException("Invalid room id.");

            return await GetOpeningsForRoomsAsync(
                new[] { room }, pagingOptions, sortOptions, searchOptions, ct);
        }


        private async Task<PagedResults<OpeningResource>> GetOpeningsForRoomsAsync(
            RoomEntity[] rooms,
            PagingOptions pagingOptions,
            SortOptions<OpeningResource, OpeningEntity> sortOptions,
            SearchOptions<OpeningResource, OpeningEntity> searchOptions,
            CancellationToken ct)
        {
            var allOpenings = new List<OpeningEntity>();

            foreach (var room in rooms)
            {
                // Generate a sequence of raw opening slots
                var allPossibleOpenings = _dateLogicService.GetAllSlots(
                        DateTimeOffset.UtcNow,
                        _dateLogicService.FurthestPossibleBooking(DateTimeOffset.UtcNow))
                    .ToArray();

                var conflictedSlots = await GetConflictingSlots(
                    room.Id,
                    allPossibleOpenings.First().StartAt,
                    allPossibleOpenings.Last().EndAt,
                    ct);

                // Remove the slots that have conflicts and project
                var openings = allPossibleOpenings
                    .Except(conflictedSlots, new BookingRangeComparer())
                    .Select(slot => new OpeningEntity
                    {
                        RoomId = room.Id,
                        Rate = room.Rate,
                        StartAt = slot.StartAt,
                        EndAt = slot.EndAt
                    });

                allOpenings.AddRange(openings);
            }

            var pseudoQuery = allOpenings.AsQueryable();
            pseudoQuery = searchOptions.Apply(pseudoQuery);
            pseudoQuery = sortOptions.Apply(pseudoQuery);

            var size = pseudoQuery.Count();

            var items = pseudoQuery
                .Skip(pagingOptions.Offset.Value)
                .Take(pagingOptions.Limit.Value)
                .ProjectTo<OpeningResource>()
                .ToArray();

            return new PagedResults<OpeningResource>
            {
                TotalSize = size,
                Items = items
            };
        }

        public async Task<IEnumerable<BookingRange>> GetConflictingSlots(
            Guid roomId,
            DateTimeOffset start,
            DateTimeOffset end,
            CancellationToken ct)
        {
            return await _context.Bookings
                .Where(b => b.Room.Id == roomId && _dateLogicService.DoesConflict(b, start, end))
                // Split each existing booking up into a set of atomic slots
                .SelectMany(existing => _dateLogicService.GetAllSlots(existing.StartAt, existing.EndAt))
                .ToArrayAsync(ct);
        }

    }
}