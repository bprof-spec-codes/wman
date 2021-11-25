﻿using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Wman.Data.DB_Models;
using Wman.Logic.DTO_Models;
using Wman.Logic.Helpers;
using Wman.Logic.Interfaces;
using Wman.Repository.Interfaces;

namespace Wman.Logic.Classes
{
    public class UserLogic : IUserLogic
    {
        UserManager<WmanUser> userManager;
        IMapper mapper;
        //EventRepo eventRepo;
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
                .Include(z => z.ProfilePicture);
            WmanUser selectedUser;
            var profileUrl = string.Empty;
            foreach (var username in usernames)
            {
                selectedUser = allUsers.Where(x => x.UserName == username).SingleOrDefault();
                if (selectedUser == null)
                {
                    throw new NotFoundException(WmanError.UserNotFound);
                }
                if (await userManager.IsInRoleAsync(selectedUser, "Worker") == false)
                {
                    throw new NotMemberOfRoleException(WmanError.NotAWorker);
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
                    userID = selectedUser.Id,
                    Username = username,
                    Percent = Convert.ToInt32(CalculateLoad(selectedUser)),
                    ProfilePicUrl = profileUrl
                });
            }

            return output;
        }

        public async Task<IEnumerable<WorkloadDTO>> GetWorkLoads()
        {
            var output = new List<WorkloadDTO>();
            var allUsers = userManager.Users
                .Include(y => y.WorkEvents)
                .Include(z => z.ProfilePicture);
            var profileUrl = string.Empty;
            foreach (var user in allUsers)
            {
                if (await userManager.IsInRoleAsync(user, "Worker"))
                {
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
                        userID = user.Id,
                        Username = user.UserName,
                        Percent = Convert.ToInt32(CalculateLoad(user)),
                        ProfilePicUrl = profileUrl
                    });
                }
                
            }

            return output;
        }

        public async Task<IEnumerable<AssignedEventDTO>> WorkEventsOfUser(string username) //Kept this as a legacy method, because it might be already used on FE with this older DTO. But this essentially does the same as this.WorkEventsOfLoggedInUser(), just a differently formatted output. Should probably be deleted, together with the endpoint referencing this. 
        {

            var events = await this.GetWorkEventsOfUser(username);
            var mapped = mapper.Map<IEnumerable<AssignedEventDTO>>(events);

            return mapped;
        }

        public async Task<IEnumerable<WorkEventForWorkCardDTO>> WorkEventsOfLoggedInUser(string username)
        {
            var events = await this.GetWorkEventsOfUser(username);
            var output = mapper.Map<IEnumerable<WorkEventForWorkCardDTO>>(events);
            return output;
        }

        public async Task<WorkEventForWorkCardDTO> GetEventDetailsForWorker(string username, int id)
        {
            var selectedUser = await userManager.Users
                .Where(x => x.UserName == username)
                .Include(y => y.WorkEvents)
                .AsNoTracking()
                .SingleOrDefaultAsync();
            if (selectedUser == null)
            {
                throw new NotFoundException(WmanError.UserNotFound);
            }
            var job = selectedUser.WorkEvents.Where(x => x.Id == id).SingleOrDefault();
            if (job != null)
            {
                return mapper.Map<WorkEventForWorkCardDTO>(job);
            }
            throw new InvalidOperationException(WmanError.NotHisBusiness);
        }

        private async Task<IEnumerable<WorkEvent>> GetWorkEventsOfUser(string username)
        {
            var selectedUser = await userManager.Users
                .Where(x => x.UserName == username)
                .Include(y => y.WorkEvents)
                .ThenInclude(z => z.Address)
                .AsNoTracking()
                .SingleOrDefaultAsync();
            if (selectedUser == null)
            {
                throw new NotFoundException(WmanError.UserNotFound);
            }
            var output = selectedUser.WorkEvents;
            return output;
        }

        private double CalculateLoad(WmanUser user)
        {
            var selectedDate = DateTime.Now;
            var beforeToday = WorksBeforeToday(user.WorkEvents, selectedDate);
            var fromToday = RemainingWorks(user.WorkEvents, selectedDate);

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
            var hoursInMonth = DateTime.DaysInMonth(selectedDate.Year, selectedDate.Month) * 8;
            return (tsSUM.TotalHours / hoursInMonth) * 100;
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
