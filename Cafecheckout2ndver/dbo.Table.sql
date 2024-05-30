CREATE TABLE [dbo].[Products]
(
	[Product_Id] VARCHAR(50) PRIMARY KEY NOT NULL,
	[Product_Name] VARCHAR(50),
	[Product_Desc] VARCHAR(50),
	[Product_Price] INT,
	[Product_Stocks] INT NULL
)
