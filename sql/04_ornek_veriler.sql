-- 04) Örnek veriler
-- Boş veritabanında çalıştırılmalı, çünkü ID'lerin 1'den başlayarak verildiğini varsaydık.
-- IMDb puanlarını yaklaşık girdik, zamanla değişebiliyor.

USE FilmPlatformuDB;
GO
SET NOCOUNT ON;
GO

-- Kategoriler (ID'leri 1-12)
INSERT INTO Kategoriler (KategoriAdi) VALUES
 (N'Dram'), (N'Suç'), (N'Aksiyon'), (N'Bilim Kurgu'), (N'Gerilim'), (N'Komedi'),
 (N'Fantastik'), (N'Gizem'), (N'Savaş'), (N'Romantik'), (N'Korku'), (N'Macera');
GO

-- İçerikler: 1-10 arası film, 11-15 arası dizi
INSERT INTO Icerikler (Baslik, OrijinalBaslik, IcerikTuru, YayinYili, Ulke, Dil, YasSiniri, ImdbPuani, YapimSirketi, Konu) VALUES
 (N'Esaretin Bedeli', N'The Shawshank Redemption', N'Film', 1994, N'ABD', N'İngilizce', 16, 9.3,
  N'Castle Rock Entertainment',
  N'Karısını ve sevgilisini öldürmekle suçlanan bankacı Andy Dufresne, ömür boyu hapis cezasıyla Shawshank Cezaevi''ne gönderilir. Yıllar içinde mahkûm Red ile derin bir dostluk kurar ve umudunu hiç kaybetmez.'),
 (N'Baba', N'The Godfather', N'Film', 1972, N'ABD', N'İngilizce', 18, 9.2,
  N'Paramount Pictures',
  N'Corleone mafya ailesinin reisi Vito Corleone''ye yapılan suikastın ardından, aile işlerinden uzak duran oğlu Michael istemeden de olsa ailenin başına geçer.'),
 (N'Kara Şövalye', N'The Dark Knight', N'Film', 2008, N'ABD', N'İngilizce', 13, 9.0,
  N'Warner Bros. / Legendary Pictures / Syncopy',
  N'Batman, Komiser Gordon ve savcı Harvey Dent ile birlikte Gotham''daki organize suçu bitirmeye çalışırken, şehri kaosa sürükleyen Joker ile karşı karşıya gelir.'),
 (N'Başlangıç', N'Inception', N'Film', 2010, N'ABD', N'İngilizce', 13, 8.8,
  N'Warner Bros. / Legendary Pictures / Syncopy',
  N'İnsanların rüyalarına girerek sırlarını çalan Dom Cobb''a, bu kez bir fikri çalmak yerine birinin zihnine yerleştirme görevi verilir.'),
 (N'Yıldızlararası', N'Interstellar', N'Film', 2014, N'ABD', N'İngilizce', 7, 8.7,
  N'Paramount Pictures / Warner Bros. / Legendary Pictures / Syncopy',
  N'Dünya yaşanmaz hale gelirken eski pilot Cooper, insanlık için yeni bir yuva bulmak amacıyla bir solucan deliğinden geçen ekibe katılır.'),
 (N'Parazit', N'Gisaengchung', N'Film', 2019, N'Güney Kore', N'Korece', 16, 8.5,
  N'Barunson E&A',
  N'Yoksul Kim ailesi, zengin Park ailesinin evine birer birer çalışan olarak sızar; ancak evin sakladığı sır her şeyi değiştirir.'),
 (N'Ayla', NULL, N'Film', 2017, N'Türkiye', N'Türkçe', 13, 8.3,
  N'Dijital Sanatlar',
  N'Kore Savaşı''nda Türk askeri Süleyman, savaş alanında bulduğu küçük bir kız çocuğunu sahiplenir ve ona Ayla adını verir. Gerçek bir hikâyeden uyarlanmıştır.'),
 (N'Babam ve Oğlum', NULL, N'Film', 2005, N'Türkiye', N'Türkçe', 7, 8.2,
  N'Avşar Film',
  N'Yıllar önce babasıyla küsüp köyü terk eden Sadık, küçük oğluyla birlikte Ege''deki baba evine geri döner ve ailesiyle yüzleşir.'),
 (N'Kış Uykusu', NULL, N'Film', 2014, N'Türkiye', N'Türkçe', 13, 8.0,
  N'Zeyno Film',
  N'Kapadokya''da küçük bir otel işleten emekli oyuncu Aydın, kış gelince karısı ve kız kardeşiyle olan ilişkilerindeki çatlaklarla yüzleşir. Cannes''da Altın Palmiye kazanmıştır.'),
 (N'Matrix', N'The Matrix', N'Film', 1999, N'ABD', N'İngilizce', 16, 8.7,
  N'Warner Bros. / Village Roadshow Pictures',
  N'Bilgisayar korsanı Neo, yaşadığı dünyanın makineler tarafından yaratılmış bir simülasyon olduğunu öğrenir ve gerçeklik için savaşan direnişe katılır.'),
 (N'Breaking Bad', NULL, N'Dizi', 2008, N'ABD', N'İngilizce', 18, 9.5,
  N'Sony Pictures Television',
  N'Kanser teşhisi konan kimya öğretmeni Walter White, ailesinin geleceğini güvenceye almak için eski öğrencisi Jesse ile uyuşturucu üretmeye başlar.'),
 (N'Stranger Things', NULL, N'Dizi', 2016, N'ABD', N'İngilizce', 16, 8.6,
  N'21 Laps Entertainment / Netflix',
  N'1980''lerde Hawkins kasabasında küçük Will''in ortadan kaybolmasıyla arkadaşları, gizemli güçlere sahip bir kız ve doğaüstü olaylarla karşılaşır.'),
 (N'Dark', NULL, N'Dizi', 2017, N'Almanya', N'Almanca', 16, 8.7,
  N'Wiedemann & Berg / Netflix',
  N'Küçük bir Alman kasabasında kaybolan çocuklar, dört ailenin nesiller boyu süren sırlarını ve zaman yolculuğu gizemini ortaya çıkarır.'),
 (N'Ezel', NULL, N'Dizi', 2009, N'Türkiye', N'Türkçe', 16, 8.6,
  N'Ay Yapım',
  N'En yakın arkadaşları ve sevdiği kadın tarafından ihanete uğrayıp hapse düşen Ömer, yüzünü ve kimliğini değiştirerek Ezel adıyla intikam almak için geri döner.'),
 (N'Leyla ile Mecnun', NULL, N'Dizi', 2011, N'Türkiye', N'Türkçe', 7, 8.8,
  NULL,
  N'Aynı gün doğan Leyla ile Mecnun''un klasik aşk hikâyesi, absürt mizah ve fantastik öğelerle yeniden yorumlanır.');
GO

-- Film süreleri (dakika), ilk 10 içerik film olduğu için onları ekliyoruz
INSERT INTO Filmler (IcerikID, SureDakika) VALUES
 (1, 142), (2, 175), (3, 152), (4, 148), (5, 169),
 (6, 132), (7, 125), (8, 108), (9, 196), (10, 136);
GO

-- Dizi bilgileri, 11-15 arası diziler
INSERT INTO Diziler (IcerikID, BitisYili, Durum) VALUES
 (11, 2013, N'Tamamlandı'),
 (12, 2025, N'Tamamlandı'),
 (13, 2020, N'Tamamlandı'),
 (14, 2011, N'Tamamlandı'),
 (15, 2013, N'Tamamlandı');
GO

-- Sezonlar (ID'leri 1-7), sağdaki yorumda hangi diziye ait olduğu yazıyor
INSERT INTO Sezonlar (DiziID, SezonNo, YayinYili) VALUES
 (11, 1, 2008),   -- 1  Breaking Bad S1
 (11, 2, 2009),   -- 2  Breaking Bad S2
 (12, 1, 2016),   -- 3  Stranger Things S1
 (12, 2, 2017),   -- 4  Stranger Things S2
 (13, 1, 2017),   -- 5  Dark S1
 (14, 1, 2009),   -- 6  Ezel S1
 (15, 1, 2011);   -- 7  Leyla ile Mecnun S1
GO

-- Bölümler: her diziye birkaç örnek bölüm ekledik, gerisi arayüzden eklenebiliyor
INSERT INTO Bolumler (SezonID, BolumNo, BolumAdi, SureDakika) VALUES
 (1, 1, N'Pilot', 58),
 (1, 2, N'Cat''s in the Bag...', 48),
 (1, 3, N'...And the Bag''s in the River', 48),
 (2, 1, N'Seven Thirty-Seven', 47),
 (2, 2, N'Grilled', 48),
 (3, 1, N'Chapter One: The Vanishing of Will Byers', 48),
 (3, 2, N'Chapter Two: The Weirdo on Maple Street', 55),
 (3, 3, N'Chapter Three: Holly, Jolly', 51),
 (4, 1, N'Chapter One: MADMAX', 48),
 (5, 1, N'Secrets', 51),
 (5, 2, N'Lies', 44),
 (5, 3, N'Past and Present', 45),
 (6, 1, N'1. Bölüm', 90),
 (6, 2, N'2. Bölüm', 90),
 (6, 3, N'3. Bölüm', 90),
 (7, 1, N'1. Bölüm', 90),
 (7, 2, N'2. Bölüm', 90),
 (7, 3, N'3. Bölüm', 90);
GO

-- Hangi yapım hangi türde (içerik ID, kategori ID)
-- 1 Dram, 2 Suç, 3 Aksiyon, 4 Bilim Kurgu, 5 Gerilim, 6 Komedi,
-- 7 Fantastik, 8 Gizem, 9 Savaş, 10 Romantik, 11 Korku, 12 Macera
INSERT INTO IcerikKategorileri (IcerikID, KategoriID) VALUES
 (1, 1), (1, 2),
 (2, 1), (2, 2),
 (3, 3), (3, 2), (3, 1),
 (4, 4), (4, 3), (4, 5),
 (5, 4), (5, 1), (5, 12),
 (6, 1), (6, 5), (6, 6),
 (7, 1), (7, 9),
 (8, 1),
 (9, 1),
 (10, 4), (10, 3),
 (11, 1), (11, 2), (11, 5),
 (12, 4), (12, 11), (12, 8),
 (13, 4), (13, 8), (13, 5),
 (14, 1), (14, 2),
 (15, 6), (15, 10), (15, 7);
GO

-- Kişiler (ID'leri 1-61), sağdaki numara kişinin ID'si
INSERT INTO Kisiler (Ad, Soyad, DogumTarihi, Ulke) VALUES
 (N'Frank', N'Darabont', NULL, N'ABD'),                  -- 1
 (N'Tim', N'Robbins', NULL, N'ABD'),                     -- 2
 (N'Morgan', N'Freeman', '1937-06-01', N'ABD'),          -- 3
 (N'Niki', N'Marvin', NULL, N'ABD'),                     -- 4
 (N'Francis Ford', N'Coppola', NULL, N'ABD'),            -- 5
 (N'Marlon', N'Brando', '1924-04-03', N'ABD'),           -- 6
 (N'Al', N'Pacino', '1940-04-25', N'ABD'),               -- 7
 (N'James', N'Caan', NULL, N'ABD'),                      -- 8
 (N'Albert S.', N'Ruddy', NULL, N'Kanada'),              -- 9
 (N'Christopher', N'Nolan', '1970-07-30', N'İngiltere'), -- 10
 (N'Emma', N'Thomas', NULL, N'İngiltere'),               -- 11
 (N'Christian', N'Bale', '1974-01-30', N'İngiltere'),    -- 12
 (N'Heath', N'Ledger', '1979-04-04', N'Avustralya'),     -- 13
 (N'Gary', N'Oldman', NULL, N'İngiltere'),               -- 14
 (N'Charles', N'Roven', NULL, N'ABD'),                   -- 15
 (N'Leonardo', N'DiCaprio', '1974-11-11', N'ABD'),       -- 16
 (N'Joseph', N'Gordon-Levitt', NULL, N'ABD'),            -- 17
 (N'Matthew', N'McConaughey', NULL, N'ABD'),             -- 18
 (N'Anne', N'Hathaway', NULL, N'ABD'),                   -- 19
 (N'Jessica', N'Chastain', NULL, N'ABD'),                -- 20
 (N'Joon-ho', N'Bong', NULL, N'Güney Kore'),             -- 21
 (N'Kang-ho', N'Song', NULL, N'Güney Kore'),             -- 22
 (N'Sin-ae', N'Kwak', NULL, N'Güney Kore'),              -- 23
 (N'Can', N'Ulkay', NULL, N'Türkiye'),                   -- 24
 (N'İsmail', N'Hacıoğlu', NULL, N'Türkiye'),             -- 25
 (N'Çağan', N'Irmak', NULL, N'Türkiye'),                 -- 26
 (N'Fikret', N'Kuşkan', NULL, N'Türkiye'),               -- 27
 (N'Çetin', N'Tekindor', NULL, N'Türkiye'),              -- 28
 (N'Nuri Bilge', N'Ceylan', NULL, N'Türkiye'),           -- 29
 (N'Haluk', N'Bilginer', NULL, N'Türkiye'),              -- 30
 (N'Melisa', N'Sözen', NULL, N'Türkiye'),                -- 31
 (N'Demet', N'Akbağ', NULL, N'Türkiye'),                 -- 32
 (N'Zeynep', N'Özbatur Atakan', NULL, N'Türkiye'),       -- 33
 (N'Lana', N'Wachowski', NULL, N'ABD'),                  -- 34
 (N'Lilly', N'Wachowski', NULL, N'ABD'),                 -- 35
 (N'Keanu', N'Reeves', '1964-09-02', N'Kanada'),         -- 36
 (N'Laurence', N'Fishburne', NULL, N'ABD'),              -- 37
 (N'Carrie-Anne', N'Moss', NULL, N'Kanada'),             -- 38
 (N'Joel', N'Silver', NULL, N'ABD'),                     -- 39
 (N'Vince', N'Gilligan', NULL, N'ABD'),                  -- 40
 (N'Bryan', N'Cranston', '1956-03-07', N'ABD'),          -- 41
 (N'Aaron', N'Paul', NULL, N'ABD'),                      -- 42
 (N'Anna', N'Gunn', NULL, N'ABD'),                       -- 43
 (N'Matt', N'Duffer', NULL, N'ABD'),                     -- 44
 (N'Ross', N'Duffer', NULL, N'ABD'),                     -- 45
 (N'Shawn', N'Levy', NULL, N'Kanada'),                   -- 46
 (N'Millie Bobby', N'Brown', NULL, N'İngiltere'),        -- 47
 (N'Winona', N'Ryder', NULL, N'ABD'),                    -- 48
 (N'David', N'Harbour', NULL, N'ABD'),                   -- 49
 (N'Baran bo', N'Odar', NULL, N'Almanya'),               -- 50
 (N'Jantje', N'Friese', NULL, N'Almanya'),               -- 51
 (N'Louis', N'Hofmann', NULL, N'Almanya'),               -- 52
 (N'Uluç', N'Bayraktar', NULL, N'Türkiye'),              -- 53
 (N'Kerem', N'Çatay', NULL, N'Türkiye'),                 -- 54
 (N'Kenan', N'İmirzalıoğlu', NULL, N'Türkiye'),          -- 55
 (N'Cansu', N'Dere', NULL, N'Türkiye'),                  -- 56
 (N'Tuncel', N'Kurtiz', NULL, N'Türkiye'),               -- 57
 (N'Onur', N'Ünlü', NULL, N'Türkiye'),                   -- 58
 (N'Ali', N'Atay', NULL, N'Türkiye'),                    -- 59
 (N'Serkan', N'Keskin', NULL, N'Türkiye'),               -- 60
 (N'Ahmet Mümtaz', N'Taylan', NULL, N'Türkiye');         -- 61
GO

-- Kim hangi yapımda ne iş yapmış, oyuncular için karakter adını da yazdık
INSERT INTO IcerikKisileri (IcerikID, KisiID, Gorev, KarakterAdi) VALUES
 -- 1 Esaretin Bedeli
 (1, 1, N'Yönetmen', NULL), (1, 1, N'Senarist', NULL), (1, 4, N'Yapımcı', NULL),
 (1, 2, N'Oyuncu', N'Andy Dufresne'), (1, 3, N'Oyuncu', N'Ellis "Red" Redding'),
 -- 2 Baba
 (2, 5, N'Yönetmen', NULL), (2, 9, N'Yapımcı', NULL),
 (2, 6, N'Oyuncu', N'Vito Corleone'), (2, 7, N'Oyuncu', N'Michael Corleone'), (2, 8, N'Oyuncu', N'Sonny Corleone'),
 -- 3 Kara Şövalye
 (3, 10, N'Yönetmen', NULL), (3, 10, N'Yapımcı', NULL), (3, 11, N'Yapımcı', NULL), (3, 15, N'Yapımcı', NULL),
 (3, 12, N'Oyuncu', N'Bruce Wayne / Batman'), (3, 13, N'Oyuncu', N'Joker'), (3, 14, N'Oyuncu', N'James Gordon'),
 -- 4 Başlangıç
 (4, 10, N'Yönetmen', NULL), (4, 10, N'Senarist', NULL), (4, 10, N'Yapımcı', NULL), (4, 11, N'Yapımcı', NULL),
 (4, 16, N'Oyuncu', N'Dom Cobb'), (4, 17, N'Oyuncu', N'Arthur'),
 -- 5 Yıldızlararası
 (5, 10, N'Yönetmen', NULL), (5, 10, N'Yapımcı', NULL), (5, 11, N'Yapımcı', NULL),
 (5, 18, N'Oyuncu', N'Cooper'), (5, 19, N'Oyuncu', N'Brand'), (5, 20, N'Oyuncu', N'Murph'),
 -- 6 Parazit
 (6, 21, N'Yönetmen', NULL), (6, 21, N'Senarist', NULL), (6, 23, N'Yapımcı', NULL),
 (6, 22, N'Oyuncu', N'Kim Ki-taek'),
 -- 7 Ayla
 (7, 24, N'Yönetmen', NULL),
 (7, 25, N'Oyuncu', N'Süleyman Dilbirliği'),
 -- 8 Babam ve Oğlum
 (8, 26, N'Yönetmen', NULL), (8, 26, N'Senarist', NULL),
 (8, 27, N'Oyuncu', N'Sadık'), (8, 28, N'Oyuncu', N'Hüseyin'),
 -- 9 Kış Uykusu
 (9, 29, N'Yönetmen', NULL), (9, 33, N'Yapımcı', NULL),
 (9, 30, N'Oyuncu', N'Aydın'), (9, 31, N'Oyuncu', N'Nihal'), (9, 32, N'Oyuncu', N'Necla'),
 -- 10 Matrix
 (10, 34, N'Yönetmen', NULL), (10, 35, N'Yönetmen', NULL), (10, 39, N'Yapımcı', NULL),
 (10, 36, N'Oyuncu', N'Neo'), (10, 37, N'Oyuncu', N'Morpheus'), (10, 38, N'Oyuncu', N'Trinity'),
 -- 11 Breaking Bad
 (11, 40, N'Yönetmen', NULL), (11, 40, N'Senarist', NULL), (11, 40, N'Yapımcı', NULL),
 (11, 41, N'Oyuncu', N'Walter White'), (11, 42, N'Oyuncu', N'Jesse Pinkman'), (11, 43, N'Oyuncu', N'Skyler White'),
 -- 12 Stranger Things
 (12, 44, N'Yönetmen', NULL), (12, 45, N'Yönetmen', NULL), (12, 46, N'Yapımcı', NULL),
 (12, 47, N'Oyuncu', N'Eleven'), (12, 48, N'Oyuncu', N'Joyce Byers'), (12, 49, N'Oyuncu', N'Jim Hopper'),
 -- 13 Dark
 (13, 50, N'Yönetmen', NULL), (13, 51, N'Senarist', NULL),
 (13, 52, N'Oyuncu', N'Jonas Kahnwald'),
 -- 14 Ezel
 (14, 53, N'Yönetmen', NULL), (14, 54, N'Yapımcı', NULL),
 (14, 55, N'Oyuncu', N'Ezel Bayraktar'), (14, 56, N'Oyuncu', N'Eyşan'), (14, 57, N'Oyuncu', N'Ramiz Dayı'),
 -- 15 Leyla ile Mecnun
 (15, 58, N'Yönetmen', NULL),
 (15, 59, N'Oyuncu', N'Mecnun'), (15, 60, N'Oyuncu', N'İsmail'), (15, 61, N'Oyuncu', N'Erdal Bakkal');
GO

PRINT N'Örnek veriler eklendi.';
GO
