# KPL-FE

Frontend Desktop UI untuk sistem POS (Point of Sale) Food & Beverage berbasis WPF (Windows Presentation Foundation).

## Overview

Project ini adalah antarmuka kasir yang terintegrasi langsung dengan backend `Tubes_POS_API` melalui HTTP REST. Fitur-fitur utamanya mencakup:

- sinkronisasi menu secara realtime
- sistem keranjang belanja (cart) pada transaksi
- integrasi sistem pembayaran
- tampilan riwayat transaksi

## Tech Stack

- .NET 10
- WPF (Windows Presentation Foundation)
- ModernWpfUI (Desain UI ala Windows 11)
- `HttpClient` untuk integrasi REST API

## Current Modules

| Module      | What it covers                                                                                                           |
| ----------- | ------------------------------------------------------------------------------------------------------------------------ |
| Main Layout | Navigasi utama antara halaman Transaksi, Menu, dan Riwayat menggunakan `NavigationView`.                                 |
| Transaction | Layout 3 kolom (List Transaksi Aktif, Browser Menu dengan Filter, Keranjang Belanja), penambahan/pengurangan _quantity_. |
| Payment     | (Work in Progress) Dialog pembayaran dan integrasi API untuk menyelesaikan transaksi.                                    |

## Project Structure

```text
Tubes-FE/
├── KPL-FE.sln
├── README.md
└── KPL-FE/
    ├── Controllers/        # Service HTTP Client untuk memanggil backend
    ├── Models/             # Data Transfer Objects (DTO) untuk mapping JSON backend
    ├── Views/              # Halaman (.xaml) dan dialog UI
    ├── App.xaml            # Entry point aplikasi WPF
    ├── MainWindow.xaml     # Shell navigasi utama
    └── KPL-FE.csproj
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

## UI Notes

- Desain difokuskan pada fungsionalitas POS konvensional di layar sentuh atau monitor kasir (lebar/landscape).
- Komponen _dialog_ (modal) digunakan untuk aksi-aksi penting seperti membuat transaksi baru atau proses pembayaran agar tidak mengganggu aliran navigasi utama.

## Status

This project is for an academic final-semester POS frontend demo.
