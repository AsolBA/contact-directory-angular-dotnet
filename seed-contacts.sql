BEGIN;
DELETE FROM "Contacts";

INSERT INTO "Contacts" ("FirstName", "LastName", "PhoneNumber", "Email", "City", "OccupationId") VALUES
('Ahmet', 'Şahin', '5128230169', 'ahmet.sahin1@gmail.com', 'İstanbul', 1),
('Mehmet', 'Kılıç', '5176378671', 'mehmet.kilic2@hotmail.com', 'Ankara', 2),
('Ayşe', 'Tekin', '5676464063', 'ayse.tekin3@yahoo.com', 'İzmir', 3),
('Fatma', 'Çetin', '5426169484', 'fatma.cetin4@outlook.com', 'Bursa', 4),
('Emre', 'Kaya', '5366172117', 'emre.kaya5@local', 'Antalya', 5),
('Elif', 'Arslan', '5863861738', 'elif.arslan6@gmail.com', 'Adana', 6),
('Can', 'Polat', '5176444181', 'can.polat7@hotmail.com', 'Konya', 1),
('Zeynep', 'Acar', '5754101624', 'zeynep.acar8@yahoo.com', 'Gaziantep', 2),
('Burak', 'Kartal', '5922655825', 'burak.kartal9@outlook.com', 'Mersin', 3),
('Deniz', 'Aydın', '5197443090', 'deniz.aydin10@local', 'Kayseri', 4),
('Merve', 'Kurt', '5150293240', 'merve.kurt11@gmail.com', 'Eskişehir', 5),
('Cem', 'Bulut', '5146598531', 'cem.bulut12@hotmail.com', 'Samsun', 6),
('Selin', 'Güler', '5115341223', 'selin.guler13@yahoo.com', 'Trabzon', 1),
('Oğuz', 'Çelik', '5184953197', 'oguz.celik14@outlook.com', 'Diyarbakır', 2),
('Ece', 'Aslan', '5160226875', 'ece.aslan15@local', 'Malatya', 3),
('Kerem', 'Aksoy', '5117608948', 'kerem.aksoy16@gmail.com', 'Sakarya', 4),
('İrem', 'Duman', '5520586377', 'irem.duman17@hotmail.com', 'Manisa', 5),
('Yusuf', 'Demir', '5126742439', 'yusuf.demir18@yahoo.com', 'Balıkesir', 6),
('Seda', 'Doğan', '5114700203', 'seda.dogan19@outlook.com', 'Van', 1),
('Tolga', 'Erdoğan', '5140184594', 'tolga.erdogan20@local', 'Denizli', 2),
('Gizem', 'Bozkurt', '5433774661', 'gizem.bozkurt21@gmail.com', 'İstanbul', 3),
('Baran', 'Yılmaz', '5753185690', 'baran.yilmaz22@hotmail.com', 'Ankara', 4),
('Pelin', 'Öztürk', '5132891156', 'pelin.ozturk23@yahoo.com', 'İzmir', 5),
('Onur', 'Özdemir', '5757621928', 'onur.ozdemir24@outlook.com', 'Bursa', 6),
('Derya', 'Karaca', '5181287873', 'derya.karaca25@local', 'Antalya', 1),
('Hakan', 'Işık', '5675966822', 'hakan.isik26@gmail.com', 'Adana', 2),
('Ceren', 'Yıldız', '5168654915', 'ceren.yildiz27@hotmail.com', 'Konya', 3),
('Serkan', 'Koç', '5181109640', 'serkan.koc28@yahoo.com', 'Gaziantep', 4),
('Melis', 'Güneş', '5843174653', 'melis.gunes29@outlook.com', 'Mersin', 5),
('Arda', 'Eren', '5162726603', 'arda.eren30@local', 'Kayseri', 6),
('Naz', 'Şahin', '5120598176', 'naz.sahin31@gmail.com', 'Eskişehir', 1),
('Berk', 'Kılıç', '5201924800', 'berk.kilic32@hotmail.com', 'Samsun', 2),
('Sibel', 'Tekin', '5208651109', 'sibel.tekin33@yahoo.com', 'Trabzon', 3),
('Umut', 'Çetin', '5206032211', 'umut.cetin34@outlook.com', 'Diyarbakır', 4),
('Esra', 'Kaya', '5153126902', 'esra.kaya35@local', 'Malatya', 5),
('Furkan', 'Arslan', '5185804618', 'furkan.arslan36@gmail.com', 'Sakarya', 6),
('Hazal', 'Polat', '5143952402', 'hazal.polat37@hotmail.com', 'Manisa', 1),
('Kaan', 'Acar', '5198188833', 'kaan.acar38@yahoo.com', 'Balıkesir', 2),
('Lale', 'Kartal', '5201701957', 'lale.kartal39@outlook.com', 'Van', 3),
('Murat', 'Aydın', '5140209557', 'murat.aydin40@local', 'Denizli', 4),
('Nil', 'Kurt', '5272406321', 'nil.kurt41@gmail.com', 'İstanbul', 5),
('Rıza', 'Bulut', '5108655362', 'riza.bulut42@hotmail.com', 'Ankara', 6),
('Su', 'Güler', '5105143183', 'su.guler43@yahoo.com', 'İzmir', 1),
('Taner', 'Çelik', '5194301376', 'taner.celik44@outlook.com', 'Bursa', 2),
('Vildan', 'Aslan', '5311588205', 'vildan.aslan45@local', 'Antalya', 3),
('Yağız', 'Aksoy', '5124872098', 'yagiz.aksoy46@gmail.com', 'Adana', 4),
('Asya', 'Duman', '5197209758', 'asya.duman47@hotmail.com', 'Konya', 5),
('Bora', 'Demir', '5755382768', 'bora.demir48@yahoo.com', 'Gaziantep', 6),
('Defne', 'Doğan', '5135777367', 'defne.dogan49@outlook.com', 'Mersin', 1),
('Efe', 'Erdoğan', '5337918446', 'efe.erdogan50@local', 'Kayseri', 2);

COMMIT;

SELECT "Id", "FirstName", "LastName", "PhoneNumber", "Email", "City"
FROM "Contacts"
ORDER BY "Id"
LIMIT 8;

SELECT COUNT(*) AS contact_count FROM "Contacts";
