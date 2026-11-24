// S2: аутентификация (логин + профиль). CPU-bound сценарий (хеширование пароля).
//
// Запуск:
//   k6 run -e BASE_URL=http://localhost:8080 loadtest/k6/scenarios/02-auth.js
import { sleep } from 'k6';
import { SUMMARY_STATS, PASSWORD, BOTS, botEmail, stages } from '../lib/config.js';
import { authFlow } from '../lib/flows.js';

export const options = {
  scenarios: {
    auth: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: stages([
        { duration: '20s', target: 10 },
        { duration: '1m30s', target: 20 },
        { duration: '20s', target: 0 },
      ]),
      gracefulRampDown: '10s',
    },
  },
  thresholds: {
    checks: ['rate>0.99'],
    http_req_failed: ['rate<0.01'],
    'http_req_duration{expected_response:true}': ['p(95)<800'],
  },
  summaryTrendStats: SUMMARY_STATS,
};

export default function () {
  var index = ((__VU - 1) % BOTS) + 1;
  authFlow(botEmail(index), PASSWORD);
  sleep(0.5 + Math.random());
}
