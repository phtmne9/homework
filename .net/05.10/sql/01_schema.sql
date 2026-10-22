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

if not exists (select 1 from sys.indexes where name = 'IX_Auto_Producer')
    create index IX_Auto_Producer on dbo.Auto (Producer) include (Name, Color, Cost);
go
