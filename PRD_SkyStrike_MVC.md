# PRD: SkyStrike MVC - Game Pesawat Tempur C#

## 1. Ringkasan Proyek
**SkyStrike MVC** adalah sebuah game *arcade shooter* (Shoot 'em up) sederhana yang dibangun menggunakan bahasa pemrograman C#. Fokus utama dari proyek ini adalah mengimplementasikan arsitektur **Model-View-Controller (MVC)** untuk memisahkan logika permainan, manajemen data, dan rendering grafis.

## 2. Fitur Utama (MVP)
| Fitur | Deskripsi |
| :--- | :--- |
| **Kontrol Player** | Pesawat dapat bergerak ke kiri dan kanan menggunakan input keyboard (A/D atau Panah). |
| **Sistem Senjata** | Mekanisme menembak peluru secara linear dengan tombol *Space*. |
| **AI Musuh Sederhana** | Musuh (Pesawat musuh atau Asteroid) muncul dari atas layar dan bergerak turun dengan kecepatan bervariasi. |
| **Deteksi Tabrakan** | Pengecekan overlap antara peluru-musuh (poin) dan musuh-pemain (mengurangi nyawa). |
| **HUD & Skor** | Tampilan informasi skor, nyawa, dan status Game Over secara real-time. |

## 3. Arsitektur MVC
Proyek ini akan dibagi menjadi tiga lapisan utama untuk memastikan kode bersifat modular dan mudah diuji.

### A. Model (Logika & Data)
Bagian ini murni berisi variabel data dan perhitungan matematika tanpa ketergantungan pada UI.
- **`Entity.cs`**: *Base class* untuk semua objek bergerak (posisi X/Y, kecepatan, dimensi).
- **`Player.cs`**: Menyimpan status pesawat pemain (HP, Level Senjata).
- **`EnemyManager.cs`**: Mengelola daftar musuh yang aktif dan logika *spawning*.
- **`Physics.cs`**: Berisi algoritma deteksi tabrakan (*Axis-Aligned Bounding Box* / AABB).

### B. View (Rendering & UI)
Bagian ini bertanggung jawab untuk menampilkan data dari Model ke layar.
- **`GameRenderer.cs`**: Menangani proses menggambar sprite (gambar .png) ke kanvas berdasarkan koordinat dari Model.
- **`UIScreen.cs`**: Menampilkan teks skor, tombol restart, dan animasi transisi.

### C. Controller (Input & Engine)
Bertindak sebagai jembatan yang mengatur aliran data.
- **`InputController.cs`**: Menangkap input keyboard dan mengirimkan perintah aksi ke Model.
- **`GameEngine.cs`**: Berisi *Game Loop* (Timer) yang memerintahkan Model untuk update posisi dan memerintahkan View untuk menggambar ulang.

## 4. Spesifikasi Teknis
- **Bahasa**: C# 10+
- **Framework**: .NET 6.0/8.0 (Windows Forms untuk UI cepat atau SkiaSharp untuk rendering yang lebih halus).
- **Pola Desain**: MVC (Model-View-Controller) & Singleton untuk Game Manager.
- **Aset**: Gambar format `.png` transparan (32-bit) untuk menghindari kotak latar belakang.

## 5. Alur Kerja (E2E Logic)
1. **Input**: User menekan tombol tembak.
2. **Controller**: Menerima input, memvalidasi jeda tembak (*cooldown*), dan memerintahkan Model untuk membuat objek peluru baru.
3. **Model**: Menambahkan koordinat peluru ke dalam daftar `ActiveBullets`.
4. **Physics (Model)**: Menghitung pergerakan peluru ke atas setiap frame.
5. **View**: Mengambil koordinat terbaru dari daftar peluru dan menggambar setiap peluru di layar.

## 6. Rencana Pengembangan (Roadmap)
1. **Fase 1 (Struktur)**: Inisialisasi proyek dan pembuatan folder M-V-C.
2. **Fase 2 (Gerakan)**: Implementasi pergerakan pemain dan pembatasan layar (Boundary).
3. **Fase 3 (Mekanisme)**: Implementasi sistem peluru dan musuh (Spawning logic).
4. **Fase 4 (Fisika)**: Implementasi deteksi tabrakan dan sistem skor.
5. **Fase 5 (UI/UX)**: Penggantian bentuk primitif (kotak) dengan aset gambar asli dan penambahan suara (opsional).

---
*Dokumen ini disusun untuk memberikan panduan teknis bagi pengembang dalam membangun aplikasi game berbasis objek yang terstruktur.*
