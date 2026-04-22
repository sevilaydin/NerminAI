import os
import json
import time
import uuid
from datetime import datetime
import requests
import streamlit as st

API_BASE_URL = os.getenv("API_BASE_URL", "http://localhost:5054")
CHATS_FILE = os.path.join(os.path.dirname(__file__), "chats.json")
TOP_K = 8
MIN_SIMILARITY = 0.1

# ── Konuşma kayıt/yükle ──────────────────────────────────────────────────────
def load_chats():
    if os.path.exists(CHATS_FILE):
        with open(CHATS_FILE, "r", encoding="utf-8") as f:
            return json.load(f)
    return {}

def save_chats(chats):
    with open(CHATS_FILE, "w", encoding="utf-8") as f:
        json.dump(chats, f, ensure_ascii=False, indent=2)

def chat_title(messages):
    for m in messages:
        if m["role"] == "user":
            t = m["content"][:40]
            return t + ("..." if len(m["content"]) > 40 else "")
    return "Yeni sohbet"

# ── Session state init ───────────────────────────────────────────────────────
if "chat_id" not in st.session_state:
    st.session_state.chat_id = str(uuid.uuid4())
if "messages" not in st.session_state:
    st.session_state.messages = []
if "last_suggestions" not in st.session_state:
    st.session_state.last_suggestions = []

st.set_page_config(page_title="Nermin", page_icon="🌺", layout="wide")

st.markdown("""
<style>
    @import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap');
    html, body, [class*="css"] { font-family: 'Inter', sans-serif; }

    .stApp { background: linear-gradient(135deg, #fff5f0 0%, #ffeef5 50%, #fff0e8 100%); }
    header { visibility: hidden; }
    footer { visibility: hidden; }

    /* Sidebar */
    section[data-testid="stSidebar"] {
        background: linear-gradient(180deg, #fff0f5, #fde8d8) !important;
        border-right: 1px solid rgba(220,120,140,0.15) !important;
    }
    [data-testid="collapsedControl"] { display: flex !important; }

    .block-container { padding-top: 1.5rem; padding-bottom: 2rem; max-width: 750px; }

    .nermin-header {
        text-align: center; padding: 2rem 1.5rem 1.2rem;
        background: linear-gradient(135deg, #fff0f5, #fde8d8);
        border-radius: 24px; margin-bottom: 1.2rem;
        box-shadow: 0 4px 24px rgba(210,80,100,0.08);
        border: 1px solid rgba(220,120,140,0.15);
    }
    .nermin-header h1 {
        font-size: 1.8rem; font-weight: 700;
        background: linear-gradient(135deg, #c8365a, #e8704a);
        -webkit-background-clip: text; -webkit-text-fill-color: transparent;
        background-clip: text; margin: 0 0 0.3rem 0;
    }
    .nermin-header .subtitle { color: #b07080; font-size: 0.88rem; margin: 0; font-style: italic; }
    .nermin-header .tags { margin-top: 0.7rem; display: flex; justify-content: center; gap: 0.4rem; flex-wrap: wrap; }
    .tag { background: rgba(200,80,100,0.1); color: #c0406a; padding: 0.2rem 0.6rem; border-radius: 20px; font-size: 0.72rem; font-weight: 500; }

    [data-testid="stChatMessage"]:has([data-testid="chatAvatarIcon-user"]) {
        background: linear-gradient(135deg, #fce4ec, #ffd6e8) !important;
        border-radius: 18px !important; border: 1px solid rgba(220,130,160,0.2) !important;
    }
    [data-testid="stChatMessage"]:has([data-testid="chatAvatarIcon-assistant"]) {
        background: rgba(255,255,255,0.85) !important; border-radius: 18px !important;
        border: 1px solid rgba(240,200,210,0.4) !important;
    }
    .stChatInput > div {
        border-radius: 24px !important; box-shadow: 0 4px 20px rgba(200,80,120,0.1) !important;
        border: 1.5px solid rgba(210,120,150,0.3) !important; background: white !important;
    }
    .stButton > button {
        background: rgba(255,255,255,0.85) !important; border: 1px solid rgba(210,140,160,0.35) !important;
        border-radius: 14px !important; color: #b06070 !important; font-size: 0.82rem !important;
        font-weight: 500 !important; padding: 0.5rem 0.8rem !important; transition: all 0.2s !important;
    }
    .stButton > button:hover { background: #fce4ec !important; color: #c0406a !important; }
    .stChatMessage .stCaption { color: #d0a8b8 !important; font-size: 0.72rem !important; }
</style>
""", unsafe_allow_html=True)

