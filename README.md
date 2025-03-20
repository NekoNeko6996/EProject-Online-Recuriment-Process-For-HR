# EProject Online Recuriment Process For HR
> **_NOTE:_** Mọi người không chỉnh sửa mã do người khác viết, có thể inbook tác giả để hỏi và yêu cầu chỉnh sửa nhưng không được tự ý sửa mã không phải do bản thân viết!

## Nutget Package
1. Bootstrap
2. EntityFramework
3. FontAwesome
4. Microsoft.AspNet.Identity.EntityFramework
5. Install-Package Microsoft.AspNet.Identity.Owin

## Hướng Dẫn Xuất và Import Source Code ASP.NET MVC Không Bị Lỗi
### Cách Xuất Source Code từ Máy A
1. Mở Visual Studio ➜ **Clean Solution**.
2. Đóng Visual Studio.
3. Tắt chế độ IIS Express (nếu đang bật).
4. Xóa các thư mục tạm trong project:
   ```bash
   bin/
   obj/
   .vs/
   packages/
   ```
5. Nén toàn bộ thư mục project thành file .zip.

### Cách Import vào Máy B
1. Giải nén file .zip.
2. Mở Visual Studio.
3. Tools ➜ NuGet Package Manager ➜ Manage NuGet Packages for Solution.
4. Chọn tab Installed và nhấn Restore All Packages.
5. Chạy lệnh sau trong NuGet Console
   ```bash
   Update-Package -reinstall
   ```
6. Clean và Build lại solution

## Database
### Nếu kiểm tra mà không thấy database
  1. mở nutget package manager console
  2. chạy:
     ```
     Enable-migrations
     ```
     ```
     Update-database
     ```
