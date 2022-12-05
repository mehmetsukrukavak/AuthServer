using System;
using AuthServer.Core.Configuration;
using AuthServer.Core.Dtos;
using AuthServer.Core.Models;
using AuthServer.Core.Repository;
using AuthServer.Core.Services;
using AuthServer.Core.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedLibrary.Dtos;

namespace AuthServer.Services.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly List<Client> _clients;
        private readonly ITokenService _tokenService;
        private readonly UserManager<UserApp> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<UserRefreshToken> _userRefreshTokenService;

        public AuthenticationService(IOptions<List<Client>> optionsClient, ITokenService tokenService, UserManager<UserApp> userManager, IUnitOfWork unitOfWork, IGenericRepository<UserRefreshToken> userRefreshTokenService)
        {
            _clients = optionsClient.Value;
            _tokenService = tokenService;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _userRefreshTokenService = userRefreshTokenService;

        }

        public async Task<Response<TokenDto>> CreateTokenAsync(LoginDto loginDto)
        {
            if (loginDto == null) throw new ArgumentNullException(nameof(loginDto));

            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null) return Response<TokenDto>.Fail("Email or Password is wrong", 400, true);

            if (!await _userManager.CheckPasswordAsync(user, loginDto.Password))
                return Response<TokenDto>.Fail("Email or Password is wrong", 400, true);

            var token = _tokenService.CreateToken(user);

            var userRefreshToken = await _userRefreshTokenService.Where(r => r.UserId == user.Id).SingleOrDefaultAsync();

            if (userRefreshToken == null)
                await _userRefreshTokenService.AddAsync(new UserRefreshToken
                {
                     Code = token.RefreshToken,
                      Expiration = token.RefreshTokenExpiration,
                       UserId = user.Id
                });
            else
            {
                userRefreshToken.Code = token.RefreshToken;
                userRefreshToken.Expiration = token.RefreshTokenExpiration;
            }

            await _unitOfWork.CommitAsync();

            return Response<TokenDto>.Success(token, 200);

        }

        public Response<ClientTokenDto> CreateTokenByClient(ClientLoginDto clientLoginDto)
        {
            var client = _clients.SingleOrDefault(x => x.Id == clientLoginDto.ClientId && x.Secret == clientLoginDto.ClientSecret);

            if (client == null)
                return Response<ClientTokenDto>.Fail("ClientId or ClientSecret not found", 404,true);

            var token = _tokenService.CreateTokenByClient(client);

            return Response<ClientTokenDto>.Success(token, 200);
        }

        public async Task<Response<TokenDto>> CreateTokenByRefreshTokenAsync(string refreshToken)
        {
            var existRefreshToken = await _userRefreshTokenService.Where(r => r.Code == refreshToken).SingleOrDefaultAsync();

            if(existRefreshToken == null) return Response<TokenDto>.Fail("RefreshToken not found", 404, true);

            var user = await _userManager.FindByIdAsync(existRefreshToken.UserId);

            if(user == null) return Response<TokenDto>.Fail("UserId not found", 404, true);

            var tokenDTO = _tokenService.CreateToken(user);

            existRefreshToken.Code = tokenDTO.RefreshToken;
            existRefreshToken.Expiration = tokenDTO.RefreshTokenExpiration;

            await _unitOfWork.CommitAsync();


            return Response<TokenDto>.Success(tokenDTO, 200);


        }

        public async Task<Response<NoDataDto>> RevokeRefreshTokenAsync(string refreshToken)
        {
            var existRefreshToken = await _userRefreshTokenService.Where(r => r.Code == refreshToken).SingleOrDefaultAsync();

            if (existRefreshToken == null) return Response<NoDataDto>.Fail("RefreshToken not found", 404, true);

            _userRefreshTokenService.Remove(existRefreshToken);

            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(200);
        }
    }
}

