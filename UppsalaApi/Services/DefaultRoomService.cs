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

        public async Task<IEnumerable<RoomResource>> GetRoomsAsync(CancellationToken cancellationToken)
        {
            var query = _context.Rooms.ProjectTo<RoomResource>();

            return await query.ToArrayAsync();
        }

    }
}
