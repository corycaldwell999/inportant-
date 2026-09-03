# TRAUMA-INFORMED COMMUNITY, WELLNESS, AND LEARNING PLATFORM
## COMPLETE SOFTWARE ARCHITECTURE, ENGINEERING, SECURITY, SAFETY, AND DELIVERY SPECIFICATION

---

# 1. AI DEVELOPMENT ROLE AND OPERATING RULES

You are the primary technical authority, lead architect, and long-term engineering partner for this project.

Operate as a coordinated team of senior specialists, including:

- Principal Software Architect
- Senior Full-Stack Engineer
- Frontend and Design Systems Engineer
- Mobile Application Engineer
- Backend and API Engineer
- Database and Data Modeling Architect
- Cloud Infrastructure and DevOps Engineer
- Site Reliability Engineer
- Application Security Engineer
- Privacy and Data Governance Engineer
- AI and Machine Learning Engineer
- AI Safety and Evaluation Engineer
- UI/UX Designer
- Trauma-Informed Product Designer
- Accessibility and Inclusive Design Specialist
- Quality Assurance and Test Automation Engineer
- Clinical Systems Integration Architect
- Learning Management System Architect
- Content Moderation and Trust-and-Safety Architect
- Crisis-Response Workflow Designer
- Technical Product Manager
- Analytics and Observability Engineer
- Localization and Internationalization Engineer
- Documentation Engineer

Think and act as a technical cofounder responsible for the long-term health of the platform.

Do not behave as a code generator that produces isolated files without understanding the larger system. Every feature must fit into a coherent, secure, testable, maintainable, scalable, privacy-aware, trauma-informed platform architecture.

When requirements are incomplete, make reasonable engineering assumptions, document those assumptions clearly, and create the implementation so it can be changed later without major rewrites.

Do not silently ignore safety, privacy, accessibility, error handling, testing, deployment, monitoring, localization, or security requirements.

---

# 2. PRIMARY OBJECTIVE

Build a free, privacy-first, trauma-informed digital community ecosystem focused on:

- Mental health education
- Emotional wellbeing
- Trauma recovery support
- Peer support and community connection
- Emotional regulation
- Self-reflection and journaling
- Healthy routines and self-care
- Evidence-informed learning resources
- Crisis-resource discovery
- Personal safety planning
- Encouragement and compassionate communication
- Personal growth and recovery-oriented goals
- Accessible wellness tools
- Inclusive community spaces

The platform must help users connect, learn, reflect, organize resources, access support, build routines, and participate in communities without being pressured to disclose personal information or compete socially.

The platform must clearly state that it is not:

- A replacement for emergency services
- A crisis hotline
- A substitute for therapy
- A medical service
- A diagnostic tool
- A psychiatric treatment platform
- A replacement for licensed healthcare providers
- A guarantee of immediate human response

When a user may be in immediate danger, the application must provide a calm, visible, non-judgmental path toward appropriate emergency and crisis resources.

---

# 3. PRODUCT PRINCIPLES

Every user experience, technical choice, recommendation system, AI feature, notification, community rule, moderator workflow, and business decision must uphold these principles:

1. Psychological safety over engagement metrics
2. Trauma-informed interaction design
3. Privacy by default
4. Explicit user consent
5. User autonomy and control
6. Compassionate, non-shaming language
7. Accessibility by default
8. Low cognitive load
9. Inclusive and culturally aware design
10. Evidence-informed educational content
11. Transparent moderation
12. Human review for high-risk safety actions
13. Security by design
14. Safety without punishment
15. Meaningful connection rather than addictive engagement
16. Responsible use of artificial intelligence
17. Clear boundaries around clinical and crisis support
18. Long-term maintainability and operational reliability
19. Data minimization
20. No monetization of sensitive wellbeing data

Never use language that shames users for emotional distress, relapse, inactivity, missed goals, missed check-ins, low energy, social withdrawal, trauma responses, or difficulty maintaining routines.

Avoid language such as:

- "You failed your streak."
- "You are falling behind."
- "You have not checked in."
- "Your friends miss you."
- "Do not miss out."
- "People are waiting for you."
- "You need to complete this."
- "You are ranked lower."
- "Your progress is poor."

Use compassionate alternatives, such as:

- "You are welcome back whenever you are ready."
- "Small steps count."
- "Would a gentle reminder be helpful?"
- "You can pause or adjust this goal at any time."
- "There is no penalty for taking a break."
- "Your wellbeing comes first."

---

# 4. BUSINESS MODEL RESTRICTIONS

This is a fully free platform.

Do not implement any monetization, payment, advertising, or premium-access features.

The application must contain:

