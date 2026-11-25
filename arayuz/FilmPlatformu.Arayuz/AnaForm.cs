namespace FilmPlatformu.Arayuz;

/// <summary>
/// Programın ana penceresi. Kod çok uzamasın diye her sekmeyi ayrı dosyaya
/// böldük (partial class sayesinde hepsi aynı sınıf sayılıyor):
///   AnaForm.Katalog.cs   -> Filmler ve Diziler sekmesi (listeleme, arama, ekleme, silme...)
///   AnaForm.Tablolar.cs  -> Tablolar ve Görünümler sekmesi (veritabanını olduğu gibi gösteriyor)
/// </summary>
public partial class AnaForm : Form
{
    private readonly TabControl _sekmeler = new() { Dock = DockStyle.Fill, Padding = new Point(14, 6) };
    private readonly ToolStripStatusLabel _durum = new() { Spring = true, TextAlign = ContentAlignment.MiddleLeft };

    private readonly TabPage _katalogSekmesi;
    private readonly TabPage _tablolarSekmesi;

    public AnaForm()
    {
        Text = "Film ve Dizi Platformu - Veritabanı Arayüzü";
        Font = new Font("Segoe UI", 9.5f);
        Size = new Size(1400, 860);
        MinimumSize = new Size(1050, 650);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Ui.Zemin;
        KeyPreview = true;

        _katalogSekmesi = KatalogSekmesiOlustur();
        _tablolarSekmesi = TablolarSekmesiOlustur();
        _sekmeler.TabPages.AddRange(new[] { _katalogSekmesi, _tablolarSekmesi });

        var durumCubugu = new StatusStrip { BackColor = Color.White };
        durumCubugu.Items.Add(_durum);
        _durum.Text = $"Bağlı: {Veritabani.Sunucu}  •  Veritabanı: {Veritabani.VeritabaniAdi}";

        Controls.Add(_sekmeler);
        Controls.Add(durumCubugu);

        _sekmeler.SelectedIndexChanged += (_, _) => Guvenli(SekmeDegisti);
        KeyDown += AnaForm_KeyDown;
        Load += (_, _) => Guvenli(() =>
        {
            KatalogHazirla();
            NesneAgaciniYukle();
        });
    }

    private void SekmeDegisti()
    {
        // Diğer sekmede bir şey değiştirilmiş olabilir, o yüzden sekmeye her geçişte verileri yeniliyoruz.
        if (_sekmeler.SelectedTab == _katalogSekmesi) KatalogYenile();
        else if (_sekmeler.SelectedTab == _tablolarSekmesi) { NesneAgaciniYukle(); SeciliTabloyuYenidenAc(); }
    }

    private void AnaForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.F5)
        {
            e.Handled = true;
            Guvenli(SekmeDegisti);
        }
        else if (e.Control && e.KeyCode == Keys.S && _sekmeler.SelectedTab == _tablolarSekmesi)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            Guvenli(TabloyuKaydet);
        }
    }

    /// <summary>
    /// Bütün buton işlemlerini bunun içinden çağırıyoruz. Bir hata çıkarsa program kapanmıyor,
    /// hatayı mesaj kutusunda gösteriyor (mesela tetikleyicinin THROW ettiği mesaj).
    /// </summary>
    private void Guvenli(Action islem)
    {
        try
        {
            Cursor = Cursors.WaitCursor;
            islem();
        }
        catch (Exception ex)
        {
            Ui.Hata(ex);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }
}
