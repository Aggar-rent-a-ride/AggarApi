using CORE.Constants;
using CORE.DTOs;
using CORE.DTOs.Payment;
using CORE.Services.IServices;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
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

        public async Task<ResponseDto<StripeAccountCreation>> CreateStripeAccountAsync(int renterId)
        {
            Renter? renter = await _unitOfWork.Renters.GetAsync(renterId);
            if(renter == null)
            {
                return new ResponseDto<StripeAccountCreation>
                {
                    Message = "No Renter with this Id",
                    StatusCode = StatusCodes.InternalServerError
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
                        Dob = new DobOptions
                        {
                            Day = renter.DateOfBirth.Day,
                            Month = renter.DateOfBirth.Month,
                            Year = renter.DateOfBirth.Year
                        },
                        Phone = "(555) 123-4567",
                        Address = new AddressOptions
                        {
                            City = renter.Name
                        },
                        SsnLast4 = "0002", // Test SSN (US only)
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
                Account stripeAccount = await accountService.CreateAsync(accountOptions);

                var tokenOptions = new TokenCreateOptions
                {
                    BankAccount = new TokenBankAccountOptions
                    {
                        Country = "US",
                        Currency = "usd",
                        AccountNumber = "000123456789",
                        RoutingNumber = "110000000",
                        AccountHolderType = "individual"
                    }
                };

                var tokenService = new TokenService();
                Token bankAccountToken = await tokenService.CreateAsync(tokenOptions);

                var externalAccountOptions = new AccountExternalAccountCreateOptions
                {
                    ExternalAccount = bankAccountToken.Id,
                    DefaultForCurrency = true
                };

                var externalAccountService = new AccountExternalAccountService();
                BankAccount bankAccount = (BankAccount)await externalAccountService.CreateAsync(
                    stripeAccount.Id,
                    externalAccountOptions
                );

                return new ResponseDto<StripeAccountCreation>
                {
                    Data = {
                    StripeAccountId = stripeAccount.Id,
                    BankAccountId = bankAccount.Id,
                    IsVerified = stripeAccount.Capabilities?.Transfers == "active"
                    },
                    StatusCode = StatusCodes.OK,
                };
            

        }
    }
}
