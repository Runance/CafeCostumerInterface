CREATE TABLE [dbo].[Customers] (
    [ID]          INT          IDENTITY (1, 1) NOT NULL,
    [Customer_id] VARCHAR (50) NOT NULL,
    [Entry_type]  VARCHAR (50) NOT NULL,
    [Entry_time]  DATETIME     DEFAULT (getdate()) NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC)
);