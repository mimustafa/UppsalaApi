using UppsalaApi.Infrastructure;
using UppsalaApi.Models;
using UppsalaApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UppsalaApi.Controllers
{
    [Route("/[controller]")]
    public class RoomsController : Controller
    {
        private readonly IRoomService _roomService;
        private readonly IOpeningService _openingService;
        private readonly IDateLogicService _dateLogicService;
        private readonly IBookingService _bookingService;
        private readonly IUserService _userService;
        private readonly PagingOptions _defaultPagingOptions;

        public RoomsController(
            IRoomService roomService,
            IOpeningService openingService,
            IDateLogicService dateLogicService,
            IBookingService bookingService,
            IUserService userService,
            IOptions<PagingOptions> defaultPagingOptionsAccessor)
        {
            _roomService = roomService;
            _openingService = openingService;
            _dateLogicService = dateLogicService;
            _bookingService = bookingService;
            _userService = userService;
            _defaultPagingOptions = defaultPagingOptionsAccessor.Value;
        }

        [HttpGet(Name = nameof(GetRoomsAsync))]
        public async Task<IActionResult> GetRoomsAsync(
            [FromQuery] PagingOptions pagingOptions,
            [FromQuery] SortOptions<RoomResource, RoomEntity> sortOptions,
            [FromQuery] SearchOptions<RoomResource, RoomEntity> searchOptions,
            CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiError(ModelState));

            pagingOptions.Offset = pagingOptions.Offset ?? _defaultPagingOptions.Offset;
            pagingOptions.Limit = pagingOptions.Limit ?? _defaultPagingOptions.Limit;

            var rooms = await _roomService.GetRoomsAsync(
                pagingOptions,
                sortOptions,
                searchOptions,
                ct);

            var collection = PagedCollection<RoomResource>.Create<RoomsResponse>(
                Link.ToCollection(nameof(GetRoomsAsync)),
                rooms.Items.ToArray(),
                rooms.TotalSize,
                pagingOptions);

            collection.Openings = Link.ToCollection(nameof(GetAllRoomOpeningsAsync));
            collection.RoomsQuery = FormMetadata.FromResource<RoomResource>(
                Link.ToForm(
                    nameof(GetRoomsAsync),
                    null,
                    Link.GetMethod,
                    Form.QueryRelation));

            return Ok(collection);
        }

        // GET /rooms/openings
        [HttpGet("openings", Name = nameof(GetAllRoomOpeningsAsync))]
        [ResponseCache(CacheProfileName = "Collection",
                       VaryByQueryKeys = new[] { "offset", "limit", "orderBy", "search" })]
        public async Task<IActionResult> GetAllRoomOpeningsAsync(
            [FromQuery] PagingOptions pagingOptions,
            [FromQuery] SortOptions<OpeningResource, OpeningEntity> sortOptions,
            [FromQuery] SearchOptions<OpeningResource, OpeningEntity> searchOptions,
            CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiError(ModelState));

            pagingOptions.Offset = pagingOptions.Offset ?? _defaultPagingOptions.Offset;
            pagingOptions.Limit = pagingOptions.Limit ?? _defaultPagingOptions.Limit;

            var openings = await _openingService.GetOpeningsAsync(
                pagingOptions,
                sortOptions,
                searchOptions,
                ct);

            var collection = PagedCollection<OpeningResource>.Create<OpeningsResponse>(
                Link.ToCollection(nameof(GetAllRoomOpeningsAsync)),
                openings.Items.ToArray(),
                openings.TotalSize,
                pagingOptions);

            collection.OpeningsQuery = FormMetadata.FromResource<OpeningResource>(
                Link.ToForm(
                    nameof(GetAllRoomOpeningsAsync),
                    null,
                    Link.GetMethod,
                    Form.QueryRelation));

            return Ok(collection);
        }


        // GET /rooms/{roomId}
        [HttpGet("{roomId}", Name = nameof(GetRoomByIdAsync))]
        [ResponseCache(CacheProfileName = "Resource")]
        [Etag]
        public async Task<IActionResult> GetRoomByIdAsync(Guid roomId, CancellationToken ct)
        {
            var room = await _roomService.GetRoomAsync(roomId, ct);
            if (room == null) return NotFound();

            if (!Request.GetEtagHandler().NoneMatch(room))
            {
                return StatusCode(304, room);
            }

            return Ok(room);
        }

        [Authorize]
        [HttpPost("{roomId}/bookings", Name = nameof(CreateBookingForRoomAsync))]
        public async Task<IActionResult> CreateBookingForRoomAsync(
            Guid roomId,
            [FromBody] BookingForm bookingForm,
            CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiError(ModelState));

            var userId = await _userService.GetUserIdAsync(User);
            if (userId == null) return Unauthorized();

            var room = await _roomService.GetRoomAsync(roomId, ct);
            if (room == null) return NotFound();

            var minimumStay = _dateLogicService.GetMinimumStay();
            bool tooShort = (bookingForm.EndAt.Value - bookingForm.StartAt.Value) < minimumStay;
            if (tooShort) return BadRequest(
                new ApiError($"The minimum booking duration is {minimumStay.TotalHours}."));

            var conflictedSlots = await _openingService.GetConflictingSlots(
                roomId, bookingForm.StartAt.Value, bookingForm.EndAt.Value, ct);
            if (conflictedSlots.Any()) return BadRequest(
                new ApiError("This time conflicts with an existing booking."));

            // todo pass user id here actually
            var bookingId = await _bookingService.CreateBookingAsync(
                userId.Value, roomId, bookingForm.StartAt.Value, bookingForm.EndAt.Value, ct);

            return Created(
                Url.Link(nameof(BookingsController.GetBookingByIdAsync),
                new { bookingId }),
                null);
        }

        [HttpGet("{roomId}/openings", Name = nameof(GetRoomOpeningsByRoomId))]
        [ResponseCache(CacheProfileName = "Collection",
                       VaryByQueryKeys = new[] { "roomId", "offset", "limit", "orderBy", "search" })]
        public async Task<IActionResult> GetRoomOpeningsByRoomId(
            Guid roomId,
            [FromQuery] PagingOptions pagingOptions,
            [FromQuery] SortOptions<OpeningResource, OpeningEntity> sortOptions,
            [FromQuery] SearchOptions<OpeningResource, OpeningEntity> searchOptions,
            CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiError(ModelState));

            pagingOptions.Offset = pagingOptions.Offset ?? _defaultPagingOptions.Offset;
            pagingOptions.Limit = pagingOptions.Limit ?? _defaultPagingOptions.Limit;

            var room = await _roomService.GetRoomAsync(roomId, ct);
            if (room == null) return NotFound();

            var openings = await _openingService.GetOpeningsByRoomIdAsync(
                roomId,
                pagingOptions,
                sortOptions,
                searchOptions,
                ct);

            var collectionLink = Link.ToCollection(
                nameof(GetRoomOpeningsByRoomId), new { roomId });

            var collection = PagedCollection<OpeningResource>.Create<OpeningsResponse>(
                collectionLink,
                openings.Items.ToArray(),
                openings.TotalSize,
                pagingOptions);

            collection.OpeningsQuery = FormMetadata.FromResource<OpeningResource>(
                Link.ToForm(nameof(GetRoomOpeningsByRoomId),
                            new { roomId }, Link.GetMethod, Form.QueryRelation));

            return Ok(collection);
        }
    }
}