Create database Webapp
USE [Webapp]
go
--DROP DATABASE Webapp
-- Bảng sản phẩm (Products Table)
-- Bảng thương hiệu (Brands Table)
CREATE TABLE Brands (
    BrandID INT PRIMARY KEY IDENTITY(1,1),
    BrandName VARCHAR(255) NOT NULL,
    Description TEXT
);

-- Bảng danh mục (Categories Table)
CREATE TABLE Categories (
    CategoryID INT PRIMARY KEY IDENTITY(1,1),
    CategoryName VARCHAR(255) NOT NULL,
    Description TEXT
);

-- Bảng sản phẩm (Products Table)
CREATE TABLE Products (
    ProductID INT PRIMARY KEY IDENTITY(1,1),
    ProductName VARCHAR(255) NOT NULL,
    BrandID INT,
    CategoryID INT,
    Price DECIMAL(10, 2) NOT NULL,
    StockQuantity INT NOT NULL,
    Description TEXT,
    ImageURL VARCHAR(255),
    DateAdded DATETIME DEFAULT GETDATE(),
    LastUpdated DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (BrandID) REFERENCES Brands(BrandID),
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID)
);

-- Bảng người dùng (Users Table)
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    UserName VARCHAR(255) NOT NULL,
    Email VARCHAR(255) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    PhoneNumber VARCHAR(15),
    ShippingAddress TEXT,
    DateJoined DATETIME DEFAULT GETDATE(),
    UserType VARCHAR(50) CHECK (UserType IN ('Admin', 'Customer')) NOT NULL
);


-- Bảng trạng thái đơn hàng (OrderStatus Table)
CREATE TABLE OrderStatus (
    StatusID INT PRIMARY KEY IDENTITY(1,1),
    StatusName VARCHAR(50) NOT NULL,
    Description TEXT
);

-- Bảng đơn hàng (Orders Table)
CREATE TABLE Orders (
    OrderID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT,
    OrderDate DATETIME DEFAULT GETDATE(),
    TotalAmount DECIMAL(10, 2) NOT NULL,
    ShippingAddress TEXT,
    BillingAddress TEXT,
    PaymentMethod VARCHAR(50) CHECK (PaymentMethod IN ('Credit Card', 'E-Wallet')) NOT NULL,
    ShippingStatus INT,
    TrackingNumber VARCHAR(50),
    EstimatedDeliveryDate DATETIME,
    DeliveryDate DATETIME,
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (ShippingStatus) REFERENCES OrderStatus(StatusID)
);



-- Bảng chi tiết đơn hàng (OrderDetails Table)
CREATE TABLE OrderDetails (
    OrderDetailID INT PRIMARY KEY IDENTITY(1,1),
    OrderID INT,
    ProductID INT,
    Quantity INT NOT NULL,
    Price DECIMAL(10, 2) NOT NULL,
    Discount DECIMAL(10, 2) DEFAULT 0,
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID),
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);

-- Bảng lịch sử giao dịch (TransactionHistory Table)
CREATE TABLE TransactionHistory (
    TransactionID INT PRIMARY KEY IDENTITY(1,1),
    OrderID INT,
    TransactionDate DATETIME DEFAULT GETDATE(),
    PaymentMethod VARCHAR(50) CHECK (PaymentMethod IN ('Credit Card', 'E-Wallet')),
    TransactionAmount DECIMAL(10, 2) NOT NULL,
    TransactionStatus VARCHAR(50) CHECK (TransactionStatus IN ('Thành công', 'Thất bại', 'Đang xử lý')),
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID)
);

-- Bảng giỏ hàng (ShoppingCart Table)
CREATE TABLE ShoppingCart (
    CartID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT,
    ProductID INT,
    Quantity INT NOT NULL,
    AddedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);

-- Thêm chỉ mục vào bảng Products để tìm kiếm nhanh hơn theo BrandID và CategoryID
CREATE INDEX idx_products_brand_category ON Products (BrandID, CategoryID);

-- Thêm chỉ mục vào bảng Users để tìm kiếm nhanh hơn theo Email (Email thường được sử dụng để đăng nhập)
CREATE INDEX idx_users_email ON Users (Email);

-- Thêm chỉ mục vào bảng Orders để tìm kiếm nhanh hơn theo UserID và OrderDate
CREATE INDEX idx_orders_user_orderdate ON Orders (UserID, OrderDate);

-- Thêm chỉ mục vào bảng OrderDetails để tìm kiếm nhanh hơn theo OrderID
CREATE INDEX idx_orderdetails_orderid ON OrderDetails (OrderID);

-- Thêm chỉ mục vào bảng TransactionHistory để tìm kiếm nhanh hơn theo OrderID và TransactionDate
CREATE INDEX idx_transactionhistory_orderid_date ON TransactionHistory (OrderID, TransactionDate);

-- Thêm chỉ mục vào bảng ShoppingCart để tìm kiếm nhanh hơn theo UserID và ProductID
CREATE INDEX idx_shoppingcart_user_product ON ShoppingCart (UserID, ProductID);
