using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace FilmPlatformu.Arayuz;

/// <summary>
/// Afişlerle ilgili her şey burada. Veritabanında resim varsa onu kullanıyoruz,
/// yoksa filmin adını yazan renkli bir afişi kendimiz çiziyoruz.
/// Afişlerin hepsi gerçek film afişleri gibi dikey (2:3 oranında).
/// </summary>
internal static class Afisler
{
    // Otomatik afişlerde kullandığımız renkler (üst renk, alt renk). Hepsi koyu ki beyaz yazı okunsun.
    private static readonly (Color Ust, Color Alt)[] Paletler =
    {
        (Color.FromArgb(49, 46, 129),  Color.FromArgb(17, 24, 39)),
        (Color.FromArgb(127, 29, 29),  Color.FromArgb(28, 25, 23)),
        (Color.FromArgb(6, 78, 59),    Color.FromArgb(17, 24, 39)),
        (Color.FromArgb(120, 53, 15),  Color.FromArgb(28, 25, 23)),
        (Color.FromArgb(30, 64, 175),  Color.FromArgb(15, 23, 42)),
        (Color.FromArgb(91, 33, 182),  Color.FromArgb(24, 24, 27)),
        (Color.FromArgb(14, 116, 144), Color.FromArgb(15, 23, 42)),
        (Color.FromArgb(157, 23, 77),  Color.FromArgb(24, 24, 27))
    };

    public static Image? Yukle(object? veri)
    {
        if (veri is not byte[] baytlar || baytlar.Length == 0) return null;
        try
        {
            using var ms = new MemoryStream(baytlar);
            using var img = Image.FromStream(ms);
            return new Bitmap(img);   // kopyasını alıyoruz, yoksa stream kapanınca resim bozuluyor
        }
        catch
        {
            return null;   // resim bozuksa hata vermesin, otomatik afiş çizilsin
        }
    }

    /// <summary>Resim varsa onu afiş boyutuna sığdırıyor, yoksa otomatik afiş çiziyor.</summary>
    public static Bitmap Ciz(Image? gorsel, string baslik, string tur, int genislik, int yukseklik)
    {
        var bmp = new Bitmap(genislik, yukseklik);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
        var alan = new Rectangle(0, 0, genislik, yukseklik);

        if (gorsel != null)
        {
            // Resmi alanı tamamen dolduracak kadar büyütüp ortalıyoruz, taşan kısmı kesiliyor
            float oran = Math.Max((float)genislik / gorsel.Width, (float)yukseklik / gorsel.Height);
            float w = gorsel.Width * oran, h = gorsel.Height * oran;
            g.DrawImage(gorsel, (genislik - w) / 2, (yukseklik - h) / 2, w, h);
            return bmp;
        }

        var (ust, alt) = Paletler[Math.Abs(KararliHash(baslik)) % Paletler.Length];
        using (var firca = new LinearGradientBrush(alan, ust, alt, 90f))
            g.FillRectangle(firca, alan);

        // Arka plana süs olsun diye çapraz çizgiler
        using (var kalem = new Pen(Color.FromArgb(28, 255, 255, 255), Math.Max(1, genislik / 60f)))
            for (int i = -yukseklik; i < genislik; i += genislik / 6)
                g.DrawLine(kalem, i, yukseklik, i + yukseklik, 0);

        // Filmin adını ortaya yazıyoruz (uzunsa alt satıra geçiyor)
        float yazi = Math.Max(9f, genislik / 9.5f);
        using var font = new Font("Segoe UI Semibold", yazi, FontStyle.Bold, GraphicsUnit.Pixel);
        var metinAlani = new RectangleF(genislik * 0.1f, yukseklik * 0.28f, genislik * 0.8f, yukseklik * 0.44f);
        using var bicim = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisWord };
        g.DrawString(baslik, font, Brushes.White, metinAlani, bicim);

        // En altta küçük harflerle FİLM ya da DİZİ yazıyor
        using var kucuk = new Font("Segoe UI", Math.Max(7f, genislik / 16f), FontStyle.Regular, GraphicsUnit.Pixel);
        using var soluk = new SolidBrush(Color.FromArgb(170, 255, 255, 255));
        g.DrawString(tur == "Dizi" ? "D İ Z İ" : "F İ L M", kucuk, soluk,
            new RectangleF(0, yukseklik * 0.8f, genislik, yukseklik * 0.1f), bicim);
        return bmp;
    }

    /// <summary>Seçilen resmi en fazla 400x600 boyutuna küçültüp JPEG yapıyor, veritabanı gereksiz büyümesin diye.</summary>
    public static byte[] DosyadanHazirla(string yol)
    {
        using var kaynak = Image.FromFile(yol);
        float oran = Math.Min(1f, Math.Min(400f / kaynak.Width, 600f / kaynak.Height));
        int w = Math.Max(1, (int)(kaynak.Width * oran)), h = Math.Max(1, (int)(kaynak.Height * oran));
        using var kucuk = new Bitmap(w, h);
        using (var g = Graphics.FromImage(kucuk))
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(kaynak, 0, 0, w, h);
        }
        using var ms = new MemoryStream();
        var jpeg = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
        using var p = new EncoderParameters(1);
        p.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 85L);
        kucuk.Save(ms, jpeg, p);
        return ms.ToArray();
    }

    private static int KararliHash(string s)
    {
        unchecked
        {
            int h = 23;
            foreach (var c in s) h = h * 31 + c;
            return h == int.MinValue ? 0 : h;
        }
    }
}
