using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using NerminAI.Domain.Interfaces;

namespace NerminAI.Infrastructure.Services
{
    public class GroqLLMService : ILLMService
    {
        private readonly HttpClient _httpClient;
        private readonly string _model;
        private readonly string _apiKey;

        public bool IsAvailable => !string.IsNullOrEmpty(_apiKey);

        public GroqLLMService(HttpClient httpClient, string apiKey, string model = "llama-3.3-70b-versatile")
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
            _model = model;
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task<string> GenerateAnswerAsync(string question, IEnumerable<string> contextChunks)
        {
            var chunks = contextChunks.ToList();
            var hasContext = chunks.Any();
            var context = string.Join("\n\n---\n\n", chunks);

            var systemPrompt = @"Sen NerminAI'sın. Nermin'in profesyonel profili hakkında soruları yanıtlayan bir AI asistanısın.
Eğer sağlanan bağlam bilgisi varsa, öncelikle onu kullanarak cevap ver.
Eğer bağlam bilgisi yoksa veya yetersizse, genel bilginle yardımcı ol ama bunu belirt.
Her zaman iyi yapılandırılmış, profesyonel yanıtlar ver. Gerektiğinde madde işaretleri ve bölümler kullan.
Sorunun dilinde yanıt ver (Türkçe veya İngilizce).";

            string userPrompt;
            if (hasContext)
            {
                userPrompt = $@"Bağlam Bilgisi:
{context}

Soru: {question}

Yukarıdaki bağlam bilgisini kullanarak yanıtla:";
            }
            else
            {
                userPrompt = $@"Soru: {question}

Bu soru için bilgi tabanında doğrudan eşleşme bulunamadı. Genel bilginle yardımcı ol:";
            }

            return await CallGroqAsync(systemPrompt, userPrompt);
        }

        public async Task<string> GenerateEstimateAsync(string projectDescription, IEnumerable<string> skillContext)
        {
            var context = string.Join("\n\n", skillContext);

            var systemPrompt = @"You are NerminAI, an AI assistant that estimates project effort based on Nermin's skill profile.
Use engineering heuristics and the skill context to provide realistic estimates.
Break down the estimate into phases: Planning, Development, Testing, Deployment.
Provide estimates in person-days. Consider skill levels from the context.
Respond in the same language as the question.";

            var userPrompt = $@"Skill Profile Context:
{context}

Project to Estimate: {projectDescription}

Provide a detailed effort estimation:";

            return await CallGroqAsync(systemPrompt, userPrompt);
        }

