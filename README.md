# Real-Time Polls API

![.NET](https://img.shields.io/badge/.NET-8-blueviolet) ![PostgreSQL](https://img.shields.io/badge/PostgreSQL-blue) ![Redis](https://img.shields.io/badge/Redis-red) ![WebSockets](https://img.shields.io/badge/WebSockets-lightgrey)

API para criação e votação em enquetes em tempo real, construída com .NET 8, ASP.NET Core, PostgreSQL, Redis e WebSockets.

## Descrição

Este projeto é uma API backend que permite aos usuários:
* Criar novas enquetes com múltiplas opções.
* Votar em uma das opções de uma enquete.
* Visualizar os resultados da votação em tempo real através de uma conexão WebSocket.

A arquitetura foi desenhada seguindo os princípios **SOLID** e uma abordagem de **design em camadas (Layered Architecture)** para garantir manutenibilidade, testabilidade e escalabilidade.

## Tecnologias Utilizadas

* **Backend:** [.NET 8](https://dotnet.microsoft.com/download/dotnet/8.0) (C#)
* **Framework:** [ASP.NET Core](https://docs.microsoft.com/aspnet/core/)
* **Banco de Dados Principal:** [PostgreSQL](https://www.postgresql.org/) para persistência dos dados das enquetes e votos.
* **Banco de Dados em Memória/Cache:** [Redis](https://redis.io/) para contagem de votos e mensageria (Pub/Sub).
* **Comunicação em Tempo Real:** WebSockets
* **ORM:** [Entity Framework Core](https://docs.microsoft.com/ef/core/)
* **Infraestrutura:** [Docker](https://www.docker.com/) e Docker Compose

## Pré-requisitos

Para executar este projeto, você precisará ter instalado em sua máquina:
* [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0) ou superior.
* [Docker](https://www.docker.com/products/docker-desktop/) e Docker Compose.

## Como Executar o Projeto

Siga os passos abaixo para colocar a aplicação no ar.

**1. Clone o repositório:**
```bash
git clone [https://github.com/gquadros-dev/polls-csharp.git](https://github.com/gquadros-dev/polls-csharp.git)
cd polls-csharp
```

**2. Inicie a infraestrutura (PostgreSQL e Redis):**
Use o Docker Compose para iniciar os contêineres dos bancos de dados em segundo plano.
```bash
docker-compose up -d
```
Isso irá criar e iniciar um container para o PostgreSQL (acessível na porta `5433`) e um para o Redis (acessível na porta `6380`).

**3. Aplique as Migrations do Banco de Dados:**
Este comando irá criar o schema do banco de dados e as tabelas necessárias no container do PostgreSQL.
```bash
dotnet ef database update
```

**4. Execute a Aplicação:**
Agora, basta iniciar a API .NET.
```bash
dotnet run
```
A aplicação estará rodando e pronta para receber requisições em `http://localhost:3333`.

## Endpoints da API

### Enquetes (Polls)

* **Criar uma enquete**
    * `POST /polls`
    * **Body (JSON):**
        ```json
        {
          "title": "Qual sua linguagem de programação favorita?",
          "options": ["C#", "TypeScript", "Python"]
        }
        ```

* **Obter uma enquete**
    * `GET /polls/{pollId}`
    * Retorna os detalhes da enquete e a contagem de votos atual para cada opção.

### Votos (Votes)

* **Votar em uma enquete**
    * `POST /polls/{pollId}/votes`
    * **Body (JSON):**
        ```json
        {
          "pollOptionId": "..."
        }
        ```

* **Obter resultados em tempo real**
    * `GET /polls/{pollId}/results`
    * Endpoint para conexão via **WebSocket**. Uma vez conectado, o cliente receberá uma mensagem a cada novo voto computado para a enquete.
    * **Exemplo de mensagem recebida:**
        ```json
        {
          "pollOptionId": "...",
          "votes": 123
        }
        ```