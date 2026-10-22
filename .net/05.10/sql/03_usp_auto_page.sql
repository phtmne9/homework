use AutoDb;
go

create or alter procedure dbo.usp_AutoPage
    @Page     int = 1,
    @PageSize int = 10,
    @Producer nvarchar(100) = null
as
begin
    set nocount on;

    if @Page < 1 set @Page = 1;
    if @PageSize < 1 set @PageSize = 10;
    if @PageSize > 200 set @PageSize = 200;

    select Id, Name, Producer, Color, Cost
    from dbo.Auto
    where @Producer is null or Producer = @Producer
    order by Id
    offset (@Page - 1) * @PageSize rows
    fetch next @PageSize rows only;

    select count(*)
    from dbo.Auto
    where @Producer is null or Producer = @Producer;
end
go
