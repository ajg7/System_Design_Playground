// Steady browsing traffic across the Pokédex. Run BEFORE writing cache code and write down p50/p99.
//   docker compose run --rm k6 run baseline.js
import http from 'k6/http';
import { check } from 'k6';

const IDS = [1, 4, 6, 7, 9, 25, 39, 52, 54, 94, 130, 131, 133, 143, 144, 145, 146, 149, 150, 151, 249, 250, 384, 445, 448, 658, 778, 888];

export const options = {
  vus: 30,
  duration: '30s',
  summaryTrendStats: ['avg', 'p(50)', 'p(95)', 'p(99)', 'max'],
};

export default function () {
  const id = IDS[Math.floor(Math.random() * IDS.length)];
  const res = http.get(`${__ENV.BASE_URL}/species/${id}`);
  check(res, { '200': (r) => r.status === 200 });
}
