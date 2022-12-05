using System;
using AuthServer.Core.Dtos;
using AuthServer.Core.Models;
using AuthServer.Core.Services;
using Microsoft.AspNetCore.Identity;
using SharedLibrary.Dtos;

namespace AuthServer.Services.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<UserAppDto> _userManager;

        public UserService(UserManager<UserAppDto> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Response<UserAppDto>> CreateUserAsync(CreateUserDto createUserDto)
        {
            var user = new UserAppDto { Email = createUserDto.Email, UserName = createUserDto.UserName };


            IdentityResult result = await _userManager.CreateAsync(user, createUserDto.Password.ToString());

            if(!result.Succeeded)
            {
                var errors = result.Errors.Select(x => x.Description).ToList();

                return Response<UserAppDto>.Fail(new ErrorDto(errors, true), 400);
            }

            return Response<UserAppDto>.Success(ObjectMapper.Mapper.Map<UserAppDto>(user), 200);

        }

        public async Task<Response<UserAppDto>> GetUserByNameAsync(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null) Response<UserAppDto>.Fail("UserName not found", 404,true);

            return Response<UserAppDto>.Success(ObjectMapper.Mapper.Map<UserAppDto>(user), 200);

        }
    }
}

