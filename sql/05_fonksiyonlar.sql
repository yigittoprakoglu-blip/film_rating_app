-- 05) Fonksiyonlar
-- 2 tane skaler (tek değer döndüren) ve 2 tane tablo döndüren fonksiyon yazdık.

USE FilmPlatformuDB;
GO

-- Dakikayı "2 sa 22 dk" şeklinde yazıyor (film sürelerini saat olarak göstermek için)
-- Örnek: SELECT dbo.fn_SureMetni(142);
CREATE OR ALTER FUNCTION dbo.fn_SureMetni (@Dakika INT)
RETURNS NVARCHAR(20)
AS
BEGIN
    IF @Dakika IS NULL RETURN NULL;
    IF @Dakika < 60 RETURN CAST(@Dakika AS NVARCHAR(10)) + N' dk';   -- 1 saatten kısaysa sadece dakika
    -- 60'a bölüm saati, 60'a bölümden kalan dakikayı veriyor
    RETURN CAST(@Dakika / 60 AS NVARCHAR(10)) + N' sa '
         + CAST(@Dakika % 60 AS NVARCHAR(10)) + N' dk';
END;
GO

-- Bir dizinin bütün bölümlerinin toplam süresi (dakika)
CREATE OR ALTER FUNCTION dbo.fn_DiziToplamSure (@DiziID INT)
RETURNS INT
AS
BEGIN
    RETURN (SELECT ISNULL(SUM(b.SureDakika), 0)   -- hiç bölüm yoksa NULL yerine 0 dönsün
            FROM dbo.Sezonlar AS s
            JOIN dbo.Bolumler AS b ON b.SezonID = s.SezonID
            WHERE s.DiziID = @DiziID);
END;
GO

-- Kategori adını verince o kategorideki yapımları getiriyor (tablo döndüren fonksiyon)
-- Örnek: SELECT * FROM dbo.fn_KategoriIcerikleri(N'Bilim Kurgu');
CREATE OR ALTER FUNCTION dbo.fn_KategoriIcerikleri (@KategoriAdi NVARCHAR(50))
RETURNS TABLE
AS
RETURN
(
    SELECT i.IcerikID, i.Baslik, i.IcerikTuru, i.YayinYili, i.ImdbPuani
    FROM dbo.Icerikler AS i
    JOIN dbo.IcerikKategorileri AS ik ON ik.IcerikID = i.IcerikID
    JOIN dbo.Kategoriler AS k ON k.KategoriID = ik.KategoriID
    WHERE k.KategoriAdi = @KategoriAdi
);
GO

-- Bir kişinin hangi yapımlarda ne görev aldığını gösteriyor (filmografi)
-- Örnek: SELECT * FROM dbo.fn_KisiFilmografisi(10);   -- 10 = Christopher Nolan
CREATE OR ALTER FUNCTION dbo.fn_KisiFilmografisi (@KisiID INT)
RETURNS TABLE
AS
RETURN
(
    SELECT k.Ad + N' ' + k.Soyad AS Kisi,
           i.Baslik, i.IcerikTuru, i.YayinYili, i.ImdbPuani,
           ik.Gorev, ik.KarakterAdi
    FROM dbo.IcerikKisileri AS ik
    JOIN dbo.Kisiler   AS k ON k.KisiID = ik.KisiID
    JOIN dbo.Icerikler AS i ON i.IcerikID = ik.IcerikID
    WHERE ik.KisiID = @KisiID
);
GO

PRINT N'4 fonksiyon oluşturuldu.';
GO
