# Phần mở đầu, bà chủ quán và tutorial ca đầu

Kịch bản đầy đủ (giọng văn, nhân vật, mọi câu thoại): `docs/design/intro-tutorial-script.md`.

## Phần mở đầu (intro)

- Chỉ chạy ở lần chơi đầu, ngay trước tutorial: 6 cảnh kiểu truyện tranh (phố cổ, phòng trọ, cổng trường, trước cửa quán, trong quán, chú Tư). Đọc tiếp bằng E / Space / Enter / chuột trái; nút **BỎ QUA PHẦN MỞ ĐẦU** chuyển thẳng sang tutorial.
- Đồng hồ ca, bát và khách đứng yên trong lúc chiếu. Esc tạm dừng cả intro (menu tạm dừng hiện lên trên).
- "Chị dạy lại từ đầu đi ạ" chỉ chạy lại tutorial, không chiếu lại intro.
- **Sửa nội dung:** `Assets/Data/IntroContent.asset`:
  - `characters`: khoá (`vanh`, `linh`, `owner`, `chef`), tên hiện trên bảng (tối đa 14 ký tự), chân dung, 5 frame đứng.
  - `scenes`: tiêu đề, ảnh nền (`Assets/Textures/Intro/Backgrounds`, 320×180), nhân vật có sẵn trên sân khấu, danh sách câu.
  - Mỗi câu: người nói (`narrator` = người kể, chữ nghiêng, không tên), nội dung (tối đa 150 ký tự), hiệu ứng (Shake, Flash, ZoomIn, SlowZoom) và chuyển động của một nhân vật (EnterLeft, EnterRight, ExitRight, Jump).
- Sửa xong chạy lại test `DialogueContentTests` (Test Runner → EditMode) để kiểm câu quá dài hoặc người nói không tồn tại.

## Luật chơi

- Bà chủ đứng sau quầy thu ngân (góc trên bên trái phòng ăn). Đứng trước quầy, nhấn **E** để nói chuyện:
  - **Chị cho em xin bí kíp**: bà nói một bí kíp, lần sau nói bí kíp tiếp theo.
  - **Chị dạy lại từ đầu đi ạ**: chạy lại tutorial (không chiếu intro). Lần này ca **không** dừng lại.
- Bà phản ứng bằng bong bóng thoại cạnh đầu: vỡ bát, nộp bát bẩn, rửa sạch 3 bát liên tiếp, bát chất từ 12 cái trở lên, còn 20 giây. Tối đa 1 câu mỗi 6 giây, cùng một loại câu cách nhau ít nhất 15 giây, câu quan trọng hơn được nói trước (vỡ > bẩn > bát dồn > hết giờ > khen). Bà im lặng khi đang có hội thoại, đang ở màn rửa hoặc tutorial đang chạy.
- **Tutorial** chỉ chạy ở lần chơi đầu. Trong lúc chạy, đồng hồ ca, việc sinh bát và sinh khách **đứng yên**. Nút **BỎ QUA** ở góc trên bên phải kết thúc ngay và bắt đầu ca.
  Bà chủ nói một câu khi mở mỗi bước và một câu khen/chê khi xong bước. Đứng yên 12 giây thì bà nhắc (tối đa 3 câu mỗi bước). Khi bà ở ngoài màn hình hoặc đang ở màn rửa, câu của bà hiện dưới khung NHIỆM VỤ.
  1. Nghe bà chủ chào (E / Space / Enter / chuột trái để đọc tiếp).
  2. Đi lại bằng WASD (2 đơn vị).
  3. Nhặt chồng bát tutorial (mũi tên vàng chỉ vào). Nếu chồng đó biến mất trước khi nhặt, một chồng mới được sinh ra.
  4. Đọc nhắc về thể lực (tự qua sau 4 giây, đứng yên khi tạm dừng).
  5. Mang bát đến máy rửa và nhấn E (có mũi tên ở mép màn hình khi máy rửa ngoài khung hình). Nếu bát rơi vỡ trên đường, tutorial sinh chồng mới.
  6. Rửa xong một bát (sạch hay bẩn đều được).
  7. Đóng màn rửa, nghe bà chủ dặn → ca bắt đầu.

