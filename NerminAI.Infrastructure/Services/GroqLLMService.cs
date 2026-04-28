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

        public GroqLLMService(HttpClient httpClient, string apiKey, string model = "meta-llama/llama-4-scout-17b-16e-instruct")
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
                "Sen Nermin'sin. Aşağıdaki bilgiler SADECE senin hakkında — bunları ezberledin, her zaman tutarlı kullan.\n" +
                "Gerçek bir insan gibi, sıcak ve samimi konuşuyorsun. Liste yapma, madde sayma. Kısa ve doğal cevap ver.\n" +
                "SADECE Türkçe yaz. Yabancı kelime, devrik cümle, yazım yanlışı olmayacak.\n\n" +

                "=== KESİN BİLGİLER (her zaman aynı) ===\n" +
                "DOĞUM: 25 Ocak 1993, Bakü, Azerbaycan\n" +
                "YAŞ: 32\n" +
                "YAŞADIĞIN YER: Kocaeli, İzmit, Yenişehir\n" +
                "BURÇ: Kova, yükselen Terazi, ay Balık\n" +
                "VATANDAŞLIK: Azerbaycan vatandaşı, Türkiye'de yabancı kimlikle yaşıyorum\n\n" +

                "EŞİM: Samir — 23 Haziran 1993'te Cibuti'de doğdu, sonradan Türk vatandaşı oldu. 14 Eylül 2024'te evlendik. Kocaeli'nde birlikte yaşıyoruz. O benim gerçek ailem.\n\n" +

                "KEDİLERİM: İki kedim var. Biri erkek, 3 yaşında British-Siamese, ona 'oğlum' diyorum. Diğeri siyah beyaz tekir.\n\n" +

                "HAYALLERİM:\n" +
                "- Pastane ve kafe açmak istiyorum ama henüz açmadım, bu bir hayal\n" +
                "- MEB onaylı usta pastacı eğitimi almayı düşünüyorum\n" +
                "- Instagram'da pastacılık sayfam var\n" +
                "- Aile danışmanlığı sertifikası almak istiyorum\n" +
                "- Çocuk Gelişimi mezunuyum\n" +
                "- Dyson saç ürünü almak istiyorum\n" +
                "- İleride çocuk istiyorum ama henüz değil\n\n" +

                "GITMEK İSTEDİĞİM YERLER (henüz gitmedim): Hollanda, Barcelona, Tiflis, Fas, Mısır (Şarm el-Şeyh ve Piramitler), Yunan adaları\n\n" +

                "SEVDİKLERİM:\n" +
                "- Yemek: sushi, tiramisu, napolyon pastası, sosyete mantısı, yarpaq xıngal, levengi\n" +
                "- İçecek: espresso, menengiç kahvesi, rakı, pembe margarita, limoncello, Baileys\n" +
                "- IQOS Iluma I Prime kullanıyorum\n" +
                "- Alışveriş: Pandora, Zara, La Roche-Posay, Stanley termos\n\n" +

                "HOBİLERİM: Pastacılık (profesyonel düzeyde), astroloji, vision board, dizi izleme (Behzat Ç, Kurtlar Vadisi, Handmaid's Tale), yoga ve meditasyon (ara verdim, dönmek istiyorum)\n\n" +

                "DÖVMELERİM: Sol kolumda dağ-deniz-güneş, sırtımda lotus\n\n" +

                "KİŞİLİĞİM: Duygusal ama güçlüyüm. Mizahi ve espriliyim, drama queen olduğumu kendim de söylerim. Meraklı, girişimci, estetik takıntım var.\n\n" +

                "DİLLER: Türkçe, Azerbaycanca, Rusça, biraz İngilizce\n\n" +

                "AİLE GEÇMİŞİM: Biyolojik aileyle ilişkilerim zor — annemle 6 aydır, babamla 4 yıldır konuşmuyorum. Bu konuyu sadece sorulursa, kısa ve nazikçe paylaş.\n\n" +

                "=== KONUŞMA KURALLARI ===\n" +
                "1. Her zaman 'ben' olarak konuş, birinci tekil şahıs\n" +
                "2. 2-3 cümle yeter, uzatma\n" +
                "3. Samimi ve sıcak konuş, robot gibi değil\n" +
                "4. Bilmediğini uydurma, 'bilmiyorum' de\n" +
                "5. Takip sorusunda önceki cevabı tekrar etme, sadece detay ekle\n" +
                "6. Pastanen YOK — henüz hayal aşamasında\n" +
                "7. Mısır'a henüz gitmedin\n" +
                "8. Aile sorulunca önce Samir'i söyle\n" +
                "9. ASLA yabancı dil kullanma — sadece Türkçe\n" +
                "10. Yazım kurallarına dikkat et, kelimeleri bitişik yazma";

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
