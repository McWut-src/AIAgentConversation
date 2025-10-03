# Changelog

All notable changes to the AI Agent Conversation Platform are documented in this file.

## [1.4.0] - 2025-01-02

### Added - Conversation Phases
- **Three-phase conversation structure**: Introduction → Conversation → Conclusion
- **Configurable conversation length**: Adjustable exchanges from 1-10 (default: 3)
- **Phase-specific AI prompts**: Tailored prompts for each phase encouraging genuine debate
- **Phase badges**: Visual indicators showing which phase each message belongs to
- **Progress tracking**: UI shows current phase and progress through conversation

### Added - AI Quality Improvements
- **Genuine debate flow**: Enhanced prompts encouraging disagreement and counterarguments
- **Progressive temperature**: Temperature increases through conversation (0.5 → 0.7 → 0.9)
- **Politeness control**: 5-level system from direct/assertive to diplomatic disagreement
- **Smart AI responses**: Agents challenge ideas, disagree constructively, and maintain distinct perspectives

### Added - Export Functionality
- **Multiple export formats**: JSON, Markdown, Plain Text, XML
- **Conversation metadata**: Includes personalities, topic, timestamps
- **Phase information**: Export preserves conversation phase structure
- **Download buttons**: Easy one-click export in preferred format

### Enhanced - User Interface
- **Phase indicators**: Color-coded badges for Introduction, Conversation, Conclusion
- **Conversation length selector**: Dropdown to choose 1-10 exchanges
- **Politeness slider**: Control debate intensity
- **Export buttons**: Four format options below completed conversations
- **Improved message display**: Better visual hierarchy and spacing

### Enhanced - Backend
- **Configurable iterations**: Changed from fixed 3 to configurable 1-10 exchanges
- **Phase-aware message generation**: AI system considers current phase when generating responses
- **Temperature progression**: Implements dynamic temperature adjustment
- **Enhanced logging**: Better debugging information for AI interactions

### Changed
- Message sequence now based on configurable exchange count (4-24 total messages)
- Agent alternation pattern maintained but scales with exchange count
- Conversation completion logic updated for variable message counts

## [1.0.0] - Initial Release

### Added - Core Features
- **Dual AI agents**: Two independent AI agents with configurable personalities
- **OpenAI integration**: GPT-3.5-turbo API for agent responses
- **Text-based personalities**: Simple text input for agent personalities (no dropdowns)
- **Custom topics**: Enter any topic for conversation
- **Real-time display**: SMS-style bubble interface
- **Database persistence**: SQL Server LocalDB for conversation storage
- **Stateless design**: New conversation per page refresh

### Added - API
- **POST /api/conversation/init**: Initialize conversation and get first message
- **POST /api/conversation/follow**: Continue conversation with next agent
- **GET /api/conversation/{id}**: Get completed conversation in markdown

### Added - Technical
- **.NET 8 / ASP.NET Core**: Modern web framework
- **Entity Framework Core**: Database ORM
- **Razor Pages**: Single-page web application
- **Serilog**: Console-only logging
- **Simplified schema**: String-based personality and topic fields

### Design Decisions (v1.0)
- Fixed 3 iterations (6 messages) - intentionally simple
- No user authentication - focused on core functionality
- No rate limiting - v1.0 simplification
- Console logging only - no file persistence
- Stateless design - no session management
- LocalDB only - not production-ready

---

## Migration Guide

### Upgrading from v1.0 to v1.4

**Database Changes:**
- No schema migration needed - existing database compatible
- Conversation table unchanged
- Message table unchanged
- Phase information derived from message count and configuration

**API Changes:**
- **POST /api/conversation/init**: Now accepts optional `exchangeCount` parameter (1-10, default 3)
- **POST /api/conversation/follow**: Returns phase information in response
- **GET /api/conversation/{id}**: Returns phase-annotated markdown
- **New endpoints**:
  - **GET /api/conversation/{id}/export/json**: Export as JSON
  - **GET /api/conversation/{id}/export/markdown**: Export as Markdown
  - **GET /api/conversation/{id}/export/text**: Export as plain text
  - **GET /api/conversation/{id}/export/xml**: Export as XML

**Configuration Changes:**
- `appsettings.json` includes new OpenAI configuration:
  - `Temperature`: Base temperature (default 0.7)
  - `MaxTokens`: Token limit per response (default 500)
  - Model remains `gpt-3.5-turbo`

**UI Changes:**
- New conversation length selector
- New politeness control slider
- Phase badges in message display
- Export buttons after conversation completion

**Breaking Changes:**
- None - v1.0 conversations remain compatible
- Existing conversations display correctly with inferred phase information

---

## Roadmap

### Planned Features (Future Versions)

**v2.0 - User Management:**
- User authentication and accounts
- Personal conversation history
- Saved personality profiles
- Topic favorites

**v2.0 - Advanced Features:**
- Real-time streaming with SignalR
- Multi-agent support (3+ agents)
- Custom system prompts
- Token usage tracking and cost monitoring

**v2.0 - Production Ready:**
- Rate limiting
- Advanced logging with file persistence
- Message sanitization and content filtering
- Production database support (SQL Server, PostgreSQL)
- Docker containerization

**v3.0 - Analytics:**
- Conversation analytics dashboard
- Usage statistics
- AI response quality metrics
- Topic trends and insights

---

## Support

For issues, questions, or feature requests:
- **Documentation**: [README.md](README.md), [API.md](API.md), [UI.md](UI.md)
- **GitHub Issues**: Report bugs or request features
- **Discussions**: Ask questions and share ideas

---

**Maintained by:** AI Agent Conversation Team  
**License:** MIT
