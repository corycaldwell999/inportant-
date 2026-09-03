import { z } from 'zod';

/**
 * Trauma-Informed Platform - Shared Validation Schemas
 * Using Zod for runtime type safety and validation
 */

// ============================================================================
// Auth Validation
// ============================================================================

export const EmailSchema = z.string().email().toLowerCase();

export const PasswordSchema = z
  .string()
  .min(10, 'Password must be at least 10 characters')
  .regex(/[A-Z]/, 'Password must contain an uppercase letter')
  .regex(/[a-z]/, 'Password must contain a lowercase letter')
  .regex(/[0-9]/, 'Password must contain a number');

export const DisplayNameSchema = z
  .string()
  .min(2, 'Display name must be at least 2 characters')
  .max(50, 'Display name must be at most 50 characters');

export const AuthCredentialsSchema = z.object({
  email: EmailSchema,
  password: PasswordSchema,
});

export const RegistrationDataSchema = z.object({
  email: EmailSchema,
  password: PasswordSchema,
  displayName: DisplayNameSchema,
  acceptTerms: z.boolean().refine((val) => val === true, {
    message: 'You must accept the terms of service',
  }),
  acceptPrivacyPolicy: z.boolean().refine((val) => val === true, {
    message: 'You must accept the privacy policy',
  }),
});

// ============================================================================
// Profile Validation
// ============================================================================

export const UserProfileSchema = z.object({
  displayName: DisplayNameSchema,
  avatar: z.string().url().optional(),
  pronouns: z.string().max(50).optional(),
  bio: z.string().max(500).optional(),
  interests: z.array(z.string()).max(20),
  isPrivate: z.boolean().default(false),
  allowDirectMessages: z.boolean().default(true),
  allowMentions: z.boolean().default(true),
  allowDiscovery: z.boolean().default(false),
});

// ============================================================================
// Community Validation
// ============================================================================

export const CommunitySchema = z.object({
  name: z.string().min(2).max(100),
  description: z.string().max(1000),
  rules: z.string().max(5000).optional(),
  visibility: z.enum(['public', 'private', 'invite-only', 'hidden']),
});

// ============================================================================
// Content Validation
// ============================================================================

export const PostContentSchema = z
  .string()
  .min(1, 'Post cannot be empty')
  .max(5000, 'Post exceeds maximum length');

export const PostSchema = z.object({
  communityId: z.string().uuid(),
  content: PostContentSchema,
  visibility: z.enum(['public', 'private', 'community-only']),
  contentWarning: z.string().max(200).optional(),
  isAnonymous: z.boolean().default(false),
});

export const CommentSchema = z.object({
  postId: z.string().uuid(),
  content: z.string().min(1).max(2000),
  isAnonymous: z.boolean().default(false),
});

export const ReactionSchema = z.object({
  targetId: z.string().uuid(),
  targetType: z.enum(['post', 'comment']),
  reaction: z.enum([
    'hear_you',
    'thinking_of_you',
    'thank_you',
    'not_alone',
    'sending_support',
    'helpful',
    'relate',
    'gentle_encouragement',
  ]),
});

// ============================================================================
// Journal Validation
// ============================================================================

export const JournalEntrySchema = z.object({
  content: z.string().min(1).max(50000),
  mood: z.string().optional(),
  tags: z.array(z.string()).max(10),
});

// ============================================================================
// Wellness Validation
// ============================================================================

export const MoodCheckInSchema = z.object({
  mood: z.string().min(1).max(50),
  intensity: z.number().min(1).max(10),
  notes: z.string().max(500).optional(),
});

export const WellnessGoalSchema = z.object({
  title: z.string().min(2).max(100),
  description: z.string().max(500).optional(),
  category: z.string().min(1),
});

// ============================================================================
// Safety & Crisis Validation
// ============================================================================

export const SafetyContactSchema = z.object({
  name: z.string().min(1).max(100),
  relationship: z.string().min(1).max(100),
  contact: z.string().min(1).max(100),
});

export const SafetyPlanSchema = z.object({
  warningSignals: z.array(z.string()).min(1).max(20),
  copingStrategies: z.array(z.string()).min(1).max(20),
  trustedContacts: z.array(SafetyContactSchema).min(1).max(10),
  reasons: z.array(z.string()).min(1).max(10),
});

// ============================================================================
// Learning Validation
// ============================================================================

export const CourseSchema = z.object({
  title: z.string().min(2).max(200),
  description: z.string().max(2000),
  estimatedHours: z.number().positive(),
});

export const LessonSchema = z.object({
  title: z.string().min(2).max(200),
  content: z.string().min(1),
  order: z.number().nonnegative(),
});

// ============================================================================
// Reporting Validation
// ============================================================================

export const ReportSchema = z.object({
  reportedContentId: z.string().uuid().optional(),
  reportedUserId: z.string().uuid().optional(),
  reportReason: z.enum([
    'spam',
    'harassment',
    'hate_speech',
    'violence',
    'self_harm',
    'suicide_risk',
    'abuse',
    'impersonation',
    'privacy_violation',
    'misinformation',
    'other',
  ]),
  description: z.string().max(2000),
});

// ============================================================================
// Messaging Validation
// ============================================================================

export const DirectMessageSchema = z.object({
  conversationId: z.string().uuid(),
  content: z.string().min(1).max(2000),
});

// ============================================================================
// Pagination Validation
// ============================================================================

export const PaginationSchema = z.object({
  page: z.number().int().positive().default(1),
  limit: z.number().int().min(1).max(100).default(20),
});

// Type exports
export type EmailType = z.infer<typeof EmailSchema>;
export type PasswordType = z.infer<typeof PasswordSchema>;
export type DisplayNameType = z.infer<typeof DisplayNameSchema>;
export type AuthCredentials = z.infer<typeof AuthCredentialsSchema>;
export type RegistrationData = z.infer<typeof RegistrationDataSchema>;
export type UserProfile = z.infer<typeof UserProfileSchema>;
export type Community = z.infer<typeof CommunitySchema>;
export type Post = z.infer<typeof PostSchema>;
export type Comment = z.infer<typeof CommentSchema>;
export type Reaction = z.infer<typeof ReactionSchema>;
export type JournalEntry = z.infer<typeof JournalEntrySchema>;
export type MoodCheckIn = z.infer<typeof MoodCheckInSchema>;
export type WellnessGoal = z.infer<typeof WellnessGoalSchema>;
export type SafetyContact = z.infer<typeof SafetyContactSchema>;
export type SafetyPlan = z.infer<typeof SafetyPlanSchema>;
export type Course = z.infer<typeof CourseSchema>;
export type Lesson = z.infer<typeof LessonSchema>;
export type Report = z.infer<typeof ReportSchema>;
export type DirectMessage = z.infer<typeof DirectMessageSchema>;
export type Pagination = z.infer<typeof PaginationSchema>;
