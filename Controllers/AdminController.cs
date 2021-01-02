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

        [HttpPost("users-money")]
        public async Task<IActionResult> MoneyOperation(MoneyManipulationOperation operation, string currencyName, int accountId, float amount)
        {
            if (operation == null || currencyName == null || accountId == null || amount == null)
                return BadRequest("Incorrect request params");
            
            // try
            // {
            //     targetSum = Convert.ToDouble(sum.Replace(".", ","));
            //     accountId = Convert.ToInt32(accId);
            //     operationId = Convert.ToInt32(operId);
            // }
            // catch (Exception e)
            // {
            //     return BadRequest("Sum, operationId and accountId must be number");
            // }

            var currency = await _db.CurrencyRecords.FirstOrDefaultAsync(c => c.Name.Equals(currencyName));

            if (currency == null)
                return BadRequest("Currency doesn't exist");

            var userWallet = currency.WalletRecords.FirstOrDefault(w => w.IdAccount == accountId && w.IdCurrency == currency.Id);

            switch (operation)
            {
                case MoneyManipulationOperation.Charge:
                    if (userWallet == null)
                        await _db.WalletRecords
                            .AddAsync(new WalletRecord
                        {
                            IdAccount = accountId,
                            IdCurrency = currency.Id,
                            CashValue = amount
                        });
                    else
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
            return Ok("money operation completed");
        }

        [HttpPost("commission")]
        public async Task<IActionResult> CommissionOperation(float? transferCommission, float? depositCommission, float? withdrawCommission, bool? isAbsoluteType, string currencyName, int? userId)
        {
            UserRecord user = null;
            CurrencyRecord currency = null;
            CommissionLevel commissionLevel;
            
            // try
            // {
            //     accountId = Convert.ToInt32(accId);
            // }
            // catch (Exception e)
            // {
            //     return BadRequest("Commissions and accountId must be number");
            // }
            
            if (currencyName != null)
            {
                commissionLevel = CommissionLevel.Global;
                currency = await _userService.FindCurrencyAsync(currencyName);
                
                if (currency == null)
                    return BadRequest("Currency doesn't exist!");    
                
            }
            else if (userId != null)
            {
                commissionLevel = CommissionLevel.Personal;
                user = await _userService.FindUserByIdAsync((int) userId, null);

                if (user == null)
                    return BadRequest("User doesn't exist!");
            }
            else
                return BadRequest("Incorrect request params");

            
            if (transferCommission != null)
            {
                if (commissionLevel == CommissionLevel.Global)
                    currency.CommissionRecord.TransferCommission = transferCommission;
                else
                    user.CommissionRecord.TransferCommission = transferCommission;
            }
            
            if (depositCommission != null)
            {
                if (commissionLevel == CommissionLevel.Global)
                    currency.CommissionRecord.DepositCommission = depositCommission;
                else
                    user.CommissionRecord.DepositCommission = depositCommission;
            }
            
            if (withdrawCommission != null)
            {
                if (commissionLevel == CommissionLevel.Global)
                    currency.CommissionRecord.WithdrawCommission = transferCommission;
                else
                    user.CommissionRecord.WithdrawCommission = transferCommission;
            }

            if (isAbsoluteType != null)
            {
                if (commissionLevel == CommissionLevel.Global)
                    currency.CommissionRecord.IsAbsoluteType = true;
                else
                    user.CommissionRecord.IsAbsoluteType = true;
            }
            
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

            var operationType = request.OperationType;
            var currency = await _db.CurrencyRecords.FirstOrDefaultAsync(c => c.Name.Equals(request.Currency));
            var senderWallet = await _db.WalletRecords.FirstOrDefaultAsync(w => w.IdCurrency == currency.Id && w.IdAccount == request.SenderId);

            switch (operationType)
            {
                case OperationType.Transfer:
                    var recipientWallet = await _db.WalletRecords.
                        FirstOrDefaultAsync(w => w.IdCurrency == currency.Id && w.IdAccount == request.RecipientId);
                    recipientWallet.CashValue += request.Amount;
                    senderWallet.CashValue -= request.Amount + request.Commission;
                    break;
                case OperationType.Deposit:
                    senderWallet.CashValue += request.Amount - request.Commission;
                    break;
                case OperationType.Withdraw:
                    senderWallet.CashValue -= request.Amount + request.Commission;
                    break;
                default:
                    return BadRequest("Operation not found");
            }

            request.Status = "completed";
            await _db.SaveChangesAsync();
            return Ok("Request has been completed");
        }
    }
}