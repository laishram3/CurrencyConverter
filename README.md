To test API endpoints that require authorization, include the following JWT token in the Authorization header:

"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiMTIzNCIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwiZXhwIjoxODA0NjQ0MjU4LCJpc3MiOiJ0ZXN0IiwiYXVkIjoiY3VycmVuY3lfYXBpIn0.A_Ft3Ww5UtWPWa2bgy9F60VLX6--9PLfiEIY9uFHBAc"

Some endpoints require the literal word Bearer, others accept the token directly.

Base URL: http://yourserver.com/api
Endpoints:
- GET /currency/latest?base=EUR&provider=frankfurter
- GET /currency/convert?from=EUR&to=USD&amount=10
- GET /currency/historical?from=EUR&to=USD&start=2025-08-01&end=2025-08-27
