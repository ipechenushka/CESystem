using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CESystem.Controllers;
using CESystem.DB;
using CESystem.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CESystem.ClientPart
{
    public interface IUserService
    {
        public Task<UserRecord> FindUserByNameAsync(string name, string passwd);

        public Task<UserRecord> FindUserByIdAsync(int id, string passwd);
        public AccountRecord FindUserAccount(UserRecord user, int accountId);
        public WalletRecord FindUserWallet(AccountRecord userAccount, int currencyId);
        public Task<CurrencyRecord> FindCurrencyAsync(string name);

        public void AddRequestToConfirm(OperationType operationType,
            AccountRecord fromAccount,
            AccountRecord toAccount,
            float amount,
            float commission,
            string currency);
        public string HashPassword(string password);
    }
    
    public class UserService : IUserService
    {
        private readonly LocalDbContext _db;
        
        public UserService(LocalDbContext localDbContext)
        {
            _db = localDbContext;
        }
        
        public Task<UserRecord> FindUserByNameAsync(string name, string passwd)
        {
            return passwd == null
                ? _db.UserRecords.FirstOrDefaultAsync(u => u.Name.Equals(name))
                : _db.UserRecords.FirstOrDefaultAsync(x => x.Name.Equals(name) && x.Password.Equals(passwd));
        }

        public Task<UserRecord> FindUserByIdAsync(int id, string passwd)
        {
            return passwd == null 
                ? _db.UserRecords.FirstOrDefaultAsync(u => u.Id == id) 
                : _db.UserRecords.FirstOrDefaultAsync(x => x.Id == id && x.Password.Equals(passwd));        
        }
        
        public AccountRecord FindUserAccount(UserRecord user, int accountId)
        {
           return user.AccountRecords.FirstOrDefault(a => a.Id == accountId);
        }
        
        public WalletRecord FindUserWallet(AccountRecord userAccount, int currencyId)
        {
            return userAccount.WalletRecords.FirstOrDefault(w => w.IdCurrency == currencyId);
        }

        public Task<CurrencyRecord> FindCurrencyAsync(string name)
        {
            return _db.CurrencyRecords.FirstOrDefaultAsync(c => c.Name.Equals(name));
        }

        public void AddRequestToConfirm(OperationType operationType, AccountRecord fromAccount, AccountRecord toAccount, float amount, float commission, string currency)
        {
            var req = new ConfirmRequestRecord
            {
                OperationType = operationType,
                SenderId = fromAccount.Id,
                SenderAccountRecord = fromAccount,
                RecipientId = toAccount.Id,
                Amount = amount,
                Commission = commission,
                Currency = currency,
                FormationDate = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss"),
                Status = "active"
            };

            _db.ConfirmRequestRecords.AddAsync(req);
            _db.SaveChangesAsync();
        }

        public string HashPassword(string password)
        {
            return Convert
                .ToBase64String(new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(password)));
        }
    }
}