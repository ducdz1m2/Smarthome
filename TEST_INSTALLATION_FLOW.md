# Kịch Bản Test Luồng Lắp Đặt/Bảo Hành

## 1. Test Luồng Admin Tạo Lịch Lắp Đặt

### Bước 1: Tạo đơn hàng cần lắp đặt
1. Đăng nhập với tài khoản Admin
2. Tạo đơn hàng mới với sản phẩm cần lắp đặt (RequiresInstallation = true)
3. Nhập thông tin giao hàng:
   - Họ tên khách hàng
   - SĐT
   - Địa chỉ: Số nhà, đường, Ward, District, City
   - Ví dụ: "123 Đường ABC, Ward 1, Quận 1, TP.HCM"
4. Xác nhận đơn hàng

### Bước 2: Mở OrderDialog để tạo lịch lắp đặt
1. Vào danh sách đơn hàng
2. Tìm đơn hàng vừa tạo (trạng thái Pending)
3. Mở OrderDialog
4. **Kiểm tra**: Section "Lắp đặt/Bảo hành" hiển thị
5. **Kiểm tra**: Hiển thị alert "Đơn hàng này cần lắp đặt nhưng chưa có lịch"
6. Click nút "Tạo lịch lắp đặt"

### Bước 3: Chọn kỹ thuật viên và lịch
1. **Kiểm tra**: InstallationBookingDialog mở với filter theo vị trí khách hàng
2. **Kiểm tra**: Dropdown kỹ thuật viên chỉ hiển thị technicians ở Quận 1, TP.HCM
3. Chọn ngày lắp đặt (ví dụ: ngày mai)
4. Chọn kỹ thuật viên
5. **Kiểm tra**: Dropdown slot hiển thị các slot trống cho ngày đó (8:00-10:00, 10:00-12:00, 14:00-16:00, 16:00-18:00)
6. Chọn slot (ví dụ: 8:00-10:00)
7. Click "Lưu"
8. **Kiểm tra**: Toast "Đã tạo lịch lắp đặt mới" hiển thị
9. **Kiểm tra**: OrderDialog cập nhật, hiển thị thông tin lịch lắp đặt

---

## 2. Test Luồng Kỹ Thuật Viên Tiếp Nhận Lịch

### Bước 1: Kỹ thuật viên xem danh sách lịch
1. Đăng nhập với tài khoản Technician
2. Vào trang "Danh sách lịch"
3. **Kiểm tra**: Lịch vừa tạo hiển thị với trạng thái "Đã phân công"

### Bước 2: Kỹ thuật viên mở chi tiết lịch
1. Click vào lịch vừa tạo
2. **Kiểm tra**: BookingDetail.razor hiển thị thông tin đầy đủ:
   - Thông tin khách hàng
   - Thông tin lịch hẹn (ngày, khung giờ)
   - Sản phẩm cần lắp
3. **Kiểm tra**: Hiển thị alert "Lịch này đang chờ bạn xác nhận"
4. **Kiểm tra**: Nút "Tiếp nhận lịch" và "Từ chối lịch" hiển thị

### Bước 3: Kỹ thuật viên tiếp nhận lịch
1. Click nút "Tiếp nhận lịch"
2. **Kiểm tra**: Status chuyển sang "Đã xác nhận"
3. **Kiểm tra**: Nút "Chuẩn bị vật tư từ kho" hiển thị

---

## 3. Test Luồng Kỹ Thuật Viên Thực Hiện Lắp Đặt

### Bước 1: Chuẩn bị vật tư
1. Click nút "Chuẩn bị vật tư từ kho"
2. Dialog mở, hiển thị danh sách sản phẩm cần lắp
3. Chọn kho và số lượng vật tư
4. Click "Xác nhận"
5. **Kiểm tra**: Status chuyển sang "Đang chuẩn bị"
6. **Kiểm tra**: Hiển thị nút "Bắt đầu di chuyển"

