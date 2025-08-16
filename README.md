# 🎮 Sistema de Matchmaking Unity Photon Pun2 + Node.js

Um sistema completo de matchmaking para jogos multiplayer Unity com Photon PUN2, utilizando servidor Node.js + PostgreSQL para gerenciar salas e emparelhar jogadores automaticamente.

## 🌟 Visão Geral

Este sistema permite que jogadores de um jogo desenvolvido na Unity se conectem automaticamente com outros jogadores disponíveis, criando partidas balanceadas sem necessidade de buscar salas manualmente. O servidor gerencia todas as salas ativas e faz o emparelhamento inteligente.

## 🏗️ Arquitetura do Sistema

```
Unity Client (C#) ←→ Node.js Server ←→ PostgreSQL Database
        ↓                    ↓
   Photon PUN2         Render.com Hosting
```

### Fluxo de Funcionamento:
1. **Cliente Unity** envia requisição de matchmaking
2. **Servidor Node.js** processa e armazena no PostgreSQL
3. **Sistema emparelha** jogadores disponíveis
4. **Clientes Unity** recebem sala para conectar
5. **Photon PUN2** gerencia a conexão multiplayer

## ⚡ Funcionalidades Principais

### 🔧 Servidor Node.js
- **API RESTful** para comunicação com Unity
- **Emparelhamento automático** de jogadores
- **Limpeza automática** de salas inativas (5+ minutos)
- **Sistema de salas únicas** com prevenção de duplicatas
- **Logging detalhado** para debugging
- **Segurança** com Helmet.js e CORS

### 🎮 Cliente Unity (C#)
- **Integração Photon PUN2** para multiplayer
- **Sistema de corrotinas** para verificação contínua
- **Gerenciamento automático** de conexões
- **Tratamento de erros** e reconexão
- **Interface simples** para ativar/desativar matchmaking

### 🗄️ Banco de Dados
- **PostgreSQL** para persistência robusta
- **Tabela otimizada** para consultas rápidas
- **Índices automáticos** para performance
- **Limpeza automática** de dados antigos

## 🚀 Deploy no Render.com

### 1. Preparação dos Arquivos

Crie um `package.json` na raiz do projeto:


### 2. Configuração do PostgreSQL

1. **Acesse [Render.com](https://render.com)**
2. **Crie um PostgreSQL Database**:
   - Clique em "New +" → "PostgreSQL"
   - Nome: `matchmaking-db`
   - Database: `bd_matchmaking`
   - User: `matchmaking_user`
   - Region: escolha mais próxima
3. **Copie a Connection String** fornecida

### 3. Deploy do Servidor

1. **Conecte seu repositório GitHub**:
   - Faça push dos arquivos para um repositório
   - No Render: "New +" → "Web Service"
   - Conecte o repositório


## 🔧 Integração Unity

### 1. Configuração no Unity

1. **Instale Photon PUN2** via Asset Store
2. **Configure Photon** com sua App ID
3. **Adicione o script** `matchmaking.cs` a um GameObject
4. **Configure a URL** no script:

```csharp
public string ServerUrl = "https://seu-app.onrender.com/matchmaking";
```

### 2. Uso do Sistema

```csharp
// Para iniciar matchmaking
matchmakingScript.SetReady(true);

// Para cancelar matchmaking  
matchmakingScript.SetReady(false);
```

### 3. Callbacks Importantes

O script já inclui todos os callbacks necessários:

- `OnJoinedLobby()` - Quando conecta ao lobby Photon
- `OnLeftRoom()` - Quando sai de uma sala
- `OnJoinedRoom()` - Quando entra na sala matched
- `OnLeftLobby()` - Gerenciamento de reconexão

## 📊 Estrutura do Banco de Dados

### Tabela: `matchmaking_rooms`

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `id` | SERIAL | ID único da sala |
| `room_name` | VARCHAR(64) | Nome único da sala |
| `player_id` | TEXT | Lista de jogadores (CSV) |
| `created_by` | VARCHAR(64) | Criador da sala |
| `target_room` | VARCHAR(64) | Sala emparelhada |
| `last_update` | TIMESTAMP | Última atividade |

### Consultas Otimizadas

```sql
-- Buscar sala disponível para match
SELECT room_name FROM matchmaking_rooms 
WHERE room_name != $1 AND target_room IS NULL 
ORDER BY last_update ASC LIMIT 1;

-- Verificar se jogador tem match
SELECT target_room FROM matchmaking_rooms 
WHERE $1 = ANY(string_to_array(player_id, ','));

-- Limpar salas antigas
DELETE FROM matchmaking_rooms
WHERE NOW() - last_update > INTERVAL '5 minutes';
```

## 🔌 API Endpoints

### POST `/matchmaking`

**Parâmetros:**
```json
{
  "action": "set_ready|check_room|unset_ready",
  "playerId": "string",
  "roomName": "string (opcional)",
  "players": "string (opcional)"
}
```

**Respostas:**

#### set_ready
```json
// Aguardando outros jogadores
{ "status": "waiting" }

// Match encontrado
{ "status": "ready_set" }
```

#### check_room
```json
// Match encontrado
{ "status": "found", "room": "Room_1234" }

// Ainda aguardando
{ "status": "not_found" }
```

#### unset_ready
```json
{ "status": "unset" }
```

### GET `/versalas`

Lista todas as salas ativas:

```json
{
  "total": 3,
  "salas": [
    {
      "room_name": "Room_1234",
      "player_id": "Player1,Player2",
      "created_by": "Player1",
      "target_room": "Room_5678",
      "last_update": "2025-01-15T10:30:00.000Z"
    }
  ]
}
```

### GET `/ping`

Verificação de status:

```json
{
  "status": "online",
  "database": "PostgreSQL", 
  "timestamp": "2025-01-15T10:30:00.000Z"
}
```

## ⚙️ Configurações Avançadas

### Performance
- **Limpeza automática** a cada 5 minutos
- **Índices otimizados** para consultas rápidas
- **Connection pooling** PostgreSQL
- **Timeout configurável** para requests

### Segurança
```javascript
app.use(helmet()); // Proteções de segurança
app.use(cors());   // CORS habilitado
```

### Monitoramento
```javascript
// Logs detalhados
console.log(`🗑️ ${rowCount} sala(s) removida(s) por inatividade`);
console.log('✅ Conectado ao PostgreSQL com sucesso!');
```

## 🐛 Debugging

### Problemas Comuns

**1. Unity não conecta ao servidor:**
```csharp
// Verifique a URL no Inspector
public string ServerUrl = "https://seu-app.onrender.com/matchmaking";

// Verifique os logs no Console
Debug.Log("Matchmaking response: " + www.downloadHandler.text);
```

**2. Servidor não responde:**
```bash
# Verifique logs no Render
# Dashboard → seu-app → Logs

# Teste endpoint diretamente
curl https://seu-app.onrender.com/ping
```

**3. Database connection error:**
```javascript
// Verifique connection string nas variáveis de ambiente
const pool = new Pool({
  connectionString: process.env.DATABASE_URL,
  ssl: { rejectUnauthorized: false }
});
```

### Logs Úteis

No Unity:
```csharp
Debug.Log("Match found! Joining room: " + response.room);
Debug.Log("No match yet, waiting...");
Debug.LogError("Error checking room: " + www.error);
```

No Servidor:
```javascript
console.log('✅ Conectado ao PostgreSQL com sucesso!');
console.log(`🚀 Servidor rodando na porta ${PORT}`);
console.log(`🗑️ ${rowCount} sala(s) removida(s) por inatividade`);
```
