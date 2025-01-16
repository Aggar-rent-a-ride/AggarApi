using CORE.DTOs;
using CORE.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services.IServices
{
    public interface IAuthService
    {
        Task<ResponseDto<AuthDto>> RegisterAsync(RegisterDto registerDto, List<string> roles);
        Task<ResponseDto<AuthDto>> LoginAsync(LoginDto loginDto);
        Task<ResponseDto<object>> SendActivationCodeAsync(int userId);
        Task<ResponseDto<AuthDto>> ActivateAccountAsync(AccountActivationDto dto);
        Task<ResponseDto<AuthDto>> RefreshAccessTokenAsync(string refreshToken);
        Task<ResponseDto<object>> RevokeRefreshTokenAsync(string refreshToken);
        Task<ResponseDto<object>> UpdateUserRolesAsync(int userId, List<string> roles);
    }
}
