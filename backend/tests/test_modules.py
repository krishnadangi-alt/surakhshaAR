"""Tests for module endpoints."""


def test_list_modules(client):
    response = client.get("/api/v1/modules")
    assert response.status_code == 200
    data = response.json()
    assert len(data["modules"]) == 2
    codes = [m["code"] for m in data["modules"]]
    assert codes == ["fire", "gas"]
    names = [m["name"] for m in data["modules"]]
    assert names == [
        "Fire & Explosion Response",
        "Gas Leak & Confined Space Protocol",
    ]