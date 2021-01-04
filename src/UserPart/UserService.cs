using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using CESystem.Controllers;
using CESystem.DB;
using CESystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CESystem.ClientPart
{
    public interface IUserService
    {
        public Task AddOperationHistory(OperationType operationType, int userId, int accountId, float amount,
            float commission, string currencyName);
        public Task<UserRecord> FindUserByNameAsync(string name);
        public Task<UserRecord> FindUserByIdAsync(int id);
        public Task<UserRecord> FindUserByAccountIdAsync(int accountId);
        public Task<AccountRecord> FindUserAccountAsync(int userId, int accountId);
        public Task<WalletRecord> FindUserWalletAsync(int accountId, int currencyId);
        public Task<CurrencyRecord> FindCurrencyAsync(string name);
        public WalletRecord CreateNewWallet(int accountId, int currencyId);
        public void AddRequestToConfirm(OperationType operationType,
            AccountRecord fromAccount,
            AccountRecord toAccount,
            float amount,
            float commission,
            string currency);
    }
    
    public class UserService : IUserService
    {
        private readonly LocalDbContext _db;
        
        public UserService(LocalDbContext localDbContext)
        {
            _db = localDbContext;
        }
        
        public Task<UserRecord> FindUserByNameAsync(string name)
        {
            return _db.UserRecords.FirstOrDefaultAsync(u => u.Name.Equals(name));
        }

        public Task<UserRecord> FindUserByIdAsync(int id)
        {
            return _db.UserRecords.FirstOrDefaultAsync(u => u.Id == id);
        }
        public async Task<UserRecord> FindUserByAccountIdAsync(int accountId)
        {
            var account = await _db.AccountRecords.FirstOrDefaultAsync(a => a.Id == accountId);
            return  await _db.UserRecords.FirstOrDefaultAsync(u => u.Id == account.UserId);
        }
        
        public Task<AccountRecord> FindUserAccountAsync(int userId, int accountId)
        {
           return _db.AccountRecords.FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);
        }
        
        public Task<WalletRecord> FindUserWalletAsync(int accountId, int currencyId)
        {
            return _db.WalletRecords.FirstOrDefaultAsync(w => w.IdCurrency == currencyId && w.IdAccount == accountId);
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
                Status = RequestStatus.Active
            };

            _db.ConfirmRequestRecords.AddAsync(req);
            _db.SaveChangesAsync();
        }
        
        public async Task AddOperationHistory(OperationType operationType, int userId, int accountId, float amount, float commission, string currencyName)
        {
             await _db.OperationsHistoryRecords
                .AddAsync(new OperationsHistoryRecord
                {
                    UserId = userId,
                    AccountId = accountId,
                    Type = operationType,
                    Sum = amount,
                    Commission = commission,
                    Currency = currencyName,
                    Date = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss")
                });
        }
        
        public WalletRecord CreateNewWallet(int accountId, int currencyId)
        {
            return new WalletRecord {IdAccount = accountId, IdCurrency = currencyId, CashValue = 0.0f};
        }
    }
}