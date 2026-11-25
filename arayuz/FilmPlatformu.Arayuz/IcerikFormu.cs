using System.Data;

namespace FilmPlatformu.Arayuz;

/// <summary>
/// Film / dizi ekleme ve düzenleme penceresi. İkisi için aynı formu kullanıyoruz:
/// ID gelmezse yeni kayıt (sp_IcerikEkle), ID gelirse düzenleme (sp_IcerikGuncelle).
/// </summary>
public class IcerikFormu : DiyalogFormu
{
    private readonly int? _icerikID;

    private readonly TextBox _baslik = new();
    private readonly TextBox _orijinal = new() { PlaceholderText = "(isteğe bağlı)" };
    private readonly ComboBox _tur = Ui.Secim();
    private readonly NumericUpDown _yil = new() { Minimum = 1888, Maximum = 2100, Value = DateTime.Now.Year };
    private readonly TextBox _ulke = new() { Text = "Türkiye" };
    private readonly TextBox _dil = new() { Text = "Türkçe" };
    private readonly ComboBox _yas = Ui.Secim();
    private readonly NumericUpDown _imdb = new() { Minimum = 0, Maximum = 10, DecimalPlaces = 1, Increment = 0.1m };
    private readonly TextBox _sirket = new() { PlaceholderText = "(isteğe bağlı)" };
    private readonly NumericUpDown _sure = new() { Minimum = 1, Maximum = 1000, Value = 120 };
    private readonly Label _sureMetni = new() { AutoSize = true, ForeColor = Ui.SolukYazi, Margin = new Padding(8, 7, 3, 3) };
    private readonly NumericUpDown _bitisYili = new() { Minimum = 0, Maximum = 2100, Value = 0 };
    private readonly ComboBox _durum = Ui.Secim();
    private readonly CheckedListBox _kategoriler = new() { CheckOnClick = true, MultiColumn = true, ColumnWidth = 130, BorderStyle = BorderStyle.FixedSingle };
    private readonly TextBox _konu = new() { Multiline = true, ScrollBars = ScrollBars.Vertical };
    private readonly PictureBox _afisKutusu = new() { Size = new Size(80, 120), SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle };

    private byte[]? _yeniAfis;      // kullanıcının yeni seçtiği resim
    private object? _mevcutAfis;    // düzenlerken veritabanında zaten olan resim
    private bool _afisKaldir;

    public int? KaydedilenID { get; private set; }

