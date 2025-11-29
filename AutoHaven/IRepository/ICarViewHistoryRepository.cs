using AutoHaven.Models;
using System.Collections.Generic;

namespace AutoHaven.IRepository
{
    public interface ICarViewHistoryRepository
    {
        IEnumerable<CarViewHistoryModel> Get();
        IEnumerable<CarViewHistoryModel> GetByUserId(int userId);
        IEnumerable<CarViewHistoryModel> GetLatestPerListingForUser(int userId, int skip, int take);
        void Insert(CarViewHistoryModel item);
        void Delete(int id);
        void DeleteByUser(int userId);
    }
}