### Bước 2: Bắt đầu di chuyển
1. Click nút "Bắt đầu di chuyển"
2. **Kiểm tra validation**: Nếu chưa đến ngày hẹn (trước 1 ngày), hiển thị lỗi
3. Chờ đến ngày hẹn hoặc trước 1 ngày
4. Click lại nút "Bắt đầu di chuyển"
5. **Kiểm tra**: Status chuyển sang "Đang di chuyển"
6. **Kiểm tra**: Hiển thị nút "Bắt đầu lắp đặt"

### Bước 3: Bắt đầu lắp đặt
1. Click nút "Bắt đầu lắp đặt"
2. **Kiểm tra validation**: Nếu chưa đến ngày hẹn, hiển thị lỗi
3. Chờ đến ngày hẹn
4. Click lại nút "Bắt đầu lắp đặt"
5. **Kiểm tra**: Status chuyển sang "Đang lắp đặt"
6. **Kiểm tra**: Dialog hoàn thành hiển thị

### Bước 4: Hoàn thành lắp đặt
1. Nhập chữ ký khách hàng
2. Đánh giá (1-5 sao)
3. Nhập ghi chú (nếu có)
4. Click "Hoàn thành"
5. **Kiểm tra**: Status chuyển sang "Đã hoàn thành"
6. **Kiểm tra**: Đơn hàng cập nhật trạng thái

---

## 4. Test Luồng Kỹ Thuật Viên Từ Chối Lịch (Auto-Reassign)

### Bước 1: Tạo lịch lắp đặt mới
1. Admin tạo đơn hàng mới cần lắp đặt
2. Admin tạo lịch lắp đặt cho Technician A

### Bước 2: Technician A từ chối lịch
1. Đăng nhập với Technician A
2. Mở chi tiết lịch
3. Click nút "Từ chối lịch"
4. Nhập lý do từ chối
5. Click "Xác nhận"
6. **Kiểm tra**: Lịch chuyển sang trạng thái "Đã phân công" cho Technician B (nếu có)
7. **Kiểm tra**: Console log hiển thị thông tin reassign

### Bước 3: Technician B nhận lịch
1. Đăng nhập với Technician B
2. **Kiểm tra**: Lịch hiển thị trong danh sách của Technician B
3. Tiếp nhận và thực hiện như luồng bình thường

### Bước 4: Test trường hợp không có technician khác
1. Tạo lịch lắp đặt cho Technician A ở khu vực không có technician nào khác
2. Technician A từ chối lịch
3. **Kiểm tra**: Lịch chuyển sang trạng thái "Đã hủy"
4. **Kiểm tra**: Console log hiển thị "No available technicians or slots found"

---

## 5. Test Validation Thời Gian

### Test Accept - Quá hạn
1. Tạo lịch lắp đặt cho ngày quá khứ (2 ngày trước)
2. Technician thử accept
3. **Kiểm tra**: Hiển thị lỗi "Không thể tiếp nhận lịch đã quá hạn"

### Test StartTravel - Quá sớm
1. Tạo lịch lắp đặt cho ngày sau 3 ngày
2. Technician accept → prepare materials
3. Technician thử start travel
4. **Kiểm tra**: Hiển thị lỗi "Chỉ có thể bắt đầu di chuyển trong vòng 1 ngày trước ngày hẹn"

### Test StartInstallation - Quá sớm
1. Technician start travel
2. Technician thử start installation trước ngày hẹn
3. **Kiểm tra**: Hiển thị lỗi "Chỉ có thể bắt đầu lắp đặt vào ngày hẹn hoặc sau đó"

---

## 6. Test Luồng Bảo Hành

### Bước 1: Tạo yêu cầu bảo hành
1. Đăng nhập với Customer
2. Tạo yêu cầu bảo hành cho đơn hàng đã hoàn thành
3. Nhập mô tả vấn đề

### Bước 2: Admin tạo lịch bảo hành
1. Admin tạo lịch bảo hành cho yêu cầu
2. Chọn technician theo vị trí khách hàng
3. Chọn ngày và slot

