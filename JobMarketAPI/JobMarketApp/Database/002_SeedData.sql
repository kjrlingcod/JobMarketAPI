IF NOT EXISTS (SELECT 1 FROM dbo.Customers)
BEGIN
    INSERT INTO dbo.Customers (Id, FirstName, LastName)
    VALUES
    (NEWID(), 'Klyde', 'Lingcod'),
    (NEWID(), 'John', 'Smith'),
    (NEWID(), 'Jane', 'Doe'),
    (NEWID(), 'Abigail', 'Campbell');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Contractors)
BEGIN
    INSERT INTO dbo.Contractors (Id, Name, Rating)
    VALUES
    (NEWID(), 'Keystone Projects', 4.50),
    (NEWID(), 'Ironclad Builders', 4.20),
    (NEWID(), 'Solid Build Co.', 3.60),
    (NEWID(), 'Precision Builders', 4.90);
END;