    public IcerikFormu(int? icerikID = null)
        : base(icerikID == null ? "Yeni Film / Dizi Ekle" : "Film / Dizi Düzenle", 580, 830)
    {
        _icerikID = icerikID;

        _tur.Items.AddRange(new object[] { "Film", "Dizi" });
        _tur.SelectedIndex = 0;
        _yas.Items.AddRange(new object[] { "0", "7", "13", "16", "18" });
        _yas.SelectedIndex = 0;
        _durum.Items.AddRange(new object[] { "Devam Ediyor", "Tamamlandı", "İptal" });
        _durum.SelectedIndex = 0;

        var surePaneli = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
        _sure.Width = 90;
        surePaneli.Controls.Add(_sure);
        surePaneli.Controls.Add(_sureMetni);
        _sure.ValueChanged += (_, _) => SureMetniniGuncelle();
        SureMetniniGuncelle();

        Satir("Tür:", _tur);
        Satir("Başlık: *", _baslik);
        Satir("Orijinal başlık:", _orijinal);
        Satir("Yayın yılı: *", _yil);
        Satir("Ülke: *", _ulke);
        Satir("Dil: *", _dil);
        Satir("Yaş sınırı:", _yas);
        Satir("IMDb puanı:", _imdb);
        Satir("Yapım şirketi:", _sirket);
        Satir("Süre (dakika):", surePaneli);
        Satir("Bitiş yılı:", _bitisYili);
        Satir("Dizi durumu:", _durum);
        Satir("Kategoriler:", _kategoriler, 110);
        Satir("Konu:", _konu, 110);

        // Afiş kısmı: küçük önizleme, yanında Görsel Seç ve Kaldır butonları
        var afisSec = Ui.Dugme("Görsel Seç...", ana: false);
        var afisKaldir = Ui.Dugme("Kaldır", ana: false);
        var afisDugmeler = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
        afisDugmeler.Controls.Add(afisSec);
        afisDugmeler.Controls.Add(afisKaldir);
        afisDugmeler.Controls.Add(new Label { Text = "JPG / PNG, dikey (2:3)\nboşsa otomatik afiş çizilir", AutoSize = true, ForeColor = Ui.SolukYazi });
        var afisPaneli = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
        afisPaneli.Controls.Add(_afisKutusu);
        afisPaneli.Controls.Add(afisDugmeler);
        Satir("Afiş:", afisPaneli, 140);
        afisSec.Click += (_, _) => AfisSec();
        afisKaldir.Click += (_, _) => { _yeniAfis = null; _afisKaldir = true; AfisOnizle(); };
        _baslik.TextChanged += (_, _) => { if (_yeniAfis == null && (_mevcutAfis == null || _afisKaldir)) AfisOnizle(); };
        Tablo.Controls.Add(new Label
        {
            Text = "IMDb 0 ise boş kabul edilir. Bitiş yılı 0 ise boş kabul edilir (devam eden diziler).",
            AutoSize = true,
            ForeColor = Ui.SolukYazi
        });
        Tablo.SetColumnSpan(Tablo.Controls[^1], 2);

        _tur.SelectedIndexChanged += (_, _) => TureGoreAyarla();

        // Kategorileri elle yazmadık, veritabanından çekiyoruz (yeni kategori eklenirse burada da çıksın)
        var kategoriler = Veritabani.Sorgu("SELECT KategoriID, KategoriAdi FROM Kategoriler ORDER BY KategoriAdi");
        foreach (DataRow r in kategoriler.Rows)
            _kategoriler.Items.Add(new Secenek((int)r["KategoriID"], (string)r["KategoriAdi"]));

        if (icerikID != null) Yukle(icerikID.Value);
        TureGoreAyarla();
        AfisOnizle();
    }

