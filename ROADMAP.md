# PC Performance Manager - Geliştirme Roadmap

## 📋 Genel Bakış
Bu doküman, PC Performance Manager uygulamasının adım adım geliştirme planını içermektedir.

---

## ✅ Tamamlanan Adımlar

### 1. Proje Altyapısı
- ✅ .NET 8.0 WPF projesi oluşturuldu
- ✅ MVVM mimarisi kuruldu (CommunityToolkit.Mvvm)
- ✅ Klasör yapısı düzenlendi (Views, ViewModels, Models, Services, Helpers)
- ✅ app.manifest dosyası eklendi (Yönetici izinleri yapılandırıldı)
- ✅ Temel MainWindow ve MainViewModel oluşturuldu
- ✅ Navigasyon menüsü ve içerik alanı hazırlandı

---

## 🚀 Geliştirme Planı (Sırasıyla)

### Faz 1: Servis Katmanı ve Yardımcı Sınıflar

#### 1.1 Helpers Klasörü
- [ ] **AdminHelper.cs**: Yönetici izinlerini kontrol eden yardımcı sınıf
- [ ] **SystemInfoHelper.cs**: Sistem bilgilerini toplayan yardımcı sınıf
- [ ] **Logger.cs**: Loglama mekanizması (opsiyonel)

#### 1.2 Models Klasörü
- [ ] **SystemInfo.cs**: Sistem bilgileri modeli (RAM, CPU, Disk)
- [ ] **PowerPlan.cs**: Güç planı modeli
- [ ] **CleanupItem.cs**: Temizlenecek dosya/klasör modeli
- [ ] **MemoryInfo.cs**: RAM kullanım bilgileri modeli

#### 1.3 Services Klasörü
- [ ] **IMemoryService.cs / MemoryService.cs**: RAM yönetimi servisi
  - RAM kullanım bilgilerini alma
  - RAM temizleme işlemleri
  - Working set temizleme
  
- [ ] **IPowerService.cs / PowerService.cs**: Güç yönetimi servisi
  - Aktif güç planını alma/değiştirme
  - Güç planlarını listeleme
  - Güç ayarlarını yapılandırma
  
- [ ] **ICleanupService.cs / CleanupService.cs**: Dosya temizleme servisi
  - Geçici dosyaları bulma
  - Disk alanı hesaplama
  - Dosya/klasör silme işlemleri
  - Güvenli silme doğrulaması

---

### Faz 2: ViewModel Geliştirmeleri

#### 2.1 DashboardViewModel
- [ ] Sistem özet bilgileri (RAM, CPU, Disk kullanımı)
- [ ] Gerçek zamanlı performans grafikleri/göstergeleri
- [ ] Hızlı aksiyonlar (Hızlı RAM temizleme, vb.)

#### 2.2 RamViewModel
- [ ] RAM kullanım istatistikleri (Toplam, Kullanılan, Boş)
- [ ] Süreç listesi ve RAM kullanımları
- [ ] RAM temizleme butonu ve komutları
- [ ] Otomatik RAM temizleme seçenekleri

#### 2.3 PowerViewModel
- [ ] Mevcut güç planını gösterme
- [ ] Güç planlarını listeleme
- [ ] Güç planı değiştirme
- [ ] Güç ayarları yapılandırma (CPU, Ekran, vb.)

#### 2.4 CleanupViewModel
- [ ] Temizlenecek dosya türlerini listeleme
- [ ] Disk alanı analizi
- [ ] Seçili öğeleri temizleme
- [ ] Temizleme özeti ve sonuçları

---

### Faz 3: View (UI) Geliştirmeleri

#### 3.1 DashboardView
- [ ] Sistem bilgileri kartları
- [ ] Performans grafikleri (ProgressBar veya Chart)
- [ ] Hızlı erişim butonları
- [ ] Modern ve kullanıcı dostu tasarım

#### 3.2 RamView
- [ ] RAM kullanım göstergesi (ProgressBar, Circular Progress)
- [ ] Süreç listesi (DataGrid)
- [ ] RAM temizleme butonları
- [ ] Ayarlar paneli

#### 3.3 PowerView
- [ ] Güç planı listesi (ListBox/ComboBox)
- [ ] Güç ayarları formu
- [ ] Güç planı değiştirme butonları
- [ ] Bilgilendirme mesajları

#### 3.4 CleanupView
- [ ] Temizleme kategorileri (CheckBox listesi)
- [ ] Disk alanı gösterimi
- [ ] Analiz ve Temizle butonları
- [ ] İlerleme çubuğu (ProgressBar)
- [ ] Sonuç özeti

---

### Faz 4: Gelişmiş Özellikler

#### 4.1 Bildirimler ve Uyarılar
- [ ] Toast bildirimleri (RAM yüksek kullanım uyarısı)
- [ ] Kullanıcı onay dialogları (kritik işlemler için)
- [ ] Hata mesajları ve exception handling

#### 4.2 Ayarlar
- [ ] SettingsViewModel ve SettingsView
- [ ] Uygulama ayarları (Otomatik başlangıç, vb.)
- [ ] Tema seçenekleri (opsiyonel)

#### 4.3 Performans İzleme
- [ ] Gerçek zamanlı sistem izleme
- [ ] Timer/DispatcherTimer ile periyodik güncellemeler
- [ ] Sistem kaynaklarının düşük kullanımı

---

### Faz 5: Test ve Optimizasyon

#### 5.1 Kod Kalitesi
- [ ] Exception handling ve error logging
- [ ] Kod yorumları ve dokümantasyon
- [ ] Code review ve refactoring

#### 5.2 Performans Optimizasyonu
- [ ] UI thread blocking önleme (async/await)
- [ ] Bellek sızıntılarını önleme
- [ ] Servis katmanında caching

#### 5.3 Kullanıcı Deneyimi
- [ ] Loading göstergeleri
- [ ] Kullanıcı geri bildirimleri
- [ ] Keyboard shortcuts (opsiyonel)

---

## 🔧 Teknik Notlar

### Yönetici İzinleri
- Uygulama yönetici izinleri gerektirir (app.manifest)
- Uygulama çalıştırıldığında UAC (User Account Control) onayı ister

### MVVM Deseni
- ViewModels: CommunityToolkit.Mvvm kullanıyor
- ObservableObject, ObservableProperty, RelayCommand
- View'lar ViewModel'lere DataBinding ile bağlı

### Servis Tasarımı
- Servisler interface'ler üzerinden tanımlanmalı (Dependency Injection için hazırlık)
- Servisler test edilebilir olmalı
- Windows API çağrıları servislerde encapsulate edilmeli

---

## 📝 Notlar
- Her faz tamamlandıktan sonra test edilmeli
- Gerektiğinde önceki fazlara geri dönüş yapılabilir
- Kullanıcı geri bildirimlerine göre öncelikler değişebilir




