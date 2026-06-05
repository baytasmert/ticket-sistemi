namespace HelpDesk.Models
{
    public enum TicketDurumu
    {
        Açık = 0,
        İşlemde = 1,
        Çözüldü = 2,
        Kapatıldı = 3
    }

    public enum TicketOnceligi
    {
        Düşük = 0,
        Orta = 1,
        Yüksek = 2,
        Kritik = 3
    }

    // Talep üzerinde yapılan işlemlerin türü. Her işlem TicketHistory olarak
    // kaydedilir; böylece talebin tam bir denetim/etkinlik geçmişi oluşur.
    public enum TicketIslemTuru
    {
        Olusturuldu = 0,
        DurumDegisti = 1,
        Atandi = 2,
        YanitEklendi = 3,
        DahiliNot = 4,
        Kapatildi = 5,
        YenidenAcildi = 6
    }
}
