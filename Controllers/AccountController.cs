using System;
using System.Linq;
using CESystem.AdminPart;
using CESystem.DB;
using CESystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CESystem.Controllers
{
    [Authorize(Roles = "client, admin")]
    [Route("/account/user")]
    public class UserController : Controller
    {
        private readonly LocalDbContext _db;
        private readonly IAdminContext _adminContext;
        
        public UserController(LocalDbContext dbContext, IAdminContext adminContext)
        {
           _db = dbContext;
           _adminContext = adminContext;
        }

        [HttpGet("home")]
        public IActionResult Home()
        {
            return Ok($"Welcome Client {HttpContext.User.Identity.Name}");
        }

        [HttpPost("transfer")]
        public IActionResult TransferMoney(string toUserName, string sum, string curr)
        {
            if (curr == null || sum == null)
                return BadRequest("Incorrect request params");
            
            double targetSum;
            
            try
            {
                targetSum = Convert.ToDouble(sum.Replace(".", ","));
            }
            catch (Exception e)
            {
                return BadRequest("Sum must be number");
            }
            
            Currency currency = _db.Currencies.FirstOrDefault(c => c.Name.Equals(curr));;

            if (currency == null)
                return BadRequest("Currency not found");
            
            User toUser = _db.Users.FirstOrDefault(u => u.Name == toUserName);
            User fromUser = _db.Users.FirstOrDefault(u => u.Name == HttpContext.User.Identity.Name);

            if (toUser == null)
                return NotFound("Host user not found");
            
            Account fromAccount = _db.Accounts.FirstOrDefault(a => a.UserId == fromUser.Id);
            Account toAccount = _db.Accounts.FirstOrDefault(a => a.UserId == toUser.Id);


            Purse toPurse = _db.Purses.FirstOrDefault(p => p.IdAccount == toAccount.Id && p.IdCurrency == currency.Id);
            Purse fromPurse = _db.Purses.FirstOrDefault(p => p.IdAccount == fromAccount.Id && p.IdCurrency == currency.Id);

            if (fromPurse == null)
                return BadRequest("You don't have money in this currency");
            
            var transferCommission = 0.0;

            var personalCommission = fromPurse.TransferCommission;
            var personalCommissionStatus = fromPurse.IsAbsoluteCommissionValue;
            var publicCommission = currency.TransferCommission;
            var upperCommissionLimit = currency.UpperCommissionLimit;
            var lowerCommissionLimit = currency.LowerCommissionLimit;
            var commissionStatus = currency.IsAbsoluteCommissionValue;
            var commissionConfirmValue = currency.ConfirmCommissionLimit;

            if (personalCommission == null)
            {
                if (publicCommission != null)
                { 
                    if (commissionStatus == null)
                        transferCommission = (double) (targetSum * publicCommission / 100.0);
                    else
                        transferCommission = (double) publicCommission;
                }
            }
            else
            { 
                if (personalCommissionStatus == null)
                    transferCommission = (double) (targetSum * personalCommission / 100.0);
                else
                    transferCommission = (double) personalCommission;
            }

            if (transferCommission > upperCommissionLimit)
                transferCommission = (double) upperCommissionLimit;

            if (transferCommission < lowerCommissionLimit)
                transferCommission = (double) lowerCommissionLimit;

            if (fromPurse.CashValue - transferCommission - targetSum < 0.0)
                return BadRequest("You don't have enough money to make a transfer");

            if (toPurse == null)
            { 
                toPurse = new Purse
                {
                    IdAccount = toAccount.Id,
                    Account = toAccount,
                    Currency = currency,
                    IdCurrency = currency.Id,
                    CashValue = targetSum
                };

                _db.Purses.Add(toPurse);
                _db.SaveChanges();
            }

            if (commissionConfirmValue != null && targetSum >= commissionConfirmValue)
            {
                _adminContext.AddRequestToConfirmTransfer(fromPurse, toPurse, targetSum);
                return Ok("your transaction is pending confirmation, please wait.");
            }
            
            toPurse.CashValue += targetSum;
            fromPurse.CashValue -= transferCommission + targetSum;
            
            _db.SaveChanges();

            return Ok("Transfer completed");
        }

        [HttpPost("deposit")]
        public IActionResult DepositMoney(string sum, string curr)
        {
            if (curr == null || sum == null)
                return BadRequest("Incorrect request params");
            
            double targetSum;
            
            try
            {
                targetSum = Convert.ToDouble(sum.Replace(".", ","));
            }
            catch (Exception e)
            {
                return BadRequest("Sum must be number");
            }
            
            Currency currency = _db.Currencies.FirstOrDefault(c => c.Name.Equals(curr));;

            if (currency == null)
                return BadRequest("Currency not found");
            
            User me = _db.Users.FirstOrDefault(u => u.Name == HttpContext.User.Identity.Name);
            Account myAccount = _db.Accounts.FirstOrDefault(a => a.UserId == me.Id);
            Purse myPurse = _db.Purses.FirstOrDefault(p => p.IdAccount == myAccount.Id && p.IdCurrency == currency.Id);

            if (myPurse == null)
            {
                myPurse = new Purse
                {
                    IdAccount = myAccount.Id,
                    Account = myAccount,
                    Currency = currency,
                    IdCurrency = currency.Id,
                };

                _db.Purses.Add(myPurse);
                _db.SaveChanges();
            }
            
            var depositCommission = 0.0;
            var personalCommission = myPurse.DepositCommission;
            var personalCommissionStatus = myPurse.IsAbsoluteCommissionValue;
            var publicCommission = currency.DepositCommission;
            var commissionStatus = currency.IsAbsoluteCommissionValue;

            if (personalCommission == null)
            {
                if (publicCommission != null)
                { 
                    if (commissionStatus == null)
                        depositCommission = (double) (targetSum * publicCommission / 100.0);
                    else
                        depositCommission = (double) publicCommission;
                }
            }
            else
            { 
                if (personalCommissionStatus == null)
                    depositCommission = (double) (targetSum * personalCommission / 100.0);
                else
                    depositCommission = (double) personalCommission;
            }
            
            if (currency.ConfirmCommissionLimit != null && targetSum > currency.ConfirmCommissionLimit)
            {
                _adminContext.AddRequestToConfirmOther(myPurse, targetSum, "deposit");
                return Ok("your transaction is pending confirmation, please wait.");
            }
            
            myPurse.CashValue += targetSum - depositCommission;
            _db.SaveChanges();
            
            return Ok("Deposit completed");
        }
        
        [HttpPost("withdraw")]
        public IActionResult WithdrawMoney(string sum, string curr)
        {
            if (curr == null || sum == null)
                return BadRequest("Incorrect request params");
            
            double targetSum;
            
            try
            {
                targetSum = Convert.ToDouble(sum.Replace(".", ","));
            }
            catch (Exception e)
            {
                return BadRequest("Sum must be number");
            }
            
            Currency currency = _db.Currencies.FirstOrDefault(c => c.Name.Equals(curr));;

            if (currency == null)
                return BadRequest("Currency not found");
            
            User me = _db.Users.FirstOrDefault(u => u.Name == HttpContext.User.Identity.Name);
            Account myAccount = _db.Accounts.FirstOrDefault(a => a.UserId == me.Id);
            Purse myPurse = _db.Purses.FirstOrDefault(p => p.IdAccount == myAccount.Id && p.IdCurrency == currency.Id);

            if (myPurse == null)
                return BadRequest("You don't have money in this currency");

            var withdrawCommission = 0.0;
            var personalCommission = myPurse.WithdrawCommission;
            var personalCommissionStatus = myPurse.IsAbsoluteCommissionValue;
            var publicCommission = currency.WithdrawCommission;
            var commissionStatus = currency.IsAbsoluteCommissionValue;

            if (personalCommission == null)
            {
                if (publicCommission != null)
                { 
                    if (commissionStatus == null)
                        withdrawCommission = (double) (targetSum * publicCommission / 100.0);
                    else
                        withdrawCommission = (double) publicCommission;
                }
            }
            else
            { 
                if (personalCommissionStatus == null)
                    withdrawCommission = (double) (targetSum * personalCommission / 100.0);
                else
                    withdrawCommission = (double) personalCommission;
            }
            
            if (myPurse.CashValue - targetSum - withdrawCommission < 0.0) 
                return BadRequest("You don't have enough money to make withdraw");
            
            if (currency.ConfirmCommissionLimit != null && targetSum > currency.ConfirmCommissionLimit)
            {
                _adminContext.AddRequestToConfirmOther(myPurse, targetSum, "withdraw");
                return Ok("your transaction is pending confirmation, please wait.");
            }
            
            myPurse.CashValue -= targetSum + withdrawCommission;
            _db.SaveChanges();
            
            return Ok("Withdraw completed");
        }
    }
}