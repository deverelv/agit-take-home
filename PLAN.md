# PLAN.md: Access Request Hub (MVP Phase 1)

## 1. Problem Understanding
Perusahaan saat ini masih mengelola permintaan akses aplikasi internal melalui email atau chat. Proses ini memiliki berbagai kendala kritis:
* Sulit dilacak dan tidak memiliki *single source of truth*.
* Rentan terhadap pengiriman permintaan ganda (*duplicate request*).
* Risiko *conflict approval* ketika dua approver bertindak pada waktu yang berdekatan.
* Belum adanya sistem otomatis untuk menangani aturan *high-risk* (Production atau Admin) serta validasi otorisasi server-side secara konsisten.

## 2. Architecture & Data Model Ringkas
* **Arsitektur**: Monolith (Backend API + Database relasional + Frontend ringan dengan *user switcher* simulasi).
* **Data Model (Relational Schema)**:
  * `users`: Menyimpan daftar demo user (`alice@example.local`, `bob@example.local`, `carol@example.local`, `dana@example.local`, `erin@example.local`) beserta role dan relasi atasan (*manager*).
  * `applications`: Menyimpan data aplikasi (`CRM`, `Finance Portal`) beserta penanggung jawab (*system owner*).
  * `access_requests`: Menyimpan data utama permohonan, dilengkapi dengan kolom `client_request_id` (untuk *idempotency*), status transisi, `policy_version` (`v1`), dan `version` (sebagai *concurrency token*).
  * `audit_logs`: Tabel *append-only* untuk mencatat setiap perubahan status kritis secara atomik.

## 3. Implementation Order
1. **Database & Seed**: Inisialisasi skema database relasional (`users`, `applications`, `access_requests`, `audit_logs`) beserta *seed data* demo user.
2. **Backend Core**: Implementasi logika backend inti:
   * Endpoint pembuatan request dengan proteksi *idempotency* (`ClientRequestId`).
   * *State machine* untuk transisi status dan kalkulasi *high-risk* (`Environment = Production` ATAU `AccessLevel = Admin`).
   * Mekanisme *optimistic concurrency* (`version`) untuk proses *approve/reject*.
   * Penegakan otorisasi ketat di sisi server (pencegahan *self-approval*, validasi manager direct report, dan system owner aplikasi).
   * Pencatatan *audit trail* atomik.
3. **Frontend**: Pembuatan UI frontend sederhana (User Switcher, Form Create Request, My Requests, dan Approval Inbox).
4. **Automated Tests**: Penulisan dan eksekusi *automated tests* untuk seluruh skenario wajib.
5. **Documentation**: Finalisasi dokumentasi dan penerapan Git tags (`assessment-start` & `phase-1-complete`).

## 4. Test Strategy
Pengujian otomatis dirancang untuk membuktikan seluruh kriteria skenario wajib:
* **Standard Request**: Alice mengajukan CRM, NonProduction, Read. Bob (Manager) meng-approve -> Status langsung `Approved`.
* **Production / High-Risk Request**: Alice mengajukan CRM, Production, Read. Bob approve -> Menunggu Carol (System Owner); Carol approve -> Status `Approved`.
* **Admin Access Request**: Alice mengajukan Finance Portal, NonProduction, Admin. Bob approve -> Menunggu Dana (System Owner).
* **Unauthorized Approval**: User yang tidak memiliki hak akses mencoba memanggil endpoint *approve/reject* -> Ditolak oleh backend.
* **Duplicate Submit (Idempotency)**: Pengiriman `ClientRequestId` yang sama secara bersamaan -> Hanya menghasilkan satu row data bisnis.
* **Concurrent Action / Stale Conflict**: Dua aksi bersamaan pada *version* request yang sama -> Hanya satu yang berhasil, lainnya menerima *conflict error* yang jelas.
* **Rejected Request**: Penolakan wajib menyertakan alasan (*reason*), tersimpan di audit event, dan request terkunci sebagai status terminal.

## 5. Trade-Off Penting
1. **Optimistic vs Pessimistic Locking**: 
   * Dipilih *Optimistic* menggunakan kolom *version* karena bentrok pada approval aplikasi internal jarang terjadi, sehingga performa lebih ringan dan cocok untuk MVP.
2. **Autentikasi Simulasi vs Kompleks**:
   * Menggunakan *user switcher* berbasis header/session lokal sesuai spesifikasi asesmen agar fokus penilaian tetap tertuju pada ketepatan aturan bisnis dan *authorization backend*.

## 6. Perubahan Plan Selama Implementasi
* Belum ada perubahan terhadap implementation plan pada tahap awal.