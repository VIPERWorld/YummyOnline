USE [YummyOnlineHotelBaseDB]
GO
BEGIN TRANSACTION
CREATE PARTITION FUNCTION [DinePartitionFun](nvarchar(14)) AS RANGE RIGHT FOR VALUES ()


CREATE PARTITION SCHEME [DinePartitionSchema] AS PARTITION [DinePartitionFun] TO ([PRIMARY])


ALTER TABLE [dbo].[DineMenus] DROP CONSTRAINT [FK_DineMenus_Dines]


ALTER TABLE [dbo].[DinePaidDetails] DROP CONSTRAINT [FK_DinePaidDetails_Dines]


ALTER TABLE [dbo].[DineRemarks] DROP CONSTRAINT [FK_DineRemarks_Dines]


ALTER TABLE [dbo].[TakeOuts] DROP CONSTRAINT [FK_TakeOuts_Dines]


ALTER TABLE [dbo].[Dines] DROP CONSTRAINT [PK_Dines]


SET ANSI_PADDING ON

ALTER TABLE [dbo].[Dines] ADD  CONSTRAINT [PK_Dines] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [DinePartitionSchema]([Id])


ALTER TABLE [dbo].[DineMenus]  WITH CHECK ADD  CONSTRAINT [FK_DineMenus_Dines] FOREIGN KEY([DineId])
REFERENCES [dbo].[Dines] ([Id])
ON DELETE CASCADE
ALTER TABLE [dbo].[DineMenus] CHECK CONSTRAINT [FK_DineMenus_Dines]


ALTER TABLE [dbo].[DinePaidDetails]  WITH CHECK ADD  CONSTRAINT [FK_DinePaidDetails_Dines] FOREIGN KEY([DineId])
REFERENCES [dbo].[Dines] ([Id])
ON DELETE CASCADE
ALTER TABLE [dbo].[DinePaidDetails] CHECK CONSTRAINT [FK_DinePaidDetails_Dines]


ALTER TABLE [dbo].[DineRemarks]  WITH CHECK ADD  CONSTRAINT [FK_DineRemarks_Dines] FOREIGN KEY([Dine_Id])
REFERENCES [dbo].[Dines] ([Id])
ON DELETE CASCADE
ALTER TABLE [dbo].[DineRemarks] CHECK CONSTRAINT [FK_DineRemarks_Dines]


ALTER TABLE [dbo].[TakeOuts]  WITH CHECK ADD  CONSTRAINT [FK_TakeOuts_Dines] FOREIGN KEY([Id])
REFERENCES [dbo].[Dines] ([Id])
ON DELETE CASCADE
ALTER TABLE [dbo].[TakeOuts] CHECK CONSTRAINT [FK_TakeOuts_Dines]

COMMIT TRANSACTION