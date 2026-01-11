# Known Bugs and Issues

## Disk Analyzer - Büyük Dosyaları Bul

**Status**: Pending Investigation  
**Date**: 2025-01-XX  
**Priority**: Medium

### Description
"Büyük Dosyaları Bul" özelliği çalışmıyor. Kullanıcı butona tıkladığında sonuç görünmüyor.

### Steps to Reproduce
1. Disk Analizi sayfasına git
2. "Büyük Dosyalar" sekmesini seç
3. Bir disk seç (örn: C:)
4. Minimum dosya boyutu ayarla (örn: 100 MB)
5. "Büyük Dosyaları Bul" butonuna tıkla
6. Sonuç görünmüyor

### Expected Behavior
Büyük dosyalar listelenmeli ve DataGrid'de görüntülenmeli.

### Actual Behavior
Sonuç görünmüyor, liste boş kalıyor.

### Technical Details
- `DiskAnalyzerService.FindLargeFilesAsync()` metodu çağrılıyor
- Debug log'lar eklendi ama henüz test edilmedi
- `FindLargeFilesRecursive()` metodu recursive olarak dosyaları arıyor
- DriveLetter formatı kontrol edilmeli (örn: "C:" vs "C:\")

### Potential Issues
1. DriveLetter formatı yanlış olabilir (trailing backslash eksik/fazla)
2. Recursive search çok yavaş olabilir ve timeout olabilir
3. UI thread'de güncelleme sorunu olabilir
4. Exception yakalanıyor ama kullanıcıya gösterilmiyor olabilir

### Next Steps
1. Debug log'ları kontrol et
2. DriveLetter formatını doğrula
3. Exception handling'i iyileştir
4. Progress indicator ekle (uzun süren işlemler için)
5. Test et ve sonuçları analiz et
