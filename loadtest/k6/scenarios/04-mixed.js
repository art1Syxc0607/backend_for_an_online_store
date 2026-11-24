// S4: смешанный профиль, близкий к реальному: 70% чтение каталога,
// 10% логин, 20% покупка (корзина -> заказ -> оплата).
//
// Запуск:
//   k6 run -e BASE_URL=http://localhost:8080 loadtest/k6/scenarios/04-mixed.js
import { sleep } from 'k6';
import { SUMMARY_STATS, PASSWORD, BOTS, botEmail, stages } from '../lib/config.js';
import { loginAll } from '../lib/auth.js';
import { fetchProductIds } from '../lib/data.js';
import { catalogRead, authFlow, orderFlow } from '../lib/flows.js';

export const options = {
  scenarios: {
    mixed: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: stages([
        { duration: '30s', target: 30 },
        { duration: '3m', target: 50 },
        { duration: '30s', target: 0 },
      ]),
      gracefulRampDown: '15s',
    },
  },
  thresholds: {
    checks: ['rate>0.99'],
    http_req_failed: ['rate<0.01'],
    // В mixed доля запросов оплаты мала, но confirm содержит искусственную
    // задержку 1с (PaymentService.ConfirmPaymentAsync), поэтому порог выше.
    'http_req_duration{expected_response:true}': ['p(95)<800'],
  },
  summaryTrendStats: SUMMARY_STATS,
};

export function setup() {
  return {
    tokens: loginAll(BOTS),
    productIds: fetchProductIds(50),
  };
}

export default function (data) {
  var index = ((__VU - 1) % BOTS) + 1;
  var roll = Math.random();

  if (roll < 0.7) {
    catalogRead(data.productIds);
  } else if (roll < 0.8) {
    authFlow(botEmail(index), PASSWORD);
  } else {
    var token = data.tokens[(__VU - 1) % data.tokens.length];
    orderFlow(token, data.productIds);
  }

  sleep(0.5 + Math.random());
}
