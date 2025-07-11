﻿using CORE.DTOs;
using CORE.DTOs.Booking;
using DATA.Models;
using DATA.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services.IServices
{
    public interface IBookingService
    {
        public Task<ResponseDto<BookingDetailsDto>> CreateBookingAsync(CreateBookingDto createBookingDto, int customerId);
        public Task<bool> CheckVehicleAvailability(int vehicleId, DateTime startDate, DateTime endDate);
        public Task<ResponseDto<BookingDetailsDto>> GetBookingDetailsByIdAsync(int bookingId, int userId);
        public Task<ResponseDto<ConfirmBookingDto>> ConfirmBookingAsync(int customerId, int bookingId);
        public Task<ResponseDto<object>> CancelBookingAsync(int bookingId, int customerId);
        public Task<ResponseDto<object>> ResponseBookingRequestAsync(int bookingId, int renterId, bool isAccepted);
        //public Task<ResponseDto<PagedResultDto<BookingSummaryDto>>> GetBookingAsync(int userId, int pageIndex, int pageSize); // maybe in profile
        Task<ResponseDto<BookingDetailsDto>> GetBookingByRentalIdAsync(int rentalId);
        Task<ResponseDto<PagedResultDto<IEnumerable<BookingDetailsDto>>>> GetBookingsByStatusAsync(BookingStatus? status, int pageNo, int pageSize, int maxPageSize = 50);
        Task<ResponseDto<int>> GetBookingsByStatusCountAsync(BookingStatus? status);
        public Task HandleBookingPaymentSucceededAsync(int bookingId, string paymentIntentId);
        public Task HandleBookingPaymentFailedAsync(int bookingId, string paymentIntentId);
        Task<ResponseDto<PagedResultDto<IEnumerable<BookingSummaryDto>>>> GetRenterPendingBookingsAsync(int renterId, int pageNo, int pageSize, int maxPageSize = 100);
        Task<ResponseDto<PagedResultDto<IEnumerable<BookingSummaryDto>>>> GetUserBookingsAsync(int userId, BookingStatus? status, int pageNo, int pageSize, int maxPageSize = 100);
        Task<ResponseDto<IEnumerable<Interval>>> GetBookingsInterval(int renterId);
    }
}
