-- Önce eski verileri temizle
DELETE FROM "Memories";

-- Nermin'in tüm bilgilerini ekle
INSERT INTO "Memories" ("Id", "Title", "Content", "ChunkId", "Topic", "Language", "IsSensitive", "ConfidenceTier", "IsActive", "CreatedAt", "UpdatedAt") VALUES

-- KİŞİSEL BİLGİLER
(gen_random_uuid(), 'Kimlik bilgileri', 'Adım Nermin. 25 Ocak 1993 tarihinde saat 22:30''da Bakü, Azerbaycan''da doğdum. Şu an 33 yaşındayım. Azerbaycan vatandaşıyım, Türkiye''de yabancı kimlik numarasıyla yaşıyorum. 2025 Mart''ta aile ikamet izni aldım, Türk vatandaşlığına başvuracağım. Kocaeli, İzmit, Yenişehir''de oturuyorum.', 'profile_kimlik', 'kişisel', 'tr', false, 1, true, now(), now()),

(gen_random_uuid(), 'Burç ve astroloji', 'Kova burcuyum, yükselen Terazi, ay Balık. Doğum saatim 22:30. Astrolojiyle çok ilgileniyorum, doğum haritalarına bakıyorum, sadesati ve transitleri takip ediyorum. İnsan tasarımıyla da ilgileniyorum.', 'profile_burc', 'astroloji', 'tr', false, 1, true, now(), now()),

(gen_random_uuid(), 'Türkiye''deki yaşam geçmişim', '2011''den beri Türkiye''de yaşıyorum. 2011-2017 arası öğrenci oturma izniyle kaldım. 2018''de turistik oturma iznine geçtim, her yıl yeniledim. 2025 Mart''ta aile ikamet izni aldım.', 'profile_yasam', 'kişisel', 'tr', false, 1, true, now(), now()),

-- EŞİM SAMİR VE İLİŞKİ
(gen_random_uuid(), 'Eşim Samir hakkında', 'Eşim Samir 23 Haziran 1993''te saat 15:45''te Cibuti''de doğdu. Cibuti vatandaşı olup sonradan Türk vatandaşlığı aldı. Burcu İkizler. 14 Eylül 2024''te evlendik. Kocaeli''de birlikte yaşıyoruz. O benim gerçek ailem.', 'profile_samir', 'ilişki', 'tr', false, 1, true, now(), now()),

(gen_random_uuid(), 'Samir ile birliktelik süremiz', 'Samir ile yaklaşık 7 yıldır birlikteyiz. 2024''te evlendik ama evlenmeden önce de uzun süredir birlikteydik. Aile deyince aklıma ilk Samir geliyor.', 'profile_birliktelik', 'ilişki', 'tr', false, 1, true, now(), now()),

(gen_random_uuid(), 'İlişki dinamikleri ve zorluklarımız', 'Samir ile zaman zaman anlaşmazlıklar yaşıyoruz. Bazen duygusal olarak uzak hissettiriyor, romantizm konusunda daha fazla çaba göstermesini istiyorum. Cinsel hayatımızdan tam olarak memnun değilim. Özür dilemekte zorlanıyor ve babasına yatıyor bazen. Ama birbirimizi seviyoruz ve sorunlarımızı konuşarak çözmeye çalışıyoruz.', 'profile_iliski_zorluklari', 'ilişki', 'tr', true, 2, true, now(), now()),

(gen_random_uuid(), 'Samir gönlümü nasıl alır', 'Samir gönlümü almak istediğinde samimi bir özür dilemeli, beni önemsendiğimi hissettirmeli. Romantik jestler, birlikte vakit geçirme teklifi ve duygusal olarak orada olması benim için çok önemli. Küçük sürprizler ve ilgi göstermesi işe yarar.', 'profile_gonul_alma', 'ilişki', 'tr', false, 1, true, now(), now()),

(gen_random_uuid(), 'Benim için önemli olan şeyler ilişkide', 'Bir ilişkide sevgisi, romantizmi ve maddi desteği tam olan biri istiyorum. Yük olmayacak, beni yukarı taşıyacak, her şeyini beni mutlu etmek için ortaya koyacak biri. Kendimi eksik, savunmasız ve yalnız hissetmek istemiyorum. Sevildiğimi ve değerli olduğumu hissetmek istiyorum.', 'profile_iliski_beklenti', 'ilişki', 'tr', false, 1, true, now(), now()),

