﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UppsalaApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;
using System.Security.Claims;
using AutoMapper;

namespace UppsalaApi.Services
{
    public class DefaultUserService : IUserService
    {
        private readonly UserManager<UserEntity> _userManager;

        public DefaultUserService(UserManager<UserEntity> userManager)
        {
            _userManager = userManager;
        }

        public async Task<(bool Succeeded, string Error)> CreateUserAsync(RegisterForm form)
        {
            var entity = new UserEntity
            {
                Email = form.Email,
                UserName = form.Email,
                FirstName = form.FirstName,
                LastName = form.LastName,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var result = await _userManager.CreateAsync(entity, form.Password);
            if (!result.Succeeded)
            {
                var firstError = result.Errors.FirstOrDefault()?.Description;
                return (false, firstError);
            }

            return (true, null);
        }

        public async Task<UserResource> GetUserAsync(ClaimsPrincipal user)
        {
            var entity = await _userManager.GetUserAsync(user);
            return Mapper.Map<UserResource>(entity);
        }

        public async Task<UserResource> GetUserByIdAsync(Guid userId, CancellationToken ct)
        {
            var user = await _userManager.Users
                .SingleOrDefaultAsync(x => x.Id == userId, ct);

            return Mapper.Map<UserResource>(user);
        }

        public async Task<Guid?> GetUserIdAsync(ClaimsPrincipal principal)
        {
            var user = await _userManager.GetUserAsync(principal);
            if (user == null) return null;

            return user.Id;
        }

        public async Task<PagedResults<UserResource>> GetUsersAsync(
            PagingOptions pagingOptions,
            SortOptions<UserResource, UserEntity> sortOptions,
            SearchOptions<UserResource, UserEntity> searchOptions,
            CancellationToken ct)
        {
            IQueryable<UserEntity> query = _userManager.Users;
            query = searchOptions.Apply(query);
            query = sortOptions.Apply(query);

            var size = await query.CountAsync(ct);

            var items = await query
                .Skip(pagingOptions.Offset.Value)
                .Take(pagingOptions.Limit.Value)
                .ProjectTo<UserResource>()
                .ToArrayAsync(ct);

            return new PagedResults<UserResource>
            {
                Items = items,
                TotalSize = size
            };
        }
    }
}
