using System;
using System.Collections.Generic;
using System.Linq;
using CESystem.AdminPart;
using CESystem.DB;
using CESystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CESystem.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("/account/admin")]
    public class AdminController : Controller
    {
        private readonly IAdminContext _adminContext;
        private readonly LocalDbContext _db;

        public AdminController(IAdminContext adminAction, LocalDbContext localDbContext)
        {
            _adminContext = adminAction;
            _db = localDbContext;
        }

        [HttpGet("home")]
        public IActionResult Home()
        {
            return Ok($"Active requests to confirm: {_adminContext.RequestCount()}");
        }

        [HttpPost("money_operation")]
        public IActionResult MoneyOperation(string operId, string curr, string sum, string accId)
        {
            if (operId == null || curr == null || sum == null || accId == null)
                return BadRequest("Incorrect request params");
            
            double targetSum;
            int accountId;
            int operationId;
            
            try
            {
                targetSum = Convert.ToDouble(sum.Replace(".", ","));
                accountId = Convert.ToInt32(accId);
                operationId = Convert.ToInt32(operId);
            }
            catch (Exception e)
            {
                return BadRequest("Sum, operationId and accountId must be number");
            }

            Currency currency = _db.Currencies.FirstOrDefault(c => c.Name.Equals(curr));

            if (currency == null)
                return BadRequest("Currency doesn't exist");

            Purse userPurse = _db.Purses.FirstOrDefault(p => p.IdAccount == accountId && p.IdCurrency == currency.Id);
            
            
            //operationId = 1: add money.
            //operationId = 2: delete money.
            switch (operationId)
            {
                case 1:
                    if (userPurse == null)
                        _db.Purses.Add(new Purse
                            {IdAccount = accountId, IdCurrency = currency.Id, CashValue = targetSum});
                    else
                        userPurse.CashValue += targetSum;
                    break;
                case 2:
                    if (userPurse == null)
                        return NotFound("User purse not found");
                    
                    if (userPurse.CashValue - targetSum < 0.0)
                        return BadRequest("The amount on the account must not be negative");

                    userPurse.CashValue -= targetSum;
                    break;
                default:
                    return BadRequest("Undefined operation");
            }

            _db.SaveChanges();
            return Ok("money operation completed");
        }

        [HttpPost("personal_commission_operation")]
        public IActionResult PersonalCommissionOperation(string transferCommission, string depositCommission, string withdrawCommission , string curr, string accId, string isAbsolute)
        {
            if (curr == null || accId == null)
                return BadRequest("Incorrect request params");
            
            int accountId;
            
            try
            {
                accountId = Convert.ToInt32(accId);
            }
            catch (Exception e)
            {
                return BadRequest("Commissions and accountId must be number");
            }

            Currency currency = _db.Currencies.FirstOrDefault(c => c.Name.Equals(curr));

            if (currency == null)
                return BadRequest("Currency doesn't exist");

            Purse userPurse = _db.Purses.FirstOrDefault(p => p.IdAccount == accountId && p.IdCurrency == currency.Id);

            if (userPurse == null)
                return BadRequest("This user don't have a purse with that currency");

            try
            {
                if (transferCommission != null)
                    userPurse.TransferCommission = Convert.ToDouble(transferCommission.Replace(".", ","));
                
                if (transferCommission != null)
                    userPurse.DepositCommission = Convert.ToDouble(depositCommission.Replace(".", ","));
                
                if (transferCommission != null)
                    userPurse.WithdrawCommission = Convert.ToDouble(withdrawCommission.Replace(".", ","));
            }
            catch (Exception e)
            {
                return BadRequest("Commissions and accountId must be number");
            }

            if (isAbsolute != null)
                userPurse.IsAbsoluteCommissionValue = true;

            _db.SaveChanges();
            return Ok("Personal commission operations completed");
        }

        [HttpPost("currency_commission_operation")]
        public IActionResult CurrencyCommissionOperation(string transferCommission, string depositCommission, string withdrawCommission , string curr, string isAbsolute)
        {
            if (curr == null)
                return BadRequest("Incorrect request params");

            Currency currency = _db.Currencies.FirstOrDefault(c => c.Name.Equals(curr));

            if (currency == null)
                return BadRequest("Currency doesn't exist");
            
            try
            {
                if (transferCommission != null)
                    currency.TransferCommission = Convert.ToDouble(transferCommission.Replace(".", ","));
                
                if (transferCommission != null)
                    currency.DepositCommission = Convert.ToDouble(depositCommission.Replace(".", ","));
                
                if (transferCommission != null)
                    currency.WithdrawCommission = Convert.ToDouble(withdrawCommission.Replace(".", ","));
            }
            catch (Exception e)
            {
                return BadRequest("Commissions must be number");
            }

            if (isAbsolute != null)
                currency.IsAbsoluteCommissionValue = true;

            _db.SaveChanges();
            return Ok("Currency commission operations completed");
        }
        
        [HttpPost("currency_limit_operation")]
        public IActionResult CurrencyLimitOperation(string curr, string low, string up, string confirm)
        {
            if (curr == null)
                return BadRequest("Incorrect request params");

            Currency currency = _db.Currencies.FirstOrDefault(c => c.Name.Equals(curr));

            if (currency == null)
                return BadRequest("Currency doesn't exist");
            
            try
            {
                if (low != null)
                    currency.LowerCommissionLimit = Convert.ToDouble(low.Replace(".", ","));
                
                if (up != null)
                    currency.UpperCommissionLimit = Convert.ToDouble(up.Replace(".", ","));
                
                if (confirm != null)
                    currency.ConfirmCommissionLimit = Convert.ToDouble(confirm.Replace(".", ","));
            }
            catch (Exception e)
            {
                return BadRequest("Limits must be numbers");
            }

            _db.SaveChanges();
            return Ok("Currency limit operations completed");
        }

        [HttpPost("currency_operation")]
        public IActionResult CurrencyOperation(string id, string curr)
        {
            if (curr == null || id == null)
                return BadRequest("Incorrect request params");
            
            int operationId;
            
            try
            {
                operationId = Convert.ToInt32(id);
            }
            catch (Exception e)
            {
                return BadRequest("OperationId must be number");
            }
            
            Currency currency = _db.Currencies.FirstOrDefault(c => c.Name.Equals(curr));
            
            //operationId = 1: add currency.
            //operationId = 2: delete money.
            switch (operationId)
            {
                case 1:
                    if (currency != null)
                        return BadRequest("This currency is already exist");
                    
                    _db.Currencies.Add(new Currency {Name = curr});
                    break;
                case 2:
                    if (currency == null)
                        return BadRequest("This currency doesn't exist");

                    _db.Currencies.Remove(currency);
                    break;
                default:
                    return BadRequest("Undefined operation");
            }

            _db.SaveChanges();
            return Ok("Currency operation completed");
        }

        [HttpGet("confirm")]
        public IActionResult Confirm()
        {
            _adminContext.ConfirmAllRequests();
            return Ok("All requests has been completed");
        }

    }
}