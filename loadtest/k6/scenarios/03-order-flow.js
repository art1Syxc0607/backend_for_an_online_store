// S3: пользовательский флоу с записью:
//   очистка корзины -> добавление товара -> чтение корзины -> checkout
//   -> initiate payment -> confirm payment
//
// Запуск:
//   k6 run -e BASE_URL=http://localhost:8080 loadtest/k6/scenarios/03-order-flow.js
import { sleep } from 'k6';
import { SUMMARY_STATS, BOTS, stages } from '../lib/config.js';
import { loginAll } from '../lib/auth.js';
import { fetchProductIds } from '../lib/data.js';
import { orderFlow } from '../lib/flows.js';

export const options = {
  scenarios: {
    order_flow: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: stages([
        { duration: '20s', target: 10 },
        { duration: '2m', target: 20 },
        { duration: '20s', target: 0 },
      ]),
      gracefulRampDown: '15s',
    },
  },
  thresholds: {
    checks: ['rate>0.99'],
    http_req_failed: ['rate<0.01'],
    // confirm payment в текущей реализации содержит искусственную задержку 1с
    // (эмуляция платёжного шлюза, PaymentService.ConfirmPaymentAsync).
    'http_req_duration{expected_response:true}': ['p(95)<1500'],
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
  var token = data.tokens[(__VU - 1) % data.tokens.length];
  orderFlow(token, data.productIds);
  sleep(0.5 + Math.random());
}
