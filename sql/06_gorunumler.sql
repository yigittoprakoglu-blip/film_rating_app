-- 06) Görünümler (view)
-- Uzun JOIN'li sorguları view olarak kaydettik, arayüzde her seferinde aynısını yazmıyoruz.

USE FilmPlatformuDB;
GO

-- Bir film/dizinin bütün bilgileri tek satırda (arayüzdeki detay kısmı buradan geliyor)
CREATE OR ALTER VIEW dbo.vw_IcerikDetaylari
AS
SELECT
    i.IcerikID,
    i.Baslik,
    i.OrijinalBaslik,
    i.IcerikTuru,
    i.YayinYili,
    i.Ulke,
    i.Dil,
    i.YasSiniri,
    i.ImdbPuani,
    i.Konu,
    i.YapimSirketi,
    -- Filmse "2 sa 22 dk", diziyse "2 sezon, 5 bölüm" yazıyoruz
    CASE WHEN i.IcerikTuru = N'Film'
         THEN dbo.fn_SureMetni(f.SureDakika)
         ELSE CAST(ds.SezonSayisi AS NVARCHAR(5)) + N' sezon, '
            + CAST(ds.BolumSayisi AS NVARCHAR(5)) + N' bölüm'
    END AS Sure,
    f.SureDakika AS FilmSureDakika,
    CASE WHEN i.IcerikTuru = N'Dizi' THEN ds.SezonSayisi END AS SezonSayisi,
    CASE WHEN i.IcerikTuru = N'Dizi' THEN ds.BolumSayisi END AS BolumSayisi,
    d.BitisYili,
    d.Durum AS DiziDurumu,
    kat.Kategoriler,
    yon.Yonetmenler,
    yap.Yapimcilar,
    oy.Oyuncular
FROM dbo.Icerikler AS i
-- LEFT JOIN kullandık çünkü içerik ya film ya dizi, ikisinden biri hep boş geliyor
LEFT JOIN dbo.Filmler AS f ON f.IcerikID = i.IcerikID
LEFT JOIN dbo.Diziler AS d ON d.IcerikID = i.IcerikID
-- Dizinin kaç sezonu ve kaç bölümü var
OUTER APPLY (
    SELECT COUNT(DISTINCT s.SezonID) AS SezonSayisi,
           COUNT(b.BolumID)          AS BolumSayisi
    FROM dbo.Sezonlar AS s
    LEFT JOIN dbo.Bolumler AS b ON b.SezonID = s.SezonID
    WHERE s.DiziID = i.IcerikID
) AS ds
-- Kategoriler: STRING_AGG birden fazla satırı "Dram, Suç" gibi tek yazıya çeviriyor
OUTER APPLY (
    SELECT STRING_AGG(k.KategoriAdi, N', ') WITHIN GROUP (ORDER BY k.KategoriAdi) AS Kategoriler
    FROM dbo.IcerikKategorileri AS ik
    JOIN dbo.Kategoriler AS k ON k.KategoriID = ik.KategoriID
    WHERE ik.IcerikID = i.IcerikID
) AS kat
-- Yönetmenler
OUTER APPLY (
    SELECT STRING_AGG(ks.Ad + N' ' + ks.Soyad, N', ') AS Yonetmenler
    FROM dbo.IcerikKisileri AS x
    JOIN dbo.Kisiler AS ks ON ks.KisiID = x.KisiID
    WHERE x.IcerikID = i.IcerikID AND x.Gorev = N'Yönetmen'
) AS yon
-- Yapımcılar
OUTER APPLY (
    SELECT STRING_AGG(ks.Ad + N' ' + ks.Soyad, N', ') AS Yapimcilar
    FROM dbo.IcerikKisileri AS x
    JOIN dbo.Kisiler AS ks ON ks.KisiID = x.KisiID
    WHERE x.IcerikID = i.IcerikID AND x.Gorev = N'Yapımcı'
) AS yap
-- Oyuncular
OUTER APPLY (
    SELECT STRING_AGG(ks.Ad + N' ' + ks.Soyad, N', ') AS Oyuncular
    FROM dbo.IcerikKisileri AS x
    JOIN dbo.Kisiler AS ks ON ks.KisiID = x.KisiID
    WHERE x.IcerikID = i.IcerikID AND x.Gorev = N'Oyuncu'
) AS oy;
GO

-- Dizilerin bölüm listesi, bölümlere "S01E01" gibi kod da veriyoruz
CREATE OR ALTER VIEW dbo.vw_DiziBolumleri
AS
SELECT
    i.IcerikID AS DiziID,
    i.Baslik   AS DiziAdi,
    s.SezonNo,
    b.BolumNo,
    -- RIGHT('0' + ...) tek haneli sayıların başına 0 koyuyor: 1 -> 01
    N'S' + RIGHT(N'0' + CAST(s.SezonNo AS NVARCHAR(3)), 2)
  + N'E' + RIGHT(N'0' + CAST(b.BolumNo AS NVARCHAR(3)), 2) AS BolumKodu,
    b.BolumID,
    b.BolumAdi,
    b.SureDakika,
    dbo.fn_SureMetni(b.SureDakika) AS Sure
FROM dbo.Icerikler AS i
JOIN dbo.Sezonlar  AS s ON s.DiziID  = i.IcerikID
JOIN dbo.Bolumler  AS b ON b.SezonID = s.SezonID;
GO

-- Kim hangi yapımda ne görev almış, tek tabloda görmek için
CREATE OR ALTER VIEW dbo.vw_OyuncuVeEkip
AS
SELECT
    i.IcerikID,
    i.Baslik,
    i.IcerikTuru,
    k.KisiID,
    k.Ad + N' ' + k.Soyad AS Kisi,
    ik.Gorev,
    ik.KarakterAdi,
    k.Ulke
FROM dbo.IcerikKisileri AS ik
JOIN dbo.Icerikler AS i ON i.IcerikID = ik.IcerikID
JOIN dbo.Kisiler   AS k ON k.KisiID  = ik.KisiID;
GO

-- Her kategoride kaç film, kaç dizi var ve ortalama IMDb puanı ne
CREATE OR ALTER VIEW dbo.vw_KategoriIstatistikleri
AS
SELECT
    k.KategoriID,
    k.KategoriAdi,
    SUM(CASE WHEN i.IcerikTuru = N'Film' THEN 1 ELSE 0 END) AS FilmSayisi,   -- SUM(CASE ...) ile sayıyoruz
    SUM(CASE WHEN i.IcerikTuru = N'Dizi' THEN 1 ELSE 0 END) AS DiziSayisi,
    CAST(AVG(i.ImdbPuani) AS DECIMAL(3,1))                   AS OrtalamaImdb
FROM dbo.Kategoriler AS k
LEFT JOIN dbo.IcerikKategorileri AS ik ON ik.KategoriID = k.KategoriID
LEFT JOIN dbo.Icerikler AS i ON i.IcerikID = ik.IcerikID
GROUP BY k.KategoriID, k.KategoriAdi;
GO

PRINT N'4 görünüm oluşturuldu.';
GO
