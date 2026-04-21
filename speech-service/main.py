"""
DTHub Speech Service — STT (Faster-Whisper) + TTS (sherpa-onnx + Piper VITS tiếng Việt)

Khởi động:
    uvicorn main:app --host 0.0.0.0 --port 8003 --reload

Endpoints:
    GET  /health          — kiểm tra trạng thái
    POST /stt             — audio file -> text (multipart/form-data, field: audio)
    POST /tts             — {"text": "..."} -> audio/wav stream

Tải model tiếng Việt (chạy 1 lần):
    python download_model.py
    hoặc tải thủ công từ:
    https://github.com/k2-fsa/sherpa-onnx/releases/download/tts-models/vits-piper-vi_VN-vivos-x_low.tar.bz2
    Giải nén vào thư mục models/
"""

import io
import logging
import os
import struct
import tempfile

from fastapi import FastAPI, File, HTTPException, UploadFile
from fastapi.responses import StreamingResponse
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel

logger = logging.getLogger(__name__)

app = FastAPI(title="DTHub Speech Service", version="2.0.0")

# CORS configuration
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# ── Config ────────────────────────────────────────────────────────────────────
WHISPER_MODEL_SIZE = os.getenv("WHISPER_MODEL", "medium")

# Thư mục chứa model đã giải nén: models/vits-piper-vi_VN-vivos-x_low/
MODEL_DIR = os.getenv(
    "TTS_MODEL_DIR",
    os.path.join("models", "vits-piper-vi_VN-vais1000-medium")
)
TTS_SPEAKER_ID = int(os.getenv("TTS_SPEAKER_ID", "0"))

# ── Lazy-load Whisper ─────────────────────────────────────────────────────────
_whisper_model = None

def get_whisper():
    global _whisper_model
    if _whisper_model is None:
        from faster_whisper import WhisperModel
        logger.info("Đang tải Whisper '%s'...", WHISPER_MODEL_SIZE)
        _whisper_model = WhisperModel(WHISPER_MODEL_SIZE, device="cpu", compute_type="int8")
        logger.info("Whisper sẵn sàng.")
    return _whisper_model


# ── Lazy-load TTS (sherpa-onnx) ───────────────────────────────────────────────
_tts_engine = None

def get_tts():
    global _tts_engine
    if _tts_engine is not None:
        return _tts_engine

    onnx_file = os.path.join(MODEL_DIR, "vi_VN-vais1000-medium.onnx")
    tokens_file = os.path.join(MODEL_DIR, "tokens.txt")
    espeak_dir = os.path.join(MODEL_DIR, "espeak-ng-data")

    if not os.path.exists(onnx_file):
        return None  # model chưa tải

    try:
        import sherpa_onnx
        config = sherpa_onnx.OfflineTtsConfig(
            model=sherpa_onnx.OfflineTtsModelConfig(
                vits=sherpa_onnx.OfflineTtsVitsModelConfig(
                    model=onnx_file,
                    lexicon="",
                    data_dir=espeak_dir,
                    tokens=tokens_file,
                ),
                num_threads=2,
            ),
            max_num_sentences=1,
        )
        if not config.validate():
            logger.error("TTS config không hợp lệ")
            return None
        _tts_engine = sherpa_onnx.OfflineTts(config)
        logger.info("TTS (sherpa-onnx) sẵn sàng.")
    except Exception as e:
        logger.error("Không thể khởi tạo TTS: %s", e)
        return None

    return _tts_engine


# ── Endpoints ─────────────────────────────────────────────────────────────────

@app.get("/health")
def health():
    onnx_file = os.path.join(MODEL_DIR, "vi_VN-vais1000-medium.onnx")
    model_ok = os.path.exists(onnx_file)
    tts_ready = get_tts() is not None
    return {
        "status": "ok",
        "whisper_model": WHISPER_MODEL_SIZE,
        "tts_engine": "sherpa-onnx",
        "tts_model_dir": MODEL_DIR,
        "tts_model_found": model_ok,
        "tts_ready": tts_ready,
    }


@app.get("/metadata")
def metadata():
    """MCP-compatible metadata endpoint để Django nhận diện đây là MCP server."""
    return _speech_tools_schema()


@app.get("/mcp/tools")
def mcp_tools():
    """Endpoint chuẩn MCP — trả về danh sách tools."""
    return {"tools": _speech_tools_schema()["tools"]}


@app.get("/mcp/resources")
def mcp_resources():
    return {"resources": []}


@app.get("/mcp/info")
def mcp_info():
    return {"name": "DTHub Speech Service", "version": "2.0.0"}


