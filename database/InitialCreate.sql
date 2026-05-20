IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520181103_InitialCreate'
)
BEGIN
    CREATE TABLE [Customers] (
        [Id] int NOT NULL IDENTITY,
        [CompanyName] nvarchar(180) NOT NULL,
        [ContactPerson] nvarchar(120) NOT NULL,
        [Phone] nvarchar(30) NOT NULL,
        [Email] nvarchar(160) NOT NULL,
        [City] nvarchar(80) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Customers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520181103_InitialCreate'
)
BEGIN
    CREATE TABLE [ErpModules] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(120) NOT NULL,
        [Description] nvarchar(300) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_ErpModules] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520181103_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] int NOT NULL IDENTITY,
        [FullName] nvarchar(120) NOT NULL,
        [Email] nvarchar(160) NOT NULL,
        [Role] nvarchar(40) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520181103_InitialCreate'
)
BEGIN
    CREATE TABLE [Branches] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(120) NOT NULL,
        [Address] nvarchar(240) NOT NULL,
        [City] nvarchar(80) NOT NULL,
        [CustomerId] int NOT NULL,
        CONSTRAINT [PK_Branches] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Branches_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520181103_InitialCreate'
)
BEGIN
    CREATE TABLE [Tickets] (
        [Id] int NOT NULL IDENTITY,
        [TicketNumber] nvarchar(40) NOT NULL,
        [Title] nvarchar(180) NOT NULL,
        [Description] nvarchar(4000) NOT NULL,
        [Priority] nvarchar(30) NOT NULL,
        [Status] nvarchar(40) NOT NULL,
        [CustomerId] int NOT NULL,
        [BranchId] int NULL,
        [ErpModuleId] int NOT NULL,
        [CreatedByUserId] int NOT NULL,
        [AssignedToUserId] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [ResolvedAt] datetime2 NULL,
        [ClosedAt] datetime2 NULL,
        CONSTRAINT [PK_Tickets] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Tickets_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Tickets_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Tickets_ErpModules_ErpModuleId] FOREIGN KEY ([ErpModuleId]) REFERENCES [ErpModules] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Tickets_Users_AssignedToUserId] FOREIGN KEY ([AssignedToUserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Tickets_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520181103_InitialCreate'
)
BEGIN
    CREATE TABLE [TicketComments] (
        [Id] int NOT NULL IDENTITY,
        [TicketId] int NOT NULL,
        [AuthorUserId] int NOT NULL,
        [Body] nvarchar(4000) NOT NULL,
        [IsInternal] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_TicketComments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TicketComments_Tickets_TicketId] FOREIGN KEY ([TicketId]) REFERENCES [Tickets] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_TicketComments_Users_AuthorUserId] FOREIGN KEY ([AuthorUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520181103_InitialCreate'
)
BEGIN
    CREATE TABLE [TicketStatusHistory] (
        [Id] int NOT NULL IDENTITY,
        [TicketId] int NOT NULL,
        [FromStatus] nvarchar(40) NULL,
        [ToStatus] nvarchar(40) NOT NULL,
        [ChangedByUserId] int NULL,
        [Note] nvarchar(800) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_TicketStatusHistory] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TicketStatusHistory_Tickets_TicketId] FOREIGN KEY ([TicketId]) REFERENCES [Tickets] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_TicketStatusHistory_Users_ChangedByUserId] FOREIGN KEY ([ChangedByUserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520181103_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Branches_CustomerId] ON [Branches] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520181103_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TicketComments_AuthorUserId] ON [TicketComments] ([AuthorUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520181103_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TicketComments_TicketId] ON [TicketComments] ([TicketId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520181103_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tickets_AssignedToUserId] ON [Tickets] ([AssignedToUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520181103_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tickets_BranchId] ON [Tickets] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520181103_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tickets_CreatedByUserId] ON [Tickets] ([CreatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520181103_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tickets_CustomerId] ON [Tickets] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520181103_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tickets_ErpModuleId] ON [Tickets] ([ErpModuleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520181103_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Tickets_TicketNumber] ON [Tickets] ([TicketNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520181103_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TicketStatusHistory_ChangedByUserId] ON [TicketStatusHistory] ([ChangedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520181103_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TicketStatusHistory_TicketId] ON [TicketStatusHistory] ([TicketId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520181103_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520181103_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260520181103_InitialCreate', N'10.0.8');
END;

COMMIT;
GO

