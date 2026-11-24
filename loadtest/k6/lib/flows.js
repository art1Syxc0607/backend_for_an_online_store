// Переиспользуемые флоу для сценариев.
import http from 'k6/http';
import { check } from 'k6';
import { BASE_URL, JSON_HEADERS } from './config.js';
import { authHeaders } from './auth.js';
import { pick } from './data.js';

function ok200(r) {
  return r.status === 200;
}

function ok204(r) {
  return r.status === 204;
}

// --- Чтение каталога (анонимно) ---
export function catalogRead(productIds) {
  var ok = true;

  var res = http.get(BASE_URL + '/api/products/filter?PageNumber=1&PageSize=20', {
    headers: JSON_HEADERS,
    tags: { name: 'GET /api/products/filter' },
  });
  ok = check(res, { 'filter 200': ok200 }) && ok;

  res = http.get(BASE_URL + '/api/products/' + pick(productIds), {
    headers: JSON_HEADERS,
    tags: { name: 'GET /api/products/{id}' },
  });
  ok = check(res, { 'product 200': ok200 }) && ok;

  res = http.get(BASE_URL + '/api/categories', {
    headers: JSON_HEADERS,
    tags: { name: 'GET /api/categories' },
  });
  ok = check(res, { 'categories 200': ok200 }) && ok;

  res = http.get(BASE_URL + '/api/products/popularProducts?pageNumber=1&pageSize=20', {
    headers: JSON_HEADERS,
    tags: { name: 'GET /api/products/popularProducts' },
  });
  ok = check(res, { 'popular 200': ok200 }) && ok;

  // ~10% запросов с уникальным SearchText промахиваются мимо MemoryCache
  // и реально идут в PostgreSQL - это "холодное" чтение.
  if (Math.random() < 0.1) {
    var q = 'loadtest-' + Math.floor(Math.random() * 1000000000);
    res = http.get(
      BASE_URL + '/api/products/filter?SearchText=' + q + '&PageNumber=1&PageSize=20',
      { headers: JSON_HEADERS, tags: { name: 'GET /api/products/filter (uncached)' } }
    );
    ok = check(res, { 'uncached filter 200': ok200 }) && ok;
  }

  return ok;
}

// --- Вход + профиль ---
export function authFlow(email, password) {
  var res = http.post(
    BASE_URL + '/api/auth/login',
    JSON.stringify({ Email: email, Password: password }),
    { headers: JSON_HEADERS, tags: { name: 'POST /api/auth/login' } }
  );
  var ok = check(res, { 'login 200': ok200 });
  if (!ok) {
    return false;
  }

  var body = res.json();
  var token = body && (body.token || body.Token);

  res = http.get(BASE_URL + '/api/auth/profile', {
    headers: authHeaders(token),
    tags: { name: 'GET /api/auth/profile' },
  });
  return check(res, { 'profile 200': ok200 });
}

// --- Полный флоу: корзина -> заказ -> оплата ---
export function orderFlow(token, productIds, shippingAddress) {
  var headers = authHeaders(token);
  var address = shippingAddress || 'Load Test Street 1, Moscow';

  var res = http.del(BASE_URL + '/api/cart/clear', null, {
    headers: headers,
    tags: { name: 'DELETE /api/cart/clear' },
  });
  if (!check(res, { 'clear cart 204': ok204 })) {
    return false;
  }

  res = http.post(
    BASE_URL + '/api/cart',
    JSON.stringify({ ProductId: pick(productIds), Quantity: 1 }),
    { headers: headers, tags: { name: 'POST /api/cart' } }
  );
  if (!check(res, { 'add to cart 204': ok204 })) {
    return false;
  }

  res = http.get(BASE_URL + '/api/cart', {
    headers: headers,
    tags: { name: 'GET /api/cart' },
  });
  if (!check(res, { 'get cart 200': ok200 })) {
    return false;
  }

  res = http.post(
    BASE_URL + '/api/cart/Checkout',
    JSON.stringify({ ShippingAddress: address }),
    { headers: headers, tags: { name: 'POST /api/cart/Checkout' } }
  );
  if (!check(res, { 'checkout 200': ok200 })) {
    return false;
  }
  var orderId = res.json();

  res = http.post(
    BASE_URL + '/api/order/initiate',
    JSON.stringify({ OrderId: orderId, Method: 'Card' }),
    { headers: headers, tags: { name: 'POST /api/order/initiate' } }
  );
  if (!check(res, { 'initiate payment 200': ok200 })) {
    return false;
  }

  var payment = res.json();
  var intentId = payment && (payment.paymentIntentId || payment.PaymentIntentId);
  if (!intentId) {
    console.error('initiate payment has no paymentIntentId: ' + res.body);
    return false;
  }

  res = http.post(
    BASE_URL + '/api/order/confirm',
    JSON.stringify({ PaymentIntentId: intentId }),
    { headers: headers, tags: { name: 'POST /api/order/confirm' } }
  );
  return check(res, { 'confirm payment 200': ok200 });
}