# ── Sidebar: Sohbet geçmişi ───────────────────────────────────────────────────
with st.sidebar:
    st.markdown("### 🌺 Sohbetler")

    if st.button("+ Yeni Sohbet", use_container_width=True):
        # Mevcut sohbeti kaydet
        if st.session_state.messages:
            chats = load_chats()
            chats[st.session_state.chat_id] = {
                "title": chat_title(st.session_state.messages),
                "messages": st.session_state.messages,
                "date": datetime.now().strftime("%d.%m.%Y %H:%M")
            }
            save_chats(chats)
        st.session_state.chat_id = str(uuid.uuid4())
        st.session_state.messages = []
        st.session_state.last_suggestions = []
        st.rerun()

    st.markdown("---")

    chats = load_chats()
    if chats:
        # En yeni en üstte
        sorted_ids = sorted(chats.keys(), key=lambda k: chats[k].get("date", ""), reverse=True)
        for cid in sorted_ids:
            c = chats[cid]
            is_active = cid == st.session_state.chat_id
            label = ("▶ " if is_active else "") + c["title"]
            col1, col2 = st.columns([5, 1])
            if col1.button(label, key=f"chat_{cid}", use_container_width=True):
                # Mevcut aktif sohbeti kaydet
                if st.session_state.messages:
                    chats[st.session_state.chat_id] = {
                        "title": chat_title(st.session_state.messages),
                        "messages": st.session_state.messages,
                        "date": datetime.now().strftime("%d.%m.%Y %H:%M")
                    }
                st.session_state.chat_id = cid
                st.session_state.messages = c["messages"]
                st.session_state.last_suggestions = []
                save_chats(chats)
                st.rerun()
            if col2.button("🗑", key=f"del_{cid}"):
                del chats[cid]
                save_chats(chats)
                if cid == st.session_state.chat_id:
                    st.session_state.chat_id = str(uuid.uuid4())
                    st.session_state.messages = []
                    st.session_state.last_suggestions = []
                st.rerun()
            st.caption(c.get("date", ""))
    else:
        st.caption("Henüz kayıtlı sohbet yok.")

# ── Ana alan ──────────────────────────────────────────────────────────────────
st.markdown("""
<div class="nermin-header">
    <span style="font-size:3rem">🌺</span>
    <h1>Nermin</h1>
    <p class="subtitle">Hayat dolu, içten, meraklı — bana her şeyi sorabilirsin</p>
    <div class="tags">
        <span class="tag">✈️ Gezgin</span>
        <span class="tag">🐱 Kedi annesi</span>
        <span class="tag">☕ Kahve aşığı</span>
        <span class="tag">🎂 Pastacı ruhu</span>
        <span class="tag">💛 Azerbaycan asıllı</span>
    </div>
</div>
""", unsafe_allow_html=True)

# Mesajları göster
for i, message in enumerate(st.session_state.messages):
    role_label = "user" if message["role"] == "user" else "assistant"
    with st.chat_message(role_label):
        st.markdown(message["content"])
        if role_label == "assistant" and "latency" in message:
            st.caption(f"⏱ {message['latency']}ms")
        if role_label == "assistant" and i == len(st.session_state.messages) - 1:
            if st.button("✏️ Bu cevap yanlış, düzelt", key=f"fix_{i}"):
                st.session_state["correction_mode"] = True
                st.rerun()

