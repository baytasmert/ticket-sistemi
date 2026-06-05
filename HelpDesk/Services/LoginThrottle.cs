using System.Collections.Concurrent;

namespace HelpDesk.Services
{
    /// <summary>
    /// IP (veya başka bir anahtar) başına başarısız giriş denemelerini izleyen,
    /// thread-safe (eşzamanlı isteklere güvenli) hız sınırlayıcı. Hem müşteri
    /// (Account) hem personel (Staff) girişinde paylaşılan TEK kaynaktır ve DI'da
    /// singleton kaydedilir.
    ///
    /// Önceki tasarımda her controller'da ayrı, kilitsiz <c>static Dictionary</c>
    /// vardı: eşzamanlı isteklerde race condition (çökme/bozuk sayım) ve hiç
    /// temizlenmediği için sınırsız bellek büyümesi riski taşıyordu.
    /// </summary>
    public class LoginThrottle
    {
        private readonly ConcurrentDictionary<string, Entry> _entries = new();
        private int _opCount;

        public int MaxAttempts { get; }
        public TimeSpan LockoutDuration { get; }

        public LoginThrottle(int maxAttempts = 5, int lockoutMinutes = 15)
        {
            MaxAttempts = maxAttempts;
            LockoutDuration = TimeSpan.FromMinutes(lockoutMinutes);
        }

        private sealed class Entry
        {
            public int Attempts;
            public DateTime LastAttemptUtc;
        }

        /// <summary>Anahtar kilitliyse kalan süreyi, kilitli değilse <c>null</c> döndürür.</summary>
        public TimeSpan? GetLockoutRemaining(string key)
        {
            if (_entries.TryGetValue(key, out var e))
            {
                var elapsed = DateTime.UtcNow - e.LastAttemptUtc;
                if (elapsed >= LockoutDuration)
                {
                    _entries.TryRemove(key, out _); // pencere doldu → sıfırla
                    return null;
                }
                if (e.Attempts >= MaxAttempts)
                    return LockoutDuration - elapsed;
            }
            return null;
        }

        /// <summary>
        /// Başarısız bir denemeyi kaydeder. Yalnızca kilitli DEĞİLken çağrılmalı
        /// (çağıran önce <see cref="GetLockoutRemaining"/> ile kontrol eder); böylece
        /// kilit anı (LastAttemptUtc) donar ve süre doğru biçimde dolar.
        /// </summary>
        public void RegisterFailure(string key)
        {
            _entries.AddOrUpdate(key,
                _ => new Entry { Attempts = 1, LastAttemptUtc = DateTime.UtcNow },
                (_, e) =>
                {
                    // Kilit penceresi dolduysa sayaç baştan başlar.
                    e.Attempts = (DateTime.UtcNow - e.LastAttemptUtc >= LockoutDuration) ? 1 : e.Attempts + 1;
                    e.LastAttemptUtc = DateTime.UtcNow;
                    return e;
                });
            SweepIfNeeded();
        }

        /// <summary>Başarılı giriş sonrası sayaç sıfırlanır.</summary>
        public void Reset(string key) => _entries.TryRemove(key, out _);

        // Süresi geçmiş (terk edilmiş) kayıtları periyodik olarak temizler; böylece
        // sözlük sınırsız büyümez. Her 100 işlemde bir taranır (ucuz amortisman).
        private void SweepIfNeeded()
        {
            if (Interlocked.Increment(ref _opCount) % 100 != 0) return;
            var now = DateTime.UtcNow;
            foreach (var kv in _entries)
                if (now - kv.Value.LastAttemptUtc >= LockoutDuration)
                    _entries.TryRemove(kv.Key, out _);
        }
    }
}
