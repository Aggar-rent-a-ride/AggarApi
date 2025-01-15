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
        Task<AuthDto> RegisterAsync(RegisterDto registerDto, List<string> roles);
        Task<AuthDto> LoginAsync(LoginDto loginDto);
        Task<ResponseDto<object>> SendActivationCodeAsync(int userId);
        Task<AuthDto> ActivateAccount(AccountActivationDto dto);
    }
}