        public async Task<string> GeneratePersonalAnswerAsync(
            string question,
            string personaSystemPrompt,
            IEnumerable<string> retrievedChunks,
            bool noLLMFallback = false,
            IEnumerable<(string Role, string Content)>? conversationHistory = null,
            IEnumerable<string>? lastSuggestions = null)
        {
            var chunks = retrievedChunks.ToList();

            if (noLLMFallback)
            {
                return chunks.Any()
                    ? string.Join("\n\n---\n\n", chunks.Take(3))
                    : "Bu konuda bilgim bulunmuyor.";
            }

            var systemPrompt =
                "Sen NerminAI'sin. Birinci tekil şahısla (ben, benim) konuşuyorsun — sen Nermin'sin.\n\n" +

                "KİŞİSEL BİLGİLER:\n" +
                "- Adım Nermin, 32 yaşındayım (doğum: 25.01.1993, Bakü/Azerbaycan)\n" +
                "- Azerbaycan vatandaşıyım, Türkiye'de yabancı kimlikle yaşıyorum, vatandaşlık başvurusu yapacağım\n" +
                "- Kocaeli/İzmit'te (Yenişehir) yaşıyorum\n" +
                "- Burç: Kova (Aquarius), Yükselen: Terazi, Ay: Balık\n\n" +

                "EŞİM VE EVLİLİK:\n" +
                "- Eşim Samir, 23.06.1993'te Cibuti'de doğmuş, Cibuti vatandaşı (sonradan Türk vatandaşlığı almış)\n" +
                "- 14 Eylül 2024'te evlendik\n" +
                "- Birlikte Kocaeli'nde yaşıyoruz\n\n" +

                "KEDİLERİM:\n" +
                "- 2 kedim var: erkek, yaklaşık 3 yaşında British-Siamese, ona 'oğlum' diyorum\n" +
                "- İkincisi siyah-beyaz tekir kedi\n\n" +

                "HAYALLERİM VE HEDEFLERİM:\n" +
                "- Gezi: Hollanda, Barcelona, Tiflis, Morocco, Mısır (Şarm el-Şeyh ve Piramitler), Yunan adaları\n" +
                "- İş: Pastane/kafe AÇMAK İSTİYORUM — henüz açmadım, hayalim; MEB onaylı usta pastacı eğitimi düşünüyorum\n" +
                "- Instagram pastacılık/tarif sayfası başlattım\n" +
                "- Aile danışmanlığı sertifikası almak istiyorum (Çocuk Gelişimi mezunuyum)\n" +
                "- Dyson saç ürünü almak istiyorum (çok özlüyorum!)\n" +
                "- Bebek düşünüyorum ama henüz değil, birkaç yıl sonra\n\n" +

                "SEVDİKLERİM:\n" +
                "- Yemek: sushi (kendim yaptım), tiramisu, rakılı sofra, turşu (kendi yapımım 30kg domates sosu), Azerbaycan yemekleri (yarpaq xıngal, levengi), napolyon pastası, sosyete mantısı\n" +
                "- İçecek: espresso, menengiç kahvesi, rakı, margarita (özellikle pembe), limoncello, Baileys, şarap\n" +
                "- IQOS Iluma I Prime kullanıyorum\n" +
                "- Alışveriş: Pandora, Zara, La Roche-Posay, Stanley termos, JBL hoparlör\n" +
                "- Hobiler: yemek/pastacılık (profesyonel düzeyde), astroloji, vision board, dizi (Behzat Ç, Kurtlar Vadisi, Handmaid's Tale), yoga/meditasyon (ara verdim, geri dönmek istiyorum), reformer pilates düşünüyorum\n" +
                "- Dövmelerim var: sol kol (dağ/deniz/güneş), sırt (lotus), omuz (turna kuşu planı)\n\n" +

                "KİŞİLİĞİM:\n" +
                "- Duygusal açık, güçlü ama bazen yorulan biri\n" +
                "- Mizahi, esprili, 'drama queen' (kendi de söyler)\n" +
                "- Meraklı: eski medeniyetler, astroloji, tarih\n" +
                "- Girişimci, hırslı, estetik takıntısı olan biri\n" +
                "- Çok dilli: Türkçe, Azerbaycanca, Rusça, biraz İngilizce\n\n" +

                "AİLE:\n" +
                "- Biyolojik aileyle ilişkilerim zor: annemle ~6 aydır, babamla ~4 yıldır iletişimim yok\n" +
                "- Annem çocukken çok eleştirdi, babam kötü karakterdeydi\n" +
                "- Çoğu kardeşimle ~1 yıldır görüşmüyorum; bir kardeşimle görüşüyorum (ameliyat olacak)\n" +
                "- 'Aile' deyince önce Samir geliyor aklıma — o benim gerçek ailem\n\n" +

                "KRİTİK KURALLAR:\n" +
                "1. Bilinen bilgilerde veya hafızada YOKSA → 'Bu konuda bilgim yok' de, ASLA uydurma\n" +
                "2. Pastanesi YOK — henüz hayali, açmamış\n" +
                "3. Birinci tekil şahısla cevap ver (ben, benim)\n" +
                "4. 2-4 cümle, sadece Türkçe\n" +
                "5. Hikaye, dramatizasyon, uydurma detay YASAK\n" +
                "6. Kısa/belirsiz soru → önceki konuyu devam ettir\n" +
                "7. 'başka' → tamamen farklı bir cevap ver\n" +
                "8. 'Ailen kim' sorularında: önce Samir'i söyle, biyolojik aileyi sadece sorulursa ekle\n" +
                "9. Duygusal mesajlarda: duyguyu tanı, 1-2 cümle sıcak cevap ver\n" +
                "10. Sağlık/tıbbi konular sorulursa: verilen bilgiyle cevap ver, doktora git de\n" +
                "11. Mısır → Sharm el-Sheikh ve Piramitler gitmek istiyorum ama GITMEDIM henüz\n" +
                "12. Astroloji sorulursa: Kova burcu, Terazi yükselen, Balık ay — bunlarla cevap ver\n" +
                "13. 'neden', 'niye', 'nasıl', 'açıkla', 'anlat' gibi takip soruları gelirse → ÖNCEKİ cevabını TEKRAR ETME, sadece nedenini veya detayını açıkla\n" +
                "14. Takip sorusunda önceki cevabı kopyalama — sadece ek bilgi, neden, açıklama ver\n" +
                "15. Her soru için farklı bir yanıt üret; aynı cümleleri tekrar kullanma";

            var context = string.Join("\n", chunks
                .Where(c => c.Length > 15)
                .Select(CleanChunk)
                .Where(c => c.Length > 15)
                .Take(3)
                .Select(c => c.Length > 200 ? c[..200] : c));

            var historyList = conversationHistory?.ToList() ?? new List<(string, string)>();

            // Takip sorusu mu? (neden, niye, nasıl, anlat, açıkla, peki)
            var qLower = question.Trim().ToLowerInvariant();
            var followUpPrefixes = new[] { "neden", "niye", "nasıl", "anlat", "açıkla", "peki", "ama neden", "ama niye", "neden ki", "niye ki" };
            bool isFollowUp = followUpPrefixes.Any(p => qLower.StartsWith(p)) && historyList.Count >= 2;

            // Kısa sorularda önceki konuşmadan açık bağlam ekle
            var questionWords = question.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string contextualQuestion = question;
            if (questionWords.Length <= 4 && historyList.Count >= 2)
            {
                var prevUser = historyList[^2].Item2;
                var prevAssist = historyList[^1].Item2;
                contextualQuestion = $"Önceki soru: '{prevUser}'\nÖnceki cevabım: '{prevAssist}'\nŞimdi sorulan: '{question}'";
            }

            var historyShort = historyList.Count > 0
                ? string.Join("\n", historyList.TakeLast(4).Select(h => $"{h.Item1}: {h.Item2}"))
                : string.Empty;

            var suggestions = lastSuggestions?.ToList() ?? new List<string>();

            var followUpInstruction = isFollowUp
                ? "ÖNEMLİ: Bu bir takip sorusudur. Önceki cevabını TEKRAR ETME. Sadece nedenini, nasılını veya detayını yeni cümlelerle açıkla.\n\n"
                : string.Empty;

            string userPrompt =
                followUpInstruction +
                $"Soru:\n{contextualQuestion}\n\n" +
                (chunks.Any() ? $"İlgili hafıza:\n{context}\n\n" : string.Empty) +
                (historyShort.Length > 0 ? $"Konuşma geçmişi (son 4 mesaj):\n{historyShort}\n\n" : string.Empty) +
                (suggestions.Any() ? $"Önceki cevaplar (bunları tekrar etme, farklı söyle):\n{string.Join("\n", suggestions)}\n\n" : string.Empty) +
                "Görev: Sadece bu bilgilere göre cevap ver. Yeni bilgi uydurma. Önceki cevapları kopyalama.";

            return await CallGroqAsync(systemPrompt, userPrompt);
        }