def _speech_tools_schema():
    return {
        "name": "DTHub Speech Service",
        "version": "2.0.0",
        "description": "STT (Faster-Whisper) + TTS (sherpa-onnx Piper VITS tiếng Việt)",
        "tools": [
            {
                "name": "speech_to_text",
                "description": "Chuyển audio thành văn bản tiếng Việt",
                "inputSchema": {
                    "type": "object",
                    "properties": {
                        "audio": {"type": "string", "description": "File audio (multipart upload)"}
                    }
                }
            },
            {
                "name": "text_to_speech",
                "description": "Chuyển văn bản thành giọng nói tiếng Việt",
                "inputSchema": {
                    "type": "object",
                    "properties": {
                        "text": {"type": "string", "description": "Văn bản cần đọc"},
                        "speed": {"type": "number", "description": "Tốc độ đọc (mặc định 1.0)"},
                        "speaker_id": {"type": "integer", "description": "ID giọng đọc"}
                    },
                    "required": ["text"]
                }
            }
        ]
    }


@app.post("/stt")
async def speech_to_text(audio: UploadFile = File(...)):
    """Nhận audio blob (webm/wav/...), trả về text tiếng Việt qua Faster-Whisper."""
    raw = await audio.read()
    if not raw:
        raise HTTPException(status_code=400, detail="File audio rỗng")

    suffix = _guess_suffix(audio.filename or "", audio.content_type or "")
    with tempfile.NamedTemporaryFile(suffix=suffix, delete=False) as tmp:
        tmp.write(raw)
        tmp_path = tmp.name

    try:
        # Try to convert webm to wav if FFmpeg is available for better Whisper compatibility
        if suffix == ".webm":
            wav_path = tmp_path.replace(suffix, ".wav")
            if _convert_webm_to_wav(tmp_path, wav_path):
                os.unlink(tmp_path)
                tmp_path = wav_path
                logger.info(f"Converted webm to wav: {wav_path}")
            else:
                logger.warning("FFmpeg not available, processing webm directly")
        
        model = get_whisper()
        segments, info = model.transcribe(
            tmp_path,
            language="vi",
            beam_size=5,
            vad_filter=False,  # Disable VAD to avoid filtering out speech
            word_timestamps=True,
        )
        text = " ".join(seg.text.strip() for seg in segments).strip()
        logger.info(f"Transcription result: '{text}' (language: {info.language}, duration: {info.duration})")
        return {"text": text, "language": info.language, "duration": info.duration}
    except Exception as e:
        logger.error("STT error: %s", e)
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        if os.path.exists(tmp_path):
            os.unlink(tmp_path)


class TTSRequest(BaseModel):
    text: str
    speed: float = 1.0
    speaker_id: int = TTS_SPEAKER_ID


@app.post("/tts")
def text_to_speech(req: TTSRequest):
    """Nhận text, dùng sherpa-onnx VITS tiếng Việt, trả về audio/wav."""
    if not req.text.strip():
        raise HTTPException(status_code=400, detail="Text rỗng")

    tts = get_tts()
    if tts is None:
        raise HTTPException(
            status_code=503,
            detail=(
                "Model TTS chưa sẵn sàng. Chạy: python download_model.py\n"
                "hoặc tải thủ công từ:\n"
                "https://github.com/k2-fsa/sherpa-onnx/releases/download/"
                "tts-models/vits-piper-vi_VN-vivos-x_low.tar.bz2\n"
                f"Giải nén vào: {MODEL_DIR}"
            )
        )

    try:
        # Chuẩn hóa text trước — xử lý công thức, ký hiệu đặc biệt
        clean_text = _normalize_text(req.text)
        sentences = _split_sentences(clean_text)
        all_samples = []
        sample_rate = None

        for sent in sentences:
            if not sent.strip():
                continue
            audio = tts.generate(
                text=sent,
                sid=req.speaker_id,
                speed=req.speed,
            )
            if sample_rate is None:
                sample_rate = audio.sample_rate
            all_samples.extend(audio.samples)
            # Khoảng lặng tỉ lệ theo độ dài câu: câu dài nghỉ lâu hơn
            pause_sec = 0.15 + min(len(sent) / 200, 0.25)
            silence_len = int(audio.sample_rate * pause_sec)
            all_samples.extend([0.0] * silence_len)

        wav_bytes = _samples_to_wav(all_samples, sample_rate or 22050)
        return StreamingResponse(io.BytesIO(wav_bytes), media_type="audio/wav")
    except Exception as e:
        logger.error("TTS error: %s", e)
        raise HTTPException(status_code=500, detail=str(e))


