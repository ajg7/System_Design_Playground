// The legendary reveal: 500 trainers open Mewtwo's page at the same instant.
// 1. Flush the cache:        docker compose exec redis redis-cli flushall
// 2. Note the DB counter:    curl -s localhost:8080/metrics | grep lab_db_queries_total
// 3. WRITE YOUR PREDICTION in attempts/predictions.md
// 4. Run:                    docker compose run --rm k6 run stampede.js
// 5. Read the counter again and diff.
import http from 'k6/http';
import { check } from 'k6';

export const options = {
  scenarios: {
    reveal: { executor: 'per-vu-iterations', vus: 500, iterations: 1, maxDuration: '60s' },
  },
  summaryTrendStats: ['avg', 'p(50)', 'p(95)', 'p(99)', 'max'],
};

export default function () {
  const res = http.get(`${__ENV.BASE_URL}/species/150`);
  check(res, { '200': (r) => r.status === 200 });
}
