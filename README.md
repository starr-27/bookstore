两个账号
管理员admin@bookstore.local  ChangeMe!12345
顾客    3496614013@qq.com 000000

如何在gitbash里运行
cd /d/0/WebApplication1/WebApplication1（切换到你的项目根目录）
dotnet run

方便在NAVICAT里查看
各种表的作用：
一、EF/Core 自己用的
•	__efmigrationshistory
EF Core 迁移历史表。记录每次 dotnet-ef migrations / database update 已经应用了哪些迁移，避免重复执行建表/改表。
二、Identity（登录/用户/角色/权限）相关
•	aspnetusers
用户主表。保存账号基本信息（Email、UserName、PasswordHash 等），以及我们扩展的 UserType（顾客/管理员）。
•	aspnetroles
角色表。保存角色定义，例如 Admin、Customer。
•	aspnetuserroles
用户-角色关联表。表示某个用户属于哪些角色（注册会加 Customer；种子管理员会加 Admin）。
•	aspnetuserclaims
用户 Claim 表。给单个用户附加声明（权限/属性）。本项目目前一般用不到，但 Identity 支持。
•	aspnetroleclaims
角色 Claim 表。给某个角色附加声明（权限/属性）。本项目目前未用。
•	aspnetuserlogins
外部登录映射表（例如 Google、Microsoft、微信等第三方登录）。本项目目前未用。
•	aspnetusertokens
用户令牌表（如重置密码、双因素、remember-me 等 token）。本项目目前未用，但 Identity 会用它支持这些功能。
三、书店业务表（你们系统功能）
书籍目录
•	books
图书主表：书号、书名、出版社、价格、库存、供书商等核心信息。
•	authors
作者表：作者基础信息（主要是名字）。
•	bookauthors
书-作者关联表（多对多），并带 AuthorOrder 用于“作者有序/最多4个作者”这种需求。
•	keywords
关键字表：关键字词条（比如“数据库”“Java”“算法”等）。
•	bookkeywords
书-关键字关联表（多对多），用于一本书多个关键字、一个关键字对应多本书。
•	publishers
出版社表：出版社名称等信息。
•	suppliers
供应商表：供书商/供货商基础信息（名称、联系人、电话、地址、启用状态等）。后台“供应商管理”用它。
顾客订单 / 发货
•	orders
订单主表：顾客是谁、收货信息、订单状态（Created/Paid/Shipped…）、总金额、时间等。
•	orderitems
订单明细表：一张订单里买了哪些书、每本书数量、下单时的单价（用于保留历史价格）。
•	shipments
发货表：一张订单对应的物流信息（快递公司、运单号、发货时间等）。后台“发货管理”填写这里，并把订单状态改为 Shipped。
采购 / 库存流水
•	purchaseorders
采购单主表：向哪个供应商采购、采购单状态、创建/收货时间等。后台“采购入库”会写入。
•	purchaseorderitems
采购单明细：采购单里采购了哪些书、数量、进货单价。
•	stockledgers
库存流水表（库存变动日志）：每次库存增加/减少都会记一笔（采购入库、销售出库、手动调整等），包含变动数量、变动后库存、时间、备注。后台“库存流水”列表就是查这个表。
