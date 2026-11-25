namespace FilmPlatformu.Arayuz;

/// <summary>Diziye yeni bölüm eklediğimiz pencere. Kaydet'e basınca sp_BolumEkle çalışıyor.</summary>
public class BolumFormu : DiyalogFormu
{
    private readonly int _diziID;
    private readonly NumericUpDown _sezon = new() { Minimum = 1, Maximum = 99 };
    private readonly NumericUpDown _bolum = new() { Minimum = 1, Maximum = 999 };
    private readonly TextBox _ad = new();
    private readonly NumericUpDown _sure = new() { Minimum = 1, Maximum = 600, Value = 45 };

    public BolumFormu(int diziID, string diziAdi) : base($"Bölüm Ekle - {diziAdi}", 420, 250)
    {
        _diziID = diziID;
        Satir("Sezon no:", _sezon);
        Satir("Bölüm no:", _bolum);
        Satir("Bölüm adı: *", _ad);
        Satir("Süre (dakika):", _sure);

        // Kolaylık olsun diye son sezonun bir sonraki bölüm numarasını hazır yazıyoruz
        var son = Veritabani.Sorgu(
            "SELECT TOP (1) SezonNo, BolumNo FROM vw_DiziBolumleri WHERE DiziID = @id ORDER BY SezonNo DESC, BolumNo DESC",
            Veritabani.P("@id", diziID));
        if (son.Rows.Count == 1)
        {
            _sezon.Value = Convert.ToDecimal(son.Rows[0]["SezonNo"]);
            _bolum.Value = Convert.ToDecimal(son.Rows[0]["BolumNo"]) + 1;
        }
        _sezon.ValueChanged += (_, _) => SonrakiBolumNo();
        _ad.Text = $"{_bolum.Value}. Bölüm";
    }

    private void SonrakiBolumNo()
    {
        var t = Veritabani.Sorgu(
            "SELECT ISNULL(MAX(BolumNo), 0) + 1 AS Sonraki FROM vw_DiziBolumleri WHERE DiziID = @id AND SezonNo = @s",
            Veritabani.P("@id", _diziID), Veritabani.P("@s", (byte)_sezon.Value));
        _bolum.Value = Convert.ToDecimal(t.Rows[0]["Sonraki"]);
    }

    protected override bool Kaydet()
    {
        if (!Zorunlu(("Bölüm adı", _ad))) return false;
        Veritabani.Yordam("sp_BolumEkle",
            Veritabani.P("@DiziID", _diziID),
            Veritabani.P("@SezonNo", (byte)_sezon.Value),
            Veritabani.P("@BolumNo", (short)_bolum.Value),
            Veritabani.P("@BolumAdi", _ad.Text.Trim()),
            Veritabani.P("@SureDakika", (short)_sure.Value));
        return true;
    }
}