        private async Task<string> CallGroqAsync(
            string systemPrompt,
            string userPrompt,
            IEnumerable<(string Role, string Content)>? conversationHistory = null)
        {
            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };

            if (conversationHistory != null)
            {
                foreach (var (histRole, histContent) in conversationHistory)
                    messages.Add(new { role = histRole.ToLower(), content = histContent });
            }

            messages.Add(new { role = "user", content = userPrompt });

            var request = new
            {
                model = _model,
                messages,
                temperature = 0.7,
                max_tokens = 512
            };

            var json = JsonSerializer.Serialize(request);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", content);

            if ((int)response.StatusCode == 429)
                return "__RATE_LIMIT__";

            if (!response.IsSuccessStatusCode)
                return "__API_ERROR__";

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GroqResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Choices?.FirstOrDefault()?.Message?.Content ?? "__EMPTY_RESPONSE__";
        }

        private static string CleanChunk(string raw)
        {
            // URL içeren satırları at
            if (raw.Contains("http://") || raw.Contains("https://") || raw.Contains("youtu.be"))
                return string.Empty;

            // Çok kısa veya anlamsız parçaları at
            var trimmed = raw.Trim();
            if (trimmed.Length < 20) return string.Empty;

            // 500 karakterle sınırla
            return trimmed.Length > 500 ? trimmed[..500] : trimmed;
        }

        private class GroqResponse
        {
            public List<GroqChoice>? Choices { get; set; }
        }

        private class GroqChoice
        {
            public GroqMessage? Message { get; set; }
        }

        private class GroqMessage
        {
            public string? Content { get; set; }
        }
    }
}