# ── Helpers ───────────────────────────────────────────────────────────────────

def _normalize_text(text: str) -> str:
    """Chuẩn hóa text trước khi đưa vào TTS — xử lý công thức, ký hiệu đặc biệt, từ tiếng Anh."""
    import re

    # Xóa markdown: **bold**, *italic*, `code`, # header
    text = re.sub(r'\*{1,2}([^*]+)\*{1,2}', r'\1', text)
    text = re.sub(r'`[^`]*`', '', text)
    text = re.sub(r'^#{1,6}\s+', '', text, flags=re.MULTILINE)

    # Công thức vật lý/hóa học phổ biến
    replacements = [
        (r'E\s*=\s*mc\s*[²2]', 'E bằng m c bình phương'),
        (r'E\s*=\s*mc\^2', 'E bằng m c bình phương'),
        (r'H₂O|H2O', 'H hai O'),
        (r'CO₂|CO2', 'C O hai'),
        (r'O₂|O2', 'O hai'),
        (r'N₂|N2', 'N hai'),
        (r'Fe₂O₃', 'F e hai O ba'),
        # Số mũ unicode
        (r'²', ' bình phương'),
        (r'³', ' lập phương'),
        (r'¹', ''),
        # Ký hiệu toán
        (r'≈', 'xấp xỉ'),
        (r'≠', 'khác'),
        (r'≤', 'nhỏ hơn hoặc bằng'),
        (r'≥', 'lớn hơn hoặc bằng'),
        (r'→', 'ra'),
        (r'←', 'từ'),
        (r'×', 'nhân'),
        (r'÷', 'chia'),
        (r'\^(\d+)', r' mũ \1'),
        # Đơn vị
        (r'(\d+)\s*km/h', r'\1 ki lô mét trên giờ'),
        (r'(\d+)\s*m/s', r'\1 mét trên giây'),
        (r'(\d+)\s*°C', r'\1 độ C'),
        (r'(\d+)\s*%', r'\1 phần trăm'),
        # Xóa URL
        (r'https?://\S+', ''),
        # Xóa dấu ngoặc đơn rỗng
        (r'\(\s*\)', ''),
    ]

    for pattern, replacement in replacements:
        text = re.sub(pattern, replacement, text)

    # Xử lý từ tiếng Anh — transliterate sang cách đọc tiếng Việt
    text = _transliterate_english(text)

    # Chuẩn hóa khoảng trắng
    text = re.sub(r'\s+', ' ', text).strip()
    return text


