# Storefront foundation

Node 24, Next.js 16.4.2, React 19.3.1; versions queried from npm when scaffolding, lockfile is authoritative.
`npm ci`, copy `.env.local.example` to `.env.local`, `npm run dev`.
`npm run typecheck`, `npm run lint`, `npm test`, `npm run build`.
`npm run contract:types` updates types from root contract. Commit updated generated.d.ts in same contract PR.

Implemented: catalog cards, loading/error/empty states, service layer with env switch. Mock fixtures typed against generated contract; extra category/order/analytics canned services in mocks/services.ts.
Pending: detail, cart, checkout, admin auth/CRUD/dashboard and browser E2E. Existing MVC provides these flows separately.
Root for Turbopack/tracing is repo root so shared contract fixtures can be imported without copying.
