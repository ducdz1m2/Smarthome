"""
Tải model TTS tiếng Việt từ GitHub releases của k2-fsa/sherpa-onnx.
Không cần HuggingFace, không cần VPN.

Chạy:
    python download_model.py
"""

import os
import tarfile
import urllib.request

MODEL_URL = (
    "https://github.com/k2-fsa/sherpa-onnx/releases/download/"
    "tts-models/vits-piper-vi_VN-vivos-x_low.tar.bz2"
)
ARCHIVE_NAME = "vits-piper-vi_VN-vivos-x_low.tar.bz2"
MODELS_DIR = "models"
EXPECTED_DIR = os.path.join(MODELS_DIR, "vits-piper-vi_VN-vivos-x_low")


def download_with_progress(url: str, dest: str):
    print(f"Đang tải: {url}")
    print(f"Lưu vào : {dest}")

    def reporthook(count, block_size, total_size):
        if total_size > 0:
            pct = count * block_size * 100 // total_size
            mb_done = count * block_size / 1_048_576
            mb_total = total_size / 1_048_576
            print(f"\r  {pct:3d}%  {mb_done:.1f}/{mb_total:.1f} MB", end="", flush=True)

    urllib.request.urlretrieve(url, dest, reporthook)
    print()  # xuống dòng sau progress


def main():
    os.makedirs(MODELS_DIR, exist_ok=True)

    # Kiểm tra đã có model chưa
    onnx_path = os.path.join(EXPECTED_DIR, "vi_VN-vivos-x_low.onnx")
    if os.path.exists(onnx_path):
        print(f"Model đã tồn tại tại: {EXPECTED_DIR}")
        print("Bỏ qua tải xuống.")
        return

    archive_path = os.path.join(MODELS_DIR, ARCHIVE_NAME)

    # Tải nếu chưa có archive
    if not os.path.exists(archive_path):
        download_with_progress(MODEL_URL, archive_path)
    else:
        print(f"Archive đã có: {archive_path}, bỏ qua tải.")

    # Giải nén
    print(f"Đang giải nén vào {MODELS_DIR}/ ...")
    with tarfile.open(archive_path, "r:bz2") as tar:
        tar.extractall(MODELS_DIR)
    print("Giải nén xong.")

    # Xóa archive để tiết kiệm dung lượng
    os.remove(archive_path)
    print("Đã xóa file nén.")

    if os.path.exists(onnx_path):
        print(f"\nModel sẵn sàng tại: {EXPECTED_DIR}")
        print("Khởi động service: uvicorn main:app --host 0.0.0.0 --port 8003")
    else:
        print("Lỗi: không tìm thấy file .onnx sau khi giải nén. Kiểm tra lại.")


if __name__ == "__main__":
    main()