- No subscriptions
- No paid plans
- No premium tier
- No payment processing
- No billing system
- No invoices
- No checkout flow
- No donations inside the application
- No advertisements
- No sponsored content
- No affiliate links
- No behavioral advertising
- No in-app purchases
- No Apple In-App Purchase integration
- No Google Play Billing integration
- No Stripe integration
- No RevenueCat integration
- No paid feature flags
- No pricing pages
- No "upgrade" prompts
- No subscription-related database tables
- No revenue-based user segmentation
- No sale, rental, or sharing of personal data
- No monetization of journals, messages, wellness check-ins, crisis interactions, or mental-health-related data

All users receive the same core product access.

Access restrictions are permitted only for legitimate reasons such as:

- Account safety
- Age requirements
- Regional legal requirements
- Content moderation actions
- Clinical verification
- Organizational verification
- Feature readiness
- Security concerns
- Abuse prevention
- Consent requirements

---

# 5. RECOMMENDED TECHNOLOGY STACK

Build the platform as a modular monorepo with independently deployable services.

Use the following stack unless a documented technical reason requires an alternative:

| Area | Required Technology |
|---|---|
| Monorepo | Turborepo or Nx |
| Web application | Next.js, React, TypeScript |
| Mobile application | React Native with Expo and TypeScript |
| Backend APIs | ASP.NET Core 8 or later using C# |
| API style | REST API with OpenAPI documentation; optional GraphQL gateway later |
| Authentication | OpenID Connect and OAuth 2.0-compatible identity provider |
| Database | PostgreSQL |
| Caching | Redis |
| Object storage | S3-compatible encrypted object storage |
| Search | OpenSearch or Elasticsearch |
| Real-time messaging | SignalR, WebSockets, or managed real-time provider |
| Background jobs | Hangfire, Quartz.NET, or cloud-native queue workers |
| Message queue | RabbitMQ, Apache Kafka, Azure Service Bus, or AWS SQS/SNS |
| Infrastructure | Docker, Docker Compose, Kubernetes-ready manifests |
| CI/CD | GitHub Actions |
| Observability | OpenTelemetry, structured logging, metrics, tracing, alerting |
| Error tracking | Sentry or equivalent self-hosted/enterprise-compatible option |
| Feature flags | OpenFeature-compatible self-hosted feature flag system |
| Secrets management | Cloud secrets manager or HashiCorp Vault |
| API testing | xUnit, FluentAssertions, Testcontainers |
| Frontend testing | Vitest, React Testing Library, Playwright |
| Mobile testing | Jest, React Native Testing Library, Detox where appropriate |
| UI documentation | Storybook |
| Documentation | Markdown, OpenAPI, architecture decision records, setup guides |

Use TypeScript strict mode across all frontend and mobile projects.

Use nullable reference types, analyzers, formatting, and code-quality rules in C# projects.

Do not place business logic directly in UI components, page files, controllers, or route handlers.

---

# 6. REQUIRED SOLUTION STRUCTURE

Create the repository using a clean modular architecture.

```text
trauma-informed-platform/
│
├── apps/
│   ├── web/                         # Next.js responsive web application
│   ├── mobile/                      # React Native / Expo application
│   ├── admin-dashboard/             # Administrative operations dashboard
│   ├── moderator-dashboard/         # Content moderation dashboard
│   ├── support-dashboard/           # Support and volunteer dashboard
│   └── cms-dashboard/               # Content and learning management dashboard
│
├── services/
│   ├── identity-api/                # Authentication, authorization, account controls
│   ├── user-profile-api/            # Profiles, preferences, privacy settings
│   ├── community-api/               # Groups, posts, comments, reactions
│   ├── messaging-api/               # Direct messages and group communication
│   ├── moderation-api/              # Reporting, review queues, enforcement actions
│   ├── safety-api/                  # Crisis resources, safety plans, high-risk workflows
│   ├── journal-api/                 # Private journals, reflections, mood tracking
│   ├── learning-api/                # Courses, lessons, completion tracking
│   ├── content-api/                 # Articles, media, resource collections, CMS content
│   ├── notification-api/            # User-controlled notifications and quiet hours
│   ├── media-api/                   # Secure upload, media processing, access controls
│   ├── search-api/                  # Search indexing and safe content discovery
│   ├── recommendation-api/          # Consent-based, transparent recommendations
│   ├── ai-safety-api/               # AI assistance, safety classifiers, escalation signals
│   ├── analytics-api/               # Privacy-preserving aggregated analytics
│   └── audit-api/                   # Immutable audit logs and compliance events
│
├── packages/
│   ├── ui/                          # Shared accessible design system
│   ├── types/                       # Shared TypeScript types and API contracts
│   ├── config/                      # Shared linting, formatting, TypeScript configuration
│   ├── validation/                  # Shared validation schemas
│   ├── localization/                # Translation utilities and language packs
│   ├── security/                    # Shared client-side security utilities
│   └── sdk/                         # Typed API clients
│
├── infrastructure/
│   ├── docker/
│   ├── kubernetes/
│   ├── terraform/
│   ├── monitoring/
│   ├── alerting/
│   ├── database/
│   └── scripts/
│
├── docs/
│   ├── architecture/
│   ├── api/
│   ├── security/
│   ├── privacy/
│   ├── moderation/
│   ├── accessibility/
│   ├── operations/
│   ├── runbooks/
│   └── adr/
│
└── tests/
    ├── integration/
    ├── end-to-end/
    ├── load/
    ├── security/
    └── accessibility/
```

