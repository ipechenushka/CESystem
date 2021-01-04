using System.Linq;
using System.Threading.Tasks;
using CESystem.ClientPart;
using CESystem.DB;
using CESystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CESystem.Controllers
{
    public enum CurrencyManipulationOperation
    {
        Add = 1,
        Delete = 2,
    }
    public enum MoneyManipulationOperation
    {
        Charge = 1,
        Withdraw = 2
    }
    public enum CommissionLevel
    {
        Global = 1,
        Personal = 2
    }

    public enum RequestStatus
    {
        Active = 1,
        Completed = 2
    }

    [Authorize(Roles = "admin")]
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly LocalDbContext _db;
        public AdminController(IUserService userService, LocalDbContext localDbContext)
        {
            _userService = userService;
            _db = localDbContext;
        }

        [HttpGet("home")]
        public IActionResult Home()
        {
            return Ok("admin home");
        }

        [HttpPost("money")]
        public async Task<IActionResult> MoneyOperation(MoneyManipulationOperation? operation, string currencyName, int? accountIdParam, float? amountParam)
        {
            if (operation == null || currencyName == null || accountIdParam == null || amountParam == null)
                return BadRequest("Incorrect request params");

            var accountId = (int) accountIdParam;
            var amount = (float) amountParam;
            var currency = await _userService.FindCurrencyAsync(currencyName);

            if (currency == null)
                return BadRequest("Currency doesn't exist");

            var userWallet = await _userService.FindUserWalletAsync(accountId, currency.Id);

            switch (operation)
            {
                case MoneyManipulationOperation.Charge:
                    if (userWallet == null)
                    {
                        userWallet = _userService.CreateNewWallet(accountId, currency.Id);
                        await _db.WalletRecords.AddAsync(userWallet);
                    }
                    
                    userWallet.CashValue += amount;
                    break;
                case MoneyManipulationOperation.Withdraw:
                    if (userWallet == null)
                        return NotFound("User wallet not found");
                    
                    if (userWallet.CashValue - amount < 0.0)
                        return BadRequest("The amount on the account must not be negative");

                    userWallet.CashValue -= amount;
                    break;
                default:
                    return BadRequest("Undefined operation");
            }

            await _db.SaveChangesAsync();
            return Ok("Money operation completed");
        }

        [HttpPost("commission")]
        public async Task<IActionResult> CommissionOperation(float? transferCommission, float? depositCommission, float? withdrawCommission, bool? isAbsoluteType, string currencyName, int? userId)
        {
            CommissionRecord commission;
            
            if (currencyName != null)
            {
                var currency = await _userService.FindCurrencyAsync(currencyName);
                if (currency == null)
                    return BadRequest("Currency doesn't exist");
                
                commission = await _db.CommissionRecords.FirstOrDefaultAsync(c => c.CurrencyId == currency.Id);
                
                if (commission == null)
                {
                    commission = new CommissionRecord {CurrencyId = currency.Id};
                    await _db.CommissionRecords.AddAsync(commission);
                }
            }
            else if (userId != null)
            {
                var user = await _userService.FindUserByIdAsync((int) userId);
                if (user == null)
                    return BadRequest("Currency doesn't exist");
                
                commission = await _db.CommissionRecords.FirstOrDefaultAsync(c => c.UserId == userId);
                
                if (commission == null)
                {
                    commission = new CommissionRecord {UserId = userId};
                    await _db.CommissionRecords.AddAsync(commission);
                }
            }
            else
                return BadRequest("Incorrect request params");
            

            if (transferCommission != null)
                commission.TransferCommission = transferCommission;

            if (depositCommission != null)
                commission.DepositCommission = depositCommission;

            if (withdrawCommission != null)
                commission.WithdrawCommission = withdrawCommission;

            if (isAbsoluteType != null)
                commission.IsAbsoluteType = true;
            
            await _db.SaveChangesAsync();
            return Ok("Commission operations completed");
        }

        
        [HttpPost("currency-limit")]
        public async Task<IActionResult> CurrencyLimitOperation(string currencyName, float? low, float? up, float? confirm)
        {
            if (currencyName == null)
                return BadRequest("Incorrect request params");

            var currency = await _userService.FindCurrencyAsync(currencyName);

            if (currency == null)
                return BadRequest("Currency doesn't exist");

            if (low != null)
                currency.LowerCommissionLimit = low;

            if (up != null)
                currency.UpperCommissionLimit = up;

            if (confirm != null)
                currency.ConfirmCommissionLimit = confirm;

            await _db.SaveChangesAsync();
            return Ok("Currency limit operations completed");
        }

        [HttpPost("currency")]
        public async Task<IActionResult> CurrencyOperation(CurrencyManipulationOperation? cmo, string currencyName)
        {
            if (currencyName == null || cmo == null)
                return BadRequest("Incorrect request params");

            var currency = await _userService.FindCurrencyAsync(currencyName);
            
            switch (cmo)
            {
                case CurrencyManipulationOperation.Add:
                    if (currency != null)
                        return BadRequest("This currency is already exist");
                    
                    await _db.CurrencyRecords.AddAsync(new CurrencyRecord {Name = currencyName});
                    break;
                case CurrencyManipulationOperation.Delete:
                    if (currency == null)
                        return BadRequest("This currency doesn't exist");

                    _db.CurrencyRecords.Remove(currency);
                    break;
                default:
                    return BadRequest("Undefined operation");
            }

            await _db.SaveChangesAsync();
            return Ok("Currency operation completed");
        }
        
        [HttpGet("confirm")]
        public async Task<IActionResult> Confirm(int? requestId)
        {
            if (requestId == null)
                return BadRequest("Incorrect request params");
            
            var request = await _db.ConfirmRequestRecords.FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
                return BadRequest("This request doesn't exist.");

            var senderUser = await _userService.FindUserByAccountIdAsync(request.SenderId);
            var currency = await _userService.FindCurrencyAsync(request.Currency);
            var senderWallet = await _userService.FindUserWalletAsync(request.SenderId, currency.Id);

            switch (request.OperationType)
            {
                case OperationType.Transfer:
                    if (request.RecipientId != null)
                    {
                        var recipientWallet = await _userService.FindUserWalletAsync((int) request.RecipientId, currency.Id);
                        recipientWallet.CashValue += request.Amount;
                    }

                    senderWallet.CashValue -= request.Amount + request.Commission;
                    break;
                case OperationType.Deposit:
                    senderWallet.CashValue += request.Amount - request.Commission;
                    break;
                case OperationType.Withdraw:
                    senderWallet.CashValue -= request.Amount + request.Commission;
                    break;
                default:
                    return BadRequest("Undefined operation");
            }
            
            await _userService.AddOperationHistory(request.OperationType, senderUser.Id, request.SenderId,
                request.Amount, request.Commission, currency.Name);
            
            request.Status = RequestStatus.Completed;
            
            await _db.SaveChangesAsync();
            return Ok("Request has been completed");
        }
    }
}