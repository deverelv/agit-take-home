## 1. Self-review
* **Authentication disimulasikan**:
  * Severity: Low
  * Status: Resolved
  * Action: Menggunakan header HTTP (X-User-Email) dan role yang tersimpan di database.
  * Evidence: RequestAccessController.cs, RequestAccessService.cs
* **Idempotency pada `ClientRequestId`**:
  * Severity: High
  * Status: Resolved
  * Action: Penanganan melalui query `ClientRequestId` di service layer.
  * Evidence: RequestAccessService.cs
* **Optimistic concurrency pada approval/rejection**:
  * Severity: Medium
  * Status: Resolved
  * Action: Menggunakan properti Version sebagai token konkurensi serta menangkap DbUpdateConcurrencyException.
  * Evidence: AccessRequest.cs, RequestAccessService.cs
* **Audit log harus atomik dengan perubahan status**:
  * Severity: Medium
  * Status: Resolved
  * Action: Menggunakan hanya satu `.SaveChangesAsync()` dalam satu flow.
  * Evidence: RequestAccessService.cs
* **Rejection membutuhkan alasan**:
  * Severity: Medium
  * Status: Not Resolved
  * Action: Menambahkan validasi server-side `string.IsNullOrWhiteSpace(dto.Reason)` pada function `RejectRequestAsync`
  * Evidence: RequestAccessService.cs
* **Requester tidak boleh melakukan approval terhadap request sendiri**:
  * Severity: High
  * Status: Resolved
  * Action: Menambahkan pengecekan server-side `request.RequesterEmail == actor.Email`
  * Evidence: AccessRequestService.cs
* **Manager hanya dapat melakukan approval terhadap direct report**:
  * Severity: High
  * Status: Resolved
  * Action: Memvalidasi relasi `request.Requester?.ManagerEmail == actor.Email` saat status `PendingManager`.
  * Evidence: AccessRequestService.cs
* **System Owner hanya dapat melakukan approval untuk application yang dimilikinya**:
  * Severity: High
  * Status: Resolved
  * Action: Memvalidasi relasi `request.Application?.SystemOwnerEmail == actor.Email` saat status `PendingSystemOwner`.
  * Evidence: AccessRequestService.cs
* **Request yang sudah berada pada terminal state tidak dapat diubah**:
  * Severity: High
  * Status: Resolved
  * Action: Memvalidasi `request.Status == "Approved" || request.Status == "Rejected"`
  * Evidence: AccessRequestService.cs
* **High-risk access harus melalui System Owner**:
  * Severity: High
  * Status: Resolved
  * Action: Mengarahkan alur ke status `PendingSystemOwner` jika kondisi high-risk terpenuhi setelah manager approve.
  * Evidence: AccessRequestService.cs
* **Menggunakan seeded data**:
  * Severity: Low
  * Status: Resolved 
  * Action: Dilakukan menggunakan `.HasData`
  * Evidence: AppDbContext.cs


## 2. Known Limitations
* Simulasi autentikasi
* Skalabilitas concurrency
* Skalabilitas seeder

## 3. Deferred Work
* Mengubah static values menjadi Enum
* Halaman request list, request detail, dan approval inbox.
* API untuk fetch data: users, applicatons, dan request detail.
