# Real-Time Polls API

![.NET](https://img.shields.io/badge/.NET-9-blueviolet) ![MS SQL Server](https://img.shields.io/badge/MS_SQL_Server-red) ![Redis](https://img.shields.io/badge/Redis-red) ![WebSockets](https://img.shields.io/badge/WebSockets-lightgrey)

API para criação e votação em enquetes em tempo real, construída com .NET 9 e focada em uma stack Microsoft com SQL Server.

## Descrição

Este projeto é uma API backend que permite aos usuários:
* Criar novas enquetes com múltiplas opções.
* Votar em uma das opções de uma enquete.
* Visualizar os resultados da votação em tempo real através de uma conexão WebSocket.

A arquitetura foi desenhada seguindo os princípios **SOLID** e uma abordagem de **design em camadas (Layered Architecture)** para garantir manutenibilidade, testabilidade e escalabilidade.

## Tecnologias Utilizadas

* **Backend:** [.NET 9](https://dotnet.microsoft.com/download/dotnet/9.0) (C#)
* **Framework:** [ASP.NET Core](https://docs.microsoft.com/aspnet/core/)
* **Banco de Dados Padrão:** [Microsoft SQL Server](https://www.microsoft.com/sql-server/) para persistência de dados.
* **Banco de Dados em Memória:** [Redis](https://redis.io/) para contagem de votos e mensageria (Pub/Sub).
* **Comunicação em Tempo Real:** WebSockets
* **ORM:** [Entity Framework Core](https://docs.microsoft.com/ef/core/)
* **Infraestrutura:** [Docker](https://www.docker.com/) e Docker Compose

## Pré-requisitos

Para executar este projeto, você precisará ter instalado em sua máquina:
* [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0) ou superior.
* [Docker](https://www.docker.com/products/docker-desktop/) e Docker Compose.

## Como Executar o Projeto (Padrão com SQL Server)

Siga os passos abaixo para colocar a aplicação no ar de forma simples e rápida.

**1. Clone o repositório:**
```bash
git clone https://github.com/gquadros-dev/polls-csharp.git
cd polls-csharp
```

**2. Inicie a infraestrutura (SQL Server e Redis):**
Use o Docker Compose para iniciar os contêineres em segundo plano.
```bash
docker-compose up -d
```
Isso irá criar e iniciar os containers do SQL Server e do Redis.

**3. Aplique as Migrations do Banco de Dados:**
Este comando irá criar o schema e as tabelas no container do SQL Server.
```bash
dotnet ef database update
```

**4. Execute a Aplicação:**
```bash
dotnet run
```
A aplicação estará rodando e no console irá aparecer algo como `http://localhost:xxxx`, que será onde está rodando.

## Usando PostgreSQL (Opcional)

Este projeto está configurado para o SQL Server por padrão, mas pode ser facilmente adaptado para usar PostgreSQL.

1.  **Ative o Serviço no Docker:** No arquivo `docker-compose.yaml`, descomente as linhas do serviço `postgres`.
2.  **Instale o Provedor:** Adicione o pacote do Npgsql ao projeto.
    ```bash
    dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
    ```
3.  **Altere a Configuração do DbContext:** No arquivo `Program.cs`, altere a linha `options.UseSqlServer(...)` para `options.UseNpgsql(...)`.
4.  **Ajuste a Connection String:** No `appsettings.json`, crie uma nova string de conexão para o PostgreSQL e aponte para ela no `Program.cs`.
5.  **Gere Novas Migrations:** Apague a pasta `Migrations` atual e gere uma nova com os comandos do EF Core, que agora criará a migration específica para o PostgreSQL.

## Endpoints da API

* **Criar uma enquete:** `POST /polls`
* **Obter uma enquete:** `GET /polls/{pollId}`
* **Votar em uma enquete:** `POST /polls/{pollId}/votes`
* **Obter resultados em tempo real (WebSocket):** `GET /polls/{pollId}/results`