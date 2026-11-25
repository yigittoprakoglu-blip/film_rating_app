-- 07) Saklı yordamlar (stored procedure)
-- Arayüzdeki arama, ekleme, güncelleme ve silme işlemlerinin hepsi bu yordamları çağırıyor.

USE FilmPlatformuDB;
GO

-- Film / dizi arama: arayüzün solundaki filtrelerin hepsi buraya geliyor
-- Seçilmeyen filtre NULL geliyor ve o koşul atlanıyor.
-- Örnek: EXEC sp_IcerikAra @KategoriIDler = N'4,8', @MinImdb = 8.7;
CREATE OR ALTER PROCEDURE dbo.sp_IcerikAra
    @Aranan        NVARCHAR(100) = NULL,
    @KategoriIDler NVARCHAR(200) = NULL,   -- "4,8" gibi virgüllü liste
    @IcerikTuru    NVARCHAR(4)   = NULL,
    @Ulke          NVARCHAR(50)  = NULL,
    @YilMin        SMALLINT      = NULL,
    @YilMax        SMALLINT      = NULL,
    @MinImdb       DECIMAL(3,1)  = NULL,
    @Siralama      NVARCHAR(10)  = N'IMDb' -- 'IMDb', 'YeniEski', 'EskiYeni' ya da 'Ad'
AS
BEGIN
    SET NOCOUNT ON;

    SELECT v.IcerikID, v.Baslik, v.IcerikTuru, v.YayinYili, v.Ulke, v.ImdbPuani,
           v.Kategoriler, v.Yonetmenler, v.Sure, i.Afis
    FROM dbo.vw_IcerikDetaylari AS v
    JOIN dbo.Icerikler AS i ON i.IcerikID = v.IcerikID
    -- (@X IS NULL OR ...) kalıbı: filtre boşsa koşul her zaman doğru oluyor
    WHERE (@Aranan IS NULL
           OR v.Baslik         LIKE N'%' + @Aranan + N'%'
           OR v.OrijinalBaslik LIKE N'%' + @Aranan + N'%'
           OR v.Oyuncular      LIKE N'%' + @Aranan + N'%'
           OR v.Yonetmenler    LIKE N'%' + @Aranan + N'%'
           OR v.Yapimcilar     LIKE N'%' + @Aranan + N'%')
      -- STRING_SPLIT "4,8" yazısını satırlara bölüyor, seçilen türlerden herhangi biri yeterli
      AND (@KategoriIDler IS NULL OR EXISTS (
              SELECT 1 FROM dbo.IcerikKategorileri AS ik
              WHERE ik.IcerikID = v.IcerikID
                AND ik.KategoriID IN (SELECT CAST(value AS INT)
                                      FROM STRING_SPLIT(@KategoriIDler, N',')
                                      WHERE LTRIM(RTRIM(value)) <> N'')))
      AND (@IcerikTuru IS NULL OR v.IcerikTuru = @IcerikTuru)
      AND (@Ulke       IS NULL OR v.Ulke = @Ulke)
      AND (@YilMin     IS NULL OR v.YayinYili >= @YilMin)
      AND (@YilMax     IS NULL OR v.YayinYili <= @YilMax)
      AND (@MinImdb    IS NULL OR v.ImdbPuani >= @MinImdb)
    -- Sıralamayı parametreye göre değiştirmek için CASE kullandık
    ORDER BY
        CASE WHEN @Siralama = N'IMDb'     THEN v.ImdbPuani END DESC,
        CASE WHEN @Siralama = N'YeniEski' THEN v.YayinYili END DESC,
        CASE WHEN @Siralama = N'EskiYeni' THEN v.YayinYili END ASC,
        v.Baslik;
END;
GO

