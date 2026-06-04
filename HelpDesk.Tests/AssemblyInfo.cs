using Xunit;

// Test sınıfları kendi izole SQLite veritabanlarını kullansa da, SQLite dosya
// erişiminde sürpriz kilitlenmeleri tamamen elemek için testleri seri çalıştırıyoruz.
// Test sayısı az ve hızlı olduğundan bu güvenli ve deterministik bir tercihtir.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
