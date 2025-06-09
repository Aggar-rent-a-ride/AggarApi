using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.Constants
{
    public static class NotificationType
    {
        public const string MessageReceived = "Message Received";
        public const string BookingRequested = "Booking Requested";
        public const string BookingAccepted = "Booking Accepted";
        public const string BookingDeclined = "Booking Declined";
        public const string BookingRetracted = "Booking Retracted";
        public const string MoneyReceived = "Money Received";
        public const string WarningReceived = "Warning Received";
        public const string AccountRemoved = "Account Removed";
        public const string AssignedToAdmin = "Assigned To Admin";
        public const string RentalReviewed = "Rental Reviewed";
        public const string BookingReminder = "Booking Reminder";
        public const string BookingCanceled = "Booking Canceled";
        public const string RentalCanceled = "Rental Canceled";
    }
}
