using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;

namespace FilmPlatformu.Arayuz;

public partial class AnaForm
{
    /// <summary>Soldaki ağaçta seçilen nesnenin bilgisi. Tur değerleri SQL Server'dan geliyor: U=tablo, V=view, P=yordam, FN/IF/TF=fonksiyon, TR=tetikleyici</summary>
    private sealed record NesneBilgisi(string Sema, string Ad, string Tur)
    {
        public string TamAd => $"[{Sema.Replace("]", "]]")}].[{Ad.Replace("]", "]]")}]";
        public bool TabloMu => Tur == "U";
        public bool GorunumMu => Tur == "V";
    }

    private readonly TreeView _nesneAgaci = new()
    {
        Dock = DockStyle.Fill,
        HideSelection = false,
        BorderStyle = BorderStyle.None,
        ItemHeight = 24,
        Font = new Font("Segoe UI", 9.5f)
    };
    private readonly TabControl _tabloSekmeleri = new() { Dock = DockStyle.Fill };
    private readonly DataGridView _tabloIzgara = Ui.Izgara(saltOkunur: false);
    private readonly DataGridView _yapiIzgara = Ui.Izgara();
    private readonly TextBox _filtre = new() { Width = 220, PlaceholderText = "Bu tabloda ara / filtrele..." };
    private readonly Label _tabloBilgi = new() { AutoSize = true, ForeColor = Ui.SolukYazi, Margin = new Padding(10, 9, 3, 3) };
    private readonly Button _kaydetDugmesi = Ui.Dugme("Kaydet (Ctrl+S)");
    private readonly Button _silDugmesi = Ui.Dugme("Seçili Satırları Sil", ana: false);
    private readonly Button _yenileDugmesi = Ui.Dugme("Yenile (F5)", ana: false);
    private readonly Button _kodDugmesi = Ui.Dugme("SQL Kodunu Göster", ana: false);

    private SqlDataAdapter? _adaptor;
    private DataTable? _acikTablo;
    private NesneBilgisi? _acikNesne;
    private bool _agacYukleniyor;

    private TabPage TablolarSekmesiOlustur()
    {
        var sekme = new TabPage("Tablolar ve Görünümler") { BackColor = Ui.Zemin };

        var bolme = new SplitContainer
        {
            Dock = DockStyle.Fill,
            FixedPanel = FixedPanel.Panel1,
            BackColor = Ui.Zemin
        };
        bolme.Panel1.Padding = new Padding(6);
        bolme.Panel1.Controls.Add(Ui.Grup("Veritabanı Nesneleri", _nesneAgaci));

        var arac = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42, Padding = new Padding(2, 4, 2, 2), WrapContents = false };
        _filtre.Margin = new Padding(4, 7, 4, 4);
        arac.Controls.AddRange(new Control[] { _kaydetDugmesi, _silDugmesi, _yenileDugmesi, _kodDugmesi, _filtre, _tabloBilgi });

        var veriSekmesi = new TabPage("Veriler");
        veriSekmesi.Controls.Add(_tabloIzgara);
        var yapiSekmesi = new TabPage("Yapı (Sütunlar, Anahtarlar)");
        yapiSekmesi.Controls.Add(_yapiIzgara);
        _tabloSekmeleri.TabPages.Add(veriSekmesi);
        _tabloSekmeleri.TabPages.Add(yapiSekmesi);

        bolme.Panel2.Padding = new Padding(0, 6, 6, 6);
        bolme.Panel2.Controls.Add(_tabloSekmeleri);
        bolme.Panel2.Controls.Add(arac);
        sekme.Controls.Add(bolme);
        bool ayarlandi = false;
        sekme.Layout += (_, _) =>
        {
            if (ayarlandi || bolme.Width <= 400) return;
            bolme.SplitterDistance = 260;
            ayarlandi = true;
        };

        _nesneAgaci.AfterSelect += (_, e) => { if (!_agacYukleniyor) Guvenli(() => NesneSecildi(e.Node)); };
        _nesneAgaci.NodeMouseDoubleClick += (_, e) => Guvenli(() => { if (e.Node?.Tag is NesneBilgisi n && !n.TabloMu) KoduGoster(n); });
        _kaydetDugmesi.Click += (_, _) => Guvenli(TabloyuKaydet);
        _silDugmesi.Click += (_, _) => Guvenli(SeciliSatirlariSil);
        _yenileDugmesi.Click += (_, _) => Guvenli(() => { NesneAgaciniYukle(); SeciliTabloyuYenidenAc(); });
        _kodDugmesi.Click += (_, _) => Guvenli(() => { if (_acikNesne != null) KoduGoster(_acikNesne); });
        _filtre.TextChanged += (_, _) => Guvenli(FiltreUygula);
        _tabloIzgara.UserDeletingRow += (_, e) =>
        {
            if (MessageBox.Show("Satır silinsin mi? (Kaydet'e basınca veritabanına yansır)", "Onay",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                e.Cancel = true;
        };

        DugmeleriAyarla();
        return sekme;
    }

    /// <summary>Veritabanındaki tablo, view, yordam, fonksiyon ve tetikleyicileri sys.objects'ten çekip ağaca diziyor.</summary>
    private void NesneAgaciniYukle()
    {
        var dt = Veritabani.Sorgu(@"
            SELECT s.name AS Sema, o.name AS Ad, RTRIM(o.type) AS Tur,
                   CASE WHEN o.type = 'U' THEN (SELECT SUM(p.rows) FROM sys.partitions AS p
                                                WHERE p.object_id = o.object_id AND p.index_id IN (0, 1)) END AS SatirSayisi,
                   OBJECT_NAME(o.parent_object_id) AS UstNesne
            FROM sys.objects AS o
            JOIN sys.schemas AS s ON s.schema_id = o.schema_id
            WHERE o.is_ms_shipped = 0
              AND o.type IN ('U', 'V', 'P', 'FN', 'IF', 'TF', 'TR')
              AND o.name NOT IN ('sysdiagrams')
              AND o.name NOT LIKE 'sp[_]%diagram%'
              AND o.name NOT LIKE 'fn[_]diagramobjects'
            ORDER BY o.name;");

        var secili = (_nesneAgaci.SelectedNode?.Tag as NesneBilgisi)?.Ad;
        _agacYukleniyor = true;
        _nesneAgaci.BeginUpdate();
        _nesneAgaci.Nodes.Clear();

        var gruplar = new (string Baslik, string[] Turler)[]
        {
            ("Tablolar", new[] { "U" }),
            ("Görünümler (View)", new[] { "V" }),
            ("Saklı Yordamlar (Procedure)", new[] { "P" }),
            ("Fonksiyonlar (Function)", new[] { "FN", "IF", "TF" }),
            ("Tetikleyiciler (Trigger)", new[] { "TR" })
        };

        TreeNode? secilecek = null;
        foreach (var (baslik, turler) in gruplar)
        {
            var satirlar = dt.AsEnumerable().Where(r => turler.Contains(r.Field<string>("Tur"))).ToList();
            var grup = _nesneAgaci.Nodes.Add($"{baslik}  [{satirlar.Count}]");
            grup.ForeColor = Ui.Vurgu;
            foreach (var r in satirlar)
            {
                var bilgi = new NesneBilgisi(r.Field<string>("Sema")!, r.Field<string>("Ad")!, r.Field<string>("Tur")!);
                string metin = bilgi.Ad;
                if (bilgi.TabloMu) metin += $"  ({r["SatirSayisi"]})";
                if (bilgi.Tur == "TR") metin += $"  → {r["UstNesne"]}";
                var dugum = grup.Nodes.Add(metin);
                dugum.Tag = bilgi;
                if (bilgi.Ad == secili) secilecek = dugum;
            }
            if (turler.Contains("U") || turler.Contains("V")) grup.Expand();
        }

        _nesneAgaci.EndUpdate();
        if (secilecek != null) _nesneAgaci.SelectedNode = secilecek;
        _agacYukleniyor = false;
    }

    private void NesneSecildi(TreeNode? dugum)
    {
        if (dugum?.Tag is not NesneBilgisi nesne) return;
        if (!KaydedilmemisDegisiklikleriSor()) return;

        if (nesne.TabloMu || nesne.GorunumMu)
            TabloAc(nesne);
        else
            ProgramlanabilirNesneyiGoster(nesne);
    }

    /// <summary>Seçilen tabloyu ızgarada açıyor. Tablolar düzenlenebiliyor, view'lar sadece okunabiliyor.</summary>
    private void TabloAc(NesneBilgisi nesne)
    {
        _adaptor?.Dispose();
        _acikNesne = nesne;
        _adaptor = new SqlDataAdapter($"SELECT * FROM {nesne.TamAd}", Veritabani.BaglantiCumlesi)
        {
            MissingSchemaAction = MissingSchemaAction.AddWithKey
        };

        if (nesne.TabloMu)
        {
            // SqlCommandBuilder UPDATE ve DELETE sorgularını bizim yerimize yazıyor. WHERE kısmında sadece primary key'e bakıyor.
            _ = new SqlCommandBuilder(_adaptor) { ConflictOption = ConflictOption.OverwriteChanges };
            // INSERT'i kendimiz yazdık: boş bırakılan sütunları göndermiyoruz ki tablodaki DEFAULT değerler çalışsın.
            _adaptor.RowUpdating += Adaptor_RowUpdating;
        }

        var tablo = new DataTable(nesne.Ad);
        _adaptor.Fill(tablo);
        foreach (DataColumn c in tablo.Columns)
        {
            if (c.AutoIncrement)
            {
                // Yeni eklenen satıra şimdilik -1, -2 gibi geçici ID veriyoruz. Asıl ID'yi kaydedince SQL Server veriyor (IDENTITY).
                c.AutoIncrementSeed = -1;
                c.AutoIncrementStep = -1;
                c.ReadOnly = true;
            }
            else
            {
                // Boş mu dolu mu kontrolünü burada yapmıyoruz, SQL Server'daki NOT NULL kısıtları zaten hata veriyor.
                c.AllowDBNull = true;
            }
        }

        _acikTablo = tablo;
        _tabloIzgara.DataSource = tablo;
        _tabloIzgara.ReadOnly = !nesne.TabloMu;
        _tabloIzgara.AllowUserToAddRows = nesne.TabloMu;
        _tabloIzgara.AllowUserToDeleteRows = nesne.TabloMu;
        foreach (DataGridViewColumn kolon in _tabloIzgara.Columns)
            if (tablo.Columns[kolon.DataPropertyName]?.AutoIncrement == true)
                kolon.DefaultCellStyle.BackColor = Ui.VurguAcik;

        _yapiIzgara.DataSource = YapiBilgisi(nesne);
        _filtre.Text = "";
        _tabloSekmeleri.SelectedIndex = 0;
        TabloBilgisiniGuncelle();
        DugmeleriAyarla();
    }

    private void Adaptor_RowUpdating(object sender, SqlRowUpdatingEventArgs e)
    {
        if (e.StatementType != StatementType.Insert || e.Row == null || _acikNesne == null || _adaptor == null)
            return;

        var sutunlar = e.Row.Table.Columns.Cast<DataColumn>()
            .Where(c => !c.AutoIncrement && e.Row[c] != DBNull.Value)
            .ToList();

        var komut = new SqlCommand
        {
            Connection = _adaptor.SelectCommand!.Connection,
            Transaction = e.Command?.Transaction,
            UpdatedRowSource = UpdateRowSource.None
        };

        if (sutunlar.Count == 0)
        {
            komut.CommandText = $"INSERT INTO {_acikNesne.TamAd} DEFAULT VALUES";
        }
        else
        {
            var adlar = string.Join(", ", sutunlar.Select(c => $"[{c.ColumnName}]"));
            var degerler = string.Join(", ", sutunlar.Select((_, i) => "@p" + i));
            komut.CommandText = $"INSERT INTO {_acikNesne.TamAd} ({adlar}) VALUES ({degerler})";
            for (int i = 0; i < sutunlar.Count; i++)
                komut.Parameters.AddWithValue("@p" + i, e.Row[sutunlar[i]]);
        }
        e.Command = komut;
    }

    private DataTable YapiBilgisi(NesneBilgisi nesne) => Veritabani.Sorgu(@"
        SELECT c.ORDINAL_POSITION AS Sira,
               c.COLUMN_NAME AS Sutun,
               c.DATA_TYPE +
                 CASE WHEN c.CHARACTER_MAXIMUM_LENGTH = -1 THEN '(MAX)'
                      WHEN c.CHARACTER_MAXIMUM_LENGTH IS NOT NULL THEN '(' + CAST(c.CHARACTER_MAXIMUM_LENGTH AS VARCHAR(10)) + ')'
                      WHEN c.DATA_TYPE IN ('decimal', 'numeric') THEN '(' + CAST(c.NUMERIC_PRECISION AS VARCHAR(5)) + ',' + CAST(c.NUMERIC_SCALE AS VARCHAR(5)) + ')'
                      ELSE '' END AS VeriTipi,
               CASE WHEN c.IS_NULLABLE = 'YES' THEN N'Evet' ELSE N'Hayır' END AS BosOlabilir,
               CASE WHEN COLUMNPROPERTY(OBJECT_ID(@tam), c.COLUMN_NAME, 'IsIdentity') = 1 THEN N'Evet' ELSE '' END AS [Identity],
               (SELECT STRING_AGG(CASE tc.CONSTRAINT_TYPE WHEN 'PRIMARY KEY' THEN 'PK' WHEN 'FOREIGN KEY' THEN 'FK' WHEN 'UNIQUE' THEN 'UQ' END, ', ')
                  FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS k
                  JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc
                    ON tc.CONSTRAINT_NAME = k.CONSTRAINT_NAME AND tc.TABLE_SCHEMA = k.TABLE_SCHEMA
                 WHERE k.TABLE_SCHEMA = c.TABLE_SCHEMA AND k.TABLE_NAME = c.TABLE_NAME AND k.COLUMN_NAME = c.COLUMN_NAME) AS Anahtar,
               (SELECT STRING_AGG(OBJECT_NAME(f.referenced_object_id) + '.' + COL_NAME(f.referenced_object_id, f.referenced_column_id), ', ')
                  FROM sys.foreign_key_columns AS f
                 WHERE f.parent_object_id = OBJECT_ID(@tam)
                   AND COL_NAME(f.parent_object_id, f.parent_column_id) = c.COLUMN_NAME) AS Referans,
               c.COLUMN_DEFAULT AS Varsayilan
        FROM INFORMATION_SCHEMA.COLUMNS AS c
        WHERE c.TABLE_SCHEMA = @sema AND c.TABLE_NAME = @ad
        ORDER BY c.ORDINAL_POSITION;",
        Veritabani.P("@tam", nesne.TamAd), Veritabani.P("@sema", nesne.Sema), Veritabani.P("@ad", nesne.Ad));

    /// <summary>Ağaçta yordam, fonksiyon ya da tetikleyici seçilince parametrelerini listeliyor.</summary>
    private void ProgramlanabilirNesneyiGoster(NesneBilgisi nesne)
    {
        _adaptor?.Dispose();
        _adaptor = null;
        _acikTablo = null;
        _acikNesne = nesne;

        var parametreler = Veritabani.Sorgu(@"
            SELECT CASE WHEN p.parameter_id = 0 THEN N'(dönüş değeri)' ELSE p.name END AS Parametre,
                   TYPE_NAME(p.user_type_id) +
                     CASE WHEN TYPE_NAME(p.user_type_id) LIKE '%char' THEN
                          '(' + CASE WHEN p.max_length = -1 THEN 'MAX'
                                     WHEN TYPE_NAME(p.user_type_id) LIKE 'n%' THEN CAST(p.max_length / 2 AS VARCHAR(10))
                                     ELSE CAST(p.max_length AS VARCHAR(10)) END + ')'
                          ELSE '' END AS VeriTipi,
                   CASE WHEN p.is_output = 1 THEN N'Evet' ELSE '' END AS Cikis
            FROM sys.parameters AS p
            WHERE p.object_id = OBJECT_ID(@tam)
            ORDER BY p.parameter_id;", Veritabani.P("@tam", nesne.TamAd));

        if (parametreler.Rows.Count == 0)
        {
            parametreler.Rows.Add(nesne.Tur == "TR" ? "(tetikleyicilerin parametresi yoktur)" : "(parametre yok)", "", "");
        }

        _tabloIzgara.DataSource = parametreler;
        _tabloIzgara.ReadOnly = true;
        _tabloIzgara.AllowUserToAddRows = false;
        _tabloIzgara.AllowUserToDeleteRows = false;
        _yapiIzgara.DataSource = null;
        _filtre.Text = "";
        _tabloBilgi.Text = $"{nesne.Sema}.{nesne.Ad} — parametreler. Kodu görmek için çift tıklayın.";
        DugmeleriAyarla();
    }

    private void KoduGoster(NesneBilgisi nesne)
    {
        if (nesne.TabloMu) return;
        var tanim = Veritabani.Sorgu("SELECT OBJECT_DEFINITION(OBJECT_ID(@tam)) AS Tanim", Veritabani.P("@tam", nesne.TamAd))
                              .Rows[0]["Tanim"] as string;
        var kod = (tanim ?? "-- Tanım okunamadı.").Replace("\r\n", "\n").Replace("\n", "\r\n");

        // Kodu sadece okunabilen ayrı bir pencerede açıyoruz (değiştirmek SSMS'ten yapılıyor)
        using var pencere = new Form
        {
            Text = $"{nesne.Sema}.{nesne.Ad} - SQL Kodu",
            Size = new Size(820, 600),
            StartPosition = FormStartPosition.CenterParent,
            MinimizeBox = false,
            ShowIcon = false,
            ShowInTaskbar = false
        };
        var kutu = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Both,
            WordWrap = false,
            BackColor = Color.White,
            Font = new Font("Consolas", 10.5f),
            Text = kod
        };
        pencere.Controls.Add(kutu);
        pencere.Shown += (_, _) => { kutu.SelectionStart = 0; kutu.SelectionLength = 0; };
        pencere.ShowDialog(this);
    }

    private void SeciliTabloyuYenidenAc()
    {
        if (_acikNesne == null) return;
        if (_acikNesne.TabloMu || _acikNesne.GorunumMu) TabloAc(_acikNesne);
        else ProgramlanabilirNesneyiGoster(_acikNesne);
    }

    private void TabloyuKaydet()
    {
        if (_adaptor == null || _acikTablo == null || _acikNesne?.TabloMu != true) return;

        _tabloIzgara.EndEdit();
        (BindingContext?[_acikTablo] as CurrencyManager)?.EndCurrentEdit();

        var degisiklik = _acikTablo.GetChanges();
        if (degisiklik == null)
        {
            Ui.Bilgi("Kaydedilecek değişiklik yok.");
            return;
        }

        try
        {
            int adet = _adaptor.Update(_acikTablo);
            NesneAgaciniYukle();
            TabloAc(_acikNesne);
            _tabloBilgi.Text += $"   ✔ {adet} satır kaydedildi.";
        }
        catch (Exception ex)
        {
            Ui.Hata(new Exception("Kayıt sırasında SQL Server hata verdi:\n\n" + ex.Message +
                                  "\n\nHatalı satır kırmızı ünlem ile işaretlendi. Düzeltip tekrar kaydedebilirsiniz.", ex));
        }
    }

    private void SeciliSatirlariSil()
    {
        if (_acikTablo == null || _acikNesne?.TabloMu != true) return;

        var satirlar = _tabloIzgara.SelectedRows.Cast<DataGridViewRow>().Where(r => !r.IsNewRow).ToList();
        if (satirlar.Count == 0 && _tabloIzgara.CurrentRow is { IsNewRow: false } aktif)
            satirlar.Add(aktif);
        if (satirlar.Count == 0)
        {
            Ui.Bilgi("Silmek için satır başlığına tıklayarak bir veya daha fazla satır seçin.");
            return;
        }

        if (MessageBox.Show($"{satirlar.Count} satır veritabanından kalıcı olarak silinecek. Emin misiniz?",
                "Silme Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        foreach (var r in satirlar) _tabloIzgara.Rows.Remove(r);
        TabloyuKaydet();
    }

    /// <summary>Başka tabloya geçmeden önce kaydedilmemiş değişiklik varsa kaydedelim mi diye soruyor.</summary>
    private bool KaydedilmemisDegisiklikleriSor()
    {
        if (_acikTablo?.GetChanges() == null) return true;
        var cevap = MessageBox.Show($"'{_acikNesne?.Ad}' tablosunda kaydedilmemiş değişiklikler var. Kaydedilsin mi?",
            "Kaydedilmemiş Değişiklik", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (cevap == DialogResult.Yes) TabloyuKaydet();
        else _acikTablo.RejectChanges();
        return true;
    }

    /// <summary>Üstteki arama kutusu: yazılan metni bütün sütunlarda arıyor (DataView.RowFilter ile, veritabanına tekrar gitmeden).</summary>
    private void FiltreUygula()
    {
        if (_acikTablo == null) return;
        var metin = _filtre.Text.Trim();
        if (metin.Length == 0)
        {
            _acikTablo.DefaultView.RowFilter = "";
        }
        else
        {
            var guvenli = new StringBuilder();
            foreach (var ch in metin)
            {
                if (ch is '*' or '%' or '[' or ']') guvenli.Append('[').Append(ch).Append(']');
                else if (ch == '\'') guvenli.Append("''");
                else guvenli.Append(ch);
            }
            var kosullar = _acikTablo.Columns.Cast<DataColumn>()
                .Where(c => c.DataType != typeof(byte[]))
                .Select(c => $"CONVERT([{c.ColumnName}], 'System.String') LIKE '%{guvenli}%'");
            _acikTablo.DefaultView.RowFilter = string.Join(" OR ", kosullar);
        }
        TabloBilgisiniGuncelle();
    }

    private void TabloBilgisiniGuncelle()
    {
        if (_acikTablo == null || _acikNesne == null) return;
        var tur = _acikNesne.TabloMu ? "tablo — hücreye tıklayıp düzenleyin, en alttaki boş satırdan ekleyin"
                                     : "görünüm — salt okunur";
        var adet = _acikTablo.DefaultView.Count == _acikTablo.Rows.Count
            ? $"{_acikTablo.Rows.Count} satır"
            : $"{_acikTablo.DefaultView.Count} / {_acikTablo.Rows.Count} satır";
        _tabloBilgi.Text = $"{_acikNesne.Sema}.{_acikNesne.Ad}  •  {adet}  •  {tur}";
    }

    private void DugmeleriAyarla()
    {
        bool tablo = _acikNesne?.TabloMu == true;
        _kaydetDugmesi.Enabled = tablo;
        _silDugmesi.Enabled = tablo;
        _filtre.Enabled = _acikTablo != null;
        _kodDugmesi.Enabled = _acikNesne != null && !tablo;
    }
}
