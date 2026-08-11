# -*- coding: utf-8 -*-
"""Generate and print UTF-8 seed SQL (ASCII source only)."""
from pathlib import Path

def u(*parts: str) -> str:
    return "".join(parts)

FIRST = [
    "Ahmet", "Mehmet", u("Ay", "\u015fe"), "Fatma", "Emre", "Elif", "Can", "Zeynep",
    "Burak", "Deniz", "Merve", "Cem", "Selin", u("O", "\u011fuz"), "Ece", "Kerem",
    u("\u0130rem"), "Yusuf", "Seda", "Tolga", "Gizem", "Baran", "Pelin", "Onur",
    "Derya", "Hakan", "Ceren", "Serkan", "Melis", "Arda", "Naz", "Berk", "Sibel",
    "Umut", "Esra", "Furkan", "Hazal", "Kaan", "Lale", "Murat", "Nil",
    u("R", "\u0131za"), "Su", "Taner", "Vildan", u("Ya", "\u011f", "\u0131z"),
    "Asya", "Bora", "Defne", "Efe",
]

LAST = [
    u("Y", "\u0131lmaz"), "Kaya", "Demir", u("\u015eahin"), u("\u00c7elik"),
    u("Y", "\u0131ld", "\u0131z"), u("Ayd", "\u0131n"), u("\u00d6zt", "\u00fcrk"),
    "Arslan", u("Do", "\u011fan"), u("K", "\u0131l", "\u0131\u00e7"), "Aslan",
    u("Ko\u00e7"), "Kurt", u("\u00d6zdemir"), "Polat", u("Erdo", "\u011fan"),
    "Tekin", "Aksoy", u("G\u00fcne\u015f"), "Bulut", "Karaca", "Acar", "Bozkurt",
    u("\u00c7etin"), "Duman", "Eren", u("G\u00fcler"), u("I\u015f\u0131k"), "Kartal",
]

CITIES = [
    u("\u0130stanbul"), "Ankara", u("\u0130zmir"), "Bursa", "Antalya", "Adana",
    "Konya", "Gaziantep", "Mersin", "Kayseri", u("Eski\u015fehir"), "Samsun",
    "Trabzon", u("Diyarbak\u0131r"), "Malatya", "Sakarya", "Manisa",
    u("Bal\u0131kesir"), "Van", "Denizli",
]

DOMAINS = ["gmail.com", "hotmail.com", "yahoo.com", "outlook.com", "local"]

TR_MAP = str.maketrans({
    "\u015f": "s", "\u015e": "s",
    "\u0131": "i", "\u0130": "i",
    "\u011f": "g", "\u011e": "g",
    "\u00fc": "u", "\u00dc": "u",
    "\u00f6": "o", "\u00d6": "o",
    "\u00e7": "c", "\u00c7": "c",
})


def slug(s: str) -> str:
    return s.translate(TR_MAP).lower()


def esc(s: str) -> str:
    return s.replace("'", "''")


# deterministic "random" phones
phones = []
x = 5321847392
for i in range(50):
    x = (x * 1103515245 + 12345) & 0x7FFFFFFF
    phones.append("5" + f"{x % 10_000_000_000:09d}"[:9])

rows = []
for i in range(50):
    fn = FIRST[i % len(FIRST)]
    ln = LAST[(i * 7 + 3) % len(LAST)]
    email = f"{slug(fn)}.{slug(ln)}{i + 1}@{DOMAINS[i % len(DOMAINS)]}"
    city = CITIES[i % len(CITIES)]
    occ = (i % 6) + 1
    rows.append(
        f"('{esc(fn)}', '{esc(ln)}', '{phones[i]}', '{email}', '{esc(city)}', {occ})"
    )

sql = """BEGIN;
DELETE FROM "Contacts";

INSERT INTO "Contacts" ("FirstName", "LastName", "PhoneNumber", "Email", "City", "OccupationId") VALUES
""" + ",\n".join(rows) + """;

COMMIT;

SELECT "Id", "FirstName", "LastName", "PhoneNumber", "Email", "City"
FROM "Contacts"
ORDER BY "Id"
LIMIT 8;

SELECT COUNT(*) AS contact_count FROM "Contacts";
"""

out = Path(__file__).with_name("seed-contacts.sql")
out.write_text(sql, encoding="utf-8")
print(f"Wrote {out} ({out.stat().st_size} bytes)")
# sanity: must contain UTF-8 for İ (C4 B0)
raw = out.read_bytes()
assert b"\xc4\xb0stanbul" in raw or "İstanbul".encode("utf-8") in raw
print("UTF-8 check OK")
