using System.Data;

namespace FilmPlatformu.Arayuz;

/// <summary>
/// "Filmler ve Diziler" sekmesi. Ekranı üçe böldük:
///   Sol  : filtreler (bir şey değiştirince liste hemen güncelleniyor)
///   Orta : film ve dizilerin afiş kartları
///   Sağ  : seçilen yapımın detayları, oyuncuları ve bölümleri
/// Ekleme, düzenleme ve silme işlemleri de bu dosyada.
/// </summary>
public partial class AnaForm
{
    // ---- Soldaki filtre paneli ----
    private readonly TextBox _fAra = new() { PlaceholderText = "Başlık, oyuncu, yönetmen..." };
    private readonly RadioButton _fTumu = new() { Text = "Tümü", AutoSize = true, Checked = true };
    private readonly RadioButton _fFilm = new() { Text = "Film", AutoSize = true };
    private readonly RadioButton _fDizi = new() { Text = "Dizi", AutoSize = true };
    private readonly CheckedListBox _fKategoriler = new() { CheckOnClick = true, BorderStyle = BorderStyle.None, IntegralHeight = false };
    private readonly ComboBox _fUlke = Ui.Secim();
    private readonly NumericUpDown _fYilMin = new() { Minimum = 1888, Maximum = 2100, Width = 72 };
    private readonly NumericUpDown _fYilMax = new() { Minimum = 1888, Maximum = 2100, Width = 72 };
    private readonly TrackBar _fImdb = new() { Minimum = 0, Maximum = 100, TickFrequency = 10, SmallChange = 1, LargeChange = 5, AutoSize = false, Height = 32 };
    private readonly Label _fImdbEtiket = new() { AutoSize = true };
    private readonly ComboBox _fSiralama = Ui.Secim();
    private readonly System.Windows.Forms.Timer _aramaZamanlayici = new() { Interval = 300 };
    private bool _filtreYukleniyor;

    // ---- Ortadaki afiş kartları ----
    private readonly FlowLayoutPanel _kartlar = new() { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(8), BackColor = Ui.Zemin };
    private readonly Label _sonucSayisi = new() { AutoSize = true, ForeColor = Ui.SolukYazi, Margin = new Padding(12, 10, 3, 0) };
    private readonly Button _yeniDugmesi = Ui.Dugme("+ Yeni Film / Dizi");
    private readonly Button _duzenleDugmesi = Ui.Dugme("Düzenle", ana: false);
    private readonly Button _icerikSilDugmesi = Ui.Dugme("Sil", ana: false);

    // ---- Sağdaki detay paneli ----
    private readonly PictureBox _detayAfis = new() { Dock = DockStyle.Left, Width = 140, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.White };
    private readonly RichTextBox _detay = new() { Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.None, BackColor = Color.White, DetectUrls = false };
    private readonly DataGridView _ekipIzgara = Ui.Izgara();
    private readonly DataGridView _bolumIzgara = Ui.Izgara();
    private readonly TabControl _altSekmeler = new() { Dock = DockStyle.Fill };
    private readonly TabPage _ekipSekmesi = new("Oyuncular ve Ekip");
    private readonly TabPage _bolumSekmesi = new("Bölümler");
    private readonly Button _kisiEkleDugmesi = Ui.Dugme("+ Kişi Ekle", ana: false);
    private readonly Button _kisiCikarDugmesi = Ui.Dugme("Seçiliyi Çıkar", ana: false);
    private readonly Button _bolumEkleDugmesi = Ui.Dugme("+ Bölüm Ekle", ana: false);
    private readonly Button _bolumSilDugmesi = Ui.Dugme("Seçili Bölümü Sil", ana: false);

    private IcerikKarti? _seciliKart;

    private int? SeciliIcerikID => _seciliKart?.IcerikID;
    private string SeciliIcerikAdi => _seciliKart?.Baslik ?? "";
    private bool _seciliDizi;