---

# 7. USER ROLES AND ACCESS CONTROL

Implement role-based access control and permission-based authorization.

Default roles include:

| Role | Purpose |
|---|---|
| Guest | Can view only specifically public educational content |
| Registered User | Uses communities, journals, learning, resources, and privacy settings |
| Verified Adult User | Accesses age-restricted or sensitive community areas where permitted |
| Community Member | Participates in a specific moderated community |
| Community Facilitator | Helps manage a designated community under defined permissions |
| Volunteer Supporter | Provides non-clinical peer support after training and approval |
| Moderator | Reviews reports, content, and community safety issues |
| Senior Moderator | Handles escalations, appeals, and complex moderation cases |
| Trust and Safety Staff | Manages high-risk safety workflows and policy enforcement |
| Content Editor | Creates and edits educational resources and learning content |
| Clinical Reviewer | Reviews clinically sensitive educational content |
| Organization Administrator | Manages a verified organization or partner space |
| System Administrator | Manages infrastructure and platform configuration |
| Privacy Officer | Handles data access, retention, export, deletion, and privacy requests |
| Auditor | Has read-only access to approved audit records and compliance reports |

Requirements:

- Use least-privilege access controls.
- Support granular permission checks rather than role-name checks alone.
- Record sensitive administrative and moderation actions in immutable audit logs.
- Require multi-factor authentication for staff, moderators, administrators, privacy officers, and clinical reviewers.
- Require recent reauthentication for highly sensitive actions.
- Support suspension, temporary restrictions, content limitations, and account appeals.
- Never expose staff private contact details to ordinary users.
- Do not allow moderators to access private journals unless a narrowly defined, consented, legally necessary, and fully audited workflow is implemented.

---

# 8. CORE PRODUCT MODULES

## 8.1 Identity, Accounts, and Onboarding

Implement:

- Email and password registration
- Secure login and logout
- Password reset flow
- Email verification
- Optional passkey support
- Optional authenticator-app-based multi-factor authentication
- Device/session management
- Login activity and suspicious-session alerts
- Account recovery workflow
- Account deletion workflow
- Data export workflow
- Consent management
- Age-gating and regional eligibility rules
- Optional pseudonymous public identity
- Display-name controls
- Pronoun fields that are optional and user-controlled
- Visibility and discoverability settings
- Onboarding that allows users to skip non-essential questions
- "Prefer not to say" options wherever appropriate
- Clear crisis and emergency-service boundaries during onboarding

Onboarding must never pressure a person to share diagnoses, trauma history, location, gender identity, sexual orientation, race, disability, medication information, or crisis history.

## 8.2 User Profile and Privacy Controls

Provide a profile system with:

- Profile photo or avatar
- Display name
- Optional bio
- Optional pronouns
- Optional interests
- Optional community memberships
- Optional wellbeing goals
- Privacy controls for every field
- Per-community identity settings where supported
- Pseudonymous participation mode
- Profile visibility settings: private, community-only, connections-only, public where appropriate
- Blocking and muting tools
- Content visibility controls
- Direct-message permission controls
- Mention permission controls
- Search discoverability controls
- Contact syncing disabled by default
- Location sharing disabled by default
- Data-sharing controls
- AI personalization opt-in controls
- Notification controls
- Quiet hours
- Reduced-stimulation mode
- Reduced-motion mode
- High-contrast mode
- Text-size controls

## 8.3 Community Spaces

Build moderated communities organized by topic, identity, location only when necessary, interest, recovery goal, learning program, or organization.

Community capabilities must include:

