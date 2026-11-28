# NPC đầu bếp và trợ giúp

Tính năng dựa trên mục “Hỗ trợ từ NPC” / “Trợ giúp từ NPC” trong tài liệu thiết kế nhóm 3. Làm trên nhánh `feat/dinh-dish-washing`, sau khi cập nhật `develop` tại `2517d10`.

## Cách chơi

- Mở `Assets/Scenes/GamePlay.unity`, bấm Play, hoàn thành hoặc bỏ qua hướng dẫn để vào ca.
- Đầu bếp đứng cạnh quầy phía trên trong bếp, bên phải phòng ăn (vị trí 15.6, 4.5).
- Sau 20 giây của ca, dấu `!` xuất hiện trên đầu đầu bếp trong 20 giây. Đến gần, nhấn **E** để nhận một chai nước.
- Hộp thoại cho chọn **Uống ngay** hoặc **Để dành**. Nước đã nhận xuất hiện trên HUD; bấm **Uống [R]** hoặc phím **R** để dùng sau, kể cả lúc rửa bát.
- Mỗi chai hồi **40 điểm thể lực**, tối đa bằng thể lực tối đa. Khi thể lực đầy, chai được giữ lại, không bị tiêu hao.
- Một đợt chỉ nhận một chai. Hết thời hạn thì mất cơ hội nhận của đợt đó; nước đã nhận vẫn được giữ đến hết ca.
- Đợt tiếp theo ở giây 70 và 120 nếu ca còn chạy. Thời gian chờ tính từ lúc vào ca, không tính đoạn hướng dẫn; tạm dừng game sẽ dừng thời gian. Nói chuyện không dừng đồng hồ ca.
- Khi không có nước, đầu bếp vẫn giải thích cách giữ thể lực. Vật phẩm không mang sang ca mới.

## Chỉnh trong Inspector

Chọn `Chef` trong GamePlay hoặc prefab `Assets/Prefabs/Chef.prefab`:

- `Help Enabled`: tắt hỗ trợ (có thể dùng cho Hell khi hệ thống độ khó được nối vào).
- `Offer At Seconds`: các mốc trợ giúp tính từ đầu ca.
- `Offer Duration`: thời hạn nhận mỗi đợt.
- `Stamina Recovery`: số điểm hồi mỗi chai.

Các giá trị 40 / 20 / 70 / 120 là thông số prototype có thể cân chỉnh; tài liệu không ấn định chúng. Chưa tự gắn với chế độ Hell vì dự án chưa có lựa chọn độ khó tương ứng.

## Kiểm tra

- Trước giây 20 chưa nhận được nước; trong khoảng 20–40 nhận đúng một lần.
- Bỏ lỡ đợt đầu, đợi đợt sau; không nhận lại được đợt đã hết hạn.
- Chọn để dành, làm giảm thể lực bằng cách bê bát rồi di chuyển, bấm R: thể lực tăng, số chai giảm một.
- Thử khi thể lực đầy: không mất chai. Thử Pause: R không dùng chai.
- Thử khi đang rửa bát: R dùng được chai đã giữ; không làm sạch bát hoặc cộng tiền.
- Chạy lại ca: vật phẩm trở về 0; đầu bếp, chủ quán và tutorial hoạt động độc lập.

Đã kiểm tra biên dịch C# và chạy kiểm thử logic độc lập cho thời điểm mở đợt, nhận một lần, hết hạn, giữ vật phẩm, dùng hết và khởi tạo ca mới. Có `ChefHelpSupplyTests` trong EditMode. Cần xác nhận hình ảnh và thao tác trực tiếp bằng Play trong Unity; lần chạy Unity batch không tạo kết quả test nên chưa tính là PlayMode đã qua.
