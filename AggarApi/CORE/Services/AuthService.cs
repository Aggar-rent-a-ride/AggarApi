using AutoMapper;
using CORE.DTOs.Auth;
using CORE.Services.IServices;
using DATA.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services
{
    public class AuthService: IAuthService
    {
        private readonly IOptions<JwtConfig> _jwt;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public AuthService(IOptions<JwtConfig> jwt,
            UserManager<AppUser> userManager,
            IConfiguration configuration, IMapper mapper)
        {
            _jwt = jwt;
            _userManager = userManager;
            _configuration = configuration;
            _mapper = mapper;
        }
        private async Task<string?> ValidateRegistrationAsync(RegisterDto registerDto)
        {
            if (!registerDto.AggreedTheTerms)
                return "You must agree to the Terms and Conditions to register";
            if (await _userManager.FindByNameAsync(registerDto.Username) != null)
                return "Username already exists";
            if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
                return "Email already exists";
            return null;
        }

        public async Task<AuthDto> RegisterAsync(RegisterDto registerDto, List<string> roles)
        {
            if (await ValidateRegistrationAsync(registerDto) is string validationMessage)
            {
                return new AuthDto { Message = validationMessage };
            }

            AppUser user = registerDto.IsCustomer ? _mapper.Map<Customer>(registerDto) : _mapper.Map<Renter>(registerDto);
            
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (result.Succeeded == false)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new AuthDto { Message = errors };
            }

            if (roles != null && roles.Any())
            {
                var roleResult = await _userManager.AddToRolesAsync(user, roles);
                if (!roleResult.Succeeded)
                {
                    var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    return new AuthDto { Message = $"User created, but roles couldn't be assigned: {roleErrors}" };
                }
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            return new AuthDto
            {
                IsAuthenticated = true,
                Message = "Registered Successfully",
                Roles = userRoles.ToList(),
                Username = registerDto.Username,
                Email = registerDto.Email,
            };
        }
    }
}
