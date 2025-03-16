using System;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Sem3EProjectOnlineCPFH.Models;

namespace Sem3EProjectOnlineCPFH
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            // Cấu hình sử dụng Cookie để lưu thông tin đăng nhập
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),  // Trang login mặc định
                LogoutPath = new PathString("/Account/Logout"),
                ExpireTimeSpan = TimeSpan.FromMinutes(60),  // Hết hạn sau 60 phút
                SlidingExpiration = true,
                CookieSecure = CookieSecureOption.SameAsRequest
            });
        }
    }
}
