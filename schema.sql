CREATE DATABASE CertifyDb;
   GO
   USE CertifyDb;
   GO

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
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [FullName] nvarchar(max) NOT NULL,
        [CPR] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE TABLE [CertificationTracks] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NULL,
        CONSTRAINT [PK_CertificationTracks] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE TABLE [Courses] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [Category] nvarchar(100) NOT NULL,
        [Fee] decimal(18,2) NOT NULL,
        [Capacity] int NOT NULL,
        [DurationHours] int NOT NULL,
        [PrerequisiteCourseId] int NULL,
        CONSTRAINT [PK_Courses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Courses_Courses_PrerequisiteCourseId] FOREIGN KEY ([PrerequisiteCourseId]) REFERENCES [Courses] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE TABLE [Instructors] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Email] nvarchar(255) NOT NULL,
        [ExpertiseAreas] nvarchar(500) NULL,
        CONSTRAINT [PK_Instructors] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE TABLE [Rooms] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Capacity] int NOT NULL,
        [Equipment] nvarchar(500) NULL,
        CONSTRAINT [PK_Rooms] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [Title] nvarchar(max) NOT NULL,
        [Message] nvarchar(500) NOT NULL,
        [EntityType] nvarchar(max) NOT NULL,
        [EntityId] int NULL,
        [IsRead] bit NOT NULL DEFAULT CAST(0 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [ReadAt] datetime2 NULL,
        [SendEmail] bit NOT NULL,
        [SendSms] bit NOT NULL,
        [ApplicationUserId] nvarchar(450) NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notifications_AspNetUsers_ApplicationUserId] FOREIGN KEY ([ApplicationUserId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_Notifications_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE TABLE [Payments] (
        [Id] int NOT NULL IDENTITY,
        [TraineeId] nvarchar(450) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [PaymentDate] datetime2 NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [Description] nvarchar(max) NULL,
        CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Payments_AspNetUsers_TraineeId] FOREIGN KEY ([TraineeId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE TABLE [Certifications] (
        [Id] int NOT NULL IDENTITY,
        [TraineeId] nvarchar(450) NOT NULL,
        [CertificationTrackId] int NOT NULL,
        [CertificateNumber] nvarchar(50) NOT NULL,
        [IssueDate] datetime2 NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_Certifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Certifications_AspNetUsers_TraineeId] FOREIGN KEY ([TraineeId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Certifications_CertificationTracks_CertificationTrackId] FOREIGN KEY ([CertificationTrackId]) REFERENCES [CertificationTracks] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE TABLE [TrackCourses] (
        [CertificationTrackId] int NOT NULL,
        [CourseId] int NOT NULL,
        [CourseId1] int NULL,
        CONSTRAINT [PK_TrackCourses] PRIMARY KEY ([CertificationTrackId], [CourseId]),
        CONSTRAINT [FK_TrackCourses_CertificationTracks_CertificationTrackId] FOREIGN KEY ([CertificationTrackId]) REFERENCES [CertificationTracks] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_TrackCourses_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_TrackCourses_Courses_CourseId1] FOREIGN KEY ([CourseId1]) REFERENCES [Courses] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE TABLE [ScheduledSessions] (
        [Id] int NOT NULL IDENTITY,
        [CourseId] int NOT NULL,
        [InstructorId] int NOT NULL,
        [RoomId] int NOT NULL,
        [StartDateTime] datetime2 NOT NULL,
        [EndDateTime] datetime2 NOT NULL,
        [Capacity] int NOT NULL,
        CONSTRAINT [PK_ScheduledSessions] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Session_Capacity] CHECK ([Capacity] > 0),
        CONSTRAINT [CK_Session_DateRange] CHECK ([EndDateTime] > [StartDateTime]),
        CONSTRAINT [FK_ScheduledSessions_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ScheduledSessions_Instructors_InstructorId] FOREIGN KEY ([InstructorId]) REFERENCES [Instructors] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ScheduledSessions_Rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE TABLE [Enrollments] (
        [Id] int NOT NULL IDENTITY,
        [TraineeId] nvarchar(450) NOT NULL,
        [ScheduledSessionId] int NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [PaymentStatus] nvarchar(50) NOT NULL,
        [Result] nvarchar(20) NULL,
        [EnrollmentFee] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_Enrollments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Enrollments_AspNetUsers_TraineeId] FOREIGN KEY ([TraineeId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Enrollments_ScheduledSessions_ScheduledSessionId] FOREIGN KEY ([ScheduledSessionId]) REFERENCES [ScheduledSessions] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConcurrencyStamp', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[AspNetRoles]'))
        SET IDENTITY_INSERT [AspNetRoles] ON;
    EXEC(N'INSERT INTO [AspNetRoles] ([Id], [ConcurrencyStamp], [Name], [NormalizedName])
    VALUES (N''1'', NULL, N''Trainee'', N''TRAINEE''),
    (N''2'', NULL, N''Instructor'', N''INSTRUCTOR''),
    (N''3'', NULL, N''TrainingCoordinator'', N''TRAININGCOORDINATOR'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConcurrencyStamp', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[AspNetRoles]'))
        SET IDENTITY_INSERT [AspNetRoles] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Certifications_CertificationTrackId] ON [Certifications] ([CertificationTrackId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Certifications_TraineeId_CertificationTrackId] ON [Certifications] ([TraineeId], [CertificationTrackId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Courses_Category] ON [Courses] ([Category]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Courses_PrerequisiteCourseId] ON [Courses] ([PrerequisiteCourseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Enrollments_ScheduledSessionId] ON [Enrollments] ([ScheduledSessionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Enrollments_TraineeId_ScheduledSessionId] ON [Enrollments] ([TraineeId], [ScheduledSessionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Instructors_Email] ON [Instructors] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_ApplicationUserId] ON [Notifications] ([ApplicationUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Payments_TraineeId] ON [Payments] ([TraineeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ScheduledSessions_CourseId] ON [ScheduledSessions] ([CourseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ScheduledSessions_InstructorId_StartDateTime_EndDateTime] ON [ScheduledSessions] ([InstructorId], [StartDateTime], [EndDateTime]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ScheduledSessions_RoomId_StartDateTime_EndDateTime] ON [ScheduledSessions] ([RoomId], [StartDateTime], [EndDateTime]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TrackCourses_CourseId] ON [TrackCourses] ([CourseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TrackCourses_CourseId1] ON [TrackCourses] ([CourseId1]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260513141835_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260513141835_InitialCreate', N'9.0.16');
END;

COMMIT;
GO

