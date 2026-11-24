// Данные для сценариев: id товаров из каталога.
import http from 'k6/http';
import { BASE_URL, JSON_HEADERS } from './config.js';

export function fetchProductIds(limit) {
  var size = limit || 50;
  var url =
    BASE_URL +
    '/api/products/filter?PageNumber=1&PageSize=' +
    size +
    '&OnlyAvailable=true';

  var res = http.get(url, {
    headers: JSON_HEADERS,
    tags: { name: 'GET /api/products/filter (setup)' },
  });

  if (res.status !== 200) {
    throw new Error('product filter failed: ' + res.status + ' ' + res.body);
  }

  var body = res.json();
  var items = (body && (body.items || body.Items)) || [];
  if (!items.length) {
    throw new Error('no products returned: seed the database before running load tests');
  }

  var ids = [];
  for (var i = 0; i < items.length; i++) {
    var id = items[i].id || items[i].Id;
    if (id) {
      ids.push(id);
    }
  }
  if (!ids.length) {
    throw new Error('product list has no ids');
  }
  return ids;
}

export function pick(items) {
  return items[Math.floor(Math.random() * items.length)];
}
