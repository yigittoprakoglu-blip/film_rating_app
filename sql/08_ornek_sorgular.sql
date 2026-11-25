-- 08) Örnek sorgular
-- Sunumda göstermek için. SSMS'te istediğimiz sorguyu seçip F5'e basarak tek tek çalıştırabiliriz.

USE FilmPlatformuDB;
GO

-- 1) SELECT + WHERE + ORDER BY: IMDb puanı 8.7 ve üzeri yapımlar
SELECT Baslik, IcerikTuru, YayinYili, ImdbPuani
FROM Icerikler
WHERE ImdbPuani >= 8.7
ORDER BY ImdbPuani DESC;

-- 2) INNER JOIN: filmler ve süreleri (saat olarak)
SELECT i.Baslik, f.SureDakika, dbo.fn_SureMetni(f.SureDakika) AS Sure
FROM Icerikler AS i
INNER JOIN Filmler AS f ON f.IcerikID = i.IcerikID
ORDER BY f.SureDakika DESC;

-- 3) Çoklu JOIN: Christopher Nolan'ın yönettiği filmler
SELECT i.Baslik, i.YayinYili, i.ImdbPuani
FROM Icerikler AS i
JOIN IcerikKisileri AS ik ON ik.IcerikID = i.IcerikID
JOIN Kisiler AS k ON k.KisiID = ik.KisiID
WHERE k.Ad = N'Christopher' AND k.Soyad = N'Nolan' AND ik.Gorev = N'Yönetmen';

-- 4) Bir filmin oyuncuları ve canlandırdıkları karakterler
SELECT k.Ad + N' ' + k.Soyad AS Oyuncu, ik.KarakterAdi
FROM IcerikKisileri AS ik
JOIN Kisiler AS k ON k.KisiID = ik.KisiID
WHERE ik.IcerikID = 3 AND ik.Gorev = N'Oyuncu';    -- Kara Şövalye

-- 5) Bir dizinin bölümleri ve süreleri
SELECT BolumKodu, BolumAdi, Sure
FROM vw_DiziBolumleri
WHERE DiziAdi = N'Breaking Bad'
ORDER BY SezonNo, BolumNo;

-- 6) GROUP BY + HAVING: 3'ten fazla yapımda yer alan kategoriler
SELECT k.KategoriAdi, COUNT(*) AS YapimSayisi
FROM Kategoriler AS k
JOIN IcerikKategorileri AS ik ON ik.KategoriID = k.KategoriID
GROUP BY k.KategoriAdi
HAVING COUNT(*) > 3
ORDER BY YapimSayisi DESC;

-- 7) Alt sorgu: IMDb ortalamasının üzerindeki yapımlar
SELECT Baslik, ImdbPuani
FROM Icerikler
WHERE ImdbPuani > (SELECT AVG(ImdbPuani) FROM Icerikler)
ORDER BY ImdbPuani DESC;

-- 8) Birden fazla yapımda yer alan kişiler
SELECT k.Ad + N' ' + k.Soyad AS Kisi, COUNT(DISTINCT ik.IcerikID) AS YapimSayisi
FROM Kisiler AS k
JOIN IcerikKisileri AS ik ON ik.KisiID = k.KisiID
GROUP BY k.Ad, k.Soyad
HAVING COUNT(DISTINCT ik.IcerikID) > 1;

-- 9) CTE + ROW_NUMBER: her kategorinin en yüksek puanlı yapımı
;WITH Sirali AS (
    SELECT k.KategoriAdi, i.Baslik, i.ImdbPuani,
           ROW_NUMBER() OVER (PARTITION BY k.KategoriID ORDER BY i.ImdbPuani DESC) AS Sira
    FROM Kategoriler AS k
    JOIN IcerikKategorileri AS ik ON ik.KategoriID = k.KategoriID
    JOIN Icerikler AS i ON i.IcerikID = ik.IcerikID
)
SELECT KategoriAdi, Baslik, ImdbPuani
FROM Sirali
WHERE Sira = 1
ORDER BY ImdbPuani DESC;

-- 10) Görünümler
SELECT * FROM vw_IcerikDetaylari ORDER BY ImdbPuani DESC;
SELECT * FROM vw_DiziBolumleri ORDER BY DiziAdi, SezonNo, BolumNo;
SELECT * FROM vw_OyuncuVeEkip ORDER BY Baslik, Gorev;
SELECT * FROM vw_KategoriIstatistikleri ORDER BY FilmSayisi + DiziSayisi DESC;

-- 11) Fonksiyonlar
SELECT dbo.fn_SureMetni(196) AS KisUykusuSuresi;
SELECT dbo.fn_DiziToplamSure(11) AS BreakingBadToplamDakika;
SELECT * FROM dbo.fn_KategoriIcerikleri(N'Bilim Kurgu');
SELECT * FROM dbo.fn_KisiFilmografisi(10);   -- Christopher Nolan

-- 12) Saklı yordamlar
EXEC sp_IcerikAra @Aranan = N'nolan';
EXEC sp_IcerikAra @KategoriIDler = N'4', @MinImdb = 8.7;        -- 4 = Bilim Kurgu
EXEC sp_IcerikAra @IcerikTuru = N'Dizi', @Ulke = N'Türkiye';
EXEC sp_EnIyiIcerikler @Adet = 5, @IcerikTuru = N'Film';

-- 13) Ekleme denemesi: TRANSACTION içinde ekleyip gösteriyoruz, sonra ROLLBACK ile geri alıyoruz
-- (böylece veritabanında deneme kaydı kalmıyor)
BEGIN TRANSACTION;
    EXEC sp_IcerikEkle @Baslik = N'Deneme Filmi', @IcerikTuru = N'Film', @YayinYili = 2024,
         @Ulke = N'Türkiye', @Dil = N'Türkçe', @ImdbPuani = 7.0, @SureDakika = 100, @KategoriIDler = N'1,6';
    SELECT Baslik, Sure, Kategoriler FROM vw_IcerikDetaylari WHERE Baslik = N'Deneme Filmi';
ROLLBACK TRANSACTION;

-- Tetikleyici denemesi: bir diziyi Filmler tablosuna eklemek hata verir
-- INSERT INTO Filmler (IcerikID, SureDakika) VALUES (11, 50);
GO
