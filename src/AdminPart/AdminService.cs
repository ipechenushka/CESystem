using System.Threading.Tasks;
using CESystem.DB;
using CESystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CESystem.AdminPart
{
    public interface IAdminService
    {
        public ValueTask<EntityEntry<CommissionRecord>> AddCommissionAsync(CommissionRecord commissionRecord);
        public ValueTask<EntityEntry<CurrencyRecord>> AddCurrencyAsync(CurrencyRecord currencyRecord);

        public EntityEntry<CurrencyRecord> DeleteCurrency(CurrencyRecord currencyRecord);

        public Task<ConfirmRequestRecord> FindRequestToConfirm(int reqId);
    }
    
    public class AdminService : IAdminService
    {
        private readonly LocalDbContext _db;

        public AdminService(LocalDbContext localDbContext) =>
            _db = localDbContext;

        public ValueTask<EntityEntry<CommissionRecord>> AddCommissionAsync(CommissionRecord commissionRecord) =>
            _db.CommissionRecords.AddAsync(commissionRecord);

        public ValueTask<EntityEntry<CurrencyRecord>> AddCurrencyAsync(CurrencyRecord currencyRecord) =>
            _db.CurrencyRecords.AddAsync(currencyRecord);

        public EntityEntry<CurrencyRecord> DeleteCurrency(CurrencyRecord currencyRecord) =>
            _db.Remove(currencyRecord);

        public Task<ConfirmRequestRecord> FindRequestToConfirm(int reqId) =>
            _db.ConfirmRequestRecords.FirstOrDefaultAsync(c => c.Id == reqId);
    }
}