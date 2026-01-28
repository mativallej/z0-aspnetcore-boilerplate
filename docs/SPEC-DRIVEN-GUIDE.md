# Spec-Driven Development with Claude Code

> Write a spec. Get a full project. Ship faster.

## The Idea

Instead of coding feature by feature, you write a **complete specification** upfront, then let **Claude Code** implement the entire project in one go.

```
SPEC.md  →  Claude Code  →  Full Project
```

## How It Works

### 1. Write Your Spec

Create a `SPEC.md` with:
- Project overview & tech stack
- Solution structure
- Entity definitions
- API endpoints
- Database schema
- Testing strategy
- Deployment config

### 2. Run Claude Code

```
Read SPEC.md and implement all phases from the checklist.
```

### 3. Get Your Project

Claude generates:
- Complete source code
- Tests (unit + integration)
- Docker setup
- CI/CD pipeline
- Documentation

## Real Example

This boilerplate was built entirely from a spec:

| Metric | Result |
|--------|--------|
| **Files generated** | 81 |
| **Lines of code** | 5,750+ |
| **Layers** | 4 (Clean Architecture) |
| **Test coverage** | Unit + Integration |

## Spec Template

```markdown
# Project: {name}

## Overview
| Attribute | Value |
|-----------|-------|
| Framework | .NET 9.0 |
| Database  | PostgreSQL |
| Auth      | JWT |

## Solution Structure
src/
├── Domain/
├── Application/
├── Infrastructure/
└── WebApi/

## Entities
- User: Id, Email, Name
- Item: Id, Name, Description

## API Endpoints
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/items | List items |
| POST | /api/items | Create item |

## Implementation Checklist
- [ ] Phase 1: Setup
- [ ] Phase 2: Domain
- [ ] Phase 3: API
- [ ] Phase 4: Tests
```

## Why It Works

| Traditional | Spec-Driven |
|-------------|-------------|
| Code → Refactor → Repeat | Plan → Generate → Ship |
| Context lost between sessions | Full context in one doc |
| Inconsistent architecture | Consistent from start |
| Manual boilerplate | Automated generation |

## Pro Tips

1. **Be specific** — Include code snippets for complex logic
2. **Use checklists** — Claude tracks progress automatically
3. **Define schemas** — Show exact JSON/DB structures
4. **Include examples** — Sample requests/responses help

## Get Started

1. Copy the spec template
2. Customize for your project
3. Open Claude Code
4. Paste: `Read SPEC.md and implement all phases`
5. Watch your project come to life

---

**Built with Claude Code** • [View Full Spec](SPEC.md)
