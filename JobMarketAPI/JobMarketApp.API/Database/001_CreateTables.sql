-- Create Tables
IF NOT EXISTS (
    SELECT 1 
    FROM sys.tables 
    WHERE name = 'Customers'
)
BEGIN
    CREATE TABLE Customers (
        [Id] UNIQUEIDENTIFIER NOT NULL 
            CONSTRAINT PK_Customers PRIMARY KEY
            DEFAULT NEWSEQUENTIALID(),
        [FirstName] VARCHAR(100) NOT NULL,
        [LastName] VARCHAR(100) NOT NULL
    )
END

IF NOT EXISTS (
    SELECT 1 
    FROM sys.tables 
    WHERE name = 'Contractors'
)
BEGIN
    CREATE TABLE Contractors (
        [Id] UNIQUEIDENTIFIER NOT NULL 
            CONSTRAINT PK_Contractors PRIMARY KEY
            DEFAULT NEWSEQUENTIALID(),
        [Name] NVARCHAR(100) NOT NULL,
        [Rating] DECIMAL(18,2) NULL
    )
END

IF NOT EXISTS (
    SELECT 1 
    FROM sys.tables 
    WHERE name = 'Jobs'
)
BEGIN
    CREATE TABLE Jobs (
        [Id] UNIQUEIDENTIFIER NOT NULL 
            CONSTRAINT PK_Jobs PRIMARY KEY
            DEFAULT NEWSEQUENTIALID(),
        [CustomerId] UNIQUEIDENTIFIER NOT NULL,
        [StartDate] DATETIME2(3) NOT NULL,
        [DueDate] DATETIME2(3) NOT NULL,
        [Budget] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [Description] NVARCHAR(1000) NOT NULL,
        [AcceptedBy] UNIQUEIDENTIFIER NULL DEFAULT NULL

        CONSTRAINT FK_Jobs_Customers FOREIGN KEY ([CustomerId]) REFERENCES Customers([Id])
    )
END

IF NOT EXISTS (
    SELECT 1 
    FROM sys.tables 
    WHERE name = 'JobOffers'
)
BEGIN
    CREATE TABLE JobOffers (
        [Id] UNIQUEIDENTIFIER NOT NULL 
            CONSTRAINT PK_JobOffers PRIMARY KEY
            DEFAULT NEWSEQUENTIALID(),
        [JobId] UNIQUEIDENTIFIER NOT NULL,
        [ContractorId] UNIQUEIDENTIFIER NOT NULL,
        [Price] DECIMAL(18, 2) NOT NULL DEFAULT 0,

        CONSTRAINT FK_JobOffers_Jobs FOREIGN KEY ([JobId]) REFERENCES Jobs([Id]),
        CONSTRAINT FK_JobOffers_Contractors FOREIGN KEY ([ContractorId]) REFERENCES Contractors([Id]),
    )
END