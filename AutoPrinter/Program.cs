﻿using HotelDAO.Models;
using Newtonsoft.Json;
using Protocal;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Utility;
using YummyOnlineTcpClient;

namespace AutoPrinter {
	class Program {
		private static int hotelId = Convert.ToInt32(ConfigurationManager.AppSettings["HotelId"]);
		private static string tcpServerIp = ConfigurationManager.AppSettings["TcpServerIp"].ToString();
		private static int tcpServerPort = Convert.ToInt32(ConfigurationManager.AppSettings["TcpServerPort"]);

		static void Main(string[] args) {
			Console.Title = "YummyOnline自助打印";
			Console.WriteLine($"版本: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}");
			Process[] tProcess = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
			if(tProcess.Length > 1) {
				Console.WriteLine("打印程序已经打开，请关闭后再打开打印程序。");
				Console.Read();
				return;
			}

			TcpClient tcp = new TcpClient(
				IPAddress.Parse(tcpServerIp),
				tcpServerPort,
				new PrintDineClientConnectProtocal(hotelId)
			);
			tcp.CallBackWhenMessageReceived = async (t, p) => {
				if(t != TcpProtocalType.PrintDine) {
					return;
				}
				PrintDineProtocal protocal = (PrintDineProtocal)p;

				await print(protocal.DineId, protocal.PrintTypes);
			};
			tcp.CallBackWhenConnected = () => {
				Console.WriteLine("服务器连接成功");
				log(Log.LogLevel.Success, "Printer Connected");
			};
			tcp.CallBackWhenExceptionOccured = e => {
				Console.WriteLine(e.Message);
				log(Log.LogLevel.Error, e.Message);
			};

			tcp.Start();

			while(true) {
				string cmd = Console.ReadLine();
				switch(cmd.ToLower()) {
					case "test":
						print("00000000000000", new List<PrintType> { PrintType.KitchenOrder, PrintType.Recipt, PrintType.ServeOrder }).Wait();
						break;
					case "list printers":
						List<string> printers = DinePrinter.ListPrinters();
						for(int i = 0; i < printers.Count; i++) {
							Console.WriteLine($"{i + 1} {printers[i]}");
						}
						break;
				}
			}
		}

		static async Task<DineForPrintingProtocal> getDineForPrinting(string dineId) {
			object postData = new {
				HotelId = hotelId,
				DineId = dineId
			};
			string response = await HttpPost.PostAsync(ConfigurationManager.AppSettings["RemoteGetDineForPrintingUrl"].ToString(), postData);
			if(response == null) {
				return null;
			}
			return JsonConvert.DeserializeObject<DineForPrintingProtocal>(response);
		}

		static async Task print(string dineId, List<PrintType> printTypes) {
			try {
				DineForPrintingProtocal dp = await getDineForPrinting(dineId);
				if(dp == null) {
					Console.WriteLine("获取订单信息失败，请检查网络设置");
					return;
				}
				DinePrinter dinePrinter = new DinePrinter(dp, printTypes);
				dinePrinter.Print();
				Console.WriteLine($"正在打印 单号: {dp.Dine.Id}");
			}
			catch(Exception e) {
				Console.WriteLine("无法打印, 请检查打印机设置");
				Console.WriteLine($"订单号: {dineId}, 错误信息: {e}");
				log(Log.LogLevel.Error, $"DineId: {dineId}, {e.Message}");
				return;
			}
			Console.WriteLine($"打印成功 单号: {dineId}");
			printCompleted(dineId);
		}

		static void printCompleted(string dineId) {
			object postData = new {
				HotelId = hotelId,
				DineId = dineId
			};
			var _ = HttpPost.PostAsync(ConfigurationManager.AppSettings["RemotePrintCompletedUrl"].ToString(), postData);
		}

		static void log(Log.LogLevel level, string message) {
			object postData = new {
				HotelId = hotelId,
				Level = level,
				Message = message
			};
			var _ = HttpPost.PostAsync(ConfigurationManager.AppSettings["RemoteLogUrl"].ToString(), postData);
		}
	}
}
