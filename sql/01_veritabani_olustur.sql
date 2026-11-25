-- 01) Veritabanını oluşturma
-- Dikkat: veritabanı zaten varsa siliniyor ve baştan kuruluyor, içindeki veriler gider.

USE master;
GO

-- Daha önce kurulmuşsa açık bağlantıları kapatıp siliyoruz
IF DB_ID(N'FilmPlatformuDB') IS NOT NULL
BEGIN
    ALTER DATABASE FilmPlatformuDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE FilmPlatformuDB;
END
GO

-- Turkish_CI_AS seçtik ki ç, ğ, ı, ö, ş, ü harfleri doğru sıralansın
CREATE DATABASE FilmPlatformuDB COLLATE Turkish_CI_AS;
GO

USE FilmPlatformuDB;
GO

PRINT N'FilmPlatformuDB veritabanı oluşturuldu.';
GO