# Bảng map từ tiếng Anh thông dụng → phiên âm đọc tiếng Việt
_EN_WORD_MAP = {
    # ── Viết tắt học thuật / giáo dục ────────────────────────────────────────
    "gvhd": "giáo viên hướng dẫn",
    "svth": "sinh viên thực hiện",
    "cbhd": "cán bộ hướng dẫn",
    "cbpb": "cán bộ phản biện",
    "hđbv": "hội đồng bảo vệ",
    "kltn": "khóa luận tốt nghiệp",
    "lvtn": "luận văn tốt nghiệp",
    "đatn": "đồ án tốt nghiệp",
    "đa": "đồ án",
    "lv": "luận văn",
    "kl": "khóa luận",
    "tp": "thành phố",
    "tphcm": "thành phố Hồ Chí Minh",
    "hcm": "Hồ Chí Minh",
    "hn": "Hà Nội",
    "vn": "Việt Nam",
    "cntt": "công nghệ thông tin",
    "ktmt": "kỹ thuật máy tính",
    "đhbk": "đại học bách khoa",
    "đhqg": "đại học quốc gia",
    "đh": "đại học",
    "cđ": "cao đẳng",
    "thpt": "trung học phổ thông",
    "thcs": "trung học cơ sở",
    "gv": "giáo viên",
    "sv": "sinh viên",
    "hs": "học sinh",
    "cb": "cán bộ",
    "ts": "tiến sĩ",
    "ths": "thạc sĩ",
    "gs": "giáo sư",
    "pgs": "phó giáo sư",
    "nxb": "nhà xuất bản",
    "tr": "trang",
    "tt": "trung tâm",
    "vd": "ví dụ",
    "vv": "vân vân",
    "tl": "tài liệu",
    "nd": "nội dung",
    "kq": "kết quả",
    "pp": "phương pháp",
    "nc": "nghiên cứu",
    "ưu": "ưu",
    # ── Viết tắt kỹ thuật / IoT ───────────────────────────────────────────────
    "iot": "ai ô ti",
    "ai": "ây ai",
    "ml": "em eo",
    "dl": "đi eo",
    "cpu": "xê pi diu",
    "gpu": "ji pi diu",
    "ram": "ram",
    "rom": "rom",
    "ssd": "ét ét đi",
    "hdd": "hát đi đi",
    "usb": "diu ét bi",
    "hdmi": "hát đi em ai",
    "ip": "ai pi",
    "id": "ai đi",
    "ok": "ô kê",
    "led": "lét",
    "lcd": "eo xê đi",
    "ac": "ây xê",
    "dc": "đi xê",
    "mqtt": "em kiu ti ti",
    "esp": "i ét pi",
    "mcu": "em xê diu",
    "pcb": "pi xê bi",
    "gpio": "ji pi ai ô",
    "pwm": "pi đắp liu em",
    "uart": "diu a a ti",
    "i2c": "ai tu xê",
    "spi": "ét pi ai",
    "api": "ây pi ai",
    "url": "diu a eo",
    "http": "hát ti ti pi",
    "https": "hát ti ti pi ét",
    "json": "giây son",
    "html": "hát ti em eo",
    "css": "xê ét ét",
    "diy": "đi ai oai",
    # ── Từ tiếng Anh thông dụng ───────────────────────────────────────────────
    "wifi": "oai phai", "wi-fi": "oai phai",
    "bluetooth": "blu tút",
    "internet": "in tơ nét",
    "online": "on lai",
    "offline": "óp lai",
    "website": "uép sai",
    "server": "sơ vơ",
    "client": "clai ần",
    "database": "đây ta bây",
    "software": "sóp ue",
    "hardware": "ha ue",
    "firmware": "phơm ue",
    "update": "ắp đết",
    "upload": "ắp lốt",
    "download": "đao lốt",
    "backup": "bắc ắp",
    "reset": "ri sét",
    "reboot": "ri bút",
    "login": "lóc in",
    "logout": "lóc ao",
    "password": "pát xờ",
    "username": "diu dơ nêm",
    "email": "i meo",
    "chat": "chét",
    "bot": "bót",
    "app": "áp",
    "api": "ây pi ai",
    "url": "diu a eo",
    "http": "hát ti ti pi",
    "https": "hát ti ti pi ét",
    "json": "giây son",
    "html": "hát ti em eo",
    "css": "xê ét ét",
    "python": "pai thần",
    "javascript": "gia va xcríp",
    "linux": "li nắc",
    "windows": "uin đô",
    "android": "an đờ roi",
    "ios": "ai ô ét",
    "cloud": "clao",
    "token": "tô ken",
    "cache": "cát",
    "debug": "đi bắc",
    "error": "e rơ",
    "warning": "wo ninh",
    "status": "xtê tớt",
    "mode": "mốt",
    "model": "mô đồ",
    "sensor": "xen xơ",
    "relay": "ri lây",
    "router": "ru tơ",
    "gateway": "gết uây",
    "dashboard": "đét bót",
    "stream": "xtrim",
    "socket": "xóc két",
    "port": "pót",
    "host": "hốt",
    "proxy": "prốc xi",
    "plugin": "plắc in",
    "module": "mô đun",
    "input": "in pút",
    "output": "ao pút",
    "default": "đi phôn",
    "config": "con phích",
    "setting": "xét tinh",
    "profile": "prô phai",
    "account": "a cao",
    "admin": "át min",
    "user": "diu dơ",
    "device": "đi vai",
    "network": "nét uơc",
    "connect": "cô nếc",
    "disconnect": "đít cô nếc",
    "enable": "en nây bồ",
    "disable": "đít xây bồ",
    "active": "ắc típ",
    "inactive": "in ắc típ",
    # Viết tắt phổ biến đã có ở trên, bỏ qua phần trùng
}

# Bảng đọc từng chữ cái tiếng Anh theo tiếng Việt
_LETTER_SOUNDS = {
    'a': 'ây', 'b': 'bi', 'c': 'xê', 'd': 'đi', 'e': 'i',
    'f': 'éph', 'g': 'ji', 'h': 'hát', 'i': 'ai', 'j': 'giây',
    'k': 'kây', 'l': 'eo', 'm': 'em', 'n': 'en', 'o': 'ô',
    'p': 'pi', 'q': 'kiu', 'r': 'a', 's': 'ét', 't': 'ti',
    'u': 'diu', 'v': 'vi', 'w': 'đắp liu', 'x': 'éc', 'y': 'oai',
    'z': 'dét',
}


def _spell_english_word(word: str) -> str:
    """Đọc từng chữ cái của từ tiếng Anh theo âm tiếng Việt."""
    return ' '.join(_LETTER_SOUNDS.get(c.lower(), c) for c in word if c.isalpha())


