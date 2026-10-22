if db_id('AutoDb') is null
    create database AutoDb;
go

use AutoDb;
go

if object_id('dbo.Auto') is null
    create table dbo.Auto
    (
        Id       int identity(1, 1) not null constraint PK_Auto primary key,
        Name     nvarchar(100)      not null,
        Producer nvarchar(100)      not null,
        Color    nvarchar(50)       not null,
        Cost     decimal(12, 2)     not null
    );
go

if not exists (select 1 from dbo.Auto)
    insert into dbo.Auto (Name, Producer, Color, Cost)
    values
        (N'Camry', N'Toyota', N'Чёрный', 3200000.00),
        (N'Octavia', N'Skoda', N'Белый', 2450000.00),
        (N'X5', N'BMW', N'Синий', 8900000.00);
go
