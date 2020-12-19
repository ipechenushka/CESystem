using System;
using System.Collections.Generic;
using System.Linq;
using CESystem.DB;
using CESystem.Models;

namespace CESystem.AdminPart
{
    public interface IAdminContext
    {
        public void AddRequestToConfirmTransfer(Purse fromPurse, Purse toPurse, double sum);
        public void AddRequestToConfirmOther(Purse userPurse, double sum, string operation);
        public void ConfirmAllRequests();

        public int RequestCount();
    }
    
    public class AdminContext : IAdminContext
    {
        private List<Tuple<Purse, Purse, double>> _toConfirmTransfer;
        private List<Tuple<Purse, double, string>> _toConfirmOther;
        
        public AdminContext()
        {
            _toConfirmTransfer = new List<Tuple<Purse, Purse, double>>();
            _toConfirmOther =  new List<Tuple<Purse, double, string>>();
        }

        public int RequestCount()
        {
            return _toConfirmOther.Count + _toConfirmTransfer.Count;
        }

        public void AddRequestToConfirmTransfer(Purse fromPurse, Purse toPurse, double sum)
        {
            _toConfirmTransfer.Add(new Tuple<Purse, Purse, double>(fromPurse, toPurse, sum));
        }
        
        public void AddRequestToConfirmOther(Purse userPurse, double sum, string operation)
        {
            _toConfirmOther.Add(new Tuple<Purse, double, string>(userPurse, sum, operation));
        }

        public void ConfirmAllRequests()
        {
            using (LocalDbContext db = new LocalDbContext())
            {

                foreach (var value in _toConfirmTransfer)
                {
                    Purse fromPurse = db.Purses.FirstOrDefault(p =>
                        p.IdAccount == value.Item1.IdAccount && p.IdCurrency == value.Item1.IdCurrency);
                    Purse toPurse = db.Purses.FirstOrDefault(p =>
                        p.IdAccount == value.Item2.IdAccount && p.IdCurrency == value.Item2.IdCurrency);

                    toPurse.CashValue += value.Item3;
                    fromPurse.CashValue -= value.Item3;
                }

                foreach (var value in _toConfirmOther)
                {
                    Purse userPurse = db.Purses.FirstOrDefault(p =>
                        p.IdAccount == value.Item1.IdAccount && p.IdCurrency == value.Item1.IdCurrency);

                    if (value.Item3.Equals("deposit"))
                        userPurse.CashValue += value.Item2;
                    else
                        userPurse.CashValue -= value.Item2;
                }

                _toConfirmOther.Clear();
                _toConfirmTransfer.Clear();

                db.SaveChanges();
            }
        }
    }
}