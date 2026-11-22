# Chế độ rửa bát

Phạm vi: nạp bát vào máy rửa, chà sạch từng bát bằng chuột, chấm độ sạch, cộng/trừ tiền và cập nhật HUD.
Chưa có: thắng/thua, bát vỡ, thể lực, âm thanh, hình vẽ thật cho máy rửa trên bản đồ.

## Luật chơi

- Mang bát đến máy rửa (ô xanh trong khu bếp) và nhấn E: toàn bộ bát đang cầm vào máy, màn hình rửa mở ra.
- Máy trống và tay không thì không mở được màn hình rửa.
- Giữ chuột trái và di trên đĩa để xoá vết bẩn; miếng bọt biển hiện theo chuột. Chuột đứng yên không làm sạch thêm.
- Sạch 100% thì bát tự hoàn thành. Bấm XONG hoặc Space để nộp sớm: từ 90% là sạch (+5 tiền), dưới 90% là bẩn (−3 tiền). Bát nộp rồi luôn tính là đã rửa. Tiền không xuống dưới 0.
- Esc hoặc nút VỀ QUÁN đóng màn hình. Bát đang chà giữ nguyên vết bẩn; nhấn E ở máy để chà tiếp. Đóng màn hình không bao giờ tính là rửa xong.
- Hết bát trong máy thì màn hình tự đóng sau khoảng 0.8 giây.
- Đồng hồ ca vẫn chạy trong lúc rửa (Time.timeScale không đổi).
- "Cần Rửa" trên HUD là số bát đã sinh ra mà chưa rửa xong (trên bàn, đang cầm, trong máy).

## Cấu trúc

| File | Vai trò |
|---|---|
| `Assets/Scripts/Washing/DirtMask.cs` | Vết bẩn của một bát: lưới 128×128, sinh vết, xoá theo nét chà, tính % sạch. C# thuần, không cần Play Mode. |
| `Assets/Scripts/Washing/DishWashingView.cs` | Màn hình rửa: đọc chuột, vẽ vết bẩn, thanh tiến độ, nút XONG / VỀ QUÁN. |
| `Assets/Resources/DishWashingUI.prefab` | Giao diện uGUI của màn hình rửa, chỉnh được trong Editor. Được nạp bằng `Resources.Load` khi mở lần đầu. Mọi thứ nằm trong khung `Screen` 1280×720 (tỉ lệ 16:9, phần thừa của màn hình được tô tối). |
| `Assets/Textures/Washing/*.png` | Art pixel của màn hình rửa: nền bồn rửa 320×180 (hiển thị ×4), đĩa 72×72, miếng bọt biển, nút và khung thanh tiến độ. Import: Point, không nén. |
| `Assets/Scripts/DishWashingMode.cs` | Mở/đóng màn hình, khoá Player, PlayerInteractor và PlayerInput trong lúc rửa. |
| `Assets/Scripts/Interaction/Dishwasher.cs` | Hàng đợi bát bẩn (`DirtyBowls`), bát đang chà (`CurrentMask`), chấm điểm (`FinishCurrentBowl`). |
| `Assets/Scripts/Manager/GameManager.cs` | Tiền (`Money`) và số bát cần rửa (`BowlsToWash`), event `Changed`. |
| `Assets/Scripts/HudStats.cs` | Cập nhật `MoneyTXT` và `NeedWashingTXT` (gắn trên `GameUI`). |

## Chỉnh thông số (Inspector)

- `Dishwasher`: `bowlCapacity` (30), `passRatio` (0.9), `dirtSpots` (4 vết mỗi bát).
- `GameManager`: `cleanReward` (5), `dirtyPenalty` (3).
- `DishWashingUI.prefab` → `DishWashingView`: `brushRadius`, `brushStrength`, `brushSpacing` (tính theo ô lưới 128), màu vết bẩn và màu thanh tiến độ.

## Kiểm tra đã thực hiện

Trong Play Mode trên Unity 6000.3.13f1, Console không có lỗi:

- Logic vết bẩn: cùng seed ra cùng vết, vết luôn nằm trong lòng đĩa, chuột đứng yên không làm sạch thêm, kéo nhanh hay chậm cho cùng kết quả.
- Nạp 4 bát, chà dở, đóng và mở lại: vẫn là bát cũ với vết bẩn cũ.
- Sạch hết: tự hoàn thành, +5, sang bát kế. Nộp bát chưa chà: −3. Bấm XONG hai lần trong một frame chỉ tính một bát.
- Hết bát: màn hình tự đóng, nhân vật điều khiển lại được.
- Máy đầy: không nhận thêm bát; máy trống và tay không: không mở.

Chưa kiểm tra bằng chuột thật: việc chà được giả lập bằng cách gọi thẳng logic vết bẩn. Cần chơi thử để chắc vị trí chuột khớp với vết bẩn trên đĩa.
