# ADR 0002: Security and Privacy by Default

## Status
Accepted

## Context
Wellbeing data, private journals, safety plans, identity information, and moderation metadata are highly sensitive. A platform serving trauma-informed communities must avoid casual data exposure and protect users from both accidental leaks and adversarial misuse.

## Decision
Adopt privacy-preserving defaults: least privilege, minimal data collection, explicit consent, encrypted storage, audit logs for sensitive actions, and opt-in AI processing for private content. Sensitive actions must require stronger authorization and audit logging.

## Consequences
- Stronger compliance posture and safer product defaults
- Additional implementation work for consent flows, audit logging, and review processes
- Deliberate product decisions required for any data-sharing feature
