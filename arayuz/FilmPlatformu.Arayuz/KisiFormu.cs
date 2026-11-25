namespace FilmPlatformu.Arayuz;

/// <summary>Filme/diziye oyuncu, yönetmen, yapımcı ya da senarist eklediğimiz pencere (sp_KisiEkle'yi çağırıyor).</summary>
public class KisiFormu : DiyalogFormu
{
    private readonly int _icerikID;
    private readonly TextBox _ad = new();
    private readonly TextBox _soyad = new();
    private readonly ComboBox _gorev = Ui.Secim();
    private readonly TextBox _karakter = new() { PlaceholderText = "(sadece oyuncular için)" };

    public KisiFormu(int icerikID, string icerikAdi) : base($"Oyuncu / Ekip Ekle - {icerikAdi}", 420, 250)
    {
        _icerikID = icerikID;
        _gorev.Items.AddRange(new object[] { "Oyuncu", "Yönetmen", "Yapımcı", "Senarist" });
        _gorev.SelectedIndex = 0;
        _gorev.SelectedIndexChanged += (_, _) => _karakter.Enabled = _gorev.SelectedIndex == 0;

        Satir("Ad: *", _ad);
        Satir("Soyad: *", _soyad);
        Satir("Görev:", _gorev);
        Satir("Karakter adı:", _karakter);
    }

    protected override bool Kaydet()
    {
        if (!Zorunlu(("Ad", _ad), ("Soyad", _soyad))) return false;
        bool oyuncu = _gorev.SelectedIndex == 0;
        Veritabani.Yordam("sp_KisiEkle",
            Veritabani.P("@IcerikID", _icerikID),
            Veritabani.P("@Ad", _ad.Text.Trim()),
            Veritabani.P("@Soyad", _soyad.Text.Trim()),
            Veritabani.P("@Gorev", _gorev.SelectedItem!.ToString()),
            Veritabani.P("@KarakterAdi", oyuncu ? BosIseNull(_karakter.Text) : null));
        return true;
    }
}