-- IMDb puanı en yüksek yapımlar, RANK() ile sıra numarası veriyoruz
-- Örnek: EXEC sp_EnIyiIcerikler @Adet = 5, @IcerikTuru = N'Film';
CREATE OR ALTER PROCEDURE dbo.sp_EnIyiIcerikler
    @Adet       INT         = 10,
    @IcerikTuru NVARCHAR(4) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@Adet)
           RANK() OVER (ORDER BY ImdbPuani DESC) AS Sira,   -- aynı puandakiler aynı sırada
           Baslik, IcerikTuru, YayinYili, ImdbPuani, Kategoriler, Sure
    FROM dbo.vw_IcerikDetaylari
    WHERE ImdbPuani IS NOT NULL
      AND (@IcerikTuru IS NULL OR IcerikTuru = @IcerikTuru)
    ORDER BY ImdbPuani DESC, Baslik;
END;
GO

-- Yeni film ya da dizi ekleme
-- 3 tabloya birden yazıyoruz (Icerikler + Filmler/Diziler + IcerikKategorileri).
-- Biri hata verirse yarım kayıt kalmasın diye hepsini tek TRANSACTION içinde yaptık.
CREATE OR ALTER PROCEDURE dbo.sp_IcerikEkle
    @Baslik         NVARCHAR(200),
    @OrijinalBaslik NVARCHAR(200)  = NULL,
    @IcerikTuru     NVARCHAR(4),
    @YayinYili      SMALLINT,
    @Ulke           NVARCHAR(50),
    @Dil            NVARCHAR(30),
    @YasSiniri      TINYINT        = 0,
    @ImdbPuani      DECIMAL(3,1)   = NULL,
    @YapimSirketi   NVARCHAR(200)  = NULL,
    @Konu           NVARCHAR(1000) = NULL,
    @SureDakika     SMALLINT       = NULL,   -- sadece filmlerde dolu
    @BitisYili      SMALLINT       = NULL,   -- sadece dizilerde
    @DiziDurumu     NVARCHAR(15)   = NULL,   -- sadece dizilerde
    @KategoriIDler  NVARCHAR(200)  = NULL,   -- "1,4" gibi
    @Afis           VARBINARY(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;   -- herhangi bir hatada transaction otomatik iptal olsun

    IF @IcerikTuru = N'Film' AND @SureDakika IS NULL
        THROW 50010, N'Film eklerken süre (dakika) girilmelidir.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Ortak bilgileri Icerikler'e ekliyoruz
        INSERT INTO dbo.Icerikler (Baslik, OrijinalBaslik, IcerikTuru, YayinYili, Ulke, Dil,
                                   YasSiniri, ImdbPuani, YapimSirketi, Konu, Afis)
        VALUES (@Baslik, @OrijinalBaslik, @IcerikTuru, @YayinYili, @Ulke, @Dil,
                @YasSiniri, @ImdbPuani, @YapimSirketi, @Konu, @Afis);

        DECLARE @YeniID INT = SCOPE_IDENTITY();   -- yeni eklenen satırın ID'si, diğer tablolarda lazım

        -- Türüne göre Filmler ya da Diziler'e ekliyoruz
        IF @IcerikTuru = N'Film'
            INSERT INTO dbo.Filmler (IcerikID, SureDakika) VALUES (@YeniID, @SureDakika);
        ELSE
            INSERT INTO dbo.Diziler (IcerikID, BitisYili, Durum)
            VALUES (@YeniID, @BitisYili, ISNULL(@DiziDurumu, N'Devam Ediyor'));

        -- Seçilen kategorileri ara tabloya ekliyoruz
        IF @KategoriIDler IS NOT NULL
            INSERT INTO dbo.IcerikKategorileri (IcerikID, KategoriID)
            SELECT DISTINCT @YeniID, CAST(LTRIM(RTRIM(value)) AS INT)
            FROM STRING_SPLIT(@KategoriIDler, N',')
            WHERE LTRIM(RTRIM(value)) <> N'';

        COMMIT TRANSACTION;   -- her şey yolundaysa kaydet
        SELECT @YeniID AS YeniIcerikID;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;   -- hata varsa hepsini geri al
        THROW;
    END CATCH
END;
GO

-- Film / dizi bilgilerini güncelleme
-- Kategorileri tek tek karşılaştırmak yerine hepsini silip seçilenleri yeniden ekliyoruz.
-- Film/Dizi türü sonradan değiştirilemiyor.
CREATE OR ALTER PROCEDURE dbo.sp_IcerikGuncelle
    @IcerikID       INT,
    @Baslik         NVARCHAR(200),
    @OrijinalBaslik NVARCHAR(200)  = NULL,
    @YayinYili      SMALLINT,
    @Ulke           NVARCHAR(50),
    @Dil            NVARCHAR(30),
    @YasSiniri      TINYINT        = 0,
    @ImdbPuani      DECIMAL(3,1)   = NULL,
    @YapimSirketi   NVARCHAR(200)  = NULL,
    @Konu           NVARCHAR(1000) = NULL,
    @SureDakika     SMALLINT       = NULL,
    @BitisYili      SMALLINT       = NULL,
    @DiziDurumu     NVARCHAR(15)   = NULL,
    @KategoriIDler  NVARCHAR(200)  = NULL,
    @Afis           VARBINARY(MAX) = NULL,   -- yeni afiş seçildiyse geliyor, NULL ise eskisi kalıyor
    @AfisKaldir     BIT            = 0       -- 1 gelirse afişi siliyoruz
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Tur NVARCHAR(4) = (SELECT IcerikTuru FROM dbo.Icerikler WHERE IcerikID = @IcerikID);
    IF @Tur IS NULL
        THROW 50030, N'Güncellenecek içerik bulunamadı.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.Icerikler
        SET Baslik = @Baslik, OrijinalBaslik = @OrijinalBaslik, YayinYili = @YayinYili,
            Ulke = @Ulke, Dil = @Dil, YasSiniri = @YasSiniri, ImdbPuani = @ImdbPuani,
            YapimSirketi = @YapimSirketi, Konu = @Konu,
            Afis = CASE WHEN @AfisKaldir = 1 THEN NULL ELSE ISNULL(@Afis, Afis) END
        WHERE IcerikID = @IcerikID;

        IF @Tur = N'Film'
            UPDATE dbo.Filmler SET SureDakika = ISNULL(@SureDakika, SureDakika) WHERE IcerikID = @IcerikID;
        ELSE
            UPDATE dbo.Diziler SET BitisYili = @BitisYili, Durum = ISNULL(@DiziDurumu, Durum)
            WHERE IcerikID = @IcerikID;

        -- Kategorileri sıfırlayıp yeniden yazıyoruz
        DELETE FROM dbo.IcerikKategorileri WHERE IcerikID = @IcerikID;
        IF @KategoriIDler IS NOT NULL
            INSERT INTO dbo.IcerikKategorileri (IcerikID, KategoriID)
            SELECT DISTINCT @IcerikID, CAST(LTRIM(RTRIM(value)) AS INT)
            FROM STRING_SPLIT(@KategoriIDler, N',')
            WHERE LTRIM(RTRIM(value)) <> N'';

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- Film / dizi silme
-- Sadece Icerikler'den siliyoruz, ON DELETE CASCADE sayesinde süresi, sezonları,
-- bölümleri, kategori ve oyuncu bağlantıları da kendiliğinden siliniyor.
CREATE OR ALTER PROCEDURE dbo.sp_IcerikSil
    @IcerikID INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.Icerikler WHERE IcerikID = @IcerikID;
    IF @@ROWCOUNT = 0   -- hiç satır silinmediyse böyle bir ID yok demektir
        THROW 50040, N'Silinecek içerik bulunamadı.', 1;
END;
GO

-- Diziye bölüm ekleme, girilen sezon yoksa önce sezonu oluşturuyoruz
-- Örnek: EXEC sp_BolumEkle @DiziID = 13, @SezonNo = 1, @BolumNo = 4, @BolumAdi = N'Double Lives', @SureDakika = 47;
CREATE OR ALTER PROCEDURE dbo.sp_BolumEkle
    @DiziID     INT,
    @SezonNo    TINYINT,
    @BolumNo    SMALLINT,
    @BolumAdi   NVARCHAR(200),
    @SureDakika SMALLINT,
    @SezonYili  SMALLINT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Diziler WHERE IcerikID = @DiziID)
        THROW 50020, N'Bu ID''ye sahip bir dizi bulunamadı.', 1;

    BEGIN TRANSACTION;

    -- Bu sezon var mı?
    DECLARE @SezonID INT = (SELECT SezonID FROM dbo.Sezonlar
                            WHERE DiziID = @DiziID AND SezonNo = @SezonNo);
    IF @SezonID IS NULL
    BEGIN
        -- Yoksa ekliyoruz (yılı girilmediyse bu yılı yazıyoruz)
        INSERT INTO dbo.Sezonlar (DiziID, SezonNo, YayinYili)
        VALUES (@DiziID, @SezonNo, ISNULL(@SezonYili, YEAR(GETDATE())));
        SET @SezonID = SCOPE_IDENTITY();
    END

    INSERT INTO dbo.Bolumler (SezonID, BolumNo, BolumAdi, SureDakika)
    VALUES (@SezonID, @BolumNo, @BolumAdi, @SureDakika);

    COMMIT TRANSACTION;
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS YeniBolumID;
END;
GO

