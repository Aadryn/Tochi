#!/usr/bin/env python3
"""Final cleanup of remaining French words in C# documentation file."""

import codecs
import re

# File path
file_path = "/workspaces/proxy/.github/instructions/csharp.documentation.instructions.md"

# Read the file with UTF-8 encoding (no BOM)
with codecs.open(file_path, 'r', encoding='utf-8-sig') as f:
    content = f.read()

# Final translations - targeted replacements
final_translations = {
    # Isolated verbs/adjectives
    r'\bRetourne\b': 'Returns',
    r'\bObtient\b': 'Gets',
    r'\bobtient\b': 'gets',
    r'\bValide\b': 'Valid',
    r'\bvalide\b': 'valid',
    
    # Nouns
    r'\bdonnées\b': 'data',
    r'\bméthode\b': 'method',
    r'\bméthodes\b': 'methods',
    r'\bpropriété\b': 'property',
    r'\butilisateur\b': 'user',
    r'\butilisateurs\b': 'users',
    
    # Specific phrases found in grep
    r"Cette méthode vérifie": "This method checks",
    r"Détails supplémentaires, cas d'usage, contraintes": "Additional details, use cases, constraints",
    r"Lien vers classe, méthode ou propriété liée": "Link to related class, method or property",
    r"contenant les données désérialisées": "containing the deserialized data",
    r"n'est pas un JSON valide": "is not valid JSON",
    r"pour la désérialisation": "for deserialization",
    r"Vague descriptions": "Vague descriptions",
    r'Gère les données': 'Handles data',
    r"Fait quelque chose": "Does something",
    r'GetUser obtient un utilisateur': 'GetUser gets a user',
    r'Retourne a liste': 'Returns the list',
    r"Retourne a collection": "Returns a collection",
    r"filtrés selon leur statut": "filtered by their status",
    r"si l'adresse est valide": "if the address is valid",
    r'Trouvé .+ utilisateurs': lambda m: m.group(0).replace('utilisateurs', 'users').replace('Trouvé', 'Found'),
}

print("Applying final cleanup translations...")
count = 0

for french_phrase, english_phrase in final_translations.items():
    if callable(english_phrase):
        # Lambda function replacement
        matches = list(re.finditer(french_phrase, content))
        if matches:
            for match in reversed(matches):  # Replace from end to avoid offset issues
                replacement = english_phrase(match)
                content = content[:match.start()] + replacement + content[match.end():]
                count += 1
                print(f"  ✓ {match.group(0)[:60]}...")
    else:
        # Simple replacement
        if re.search(french_phrase, content):
            old_content = content
            content = re.sub(french_phrase, english_phrase, content)
            if content != old_content:
                count += 1
                print(f"  ✓ {french_phrase} → {english_phrase}")

# Write back the file with UTF-8 encoding (no BOM)
with codecs.open(file_path, 'w', encoding='utf-8') as f:
    f.write(content)

print(f"\n✅ Applied {count} final cleanup translations")
print(f"✅ File updated: {file_path}")

# Final check for remaining French words
french_words_pattern = r'\b(utilisateur|données|méthode|propriété|fonction|retourne|obtient|définit|calcule|valide|enregistre|Retourne|Obtient|Valide)\b'
remaining = set(re.findall(french_words_pattern, content, re.IGNORECASE))

if remaining:
    print(f"\n⚠️ Found {len(remaining)} unique French words remaining:")
    for word in sorted(remaining, key=str.lower):
        print(f"  - {word}")
else:
    print("\n✅ No French words detected! Translation complete.")
