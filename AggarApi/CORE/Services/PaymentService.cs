using CORE.Constants;
using CORE.DTOs;
using CORE.DTOs.Payment;
using CORE.Services.IServices;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services
{
    public class PaymentService : IPaymentService
    {
        public IUnitOfWork _unitOfWork;
        public PaymentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
                        LastName = dto.LastName,
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
                                Interval = "manual"
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
                return new ResponseDto<StripeAccountDto>
                {
                    Message = $"Stripe error: {ex.StripeError?.Message ?? ex.Message}",
                    StatusCode = StatusCodes.InternalServerError
                };
            }
            catch (Exception ex)
            {
                return new ResponseDto<StripeAccountDto>
                {
                    Message = $"Unexpected error: {ex.Message}",
                    StatusCode = StatusCodes.InternalServerError
                };
            }
        }

    }
}
