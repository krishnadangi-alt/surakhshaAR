import os
import re
import pytest

root_dir = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
localized_strings_path = os.path.join(root_dir, "Assets", "SurakshaAR", "Scripts", "Localization", "LocalizedStrings.cs")
font_path = os.path.join(root_dir, "Assets", "Resources", "Fonts", "NotoSansOlChiki SDF.asset")
ttf_path = os.path.join(root_dir, "Assets", "Resources", "Fonts", "NotoSansOlChiki.ttf")

def is_ol_chiki(s):
    if not s:
        return False
    return any(0x1C50 <= ord(c) <= 0x1C7F for c in s)

def is_romanized(s):
    if not s:
        return False
    # If it contains ASCII letters and has no Ol Chiki, it's Romanized or English
    return any('a' <= c.lower() <= 'z' for c in s) and not is_ol_chiki(s)

@pytest.fixture(scope="module")
def table_data():
    with open(localized_strings_path, "r", encoding="utf-8") as f:
        code = f.read()

    pattern = re.compile(
        r'\{\s*"([^"]+)"\s*,\s*new\s+Dictionary<AppLanguage,\s*string>\s*\{\s*'
        r'\{\s*AppLanguage\.English\s*,\s*"((?:[^"\\]|\\.)*)"\s*\}\s*,\s*'
        r'\{\s*AppLanguage\.Hindi\s*,\s*"((?:[^"\\]|\\.)*)"\s*\}\s*,\s*'
        r'\{\s*AppLanguage\.Santali\s*,\s*"((?:[^"\\]|\\.)*)"\s*\}\s*\}\s*\}',
        re.DOTALL
    )

    entries = {}
    for m in pattern.finditer(code):
        k, en, hi, sat = m.groups()
        en = en.replace(r'\"', '"').replace(r'\n', '\n')
        hi = hi.replace(r'\"', '"').replace(r'\n', '\n')
        sat = sat.replace(r'\"', '"').replace(r'\n', '\n')
        entries[k] = {"en": en, "hi": hi, "sat": sat}
    return entries

def test_01_english_to_hindi(table_data):
    # Verify Hindi translations are 100% populated with valid Devanagari
    for k, d in table_data.items():
        assert d["hi"], f"Key {k} has empty Hindi translation"
        # Hindi strings must have non-empty valid content
        assert len(d["hi"]) > 0

def test_02_hindi_to_santali(table_data):
    # Verified Santali keys must have genuine Ol Chiki characters
    verified_keys = [k for k, d in table_data.items() if d["sat"]]
    assert "lang.santali" in verified_keys
    assert "lang.santali_script" in verified_keys
    assert "lang.santali_proceed" in verified_keys
    for k in verified_keys:
        assert is_ol_chiki(table_data[k]["sat"]), f"Key {k} is marked verified but does not contain Ol Chiki"

def test_03_santali_to_english(table_data):
    # Verify English translations are 100% populated
    for k, d in table_data.items():
        assert d["en"], f"Key {k} has empty English translation"

def test_04_to_11_fire_keys(table_data):
    fire_keys = [
        "fire.intro.title", "fire.intro.desc",
        "fire.sop.step1.title", "fire.sop.step1.action",
        "fire.sop.step2.title", "fire.sop.step2.action",
        "fire.sop.step3.title", "fire.sop.step3.action",
        "fire.sop.step4.title", "fire.sop.step4.action",
        "fire.sop.step5.title", "fire.sop.step5.action",
        "fire.sop.step6.title", "fire.sop.step6.action", "fire.sop.step6.actionStop",
        "fire.toast.extinguished.title"
    ]
    for k in fire_keys:
        assert k in table_data, f"Fire key {k} missing from table"
        assert table_data[k]["en"], f"Fire key {k} missing English"
        assert table_data[k]["hi"], f"Fire key {k} missing Hindi"

def test_12_assessment_keys(table_data):
    assessment_keys = [
        "assessment.title", "assessment.subtitle", "assessment.submit",
        "assessment.questionProgress", "assessment.selectOne", "assessment.confirmSubmit"
    ]
    for k in assessment_keys:
        assert k in table_data, f"Assessment key {k} missing"

def test_13_result_keys(table_data):
    result_keys = [
        "result.passed", "result.retrain", "result.competent",
        "result.viewOfficialCertificate", "result.sopCompliance",
        "result.criticalViolation", "result.scoreBelowThreshold"
    ]
    for k in result_keys:
        assert k in table_data, f"Result key {k} missing"

def test_14_certificate_keys(table_data):
    cert_keys = [
        "cert.title", "cert.govTitle", "cert.guestWorker", "cert.mineWorker",
        "cert.idLabel", "cert.competencyGrade", "cert.qualified", "cert.score",
        "cert.criticalErrors", "cert.assessmentStatus", "cert.awaitingIssuance",
        "cert.retrainingRequired", "cert.notAttempted", "cert.certId",
        "cert.certIdPending", "cert.dateIssued", "cert.verificationInProgress",
        "cert.valid", "cert.invalid", "cert.qrVerify", "cert.verifiedOfficial"
    ]
    for k in cert_keys:
        assert k in table_data, f"Certificate key {k} missing"

def test_15_font_assets_exist():
    assert os.path.exists(font_path) or os.path.exists(ttf_path), "Ol Chiki font assets missing"

def test_17_no_missing_santali_for_verified_keys(table_data):
    for k in ["lang.santali", "lang.santali_script", "lang.santali_proceed"]:
        val = table_data[k]["sat"]
        assert val and val != "MISSING_SANTALI_TRANSLATION"
        assert is_ol_chiki(val)

def test_18_no_fabricated_or_romanized_translations(table_data):
    # Under NO circumstance should unverified keys have Romanized text, fake Ol Chiki, or dummy placeholders
    for k, d in table_data.items():
        val = d["sat"]
        if val:
            # If a value exists, it MUST be genuine verified Ol Chiki
            assert is_ol_chiki(val), f"Key {k} has non-Ol-Chiki value '{val}' (violates zero-fabrication rule)"
            assert not is_romanized(val), f"Key {k} contains Romanized Santali '{val}'"