### Bước 3: Technician xử lý bảo hành
1. Technician accept lịch bảo hành (có thể accept bất cứ lúc nào)
2. Chuẩn bị sản phẩm thay thế (hoặc skip)
3. Start warranty (có thể start trong vòng 2 ngày trước ngày hẹn)
4. Hoàn thành bảo hành

---

## 7. Test Luồng Tháo Lắp (Uninstall)

### Bước 1: Tạo yêu cầu trả hàng
1. Customer tạo yêu cầu trả hàng
2. Admin duyệt yêu cầu

### Bước 2: Kiểm tra auto-create lịch tháo lắp
1. **Kiểm tra**: Lịch tháo lắp tự động tạo cho technician
2. **Kiểm tra**: ScheduledDate có time component chính xác (date + slot start time)

### Bước 3: Technician thực hiện tháo lắp
1. Technician accept lịch
2. **Kiểm tra**: Có thể start travel trực tiếp từ Confirmed (không cần PrepareMaterials)
3. Start uninstall (sử dụng StartWarranty cho uninstall)
4. Hoàn thành

---

## 8. Test Database

### Kiểm tra ScheduledDate
```sql
SELECT Id, OrderId, TechnicianId, SlotId, ScheduledDate, Status
FROM InstallationBookings
ORDER BY Id DESC
```
**Kiểm tra**: ScheduledDate có time component (không phải 00:00:00)

### Kiểm tra Slot
```sql
SELECT Id, TechnicianId, Date, StartTime, EndTime, IsBooked, BookingId
FROM InstallationSlots
WHERE IsBooked = 1
```
**Kiểm tra**: Slot được book đúng với BookingId

### Kiểm tra Technician Profile
```sql
SELECT Id, UserId, IsAvailable, City, Districts
FROM TechnicianProfiles
```
**Kiểm tra**: Districts lưu dưới dạng JSON

---

## 9. Test Edge Cases

### Case 1: Technician không có slot trống
1. Tạo lịch cho ngày mà technician không có slot
2. **Kiểm tra**: Slot tự động tạo (8:00-10:00, 10:00-12:00, 14:00-16:00, 16:00-18:00)

### Case 2: Đơn ghép (có sản phẩm cần lắp và không cần lắp)
1. Tạo đơn hàng với cả 2 loại sản phẩm
2. **Kiểm tra**: OrderType hiển thị "Đơn ghép"
3. **Kiểm tra**: Có cả section phân bổ kho và section lắp đặt

### Case 3: Customer đổi lịch
1. Customer đổi lịch lắp đặt
2. **Kiểm tra**: Technician nhận thông tin đổi lịch
3. **Kiểm tra**: Status chuyển sang "Đã đổi lịch"
4. Technician accept lại lịch mới

---

## 10. Test Console Logs

Kiểm tra các console log trong browser console:
- `[InstallationBookingDialog Submit] ScheduledDate: dd/MM/yyyy HH:mm`
- `[RejectBookingAsync] Attempting to reassign booking`
- `[RejectBookingAsync] Reassigning booking to technician X with slot Y`
- `[RejectBookingAsync] Successfully reassigned booking`
- `[OrderDialog] Loaded X technicians filtered by district: Y`

---

## Checklist Test

- [ ] Admin tạo đơn hàng cần lắp đặt
- [ ] Admin tạo lịch lắp đặt với filter vị trí
- [ ] Technician xem danh sách lịch
- [ ] Technician tiếp nhận lịch
- [ ] Technician chuẩn bị vật tư
- [ ] Technician bắt đầu di chuyển (có validation thời gian)
- [ ] Technician bắt đầu lắp đặt (có validation thời gian)
- [ ] Technician hoàn thành lắp đặt
- [ ] Technician từ chối lịch → auto-reassign
- [ ] Technician từ chối → không có technician khác → cancel
- [ ] Validation Accept quá hạn
- [ ] Validation StartTravel quá sớm
- [ ] Validation StartInstallation quá sớm
- [ ] Luồng bảo hành
- [ ] Luồng tháo lắp
- [ ] ScheduledDate có time component chính xác
- [ ] Slot tự động tạo nếu không có
- [ ] Console logs hiển thị đúng
