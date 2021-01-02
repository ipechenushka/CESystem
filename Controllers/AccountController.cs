using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using CESystem.ClientPart;
using CESystem.DB;
using CESystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CESystem.Controllers
{
    public enum OperationType
    {
        Transfer = 1,
        Deposit = 2,
        Withdraw = 3
    }

    [Authorize(Roles = "client, admin")]
    [Route("user/{id?}")]
    public class AccountController : Controller
    {
        private readonly LocalDbContext _db;
        private readonly IUserService _userService;
        
        public AccountController(LocalDbContext dbContext, IUserService userService)
        {
           _db = dbContext;
           _userService = userService;
        }

        // [HttpGet("home")]
        // public IActionResult Home()
        // {
        //     return Ok($"Welcome to server - {HttpContext.User.Identity.Name}");
        // }

        [HttpGet, Route("")]
        public async Task<IActionResult> CreateNewAccount()
        {
            var user = await _userService.FindUserByNameAsync(HttpContext.User.Identity.Name, null);

            await _db.AccountRecords.AddAsync(new AccountRecord {UserId = user.Id});
            await _db.SaveChangesAsync();
            
            return Ok("Successful");
        }
        
        [HttpGet, Route("")]
        public async Task<IActionResult> ChangeAccount(int id)
        {
            var user = await _userService.FindUserByNameAsync(HttpContext.User.Identity.Name, null);
            user.CurrentAccount = id;
            
            await _db.SaveChangesAsync();
            
            return Redirect($"user/{id}");
        }
        
        [HttpPost, Route("transfer")]
        public async Task<IActionResult> TransferMoney(string toUserName, float amount, string inCurrency)
        {
            if (toUserName == null || inCurrency == null || amount == null)
                return BadRequest("Incorrect request params");
            
            // try
            // {
            //     targetSum = float.TryParse(sum);
            // }
            // catch (Exception e)
            // {
            //     return BadRequest("Sum must be number");
            // }

            var currency = await _userService.FindCurrencyAsync(inCurrency);

            if (currency == null)
                return BadRequest("Currency not found");

            var toUser = await _userService.FindUserByNameAsync(toUserName, null);
            var fromUser = await _userService.FindUserByNameAsync(HttpContext.User.Identity.Name, null);

            if (toUser == null)
                return BadRequest("Host user not found");

            var fromAccount = _userService.FindUserAccount(fromUser, fromUser.CurrentAccount);
            var toAccount = _userService.FindUserAccount(toUser, toUser.CurrentAccount);
            
            var fromWallet = _userService.FindUserWallet(fromAccount, currency.Id);
            var toWallet = _userService.FindUserWallet(toAccount, currency.Id);

            if (fromWallet == null)
                return BadRequest("You don't have money in this currency on this account");

            if (toWallet == null)
            { 
                toWallet = new WalletRecord
                {
                    IdAccount = toAccount.Id,
                    IdCurrency = currency.Id,
                    CurrencyRecord = currency,
                    AccountRecord = toAccount
                };
            }
            
            var commission = CalculateCommission(fromUser, OperationType.Transfer, currency, amount);
            var commissionConfirmLimit = currency.ConfirmCommissionLimit;
            
            if (fromWallet.CashValue - commission - amount < 0.0)
                return BadRequest("You don't have enough money to make a transfer");

            if (commissionConfirmLimit != null && amount >= commissionConfirmLimit)
            {
                _userService.AddRequestToConfirm(OperationType.Transfer, fromAccount, toAccount, amount, commission, currency.Name);
                return Ok("Your transaction is pending confirmation, please wait.");
            }
            
            toWallet.CashValue += amount;
            fromWallet.CashValue -= commission + amount;
            
            await _db.WalletRecords.AddAsync(toWallet);
            await _db.SaveChangesAsync();

            return Ok("Transfer completed");
        }

        [HttpPost, Route("deposit-withdraw")]
        public async Task<IActionResult> DepositMoney(OperationType operationType, float amount, string inCurrency)
        {
            if (inCurrency == null || amount == null)
                return BadRequest("Incorrect request params");

            // try
            // {
            //     targetSum = Convert.ToDouble(sum.Replace(".", ","));
            // }
            // catch (Exception e)
            // {
            //     return BadRequest("Sum must be number");
            // }

            var currency = await _userService.FindCurrencyAsync(inCurrency);

            if (currency == null)
                return BadRequest("Currency not found");
            
            var user = await _userService.FindUserByNameAsync(HttpContext.User.Identity.Name, null);
            var userAccount = _userService.FindUserAccount(user, user.CurrentAccount);
            var userWallet = _userService.FindUserWallet(userAccount, currency.Id);

            if (userWallet == null)
            {
                if (operationType == OperationType.Withdraw)
                    return BadRequest("You don't have money in this currency on this account");

                userWallet = new WalletRecord
                {
                    IdAccount = userAccount.Id,
                    AccountRecord = userAccount,
                    CurrencyRecord = currency,
                    IdCurrency = currency.Id,
                    CashValue = amount
                };
            }

            var commission = CalculateCommission(user, OperationType.Deposit, currency, amount);
            var commissionConfirmLimit = currency.ConfirmCommissionLimit;

            if (commissionConfirmLimit != null && amount > commissionConfirmLimit)
            {
                _userService.AddRequestToConfirm(operationType, userAccount, null, amount, commission, currency.Name);
                return Ok("your transaction is pending confirmation, please wait.");
            }

            switch (operationType)
            {
                case OperationType.Deposit:
                    userWallet.CashValue += amount;
                    break;
                case OperationType.Withdraw:
                    userWallet.CashValue -= amount;
                    break;
            }

            userWallet.CashValue -= commission;

            await _db.WalletRecords.AddAsync(userWallet);
            await _db.SaveChangesAsync();
            
            return Ok("Operation completed");
        }
        private float CalculateCommission(UserRecord user, OperationType type, CurrencyRecord currency, float amount)
        {
            var commission = 0.0;
            var personalCommissionType = user.CommissionRecord.IsAbsoluteType;
            var upperCommissionLimit = currency.UpperCommissionLimit;
            var lowerCommissionLimit = currency.LowerCommissionLimit;
            var globalCommissionType = currency.CommissionRecord.IsAbsoluteType;
            float? personalCommission = null;
            float? globalCommission = null;
            
            switch (type)
            {
                case OperationType.Transfer:
                    personalCommission = user.CommissionRecord.TransferCommission;
                    globalCommission = currency.CommissionRecord.TransferCommission;
                    break;
                case OperationType.Deposit:
                    personalCommission = user.CommissionRecord.DepositCommission;
                    globalCommission = currency.CommissionRecord.DepositCommission;
                    break;
                case OperationType.Withdraw:
                    personalCommission = user.CommissionRecord.WithdrawCommission;
                    globalCommission = currency.CommissionRecord.WithdrawCommission;
                    break;
            }
            
            
            if (personalCommission == null)
            {
                if (globalCommission != null)
                { 
                    if (globalCommissionType == null)
                        commission = (float) (amount * globalCommission / 100.0);
                    else
                        commission = (float) globalCommission;
                }
            }
            else
            { 
                if (personalCommissionType == null)
                    commission = (float) (amount * personalCommission / 100.0);
                else
                    commission = (float) personalCommission;
            }

            if (commission > upperCommissionLimit)
                commission = (float) upperCommissionLimit;

            if (commission < lowerCommissionLimit)
                commission = (float) lowerCommissionLimit;

            return (float) commission;
        }
    }
}