    private TabPage KatalogSekmesiOlustur()
    {
        var sekme = new TabPage("Filmler ve Diziler") { BackColor = Ui.Zemin };

        // ========== SOL TARAF: Filtreler ==========
        var filtre = new Panel { Dock = DockStyle.Left, Width = 235, AutoScroll = true, BackColor = Color.White, Padding = new Padding(14, 10, 14, 10) };
        var liste = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 1, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
        liste.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        void Baslik(string metin) => liste.Controls.Add(new Label
        {
            Text = metin,
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 9.5f),
            ForeColor = Ui.Vurgu,
            Margin = new Padding(0, 12, 0, 3)
        });
        void Ekle(Control c) { c.Dock = DockStyle.Top; c.Margin = new Padding(0, 0, 0, 2); liste.Controls.Add(c); }

        liste.Controls.Add(new Label { Text = "Filtreler", AutoSize = true, Font = new Font("Segoe UI Semibold", 13f), Margin = new Padding(0, 0, 0, 2) });
        Baslik("Ara");
        Ekle(_fAra);
        Baslik("Yapım türü");
        var turler = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
        turler.Controls.AddRange(new Control[] { _fTumu, _fFilm, _fDizi });
        liste.Controls.Add(turler);
        Baslik("Türler");
        _fKategoriler.Height = 215;
        Ekle(_fKategoriler);
        Baslik("Ülke");
        Ekle(_fUlke);
        Baslik("Yıl aralığı");
        var yillar = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
        yillar.Controls.AddRange(new Control[] { _fYilMin, new Label { Text = "–", AutoSize = true, Margin = new Padding(4, 6, 4, 0) }, _fYilMax });
        liste.Controls.Add(yillar);
        Baslik("En düşük IMDb puanı");
        Ekle(_fImdbEtiket);
        Ekle(_fImdb);
        Baslik("Sıralama");
        _fSiralama.Items.AddRange(new object[] { "IMDb puanı (yüksek → düşük)", "Yıl (yeni → eski)", "Yıl (eski → yeni)", "Ada göre (A → Z)" });
        _fSiralama.SelectedIndex = 0;
        Ekle(_fSiralama);
        var temizle = Ui.Dugme("Filtreleri Temizle", ana: false);
        temizle.Margin = new Padding(0, 16, 0, 0);
        liste.Controls.Add(temizle);
        filtre.Controls.Add(liste);

