
                局域网内的中小酒店管理系统

<img width="2078" height="1154" alt="image" src="https://github.com/user-attachments/assets/06af37eb-d214-40ed-b4b1-27d8f119d11f" />
﻿ 

## 技术栈

- ASP.NET Core 10.0（Razor Pages）
- Entity Framework Core 10.0
- SQLite（开发/部署都用这个）
- Bootstrap 5 + Flatpickr（日期选择器）

## 项目结构

```
hotel-system/
├── Pages/                      # Razor Pages
│   ├── Login/                  # 登录/退出
│   ├── Rooms/                  # 房态管理（核心页面）
│   ├── Folio/                  # 账单查询 + 收款
│   ├── Customers/              # 客户档案（admin only）
│   ├── Reports/                # 财务报表（admin only）
│   ├── Admin/                  # 系统设置（admin only）
│   ├── Records/                # 操作记录
│   └── Shared/_Layout.cshtml   # 导航栏（含角色判断）
│
├── Models/                     # 数据模型 + ViewModel
│   ├── Room.cs                 # 房间实体
│   ├── StayOrder.cs            # 订单实体
│   ├── Payment.cs              # 收款记录
│   ├── User.cs                 # 用户账号
│   ├── AuditLog.cs             # 审计日志
│   ├── RoomViewModel.cs        # 房间展示用
│   ├── CustomerViewModels.cs   # 客户档案用
│   ├── ReportViewModels.cs     # 财务报表用
│   └── RecordViewModel.cs      # 操作记录用
│
├── Data/
│   └── HotelDbContext.cs       # EF Core DbContext
│
├── Services/
│   └── RoomService.cs          # 业务逻辑（预订/入住/退房等）
│
├── wwwroot/
│   ├── css/
│   │   ├── rooms.css           # 房间网格样式
│   │   ├── login.css           # 登录页样式
│   │   ├── folio.css           # 账单页样式
│   │   ├── reports.css         # 财务报表样式
│   │   ├── customers.css       # 客户档案样式
│   │   ├── records.css         # 操作记录样式
│   │   └── admin.css           # 系统设置样式
│   └── js/
│       └── rooms.js            # 房态面板交互（弹窗/操作）
│
├── Program.cs                  # 入口：服务注册 + 种子数据
├── appsettings.json            # 配置（数据库连接字符串）
├── start.bat                   # 开发启动脚本
├── Hotel.web.csproj            # 项目文件
```

## 角色权限

| 角色 | Claim | 可访问页面 |
|------|-------|-----------|
| 前台 | `FrontDesk` | Rooms, Folio, Records |
| 管理员 | `Admin` | 全部，包含 Customers, Reports, Admin |

## 数据库

使用 SQLite，单文件 `HotelDB.sqlite`（自动生成）。

如需切换回 SQL Server：
1. 添加 `Microsoft.EntityFrameworkCore.SqlServer` 包
2. Program.cs 改 `UseSqlServer`
3. appsettings.json 改连接字符串


## 默认账号

| 用户名 | 密码 | 角色 |
|--------|------|------|
| admin | admin123 | 管理员 |
| front | front123 | 前台 |

## 页面路由

| 路由 | 页面 | 权限 |
|------|------|------|
| `/Login` | 登录 | 公开 |
| `/Rooms/Index` | 房态管理 | 登录用户 |
| `/Folio/Index` | 账单列表 | 登录用户 |
| `/Folio/Details/{id}` | 账单详情+收款 | 登录用户 |
| `/Customers/Index` | 客户档案 | Admin |
| `/Customers/Details/{name}` | 客户详情 | Admin |
| `/Reports/Index` | 财务报表 | Admin |
| `/Admin/Index` | 系统设置-房间管理 | Admin |
| `/Admin/Users` | 系统设置-账号管理 | Admin |
| `/Records/Index` | 操作记录 | 登录用户 |


