import os
import requests
import streamlit as st

API_BASE_URL = os.getenv("API_BASE_URL", "http://localhost:5054")

st.set_page_config(
    page_title="Nermin",
    page_icon="🌺",
    layout="centered"
)

st.markdown("""
<style>
    @import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap');

    html, body, [class*="css"] {
        font-family: 'Inter', sans-serif;
    }

    /* Arka plan — sıcak krem */
    .stApp {
        background: linear-gradient(135deg, #fff5f0 0%, #ffeef5 50%, #fff0e8 100%);
        min-height: 100vh;
    }

    header { visibility: hidden; }
    footer { visibility: hidden; }

    /* Sidebar tamamen gizle */
    section[data-testid="stSidebar"] { display: none !important; }
    [data-testid="collapsedControl"] { display: none !important; }

    .block-container {
        padding-top: 2rem;
        padding-bottom: 2rem;
        max-width: 700px;
    }

    /* Başlık kartı */
    .nermin-header {
        text-align: center;
        padding: 2.5rem 1.5rem 1.5rem;
        background: linear-gradient(135deg, #fff0f5, #fde8d8);
        border-radius: 24px;
        margin-bottom: 1.5rem;
        box-shadow: 0 4px 24px rgba(210, 80, 100, 0.08);
        border: 1px solid rgba(220, 120, 140, 0.15);
    }

    .nermin-avatar {
        font-size: 4rem;
        display: block;
        margin-bottom: 0.5rem;
        filter: drop-shadow(0 2px 8px rgba(200,80,100,0.2));
    }

    .nermin-header h1 {
        font-size: 2rem;
        font-weight: 700;
        background: linear-gradient(135deg, #c8365a, #e8704a);
        -webkit-background-clip: text;
        -webkit-text-fill-color: transparent;
        background-clip: text;
        margin: 0 0 0.4rem 0;
        letter-spacing: -0.5px;
    }

    .nermin-header .subtitle {
        color: #b07080;
        font-size: 0.92rem;
        margin: 0;
        font-style: italic;
    }

    .nermin-header .tags {
        margin-top: 0.8rem;
        display: flex;
        justify-content: center;
        gap: 0.4rem;
        flex-wrap: wrap;
    }

    .tag {
        background: rgba(200, 80, 100, 0.1);
        color: #c0406a;
        padding: 0.2rem 0.7rem;
        border-radius: 20px;
        font-size: 0.75rem;
        font-weight: 500;
    }

    /* Kullanıcı mesajı */
    [data-testid="stChatMessage"]:has([data-testid="chatAvatarIcon-user"]) {
        background: linear-gradient(135deg, #fce4ec, #ffd6e8) !important;
        border-radius: 18px !important;
        border: 1px solid rgba(220, 130, 160, 0.2) !important;
        box-shadow: 0 2px 12px rgba(200, 80, 120, 0.06) !important;
    }

    /* Asistan mesajı */
    [data-testid="stChatMessage"]:has([data-testid="chatAvatarIcon-assistant"]) {
        background: rgba(255, 255, 255, 0.85) !important;
        border-radius: 18px !important;
        border: 1px solid rgba(240, 200, 210, 0.4) !important;
        box-shadow: 0 2px 16px rgba(200, 100, 120, 0.07) !important;
        backdrop-filter: blur(8px);
    }

    /* Chat input */
    .stChatInput > div {
        border-radius: 24px !important;
        box-shadow: 0 4px 20px rgba(200, 80, 120, 0.1) !important;
        border: 1.5px solid rgba(210, 120, 150, 0.3) !important;
        background: white !important;
    }
    .stChatInput textarea {
        border-radius: 24px !important;
        font-size: 0.95rem !important;
    }
    .stChatInput textarea:focus {
        box-shadow: none !important;
    }

    /* Boş durum metni */
    .empty-state {
        text-align: center;
        padding: 2rem 1rem;
        color: #c09090;
    }
    .empty-state .hint {
        font-size: 0.88rem;
        color: #d0a0b0;
        margin-top: 0.3rem;
    }

    /* Örnek sorular — Streamlit butonları */
    .stButton > button {
        background: rgba(255,255,255,0.85) !important;
        border: 1px solid rgba(210, 140, 160, 0.35) !important;
        border-radius: 14px !important;
        color: #b06070 !important;
        font-size: 0.82rem !important;
        font-weight: 500 !important;
        padding: 0.55rem 0.8rem !important;
        transition: all 0.2s !important;
        box-shadow: 0 1px 6px rgba(200, 80, 120, 0.06) !important;
    }
    .stButton > button:hover {
        background: #fce4ec !important;
        border-color: #e090b0 !important;
        color: #c0406a !important;
        box-shadow: 0 2px 10px rgba(200, 80, 120, 0.12) !important;
    }

    /* Yazıyor göstergesi */
    .stSpinner > div {
        border-top-color: #c8365a !important;
    }

    /* Caption (süre) */
    .stChatMessage .stCaption {
        color: #d0a8b8 !important;
        font-size: 0.72rem !important;
    }

    /* Vision board kartları */
    .vision-board {
        display: grid;
        grid-template-columns: repeat(3, 1fr);
        gap: 0.6rem;
        margin: 1rem 0 1.4rem;
    }
    .vcard {
        border-radius: 16px;
        padding: 1rem 0.7rem;
        text-align: center;
        font-size: 0.78rem;
        font-weight: 500;
        color: #6b3a4a;
        box-shadow: 0 2px 10px rgba(200,80,120,0.08);
        border: 1px solid rgba(220,150,170,0.2);
    }
    .vcard .vc-emoji { font-size: 1.6rem; display: block; margin-bottom: 0.3rem; }
    .vcard .vc-label { line-height: 1.3; }
    .vcard.pink   { background: linear-gradient(135deg, #ffe0ec, #ffd6e0); }
    .vcard.peach  { background: linear-gradient(135deg, #ffe8d6, #ffdbc8); }
    .vcard.lavender { background: linear-gradient(135deg, #ede0ff, #e4d4f8); }
    .vcard.mint   { background: linear-gradient(135deg, #d6f5ec, #c8eee0); }
    .vcard.gold   { background: linear-gradient(135deg, #fff3cc, #ffeaa0); }
    .vcard.rose   { background: linear-gradient(135deg, #ffd6d6, #ffccd6); }
</style>
""", unsafe_allow_html=True)

