# Chế độ rửa bát

Phạm vi: khi tương tác máy rửa bát, mở giao diện cận cảnh bát bẩn.
Chưa triển khai chà rửa, đánh giá sạch, thưởng hoặc hoàn tất bát.

## Chạy thử

1. Mở Assets/Scenes/SampleScene.unity, chờ Unity biên dịch, nhấn Play.
2. Sau khi Unity biên dịch, bộ cấu hình Editor tự thêm PlayerInteractor nếu thiếu và lưu scene.
3. Dùng WASD đến ô xanh Dishwasher trong khu bếp, nhấn E. Không bắt buộc mang bát ở đợt chuyển màn hình này.
4. Màn hình DISH WASHING hiện lên, nhân vật dừng, đồng hồ ca tiếp tục chạy.
5. Nhấn nút BACK TO RESTAURANT hoặc Esc để trở lại màn chơi.
6. Nhấn E ở máy lần nữa: mở lại bát đã nạp, không lấy thêm bát từ người chơi.

Giao diện được tạo khi cần, không phải kéo/gán Canvas. Nếu scene chưa tự cập nhật, dùng Tools > Dish Rushing > Set up dish washing khi không ở Play Mode.
Máy rửa bát phải có Dishwasher và collider/layer tương tác theo hệ thống sẵn có.
Không dùng thao tác đóng màn hình để đánh dấu bát sạch.

## Tích hợp

Dishwasher.Interact gọi DishWashingMode.Open(playerInteractor). Nếu đang mang bát và máy còn chỗ, nạp bát trước khi mở; nếu không vẫn mở màn hình xem chế độ rửa.
DishWashingMode.IsOpen cho biết trạng thái; Close() trở về màn chơi.
Khi mở, lưu và tạm khóa Player, PlayerInteractor và PlayerInput, dừng Rigidbody2D.
Khi đóng hoặc scene bị hủy, khôi phục trạng thái trước đó và con trỏ.
Time.timeScale không thay đổi. PlayerInteractor chặn tương tác và ẩn lời nhắc E khi bị khóa.
DishLoaded vẫn chỉ phát khi có bát mới được nạp.

## Kiểm tra đã thực hiện

Biên dịch mã bằng trình biên dịch C# và tham chiếu của Unity 6000.3.13f1: thành công.
Người dùng đã chạy trong Play Mode, cung cấp ảnh màn hình DISH WASHING và xác nhận chạy thử ổn.

Nhánh bàn giao: feat/dinh-dish-washing. Chưa merge vào develop hoặc main.
