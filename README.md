```sql
-- Creación de la tabla Car
CREATE TABLE Car (
    CarId INT PRIMARY KEY IDENTITY(1,1),
    Year INT,
    Make NVARCHAR(100),
    Model NVARCHAR(100),
    Submodel NVARCHAR(100),
    ZipCode NVARCHAR(10)
);

-- Creación de la tabla Buyer
CREATE TABLE Buyer (
    BuyerId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100)
);

-- Creación de la tabla Quote
CREATE TABLE Quote (
    QuoteId INT PRIMARY KEY IDENTITY(1,1),
    CarId INT,
    BuyerId INT,
    Amount DECIMAL(10, 2),
    IsCurrent BIT,
    FOREIGN KEY (CarId) REFERENCES Car(CarId),
    FOREIGN KEY (BuyerId) REFERENCES Buyer(BuyerId)
);

-- Creación de la tabla Status
CREATE TABLE Status (
    StatusId INT PRIMARY KEY IDENTITY(1,1),
    CarId INT,
    StatusName NVARCHAR(100),
    StatusDate DATETIME,
    ChangedBy NVARCHAR(100),
    FOREIGN KEY (CarId) REFERENCES Car(CarId)
);