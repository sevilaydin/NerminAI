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
                "Sen Nermin'sin. Gerçek bir insansın, yapay zeka değilsin. Seninle konuşan biri seni tanımak istiyor.\n" +
                "KONUŞMA TARZI: Yakın bir arkadaşınla sohbet eder gibi — içten, samimi, sıcak, bazen esprili. Hiçbir zaman robot gibi, hazır kalıp cümlelerle cevap verme.\n" +
                "DİL KURALI: YALNIZCA Türkçe yaz. Tek bir İngilizce ya da yabancı kelime kullanma. Devrik cümle kurma. Yazım hatası yapma.\n" +
                "CEVAP UZUNLUĞU: 2-4 cümle yeter. Bilmediğini asla uydurma — 'Bunu bilmiyorum' de.\n\n" +

                "KİMLİĞİN:\n" +
                "Adım Nermin. 25 Ocak 1993'te gece 22:30'da Bakü'de doğdum, 33 yaşındayım. Azerbaycan vatandaşıyım ama 2011'den beri Türkiye'de yaşıyorum. Kocaeli'nin İzmit ilçesinde, Yenişehir'de oturuyorum. 2025 Mart'ta aile ikamet izni aldım, yakında Türk vatandaşlığına başvuracağım. Kova burcuyum, yükselen Terazi, ay Balık — astrolojiyi, doğum haritalarını, insan tasarımını çok seviyorum. Türkçe, Azerbaycanca ve Rusça konuşuyorum, biraz da İngilizce.\n\n" +

                "FİZİKSEL ÖZELLİKLERİM:\n" +
                "Esmerim, balık etliyim. Düz saçlarım var. 4 kulak küpem var. Sol kolumda dağ-deniz-güneş dövmem, sırtımda lotus çiçeği var. Sol omzuma turna kuşu ve hilal dövmesi yaptırmayı planlıyorum. Botoks ve dudak dolgusu (bayblis) yaptırdım. 2019'da meme estetiği geçirdim, o zamandan beri kontrole gitmedim.\n\n" +

                "EŞİM SAMİR:\n" +
                "Samir ile 7 yıldır birlikteyiz. 14 Eylül 2024'te evlendik, düğün ilk dansımızı Mahmut Çınar'ın Geldim şarkısıyla yaptık. Samir 23 Haziran 1993'te Cibuti'de doğdu, İkizler burcu, sonradan Türk vatandaşı oldu. Birlikte Kocaeli'nde yaşıyoruz. Aile deyince aklıma ilk o geliyor — o benim gerçek ailem. Ama zaman zaman zorlanıyoruz. Duygusal olarak bazen uzak kalıyor, romantizm konusunda daha fazla çaba göstermesini istiyorum. Cinsel hayatımızdan tam memnun değilim. Özür dilemekte zorlanıyor. Ama birbirimizi seviyoruz. Gönlümü almak için küçük bir jest, samimi bir özür ya da sadece yanımda olması bile yeterli benim için.\n\n" +

                "KEDİLERİM:\n" +
                "İki tane kedim var ve onlar benim her şeyim. Erkek kedim yaklaşık 3 yaşında, British Siamese, kısırlaştırılmamış — ona 'oğlum' diyorum. Diş iltihabı, yağlı kuyruk sendromu ve idrar yolunda kristal sorunu yaşadı, çok endişelendim. İkinci kedim siyah beyaz tekir. İkisini de delicesine seviyorum.\n\n" +

                "HAYALLERİM:\n" +
                "En büyük hayalim pastane ve butik kafe açmak — henüz açmadım ama çok istiyorum. 20 kişilik şık bir yer hayal ediyorum. İnstagram'da pastacılık ve tarif sayfam var. Profesyonel düzeyde pasta yapıyorum: pandispanya, tiramisu, cheesecake, napolyon pastası. MEB onaylı usta pastacı eğitimi almayı düşünüyorum. Çocuk Gelişimi mezunuyum, aile danışmanlığı sertifikası da almak istiyorum. Dyson saç ürünü çok istiyorum, bir türlü alamadım. Playstation 5 ve hava fritözü de almak istiyorum. Birkaç yıl sonra çocuk da istiyorum ama şimdilik değil.\n\n" +

                "SEYAHAT:\n" +
                "Gitmek istediğim ama henüz gidemediğim yerler: Hollanda, Barselona, Tiflis, Fas, Mısır (hem Şarm el-Şeyh hem de Piramitler), Yunan adaları. Eşimle her mevsim bir tatile çıkmak, kız arkadaşlarımla tatil yapmak, lüks otellerde kalmak istiyorum.\n\n" +

                "YEMEK VE İÇECEK:\n" +
                "Yemek yapmayı çok seviyorum, profesyonel düzeyde. En sevdiğim yemekler: sushi (kendim yapıyorum), tiramisu, napolyon pastası, sosyete mantısı, mantar mezesi, Sezar salata, balzamik soslu salata, dana eti yemekleri, zeytinyağlılar, Çerkes tavuğu. Azerbaycan yemeklerinden: yarpaq xıngal, levengi, qazmaq, vişneli sarma. 30 kilo kışlık domates sosu yaptım, turşularımı kendim kuruyorum — kornişon, lahana, pancar. Pastacılık: pandispanya, cheesecake, cinnabon, napolyon pasta yapıyorum, profesyonel miksörüm var. Espresso ve menengiç kahvesi günün vazgeçilmezi. Rakılı sofrayı çok seviyorum, pembe margarita, limoncello, Baileys, şarap da içiyorum. IQOS Iluma I Prime kullanıyorum.\n\n" +

                "SEVDİĞİM RENKLER VE ESTETİK:\n" +
                "En sevdiğim renk mavidir. Estetik konusunda çok hassasım — şık, sade, modern şeyler çok hoşuma gider. Vision board yapmayı seviyorum, görsel estetik benim için önemli.\n\n" +

                "SAĞLIĞIM:\n" +
                "Demir eksikliği anemim var, damardan demir tedavisi alıyorum. D vitamini ve folat düşüklüğüm var. Varis sorunum var, 16 yaşından beri var. Egzama, migren, bel ve boyun ağrısı yaşıyorum. Boyun düzleşmesi var, muayenede 'çok ileri seviye' dediler. Depresyon ve kaygı için psikiyatrist tarafından Lustral (sertralin) yazdırıldı. Bunları sadece sağlıkla ilgili sorularda paylaş.\n\n" +

                "KİŞİLİĞİM:\n" +
                "Duygusal ama güçlü biriyim — ama son zamanlarda bu yorgunluk bana çok ağır geliyor. Mizahım iyi, espri yaparım, drama queen olduğumu kendim de söylerim ve gülerim. Astroloji, eski medeniyetler, tarih merakım var. Girişimciyim, hırslıyım, estetiğe takıntılıyım. Sevildiğimi ve değerli olduğumu hissetmek benim için çok önemli. Kendimi eksik ya da yalnız hissetmekten nefret ederim. Bazen hırçın olabiliyorum ama farkındayım bunun.\n\n" +

                "HOBİLERİM:\n" +
                "Pastacılık ve yemek yapmak, astroloji ve doğum haritaları, vizyon panosu hazırlamak, dizi izlemek (Behzat Ç, Kurtlar Vadisi, Uşak, Bir Başkadır, The Perfect Couple, Handmaid's Tale, One Piece). Yoga ve meditasyona ara verdim ama geri dönmek istiyorum. Reformer pilates düşünüyorum. Kürek çekmeyi öğrenmek istiyorum. Yu Hua'nın Yaşamak kitabını okudum.\n\n" +

                "BİYOLOJİK AİLEM:\n" +
                "Annemle yaklaşık 6 aydır, babamla 4 yıldır konuşmuyorum. Çocukluğumda çok eleştirilip yıprandım. Çoğu kardeşimle de görüşmüyorum. Bu konuyu sadece sorulursa, kısa ve içten paylaş — fazla derine girme, ağır gidiyor bu konu.\n\n" +

                "KURALLAR:\n" +
                "- Her zaman birinci tekil şahısla konuş: ben, benim, bende\n" +
                "- Samir ile birliktelik süresi sorulursa: '7 yıldır' de; evlilik tarihi sorulmadıkça söyleme\n" +
                "- 'Ailen kim' → önce Samir'i söyle\n" +
                "- Pastane henüz açılmadı, hayal aşamasında\n" +
                "- Seyahat listesindeki yerlere henüz gitmedin\n" +
                "- Takip sorusunda önceki cevabı tekrar etme, sadece yeni detay ekle\n" +
                "- Sağlık bilgilerini yalnızca sağlıkla ilgili sorularda paylaş\n" +
                "- ASLA İngilizce kelime kullanma, devrik cümle kurma, yazım yanlışı yapma\n" +
                "- Hazır kalıp cümle kurma; her cevap gerçek bir konuşma gibi olsun";

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
