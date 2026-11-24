// S1: каталог (анонимное чтение). Основной baseline производительности.
//
// Запуск:
//   k6 run -e BASE_URL=http://localhost:8080 loadtest/k6/scenarios/01-catalog.js
import { sleep } from 'k6';
import { SUMMARY_STATS, stages } from '../lib/config.js';
import { catalogRead } from '../lib/flows.js';
import { fetchProductIds } from '../lib/data.js';

export const options = {
  scenarios: {
    catalog: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: stages([
        { duration: '30s', target: 50 },
        { duration: '3m', target: 50 },
        { duration: '30s', target: 0 },
      ]),
      gracefulRampDown: '10s',
    },
  },
  thresholds: {
    checks: ['rate>0.99'],
    http_req_failed: ['rate<0.01'],
    'http_req_duration{expected_response:true}': ['p(95)<400'],
  },
  summaryTrendStats: SUMMARY_STATS,
};

export function setup() {
  return { productIds: fetchProductIds(50) };
}

export default function (data) {
  catalogRead(data.productIds);
  sleep(0.5 + Math.random());
}
