use AutoDb;
go

delete from dbo.Auto;

declare @Names table (Id int identity(1, 1), Val nvarchar(100));
declare @Producers table (Id int identity(1, 1), Val nvarchar(100));
declare @Colors table (Id int identity(1, 1), Val nvarchar(50));

insert into @Names (Val) values
    (N'Camry'), (N'Corolla'), (N'Octavia'), (N'Rapid'), (N'X5'),
    (N'X3'), (N'Logan'), (N'Duster'), (N'Focus'), (N'Mondeo');

insert into @Producers (Val) values
    (N'Toyota'), (N'Skoda'), (N'BMW'), (N'Renault'),
    (N'Ford'), (N'Kia'), (N'Hyundai'), (N'Mazda');

insert into @Colors (Val) values
    (N'Чёрный'), (N'Белый'), (N'Серебристый'), (N'Синий'),
    (N'Красный'), (N'Зелёный'), (N'Серый'), (N'Коричневый');

with n as
(
    select top (100) row_number() over (order by (select null)) as rn
    from sys.all_objects
)
insert into dbo.Auto (Name, Producer, Color, Cost)
select
    (select Val from @Names where Id = 1 + abs(checksum(newid())) % 10),
    (select Val from @Producers where Id = 1 + abs(checksum(newid())) % 8),
    (select Val from @Colors where Id = 1 + abs(checksum(newid())) % 8),
    cast(300000 + abs(checksum(newid())) % 4700001 as decimal(12, 2))
from n;
go

select count(*) as Total from dbo.Auto;
go
