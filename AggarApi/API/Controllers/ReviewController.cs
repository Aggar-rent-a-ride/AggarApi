using CORE.Constants;
using CORE.DTOs;
using CORE.DTOs.Review;
using CORE.Helpers;
using CORE.Services;
using CORE.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly IUserReviewService _userReviewService;
        private readonly IVehicleReviewService _vehicleReviewService;
        public ReviewController(IReviewService reviewService, IUserReviewService userReviewService, IVehicleReviewService vehicleReviewService)
        {
            _reviewService = reviewService;
            _userReviewService = userReviewService;
            _vehicleReviewService = vehicleReviewService;
        }
        [HttpPost, Authorize(Roles = $"{Roles.Customer}, {Roles.Renter}")]
        public async Task<IActionResult> CreateReviewAsync(CreateReviewDto reviewDto)
        {
            var userId = UserHelpers.GetUserId(User);
            var roles = UserHelpers.GetUserRoles(User);
            if (roles == null || roles.Count == 0)
                return StatusCode(CORE.Constants.StatusCodes.Unauthorized, new ResponseDto<CreateReviewDto>
                {
                    StatusCode = CORE.Constants.StatusCodes.Unauthorized,
                    Message = "Unauthorized"
                });

            string role = "";
            if(roles.Contains(Roles.Customer))
                role = Roles.Customer;
            else if (roles.Contains(Roles.Renter))
                role = Roles.Renter;
            else
                return StatusCode(CORE.Constants.StatusCodes.Unauthorized, new ResponseDto<CreateReviewDto>
                {
                    StatusCode = CORE.Constants.StatusCodes.Unauthorized,
                    Message = "Unauthorized"
                });

            var result = await _reviewService.CreateReviewAsync(reviewDto, userId, role);

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("user-reviews"), Authorize]
        public async Task<IActionResult> GetUserReviewsAsync(int userId, int pageNo = 1, int pageSize = 10)
        {
            var result = await _userReviewService.GetUserReviewsAsync(userId, pageNo, pageSize);

            return StatusCode(result.StatusCode, result);
        }
        [HttpGet, Authorize]
        public async Task<IActionResult> GetReviewAsync(int reviewId)
        {
            var result = await _reviewService.GetReviewAsync(reviewId);

            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("vehicle-reviews"), Authorize]
        public async Task<IActionResult> GetVehicleReviewsAsync(int vehicleId, int pageNo = 1, int pageSize = 10)
        {
            var result = await _vehicleReviewService.GetVehicleReviewsAsync(vehicleId, pageNo, pageSize);

            return StatusCode(result.StatusCode, result);
        }
    }
}
