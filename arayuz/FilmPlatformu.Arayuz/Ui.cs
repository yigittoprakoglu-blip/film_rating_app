namespace FilmPlatformu.Arayuz;

/// <summary>Buton, tablo gibi her yerde kullandığımız kontrolleri hep aynı görünsün diye burada oluşturuyoruz.</summary>
internal static class Ui
{
    public static readonly Color Vurgu = Color.FromArgb(67, 56, 202);
    public static readonly Color VurguAcik = Color.FromArgb(238, 242, 255);
    public static readonly Color Zemin = Color.FromArgb(246, 247, 251);
    public static readonly Color SolukYazi = Color.FromArgb(100, 106, 120);

    public static DataGridView Izgara(bool saltOkunur = true)
    {
        var g = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = saltOkunur,
            AllowUserToAddRows = !saltOkunur,
            AllowUserToDeleteRows = !saltOkunur,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells,
            SelectionMode = saltOkunur ? DataGridViewSelectionMode.FullRowSelect
                                       : DataGridViewSelectionMode.RowHeaderSelect,
            MultiSelect = true,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            RowHeadersWidth = 28,
            EnableHeadersVisualStyles = false,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
            GridColor = Color.FromArgb(225, 228, 235)
        };
        g.ColumnHeadersDefaultCellStyle.BackColor = Vurgu;
        g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Vurgu;
        g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.5f);
        g.ColumnHeadersDefaultCellStyle.Padding = new Padding(2, 4, 2, 4);
        g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 252);
        g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(199, 210, 254);
        g.DefaultCellStyle.SelectionForeColor = Color.Black;
        g.DataError += (_, e) =>
        {
            e.ThrowException = false;
            MessageBox.Show("Geçersiz değer: " + e.Exception?.Message, "Veri Hatası",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        };
        return g;
    }

    public static Button Dugme(string metin, bool ana = true)
    {
        var b = new Button
        {
            Text = metin,
            AutoSize = true,
            Height = 30,
            FlatStyle = FlatStyle.Flat,
            BackColor = ana ? Vurgu : Color.White,
            ForeColor = ana ? Color.White : Vurgu,
            Padding = new Padding(8, 2, 8, 2),
            Margin = new Padding(4),
            Cursor = Cursors.Hand
        };
        b.FlatAppearance.BorderColor = Vurgu;
        return b;
    }

    /// <summary>Bir kontrolü başlıklı bir çerçevenin (GroupBox) içine koyuyor.</summary>
    public static GroupBox Grup(string baslik, Control icerik)
    {
        var g = new GroupBox
        {
            Text = baslik,
            Dock = DockStyle.Fill,
            Padding = new Padding(6),
            Font = new Font("Segoe UI Semibold", 9.5f),
            ForeColor = Vurgu
        };
        icerik.Dock = DockStyle.Fill;
        icerik.Font = new Font("Segoe UI", 9.5f);
        icerik.ForeColor = Color.Black;
        g.Controls.Add(icerik);
        return g;
    }

    /// <summary>Etiket ve kutulardan oluşan küçük bir form grubu yapıyor.</summary>
    public static GroupBox FormGrubu(string baslik, (string Etiket, Control Kontrol)[] satirlar, Button? dugme)
    {
        var tablo = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = Color.Black
        };
        tablo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 75));
        tablo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        foreach (var (etiket, kontrol) in satirlar)
        {
            tablo.Controls.Add(new Label
            {
                Text = etiket,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(3, 6, 3, 3)
            });
            kontrol.Dock = DockStyle.Fill;
            kontrol.Margin = new Padding(3);
            tablo.Controls.Add(kontrol);
        }

        if (dugme != null)
        {
            dugme.Anchor = AnchorStyles.Right;
            tablo.Controls.Add(new Label { AutoSize = true });
            tablo.Controls.Add(dugme);
        }

        var g = new GroupBox
        {
            Text = baslik,
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(8, 4, 8, 8),
            Font = new Font("Segoe UI Semibold", 9.5f),
            ForeColor = Vurgu
        };
        g.Controls.Add(tablo);
        return g;
    }

    public static ComboBox Secim() => new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        IntegralHeight = false,
        MaxDropDownItems = 15
    };

    public static void Hata(Exception ex) =>
        MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);

    public static void Bilgi(string mesaj) =>
        MessageBox.Show(mesaj, "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
}
