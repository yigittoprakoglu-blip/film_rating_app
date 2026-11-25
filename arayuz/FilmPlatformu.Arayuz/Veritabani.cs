using System.Data;
using Microsoft.Data.SqlClient;

namespace FilmPlatformu.Arayuz;

/// <summary>
/// Veritabanıyla konuşan bütün kodlar bu sınıfta. Diğer dosyalar SQL Server'a
/// doğrudan bağlanmıyor, buradaki metotları çağırıyor.
/// Sorgulara değerleri metin olarak eklemek yerine parametre (@id gibi) kullandık,
/// böylece SQL Injection yapılamıyor.
/// </summary>
public static class Veritabani
{
    // Bağlantı ayarları. Başka bir bilgisayarda sunucu adı farklıysa sadece burayı değiştirmek yeterli.
    public const string Sunucu = @".\SQLEXPRESS";
    public const string VeritabaniAdi = "FilmPlatformuDB";

    /// <summary>Bağlantı cümlesi (connection string). Windows hesabıyla giriş yapıyoruz, şifre gerekmiyor.</summary>
    public static string BaglantiCumlesi => new SqlConnectionStringBuilder
    {
        DataSource = Sunucu,
        InitialCatalog = VeritabaniAdi,
        IntegratedSecurity = true,
        TrustServerCertificate = true,
        ConnectTimeout = 5
    }.ConnectionString;

    public static SqlConnection YeniBaglanti() => new(BaglantiCumlesi);

    /// <summary>Program açılırken bağlantıyı deniyor, bağlanamazsa hata veriyor.</summary>
    public static void BaglantiyiTestEt()
    {
        using var baglanti = YeniBaglanti();
        baglanti.Open();
    }

    /// <summary>Bir SQL sorgusu çalıştırıp sonucu tablo (DataTable) olarak geri veriyor.</summary>
    public static DataTable Sorgu(string sql, params SqlParameter[] parametreler)
    {
        using var baglanti = YeniBaglanti();
        using var komut = new SqlCommand(sql, baglanti);
        komut.Parameters.AddRange(parametreler);
        using var adaptor = new SqlDataAdapter(komut);
        var tablo = new DataTable();
        adaptor.Fill(tablo);
        return tablo;
    }

    /// <summary>Saklı yordam çalıştırıyor (CommandType.StoredProcedure ile) ve sonucu tablo olarak veriyor.</summary>
    public static DataTable Yordam(string yordamAdi, params SqlParameter[] parametreler)
    {
        using var baglanti = YeniBaglanti();
        using var komut = new SqlCommand(yordamAdi, baglanti) { CommandType = CommandType.StoredProcedure };
        komut.Parameters.AddRange(parametreler);
        using var adaptor = new SqlDataAdapter(komut);
        var tablo = new DataTable();
        adaptor.Fill(tablo);
        return tablo;
    }

    /// <summary>Parametre yazmayı kısaltmak için. C#'taki null'ı SQL'in anlayacağı DBNull'a çeviriyor.</summary>
    public static SqlParameter P(string ad, object? deger) => new(ad, deger ?? DBNull.Value);
}
