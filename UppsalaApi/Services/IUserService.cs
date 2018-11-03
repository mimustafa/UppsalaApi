using UppsalaApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;

namespace UppsalaApi.Services
{
    public interface IUserService
    {
        Task<PagedResults<UserResource>> GetUsersAsync(
            PagingOptions pagingOptions,
            SortOptions<UserResource, UserEntity> sortOptions,
            SearchOptions<UserResource, UserEntity> searchOptions,
            CancellationToken ct);

        Task<(bool Succeeded, string Error)> CreateUserAsync(RegisterForm form);

        Task<Guid?> GetUserIdAsync(ClaimsPrincipal principal);

        Task<UserResource> GetUserByIdAsync(Guid userId, CancellationToken ct);

        Task<UserResource> GetUserAsync(ClaimsPrincipal user);
    }
}
