import pytest
import requests

BASE_URL = "https://playspot-production.up.railway.app"

USER_A = {"email": "usera@playspot.test", "password": "s"}
USER_B = {"email": "userb@playspot.test", "password": "s"}


# ── Helpers ────────────────────────────────────────────────────────────────────

def login(credentials):
    r = requests.post(f"{BASE_URL}/api/Auth/login", json=credentials)
    assert r.status_code == 200, f"Login failed: {r.text}"
    data = r.json()
    return data["token"], data["userId"]


def auth_headers(token):
    return {"Authorization": f"Bearer {token}"}


def create_event(token):
    payload = {
        "title": "Test Event",
        "description": "Automated test event",
        "location": "Skopje",
        "date": "2026-12-01T10:00:00",
        "sport": "Football",
        "maxParticipants": 10,
        "isPublic": True
    }
    r = requests.post(f"{BASE_URL}/api/Event", json=payload, headers=auth_headers(token))
    assert r.status_code in (200, 201), f"Create event failed: {r.text}"
    return r.json()["id"]


# ── Auth Tests ─────────────────────────────────────────────────────────────────

class TestRegister:

    def test_valid_registration(self):
        import time
        payload = {
            "firstName": "Test",
            "lastName": "User",
            "username": f"pytest_user_{int(time.time())}",
            "email": f"pytest_{int(time.time())}@test.com",
            "password": "Password@123!",
            "city": "Skopje"
        }
        r = requests.post(f"{BASE_URL}/api/Auth/register", json=payload)
        assert r.status_code == 200
        assert "token" in r.json()

    def test_duplicate_email_should_fail(self):
        payload = {
            "firstName": "Test", "lastName": "User",
            "username": "dupeuser", "email": "usera@playspot.test",
            "password": "s", "city": "Skopje"
        }
        r = requests.post(f"{BASE_URL}/api/Auth/register", json=payload)
        # BUG #1: currently returns 200, should be 400
        assert r.status_code != 200, "BUG: Duplicate email accepted"

    def test_empty_email_should_fail(self):
        payload = {
            "firstName": "Test", "lastName": "User",
            "username": "emptyemail", "email": "",
            "password": "Password@123!", "city": "Skopje"
        }
        r = requests.post(f"{BASE_URL}/api/Auth/register", json=payload)
        # BUG #1: currently returns 200, should be 400
        assert r.status_code == 400, f"BUG: Empty email accepted with {r.status_code}"

    def test_empty_password_should_fail(self):
        payload = {
            "firstName": "Test", "lastName": "User",
            "username": "emptypass", "email": "emptypass@test.com",
            "password": "", "city": "Skopje"
        }
        r = requests.post(f"{BASE_URL}/api/Auth/register", json=payload)
        # BUG #1: currently returns 200, should be 400
        assert r.status_code == 400, f"BUG: Empty password accepted with {r.status_code}"

    def test_xss_in_username_should_be_rejected(self):
        import time
        payload = {
            "firstName": "Test", "lastName": "User",
            "username": "<script>alert(1)</script>",
            "email": f"xss_{int(time.time())}@test.com",
            "password": "Password@123!", "city": "Skopje"
        }
        r = requests.post(f"{BASE_URL}/api/Auth/register", json=payload)
        # BUG #2: currently returns 200 and stores XSS payload
        assert r.status_code == 400, f"BUG: XSS in username accepted with {r.status_code}"


class TestLogin:

    def test_valid_login(self):
        r = requests.post(f"{BASE_URL}/api/Auth/login", json=USER_A)
        assert r.status_code == 200
        assert "token" in r.json()

    def test_wrong_password(self):
        r = requests.post(f"{BASE_URL}/api/Auth/login",
                          json={"email": USER_A["email"], "password": "wrongpassword"})
        assert r.status_code == 401

    def test_user_not_found(self):
        r = requests.post(f"{BASE_URL}/api/Auth/login",
                          json={"email": "ghost@nowhere.com", "password": "whatever"})
        assert r.status_code == 401

    def test_sql_injection_in_email(self):
        r = requests.post(f"{BASE_URL}/api/Auth/login",
                          json={"email": "' OR 1=1--", "password": "anything"})
        assert r.status_code != 200, "CRITICAL BUG: SQL injection succeeded"

    def test_empty_fields(self):
        r = requests.post(f"{BASE_URL}/api/Auth/login",
                          json={"email": "", "password": ""})
        assert r.status_code in (400, 401)


# ── Event Tests ────────────────────────────────────────────────────────────────

