// Авторизация ботов: логин по одному и пакетный логин в setup().
import http from 'k6/http';
import { BASE_URL, JSON_HEADERS, PASSWORD, BOTS, botEmail } from './config.js';

function readToken(res, email) {
  if (res.status !== 200) {
    throw new Error('login failed for ' + email + ': ' + res.status + ' ' + res.body);
  }
  var body = res.json();
  var token = body && (body.token || body.Token);
  if (!token) {
    throw new Error('login response has no token for ' + email);
  }
  return token;
}

export function login(email, password) {
  var res = http.post(
    BASE_URL + '/api/auth/login',
    JSON.stringify({ Email: email, Password: password || PASSWORD }),
    { headers: JSON_HEADERS, tags: { name: 'POST /api/auth/login' } }
  );
  return { token: readToken(res, email), response: res };
}

// Пакетный логин всех ботов (используется в setup(), вне замеряемого окна).
export function loginAll(count) {
  var total = count || BOTS;
  var requests = [];
  for (var i = 1; i <= total; i++) {
    requests.push({
      method: 'POST',
      url: BASE_URL + '/api/auth/login',
      body: JSON.stringify({ Email: botEmail(i), Password: PASSWORD }),
      params: { headers: JSON_HEADERS, tags: { name: 'POST /api/auth/login (setup)' } },
    });
  }

  var responses = http.batch(requests);
  var tokens = [];
  for (var j = 0; j < responses.length; j++) {
    tokens.push(readToken(responses[j], botEmail(j + 1)));
  }
  return tokens;
}

export function authHeaders(token) {
  return {
    'Content-Type': 'application/json',
    Accept: 'application/json',
    Authorization: 'Bearer ' + token,
  };
}
