# EProject Onlinerecuriment Process For HR
> **_NOTE:_** Mọi người không chỉnh sửa mã do người khác viết, có thể inbook tác giả để hỏi và yêu cầu chỉnh sửa nhưng không được tự ý sửa mã không phải do bản thân viết!

## Nutget Package
1. Bootstrap
2. EntityFramework
3. FontAwesome
4. Microsoft.AspNet.Identity.EntityFramework
5. Install-Package Microsoft.AspNet.Identity.Owin

## Init dự án này
khởi tạo database
  1. mở nutget package manager console
  2. chạy:
     ```
     Enable-migrations
     ```
     ```
     Add-migration init
     ```
     ```
     Update-database
     ```