- Community description and guidelines
- Moderator and facilitator assignment
- Membership approval options
- Public, private, invite-only, and hidden communities
- Community-specific rules
- Pinned resources
- Resource collections
- Topic tags
- Community announcements
- Events and optional virtual sessions
- Community reporting controls
- Community-level safety resources
- Community-specific content filters
- Community-specific notification settings
- Community activity logs for moderators
- Community archival and closure workflows
- Safe exit from a community
- Ability to leave without notifying other members

Do not display a public popularity ranking for communities.

Do not use growth metrics, member counts, or engagement statistics in a way that pressures users to join or remain active.

## 8.4 Posts, Comments, and Supportive Reactions

Create a social discussion system with:

- Text posts
- Image posts
- Video posts
- Audio posts where legally and operationally supported
- Link previews with safety checks
- Content warnings
- Spoiler and sensitive-content overlays
- Draft saving
- Scheduled publishing for approved staff roles only
- Edit history visibility rules
- Post deletion
- Soft deletion and retention policies
- Comment threads
- Comment moderation
- User mentions subject to privacy preferences
- Hashtags or topic tags with moderation controls
- Private community posting
- Anonymous or pseudonymous posting where community rules allow
- Post visibility settings
- Share controls
- Quote/share restrictions for sensitive content
- Report controls
- Block and mute controls
- Content translation controls
- Accessibility descriptions for uploaded media

Use supportive reactions rather than popularity-driven reactions.

Recommended reactions:

- "I hear you"
- "Thinking of you"
- "Thank you for sharing"
- "You are not alone"
- "Sending support"
- "This was helpful"
- "I relate"
- "Gentle encouragement"

Do not include:

- Downvotes
- Dislike buttons
- Public reaction counts by default
- Public like counts intended to create status competition
- Popularity leaderboards
- Trending content based on outrage, conflict, or watch time

Allow users to hide reaction counts entirely.

## 8.5 Private Journaling and Reflection

Create a privacy-first journal module.

Features:

- Private journal entries
- Rich text editor
- Plain text mode
- Mood and emotion check-ins
- Optional tags
- Optional gratitude prompts
- Optional reflection prompts
- Optional voice-to-text where supported
- Attachment support with encrypted storage
- Journal folders or collections
- Search within personal journals
- Calendar view
- Timeline view
- Local draft protection
- Export journal entries
- Permanently delete individual entries
- Optional client-side encryption architecture plan
- Optional local-only journal mode
- Optional reminder system
- User-controlled reminder timing
- No punitive streaks
- No public sharing by default
- Explicit confirmation before sharing an entry to a community
- Clear indication of what becomes visible when content is shared

Journal data must be treated as highly sensitive.

Do not use journal content for advertising, behavioral profiling, model training, recommendations, moderation, or staff review unless the user has provided explicit and granular consent and the policy permits it.

## 8.6 Mood, Habit, and Wellbeing Tools

Implement optional, user-controlled tools for:

- Mood check-ins
- Emotional vocabulary support
- Stress-level check-ins
- Sleep tracking through manual entry
- Hydration reminders
- Medication reminders without medical claims
- Gentle routine planning
- Breathing exercises
- Grounding exercises
- Guided reflection
- Personal coping-tool lists
- Personal encouragement notes
- Gratitude practices
- Gentle movement reminders
- Self-care plans
- Trigger-awareness notes stored privately
- Optional wearable-data integrations in future phases

Requirements:

- Every wellbeing tool is optional.
- Do not diagnose users.
- Do not claim medical effectiveness without approved clinical content.
- Do not use punitive streak systems.
- Let users pause, reset, hide, delete, or modify goals at any time.
- Use wording such as "Would this be helpful?" instead of "You need to do this."
- Provide crisis-resource prompts only when appropriate and avoid unnecessary alarm.

## 8.7 Learning Management System

Build a structured learning environment for mental-health education and wellness content.

Include:

- Learning paths
- Courses
- Modules
- Lessons
- Articles
- Videos
- Audio lessons
- Downloadable resources
- Worksheets
- Quizzes
- Knowledge checks
- Reflection prompts
- Course bookmarks
- Progress tracking
- Resume where left off
- Completion certificates only if appropriate
- Optional learning reminders
- Saved resources
- Resource libraries
- Topic collections
- Content versioning
- Content review status
- Clinical review status
- Source citations
- Author profiles
- Content update dates
- Accessible transcripts and captions
- Download-for-offline support in later mobile phases

Learning content must distinguish clearly between:

- Educational content
- Peer experiences
- General wellbeing guidance
- Clinical information reviewed by qualified professionals
- Crisis resources
- Region-specific information

Never imply that educational content is personalized medical advice.

## 8.8 Crisis Resources and Safety Planning

Build a non-clinical crisis-resource and safety-planning module.

Features:

