# solDocs 📚

Sistema de documentação e gerenciamento de conteúdo desenvolvido com ASP.NET Core e MongoDB.

## 🚀 Funcionalidades

- **Gestão de Tópicos**: Sistema hierárquico de tópicos (público/privado)
- **Artigos**: Criação e edição de artigos com suporte a rich text
- **Busca**: Sistema de busca em tópicos e artigos
- **Autenticação**: Sistema JWT com recuperação de senha via email
- **Upload de Imagens**: Upload com compressão automática para WebP e deduplicação por hash
- **Controle de Acesso**: Sistema de roles (admin/user)
- **Soft Delete**: Exclusão lógica de registros

## 🛠️ Tecnologias

- **Backend**: ASP.NET Core 8.0
- **Banco de Dados**: MongoDB
- **Autenticação**: JWT Bearer
- **Upload**: FTP com FluentFTP
- **Imagens**: ImageSharp (compressão WebP)
- **Email**: MailKit
- **Documentação**: Swagger/OpenAPI

## 📋 Pré-requisitos

- .NET 8.0 SDK
- MongoDB 4.4+
- Servidor FTP (para upload de imagens)
- Servidor SMTP (para emails)

## 🔧 Instalação

### 1. Clone o repositório

```bash
git clone https://github.com/seu-usuario/solDocs.git
cd solDocs
```

### 2. Configure o ambiente

Copie o arquivo de exemplo e configure suas credenciais:

```bash
cp appsettings.example.json appsettings.Development.json
```

Edite `appsettings.Development.json` e configure:

- **JwtSettings:Key** - Chave secreta para JWT (mínimo 32 caracteres)
- **AdminUserSeed** - Email, username e senha do admin inicial
- **SmtpSettings** - Credenciais do servidor de email
- **FtpSettings** - Credenciais do servidor FTP
- **MongoDbSettings** - String de conexão do MongoDB

### 3. Instale as dependências

```bash
dotnet restore
```

### 4. Execute a aplicação

```bash
dotnet run
```

A API estará disponível em `http://localhost:5204`

## 🐳 Docker

### Com Docker Compose

1. Crie um arquivo `.env` baseado no exemplo:

```bash
cp .env.example .env
```

2. Configure as variáveis no `.env`:

```env
MONGO_USER=admin
MONGO_PASSWORD=sua-senha-segura
MONGO_DATABASE=soldocs_db
```

3. Execute:

```bash
docker-compose up -d
```

A aplicação estará em `http://localhost:8080` e o MongoDB em `localhost:27018`

## 📚 Documentação da API

Após iniciar a aplicação, acesse:

- **Swagger UI**: `http://localhost:5204/swagger`

### Principais Endpoints

#### Autenticação
- `POST /api/auth/login` - Login
- `POST /api/auth/forgot-password` - Solicitar reset de senha
- `POST /api/auth/verify-code` - Verificar código de recuperação
- `POST /api/auth/reset-password` - Redefinir senha

#### Tópicos
- `GET /api/topics/tree/{type}` - Árvore de tópicos (public/private)
- `GET /api/topics/{id}` - Buscar tópico por ID
- `POST /api/topics` - Criar tópico (admin)
- `PUT /api/topics/{id}` - Atualizar tópico (admin)
- `DELETE /api/topics/{id}` - Deletar tópico (admin)

#### Artigos
- `GET /api/articles/{id}` - Buscar artigo por ID
- `GET /api/articles/topic/{topicId}` - Listar artigos de um tópico (admin)
- `POST /api/articles` - Criar artigo (admin)
- `PUT /api/articles/{id}` - Atualizar artigo (admin)
- `DELETE /api/articles/{id}` - Deletar artigo (admin)

#### Busca
- `GET /api/search?text={query}` - Buscar em tópicos e artigos

#### Upload
- `POST /api/files/upload` - Upload de imagem (admin)

#### Usuários
- `GET /api/users` - Listar usuários (admin)
- `POST /api/users` - Criar usuário (admin)
- `GET /api/users/me` - Dados do usuário logado
- `PATCH /api/users/{id}/roles` - Atualizar roles (admin)

## 🔐 Segurança

- Senhas são hasheadas com BCrypt
- JWT tokens com expiração configurável
- Sistema de roles para controle de acesso
- Índices únicos para email e username
- Validação de dados com Data Annotations
- CORS configurado (ajuste conforme necessário)

## 🤝 Contribuindo

Contribuições são bem-vindas! Por favor:

1. Faça um Fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/MinhaFeature`)
3. Commit suas mudanças (`git commit -m 'Adiciona MinhaFeature'`)
4. Push para a branch (`git push origin feature/MinhaFeature`)
5. Abra um Pull Request

## 📝 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

## 👥 Autores

- Gabriel Fernando de Quadros - [@gquadros-dev](https://github.com/gquadros-dev)

## 🐛 Problemas

Encontrou um bug? [Abra uma issue](https://github.com/gquadros-dev/solDocs/issues)

## ⚠️ Notas Importantes

- **Nunca commite** arquivos com credenciais reais
- Use variáveis de ambiente em produção
- Configure CORS adequadamente para produção
- Altere as chaves secretas do exemplo
- Use HTTPS em produção
- Configure backups regulares do MongoDB
