CREATE DATABASE movieplatformdb;
GO

USE movieplatformdb;
GO

CREATE TABLE [dbo].[user]
(
	[Id] INT NOT NULL PRIMARY KEY,
	[UserName] NVARCHAR(255) NOT NULL,
	[Password] NVARCHAR(255) NOT NULL,
)