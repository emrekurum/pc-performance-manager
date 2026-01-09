# Test Instructions

## Önemli Not
Uygulamayı test etmeden önce çalışan tüm PcPerformanceManager.exe process'lerini kapatmanız gerekiyor.

## Test Adımları

1. **Çalışan Process'leri Kapat:**
   - Task Manager'ı açın (Ctrl+Shift+Esc)
   - "Details" sekmesine gidin
   - "PcPerformanceManager.exe" process'lerini bulun
   - Sağ tıklayıp "End task" seçin

2. **Uygulamayı Derle:**
   ```powershell
   cd C:\PcOptimizer\PcPerformanceManager
   dotnet build
   ```

3. **Uygulamayı Çalıştır:**
   - Visual Studio'dan F5 ile çalıştırın, VEYA
   - Explorer'dan `bin\Debug\net8.0-windows\PcPerformanceManager.exe` dosyasına sağ tıklayıp "Run as administrator" seçin

4. **Test Senaryoları:**
   - Dashboard View'da sistem bilgileri görünüyor mu?
   - RAM View'da process listesi yükleniyor mu?
   - Power View'da güç planları listeleniyor mu?
   - Cleanup View'da analiz çalışıyor mu?

## Düzeltilen Sorunlar
- ✅ ProgressBar binding hataları düzeltildi (Mode=OneWay eklendi)
- ✅ Exception handling eklendi
- ✅ Resource dictionary'ler App.xaml içine taşındı






