using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Proge.Teams.Edu.DAL.Repositories
{

    public interface IChangeNotificationRepository : IRepository
    {
        Task InsertChangeNotification(Entities.ChangeNotification changeNotification);
    }

    public class ChangeNotificationRepository : Repository<TeamsEduDbContext>, IChangeNotificationRepository
    {
        public ChangeNotificationRepository(TeamsEduDbContext context) : base(context) { }

        public async Task InsertChangeNotification(Entities.ChangeNotification changeNotification)
        {
             await this.CreateAsync(changeNotification);
        }
    }
}