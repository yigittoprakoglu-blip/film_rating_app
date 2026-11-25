using System.Drawing.Drawing2D;

namespace FilmPlatformu.Arayuz;

/// <summary>
/// Ortadaki her bir film/dizi kartı. Üstte afiş ve IMDb puanı, altında
/// başlık, yıl, süre, yönetmen ve tür etiketleri var.
/// Kartı tek tek Label koyarak değil OnPaint ile kendimiz çizdik,
/// böylece çok kart olunca da ekran titremiyor.
/// </summary>
public sealed class IcerikKarti : Control
{
    public const int KartGenislik = 184;
    public const int KartYukseklik = 392;
    private const int Bosluk = 10;
    private const int AfisG = KartGenislik - 2 * Bosluk;   // afiş genişliği (164 px)
    private const int AfisY = AfisG * 3 / 2;               // afiş yüksekliği (246 px), 2:3 oranı için genişlik x 1.5

    public int IcerikID { get; }
    public string Baslik { get; }
    private readonly string _tur;
    private readonly string _altBilgi;
    private readonly string _yonetmen;
    private readonly string[] _etiketler;
    private readonly decimal? _imdb;
    private readonly Bitmap _afis;

    private bool _secili;
    private bool _uzerinde;

    public bool Secili
    {
        get => _secili;
        set { _secili = value; Invalidate(); }
    }

    public IcerikKarti(int id, string baslik, string tur, int yil, string sure,
                       decimal? imdb, string? yonetmen, string? kategoriler, object? afisVerisi)
    {
        IcerikID = id;
        Baslik = baslik;
        _tur = tur;
        _imdb = imdb;
        _altBilgi = $"{yil}  •  {sure}";
        _yonetmen = string.IsNullOrWhiteSpace(yonetmen) ? "" : "Yön: " + yonetmen;
        _etiketler = (kategoriler ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        using (var gorsel = Afisler.Yukle(afisVerisi))
            _afis = Afisler.Ciz(gorsel, baslik, tur, AfisG, AfisY);

        Size = new Size(KartGenislik, KartYukseklik);
        Margin = new Padding(8);
        Cursor = Cursors.Hand;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
    }

    protected override void OnMouseEnter(EventArgs e) { _uzerinde = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _uzerinde = false; Invalidate(); base.OnMouseLeave(e); }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _afis.Dispose();
        base.Dispose(disposing);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        g.Clear(Parent?.BackColor ?? Ui.Zemin);

        // Kartın beyaz arka planı ve kenarı (seçiliyse kalın mor kenar)
        var kart = new Rectangle(0, 0, Width - 1, Height - 1);
        using (var yol = Yuvarlak(kart, 10))
        {
            using var zemin = new SolidBrush(Color.White);
            g.FillPath(zemin, yol);
            var kenar = _secili ? Ui.Vurgu : _uzerinde ? Color.FromArgb(165, 180, 252) : Color.FromArgb(226, 232, 240);
            using var kalem = new Pen(kenar, _secili ? 3f : 1f);
            g.DrawPath(kalem, yol);
        }

        // Afiş resmi
        var afisAlani = new Rectangle(Bosluk, Bosluk, AfisG, AfisY);
        using (var yol = Yuvarlak(afisAlani, 8))
        {
            g.SetClip(yol);
            g.DrawImage(_afis, afisAlani);
            g.ResetClip();
        }

        // Sağ üst köşedeki sarı IMDb puanı
        if (_imdb is decimal puan)
        {
            var metin = $"★ {puan:0.0}";
            using var f = new Font("Segoe UI", 9f, FontStyle.Bold);
            var boyut = g.MeasureString(metin, f);
            var rozet = new RectangleF(afisAlani.Right - boyut.Width - 12, afisAlani.Top + 6, boyut.Width + 6, boyut.Height + 2);
            using var yol = Yuvarlak(Rectangle.Round(rozet), 6);
            using var sari = new SolidBrush(Color.FromArgb(245, 197, 24));
            g.FillPath(sari, yol);
            g.DrawString(metin, f, Brushes.Black, rozet.X + 3, rozet.Y + 1);
        }

        // Sol alt köşedeki FİLM / DİZİ yazısı
        {
            using var f = new Font("Segoe UI Semibold", 7.5f, FontStyle.Bold);
            var metin = _tur == "Dizi" ? "DİZİ" : "FİLM";
            var boyut = g.MeasureString(metin, f);
            var rozet = new RectangleF(afisAlani.Left + 6, afisAlani.Bottom - boyut.Height - 8, boyut.Width + 6, boyut.Height + 2);
            using var yol = Yuvarlak(Rectangle.Round(rozet), 5);
            using var koyu = new SolidBrush(Color.FromArgb(200, 17, 24, 39));
            g.FillPath(koyu, yol);
            g.DrawString(metin, f, Brushes.White, rozet.X + 3, rozet.Y + 1);
        }

        // Film adı (uzunsa en fazla 2 satır)
        int y = afisAlani.Bottom + 6;
        using (var f = new Font("Segoe UI Semibold", 10f, FontStyle.Bold))
        {
            var alan = new RectangleF(Bosluk, y, AfisG, 38);
            using var bicim = new StringFormat { Trimming = StringTrimming.EllipsisWord };
            g.DrawString(Baslik, f, Brushes.Black, alan, bicim);
            y += (int)Math.Min(38, g.MeasureString(Baslik, f, AfisG).Height) + 2;
        }

        // Yıl, süre ve yönetmen satırları
        using (var f = new Font("Segoe UI", 8.5f))
        using (var gri = new SolidBrush(Ui.SolukYazi))
        using (var bicim = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap })
        {
            g.DrawString(_altBilgi, f, gri, new RectangleF(Bosluk, y, AfisG, 18), bicim);
            y += 18;
            if (_yonetmen.Length > 0)
            {
                g.DrawString(_yonetmen, f, gri, new RectangleF(Bosluk, y, AfisG, 18), bicim);
                y += 19;
            }
        }

        // Tür etiketleri (Dram, Suç...). Sığmazsa alt satıra geçiyor, en fazla 2 satır.
        using (var f = new Font("Segoe UI", 7.5f))
        using (var zemin = new SolidBrush(Ui.VurguAcik))
        using (var yazi = new SolidBrush(Ui.Vurgu))
        {
            int x = Bosluk, satir = 0;
            foreach (var etiket in _etiketler)
            {
                var boyut = g.MeasureString(etiket, f);
                int w = (int)boyut.Width + 8, h = (int)boyut.Height + 3;
                if (x + w > Bosluk + AfisG)
                {
                    if (++satir == 2) break;
                    x = Bosluk;
                    y += h + 4;
                }
                var cip = new Rectangle(x, y, w, h);
                using var yol = Yuvarlak(cip, h / 2);
                g.FillPath(zemin, yol);
                g.DrawString(etiket, f, yazi, x + 4, y + 1);
                x += w + 4;
            }
        }
    }

    private static GraphicsPath Yuvarlak(Rectangle r, int yaricap)
    {
        int d = Math.Max(1, yaricap * 2);
        var yol = new GraphicsPath();
        yol.AddArc(r.X, r.Y, d, d, 180, 90);
        yol.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        yol.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        yol.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        yol.CloseFigure();
        return yol;
    }
}
