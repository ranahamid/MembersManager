CREATE TABLE [dbo].[ProfileDetails](
	[ProfileId] [int] NOT NULL,
	[SegmentId] [int] NOT NULL,
	[OptIn] [bit] NULL,
	[OptInDate] [date] NULL,
 CONSTRAINT [PK_ProfileDetails] PRIMARY KEY CLUSTERED 
(
	[ProfileId] ASC,
	[SegmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE procedure [dbo].[UpdateProfileDetails]
   @SegmentId as int,
   @OptIn as bit,
   @Query as nvarchar(1000)
AS
begin
	delete from ProfileDetails where SegmentId=@SegmentId
	declare @objcursor as cursor 
	declare @id as int, @cursorQuery as nvarchar(1000)
	set @cursorQuery = 'set @cursor = cursor forward_only static for ' + @Query + ' open @cursor;'
 
	exec sys.sp_executesql
		@cursorQuery
		,N'@cursor cursor output'
		,@objcursor output
 
	fetch next from @objcursor into @id 
	while (@@fetch_status = 0)
	begin
		insert into ProfileDetails(ProfileId,SegmentId,OptIn,OptInDate) values(@Id,@SegmentId,@OptIn,GetDate())
		update Profiles set OptIn=1 where Id=@Id
		fetch next from @objcursor into @id
	end 
	close @objcursor
	deallocate @objcursor

end

GO


--select *from Profiles where id< 38739 and OptIn=1



 public bool BulkSubscription(string query, int segmentId)
        {
            query = query.Replace("SELECT * FROM", "SELECT  prof.id as Id FROM");
            List<ProfileDetail> SelectedProfiles = new List<ProfileDetail>();
            _dbcontext.Database.ExecuteSqlCommand("UpdateProfileDetails @SegmentId, @OptIn, @Query",
            new SqlParameter("@SegmentId", segmentId),
            new SqlParameter("@OptIn", 1),
            new SqlParameter("@Query", query));