def _transliterate_english(text: str) -> str:
    """
    Chỉ thay thế các từ/viết tắt tiếng Anh có trong _EN_WORD_MAP.
    Không tự động spell từ không biết — tránh đọc sai từ tiếng Việt không dấu.
    """
    import re

    tokens = re.split(r'(\s+|[^\w\s])', text)
    result = []
    for tok in tokens:
        # Chỉ xử lý token toàn ASCII a-zA-Z
        if re.fullmatch(r'[a-zA-Z]{2,}', tok):
            key = tok.lower()
            if key in _EN_WORD_MAP:
                result.append(_EN_WORD_MAP[key])
            else:
                # Không biết từ này → giữ nguyên, để espeak-ng xử lý
                result.append(tok)
        else:
            result.append(tok)
    return ''.join(result)


def _split_sentences(text: str) -> list[str]:
    """Tách text thành câu theo dấu câu để TTS ngắt nghỉ tự nhiên."""
    import re
    # Tách theo . ! ? và dấu chấm lửng, giữ lại dấu câu
    parts = re.split(r'(?<=[.!?…])\s+', text.strip())
    # Lọc câu quá ngắn, gộp vào câu trước
    result = []
    buf = ""
    for p in parts:
        buf = (buf + " " + p).strip() if buf else p
        if len(buf) >= 10:
            result.append(buf)
            buf = ""
    if buf:
        result.append(buf)
    return result if result else [text]

def _convert_webm_to_wav(webm_path: str, wav_path: str) -> bool:
    """Convert webm audio to wav format using ffmpeg. Returns True if successful, False otherwise."""
    import subprocess
    import os
    
    # Try to find ffmpeg in common Windows locations
    ffmpeg_paths = [
        "ffmpeg",  # Try from PATH
        r"C:\Program Files\ffmpeg\bin\ffmpeg.exe",
        r"C:\ffmpeg\bin\ffmpeg.exe",
        os.path.expanduser(r"~\AppData\Local\Microsoft\WinGet\Packages\Gyan.FFmpeg_Microsoft.Winget.Source_\ffmpeg.exe"),
    ]
    
    ffmpeg_cmd = None
    for path in ffmpeg_paths:
        try:
            subprocess.run([path, "-version"], check=True, capture_output=True, timeout=5)
            ffmpeg_cmd = path
            logger.info(f"Found FFmpeg at: {path}")
            break
        except (subprocess.CalledProcessError, FileNotFoundError):
            continue
    
    if ffmpeg_cmd is None:
        logger.error("FFmpeg not found in PATH or common locations")
        return False
    
    try:
        subprocess.run(
            [ffmpeg_cmd, "-i", webm_path, "-ar", "16000", "-ac", "1", wav_path, "-y"],
            check=True,
            capture_output=True,
            timeout=30
        )
        logger.info(f"Successfully converted {webm_path} to {wav_path}")
        return True
    except subprocess.CalledProcessError as e:
        logger.error(f"FFmpeg conversion failed: {e.stderr.decode() if e.stderr else str(e)}")
        return False


def _guess_suffix(filename: str, content_type: str) -> str:
    if filename:
        ext = os.path.splitext(filename)[-1].lower()
        if ext in (".wav", ".mp3", ".ogg", ".webm", ".m4a", ".flac"):
            return ext
    ct = content_type.lower()
    if "wav" in ct:   return ".wav"
    if "ogg" in ct:   return ".ogg"
    if "mp3" in ct:   return ".mp3"
    if "webm" in ct:  return ".webm"
    return ".webm"


def _samples_to_wav(samples, sample_rate: int) -> bytes:
    """Chuyển float32 samples từ sherpa-onnx thành WAV bytes."""
    import numpy as np
    arr = np.array(samples, dtype=np.float32)
    # Clamp và chuyển sang int16
    arr = np.clip(arr, -1.0, 1.0)
    pcm = (arr * 32767).astype(np.int16).tobytes()

    channels = 1
    sampwidth = 2
    data_size = len(pcm)
    header = struct.pack(
        "<4sI4s4sIHHIIHH4sI",
        b"RIFF",
        36 + data_size,
        b"WAVE",
        b"fmt ",
        16,
        1,  # PCM
        channels,
        sample_rate,
        sample_rate * channels * sampwidth,
        channels * sampwidth,
        sampwidth * 8,
        b"data",
        data_size,
    )
    return header + pcm


if __name__ == "__main__":
    import uvicorn
    uvicorn.run("main:app", host="0.0.0.0", port=8003, reload=True)
