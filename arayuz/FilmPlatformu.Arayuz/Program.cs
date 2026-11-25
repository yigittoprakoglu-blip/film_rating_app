namespace FilmPlatformu.Arayuz;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Bir yerde hata olursa program sessizce kapanmasın, en azından mesaj göstersin.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => HataGoster(e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) => HataGoster(e.ExceptionObject as Exception);

        // Program açılınca önce veritabanına bağlanabiliyor muyuz diye deniyoruz.
        // Sunucu adı Veritabani.cs dosyasında yazıyor (.\SQLEXPRESS / FilmPlatformuDB).
        try
        {
            Veritabani.BaglantiyiTestEt();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Veritabanına bağlanılamadı ({Veritabani.Sunucu} / {Veritabani.VeritabaniAdi}).\n\n" + ex.Message +
                "\n\nKontrol edin:\n• SQL Server (SQLEXPRESS) servisi çalışıyor mu?" +
                "\n• sql klasöründeki 01-07 script'leri SSMS'te çalıştırıldı mı?" +
                "\n• Sunucu adı farklıysa Veritabani.cs içindeki 'Sunucu' değerini değiştirin.",
                "Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            Application.Run(new AnaForm());
        }
        catch (Exception ex)
        {
            HataGoster(ex);
        }
    }

    private static void HataGoster(Exception? ex)
    {
        MessageBox.Show("Beklenmeyen bir hata oluştu:\n\n" + ex?.Message, "Hata",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
