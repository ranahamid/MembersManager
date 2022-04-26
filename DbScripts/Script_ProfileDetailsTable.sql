SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProfileDetails](
 [Id] [int] IDENTITY(1,1) NOT NULL,
 [ProfileId] [int] NOT NULL,
 [SegmentId] [nvarchar](1024) NULL,
 [OptIn] [bit] NULL,
 [OptInDate] [date] NULL,
 CONSTRAINT [PK_ProfileDetails] PRIMARY KEY CLUSTERED 
(
 [Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [Members].[dbo].[Profiles]
ADD OptOut bit;