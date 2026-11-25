-- 03) Tetikleyiciler (trigger)
-- Filmler tablosuna sadece türü 'Film', Diziler tablosuna sadece türü 'Dizi' olan içerik eklenebilsin.
-- Mesela Breaking Bad'e yanlışlıkla film süresi eklenirse hata versin.

USE FilmPlatformuDB;
GO

-- Filmler tablosuna ekleme/güncelleme olunca çalışıyor
CREATE OR ALTER TRIGGER trg_Filmler_TurKontrol
ON Filmler
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    -- inserted: o an eklenen satırlar. Türü Film olmayan var mı bakıyoruz.
    IF EXISTS (SELECT 1 FROM inserted AS f
               JOIN Icerikler AS i ON i.IcerikID = f.IcerikID
               WHERE i.IcerikTuru <> N'Film')
    BEGIN
        ROLLBACK TRANSACTION;   -- yapılan işlemi geri alıyoruz
        THROW 50001, N'Filmler tablosuna yalnızca türü ''Film'' olan içerik eklenebilir.', 1;
    END
END;
GO

-- Aynı kontrolün diziler için olanı
CREATE OR ALTER TRIGGER trg_Diziler_TurKontrol
ON Diziler
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM inserted AS d
               JOIN Icerikler AS i ON i.IcerikID = d.IcerikID
               WHERE i.IcerikTuru <> N'Dizi')
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50002, N'Diziler tablosuna yalnızca türü ''Dizi'' olan içerik eklenebilir.', 1;
    END
END;
GO

PRINT N'2 tetikleyici oluşturuldu.';
GO