# Düzeltme modu
if st.session_state.get("correction_mode"):
    correction = st.text_input("Doğru cevap nedir?", key="correction_input",
                               placeholder="Doğrusu şu...")
    col1, col2 = st.columns([1, 4])
    if col1.button("Gönder", key="send_correction"):
        if correction.strip():
            last_idx = next((i for i in range(len(st.session_state.messages)-1, -1, -1)
                             if st.session_state.messages[i]["role"] == "assistant"), None)
            if last_idx is not None:
                wrong = st.session_state.messages[last_idx]["content"]
                st.session_state.messages[last_idx]["content"] = f"{wrong}\n\n_(Düzeltme: {correction})_"
                st.session_state.messages.append({
                    "role": "user",
                    "content": f"[DÜZELTME] Bir önceki cevabın yanlıştı. Doğrusu: {correction}"
                })

            # Düzeltmeyi veritabanına öğret
            try:
                # Hangi soruya düzeltme yapıldığını bul
                question = ""
                if last_idx and last_idx > 0:
                    for mi in range(last_idx - 1, -1, -1):
                        if st.session_state.messages[mi]["role"] == "user":
                            question = st.session_state.messages[mi]["content"]
                            break

                learn_content = f"Soru: {question}\nDoğru cevap: {correction}" if question else correction

                requests.post(
                    f"{API_BASE_URL}/api/memory/add",
                    json={
                        "title": f"Düzeltme: {question[:50] if question else correction[:50]}",
                        "content": learn_content,
                        "emotionalTone": 0,
                        "category": 0
                    },
                    timeout=15
                )
            except Exception:
                pass  # Kayıt başarısız olsa bile konuşma devam etsin

        st.session_state["correction_mode"] = False
        st.rerun()
    if col2.button("İptal", key="cancel_correction"):
        st.session_state["correction_mode"] = False
        st.rerun()

# ── Chat input ────────────────────────────────────────────────────────────────
typed = st.chat_input("Bir şey sor...")

if "pending_question" in st.session_state:
    prompt = st.session_state.pop("pending_question")
elif typed:
    prompt = typed
else:
    prompt = None

if prompt and prompt not in [m["content"] for m in st.session_state.messages[-1:]]:
    st.session_state.messages.append({"role": "user", "content": prompt})
    with st.chat_message("user"):
        st.markdown(prompt)

    with st.chat_message("assistant"):
        with st.spinner(""):
            try:
                history = [
                    {"role": m["role"], "content": m["content"]}
                    for m in st.session_state.messages[:-1][-6:]
                ]
                payload = {
                    "question": prompt,
                    "topK": TOP_K,
                    "minSimilarity": MIN_SIMILARITY,
                    "includeSources": False,
                    "conversationHistory": history,
                    "lastSuggestions": st.session_state.last_suggestions
                }

                resp = requests.post(f"{API_BASE_URL}/api/personal/ask", json=payload, timeout=30)
                data = resp.json() if resp.status_code == 200 else {}
                answer = data.get("answer", "")

                if answer == "__RATE_LIMIT__":
                    with st.spinner("⏳ Sunucu meşgul, 8 saniye bekleniyor..."):
                        time.sleep(8)
                    resp = requests.post(f"{API_BASE_URL}/api/personal/ask", json=payload, timeout=30)
                    data = resp.json() if resp.status_code == 200 else {}
                    answer = data.get("answer", "")

                if answer == "__RATE_LIMIT__":
                    st.warning("⚠️ Sunucu şu an çok yoğun. 10-15 saniye bekleyip tekrar sor.")
                    st.session_state.messages.pop()
                elif answer in ("__API_ERROR__", "__EMPTY_RESPONSE__", ""):
                    st.warning("⚠️ Cevap alınamadı. Lütfen tekrar dene.")
                    st.session_state.messages.pop()
                else:
                    latency = data.get("latencyMs", 0)
                    st.markdown(answer)
                    st.caption(f"⏱ {latency}ms")
                    st.session_state.last_suggestions.append(answer)
                    if len(st.session_state.last_suggestions) > 5:
                        st.session_state.last_suggestions.pop(0)
                    st.session_state.messages.append({
                        "role": "assistant", "content": answer, "latency": latency
                    })
                    # Her mesajda sohbeti otomatik kaydet
                    chats = load_chats()
                    chats[st.session_state.chat_id] = {
                        "title": chat_title(st.session_state.messages),
                        "messages": st.session_state.messages,
                        "date": datetime.now().strftime("%d.%m.%Y %H:%M")
                    }
                    save_chats(chats)

            except requests.exceptions.ConnectionError:
                st.error("API'ye bağlanılamıyor.")
            except Exception as e:
                st.error(f"Hata: {e}")