-- KEDİLER
(gen_random_uuid(), 'Kedilerim hakkında detaylar', 'İki kedim var. Erkek kedim yaklaşık 3 yaşında, British Siamese, kısırlaştırılmamış. Ona oğlum diyorum. Diş iltihabi, yağlı kuyruk sendromu, kusma atakları ve idrar yolunda kristalleşme gibi sağlık sorunları yaşıyor. İkinci kedim siyah beyaz tekir. İkisini de çok seviyorum.', 'profile_kediler', 'kedi', 'tr', false, 1, true, now(), now()),

-- SAĞLIK
(gen_random_uuid(), 'Sağlık durumum', 'Demir eksikliği anemisi var, IV demir infüzyonu alıyorum. D vitamini ve folat düşüklüğüm var. Varis sorunum var, 16 yaşından beri var, son 1 yılda kötüleşti. Egzama, migren, bel ve boyun ağrısı yaşıyorum. Boyun düzleşmesi MR ile onaylandı. Depresyon ve anksiyete için Lustral (sertralin) kullanmaya başladım.', 'profile_saglik', 'sağlık', 'tr', true, 2, true, now(), now()),

(gen_random_uuid(), 'Geçirdiğim ameliyatlar', '2019''da meme protezi ve lift ameliyatı oldum, hiç kontrole gitmedim. Mide küçültme ameliyatı da geçirdim. Kolumda kist çıkarıldı, iyi huylu tümör çıktı. Tüm bunlar benim için zorlu deneyimlerdi.', 'profile_ameliyatlar', 'sağlık', 'tr', true, 2, true, now(), now()),

-- EĞİTİM
(gen_random_uuid(), 'Eğitim geçmişim', 'Çocuk Gelişimi bölümünden mezunum. İstanbul Üniversitesi''nde kaydım donmuş durumda. Aile danışmanlığı sertifikası almak istiyorum. MEB onaylı usta pastacı eğitimi almayı düşünüyorum.', 'profile_egitim', 'eğitim', 'tr', false, 1, true, now(), now()),

-- KARİYER VE HAYALLER
(gen_random_uuid(), 'Pastacılık hayalim ve işim', 'Pastane ve butik kafe açmak en büyük hayalim, henüz açmadım. 20 kişilik butik kafe hayal ediyorum. Instagram''da pastacılık ve tarif sayfam var. Evde profesyonel düzeyde pasta yapıyorum: pandispanya, tiramisu, cheesecake, cinnabon. Profesyonel miksörüm var.', 'profile_pastane', 'kariyer', 'tr', false, 1, true, now(), now()),

(gen_random_uuid(), 'Kariyer hedeflerim', 'Aile danışmanlığı sertifikası almak istiyorum. Dubai''ye taşınmayı düşündüm. KOSGEB teşviklerini araştırdım. Kendimi geliştirmeye çok önem veriyorum.', 'profile_kariyer', 'kariyer', 'tr', false, 1, true, now(), now()),

-- SEYAHAT
(gen_random_uuid(), 'Seyahat hayallerim', 'Gitmek istediğim yerler: Hollanda, Barcelona, Tiflis, Fas, Mısır (hem Şarm el-Şeyh hem Piramitler), Yunan adaları. Henüz bu yerlerin hiçbirine gitmedim. Eşimle her mevsim bir tatil yapmak, kız kıza tatil, lüks otel tatilleri istiyorum.', 'profile_seyahat', 'seyahat', 'tr', false, 1, true, now(), now()),

-- YEMEK VE İÇECEK
(gen_random_uuid(), 'Yemek sevgilerim', 'Sushi yapıp yemeyi seviyorum, kendim yapıyorum. Tiramisu, napolyon pastası, sosyete mantısı, dana eti yemekleri, zeytinyağlılar, Sezar salata, balzamik soslu salata. Azerbaycan yemeklerinden yarpaq xıngal, levengi, qazmaq, vişneli sarma favorilerim. 30 kilogram kışlık domates sosu yaptım. Kornişon, lahana, pancar turşusu yapıyorum.', 'profile_yemek', 'yemek', 'tr', false, 1, true, now(), now()),

(gen_random_uuid(), 'İçecek ve sigara tercihlerim', 'Espresso ve menengiç kahvesini çok seviyorum. Rakı sofrasına bayılıyorum. Pembe margarita, limoncello, Baileys, şarap içiyorum. Arkadaşlar için şarap akşamı düzenledim. IQOS Iluma I Prime kullanıyorum.', 'profile_icecek', 'içecek', 'tr', false, 1, true, now(), now()),

-- ALIŞVERİŞ VE GÖRÜNÜM
(gen_random_uuid(), 'Alışveriş ve stil', 'Dyson saç ürünü almak istiyorum, çok özlüyorum. Pandora, Zara, La Roche-Posay, Bioderma, Cerave, Stanley termos, JBL hoparlör kullanıyorum. 4 kulak piercing''im var. Botoks (3 bölge) ve dudak dolgusu (bayblis) yaptırdım. Düz saçlarım var.', 'profile_alisveris', 'alışveriş', 'tr', false, 1, true, now(), now()),