class TestEvent:

    def setup_method(self):
        self.token_a, self.user_a_id = login(USER_A)
        self.token_b, self.user_b_id = login(USER_B)

    def test_create_event_no_token(self):
        r = requests.post(f"{BASE_URL}/api/Event", json={"title": "test"})
        assert r.status_code == 401

    def test_create_valid_event(self):
        event_id = create_event(self.token_a)
        assert isinstance(event_id, int)

    def test_xss_in_event_title(self):
        payload = {
            "title": "<script>alert(1)</script>",
            "description": "test", "location": "Skopje",
            "date": "2026-12-01T10:00:00", "sport": "Football",
            "maxParticipants": 10, "isPublic": True
        }
        r = requests.post(f"{BASE_URL}/api/Event", json=payload,
                          headers=auth_headers(self.token_a))
        # BUG #3: currently returns 201
        assert r.status_code == 400, f"BUG: XSS in event title accepted with {r.status_code}"

    def test_edit_own_event(self):
        event_id = create_event(self.token_a)
        r = requests.put(f"{BASE_URL}/api/Event/{event_id}",
                         json={"title": "Updated", "description": "Updated",
                               "location": "Skopje", "date": "2026-12-01T10:00:00",
                               "sport": "Football", "maxParticipants": 10, "isPublic": True},
                         headers=auth_headers(self.token_a))
        assert r.status_code == 200

    def test_edit_other_users_event(self):
        event_id = create_event(self.token_a)
        r = requests.put(f"{BASE_URL}/api/Event/{event_id}",
                         json={"title": "Hacked", "description": "Hacked",
                               "location": "Skopje", "date": "2026-12-01T10:00:00",
                               "sport": "Football", "maxParticipants": 10, "isPublic": True},
                         headers=auth_headers(self.token_b))
        assert r.status_code == 403

    def test_delete_own_event(self):
        event_id = create_event(self.token_a)
        r = requests.delete(f"{BASE_URL}/api/Event/{event_id}",
                            headers=auth_headers(self.token_a))
        assert r.status_code in (200, 204)

    def test_delete_other_users_event(self):
        event_id = create_event(self.token_a)
        r = requests.delete(f"{BASE_URL}/api/Event/{event_id}",
                            headers=auth_headers(self.token_b))
        assert r.status_code == 403


# ── JoinRequest Tests ──────────────────────────────────────────────────────────

class TestJoinRequest:

    def setup_method(self):
        self.token_a, self.user_a_id = login(USER_A)
        self.token_b, self.user_b_id = login(USER_B)
        self.event_id = create_event(self.token_a)

    def test_join_no_token(self):
        r = requests.post(f"{BASE_URL}/api/JoinRequest/{self.event_id}")
        assert r.status_code == 401

    def test_valid_join(self):
        r = requests.post(f"{BASE_URL}/api/JoinRequest/{self.event_id}",
                          headers=auth_headers(self.token_b))
        assert r.status_code in (200, 201)

    def test_duplicate_join(self):
        requests.post(f"{BASE_URL}/api/JoinRequest/{self.event_id}",
                      headers=auth_headers(self.token_b))
        r = requests.post(f"{BASE_URL}/api/JoinRequest/{self.event_id}",
                          headers=auth_headers(self.token_b))
        assert r.status_code == 400

    def test_invalid_event_id(self):
        r = requests.post(f"{BASE_URL}/api/JoinRequest/99999",
                          headers=auth_headers(self.token_b))
        assert r.status_code in (400, 404)


# ── Report & Comment Tests ─────────────────────────────────────────────────────

class TestReportAndComment:

    def setup_method(self):
        self.token_a, self.user_a_id = login(USER_A)
        self.token_b, self.user_b_id = login(USER_B)
        self.event_id = create_event(self.token_a)

    def test_report_no_token(self):
        r = requests.post(f"{BASE_URL}/api/event/{self.event_id}/report",
                          json={"reason": "spam"})
        assert r.status_code == 401

    def test_report_invalid_event(self):
        r = requests.post(f"{BASE_URL}/api/event/99999/report",
                          json={"reason": "spam"},
                          headers=auth_headers(self.token_a))
        # BUG #6: currently returns 500
        assert r.status_code in (400, 404), f"BUG: Got {r.status_code} instead of 404"

    def test_comment_no_token(self):
        r = requests.post(f"{BASE_URL}/api/Comment/event/{self.event_id}",
                          json={"content": "hello"})
        assert r.status_code == 401

    def test_comment_xss(self):
        r = requests.post(f"{BASE_URL}/api/Comment/event/{self.event_id}",
                          json={"content": "<script>alert(1)</script>"},
                          headers=auth_headers(self.token_b))
        # BUG #5: currently returns 201
        assert r.status_code == 400, f"BUG: XSS in comment accepted with {r.status_code}"

    def test_comment_invalid_event(self):
        r = requests.post(f"{BASE_URL}/api/Comment/event/99999",
                          json={"content": "hello"},
                          headers=auth_headers(self.token_a))
        # BUG #7: currently returns 500
        assert r.status_code in (400, 404), f"BUG: Got {r.status_code} instead of 404"


# ── User Tests ─────────────────────────────────────────────────────────────────

class TestUsers:

    def setup_method(self):
        self.token_a, self.user_a_id = login(USER_A)
        self.token_b, self.user_b_id = login(USER_B)

    def test_edit_own_profile(self):
        r = requests.put(f"{BASE_URL}/api/Users/{self.user_a_id}",
                         json={"firstName": "Updated", "lastName": "User",
                               "city": "Skopje", "bio": "test"},
                         headers=auth_headers(self.token_a))
        assert r.status_code == 200

    def test_edit_other_users_profile(self):
        r = requests.put(f"{BASE_URL}/api/Users/{self.user_b_id}",
                         json={"firstName": "Hacked", "lastName": "User",
                               "city": "Skopje", "bio": "hacked"},
                         headers=auth_headers(self.token_a))
        assert r.status_code == 403

    def test_edit_profile_no_token(self):
        r = requests.put(f"{BASE_URL}/api/Users/{self.user_a_id}",
                         json={"firstName": "Test"})
        assert r.status_code == 401