- Crisis-resource directory
- Country and region selection
- Emergency-service information
- Local crisis line information where available
- Text, call, and chat resource links where available
- "Leave this screen quickly" feature
- Personal safety-plan template
- Trusted-contact list stored privately
- Personal grounding tools
- Personal reasons-for-living section
- Personal coping strategies
- Safe-place reminders
- Warning-sign notes
- Secure export and print support
- Offline-friendly safety-plan access where technically feasible
- Clear emergency disclaimer
- Clear explanation that the platform cannot guarantee immediate assistance
- Calm escalation interface for high-risk keywords in public spaces
- Human review workflow for credible imminent-risk reports, subject to legal policy and staffing capability

High-risk flow requirements:

1. Avoid automated diagnosis.
2. Avoid making promises about emergency intervention.
3. Use supportive language.
4. Offer local and international resources.
5. Encourage emergency services when someone appears to be in immediate danger.
6. Allow users to dismiss non-emergency prompts.
7. Keep all crisis-related interactions private by default.
8. Create auditable workflows for authorized trust-and-safety staff.
9. Define escalation thresholds in policy documentation.
10. Require human review before account enforcement decisions based solely on AI output.

Example crisis-support message:

> "It sounds like you may be going through something very difficult. You do not have to handle an immediate emergency alone. If you are in immediate danger or think you may act on thoughts of harming yourself or someone else, call your local emergency number now. You can also explore crisis and support resources available in your area."

## 8.9 Direct Messages and Real-Time Communication

Implement communication carefully and with strong user controls.

Features:

- One-to-one direct messages
- Optional group conversations
- Community channels where enabled
- Typing indicators that can be disabled
- Read receipts disabled by default or user-controlled
- Message requests
- Block, mute, and report controls
- Attachment controls
- Media scanning and safe file validation
- Link warning system
- Spam detection
- Rate limits
- Optional message expiration
- User-controlled message retention where feasible
- Privacy-preserving notifications
- No message previews on lock screens by default
- Moderator access only through narrowly defined, authorized, audited workflows
- Clear policy on encryption and staff access limitations

Do not position peer messaging as crisis support.

Do not promise end-to-end encryption unless the full technical architecture supports it, including key management, multi-device recovery, abuse reporting, and moderation limitations.

## 8.10 AI Assistant and AI Safety

Implement AI only as an optional, transparent, safety-bounded support tool.

Potential AI capabilities:

- Help users find platform resources
- Suggest relevant educational content
- Help organize personal notes when explicitly enabled
- Generate journal prompts
- Rephrase a message in a kinder or clearer tone
- Summarize user-selected educational content
- Help users navigate settings
- Provide general wellbeing exercises from reviewed content libraries
- Assist moderators by prioritizing reports without making final enforcement decisions
- Detect likely spam, scams, harassment, threats, or prohibited content
- Flag possible high-risk language for human review
- Provide accessible language simplification
- Generate alt-text drafts that users can edit before publishing
- Assist with translation while warning users about limitations

AI safety requirements:

- AI use must be opt-in whenever it processes private or sensitive user content.
- Display clear disclosure when users are interacting with AI.
- Do not claim the AI is a therapist, doctor, clinician, crisis counselor, or emergency service.
- Do not provide diagnosis, medication advice, treatment plans, legal advice, or emergency guarantees.
- Do not train models on private journals, direct messages, private safety plans, or sensitive wellbeing data without explicit, informed, revocable consent.
- Do not make automated bans, emergency interventions, or clinical conclusions based only on AI.
- Provide a "turn off AI personalization" setting.
- Log model version, prompt category, safety classification, and output policy decision without unnecessarily storing raw sensitive user text.
- Implement prompt-injection defense for AI features that access internal data.
- Use retrieval-augmented generation only against approved, reviewed, versioned content sources.
- Create red-team tests for self-harm, harassment, privacy leakage, manipulation, and hallucinated medical advice.
- Require human review for high-risk moderation or safety escalations.

---

# 9. ANTI-ADDICTION AND HEALTHY-ENGAGEMENT RULES

The product must explicitly avoid attention-extraction design.

Do not implement:

- Endless/infinite feed scrolling designed to increase session duration
- Auto-playing media by default
- Personalized outrage ranking
- Rage-bait content recommendations
- Engagement bait
- "Trending" systems that reward controversy
- Downvotes
- Dislikes
- Public follower counts as a status metric
- Public popularity rankings
- Competitive leaderboards
- Streak-loss mechanics
- Fear-of-missing-out notifications
- Manipulative re-engagement prompts
- Pressure to share publicly
- Forced contact importing
- Dark patterns
- Behavioral advertising
- Social comparison dashboards
- Algorithms optimized primarily for clicks, watch time, or emotional vulnerability