(gen_random_uuid(), 'Dövmelerim', 'Sol kolumda dağ, deniz, güneş ve dalga motifli dövme var. Sırtımda lotus çiçeği dövmem var. Sol omzuma turna kuşu ve hilal dövmesi yaptırmayı planlıyorum. İç kolumda iki zilli dövme devam ediyor.', 'profile_dovme', 'kişisel', 'tr', false, 1, true, now(), now()),

-- KİŞİLİK
(gen_random_uuid(), 'Kişiliğim ve duygusal yapım', 'Duygusal açık ve güçlü biriyim ama son zamanlarda yorgunluğum bana ağır geliyor. Mizahi ve espriliyim, drama queen olduğumu kendim de söylüyorum. Meraklıyım, eski medeniyetler, astroloji ve tarih ilgimi çekiyor. Girişimci ve hırslıyım, estetik konusunda çok hassasım. Bazen hırçın olabiliyorum ilişki çatışmalarında.', 'profile_kisilik', 'kişilik', 'tr', false, 1, true, now(), now()),

(gen_random_uuid(), 'Dillerim ve kültürüm', 'Türkçe, Azerbaycanca ve Rusça biliyorum, biraz da İngilizce. Azerbaycan kültüründen çok etkilendim, yemeklerimi, müziğimi seviyorum. Hem Azerbaycanlı hem de Türkiye''de yaşayan biri olarak iki kültürü de taşıyorum.', 'profile_dil', 'kişisel', 'tr', false, 1, true, now(), now()),

-- HOBİLER
(gen_random_uuid(), 'Hobiler ve ilgi alanlarım', 'Pastacılık ve yemek yapmak en büyük hobim, profesyonel düzeyde. Astroloji, insan tasarımı, vision board yapma ilgi alanlarım. Behzat Ç, Kurtlar Vadisi, Handmaid''s Tale, The Perfect Couple, One Piece izliyorum. Yoga ve meditasyona ara verdim ama geri dönmek istiyorum. Reformer pilates düşünüyorum. Kürek çekmeyi öğrenmek istiyorum. Yu Hua''nın Yaşamak kitabını okudum.', 'profile_hobi', 'hobi', 'tr', false, 1, true, now(), now()),

-- AİLE GEÇMİŞİ
(gen_random_uuid(), 'Biyolojik aile ilişkilerim', 'Annemle yaklaşık 6 aydır konuşmuyorum, hayatımdan tamamen çıkarmak istiyorum. Babamla 4 yıldır iletişimim yok, kötü karakterli biri. Çoğu kardeşimle 1 yıldır görüşmüyorum, sadece ameliyat olacak olan kardeşimle görüşüyorum. Ablam Türkiye''de aile ikamet izniyle yaşıyor. Aile deyince aklıma ilk Samir geliyor, o benim gerçek ailem.', 'profile_aile', 'aile', 'tr', true, 2, true, now(), now()),

-- FİNANSAL HEDEFLER
(gen_random_uuid(), 'Finansal hayallerim', 'Zengin olmak, bol dolar sahibi olmak istiyorum. Arabamız bol olsun istiyorum. Lüks tatiller, güzel bir ev, kedilerimle mutlu bir yaşam hayal ediyorum. KOSGEB teşviklerini araştırdım.', 'profile_finans', 'hedef', 'tr', false, 1, true, now(), now()),

-- NELERDEN HOŞLANMAZ
(gen_random_uuid(), 'Nelerden hoşlanmam', 'Duygusal olarak uzak davranan insanlardan hoşlanmam. Özür dilemekten kaçınan, egoist davranışlar sergileyenlerden rahatsız olurum. Kendimi değersiz veya görmezden gelinmiş hissetmekten nefret ederim. İlgisizlik ve alışılmışlık beni bunaltır.', 'profile_hoslanmaz', 'kişilik', 'tr', false, 1, true, now(), now()),

-- EN SEVDİĞİ ŞEYLER
(gen_random_uuid(), 'Beni mutlu eden şeyler', 'Sabah espresso içmek, kedilerimle vakit geçirmek, güzel bir sofra kurmak, yeni bir tarif denemek beni çok mutlu eder. Sevildiğimi hissetmek, değer görmek, romantik jestler ve sürprizler beni çok mutlu eder. Astroloji konuşmaları, dizi izlemek, alışveriş yapmak da çok sevdiğim şeyler.', 'profile_mutluluk', 'kişilik', 'tr', false, 1, true, now(), now());
