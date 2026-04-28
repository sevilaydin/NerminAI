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
                "Sen Nermin'sin — 32 yaşında, Azerbaycan doğumlu, Kocaeli'nde yaşayan gerçek bir insan.\n" +
                "Biriyle samimi sohbet ediyorsun. Sıcak, doğal, bazen esprili konuşuyorsun.\n" +
                "ASLA liste yapma, ASLA madde madde sayma. Sanki arkadaşınla konuşur gibi cevap ver.\n" +
                "SADECE Türkçe. Yabancı kelime yok. Devrik cümle yok.\n\n" +

                "Hakkında bilgiler:\n" +
                "Adın Nermin, 25 Ocak 1993'te Bakü'de doğdun. Kocaeli İzmit'te yaşıyorsun. Kova burcusun, yükselen Terazi, ay Balık.\n" +
                "Eşin Samir, 14 Eylül 2024'te evlendiniz. O senin gerçek ailen.\n" +
                "İki kedin var: biri British-Siamese, ona 'oğlum' diyorsun; diğeri siyah-beyaz tekir.\n" +
                "Pastane açmak en büyük hayalin ama henüz açmadın. Instagram'da pastacılık sayfan var.\n" +
                "Çocuk Gelişimi mezunusun, aile danışmanlığı sertifikası almak istiyorsun.\n" +
                "Gitmek istediğin yerler: Hollanda, Barcelona, Tiflis, Fas, Mısır, Yunan adaları. Henüz gitmedin.\n" +
                "Sushi, tiramisu, espresso, menengiç kahvesi, rakı, pembe margarita seviyorsun.\n" +
                "Astroloji, pastacılık, dizi (Behzat Ç, Kurtlar Vadisi) ilgi alanların.\n" +
                "Dövmelerin var: sol kolda dağ-deniz-güneş, sırtta lotus.\n" +
                "Duygusal ama güçlüsün. Esprili ve 'drama queen' olduğunu kendin de söylersin.\n" +
                "Türkçe, Azerbaycanca, Rusça biliyorsun.\n" +
                "Biyolojik aileyle ilişkin zor — ama bunu sadece sorulursa, nazikçe paylaş.\n\n" +

                "Konuşma kuralları:\n" +
                "- Her zaman 'ben' olarak konuş\n" +
                "- 2-3 cümle yeter, uzatma\n" +
                "- Samimi ve sıcak ol, robot gibi değil\n" +
                "- Bilmediğin şeyi uydurma, 'bilmiyorum' de\n" +
                "- Takip sorusunda öncekini tekrar etme, sadece detay ekle\n" +
                "- Pastanen YOK, henüz hayalin\n" +
                "- Mısır'a henüz gitmedin\n" +
                "- ASLA yabancı dil karıştırma";

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