    private void AfisSec()
    {
        using var dlg = new OpenFileDialog { Filter = "Görseller|*.jpg;*.jpeg;*.png;*.bmp;*.webp|Tüm dosyalar|*.*", Title = "Afiş görseli seçin" };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            _yeniAfis = Afisler.DosyadanHazirla(dlg.FileName);
            _afisKaldir = false;
            AfisOnizle();
        }
        catch (Exception ex)
        {
            Ui.Hata(new Exception("Görsel okunamadı: " + ex.Message));
        }
    }

    private void AfisOnizle()
    {
        object? veri = _yeniAfis ?? (_afisKaldir ? null : _mevcutAfis);
        using var gorsel = Afisler.Yukle(veri);
        var eski = _afisKutusu.Image;
        _afisKutusu.Image = Afisler.Ciz(gorsel, string.IsNullOrWhiteSpace(_baslik.Text) ? "Yeni" : _baslik.Text,
                                        _tur.SelectedIndex == 1 ? "Dizi" : "Film", 160, 240);
        eski?.Dispose();
    }

    private sealed record Secenek(int ID, string Ad)
    {
        public override string ToString() => Ad;
    }

    private void SureMetniniGuncelle()
    {
        int dk = (int)_sure.Value;
        _sureMetni.Text = dk >= 60 ? $"= {dk / 60} saat {dk % 60} dakika" : $"= {dk} dakika";
    }

    private void TureGoreAyarla()
    {
        bool film = _tur.SelectedIndex == 0;
        _sure.Enabled = film;
        _bitisYili.Enabled = !film;
        _durum.Enabled = !film;
    }

    private void Yukle(int id)
    {
        var t = Veritabani.Sorgu(@"
            SELECT i.*, f.SureDakika, d.BitisYili, d.Durum
            FROM Icerikler AS i
            LEFT JOIN Filmler AS f ON f.IcerikID = i.IcerikID
            LEFT JOIN Diziler AS d ON d.IcerikID = i.IcerikID
            WHERE i.IcerikID = @id", Veritabani.P("@id", id));
        if (t.Rows.Count == 0) throw new Exception("İçerik bulunamadı.");
        var r = t.Rows[0];

        _tur.SelectedItem = (string)r["IcerikTuru"];
        _tur.Enabled = false;   // filmi sonradan diziye çevirmek mantıklı olmadığı için türü kilitledik
        _baslik.Text = (string)r["Baslik"];
        _orijinal.Text = r["OrijinalBaslik"] as string ?? "";
        _yil.Value = Convert.ToDecimal(r["YayinYili"]);
        _ulke.Text = (string)r["Ulke"];
        _dil.Text = (string)r["Dil"];
        _yas.SelectedItem = r["YasSiniri"].ToString();
        _imdb.Value = r["ImdbPuani"] is DBNull ? 0 : Convert.ToDecimal(r["ImdbPuani"]);
        _sirket.Text = r["YapimSirketi"] as string ?? "";
        _konu.Text = r["Konu"] as string ?? "";
        _mevcutAfis = r["Afis"] is DBNull ? null : r["Afis"];
        if (r["SureDakika"] is not DBNull) _sure.Value = Convert.ToDecimal(r["SureDakika"]);
        if (r["BitisYili"] is not DBNull) _bitisYili.Value = Convert.ToDecimal(r["BitisYili"]);
        if (r["Durum"] is string durum) _durum.SelectedItem = durum;

        var secili = Veritabani.Sorgu("SELECT KategoriID FROM IcerikKategorileri WHERE IcerikID = @id", Veritabani.P("@id", id))
                               .AsEnumerable().Select(x => (int)x["KategoriID"]).ToHashSet();
        for (int i = 0; i < _kategoriler.Items.Count; i++)
            _kategoriler.SetItemChecked(i, secili.Contains(((Secenek)_kategoriler.Items[i]).ID));
    }

    protected override bool Kaydet()
    {
        if (!Zorunlu(("Başlık", _baslik), ("Ülke", _ulke), ("Dil", _dil))) return false;

        bool film = _tur.SelectedIndex == 0;
        var kategoriler = string.Join(",", _kategoriler.CheckedItems.Cast<Secenek>().Select(s => s.ID));

        var parametreler = new List<Microsoft.Data.SqlClient.SqlParameter>
        {
            Veritabani.P("@Baslik", _baslik.Text.Trim()),
            Veritabani.P("@OrijinalBaslik", BosIseNull(_orijinal.Text)),
            Veritabani.P("@YayinYili", (short)_yil.Value),
            Veritabani.P("@Ulke", _ulke.Text.Trim()),
            Veritabani.P("@Dil", _dil.Text.Trim()),
            Veritabani.P("@YasSiniri", byte.Parse((string)_yas.SelectedItem!)),
            Veritabani.P("@ImdbPuani", _imdb.Value > 0 ? _imdb.Value : null),
            Veritabani.P("@YapimSirketi", BosIseNull(_sirket.Text)),
            Veritabani.P("@Konu", BosIseNull(_konu.Text)),
            Veritabani.P("@SureDakika", film ? (short)_sure.Value : null),
            Veritabani.P("@BitisYili", !film && _bitisYili.Value > 0 ? (short)_bitisYili.Value : null),
            Veritabani.P("@DiziDurumu", film ? null : _durum.SelectedItem?.ToString()),
            Veritabani.P("@KategoriIDler", kategoriler.Length > 0 ? kategoriler : null),
            new Microsoft.Data.SqlClient.SqlParameter("@Afis", SqlDbType.VarBinary, -1) { Value = (object?)_yeniAfis ?? DBNull.Value }
        };
        if (_icerikID != null) parametreler.Add(Veritabani.P("@AfisKaldir", _afisKaldir));

        if (_icerikID == null)
        {
            parametreler.Add(Veritabani.P("@IcerikTuru", film ? "Film" : "Dizi"));
            var sonuc = Veritabani.Yordam("sp_IcerikEkle", parametreler.ToArray());
            KaydedilenID = Convert.ToInt32(sonuc.Rows[0]["YeniIcerikID"]);
        }
        else
        {
            parametreler.Add(Veritabani.P("@IcerikID", _icerikID.Value));
            Veritabani.Yordam("sp_IcerikGuncelle", parametreler.ToArray());
            KaydedilenID = _icerikID;
        }
        return true;
    }
}
