-- 02) Tablolar, kısıtlar ve indeksler
-- Toplam 9 tablo var: Kategoriler, Icerikler, Filmler, Diziler, Sezonlar, Bolumler,
-- IcerikKategorileri, Kisiler, IcerikKisileri

USE FilmPlatformuDB;
GO

-- Kategoriler: Dram, Bilim Kurgu, Komedi gibi türler
CREATE TABLE Kategoriler (
    KategoriID      INT IDENTITY(1,1)   NOT NULL,
    KategoriAdi     NVARCHAR(50)        NOT NULL,
    CONSTRAINT PK_Kategoriler PRIMARY KEY (KategoriID),
    CONSTRAINT UQ_Kategoriler_Ad UNIQUE (KategoriAdi)   -- aynı kategori iki kere eklenmesin
);
GO

-- Icerikler: film ve dizilerin ortak bilgileri (başlık, yıl, IMDb, konu...)
-- Filme ve diziye özel bilgileri ayrı tablolara koyduk (üst tip / alt tip),
-- böylece aynı sütunları iki tabloda tekrar yazmadık.
CREATE TABLE Icerikler (
    IcerikID        INT IDENTITY(1,1)   NOT NULL,
    Baslik          NVARCHAR(200)       NOT NULL,
    OrijinalBaslik  NVARCHAR(200)       NULL,
    IcerikTuru      NVARCHAR(4)         NOT NULL,
    YayinYili       SMALLINT            NOT NULL,
    Ulke            NVARCHAR(50)        NOT NULL,
    Dil             NVARCHAR(30)        NOT NULL,
    YasSiniri       TINYINT             NOT NULL CONSTRAINT DF_Icerikler_YasSiniri DEFAULT (0),
    ImdbPuani       DECIMAL(3,1)        NULL,
    YapimSirketi    NVARCHAR(200)       NULL,
    Konu            NVARCHAR(1000)      NULL,
    Afis            VARBINARY(MAX)      NULL,      -- afiş resmi; boşsa arayüz kendisi bir afiş çiziyor
    CONSTRAINT PK_Icerikler PRIMARY KEY (IcerikID),
    -- CHECK kısıtlarıyla saçma değer girilmesini engelliyoruz
    CONSTRAINT CK_Icerikler_Tur  CHECK (IcerikTuru IN (N'Film', N'Dizi')),
    CONSTRAINT CK_Icerikler_Yil  CHECK (YayinYili BETWEEN 1888 AND 2100),   -- 1888 ilk filmin yılı
    CONSTRAINT CK_Icerikler_Yas  CHECK (YasSiniri IN (0, 7, 13, 16, 18)),
    CONSTRAINT CK_Icerikler_Imdb CHECK (ImdbPuani BETWEEN 1 AND 10)
);
GO

-- Filmler: sadece filmlerde olan bilgi (süre)
-- PK aynı zamanda FK, yani her film Icerikler'deki bir kayda bağlı (1-1 ilişki)
CREATE TABLE Filmler (
    IcerikID        INT                 NOT NULL,
    SureDakika      SMALLINT            NOT NULL,
    CONSTRAINT PK_Filmler PRIMARY KEY (IcerikID),
    CONSTRAINT CK_Filmler_Sure CHECK (SureDakika > 0),
    CONSTRAINT FK_Filmler_Icerik FOREIGN KEY (IcerikID)
        REFERENCES Icerikler (IcerikID) ON DELETE CASCADE   -- içerik silinince bu da silinsin
);
GO

-- Diziler: sadece dizilerde olan bilgiler (bitiş yılı, durum)
CREATE TABLE Diziler (
    IcerikID        INT                 NOT NULL,
    BitisYili       SMALLINT            NULL,        -- devam eden dizilerde boş
    Durum           NVARCHAR(15)        NOT NULL CONSTRAINT DF_Diziler_Durum DEFAULT (N'Devam Ediyor'),
    CONSTRAINT PK_Diziler PRIMARY KEY (IcerikID),
    CONSTRAINT CK_Diziler_Durum CHECK (Durum IN (N'Devam Ediyor', N'Tamamlandı', N'İptal')),
    CONSTRAINT FK_Diziler_Icerik FOREIGN KEY (IcerikID)
        REFERENCES Icerikler (IcerikID) ON DELETE CASCADE
);
GO

-- Sezonlar: bir dizinin birden fazla sezonu olabilir (1-N)
CREATE TABLE Sezonlar (
    SezonID         INT IDENTITY(1,1)   NOT NULL,
    DiziID          INT                 NOT NULL,
    SezonNo         TINYINT             NOT NULL,
    YayinYili       SMALLINT            NOT NULL,
    CONSTRAINT PK_Sezonlar PRIMARY KEY (SezonID),
    CONSTRAINT UQ_Sezonlar_DiziSezon UNIQUE (DiziID, SezonNo),   -- bir dizide iki tane "1. sezon" olamaz
    CONSTRAINT CK_Sezonlar_No CHECK (SezonNo >= 1),
    CONSTRAINT FK_Sezonlar_Dizi FOREIGN KEY (DiziID)
        REFERENCES Diziler (IcerikID) ON DELETE CASCADE
);
GO

