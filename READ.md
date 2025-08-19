# URL Shortener (.NET Minimal APIs)

> **ES (breve):** API mínima para acortar URLs. Docs modernas con **Scalar** (tema oscuro/claro) y **OpenAPI ES/EN**.  
> **EN:** Minimal URL Shortener API with **Scalar** docs (dark/light) and bilingual **OpenAPI (ES/EN)**.

<p align="left">
  <a href="https://dotnet.microsoft.com/"><img alt=".NET" src="https://img.shields.io/badge/.NET-8/9-512BD4?logo=dotnet&logoColor=white"></a>
  <a href="LICENSE"><img alt="License" src="https://img.shields.io/badge/License-MIT-green.svg"></a>
  <img alt="Status" src="https://img.shields.io/badge/Status-MVP-blue">
</p>

A minimal backend-only project to showcase API design, OpenAPI documentation, and clean structure.  
It starts in-memory and is ready to evolve to PostgreSQL/Redis, rate limiting, analytics, and JWT.

## ✨ Features (MVP)
- **POST `/urls`** – create a short URL (random or custom slug)
- **GET `/{slug}`** – redirect (302) to the original URL
- **Validations** – URL & slug format, conflict handling
- **Docs** – **Scalar** UI at `/docs` with **ES/EN** document selector and dark/light toggle

## 🧪 Try it
- **Docs (Scalar):** `http://localhost:5074/docs`  
- **OpenAPI JSON:**  
  - ES → `http://localhost:5074/swagger/v1-es/swagger.json`  
  - EN → `http://localhost:5074/swagger/v1-en/swagger.json`

### Example
```bash
curl -X POST http://localhost:5074/urls \
  -H "Content-Type: application/json" \
  -d '{"originalUrl":"https://github.com","customSlug":"github"}'
# -> 201 Created { "slug":"github","shortUrl":"/github","targetUrl":"https://github.com" }

curl -I http://localhost:5074/github
# -> 302 Location: https://github.com