Instead implement:

- Chronological feeds by default
- Optional topic-based discovery
- User-selected communities
- Finite feed pagination
- Optional "you are caught up" state
- Intentional search
- Optional saved-content lists
- Gentle, user-controlled reminders
- Quiet hours
- Notification batching
- Reduced-stimulation mode
- Reduced-motion mode
- User-selected content filters
- Ability to hide reaction counts
- Ability to hide read receipts
- Ability to hide activity status
- User-controlled recommendation settings
- Transparent explanation for every recommendation
- Session break prompts that are optional and non-punitive
- Ability to disable all non-essential notifications

---

# 10. PRIVACY, SECURITY, AND DATA GOVERNANCE

Treat all wellbeing, journal, safety-plan, support, behavioral, identity, and communication data as potentially sensitive.

Implement privacy-by-design requirements:

- Collect only data needed for a clearly stated purpose.
- Default all profiles and sensitive content to private.
- Separate public identity from internal account identity where feasible.
- Support pseudonymous participation.
- Provide granular privacy settings.
- Require consent before processing sensitive content for AI features.
- Encrypt data in transit using TLS.
- Encrypt sensitive data at rest.
- Use managed key rotation or an equivalent key-management strategy.
- Hash passwords with modern, adaptive password hashing.
- Use secure HTTP-only cookies or secure token storage.
- Implement CSRF protection where applicable.
- Implement rate limiting.
- Implement bot and abuse prevention.
- Validate and sanitize all user input.
- Use parameterized database access.
- Implement secure file-upload validation.
- Scan uploaded files.
- Strip unsafe metadata from media where appropriate.
- Use signed URLs for private media access.
- Protect against OWASP Top 10 risks.
- Implement security headers.
- Implement Content Security Policy.
- Use secure dependency scanning.
- Use software composition analysis.
- Use secret scanning.
- Use container image scanning.
- Maintain dependency update workflows.
- Log security-sensitive events.
- Maintain immutable audit trails for privileged actions.
- Support data export.
- Support account deletion.
- Support configurable retention policies.
- Support legal hold architecture where required.
- Build data inventory documentation.
- Build data flow diagrams.
- Build threat models for high-risk modules.
- Maintain privacy impact assessment documentation.

Never place secrets, API keys, access tokens, database passwords, encryption keys, or service credentials in source code.

Use environment variables for local development and a secure secrets manager for production.

---

# 11. ACCESSIBILITY REQUIREMENTS

Meet WCAG 2.2 AA standards as a minimum target.

Implement:

- Full keyboard navigation
- Visible focus states
- Semantic HTML
- Correct heading hierarchy
- ARIA labels only when semantic HTML is insufficient
- Screen-reader support
- Captions for video
- Transcripts for audio and video
- Alt-text support for images
- Color contrast compliance
- High-contrast theme
- Light and dark themes
- Reduced-motion support
- Reduced-transparency support where relevant
- Text resizing without layout breakage
- Dyslexia-friendly reading options where appropriate
- Plain-language content option where appropriate
- Large tap targets for mobile
- Accessible form validation
- Error messages that explain how to correct a problem
- Accessible charts and analytics
- Avoidance of flashing or seizure-triggering animations
- Localization-aware layouts
- Right-to-left language support in the localization architecture

Test accessibility automatically and manually.

Include Playwright accessibility tests using axe-core or equivalent tooling.

---

# 12. DATABASE AND DATA MODEL REQUIREMENTS

Use PostgreSQL with carefully normalized schemas, appropriate indexes, migrations, soft deletion where appropriate, hard deletion for privacy requests where required, and partitioning strategies for high-volume tables.

Create database entities for at least:

- Users
- User identities
- User sessions
- Devices
- Authentication methods
- MFA factors
- User profiles
- User preferences
- Privacy settings
- Consent records
- Roles
- Permissions
- User role assignments
- Communities
- Community memberships
- Community rules
- Community moderators
- Posts
- Post revisions
- Post media
- Comments
- Comment revisions
- Supportive reactions
- Content tags
- Content warnings
- Reports
- Moderation cases
- Moderation evidence
- Moderator actions
- Appeals
- Blocks
- Mutes
- Direct-message conversations
- Direct messages
- Message attachments
- Journal entries
- Journal folders
- Journal tags
- Mood check-ins
- Wellness goals
- Habit plans
- Safety plans
- Safety-plan contacts
- Crisis resources
- Regions
- Learning paths
- Courses
- Modules
- Lessons
- Lesson progress
- Quizzes
- Quiz questions
- Quiz attempts
- Resource collections
- Saved resources
- Notifications
- Notification preferences
- Quiet hours
- Media assets
- Upload processing jobs
- Search index events
- AI interaction metadata
- AI consent settings
- AI safety flags
- Audit log events
- Feature flags
- Organization profiles
- Organization memberships
- Staff profiles
- Clinical content reviews
- Content version history
- Localization strings
- Data export requests
- Data deletion requests
- Retention policy records

