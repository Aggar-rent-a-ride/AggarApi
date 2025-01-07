using AutoMapper;
using CORE.DTOs.Auth;
using CORE.Services.IServices;
using DATA.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services
{
    public class AuthService : IAuthService
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
        public async Task<AuthDto> RegisterAsync(RegisterDto registerDto, List<string> roles)
        {
            if (registerDto.AggreedTheTerms == false)
                return new AuthDto { Message = "you must agree on Terms and Conditions to register" };
            if (await _userManager.FindByNameAsync(registerDto.Username) != null)
                return new AuthDto { Message = "username already exists" };
            if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
                return new AuthDto { Message = "email already exists" };

            IdentityResult result = null;
            if(registerDto.IsCustomer == true)
            {
                var customer = _mapper.Map<Customer>(registerDto);
                result = await _userManager.CreateAsync(customer, registerDto.Password);
            }
            else
            {
                var renter = _mapper.Map<Renter>(registerDto);
                result = await _userManager.CreateAsync(renter, registerDto.Password);
            }

            if(result == null)
                return new AuthDto { Message = "couldn't register user" };
            else if (result.Succeeded == false)
            {
                var errors = new StringBuilder();
                foreach (var error in result.Errors)
                    errors.Append($"{error.Description}, ");
                return new AuthDto { Message = errors.ToString() };
            }

            foreach (var role in roles)
            {
                result = await _userManager.AddToRoleAsync(user, role);
                if (result.Succeeded == false)
                    return new AuthDto { Message = $"couldn't assign {role} to user" };
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            return new AuthDto
            {
                IsAuthenticated = true,
                Message = "Registered Successfully",
                Roles = userRoles.ToList(),
                Username = registerDto.Username,
                Email = user.Email
            };
        }
    }
}
