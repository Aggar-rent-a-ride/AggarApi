using DATA.Models;
using DATA.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.Constants.Includes
{
    public static class AppUserIncludes
    {
        public const string Notifications = "Notifications";
        public const string Messages = "Messages";
        public const string ReceivedMessages = "ReceivedMessages";
        public const string Reports = "Reports";
        public const string TargetedReports = "TargetedReports";
        public const string TargetedAdminActions = "TargetedAdminActions";
        public const string RefreshTokens = "RefreshTokens";
    }

    public static class CustomerIncludes
    {
        public const string Notifications = "Notifications";
        public const string Messages = "Messages";
        public const string ReceivedMessages = "ReceivedMessages";
        public const string Reports = "Reports";
        public const string TargetedReports = "TargetedReports";
        public const string TargetedAdminActions = "TargetedAdminActions";
        public const string RefreshTokens = "RefreshTokens";
        public const string RecommendedBrands = "RecommendedBrands";
        public const string RecommendedTypes = "RecommendedTypes";
        public const string FavoriteVehicles = "FavoriteVehicles";
        public const string Bookings = "Bookings";
        public const string Reviews = "Reviews";
    }

    public static class AdminIncludes
    {
        public const string Notifications = "Notifications";
        public const string Messages = "Messages";
        public const string ReceivedMessages = "ReceivedMessages";
        public const string Reports = "Reports";
        public const string TargetedReports = "TargetedReports";
        public const string TargetedAdminActions = "TargetedAdminActions";
        public const string RefreshTokens = "RefreshTokens";
        public const string Actions = "Actions";
    }

    public static class RenterIncludes
    {
        public const string Notifications = "Notifications";
        public const string Messages = "Messages";
        public const string ReceivedMessages = "ReceivedMessages";
        public const string Reports = "Reports";
        public const string TargetedReports = "TargetedReports";
        public const string TargetedAdminActions = "TargetedAdminActions";
        public const string RefreshTokens = "RefreshTokens";
        public const string Vehicles = "Vehicles";
        public const string Reviews = "Reviews";
    }

    public static class BookingIncludes
    {
        public const string Customer = "Customer";
        public const string Vehicle = "Vehicle";
        public const string Notification = "Notification";
        public const string Rental = "Rental";
    }

    public static class ReviewIncludes
    {
        public const string Notification = "Notification";
        public const string Rental = "Rental";
        public const string Reports = "Reports";
    }

    public static class CustomerReviewIncludes
    {
        public const string Notification = "Notification";
        public const string Rental = "Rental";
        public const string Reports = "Reports";
        public const string Customer = "Customer";
    }

    public static class RenterReviewIncludes
    {
        public const string Notification = "Notification";
        public const string Rental = "Rental";
        public const string Reports = "Reports";
        public const string Renter = "Renter";
    }

    public static class VehicleIncludes
    {
        public const string Renter = "Renter";
        public const string Bookings = "Bookings";
        public const string VehicleImages = "VehicleImages";
        public const string VehicleBrand = "VehicleBrand";
        public const string VehicleType = "VehicleType";
        public const string FavoriteCustomers = "FavoriteCustomers";
        public const string Reports = "Reports";
    }

    public static class VehicleBrandIncludes
    {
        public const string Vehicles = "Vehicles";
    }

    public static class VehicleTypeIncludes
    {
        public const string Vehicles = "Vehicles";
    }

    public static class VehicleImageIncludes
    {
        public const string Vehicle = "Vehicle";
    }

    public static class ReportIncludes
    {
        public const string Reporter = "Reporter";
        public const string TargetMessage = "TargetMessage";
        public const string TargetRenterReview = "TargetRenterReview";
        public const string TargetCustomerReview = "TargetCustomerReview";
        public const string TargetUser = "TargetUser";
        public const string TargetVehicle = "TargetVehicle";
    }

    public static class MessageIncludes
    {
        public const string Sender = "Sender";
        public const string Receiver = "Receiver";
        public const string Notification = "Notification";
        public const string Report = "Report";
    }

    public static class RentalIncludes
    {
        public const string Booking = "Booking";
        public const string CustomerReview = "CustomerReview";
        public const string RenterReview = "RenterReview";
    }

    public static class NotificationIncludes
    {
        public const string Reciver = "Reciver";
        public const string TargetBooking = "TargetBooking";
        public const string TargetMessage = "TargetMessage";
        public const string TargetAdminAction = "TargetAdminAction";
        public const string TargetRenterReview = "TargetRenterReview";
        public const string TargetCustomerReview = "TargetCustomerReview";
    }

    public static class RecommendedBrandIncludes
    {
        public const string Customer = "Customer";
    }

    public static class RecommendedTypeIncludes
    {
        public const string Customer = "Customer";
    }

}