-- Bölüm silme, sezonun son bölümü silindiyse boş sezon kalmasın diye sezonu da siliyoruz
CREATE OR ALTER PROCEDURE dbo.sp_BolumSil
    @BolumID INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @SezonID INT = (SELECT SezonID FROM dbo.Bolumler WHERE BolumID = @BolumID);
    IF @SezonID IS NULL
        THROW 50050, N'Silinecek bölüm bulunamadı.', 1;

    BEGIN TRANSACTION;
    DELETE FROM dbo.Bolumler WHERE BolumID = @BolumID;
    IF NOT EXISTS (SELECT 1 FROM dbo.Bolumler WHERE SezonID = @SezonID)
        DELETE FROM dbo.Sezonlar WHERE SezonID = @SezonID;
    COMMIT TRANSACTION;
END;
GO

-- Filme/diziye oyuncu ya da ekip üyesi ekleme
-- Kişi zaten kayıtlıysa onu kullanıyoruz, yoksa önce Kisiler'e ekliyoruz (aynı kişi iki kere kaydedilmesin).
CREATE OR ALTER PROCEDURE dbo.sp_KisiEkle
    @IcerikID    INT,
    @Ad          NVARCHAR(50),
    @Soyad       NVARCHAR(50),
    @Gorev       NVARCHAR(15),
    @KarakterAdi NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DECLARE @KisiID INT = (SELECT TOP (1) KisiID FROM dbo.Kisiler
                           WHERE Ad = @Ad AND Soyad = @Soyad ORDER BY KisiID);
    IF @KisiID IS NULL
    BEGIN
        INSERT INTO dbo.Kisiler (Ad, Soyad) VALUES (@Ad, @Soyad);
        SET @KisiID = SCOPE_IDENTITY();
    END

    -- Karakter adını sadece oyuncuysa yazıyoruz (tablodaki CHECK de bunu istiyor)
    INSERT INTO dbo.IcerikKisileri (IcerikID, KisiID, Gorev, KarakterAdi)
    VALUES (@IcerikID, @KisiID, @Gorev,
            CASE WHEN @Gorev = N'Oyuncu' THEN @KarakterAdi END);

    COMMIT TRANSACTION;
END;
GO

PRINT N'8 saklı yordam oluşturuldu.';
GO
