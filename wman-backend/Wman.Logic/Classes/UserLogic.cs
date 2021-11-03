﻿using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wman.Data.DB_Models;
using Wman.Logic.DTO_Models;
using Wman.Logic.Interfaces;

namespace Wman.Logic.Classes
{
    public class UserLogic : IUserLogic
    {
        UserManager<WmanUser> userManager;
        IMapper mapper;
        public UserLogic(UserManager<WmanUser> userManager, IMapper mapper)
        {
            this.userManager = userManager;
            this.mapper = mapper;
        }
        public async Task<IEnumerable<WorkloadDTO>> GetWorkLoads(IEnumerable<string> usernames)
        {
            var output = new List<WorkloadDTO>();
            var allUsers = userManager.Users
                .Include(y => y.WorkEvents)
                .AsNoTracking();
            WmanUser selectedUser;
            var profileUrl = string.Empty;
            foreach (var username in usernames)
            {
                selectedUser = allUsers.Where(x => x.UserName == username).SingleOrDefault();
                if (selectedUser == null)
                {
                    throw new ArgumentException(String.Format("User: {0} doesn't exists!", username));
                }
                if (selectedUser.ProfilePicture != null)
                {
                    profileUrl = selectedUser.ProfilePicture.Url;
                }
                else
                {
                    profileUrl = string.Empty;
                }
                output.Add(new WorkloadDTO
                {
                    Username = username,
                    Percent = Convert.ToInt32(CalculateLoad(selectedUser)),
                    ProfilePicUrl = profileUrl
                }); ;
            }

            return output;
        }

        public async Task<IEnumerable<WorkloadDTO>> GetWorkLoads()
        {
            var output = new List<WorkloadDTO>();
            var allUsers = userManager.Users
                .Include(y => y.WorkEvents)
                .AsNoTracking();
            var profileUrl = string.Empty;
            foreach (var user in allUsers)
            {
                if (await userManager.IsInRoleAsync(user, "Worker"))
                {
                    //TODO: Move output.add inside this when role management is working.
                }
                if (user.ProfilePicture != null)
                {
                    profileUrl = user.ProfilePicture.Url;
                }
                else
                {
                    profileUrl = string.Empty;
                }
                output.Add(new WorkloadDTO
                {
                    Username = user.UserName,
                    Percent = Convert.ToInt32(CalculateLoad(user)),
                    ProfilePicUrl = profileUrl
                });
            }

            return output;
        }

        public async Task<IEnumerable<AssignedEventDTO>> WorkEventsOfUser(string username)
        {
            var selectedUser = await userManager.Users
                .Where(x => x.UserName == username)
                .Include(y => y.WorkEvents)
                .ThenInclude(z => z.Address)
                .AsNoTracking()
                .SingleOrDefaultAsync();
            if (selectedUser == null)
            {
                throw new ArgumentException("User not found!");
            }
            var output = selectedUser.WorkEvents;
            if (output.Count() == 0)
            {
                throw new InvalidOperationException("User has no assigned workEvents! ");
            }
            var testResult = mapper.Map<IEnumerable<AssignedEventDTO>>(output);

            return testResult;
        }

        private double CalculateLoad(WmanUser user)
        {
            var beforeToday = WorksBeforeToday(user.WorkEvents, DateTime.Now);
            var fromToday = RemainingWorks(user.WorkEvents, DateTime.Now);

            TimeSpan tsBeforeToday = TimeSpan.Zero;
            TimeSpan tsFromToday = TimeSpan.Zero;

            foreach (var item in beforeToday)
            {
                if (item.WorkFinishDate != DateTime.MinValue && item.WorkStartDate != DateTime.MinValue)
                {
                    tsBeforeToday += (item.WorkFinishDate - item.WorkStartDate);
                }
                else
                {
                    tsFromToday += (item.EstimatedFinishDate - item.EstimatedStartDate); //Work finish/start date is not valid, but it should already be. Assuming still in progress, and adding it to the remaining work pool
                }
            }
            foreach (var item in fromToday)
            {
                if (item.EstimatedFinishDate != DateTime.MinValue && item.EstimatedStartDate != DateTime.MinValue)
                {
                    tsFromToday += (item.EstimatedFinishDate - item.EstimatedStartDate);
                }
            }

            TimeSpan tsSUM = tsBeforeToday + tsFromToday;
            return (tsSUM.TotalHours / 168) * 100;
        }
        private IEnumerable<WorkEvent> WorksBeforeToday(IEnumerable<WorkEvent> works, DateTime selectedMonth)
        {
            return works.Where(x => x.WorkStartDate.Day < selectedMonth.Day && x.WorkStartDate.Year == selectedMonth.Year && x.WorkStartDate.Month == selectedMonth.Month);
        }

        private IEnumerable<WorkEvent> RemainingWorks(IEnumerable<WorkEvent> works, DateTime selectedMonth)
        {
            return works.Where(x => x.EstimatedStartDate.Day >= selectedMonth.Day && x.EstimatedStartDate.Year == selectedMonth.Year && x.EstimatedStartDate.Month == selectedMonth.Month && x.WorkStartDate == DateTime.MinValue);
        }

    }
}
