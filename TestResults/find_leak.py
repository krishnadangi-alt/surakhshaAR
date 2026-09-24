import re

with open(r"c:\project\surakshaAR\Assets\SurakshaAR\Scripts\Localization\LocalizedStrings.cs", encoding="utf-8") as f:
    lines = f.readlines()

for i, line in enumerate(lines):
    if "AppLanguage.Hindi" in line:
        m = re.search(r'\{ AppLanguage\.Hindi,\s*"([^"]*)"\s*\}', line)
        if m:
            hi = m.group(1)
            cleaned = re.sub(r"(CO<sub>2</sub>|CO2|AR|SMS|OTP|ID|PIN|App|3D)", "", hi)
            letters = re.findall(r"[a-zA-Z]{4,}", cleaned)
            if letters:
                # Find the key on previous lines
                for prev in range(max(0, i-5), i):
                    if '"' in lines[prev]:
                        key_m = re.search(r'\{\s*"([^"]+)"', lines[prev])
                        if key_m:
                            print(f"Line {i+1}: Key={key_m.group(1)}, Word={letters}")