        // ========== ORTA: Afiş kartları ==========
        var orta = new Panel { Dock = DockStyle.Fill };
        var arac = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 46, Padding = new Padding(8, 6, 8, 2), WrapContents = false, BackColor = Ui.Zemin };
        arac.Controls.AddRange(new Control[] { _yeniDugmesi, _duzenleDugmesi, _icerikSilDugmesi, _sonucSayisi });
        orta.Controls.Add(_kartlar);
        orta.Controls.Add(arac);

        // ========== SAĞ TARAF: Detay ==========
        var sag = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, BackColor = Ui.Zemin };
        var ustDetay = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(12) };
        var araBosluk = new Panel { Dock = DockStyle.Left, Width = 14, BackColor = Color.White };
        ustDetay.Controls.Add(_detay);
        ustDetay.Controls.Add(araBosluk);
        ustDetay.Controls.Add(_detayAfis);
        sag.Panel1.Padding = new Padding(0, 8, 8, 4);
        sag.Panel1.Controls.Add(ustDetay);

        _ekipSekmesi.Controls.Add(_ekipIzgara);
        _ekipSekmesi.Controls.Add(DugmeSatiri(_kisiEkleDugmesi, _kisiCikarDugmesi));
        _bolumSekmesi.Controls.Add(_bolumIzgara);
        _bolumSekmesi.Controls.Add(DugmeSatiri(_bolumEkleDugmesi, _bolumSilDugmesi));
        _altSekmeler.TabPages.Add(_ekipSekmesi);
        _altSekmeler.TabPages.Add(_bolumSekmesi);
        sag.Panel2.Padding = new Padding(0, 4, 8, 8);
        sag.Panel2.Controls.Add(_altSekmeler);

        var bolme = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel2, BackColor = Ui.Zemin };
        bolme.Panel1.Controls.Add(orta);
        bolme.Panel2.Controls.Add(sag);

        sekme.Controls.Add(bolme);
        sekme.Controls.Add(new Panel { Dock = DockStyle.Left, Width = 1, BackColor = Color.FromArgb(226, 232, 240) });
        sekme.Controls.Add(filtre);

        bool ayarlandi = false;
        sekme.Layout += (_, _) =>
        {
            if (ayarlandi || bolme.Width <= 700 || sag.Height <= 300) return;
            bolme.SplitterDistance = Math.Max(420, bolme.Width - 430);
            sag.SplitterDistance = (int)(sag.Height * 0.55);
            ayarlandi = true;
        };

        // ========== Butonlara ve filtrelere tıklayınca ne olacağı ==========
        _aramaZamanlayici.Tick += (_, _) => { _aramaZamanlayici.Stop(); Guvenli(() => KatalogYenile()); };
        _fAra.TextChanged += (_, _) => { _aramaZamanlayici.Stop(); _aramaZamanlayici.Start(); };
        foreach (var r in new[] { _fTumu, _fFilm, _fDizi })
            r.CheckedChanged += (_, _) => { if (((RadioButton)r).Checked) FiltreDegisti(); };
        _fKategoriler.ItemCheck += (_, _) => BeginInvoke(FiltreDegisti);   // ItemCheck tik değişmeden önce çalıştığı için BeginInvoke ile biraz sonraya erteledik
        _fUlke.SelectedIndexChanged += (_, _) => FiltreDegisti();
        _fYilMin.ValueChanged += (_, _) => FiltreDegisti();
        _fYilMax.ValueChanged += (_, _) => FiltreDegisti();
        _fImdb.ValueChanged += (_, _) => { ImdbEtiketiniGuncelle(); FiltreDegisti(); };
        _fSiralama.SelectedIndexChanged += (_, _) => FiltreDegisti();
        temizle.Click += (_, _) => Guvenli(FiltreleriTemizle);

        _yeniDugmesi.Click += (_, _) => Guvenli(YeniIcerik);
        _duzenleDugmesi.Click += (_, _) => Guvenli(IcerikDuzenle);
        _icerikSilDugmesi.Click += (_, _) => Guvenli(IcerikSil);
        _kisiEkleDugmesi.Click += (_, _) => Guvenli(KisiEkle);
        _kisiCikarDugmesi.Click += (_, _) => Guvenli(KisiCikar);
        _bolumEkleDugmesi.Click += (_, _) => Guvenli(BolumEkle);
        _bolumSilDugmesi.Click += (_, _) => Guvenli(BolumSil);
        return sekme;
    }

    private static FlowLayoutPanel DugmeSatiri(params Button[] dugmeler)
    {
        var p = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(2), WrapContents = false };
        p.Controls.AddRange(dugmeler);
        return p;
    }

    private void FiltreDegisti()
    {
        if (!_filtreYukleniyor) Guvenli(() => KatalogYenile());
    }

    private void ImdbEtiketiniGuncelle() =>
        _fImdbEtiket.Text = _fImdb.Value == 0 ? "Hepsi" : $"★ {_fImdb.Value / 10m:0.0} ve üzeri";

    /// <summary>Filtrelerdeki seçenekleri (türler, ülkeler, yıllar) veritabanından çekip dolduruyor.</summary>
    private void KatalogHazirla()
    {
        _filtreYukleniyor = true;

        var secilenler = _fKategoriler.CheckedItems.Cast<Secenek>().Select(s => s.ID).ToHashSet();
        _fKategoriler.Items.Clear();
        foreach (DataRow r in Veritabani.Sorgu("SELECT KategoriID, KategoriAdi FROM Kategoriler ORDER BY KategoriAdi").Rows)
        {
            var s = new Secenek((int)r["KategoriID"], (string)r["KategoriAdi"]);
            _fKategoriler.Items.Add(s, secilenler.Contains(s.ID));
        }

        var seciliUlke = _fUlke.SelectedItem as string;
        _fUlke.Items.Clear();
        _fUlke.Items.Add("(Tüm ülkeler)");
        foreach (DataRow r in Veritabani.Sorgu("SELECT DISTINCT Ulke FROM Icerikler ORDER BY Ulke").Rows)
            _fUlke.Items.Add((string)r["Ulke"]);
        _fUlke.SelectedItem = seciliUlke != null && _fUlke.Items.Contains(seciliUlke) ? seciliUlke : "(Tüm ülkeler)";

        var yil = Veritabani.Sorgu("SELECT ISNULL(MIN(YayinYili), 1900) AS EnEski, ISNULL(MAX(YayinYili), 2025) AS EnYeni FROM Icerikler").Rows[0];
        bool ilkKez = _fYilMin.Tag == null;
        _fYilMin.Tag = Convert.ToDecimal(yil["EnEski"]);
        _fYilMax.Tag = Convert.ToDecimal(yil["EnYeni"]);
        if (ilkKez)
        {
            _fYilMin.Value = (decimal)_fYilMin.Tag;
            _fYilMax.Value = (decimal)_fYilMax.Tag;
        }
        ImdbEtiketiniGuncelle();

        _filtreYukleniyor = false;
        KatalogYenile();
    }

    private void FiltreleriTemizle()
    {
        _filtreYukleniyor = true;
        _fAra.Clear();
        _aramaZamanlayici.Stop();
        _fTumu.Checked = true;
        for (int i = 0; i < _fKategoriler.Items.Count; i++) _fKategoriler.SetItemChecked(i, false);
        _fUlke.SelectedIndex = 0;
        _fYilMin.Value = _fYilMin.Tag as decimal? ?? _fYilMin.Minimum;
        _fYilMax.Value = _fYilMax.Tag as decimal? ?? _fYilMax.Maximum;
        _fImdb.Value = 0;
        _fSiralama.SelectedIndex = 0;
        ImdbEtiketiniGuncelle();
        _filtreYukleniyor = false;
        KatalogYenile();
    }

    private sealed record Secenek(int ID, string Ad)
    {
        public override string ToString() => Ad;
    }

    /// <summary>
    /// Filtreleri sp_IcerikAra yordamına gönderip gelen sonuçlara göre kartları baştan oluşturuyor.
    /// Daha önce seçili olan film hâlâ listedeyse onu seçili bırakıyor.
    /// </summary>
    private void KatalogYenile(int? secilecekID = null)
    {
        secilecekID ??= SeciliIcerikID;

        var kategoriler = string.Join(",", _fKategoriler.CheckedItems.Cast<Secenek>().Select(s => s.ID));
        object? tur = _fFilm.Checked ? "Film" : _fDizi.Checked ? "Dizi" : null;
        object? ulke = _fUlke.SelectedIndex > 0 ? _fUlke.SelectedItem : null;
        object? yilMin = _fYilMin.Tag is decimal enEski && _fYilMin.Value > enEski ? (short)_fYilMin.Value : null;
        object? yilMax = _fYilMax.Tag is decimal enYeni && _fYilMax.Value < enYeni ? (short)_fYilMax.Value : null;
        object? minImdb = _fImdb.Value > 0 ? _fImdb.Value / 10m : null;
        string siralama = _fSiralama.SelectedIndex switch { 1 => "YeniEski", 2 => "EskiYeni", 3 => "Ad", _ => "IMDb" };

        var sonuc = Veritabani.Yordam("sp_IcerikAra",
            Veritabani.P("@Aranan", string.IsNullOrWhiteSpace(_fAra.Text) ? null : _fAra.Text.Trim()),
            Veritabani.P("@KategoriIDler", kategoriler.Length > 0 ? kategoriler : null),
            Veritabani.P("@IcerikTuru", tur),
            Veritabani.P("@Ulke", ulke),
            Veritabani.P("@YilMin", yilMin),
            Veritabani.P("@YilMax", yilMax),
            Veritabani.P("@MinImdb", minImdb),
            Veritabani.P("@Siralama", siralama));

        _kartlar.SuspendLayout();
        var eskiler = _kartlar.Controls.Cast<Control>().ToList();
        _kartlar.Controls.Clear();
        foreach (var c in eskiler) c.Dispose();
        _seciliKart = null;

        IcerikKarti? secilecek = null;
        foreach (DataRow r in sonuc.Rows)
        {
            var kart = new IcerikKarti(
                (int)r["IcerikID"], (string)r["Baslik"], (string)r["IcerikTuru"], Convert.ToInt32(r["YayinYili"]),
                r["Sure"] as string ?? "", r["ImdbPuani"] as decimal?, r["Yonetmenler"] as string,
                r["Kategoriler"] as string, r["Afis"]);
            kart.Click += (_, _) => Guvenli(() => KartSec(kart));
            kart.DoubleClick += (_, _) => Guvenli(IcerikDuzenle);
            _kartlar.Controls.Add(kart);
            if (kart.IcerikID == secilecekID) secilecek = kart;
        }
        _kartlar.ResumeLayout();

        _sonucSayisi.Text = sonuc.Rows.Count == 0
            ? "Filtrelere uygun yapım bulunamadı."
            : $"{sonuc.Rows.Count} yapım  ({sonuc.AsEnumerable().Count(r => (string)r["IcerikTuru"] == "Film")} film, " +
              $"{sonuc.AsEnumerable().Count(r => (string)r["IcerikTuru"] == "Dizi")} dizi)";

        _kartlar.AutoScrollPosition = new Point(0, 0);
        KartSec(secilecek ?? _kartlar.Controls.OfType<IcerikKarti>().FirstOrDefault(), kaydir: secilecek != null);
    }

    private void KartSec(IcerikKarti? kart, bool kaydir = false)
    {
        if (_seciliKart != null) _seciliKart.Secili = false;
        _seciliKart = kart;
        if (kart != null)
        {
            kart.Secili = true;
            if (kaydir) _kartlar.ScrollControlIntoView(kart);
        }
        DetayiGoster();
    }

    /// <summary>Seçilen yapımın bilgilerini sağ tarafa yazıyor. vw_IcerikDetaylari view'ını kullanıyor.</summary>
    private void DetayiGoster()
    {
        bool secim = SeciliIcerikID != null;
        _duzenleDugmesi.Enabled = _icerikSilDugmesi.Enabled = secim;
        _kisiEkleDugmesi.Enabled = _kisiCikarDugmesi.Enabled = secim;

        _detayAfis.Image?.Dispose();
        _detayAfis.Image = null;

        if (SeciliIcerikID is not int id)
        {
            _seciliDizi = false;
            _bolumEkleDugmesi.Enabled = _bolumSilDugmesi.Enabled = false;
            _detay.Clear();
            Yaz("Bir afişe tıklayarak detaylarını görün.", 10.5f, FontStyle.Regular, Ui.SolukYazi);
            _ekipIzgara.DataSource = null;
            _bolumIzgara.DataSource = null;
            return;
        }

        var t = Veritabani.Sorgu(
            "SELECT v.*, i.Afis FROM vw_IcerikDetaylari AS v JOIN Icerikler AS i ON i.IcerikID = v.IcerikID WHERE v.IcerikID = @id",
            Veritabani.P("@id", id));
        if (t.Rows.Count == 0) return;
        var r = t.Rows[0];
        _seciliDizi = (string)r["IcerikTuru"] == "Dizi";
        _bolumEkleDugmesi.Enabled = _bolumSilDugmesi.Enabled = _seciliDizi;
        string D(string kolon) => r[kolon] is DBNull ? "-" : r[kolon].ToString()!;

        using (var gorsel = Afisler.Yukle(r["Afis"]))
            _detayAfis.Image = Afisler.Ciz(gorsel, (string)r["Baslik"], (string)r["IcerikTuru"], 280, 420);

        _detay.Clear();
        Yaz(D("Baslik"), 15f, FontStyle.Bold, Color.FromArgb(17, 24, 39));
        if (r["OrijinalBaslik"] is string orj) Yaz($"\n{orj}", 10f, FontStyle.Italic, Ui.SolukYazi);
        var yillar = _seciliDizi && r["BitisYili"] is not DBNull ? $"{r["YayinYili"]}–{r["BitisYili"]}" : r["YayinYili"].ToString();
        Yaz($"\n{r["IcerikTuru"]}  •  {yillar}  •  {D("Ulke")}  •  {D("Dil")}  •  {r["YasSiniri"]}+\n", 9.5f, FontStyle.Regular, Ui.SolukYazi);

        Yaz("\n★ ", 13f, FontStyle.Bold, Color.FromArgb(202, 138, 4));
        Yaz(r["ImdbPuani"] is DBNull ? "-" : $"{r["ImdbPuani"]}", 13f, FontStyle.Bold, Color.Black);
        Yaz(" / 10  IMDb\n", 9.5f, FontStyle.Regular, Ui.SolukYazi);

        if (_seciliDizi)
        {
            var toplam = Convert.ToInt32(Veritabani.Sorgu("SELECT dbo.fn_DiziToplamSure(@id) AS T", Veritabani.P("@id", id)).Rows[0]["T"]);
            Alan("Süre", $"{D("Sure")} (toplam {SaatMetni(toplam)}) • {D("DiziDurumu")}");
        }
        else
        {
            Alan("Süre", D("Sure"));
        }
        Alan("Türler", D("Kategoriler"));
        Alan("Yönetmen", D("Yonetmenler"));
        Alan("Yapımcı", D("Yapimcilar"));
        Alan("Yapım şirketi", D("YapimSirketi"));
        Alan("Oyuncular", D("Oyuncular"));
        Yaz("\n\nKonu\n", 10f, FontStyle.Bold, Ui.Vurgu);
        Yaz(D("Konu"), 10f, FontStyle.Regular, Color.FromArgb(31, 41, 55));
        _detay.SelectionStart = 0;
        _detay.ScrollToCaret();

        _ekipIzgara.DataSource = Veritabani.Sorgu(
            "SELECT KisiID, Gorev, Kisi, KarakterAdi FROM vw_OyuncuVeEkip WHERE IcerikID = @id " +
            "ORDER BY CASE Gorev WHEN N'Yönetmen' THEN 0 WHEN N'Yapımcı' THEN 1 WHEN N'Senarist' THEN 2 ELSE 3 END, Kisi",
            Veritabani.P("@id", id));
        if (_ekipIzgara.Columns["KisiID"] is { } k) k.Visible = false;
        BaslikAyarla(_ekipIzgara, ("Gorev", "Görev"), ("Kisi", "Kişi"), ("KarakterAdi", "Karakter"));

        if (_seciliDizi)
        {
            _bolumSekmesi.Text = $"Bölümler ({D("BolumSayisi")})";
            _bolumIzgara.DataSource = Veritabani.Sorgu(
                "SELECT BolumID, BolumKodu, BolumAdi, Sure FROM vw_DiziBolumleri WHERE DiziID = @id ORDER BY SezonNo, BolumNo",
                Veritabani.P("@id", id));
            if (_bolumIzgara.Columns["BolumID"] is { } b) b.Visible = false;
            BaslikAyarla(_bolumIzgara, ("BolumKodu", "Bölüm"), ("BolumAdi", "Bölüm Adı"), ("Sure", "Süre"));
        }
        else
        {
            _bolumSekmesi.Text = "Bölümler (sadece dizi)";
            _bolumIzgara.DataSource = null;
            if (_altSekmeler.SelectedTab == _bolumSekmesi) _altSekmeler.SelectedTab = _ekipSekmesi;
        }
    }

    private static void BaslikAyarla(DataGridView izgara, params (string Kolon, string Baslik)[] basliklar)
    {
        foreach (var (kolon, baslik) in basliklar)
            if (izgara.Columns[kolon] is { } c) c.HeaderText = baslik;
    }

    private static string SaatMetni(int dk) => dk >= 60 ? $"{dk / 60} sa {dk % 60} dk" : $"{dk} dk";

    private void Alan(string ad, string deger)
    {
        Yaz($"\n{ad}: ", 10f, FontStyle.Bold, Color.FromArgb(55, 65, 81));
        Yaz(deger, 10f, FontStyle.Regular, Color.FromArgb(31, 41, 55));
    }

    private void Yaz(string metin, float boyut, FontStyle stil, Color renk)
    {
        _detay.SelectionStart = _detay.TextLength;
        _detay.SelectionLength = 0;
        _detay.SelectionFont = new Font("Segoe UI", boyut, stil);
        _detay.SelectionColor = renk;
        _detay.AppendText(metin);
    }

    // ---------------- Ekleme / düzenleme / silme işlemleri ----------------

    private void YeniIcerik()
    {
        using var form = new IcerikFormu();
        if (form.ShowDialog(this) != DialogResult.OK) return;
        FiltreleriTemizle();          // yeni eklenen film filtreler yüzünden görünmez olmasın
        KatalogHazirla();             // yeni bir ülke ya da yıl girildiyse filtre listelerine de gelsin
        KatalogYenile(form.KaydedilenID);
        Ui.Bilgi("Kayıt eklendi. Oyuncu/ekip ve (diziyse) bölümleri sağdaki sekmelerden ekleyebilirsiniz.");
    }

    private void IcerikDuzenle()
    {
        if (SeciliIcerikID is not int id) return;
        using var form = new IcerikFormu(id);
        if (form.ShowDialog(this) != DialogResult.OK) return;
        KatalogHazirla();
        KatalogYenile(id);
    }

    private void IcerikSil()
    {
        if (SeciliIcerikID is not int id) return;
        var ek = _seciliDizi ? "tüm sezon ve bölümleri, " : "";
        if (MessageBox.Show($"'{SeciliIcerikAdi}' {ek}oyuncu ve tür bağlantılarıyla birlikte kalıcı olarak silinecek.\n\nEmin misiniz?",
                "Silme Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        Veritabani.Yordam("sp_IcerikSil", Veritabani.P("@IcerikID", id));
        _seciliKart = null;
        KatalogHazirla();
    }

    private void KisiEkle()
    {
        if (SeciliIcerikID is not int id) return;
        using var form = new KisiFormu(id, SeciliIcerikAdi);
        if (form.ShowDialog(this) == DialogResult.OK) KatalogYenile(id);
    }

    private void KisiCikar()
    {
        if (SeciliIcerikID is not int id) return;
        if (_ekipIzgara.CurrentRow?.DataBoundItem is not DataRowView r)
        {
            Ui.Bilgi("Önce listeden bir kişi seçin.");
            return;
        }
        if (MessageBox.Show($"{r["Kisi"]} ({r["Gorev"]}) bu yapımdan çıkarılsın mı?", "Onay",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        Veritabani.Sorgu("DELETE FROM IcerikKisileri WHERE IcerikID = @i AND KisiID = @k AND Gorev = @g",
            Veritabani.P("@i", id), Veritabani.P("@k", r["KisiID"]), Veritabani.P("@g", r["Gorev"]));
        KatalogYenile(id);
    }

    private void BolumEkle()
    {
        if (SeciliIcerikID is not int id || !_seciliDizi) return;
        using var form = new BolumFormu(id, SeciliIcerikAdi);
        if (form.ShowDialog(this) != DialogResult.OK) return;
        KatalogYenile(id);
        _altSekmeler.SelectedTab = _bolumSekmesi;
    }

    private void BolumSil()
    {
        if (SeciliIcerikID is not int id) return;
        if (_bolumIzgara.CurrentRow?.DataBoundItem is not DataRowView r)
        {
            Ui.Bilgi("Önce listeden bir bölüm seçin.");
            return;
        }
        if (MessageBox.Show($"{r["BolumKodu"]} - {r["BolumAdi"]} silinsin mi?", "Onay",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        Veritabani.Yordam("sp_BolumSil", Veritabani.P("@BolumID", r["BolumID"]));
        KatalogYenile(id);
        _altSekmeler.SelectedTab = _bolumSekmesi;
    }
}
