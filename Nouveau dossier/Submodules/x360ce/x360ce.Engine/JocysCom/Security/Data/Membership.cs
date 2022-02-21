﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JocysCom.WebSites.Engine.Security.Data
{
	public partial class Membership
	{

		#region Generate / Check Tokens

		public const string PasswordResetKey = "Key";

		public string GetRedirectToken()
		{

			return JocysCom.ClassLibrary.Security.TokenHelper.GetSecurityToken(UserId, Password, JocysCom.ClassLibrary.TimeUnitType.Minutes);
		}

		public string GetPasswordResetToken()
		{
			return JocysCom.ClassLibrary.Security.TokenHelper.GetSecurityToken(UserId, Password, JocysCom.ClassLibrary.TimeUnitType.Hours);
		}

		public bool CheckRedirectToken(string key)
		{
			var isValid = JocysCom.ClassLibrary.Security.TokenHelper.CheckSecurityToken(key, UserId, Password, JocysCom.ClassLibrary.TimeUnitType.Minutes, 10);
			return isValid;
		}

		public bool CheckPasswordResetToken(string key)
		{
			var isValid = JocysCom.ClassLibrary.Security.TokenHelper.CheckSecurityToken(key, UserId, Password, JocysCom.ClassLibrary.TimeUnitType.Hours, 24);
			return isValid;
		}


		public static void SendPasswordResetKey(User user, string subject, string body)
		{
			var db = new SecurityEntities();
			var userId = user.UserId;
			var m = db.Memberships.FirstOrDefault(x => x.UserId == userId);
			var resetKey = m.GetPasswordResetToken();
			var resetUrl = JocysCom.ClassLibrary.Security.TokenHelper.GetUrl(PasswordResetKey, resetKey);
			var u = System.Web.HttpContext.Current.Request.Url;
			subject = subject.Replace("{Host}", u.Host);
			body = JocysCom.ClassLibrary.Text.Helper.Replace(body, user, false);
			body = body.Replace("{Host}", u.Host).Replace("{ResetKey}", resetUrl);
			var helper = JocysCom.ClassLibrary.Runtime.LogHelper.Current;
			helper.SendMailFrom(helper.Smtp.SmtpFrom, m.Email, "", "", subject, body, true);
		}

		#endregion


	}
}
