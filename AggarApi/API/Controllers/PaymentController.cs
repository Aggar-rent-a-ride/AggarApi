using CORE.DTOs.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {

        [HttpPost("connected-account")]
        public async Task<IActionResult> CreateConnectedAccount([FromBody] CreateConnectedAccount request)
        {
            try
            {
                var accountOptions = new AccountCreateOptions
                {
                    Type = "custom",
                    Country = request.Country,
                    Email = request.Email,
                    BusinessType = "individual",
                    TosAcceptance = new AccountTosAcceptanceOptions
                    {
                        Date = DateTime.UtcNow,
                        Ip = "127.0.0.1" // Test acceptance
                    },
                    BusinessProfile = new AccountBusinessProfileOptions
                    {
                        Url = "https://github.com/", // Fake website
                        Mcc = "5734" // Test MCC (Software)
                    },
                    Capabilities = new AccountCapabilitiesOptions
                    {
                        Transfers = new AccountCapabilitiesTransfersOptions { Requested = true }
                    },
                    Individual = new AccountIndividualOptions
                    {
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        Dob = new DobOptions
                        {
                            Day = request.DateOfBirth.Day,
                            Month = request.DateOfBirth.Month,
                            Year = request.DateOfBirth.Year
                        },
                        Phone = request.Phone,
                        Address = new AddressOptions
                        {
                            Line1 = request.AddressLine1,
                            City = request.City,
                            PostalCode = request.PostalCode,
                            Country = request.Country
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
                        Country = request.Country,
                        Currency = "usd",
                        AccountNumber = request.AccountNumber,
                        RoutingNumber = request.RoutingNumber,
                        AccountHolderName = $"{request.FirstName} {request.LastName}",
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

                return Ok(new
                {
                    StripeAccountId = stripeAccount.Id,
                    BankAccountId = bankAccount.Id,
                    IsVerified = stripeAccount.Capabilities?.Transfers == "active"
                });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { error = ex.StripeError.Message });
            }
        }
    }
}
