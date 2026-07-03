IF OBJECT_ID(N'[dbo].[Users]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Users]
    (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Email] NVARCHAR(256) NOT NULL,
        [NormalizedEmail] NVARCHAR(256) NOT NULL,
        [PasswordHash] NVARCHAR(500) NOT NULL,
        [Role] NVARCHAR(30) NOT NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_Users_IsActive] DEFAULT 1,
        [CreatedAtUtc] DATETIME2 NOT NULL,
        [UpdatedAtUtc] DATETIME2 NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );

    CREATE UNIQUE INDEX [IX_Users_NormalizedEmail]
        ON [dbo].[Users] ([NormalizedEmail]);
END;