## Sửa lời thoại

Mọi câu của bà chủ nằm trong `Assets/Data/OwnerDialogue.asset` (chọn file, sửa trong Inspector):

| Trường | Dùng ở đâu |
|---|---|
| `welcome`, `startShift` | Hội thoại đầu và cuối tutorial |
| `stepObjectives` | Dòng "NHIỆM VỤ" của 7 bước, đúng thứ tự trên |
| `stepIntro`, `stepCompleted`, `stepIdle` | Câu mở bước, câu xong bước, câu nhắc khi đứng yên (5 bước Đi lại … Chà bát) |
| `scrubDirtyReaction`, `dishBrokenDuringTutorial` | Nộp bát bẩn ở bước chà; bát tutorial bị vỡ |
| `talkPrompts`, `talkOptions`, `tips`, `tipsEasterEgg`, `replayAck` | Nói chuyện bằng E (câu hỏi chọn ngẫu nhiên; xin bí kíp quá 8 lần liền thì có câu đặc biệt) |
| `winShift`, `loseTime`, `loseBroken`, `chefEndShift` | Để sẵn cho phần thắng/thua, chưa dùng |
| `barkBroken`, `barkDirty`, `barkPileUp`, `barkTimeLow`, `barkCleanStreak` | Bong bóng thoại; nhiều câu thì nói lần lượt |

## Chỉnh thông số (Inspector)

- `Owner` (prefab `Assets/Prefabs/Owner.prefab`) → `OwnerNpc`: `globalCooldown` (6), `perKindCooldown` (15), `pileUpThreshold` (12), `timeLowSeconds` (20), `cleanStreakToPraise` (3), `bubbleSeconds` (3).
- `Tutorial` (trong scene GamePlay) → `TutorialController`: `moveDistance` (2), `staminaSeconds` (4), `idleNudgeSeconds` (12), `remarkGap` (3.2), độ cao mũi tên trên bát / máy rửa.
- `Assets/Resources/IntroUI.prefab` → `IntroPlayer`: thời gian mờ, thời gian hiện tiêu đề cảnh, khoảng cách và cỡ nhân vật.
- `Assets/Resources/DialogueUI.prefab` → `DialogueBox.charsPerSecond` (40).

## Cấu trúc

| File | Vai trò |
|---|---|
| `Scripts/Dialogue/DialogueBox.cs`, `DialogueCursor.cs`, `DialogueLine.cs`, `Resources/DialogueUI.prefab` | Hộp thoại dưới màn hình; khoá di chuyển khi mở |
| `Scripts/Owner/OwnerNpc.cs`, `BarkScheduler.cs`, `SpeechBubble.cs`, `OwnerDialogueSO.cs`, `Prefabs/Owner.prefab` | Bà chủ |
| `Scripts/Tutorial/TutorialController.cs`, `TutorialSteps.cs`, `TutorialProgress.cs`, `TutorialPointer.cs`, `TutorialHud.cs`, `EdgePointerMath.cs`, `Prefabs/Tutorial.prefab`, `Resources/TutorialUI.prefab` | Tutorial |
| `CountDownTime`, `BowlSpawn`, `CustomerSpawner`: `Hold()` / `Release()` | Giữ ca đứng yên khi tutorial chạy |
| `GameManager.BowlWashedEvent` / `BowlBrokenEvent`, `PlayerInteractor.DishPicked` | Event để tutorial và bà chủ nghe |
| `Scripts/Intro/IntroPlayer.cs`, `IntroCursor.cs`, `IntroContentSO.cs`, `Resources/IntroUI.prefab`, `Data/IntroContent.asset`, `Textures/Intro/*` | Phần mở đầu |
| `Tests/Editor/*.cs` | Test EditMode (Window → General → Test Runner → EditMode) |

## Kiểm tra lại tutorial khi test

Tutorial chỉ hiện một lần nhờ key `PlayerPrefs` **`DishRushing.TutorialDone`**. Để xem lại: nói chuyện với bà chủ → "Chị dạy lại từ đầu đi ạ", hoặc xoá key bằng Edit → Clear All PlayerPrefs.
