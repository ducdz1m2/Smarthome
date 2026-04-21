# Setup Speech Service

## 1. Tải Piper binary (Windows)

Tải file `piper_windows_amd64.zip` tại:
https://github.com/rhasspy/piper/releases/latest

Giải nén, copy `piper.exe` và `piper.dll` vào thư mục `bin/`:
```
services/speech-service/
  bin/
    piper.exe
    piper.dll   (cần thiết trên Windows)
    espeak-ng-data/  (thư mục đi kèm trong zip)
```

## 2. Tải model tiếng Việt

```bash
cd services/speech-service/models

# Model vi_VN-vivos-medium (~60MB)
curl -L -o vi_VN-vivos-medium.onnx ^
  https://huggingface.co/rhasspy/piper-voices/resolve/main/vi/vi_VN/vivos/medium/vi_VN-vivos-medium.onnx

curl -L -o vi_VN-vivos-medium.onnx.json ^
  https://huggingface.co/rhasspy/piper-voices/resolve/main/vi/vi_VN/vivos/medium/vi_VN-vivos-medium.onnx.json
```

Hoặc tải thủ công tại:
https://huggingface.co/rhasspy/piper-voices/tree/main/vi/vi_VN/vivos/medium

## 3. Cài Python dependencies

```bash
pip install -r requirements.txt
```

## 4. Khởi động

```bash
uvicorn main:app --host 0.0.0.0 --port 8003
```

Kiểm tra: http://localhost:8003/health
