using DATA.DataAccess.Context;
using DATA.DataAccess.Repositories.IRepositories;
using DATA.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.DataAccess.Repositories
{
    public class RentalRepository : BaseRepository<Rental>, IRentalRepository
    {
        public RentalRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<(int Id, int CustomerReviewId, int RenterReviewId, int CustomerId, int RenterId, int VehicleId)?> GetRentalByIdIncludingBookingThenIncludingVehicleAsync(int rentalId)
        {
            var result = await _context.Rentals
                .Select(r=> new { 
                    Id = r.Id,
                    CustomerReviewId = r.CustomerReviewId,
                    RenterReviewId = r.RenterReviewId,
                    CustomerId = r.Booking.CustomerId,
                    RenterId = r.Booking.Vehicle.RenterId,
                    VehicleId = r.Booking.VehicleId
                })
                .FirstOrDefaultAsync(r => r.Id == rentalId);

            return result == null
                ? default
                : (result.Id, result.CustomerReviewId, result.RenterReviewId,
                   result.CustomerId, result.RenterId, result.VehicleId);
        }

        public async Task<IEnumerable<Rental>> GetRentalsByUserIdAsync(int userId, int pageNo, int pageSize)
        {
            return await _context.Rentals
                .Where(r => r.Booking.CustomerId == userId || r.Booking.Vehicle.RenterId == userId)
                .Select(r=> new Rental
                {
                    Id = r.Id,
                    CustomerReviewId = r.CustomerReviewId,
                    RenterReviewId = r.RenterReviewId,
                    BookingId = r.BookingId,
                    Booking = new Booking
                    {
                        Id = r.Booking.Id,
                        CustomerId = r.Booking.CustomerId,
                        Vehicle = new Vehicle
                        {
                            Id = r.Booking.Vehicle.Id,
                            RenterId = r.Booking.Vehicle.RenterId
                        }
                    }
                })
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        public async Task<IEnumerable<Rental>> GetRentalsByUserIdAsync(int userId)
        {
            return await _context.Rentals
                .Where(r => r.Booking.CustomerId == userId || r.Booking.Vehicle.RenterId == userId)
                .Select(r => new Rental
                {
                    Id = r.Id,
                    CustomerReviewId = r.CustomerReviewId,
                    RenterReviewId = r.RenterReviewId,
                    BookingId = r.BookingId,
                    Booking = new Booking
                    {
                        Id = r.Booking.Id,
                        CustomerId = r.Booking.CustomerId,
                        Vehicle = new Vehicle
                        {
                            Id = r.Booking.Vehicle.Id,
                            RenterId = r.Booking.Vehicle.RenterId
                        }
                    }
                })
                .ToListAsync();
        }
        public async Task<IEnumerable<Rental>> GetRentalsByVehicleIdAsync(int vehicleId, int pageNo, int pageSize)
        {
            return await _context.Rentals
                .Where(r => r.Booking.VehicleId == vehicleId)
                .Select(r => new Rental
                {
                    Id = r.Id,
                    CustomerReviewId = r.CustomerReviewId,
                    RenterReviewId = r.RenterReviewId,
                    BookingId = r.BookingId
                })
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Rental>> GetUserRentalHistoryAsync(int userId, int pageNo, int pageSize)
        {
            return await _context.Rentals
                .Include(r => r.RenterReview)
                .Include(r => r.CustomerReview)
                .Where(r => r.Booking.CustomerId == userId || r.Booking.Vehicle.RenterId == userId)
                .Select(r => new Rental
                {
                    Id = r.Id,
                    Booking = new Booking
                    {
                        StartDate = r.Booking.StartDate,
                        EndDate = r.Booking.EndDate,
                        Price = r.Booking.Price,
                        Status = r.Booking.Status,
                        Discount = r.Booking.Discount,
                        Customer = new Customer
                        {
                            Id = r.Booking.Customer.Id,
                            Name = r.Booking.Customer.Name,
                            ImagePath = r.Booking.Customer.ImagePath,
                        },
                        Vehicle = new Vehicle
                        {
                            Id = r.Booking.Vehicle.Id,
                            PricePerDay = r.Booking.Vehicle.PricePerDay,
                            MainImagePath = r.Booking.Vehicle.MainImagePath,
                            Address = r.Booking.Vehicle.Address,
                            Renter = new Renter
                            {
                                Id = r.Booking.Vehicle.Renter.Id,
                                Name = r.Booking.Vehicle.Renter.Name,
                                ImagePath = r.Booking.Vehicle.Renter.ImagePath,
                            }
                        },
                    },
                    RenterReview = r.RenterReview == null? null : new RenterReview
                    {
                        Id = r.RenterReview.Id,
                        CreatedAt = r.RenterReview.CreatedAt,
                        RentalId = r.RenterReview.RentalId,
                        Behavior = r.RenterReview.Behavior,
                        Punctuality = r.RenterReview.Punctuality,
                        Care = r.RenterReview.Care,
                        Comments = r.RenterReview.Comments,
                        Renter = new Renter
                        {
                            Id = r.RenterReview.Renter.Id,
                            Name = r.RenterReview.Renter.Name,
                            ImagePath = r.RenterReview.Renter.ImagePath,
                        }
                    },
                    CustomerReview = r.CustomerReview == null? null : new CustomerReview
                    {
                        Id = r.CustomerReview.Id,
                        CreatedAt = r.CustomerReview.CreatedAt,
                        RentalId = r.CustomerReview.RentalId,
                        Behavior = r.CustomerReview.Behavior,
                        Punctuality = r.CustomerReview.Punctuality,
                        Truthfulness = r.CustomerReview.Truthfulness,
                        Comments = r.CustomerReview.Comments,
                        Customer = new Customer
                        {
                            Id = r.CustomerReview.Customer.Id,
                            Name = r.CustomerReview.Customer.Name,
                            ImagePath = r.CustomerReview.Customer.ImagePath,
                        }
                    },
                })
                .OrderByDescending(r=>r.Booking.StartDate)
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
