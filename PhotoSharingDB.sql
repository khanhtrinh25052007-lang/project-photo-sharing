USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'PhotoSharingDB')
BEGIN
    ALTER DATABASE PhotoSharingDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE PhotoSharingDB;
END
GO

CREATE DATABASE PhotoSharingDB;
GO

USE PhotoSharingDB;
GO

-- 1. BẢNG USERS (NGƯỜI DÙNG)
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    AvatarUrl NVARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- 2. BẢNG PHOTOPOSTS (BÀI ĐĂNG ẢNH & CAPTION)
CREATE TABLE PhotoPosts (
    PostId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    ImageUrl NVARCHAR(500) NOT NULL,
    Caption NVARCHAR(1000) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_PhotoPosts_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

-- 3. BẢNG COMMENTS (BÌNH LUẬN)
CREATE TABLE Comments (
    CommentId INT IDENTITY(1,1) PRIMARY KEY,
    PostId INT NOT NULL,
    UserId INT NOT NULL,
    Content NVARCHAR(500) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Comments_PhotoPosts FOREIGN KEY (PostId) REFERENCES PhotoPosts(PostId) ON DELETE CASCADE,
    CONSTRAINT FK_Comments_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

-- 4. BẢNG FOLLOWS (THEO DÕI)
CREATE TABLE Follows (
    FollowId INT IDENTITY(1,1) PRIMARY KEY,
    FollowerId INT NOT NULL,
    FollowingId INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Follows_Follower FOREIGN KEY (FollowerId) REFERENCES Users(UserId),
    CONSTRAINT FK_Follows_Following FOREIGN KEY (FollowingId) REFERENCES Users(UserId),
    CONSTRAINT UQ_Follow_Pair UNIQUE (FollowerId, FollowingId)
);
GO

-- CHÈN DỮ LIỆU MẪU
INSERT INTO Users (Username, Email, PasswordHash, AvatarUrl, CreatedAt) VALUES
(N'ban_cua_toi', N'me@huflit.edu.vn', N'123456', N'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=100', GETDATE()),
(N'linh_nguyen', N'linh@gmail.com', N'123456', N'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=100', GETDATE()),
(N'minh_tuan', N'tuan@gmail.com', N'123456', N'https://images.unsplash.com/photo-1527980965255-d3b416303d12?w=100', GETDATE());
GO

INSERT INTO PhotoPosts (UserId, ImageUrl, Caption, CreatedAt) VALUES
(2, N'https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=800', N'Chiều hoàng hôn trên biển. Một chút bình yên sau những ngày chạy đồ án căng thẳng 🌅🌊 #sunset #travel', DATEADD(HOUR, -3, GETDATE())),
(1, N'https://images.unsplash.com/photo-1517694712202-14dd9538aa97?w=800', N'Góc học tập và code đồ án ASP.NET MVC đêm khuya cùng nhóm 💻☕ #coding #aspnet #mvc', DATEADD(HOUR, -1, GETDATE()));
GO

INSERT INTO Comments (PostId, UserId, Content, CreatedAt) VALUES
(1, 3, N'Màu ảnh đẹp quá bạn ơi! Chụp bằng máy gì thế?', DATEADD(MINUTE, -120, GETDATE())),
(1, 1, N'Góc chụp đỉnh thật sự 👏', DATEADD(MINUTE, -90, GETDATE())),
(2, 2, N'Cố lên nhé, giao diện nhìn xịn rồi đó!', DATEADD(MINUTE, -30, GETDATE()));
GO

INSERT INTO Follows (FollowerId, FollowingId, CreatedAt) VALUES
(1, 2, GETDATE());
GO