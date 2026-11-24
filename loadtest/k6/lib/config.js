// Общие настройки сценариев k6.
// Переопределяются через -e KEY=VALUE при запуске.

export const BASE_URL = (__ENV.BASE_URL || 'http://localhost:8080').replace(/\/+$/, '');

// Пароль ботов должен удовлетворять PasswordRules (A-Z, a-z, 0-9, >= 6 символов).
export const PASSWORD = __ENV.LOADTEST_PASSWORD || 'LoadTest123';

export const BOT_PREFIX = __ENV.BOT_PREFIX || 'loadbot';
export const BOT_DOMAIN = __ENV.BOT_DOMAIN || 'load.test';

// Сколько ботов подготовлено скриптом Seed-LoadTestUsers.ps1.
export const BOTS = Number(__ENV.BOTS || 40);

export const JSON_HEADERS = {
  'Content-Type': 'application/json',
  Accept: 'application/json',
};

export const SUMMARY_STATS = ['avg', 'min', 'med', 'p(90)', 'p(95)', 'p(99)', 'max'];

// Профиль прогона: load (боевой профиль из сценария) или smoke (быстрая проверка).
export const PROFILE = __ENV.PROFILE || 'load';

// В режиме smoke подменяем stages на 35-секундный прогон в 2 VU.
export function stages(loadStages) {
  if (PROFILE === 'smoke') {
    return [
      { duration: '10s', target: 2 },
      { duration: '20s', target: 2 },
      { duration: '5s', target: 0 },
    ];
  }
  return loadStages;
}

function pad3(value) {
  var s = String(value);
  while (s.length < 3) {
    s = '0' + s;
  }
  return s;
}

export function botEmail(index) {
  return BOT_PREFIX + pad3(index) + '@' + BOT_DOMAIN;
}

export function botName(index) {
  return BOT_PREFIX + pad3(index);
}

// Бот, закреплённый за виртуальным пользователем k6.
export function botIndexForVu() {
  return ((__VU - 1) % BOTS) + 1;
}
