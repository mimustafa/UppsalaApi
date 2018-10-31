using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UppsalaApi.Models;
using AutoMapper;
using System.Collections.Generic;
using AutoMapper.QueryableExtensions;
//using AutoMapper.QueryableExtensions;

namespace UppsalaApi.Services
{
	public class DefaultRoomService : IRoomService
    {

        private readonly UppsalaApiContext _context;
    

        public DefaultRoomService(UppsalaApiContext context)
        {
            _context = context;
        }

        public async Task<RoomResource> GetRoomAsync(Guid id, CancellationToken cancellationToken)
        {
            var entity = await _context.Rooms.SingleOrDefaultAsync(r => r.Id == id, cancellationToken);
            if (entity == null) return null;

            return Mapper.Map<RoomResource>(entity);
        }

        public async Task<PagedResults<RoomResource>> GetRoomsAsync(
            PagingOptions pagingOptions,
            SortOptions<RoomResource, RoomEntity> sortOptions,
            SearchOptions<RoomResource, RoomEntity> searchOptions,
            CancellationToken cancellationToken)
        {
            IQueryable<RoomEntity> query = _context.Rooms;
            query = searchOptions.Apply(query);
            query = sortOptions.Apply(query);

            var size = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip(pagingOptions.Offset.Value)
                .Take(pagingOptions.Limit.Value)
                .ProjectTo<RoomResource>()
                .ToArrayAsync(cancellationToken);

            return new PagedResults<RoomResource>
            {
                Items = items,
                TotalSize = size
            };
        }

    }
}
