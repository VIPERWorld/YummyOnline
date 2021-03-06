﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YummyOnlineDAO.Models {
	public class SystemConfig {
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		/// <summary>
		/// tcp服务IP
		/// </summary>
		[MaxLength(15), Required]
		public string TcpServerIp { get; set; }
		/// <summary>
		/// tcp服务端口
		/// </summary>
		public int TcpServerPort { get; set; }
		/// <summary>
		/// tcp服务器目录
		/// </summary>
		public string TcpServerDir { get; set; }

		public string Token { get; set; }
	}
}
