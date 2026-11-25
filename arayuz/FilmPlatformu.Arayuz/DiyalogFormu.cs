namespace FilmPlatformu.Arayuz;

/// <summary>
/// Ekleme pencerelerinin (film, bölüm, kişi) hepsi aynı görünümde olduğu için
/// ortak kısmı bu sınıfa yazdık: solda etiket, sağda kutu ve altta Kaydet/İptal.
/// Diğer formlar bundan kalıtım alıyor (inheritance) ve sadece Kaydet() kısmını yazıyor.
/// </summary>
public abstract class DiyalogFormu : Form
{
    protected readonly TableLayoutPanel Tablo = new()
    {
        Dock = DockStyle.Fill,
        ColumnCount = 2,
        Padding = new Padding(16, 12, 16, 4),
        AutoScroll = true
    };

    protected DiyalogFormu(string baslik, int genislik, int yukseklik)
    {
        Text = baslik;
        Font = new Font("Segoe UI", 9.5f);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(genislik, yukseklik);
        BackColor = Color.White;

        Tablo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        Tablo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var kaydet = Ui.Dugme("Kaydet");
        var iptal = Ui.Dugme("İptal", ana: false);
        iptal.DialogResult = DialogResult.Cancel;
        var alt = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 50,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(12, 6, 12, 6)
        };
        alt.Controls.Add(kaydet);
        alt.Controls.Add(iptal);

        Controls.Add(Tablo);
        Controls.Add(alt);
        CancelButton = iptal;

        kaydet.Click += (_, _) =>
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                if (Kaydet())
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            catch (Exception ex)
            {
                Ui.Hata(ex);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        };
    }

    /// <summary>Forma bir satır ekliyor (etiket + kutu). Konu gibi büyük kutular için yükseklik verilebiliyor.</summary>
    protected void Satir(string etiket, Control kontrol, int? yukseklik = null)
    {
        Tablo.RowStyles.Add(yukseklik is int y ? new RowStyle(SizeType.Absolute, y) : new RowStyle(SizeType.AutoSize));
        Tablo.Controls.Add(new Label
        {
            Text = etiket,
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Top,
            Margin = new Padding(3, 7, 3, 3)
        });
        kontrol.Dock = DockStyle.Fill;
        kontrol.Margin = new Padding(3);
        Tablo.Controls.Add(kontrol);
    }

    /// <summary>Her form kendi kaydetme işini burada yapıyor. true dönerse pencere kapanıyor.</summary>
    protected abstract bool Kaydet();

    protected static bool Zorunlu(params (string Ad, Control Kontrol)[] alanlar)
    {
        foreach (var (ad, kontrol) in alanlar)
        {
            if (string.IsNullOrWhiteSpace(kontrol.Text))
            {
                Ui.Bilgi($"'{ad}' alanı boş bırakılamaz.");
                kontrol.Focus();
                return false;
            }
        }
        return true;
    }

    protected static object? BosIseNull(string metin) =>
        string.IsNullOrWhiteSpace(metin) ? null : metin.Trim();
}
