﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WeChat.Controllers {
	public class AccountController : OrderSystem.Controllers.BaseAccountController
    {
        public ActionResult Index()
        {
            return View("Account");
        }
    }
}