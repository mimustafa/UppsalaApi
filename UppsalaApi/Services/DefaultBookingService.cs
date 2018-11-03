using AutoMapper;
using AutoMapper.QueryableExtensions;
using UppsalaApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace UppsalaApi.Services
{
    public class DefaultBookingService : IBookingService
    {
        private readonly UppsalaApiContext _context;
        private readonly IDateLogicService _dateLogicService;
        private readonly UserManager<UserEntity> _userManager;

        public DefaultBookingService(
            UppsalaApiContext context,
            IDateLogicService dateLogicService,
            UserManager<UserEntity> userManager)
        {
            _context = context;
            _dateLogicService = dateLogicService;
            _userManager = userManager;
        }

        public async Task<Guid> CreateBookingAsync(
            Guid userId,
            Guid roomId,
            DateTimeOffset startAt,
            DateTimeOffset endAt,
            CancellationToken ct)
        {
            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Id == userId, ct);
            if (user == null) throw new InvalidOperationException("You must be logged in.");

            var room = await _context.Rooms
                .SingleOrDefaultAsync(r => r.Id == roomId, ct);
            if (room == null) throw new ArgumentException("Invalid room id.");

            var minimumStay = _dateLogicService.GetMinimumStay();
            var total = (int)((endAt - startAt).TotalHours / minimumStay.TotalHours) * room.Rate;

            var id = Guid.NewGuid();

            var newBooknig = _context.Bookings.Add(new BookingEntity
            {
                Id = id,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow,
                StartAt = startAt.ToUniversalTime(),
                EndAt = endAt.ToUniversalTime(),
                Room = room,
                User = user,
                Total = total
            });

            var created = await _context.SaveChangesAsync(ct);
            if (created < 1) throw new InvalidOperationException("Could not create the booking.");

            return id;
        }



        public async Task DeleteBookingAsync(Guid bookingId, CancellationToken ct)
        {
            var booking = await _context.Bookings
                .SingleOrDefaultAsync(b => b.Id == bookingId, ct);
            if (booking == null) return;

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<BookingResource> GetBookingAsync(
            Guid bookingId,
            CancellationToken ct)
        {
            var entity = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                .SingleOrDefaultAsync(b => b.Id == bookingId, ct);

            if (entity == null) return null;

            return Mapper.Map<BookingResource>(entity);
        }

        public async Task<BookingResource> GetBookingForUserIdAsync(
            Guid bookingId,
            Guid userId,
            CancellationToken ct)
        {
            var entity = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                .SingleOrDefaultAsync(b => b.Id == bookingId && b.User.Id == userId, ct);

            if (entity == null) return null;

            return Mapper.Map<BookingResource>(entity);
        }

        public async Task<PagedResults<BookingResource>> GetBookingsAsync(
            PagingOptions pagingOptions,
            SortOptions<BookingResource, BookingEntity> sortOptions,
            SearchOptions<BookingResource, BookingEntity> searchOptions,
            CancellationToken ct)
        {
            IQueryable<BookingEntity> query = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room);
            query = searchOptions.Apply(query);
            query = sortOptions.Apply(query);

            var size = await query.CountAsync(ct);

            var items = await query
                .Skip(pagingOptions.Offset.Value)
                .Take(pagingOptions.Limit.Value)
                .ProjectTo<BookingResource>()
                .ToArrayAsync(ct);

            return new PagedResults<BookingResource>
            {
                Items = items,
                TotalSize = size
            };
        }

        public async Task<PagedResults<BookingResource>> GetBookingsForUserIdAsync(
            Guid userId,
            PagingOptions pagingOptions,
            SortOptions<BookingResource, BookingEntity> sortOptions,
            SearchOptions<BookingResource, BookingEntity> searchOptions,
            CancellationToken ct)
        {
            IQueryable<BookingEntity> query = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                .Where(b => b.User.Id == userId);
            query = searchOptions.Apply(query);
            query = sortOptions.Apply(query);

            var size = await query.CountAsync(ct);

            var items = await query
                .Skip(pagingOptions.Offset.Value)
                .Take(pagingOptions.Limit.Value)
                .ProjectTo<BookingResource>()
                .ToArrayAsync(ct);

            return new PagedResults<BookingResource>
            {
                Items = items,
                TotalSize = size
            };
        }
    }
}