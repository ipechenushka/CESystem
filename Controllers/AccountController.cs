using System;
using System.Threading.Tasks;
using CESystem.ClientPart;
using CESystem.DB;
using CESystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CESystem.Controllers
{
    public enum OperationType
    {
        Transfer = 0,
        Deposit = 1,
        Withdraw = 2
    }

    [Authorize(Roles = "client, admin")]
    [Route("account")]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly LocalDbContext _db;
        public AccountController(IUserService userService, LocalDbContext localDbContext)
        {
            _userService = userService;
            _db = localDbContext;
        }

        [HttpGet, Route("{accountId:int}")]
        public async Task<IActionResult> ChooseAccount(int accountId)
        {
            var user = await _userService.FindUserByNameAsync(HttpContext.User.Identity.Name);
            var accessDenied = await IsAccountBelongToUser(user, accountId);

            if (accessDenied)
                return NotFound();
            
            user.CurrentAccount = accountId;
            
            await _db.SaveChangesAsync();
            return Ok($"Current account - {accountId}");
        }

        [HttpGet, Route("new-account")]
        public async Task<IActionResult> CreateNewAccount()
        {
            var user = await _userService.FindUserByNameAsync(HttpContext.User.Identity.Name);

            await _userService.AddAccountAsync(new AccountRecord {UserId = user.Id});
            await _db.SaveChangesAsync();
            
            return Ok("Created successfully");
        }
        
        [HttpGet, Route("")]
        public IActionResult Home() => Ok($"Welcome to server - {HttpContext.User.Identity.Name}");
        
        
        [HttpPost, Route("{accountId:int}/operation")]
        public async Task<IActionResult> Operation(int accountId, OperationType? operationType, float? amount, string inCurrency, string toUserName)
        {
            var user = await _userService.FindUserByNameAsync(HttpContext.User.Identity.Name);
            var accessDenied = await IsAccountBelongToUser(user, accountId);

            if (accessDenied) 
                return NotFound();
            
            if (inCurrency == null || operationType == null || amount == null || operationType == OperationType.Transfer && toUserName == null)
                return BadRequest("Incorrect request params");
            
            var targetAmount = (float) amount;
            var currency = await _userService.FindCurrencyAsync(inCurrency);

            if (currency == null)
                return BadRequest("Currency not found");
            
            var userAccount = await _userService.FindUserAccountAsync(user.Id, user.CurrentAccount);
            var userWallet = await _userService.FindUserWalletAsync(userAccount.Id, currency.Id);

            if (userWallet == null)
            {
                if (operationType == OperationType.Withdraw || operationType == OperationType.Transfer)
                    return BadRequest("You don't have money in this currency on this account");

                userWallet = _userService.CreateNewWallet(accountId, currency.Id);
                await _userService.AddWalletAsync(userWallet);
            }

            var commission = await CalculateCommission(user, operationType, currency, targetAmount);
            var confirmLimit = currency.ConfirmLimit;

            if (confirmLimit != null && targetAmount > confirmLimit)
            {
                AccountRecord toAccount = null;
                
                if (operationType == OperationType.Transfer)
                {
                    var toUser = await _userService.FindUserByNameAsync(toUserName);
                    if (toUser == null)
                        return BadRequest("Recipient user not found");
                    toAccount = await _userService.FindUserAccountAsync(toUser.Id, toUser.CurrentAccount);
                }
   
                await _userService.AddRequestToConfirmAsync((OperationType) operationType, userAccount, toAccount, targetAmount, commission, currency.Name);
                return Ok("Your transaction is pending confirmation, please wait.");
            }

            if (userWallet.CashValue - commission - targetAmount < 0.0 || userWallet.CashValue - commission + targetAmount < 0.0)
                return BadRequest("You don't have enough money to make the operation!");
            
            switch (operationType)
            {
                case OperationType.Transfer:
                {
                    var toUser = await _userService.FindUserByNameAsync(toUserName);
                    
                    if (toUser == null)
                        return BadRequest("Recipient user not found");
                    
                    var toAccount = await _userService.FindUserAccountAsync(toUser.Id, toUser.CurrentAccount);
                    var toWallet = await _userService.FindUserWalletAsync(toAccount.Id, currency.Id);

                    if (toWallet == null)
                    {
                        toWallet = _userService.CreateNewWallet(toAccount.Id, currency.Id);
                        await _userService.AddWalletAsync(toWallet);
                    }

                    toWallet.CashValue += targetAmount;
                    userWallet.CashValue -= targetAmount;
                    break;
                }
                case OperationType.Deposit:
                {
                    userWallet.CashValue += targetAmount;
                    break;
                }
                case OperationType.Withdraw:
                {
                    userWallet.CashValue -= targetAmount;
                    break;
                }
                default:
                    return BadRequest("Undefined operation");
            }

            userWallet.CashValue -= commission;

            await _userService.AddOperationHistoryAsync((OperationType) operationType, user.Id, userAccount.Id, targetAmount, commission, currency.Name);
            await _db.SaveChangesAsync();
            
            return Ok("Operation completed");
        }
        private async Task<float> CalculateCommission(UserRecord user, OperationType? type, CurrencyRecord currency, float amount)
        {
            var commission = 0.0f;
            var personalCommissionRecord = await _userService.FindUserCommissionAsync(user.Id);
            var globalCommissionRecord = await _userService.FindCurrencyCommissionAsync(currency.Id);

            if (personalCommissionRecord == null && globalCommissionRecord == null)
                return commission;
            
            var upperCommissionLimit = currency.UpperCommissionLimit;
            var lowerCommissionLimit = currency.LowerCommissionLimit;
            float? personalCommission = null;
            float? globalCommission = null;
            
            switch (type)
            {
                case OperationType.Transfer:
                    if (personalCommissionRecord != null)
                        personalCommission = personalCommissionRecord.TransferCommission;
                    if(globalCommissionRecord != null)
                        globalCommission = globalCommissionRecord.TransferCommission;
                    break;
                case OperationType.Deposit:
                    if (personalCommissionRecord != null)
                        personalCommission = personalCommissionRecord.DepositCommission;
                    if(globalCommissionRecord != null)
                        globalCommission = globalCommissionRecord.DepositCommission;
                    break;
                case OperationType.Withdraw:
                    if (personalCommissionRecord != null)
                        personalCommission = personalCommissionRecord.WithdrawCommission;
                    if(globalCommissionRecord != null)
                        globalCommission = globalCommissionRecord.WithdrawCommission;
                    break;
            }
            
            if (personalCommission == null)
            {
                if (globalCommission != null)
                { 
                    if (globalCommissionRecord.IsAbsoluteType == null)
                        commission = (float) (amount * globalCommission / 100.0);
                    else
                        commission = (float) globalCommission;
                }
            }
            else
            { 
                if (personalCommissionRecord.IsAbsoluteType == null)
                    commission = (float) (amount * personalCommission / 100.0);
                else
                    commission = (float) personalCommission;
            }

            if (commission > upperCommissionLimit)
                commission = (float) upperCommissionLimit;

            if (commission < lowerCommissionLimit)
                commission = (float) lowerCommissionLimit;

            return commission;
        }
        
        private async Task<bool> IsAccountBelongToUser(UserRecord user, int accountId)
        {
            var userAccount = await _userService.FindUserAccountAsync(user.Id, accountId);
            return userAccount == null;
        }
    }
}