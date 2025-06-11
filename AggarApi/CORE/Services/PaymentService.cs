using CORE.Constants;
using CORE.DTOs;
using CORE.DTOs.Payment;
using CORE.Services.IServices;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace CORE.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly StripeSettings _stripe;
        private readonly ILogger<PaymentService> _logger;
        private readonly PaymentPolicy _paymentPolicy;
        public PaymentService(IUnitOfWork unitOfWork,
            IOptions<StripeSettings> stripeSettings,
            ILogger<PaymentService> logger,
            IOptions<PaymentPolicy> paymentPolicy)
        {
            _unitOfWork = unitOfWork;
            _stripe = stripeSettings.Value;
            StripeConfiguration.ApiKey = _stripe.SecretKey;
            _logger = logger;
            _paymentPolicy = paymentPolicy.Value;
        }

        public async Task<ResponseDto<StripeAccountDto>> CreateStripeAccountAsync(CreateConnectedAccountDto dto, int renterId)
        {
            try
            {
                Renter? renter = await _unitOfWork.Renters.GetAsync(renterId);
                if (renter == null)
                {
                    return new ResponseDto<StripeAccountDto>
                    {
                        Message = "No Renter with this Id",
                        StatusCode = StatusCodes.BadRequest
                    };
                }

                if (renter.StripeAccount.StripeAccountId != null)
                {
                    return new ResponseDto<StripeAccountDto>
                    {
                        Message = "Renter allready has a stripe account",
                        StatusCode = StatusCodes.Conflict
                    };
                }

                var accountOptions = new AccountCreateOptions
                {
                    Type = "custom",
                    Country = "US",
                    Email = renter.Email,
                    BusinessType = "individual",
                    TosAcceptance = new AccountTosAcceptanceOptions
                    {
                        Date = DateTime.UtcNow,
                        Ip = "127.0.0.1"
                    },
                    BusinessProfile = new AccountBusinessProfileOptions
                    {
                        Url = "https://github.com/",
                        Mcc = "5734"
                    },
                    Capabilities = new AccountCapabilitiesOptions
                    {
                        Transfers = new AccountCapabilitiesTransfersOptions { Requested = true }
                    },
                    Individual = new AccountIndividualOptions
                    {
                        FirstName = renter.Name,
                        LastName = "Renter",
                        Dob = new DobOptions
                        {
                            Day = renter.DateOfBirth.Day,
                            Month = renter.DateOfBirth.Month,
                            Year = renter.DateOfBirth.Year
                        },
                        Phone = dto.Phone,
                        Address = new AddressOptions
                        {
                            City = renter.Address
                        },
                        SsnLast4 = "0002",
                        Verification = new AccountIndividualVerificationOptions
                        {
                            Document = new AccountIndividualVerificationDocumentOptions
                            {
                                Front = "file_identity_document_success",
                            }
                        }
                    },
                    Settings = new AccountSettingsOptions
                    {
                        Payouts = new AccountSettingsPayoutsOptions
                        {
                            Schedule = new AccountSettingsPayoutsScheduleOptions
                            {
                                Interval = "automatic"
                            }
                        }
                    }
                };

                var accountService = new AccountService();
                var stripeAccount = await accountService.CreateAsync(accountOptions);

                if (stripeAccount == null || string.IsNullOrEmpty(stripeAccount.Id))
                {
                    return new ResponseDto<StripeAccountDto>
                    {
                        Message = "Failed to create Stripe account.",
                        StatusCode = StatusCodes.InternalServerError
                    };
                }

                _logger.LogInformation($"Create connected account for renter {renterId}");

                var tokenService = new TokenService();
                var tokenOptions = new TokenCreateOptions
                {
                    BankAccount = new TokenBankAccountOptions
                    {
                        Country = "US",
                        Currency = "usd",
                        AccountNumber = dto.BankAccountNumber,
                        RoutingNumber = dto.BankAccountRoutingNumber,
                        AccountHolderType = "individual"
                    }
                };

                var bankAccountToken = await tokenService.CreateAsync(tokenOptions);
                if (bankAccountToken == null || string.IsNullOrEmpty(bankAccountToken.Id))
                {
                    return new ResponseDto<StripeAccountDto>
                    {
                        Message = "Failed to create bank account token.",
                        StatusCode = StatusCodes.InternalServerError
                    };
                }

                var externalAccountService = new AccountExternalAccountService();
                var externalAccountOptions = new AccountExternalAccountCreateOptions
                {
                    ExternalAccount = bankAccountToken.Id,
                    DefaultForCurrency = true
                };

                var bankAccount = await externalAccountService.CreateAsync(stripeAccount.Id, externalAccountOptions) as BankAccount;

                if (bankAccount == null || string.IsNullOrEmpty(bankAccount.Id))
                {
                    _logger.LogWarning($"Failed to attach bank account to Stripe account for renter {renterId}");
                    return new ResponseDto<StripeAccountDto>
                    {
                        Message = "Failed to attach bank account to Stripe account.",
                        StatusCode = StatusCodes.InternalServerError
                    };
                }

                // store stripe account id for renter
                renter.StripeAccount.StripeAccountId = stripeAccount.Id;
                renter.StripeAccount.BankAccountId = bankAccount.Id;
                renter.StripeAccount.CreatedAt = DateTime.UtcNow;
                await _unitOfWork.CommitAsync();

                return new ResponseDto<StripeAccountDto>
                {
                    Data = new StripeAccountDto
                    {
                        StripeAccountId = stripeAccount.Id,
                        BankAccountId = bankAccount.Id,
                        IsVerified = stripeAccount.Capabilities?.Transfers == "active"
                    },
                    StatusCode = StatusCodes.OK
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError($"Stripe error: {ex.StripeError?.Message ?? ex.Message}");
                return new ResponseDto<StripeAccountDto>
                {
                    Message = "Failed to created connected account",
                    StatusCode = StatusCodes.InternalServerError
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error: {ex.Message}");
                return new ResponseDto<StripeAccountDto>
                {
                    Message = "Failed to created connected account",
                    StatusCode = StatusCodes.InternalServerError
                };
            }
        }


        public async Task<ResponseDto<ConnectedPayoutDetailsDto>> GetRenterPayoutDetailsAsync(int renterID)
        {
            Renter? renter = await _unitOfWork.Renters.GetAsync(renterID);
            if (renter == null)
            {
                return new ResponseDto<ConnectedPayoutDetailsDto>
                {
                    StatusCode = StatusCodes.Unauthorized,
                    Message = "Renter is not found"
                };
            }

            if (renter.StripeAccount.StripeAccountId == null)
            {
                return new ResponseDto<ConnectedPayoutDetailsDto>
                {
                    StatusCode = StatusCodes.Conflict,
                    Message = "You don't have any payment account"
                };
            }

            try
            {
                var requestOptions = new RequestOptions
                {
                    StripeAccount = renter.StripeAccount.StripeAccountId
                };

                var balanceService = new BalanceService();
                var balance = await balanceService.GetAsync(requestOptions);

                var accountService = new AccountService();
                var account = await accountService.GetAsync(renter.StripeAccount.StripeAccountId);

                var connectedDto = new ConnectedPayoutDetailsDto();
                var bankAccount = account.ExternalAccounts?.Data?.FirstOrDefault();

                if (bankAccount is BankAccount bank)
                {
                    connectedDto.Last4 = bank.Last4;
                    connectedDto.RoutingNumber = bank.RoutingNumber;
                }

                var currency = "usd";
                decimal availableAmount = 0;
                decimal pendingAmount = 0;

                if (balance.Available?.Any() == true)
                {
                    var availableBalance = balance.Available.FirstOrDefault();
                    if (availableBalance != null)
                    {
                        availableAmount = availableBalance.Amount / 100m;
                        currency = availableBalance.Currency;
                    }
                }

                if (balance.Pending?.Any() == true)
                {
                    var pendingBalance = balance.Pending.FirstOrDefault(p => p.Currency == currency);
                    if (pendingBalance != null)
                    {
                        pendingAmount = pendingBalance.Amount / 100m;
                    }
                }

                connectedDto.CurrentAmount = availableAmount;
                connectedDto.UpcomingAmount = pendingAmount; 
                connectedDto.Currency = currency;

                return new ResponseDto<ConnectedPayoutDetailsDto>
                {
                    Data = connectedDto,
                    Message = "Connected Payout Details Loaded Successfully",
                    StatusCode = StatusCodes.OK
                };
            }
            catch (StripeException ex)
            {
                return new ResponseDto<ConnectedPayoutDetailsDto>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = $"Error Occured while retrieving the payment detials"
                };
            }
            catch (Exception ex)
            {
                return new ResponseDto<ConnectedPayoutDetailsDto>
                {
                    StatusCode = StatusCodes.InternalServerError,
                    Message = $"Internal Error occured while retrieving connected account"
                };
            }
        }

        // https://docs.stripe.com/payments/payment-intents
        public async Task<PaymentIntent?> CreatePaymentIntent(Booking booking)
        {
            try
            {
                var service = new PaymentIntentService();
                if(booking.PaymentIntentId != null)
                {
                    await service.CancelAsync(booking.PaymentIntentId);
                    booking.PaymentIntentId = null;
                }

                long amountInCents = (long)(booking.FinalPrice * 100);

                long fees = (long)(booking.FinalPrice * (_paymentPolicy.FeesPercentage / 100m) * 100);

                var options = new PaymentIntentCreateOptions
                {
                    Amount = amountInCents,
                    CaptureMethod = "automatic",
                    Description = $"Confirm Booking #{booking.Id} for Vehicle #{booking.VehicleId} by Customer #{booking.CustomerId}",
                    Currency = "USD",
                    Metadata = new Dictionary<string, string>
                    {
                        { "BookingId", booking.Id.ToString() },
                        { "CustomerId", booking.CustomerId.ToString() },
                        { "VehicleId", booking.VehicleId.ToString() },
                        { "Fees", fees.ToString() },
                    }
                };

                var paymentIntent = await service.CreateAsync(options);

                _logger.LogInformation($"Confirm Booking #{booking.Id} for Vehicle #{booking.VehicleId} by Customer #{booking.CustomerId}");

                return paymentIntent;
            }
            catch(StripeException ex)
            {
                _logger.LogError(ex, $"Failed to create payment intent for booking {booking.Id}", booking.Id);
                return null;
            }
        }

        public async Task<ResponseDto<PlatformBalanceDto>>PlatformBalanceAsync()
        {
            try
            {
                BalanceService balanceService = new BalanceService();
                var balance = await balanceService.GetAsync();

                PlatformBalanceDto platformBalanceDto = new();

                platformBalanceDto.AvailableBalanc = balance.Available?.FirstOrDefault()?.Amount / 100m ?? 0;
                platformBalanceDto.PendingBalance = balance.Pending?.FirstOrDefault()?.Amount / 100m ?? 0;
                platformBalanceDto.ConnectReserved = balance.ConnectReserved?.FirstOrDefault()?.Amount / 100m ?? 0;
                platformBalanceDto.Currency = balance.Available?.FirstOrDefault()?.Currency ?? "usd";

                return new ResponseDto<PlatformBalanceDto>
                {
                    Data = platformBalanceDto,
                    Message = "Platform Balance Loaded Successfuly",
                    StatusCode = StatusCodes.OK
                };
            }
            catch (StripeException ex)
            {
                throw new Exception($"Error retrieving platform balance: {ex.Message}", ex);
            }
        }

        public async Task<Transfer?> TransferToRenterAsync(string paymentIntentId, string connectedAccountId, int rentalId, long amount)
        {
            try
            {
                var chargeService = new ChargeService();
                var charges = await chargeService.ListAsync(new ChargeListOptions
                {
                    PaymentIntent = paymentIntentId
                });

                var charge = charges.FirstOrDefault();
                if (charge == null)
                {
                    _logger.LogError($"No charge found for payment intent {paymentIntentId}");
                    throw new InvalidOperationException("Original payment not found");
                }

                var transferService = new TransferService();
                var transferOptions = new TransferCreateOptions
                {
                    Amount = amount,
                    Currency = "USD",
                    Destination = connectedAccountId,
                    SourceTransaction = charge.Id,
                    Description = $"Rental payout for payment {paymentIntentId}",
                    Metadata = new Dictionary<string, string>
                    {
                        { "BookingId", charge.Metadata.TryGetValue("BookingId", out var id) ? id : "unknown" },
                        { "RentalId", rentalId.ToString() },
                        { "Fees", charge.Metadata.TryGetValue("Fees", out var fees) ? fees : "unknown" }
                    }
                };

                var transfer = await transferService.CreateAsync(transferOptions);
                _logger.LogInformation($"Transfer {transfer.Id} created for {amount} to account {connectedAccountId}, Rental: {rentalId}");

                return transfer;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, $"Failed to create transfer for payment {paymentIntentId}");
                return null;
            }
        }

        public async Task<Refund?> RefundAsync(string paymentIntentId, string connectedAccountId, int rentalId, long amount)
        {
            try
            {
                var chargeService = new ChargeService();
                var charges = await chargeService.ListAsync(new ChargeListOptions
                {
                    PaymentIntent = paymentIntentId
                });

                var charge = charges.FirstOrDefault();
                if (charge == null)
                {
                    _logger.LogError($"No charge found for payment intent {paymentIntentId}");
                    throw new InvalidOperationException("Original payment not found");
                }

                var options = new RefundCreateOptions
                {
                    PaymentIntent = charge.PaymentIntentId,
                    Amount = amount,
                    Metadata = new Dictionary<string, string>
                    {
                        { "BookingId", charge.Metadata.TryGetValue("BookingId", out var id) ? id : "unknown" },
                        { "RentalId", rentalId.ToString() }

                    }
                };

                RefundService refundService = new RefundService();
                var refund = await refundService.CreateAsync(options);

                _logger.LogInformation($"Refund {refund.Id}, Amount: {amount}, Rental: {rentalId}");

                return refund;

            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, $"Failed to create refund for payment {paymentIntentId}");
                return null;
            }
        }
    }
}