-- Bolumler: her sezonun bölümleri ve süreleri
CREATE TABLE Bolumler (
    BolumID         INT IDENTITY(1,1)   NOT NULL,
    SezonID         INT                 NOT NULL,
    BolumNo         SMALLINT            NOT NULL,
    BolumAdi        NVARCHAR(200)       NOT NULL,
    SureDakika      SMALLINT            NOT NULL,
    CONSTRAINT PK_Bolumler PRIMARY KEY (BolumID),
    CONSTRAINT UQ_Bolumler_SezonBolum UNIQUE (SezonID, BolumNo),  -- aynı sezonda aynı bölüm no tekrar edemez
    CONSTRAINT CK_Bolumler_No CHECK (BolumNo >= 1),
    CONSTRAINT CK_Bolumler_Sure CHECK (SureDakika > 0),
    CONSTRAINT FK_Bolumler_Sezon FOREIGN KEY (SezonID)
        REFERENCES Sezonlar (SezonID) ON DELETE CASCADE
);
GO

-- IcerikKategorileri: ara tablo (çoka çok)
-- Bir film birden fazla kategoride, bir kategoride de birden fazla film olabiliyor.
CREATE TABLE IcerikKategorileri (
    IcerikID        INT                 NOT NULL,
    KategoriID      INT                 NOT NULL,
    CONSTRAINT PK_IcerikKategorileri PRIMARY KEY (IcerikID, KategoriID),   -- birleşik anahtar
    CONSTRAINT FK_IcerikKategorileri_Icerik FOREIGN KEY (IcerikID)
        REFERENCES Icerikler (IcerikID) ON DELETE CASCADE,
    CONSTRAINT FK_IcerikKategorileri_Kategori FOREIGN KEY (KategoriID)
        REFERENCES Kategoriler (KategoriID) ON DELETE CASCADE
);
GO

-- Kisiler: oyuncu, yönetmen, yapımcı ve senaristlerin hepsi burada
CREATE TABLE Kisiler (
    KisiID          INT IDENTITY(1,1)   NOT NULL,
    Ad              NVARCHAR(50)        NOT NULL,
    Soyad           NVARCHAR(50)        NOT NULL,
    DogumTarihi     DATE                NULL,
    Ulke            NVARCHAR(50)        NULL,
    CONSTRAINT PK_Kisiler PRIMARY KEY (KisiID)
);
GO

-- IcerikKisileri: ara tablo (çoka çok) + görev bilgisi
-- Görev'i de anahtara kattık çünkü aynı kişi aynı filmde birden fazla görev alabiliyor
-- (mesela Christopher Nolan hem yönetmen hem yapımcı).
CREATE TABLE IcerikKisileri (
    IcerikID        INT                 NOT NULL,
    KisiID          INT                 NOT NULL,
    Gorev           NVARCHAR(15)        NOT NULL,
    KarakterAdi     NVARCHAR(100)       NULL,
    CONSTRAINT PK_IcerikKisileri PRIMARY KEY (IcerikID, KisiID, Gorev),
    CONSTRAINT CK_IcerikKisileri_Gorev CHECK (Gorev IN (N'Oyuncu', N'Yönetmen', N'Yapımcı', N'Senarist')),
    CONSTRAINT CK_IcerikKisileri_Karakter CHECK (Gorev = N'Oyuncu' OR KarakterAdi IS NULL),   -- karakter adı sadece oyunculara
    CONSTRAINT FK_IcerikKisileri_Icerik FOREIGN KEY (IcerikID)
        REFERENCES Icerikler (IcerikID) ON DELETE CASCADE,
    CONSTRAINT FK_IcerikKisileri_Kisi FOREIGN KEY (KisiID)
        REFERENCES Kisiler (KisiID) ON DELETE CASCADE
);
GO

-- İndeksler: en çok arama yaptığımız sütunlara ekledik ki sorgular hızlı çalışsın
CREATE NONCLUSTERED INDEX IX_Icerikler_Baslik        ON Icerikler (Baslik);
CREATE NONCLUSTERED INDEX IX_Icerikler_TurYil        ON Icerikler (IcerikTuru, YayinYili);
CREATE NONCLUSTERED INDEX IX_IcerikKategorileri_Kat  ON IcerikKategorileri (KategoriID);
CREATE NONCLUSTERED INDEX IX_IcerikKisileri_Kisi     ON IcerikKisileri (KisiID);
GO

PRINT N'9 tablo, kısıtlar ve indeksler oluşturuldu.';
GO
