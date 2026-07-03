IF OBJECT_ID(N'[dbo].[Trips]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Trips]
    (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [OwnerId] UNIQUEIDENTIFIER NOT NULL,
        [Title] NVARCHAR(120) NOT NULL,
        [Description] NVARCHAR(1000) NOT NULL,
        [StartDate] DATE NOT NULL,
        [EndDate] DATE NOT NULL,
        [PlannedBudget] DECIMAL(18, 2) NOT NULL,
        [Notes] NVARCHAR(2000) NOT NULL,
        [CreatedAtUtc] DATETIME2 NOT NULL,
        [UpdatedAtUtc] DATETIME2 NULL,
        CONSTRAINT [PK_Trips] PRIMARY KEY ([Id])
    );
END;

IF OBJECT_ID(N'[dbo].[Destinations]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Destinations]
    (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [TripId] UNIQUEIDENTIFIER NOT NULL,
        [Name] NVARCHAR(120) NOT NULL,
        [Location] NVARCHAR(200) NOT NULL,
        [ArrivalDate] DATE NOT NULL,
        [DepartureDate] DATE NOT NULL,
        [Description] NVARCHAR(1000) NOT NULL,
        CONSTRAINT [PK_Destinations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Destinations_Trips_TripId] FOREIGN KEY ([TripId])
            REFERENCES [dbo].[Trips] ([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_Destinations_TripId]
        ON [dbo].[Destinations] ([TripId]);
END;

IF OBJECT_ID(N'[dbo].[Activities]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Activities]
    (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [TripId] UNIQUEIDENTIFIER NOT NULL,
        [Title] NVARCHAR(120) NOT NULL,
        [Date] DATE NOT NULL,
        [Time] TIME NOT NULL,
        [Location] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(1000) NOT NULL,
        [EstimatedCost] DECIMAL(18, 2) NOT NULL,
        [Status] NVARCHAR(30) NOT NULL,
        CONSTRAINT [PK_Activities] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Activities_Trips_TripId] FOREIGN KEY ([TripId])
            REFERENCES [dbo].[Trips] ([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_Activities_TripId_Date_Time]
        ON [dbo].[Activities] ([TripId], [Date], [Time]);
END;

IF OBJECT_ID(N'[dbo].[Expenses]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Expenses]
    (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [TripId] UNIQUEIDENTIFIER NOT NULL,
        [Name] NVARCHAR(120) NOT NULL,
        [Category] NVARCHAR(30) NOT NULL,
        [Amount] DECIMAL(18, 2) NOT NULL,
        [Date] DATE NOT NULL,
        [Description] NVARCHAR(1000) NOT NULL,
        CONSTRAINT [PK_Expenses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Expenses_Trips_TripId] FOREIGN KEY ([TripId])
            REFERENCES [dbo].[Trips] ([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_Expenses_TripId_Date]
        ON [dbo].[Expenses] ([TripId], [Date]);
END;

IF OBJECT_ID(N'[dbo].[ChecklistItems]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ChecklistItems]
    (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [TripId] UNIQUEIDENTIFIER NOT NULL,
        [Text] NVARCHAR(200) NOT NULL,
        [IsCompleted] BIT NOT NULL,
        CONSTRAINT [PK_ChecklistItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ChecklistItems_Trips_TripId] FOREIGN KEY ([TripId])
            REFERENCES [dbo].[Trips] ([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_ChecklistItems_TripId]
        ON [dbo].[ChecklistItems] ([TripId]);
END;

IF OBJECT_ID(N'[dbo].[ShareLinks]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ShareLinks]
    (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [TripId] UNIQUEIDENTIFIER NOT NULL,
        [AccessLevel] NVARCHAR(30) NOT NULL,
        [TokenHash] NVARCHAR(128) NOT NULL,
        [ExpiresAtUtc] DATETIME2 NOT NULL,
        [CreatedAtUtc] DATETIME2 NOT NULL,
        [RevokedAtUtc] DATETIME2 NULL,
        CONSTRAINT [PK_ShareLinks] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ShareLinks_Trips_TripId] FOREIGN KEY ([TripId])
            REFERENCES [dbo].[Trips] ([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_ShareLinks_TripId]
        ON [dbo].[ShareLinks] ([TripId]);

    CREATE UNIQUE INDEX [IX_ShareLinks_TokenHash]
        ON [dbo].[ShareLinks] ([TokenHash]);
END;
