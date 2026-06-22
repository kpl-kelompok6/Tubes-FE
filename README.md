# KPL-FE

Frontend Desktop UI untuk sistem POS (Point of Sale) Food & Beverage berbasis WPF (Windows Presentation Foundation).

## Overview

Aplikasi desktop kasir yang terintegrasi dengan backend `Tubes_POS_API` melalui HTTP REST dengan autentikasi JWT. Fitur-fitur utamanya mencakup:

- **Setup Backend URL** — Dialog pertama kali untuk mengonfigurasi URL backend, dengan auto-prepend `http://` jika skema tidak disertakan dan verifikasi koneksi via `/health/live`.
- **Login & Registrasi** — Autentikasi pengguna dengan dua mode (Login/Registrasi), validasi client-side, penyimpanan JWT token dan data pengguna (EmployeeId, DisplayName, Role) ke global state.
- **Menu CRUD** — Kelola menu dengan tampilan kartu, filter kategori (Makanan/Minuman), pencarian nama, preview gambar live, serta Create/Update/Delete dengan konfirmasi.
- **Transaksi 3 Kolom** — Layout kasir: daftar transaksi aktif (kiri), browser menu dengan filter kategori (tengah), keranjang belanja dengan kontrol kuantitas dan PPN breakdown (kanan).
- **Pembayaran** — Pilih metode bayar (Tunai/Debit/QRIS/Transfer), input jumlah dibayar dengan hitung kembalian live, overlay proses, dan layar sukses.
- **Riwayat Transaksi** — Riwayat dengan filter tanggal, status, pencarian kode transaksi, detail item, dan PPN breakdown.
- **Laporan (Admin)** — Rekap transaksi harian, ringkasan pendapatan per kategori, dengan breakdown PPN.
- **Pengaturan & Tema** — Toggle tema gelap/terang, informasi akun, logout, ganti URL backend.

## Tech Stack

- .NET 10
- WPF (Windows Presentation Foundation)
- ModernWpfUI 0.9.6 (Desain UI ala Windows 11)
- `HttpClient` + `AuthHandler` (Bearer JWT) untuk integrasi REST API
- `System.Text.Json` untuk serialisasi JSON
- `ToastService` & `DialogService` (ModernWpf ContentDialog)

## Modules

| Module      | What it covers                                                                                                           |
| ----------- | ------------------------------------------------------------------------------------------------------------------------ |
| Setup URL   | Dialog konfigurasi backend URL saat pertama kali jalan, auto-prepend `http://`, verifikasi via `/health/live`.            |
| Auth        | Halaman Login & Registrasi dengan dua mode, validasi client-side, JWT token & user state global.                         |
| Menu        | CRUD menu dengan tampilan kartu, filter kategori, pencarian, preview gambar live, loading/error/empty states.            |
| Transaction | Layout 3 kolom (daftar transaksi aktif, browser menu, keranjang), tambah/hapus/ubah item, batalkan transaksi.            |
| Payment     | Form pembayaran dengan 4 metode, hitung kembalian live, overlay loading, layar sukses, navigasi balik ke transaksi.      |
| History     | Riwayat transaksi dengan filter tanggal/status/pencarian, detail item dan PPN breakdown.                                 |
| Report      | Laporan harian (Admin) dengan ringkasan pendapatan per kategori dan breakdown PPN.                                       |
| Settings    | Informasi akun, toggle tema, ganti URL backend, logout.                                                                  |

## Project Structure

```text
Tubes-FE/
├── KPL-FE.sln
├── README.md
└── KPL-FE/
    ├── Controllers/         # Service HTTP Client (ApiClient, AuthApi, MenuApi, etc.)
    ├── Helpers/             # Utility helper (ErrorHelper, HealthChecker)
    ├── Models/              # Data Transfer Objects (DTO) untuk mapping JSON backend
    ├── Services/            # Layanan aplikasi (ToastService, DialogService)
    ├── ViewControllers/     # Controller untuk logika navigasi dan tampilan
    ├── Views/               # Halaman (.xaml) dan dialog UI
    │   ├── Auth/            # Login/Register page
    │   ├── Core/            # Shell navigasi (NavigationRootPage, MainWindow, SetupWindow)
    │   ├── Controls/        # Custom controls (ToastNotification)
    │   ├── History/         # Riwayat transaksi
    │   ├── Menu/            # CRUD menu + dialog
    │   ├── Payment/         # Pembayaran transaksi
    │   ├── Report/          # Laporan harian
    │   ├── Settings/        # Pengaturan akun, tema, URL backend
    │   └── Transaction/     # Halaman transaksi + dialog
    ├── App.xaml             # Entry point aplikasi WPF (startup, config, login)
    └── App.xaml.cs          # Global state (Token, Role, DisplayName, BaseUrl, Api)
```

## Setup

### Prerequisites

- .NET SDK 10
- Backend `Tubes_POS_API` harus sudah berjalan secara lokal (default: `http://localhost:5146`).

### Run

Aplikasi bisa dijalankan langsung melalui Visual Studio dengan menekan tombol **Start (F5)**, atau melalui terminal:

```bash
cd KPL-FE
dotnet run
```

### First Run

Saat pertama kali dijalankan, aplikasi akan:

1. **Setup Backend URL** — Masukkan URL backend (default `http://localhost:5146`). Jika skema (`http://` atau `https://`) tidak disertakan, `http://` akan ditambahkan secara otomatis. URL diverifikasi dengan mengecek endpoint `/health/live`.
2. **Login** — Masuk dengan akun yang terdaftar di backend. Jika belum punya akun, gunakan mode Registrasi.
3. **Main Window** — Tersambung ke backend dan siap digunakan.

### Screenshots

(dapat ditambahkan tangkapan layar)

## UI Notes

- Desain difokuskan pada fungsionalitas POS konvensional di layar sentuh atau monitor kasir (lebar/landscape).
- Komponen _dialog_ (ContentDialog) digunakan untuk aksi-aksi penting seperti membuat transaksi baru atau proses pembayaran agar tidak mengganggu aliran navigasi utama.
- Toast notification non-blocking untuk feedback operasi (sukses/error/info).
- Tema gelap/terang dapat diganti di halaman Settings.
- Sidebar navigasi dengan 5 halaman (Menu, Transaction, Payment, History, Settings) — History hanya untuk role Admin.

## Status

This project is for an academic final-semester POS frontend demo.
