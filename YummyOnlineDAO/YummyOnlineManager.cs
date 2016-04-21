﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Threading.Tasks;
using YummyOnlineDAO.Models;

namespace YummyOnlineDAO {
	public class YummyOnlineManager {
		public YummyOnlineManager() {
			ctx = new YummyOnlineContext();
		}
		private YummyOnlineContext ctx;

		public async Task<SystemConfig> GetSystemConfig() {
			return await ctx.SystemConfigs.FirstOrDefaultAsync();
		}
		public async Task<bool> IsInNewDineInformClientGuids(Guid guid) {
			NewDineInformClientGuid clientGuid = await ctx.NewDineInformClientGuids.FirstOrDefaultAsync(p => p.Guid == guid);
			return clientGuid != null;
		}
		public async Task<List<NewDineInformClientGuid>> GetGuids() {
			return await ctx.NewDineInformClientGuids.ToListAsync();
		}
		public async Task<bool> AddGuid(NewDineInformClientGuid clientGuid) {
			ctx.NewDineInformClientGuids.Add(clientGuid);
			int result = await ctx.SaveChangesAsync();
			return result == 1;
		}
		public async Task<bool> DeleteGuid(Guid guid) {
			NewDineInformClientGuid clientGuid = await ctx.NewDineInformClientGuids.FirstOrDefaultAsync(p => p.Guid == guid);
			if(clientGuid == null) {
				return false;
			}
			ctx.NewDineInformClientGuids.Remove(clientGuid);
			await ctx.SaveChangesAsync();
			return true;
		}

		public async Task<Hotel> GetHotelById(int id) {
			return await ctx.Hotels.FirstOrDefaultAsync(p => p.Id == id);
		}
		public async Task<string> GetHotelConnectionStringById(int id) {
			return await ctx.Hotels.Where(p => p.Id == id).Select(p => p.ConnectionString).FirstOrDefaultAsync();
		}
		public async Task<List<Hotel>> GetHotels() {
			return await ctx.Hotels.ToListAsync();
		}
		public async Task<int> GetHotelCount() {
			return await ctx.Hotels.CountAsync();
		}

		public async Task RecordLog(Log.LogProgram program, Log.LogLevel level, string message) {
			Log log = new Log {
				Program = program,
				Level = level,
				Message = message
			};
			ctx.Logs.Add(log);
			await ctx.SaveChangesAsync();
		}

		public async Task<List<Log>> GetLogsByProgram(Log.LogProgram program, int count) {
			return await ctx.Logs.Where(p => p.Program == program)
				.OrderByDescending(p => p.Id).Take(count).ToListAsync();
		}
		public async Task<List<Log>> GetLogsByProgram(Log.LogProgram program, DateTime date, int count) {
			return await ctx.Logs.Where(p => p.Program == program && SqlFunctions.DateDiff("day", p.DateTime, date) == 0)
				.OrderByDescending(p => p.Id).Take(count).ToListAsync();
		}
		public async Task DeleteLogs(int days) {
			var logs = await ctx.Logs.Where(p => SqlFunctions.DateDiff("day", p.DateTime, DateTime.Now) >= days).ToListAsync();
			ctx.Logs.RemoveRange(logs);
			await ctx.SaveChangesAsync();
		}


		public async Task<List<User>> GetUsers(Role role) {
			return await ctx.UserRoles.Where(p => p.Role == role).Select(p => p.User).ToListAsync();
		}
		public async Task<int> GetUserCount(Role role) {
			return await ctx.UserRoles.CountAsync(p => p.Role == role);
		}
		public async Task<List<dynamic>> GetUserDailyCount(Role role) {
			List<dynamic> list = new List<dynamic>();
			for(int i = -30; i <= 0; i++) {
				DateTime t = DateTime.Now.AddDays(i);
				int count = await ctx.UserRoles.CountAsync(p => p.Role == role && SqlFunctions.DateDiff("d", p.User.CreateDate, t) == 0);
				list.Add(new {
					DateTime = t,
					Count = count
				});
			}
			return list;
		}

		public async Task<int> GetDineCount() {
			int dineCount = 0;
			List<Hotel> hotels = await GetHotels();
			foreach(Hotel h in hotels) {
				HotelDAO.HotelManagerForAdmin hotelManager = new HotelDAO.HotelManagerForAdmin(h.ConnectionString);
				dineCount += await hotelManager.GetDineCount(DateTime.Now);
			}
			return dineCount;
		}
		public async Task<List<dynamic>> GetDineDailyCount() {
			List<dynamic> list = new List<dynamic>();
			List<Hotel> hotels = await GetHotels();
			foreach(Hotel h in hotels) {
				List<dynamic> dailyCount = new List<dynamic>();
				HotelDAO.HotelManagerForAdmin hotelManager = new HotelDAO.HotelManagerForAdmin(h.ConnectionString);
				for(int i = -30; i <= 0; i++) {
					DateTime t = DateTime.Now.AddDays(i);
					int count = await hotelManager.GetDineCount(t);
					dailyCount.Add(new {
						DateTime = t,
						Count = count
					});
				}

				list.Add(new {
					HotelName = h.Name,
					DailyCount = dailyCount
				});
			}
			return list;
		}
	}
}