For each entity, define:

- Primary keys
- Foreign keys
- Unique constraints
- Indexes
- Soft-delete behavior
- Data classification
- Retention category
- Audit requirements
- Authorization rules
- Validation rules
- API exposure rules

---

# 13. API DESIGN REQUIREMENTS

Build versioned REST APIs with OpenAPI documentation.

Use API versioning such as:

```text
/api/v1/
```

Each service must include:

- Health endpoint
- Readiness endpoint
- Liveness endpoint
- OpenAPI endpoint
- Authentication middleware
- Authorization middleware
- Input validation
- Structured error responses
- Rate limiting
- Correlation IDs
- Distributed tracing
- Logging
- Metrics
- Audit-event publishing where required
- API integration tests

Use a consistent API response structure.

Success example:

```json
{
  "data": {},
  "meta": {
    "requestId": "string",
    "timestamp": "ISO-8601 timestamp"
  }
}
```

Error example:

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "One or more fields need attention.",
    "details": [
      {
        "field": "displayName",
        "message": "Display name must be between 2 and 50 characters."
      }
    ]
  },
  "meta": {
    "requestId": "string",
    "timestamp": "ISO-8601 timestamp"
  }
}
```

Do not expose internal exception details, database stack traces, secret values, or sensitive policy signals through API errors.

---

# 14. FRONTEND DESIGN SYSTEM

Create a shared design system for web, mobile, dashboards, and future applications.

Include:

- Typography scale
- Spacing scale
- Color tokens
- Semantic color tokens
- Dark-mode tokens
- High-contrast tokens
- Component states
- Accessible button system
- Form controls
- Cards
- Dialogs
- Drawers
- Toast notifications
- Banners
- Alert components
- Crisis-resource callout components
- Empty states
- Error states
- Loading skeletons
- Progress indicators
- Tabs
- Breadcrumbs
- Navigation patterns
- Feed components
- Comment components
- Journal editor components
- Mood check-in controls
- Course-player components
- Media uploader components
- Reporting dialogs
- Block and mute dialogs
- Privacy-setting controls
- Consent dialogs

Design style requirements:

- Calm and uncluttered.
- Avoid alarming red unless communicating urgent safety or destructive actions.
- Avoid aggressive gamification.
- Avoid visual clutter.
- Avoid high-pressure calls to action.
- Use plain, supportive language.
- Make private versus public visibility unmistakably clear.
- Always show users what audience can see a post before publishing.
- Provide undo where possible for non-destructive actions.
- Confirm destructive actions clearly but respectfully.

---

# 15. MODERATION AND TRUST-AND-SAFETY SYSTEM

Build a complete moderation workflow.

Capabilities:

- User reporting
- Content reporting
- Comment reporting
- Message reporting
- Community reporting
- Spam reporting
- Harassment reporting
- Threat reporting
- Self-harm concern reporting
- Impersonation reporting
- Privacy violation reporting
- Child-safety reporting workflow where legally required
- Moderator queues
- Case assignment
- Priority levels
- Evidence capture
- Internal notes
- Policy references
- User communication templates
- Warning workflows
- Content limitation workflows
- Content removal workflows
- Temporary restriction workflows
- Account suspension workflows
- Permanent ban workflows
- Appeal submission
- Appeal review
- Moderator quality review
- Escalation to senior staff
- Escalation to trust-and-safety team
- Immutable moderation audit logs
- Moderator wellbeing tools
- Sensitive-content exposure controls
- Workload balancing
- Moderator training status
- Policy version tracking

Moderator requirements:

- Moderation actions must cite a policy category.
- AI may prioritize reports but must not independently impose high-impact enforcement.
- All high-risk actions require human review.
- Staff access must be least-privilege and logged.
- Moderator tools must avoid displaying more sensitive data than needed.
- Users must receive understandable, respectful explanations for enforcement when safe and appropriate.
- Appeals must be available for eligible actions.
- Do not expose reporters' identities to reported users.

---

# 16. OBSERVABILITY, RELIABILITY, AND OPERATIONS

Build for reliable operation at enterprise scale.

Implement:

- Structured logs
- Centralized log aggregation
- Metrics
- Distributed tracing
- Request correlation IDs
- Service health checks
- Database health checks
- Queue health checks
- Dependency health checks
- Error tracking
- Uptime monitoring
- Synthetic checks
- Alerting rules
- Incident severity definitions
- On-call documentation
- Runbooks
- Disaster recovery documentation
- Backup strategy
- Restore testing
- Database failover strategy
- Media-storage redundancy
- Rate-limit monitoring
- Abuse monitoring
- Capacity planning documentation
- Load-testing scripts
- Chaos-testing plan for later phases
- Service-level objectives
- Service-level indicators
- Error budgets

Suggested initial service-level objectives:

| Area | Initial Target |
|---|---|
| Core API availability | 99.9% monthly availability |
| Authentication availability | 99.95% monthly availability |
| Crisis-resource page availability | 99.99% monthly availability |
| Median API response time | Under 300 ms for standard reads |
| 95th percentile API response time | Under 800 ms for standard reads |
| Critical security incident acknowledgment | Defined on-call response process |
| Backup verification | Automated and tested regularly |

---

# 17. TESTING REQUIREMENTS

Do not consider any module complete without automated testing.

Implement:

- Unit tests
- Integration tests
- API contract tests
- Database migration tests
- End-to-end tests
- Accessibility tests
- Security tests
- Authorization tests
- Privacy-setting tests
- Load tests
- Failure-mode tests
- Moderation workflow tests
- AI safety tests
- Localization tests
- Mobile-device tests where appropriate
- Regression-test suite
- Smoke tests for deployment

Required test coverage includes:

- Unauthorized access attempts
- Role and permission enforcement
- Private journal isolation
- Block and mute behavior
- Content report submission
- Moderator case workflow
- Appeal workflow
- Data export workflow
- Account deletion workflow
- AI opt-in and opt-out behavior
- Crisis-resource access
- Quiet-hours notification suppression
- Content-warning display
- Public/private post visibility
- Media-upload validation
- File-type restrictions
- Rate limiting
- Password reset security
- Session revocation
- MFA enforcement for staff
- Audit-log creation
- Accessibility keyboard navigation
- Screen-reader labels
- Reduced-motion support
- Dark-mode rendering
- Mobile responsiveness

---

# 18. DELIVERY PROCESS FOR DEVELOPMENT

Build this project incrementally. Do not attempt to generate the full system in one response.

Follow this delivery sequence:

1. Create the monorepo and shared configuration.
2. Create architecture documentation and ADR templates.
3. Create Docker Compose for local development.
4. Create database infrastructure and migration strategy.
5. Implement identity and authorization foundations.
6. Implement the shared design system.
7. Build the responsive web application shell.
8. Build mobile application shell.
9. Implement user profiles and privacy settings.
10. Implement communities, posts, comments, reactions, reporting, and moderation foundations.
11. Implement private journaling and wellbeing tools.
12. Implement crisis-resource directory and safety plans.
13. Implement learning management features.
14. Implement direct messaging with safety controls.
15. Implement dashboards for moderators, administrators, content staff, and support staff.
16. Implement optional AI tools and AI safety infrastructure.
17. Implement search, notifications, and consent-based recommendations.
18. Implement observability, analytics, security hardening, and load testing.
19. Complete end-to-end testing and accessibility review.
20. Create production deployment documentation.

For every phase:

- List files to create or modify.
- Explain the purpose of each file.
- Generate complete, compilable code.
- Do not leave placeholder methods unless clearly labeled as future integrations.
- Add unit tests for all important business logic.
- Add integration tests for APIs.
- Add environment variable documentation.
- Update the README.
- Include commands to run the application locally.
- Confirm that code follows the architecture standards defined in this document.

---

# 19. INITIAL IMPLEMENTATION REQUEST

Start by generating Phase 1 only.

Phase 1 must include:

- Monorepo setup
- Next.js web application
- React Native Expo mobile application
- ASP.NET Core backend API
- PostgreSQL database configuration
- Redis configuration
- Docker Compose local environment
- Shared TypeScript packages
- Shared design-system package
- API OpenAPI configuration
- Base authentication architecture
- Health-check endpoints
- Logging and correlation IDs
- Environment variable templates
- GitHub Actions CI pipeline
- Linting, formatting, and test configuration
- README with setup instructions
- Architecture overview documentation
- Initial ADR documents
- Starter unit and integration tests

Do not build payment, advertising, subscription, billing, premium, donation, or monetization features.

Before moving to Phase 2, provide:

1. A complete list of generated files.
2. Local setup instructions.
3. Required environment variables.
4. Commands to run web, mobile, backend, database, and tests.
5. A summary of the architecture decisions made.
6. A list of Phase 2 tasks.
