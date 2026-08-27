# OrderManagement — Teste Prático .NET Senior

Backend de gestão de pedidos para um e-commerce simples, construído com foco em **qualidade arquitetural**: Clean Architecture + DDD + CQRS.

---

## Decisões de Design

### Minimal API vs Controllers
Escolhi **Minimal API** porque:
- Remove boilerplate desnecessário (sem herança de ControllerBase, sem atributos de rota em classe)
- É a direção nativa do .NET moderno (a Microsoft sinalizou Controllers como legado)
- Endpoint groups (`MapGroup`) oferecem o mesmo nível de organização com menos cerimônia
- Cada endpoint é uma função pura — fácil de testar isoladamente

### Por que não `IRepository<T>` genérico?
O projeto tem **um único agregado** (`Order`). Um repositório genérico não acrescenta nada além de indireção: `IOrderRepository` expõe exatamente o contrato que o domínio precisa (`GetById`, `GetPaged`, `Add`, `Update`) sem abrir métodos que não fazem sentido para Order. Se surgirem outros agregados no futuro, cada um terá seu próprio repositório específico — mais fácil de evoluir e de entender.

### Cálculo de `TotalAmount`
Calculado como propriedade computed na entidade `Order` (domínio), não na camada de Application. Isso garante que a regra não fique espalhada pelo código e que qualquer lógica futura sobre total (desconto, imposto) fique centralizada no lugar certo.

### Pipeline de comportamentos (MediatR)
A ordem no DI é: **Logging → Validation → Handler**. Isso garante que:
1. Toda requisição válida é logada com tempo de execução
2. Erros de validação disparam antes do handler — sem desperdício de processamento
3. O handler foca 100% na lógica de negócio

---

## Estrutura do Projeto

```
src/
├── OrderManagement.Domain/          # Entidades, enums, exceções, interfaces
├── OrderManagement.Application/     # CQRS handlers, DTOs, validators, behaviors
├── OrderManagement.Infrastructure/  # EF Core, SQLite, repositórios
└── OrderManagement.API/             # Minimal API endpoints, JWT, middleware
    ├── DBLocal/                      # Banco SQLite local (dev)
    └── Middleware/                   # ExceptionHandlingMiddleware, OpenApiBearerSecuritySchemeTransformer

tests/
├── OrderManagement.UnitTests/       # Testes unitários dos handlers, domínio, validators e behaviors
└── OrderManagement.IntegrationTests/# WebApplicationFactory — testes end-to-end
```

---

## Testes

### Unitários (`OrderManagement.UnitTests`)
Isolam cada peça de lógica com mocks (NSubstitute) — sem banco, sem HTTP:

| Alvo | Cobertura |
|------|-----------|
| `Domain/` | Regras de negócio da entidade `Order` e `OrderItem` (criação, cancelamento, confirmação, validações) |
| `Orders/Commands/` | Handlers de `CreateOrder`/`CancelOrder` + seus `Validator`s (via `FluentValidation.TestHelper`) |
| `Orders/Queries/` | Handlers de `GetOrderById`/`GetOrders`, incluindo o clamping de `page`/`pageSize` |
| `Common/Behaviors/` | `ValidationBehavior` (agrega falhas de múltiplos validators, bloqueia o `next()`) e `LoggingBehavior` (repassa o resultado e relança exceções) |

### Integração (`OrderManagement.IntegrationTests`)
Sobem a API completa via `CustomWebApplicationFactory` (`WebApplicationFactory`) e validam os endpoints ponta a ponta, incluindo autenticação JWT. O banco SQLite real é substituído por um provider EF Core InMemory (nome fixo por instância de factory, para persistir estado entre requisições do mesmo teste) e a aplicação automática de migrations é pulada nesse cenário (`Database.IsRelational()`). A paralelização de collections do xUnit é desabilitada nesse projeto — o bootstrap logger do Serilog em `Program.cs` é estático e não é seguro para múltiplas instâncias de host sendo construídas ao mesmo tempo no mesmo processo.

```bash
# Rodar só os testes unitários
dotnet test tests/OrderManagement.UnitTests

# Rodar só os testes de integração
dotnet test tests/OrderManagement.IntegrationTests

# Rodar tudo
dotnet test
```

---

## Rodando Localmente

### Pré-requisitos
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

```bash
# Restaurar dependências
dotnet restore

# Rodar os testes
dotnet test

# Rodar a API
dotnet run --project src/OrderManagement.API
```

A API sobe em `http://localhost:5000` (ou a porta definida pelo launchSettings).

As migrations são aplicadas **automaticamente** na inicialização — nenhum passo manual necessário.

---

## Rodando via Docker

```bash
docker compose up --build

docker compose --profile sonar up --build
```

A API estará disponível em `http://localhost:8080`, com healthcheck em `/health` (`docker compose ps` mostra o container como `healthy` assim que a aplicação sobe).

O `docker compose up` também sobe uma instância do **Jaeger** (`jaegertracing/all-in-one`), já conectada à API via OTLP. A UI do Jaeger fica em `http://localhost:16686` — selecione o serviço `OrderManagement.API` para ver os traces de cada requisição.

---

## Endpoints

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| `POST` | `/auth/login` | ❌ | Autentica e retorna JWT |
| `POST` | `/api/orders` | ✅ | Cria um novo pedido |
| `GET` | `/api/orders?page=1&pageSize=10` | ✅ | Lista pedidos paginados |
| `GET` | `/api/orders/{id}` | ✅ | Retorna pedido por ID |
| `PATCH` | `/api/orders/{id}/cancel` | ✅ | Cancela um pedido Pending |

### Credenciais fixas (para o token JWT)
```
Email:  dev@martech.com
Senha:  Senha@123
```

### Exemplo de uso com curl

```bash
# 1. Login
TOKEN=$(curl -s -X POST http://localhost:8080/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"dev@martech.com","password":"Senha@123"}' | jq -r .token)

# 2. Criar pedido
curl -X POST http://localhost:8080/api/orders \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "items": [
      { "productName": "Notebook", "quantity": 1, "unitPrice": 2999.90 },
      { "productName": "Mouse", "quantity": 2, "unitPrice": 49.90 }
    ]
  }'

# 3. Listar pedidos
curl http://localhost:8080/api/orders \
  -H "Authorization: Bearer $TOKEN"
```

---

## Stack Técnica

| Tecnologia | Versão | Papel |
|-----------|--------|-------|
| .NET | 10 | Runtime |
| MediatR | 12 | CQRS dispatcher |
| EF Core + SQLite | 10 | Persistência |
| FluentValidation | 11 | Validação com pipeline behavior |
| Serilog | 9 | Logging estruturado |
| OpenTelemetry | 1.11 | Tracing (export console + OTLP/Jaeger) |
| Jaeger | 1.62.0 | Backend de tracing distribuído (via Docker) |
| xUnit + NSubstitute + FluentAssertions + FluentValidation (TestHelper) | latest | Testes |

---

## OpenAPI (Scalar)

Em ambiente de desenvolvimento, acesse `http://localhost:5000/scalar` para a interface interativa, ou `http://localhost:5000/openapi/{documento}.json` para a especificação JSON.

---

## Observabilidade

- **Serilog** loga todas as requisições HTTP e o request/response de cada command/query via `LoggingBehavior`
- **OpenTelemetry** rastreia spans HTTP e exporta para o console e, via OTLP, para o **Jaeger** (`http://localhost:16686` quando rodando via Docker)