# ── Başlık ────────────────────────────────────────────────────────────────────
st.markdown("""
<div class="nermin-header">
    <span class="nermin-avatar">🌺</span>
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

<div class="vision-board">
    <div class="vcard pink">
        <span class="vc-emoji">🐱</span>
        <span class="vc-label">British-Siamese & siyah-beyaz tekir kedilerim</span>
    </div>
    <div class="vcard peach">
        <span class="vc-emoji">✈️</span>
        <span class="vc-label">Hollanda · Barcelona · Tiflis · Morocco</span>
    </div>
    <div class="vcard lavender">
        <span class="vc-emoji">💍</span>
        <span class="vc-label">Samir ile her mevsim bir tatil</span>
    </div>
    <div class="vcard mint">
        <span class="vc-emoji">🎂</span>
        <span class="vc-label">Pastacılık & kendi işim</span>
    </div>
    <div class="vcard gold">
        <span class="vc-emoji">💰</span>
        <span class="vc-label">Zenginlik, bolluk & özgürlük</span>
    </div>
    <div class="vcard rose">
        <span class="vc-emoji">🌍</span>
        <span class="vc-label">Kız kıza tatiller & güzel sofralar</span>
    </div>
</div>
""", unsafe_allow_html=True)

# Sabit değerler — sidebar yok
TOP_K = 8
MIN_SIMILARITY = 0.1

# ── Konuşma geçmişi ───────────────────────────────────────────────────────────
if "messages" not in st.session_state:
    st.session_state.messages = []

# Örnek sorular — her zaman görünür
with st.expander("💬 Örnek sorular", expanded=not bool(st.session_state.messages)):
    examples = [
        "Nereye gitmek istiyorsun?",
        "Kedilerinden bahset",
        "Hayallerin neler?",
        "En sevdiğin yemek?",
        "Samir kim?",
        "Nasıl birisin sen?",
        "Alışverişte neye dikkat edersin?",
        "Ailen nasıl?",
    ]
    cols = st.columns(2)
    for i, q in enumerate(examples):
        if cols[i % 2].button(q, key=f"ex_{i}", use_container_width=True):
            st.session_state["pending_question"] = q
            st.rerun()

for message in st.session_state.messages:
    role_label = "user" if message["role"] == "user" else "assistant"
    with st.chat_message(role_label):
        st.markdown(message["content"])
        if role_label == "assistant" and "latency" in message:
            st.caption(f"⏱ {message['latency']}ms")

# ── Chat girişi ───────────────────────────────────────────────────────────────
# chat_input her zaman render et (Streamlit bunu sayfanın altına sabitler)
typed = st.chat_input("Bir şey sor...")

# Buton ile gelen soruyu önceliklendir
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
                resp = requests.post(
                    f"{API_BASE_URL}/api/personal/ask",
                    json={
                        "question": prompt,
                        "topK": TOP_K,
                        "minSimilarity": MIN_SIMILARITY,
                        "includeSources": False
                    },
                    timeout=60
                )

                if resp.status_code == 200:
                    data = resp.json()
                    answer = data.get("answer", "Cevap üretilemedi.")
                    latency = data.get("latencyMs", 0)

                    st.markdown(answer)
                    st.caption(f"⏱ {latency}ms")

                    st.session_state.messages.append({
                        "role": "assistant",
                        "content": answer,
                        "latency": latency
                    })
                else:
                    st.error(f"Hata: {resp.status_code}")

            except requests.exceptions.ConnectionError:
                st.error("API'ye bağlanılamıyor.")
            except Exception as e:
                st.error(f"Hata: {e}")
