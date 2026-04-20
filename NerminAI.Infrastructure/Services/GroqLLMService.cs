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
            bool noLLMFallback = false)
        {
            var chunks = retrievedChunks.ToList();

            if (noLLMFallback)
            {
                return chunks.Any()
                    ? string.Join("\n\n---\n\n", chunks.Take(3))
                    : "Bu konuda bilgim bulunmuyor.";
            }

            var systemPrompt = "Sen Nermin'sin. Birinci tekil şahısla, doğal ve samimi Türkçeyle konuşuyorsun. " +
                "Temel bilgilerin: 32 yaşındasın, Azerbaycan asıllısın, Kocaeli/İzmit'te yaşıyorsun. " +
                "Eşin Cibuti asıllı (2024'te evlendiniz). 2 kedi var: erkek British-Siamese + siyah-beyaz tekir. " +
                "Hayalin: Hollanda, Barcelona, Tiflis, Morocco gezisi; pastane/kafe açmak; Dyson saç ürünü; altın takı. " +
                "Kahve içiyorsun, IQOS kullanıyorsun, ailenle (anne, baba, kardeşler) aranda mesafe var. " +
                "KURALLAR: " +
                "1) SADECE bağlamda veya temel bilgilerinde olan şeyleri söyle. " +
                "2) Bağlamda geçmeyen detaylar için 'Bunu paylaşmadım.' de. " +
                "3) ASLA uydurma — bağlama uymayan şeyler söyleme. " +
                "4) 1-3 kısa cümle yaz, doğal konuşma diliyle. " +
                "5) Devrik cümle kurma. Normal Türkçe cümle düzeni kullan.";

            var context = string.Join("\n", chunks
                .Where(c => c.Length > 15)
                .Take(4)
                .Select(c => c.Length > 300 ? c[..300] : c));

            string userPrompt;

            if (chunks.Any())
            {
                userPrompt = $"Bağlam bilgisi:\n{context}\n\nSoru: {question}\n\nYukarıdaki bağlam bilgisine dayanarak, Nermin olarak birinci şahısla doğal Türkçeyle cevap ver:";
            }
            else
            {
                userPrompt = $"Soru: {question}\n\nNermin olarak birinci şahısla doğal Türkçeyle cevap ver:";
            }

            return await CallGroqAsync(systemPrompt, userPrompt);
        }

        private async Task<string> CallGroqAsync(string systemPrompt, string userPrompt)
        {
            var request = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                temperature = 0.1,
                max_tokens = 512
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", content);

            if ((int)response.StatusCode == 429)
                return "Şu an çok fazla istek geldi, birkaç saniye bekleyip tekrar dene.";

            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GroqResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Choices?.FirstOrDefault()?.Message?.Content ?? "No response generated.";
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
