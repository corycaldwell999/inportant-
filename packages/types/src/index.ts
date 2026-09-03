/**
 * Trauma-Informed Community Platform - Shared Types
 * These types are used across all applications and services
 */

// ============================================================================
// API Response Envelope Types
// ============================================================================

export type ApiEnvelope<T> = {
  data: T;
  meta: {
    requestId: string;
    timestamp: string;
  };
};

export type ApiErrorDetail = {
  field?: string;
  message: string;
};

export type ApiErrorEnvelope = {
  error: {
    code: string;
    message: string;
    details?: ApiErrorDetail[];
  };
  meta: {
    requestId: string;
    timestamp: string;
  };
};

export type PaginationMeta = {
  page: number;
  limit: number;
  total: number;
  totalPages: number;
};

export type PaginatedEnvelope<T> = ApiEnvelope<T[]> & {
  meta: ApiEnvelope<T>['meta'] & PaginationMeta;
};

// ============================================================================
// Role and Permission Types
// ============================================================================

export type UserRole =
  | 'Guest'
  | 'RegisteredUser'
  | 'VerifiedAdultUser'
  | 'CommunityMember'
  | 'CommunityFacilitator'
  | 'VolunteerSupporter'
  | 'Moderator'
  | 'SeniorModerator'
  | 'TrustAndSafetyStaff'
  | 'ContentEditor'
  | 'ClinicalReviewer'
  | 'OrganizationAdministrator'
  | 'SystemAdministrator'
  | 'PrivacyOfficer'
  | 'Auditor';

export interface Permission {
  id: string;
  name: string;
  description: string;
}

// ============================================================================
// User and Account Types
// ============================================================================

export interface User {
  id: string;
  email: string;
  displayName: string;
  avatar?: string;
  pronouns?: string;
  bio?: string;
  roles: UserRole[];
  isVerified: boolean;
  isEmailVerified: boolean;
  isAccountActive: boolean;
  createdAt: Date;
  updatedAt: Date;
  lastLoginAt?: Date;
}

export interface UserProfile {
  userId: string;
  displayName: string;
  avatar?: string;
  pronouns?: string;
  bio?: string;
  interests: string[];
  isPrivate: boolean;
  allowDirectMessages: boolean;
  allowMentions: boolean;
  allowDiscovery: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface UserPrivacySettings {
  userId: string;
  profileVisibility: 'private' | 'community-only' | 'connections-only' | 'public';
  showEmail: boolean;
  showActivity: boolean;
  allowContactSync: boolean;
  allowLocationSharing: boolean;
  allowAIPersonalization: boolean;
  allowDataSharing: boolean;
  createdAt: Date;
  updatedAt: Date;
}

// ============================================================================
// Authentication Types
// ============================================================================

export interface AuthToken {
  accessToken: string;
  refreshToken?: string;
  expiresIn: number;
  tokenType: 'Bearer';
}

export interface AuthCredentials {
  email: string;
  password: string;
}

export interface RegistrationData extends AuthCredentials {
  displayName: string;
  acceptTerms: boolean;
  acceptPrivacyPolicy: boolean;
}

export interface Session {
  id: string;
  userId: string;
  tokenHash: string;
  deviceName?: string;
  ipAddress: string;
  userAgent: string;
  expiresAt: Date;
  createdAt: Date;
  lastActivityAt: Date;
}

export interface Device {
  id: string;
  userId: string;
  name: string;
  type: 'web' | 'mobile' | 'desktop';
  platform?: string;
  lastActiveAt: Date;
  createdAt: Date;
}

// ============================================================================
// Community Types
// ============================================================================

export interface Community {
  id: string;
  name: string;
  description: string;
  rules?: string;
  visibility: 'public' | 'private' | 'invite-only' | 'hidden';
  memberCount: number;
  isArchived: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface CommunityMembership {
  id: string;
  communityId: string;
  userId: string;
  role: 'member' | 'facilitator' | 'moderator';
  joinedAt: Date;
}

// ============================================================================
// Content Types (Posts, Comments, Reactions)
// ============================================================================

export interface Post {
  id: string;
  communityId: string;
  authorId: string;
  content: string;
  visibility: 'public' | 'private' | 'community-only';
  contentWarning?: string;
  media?: MediaAttachment[];
  isAnonymous: boolean;
  createdAt: Date;
  updatedAt: Date;
  deletedAt?: Date;
}

export interface PostRevision {
  id: string;
  postId: string;
  content: string;
  editedAt: Date;
  editedBy: string;
}

export interface Comment {
  id: string;
  postId: string;
  authorId: string;
  content: string;
  isAnonymous: boolean;
  createdAt: Date;
  updatedAt: Date;
  deletedAt?: Date;
}

export type SupportiveReactionType =
  | 'hear_you'
  | 'thinking_of_you'
  | 'thank_you'
  | 'not_alone'
  | 'sending_support'
  | 'helpful'
  | 'relate'
  | 'gentle_encouragement';

export interface SupportiveReaction {
  id: string;
  targetId: string; // post or comment ID
  targetType: 'post' | 'comment';
  userId: string;
  reaction: SupportiveReactionType;
  createdAt: Date;
}

export interface ContentTag {
  id: string;
  name: string;
  description?: string;
}

// ============================================================================
// Journal Types
// ============================================================================

export interface JournalEntry {
  id: string;
  userId: string;
  content: string;
  mood?: string;
  tags: string[];
  isPrivate: boolean;
  createdAt: Date;
  updatedAt: Date;
  deletedAt?: Date;
}

export interface JournalFolder {
  id: string;
  userId: string;
  name: string;
  description?: string;
  createdAt: Date;
  updatedAt: Date;
}

// ============================================================================
// Wellness and Habit Types
// ============================================================================

export interface MoodCheckIn {
  id: string;
  userId: string;
  mood: string;
  intensity: number;
  notes?: string;
  createdAt: Date;
}

export interface WellnessGoal {
  id: string;
  userId: string;
  title: string;
  description?: string;
  category: string;
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
  completedAt?: Date;
}

export interface HabitPlan {
  id: string;
  userId: string;
  title: string;
  description?: string;
  frequency: 'daily' | 'weekly' | 'custom';
  reminderTime?: string;
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
}

// ============================================================================
// Crisis Resources and Safety
// ============================================================================

export interface CrisisResource {
  id: string;
  name: string;
  description: string;
  resourceType: 'hotline' | 'chat' | 'text' | 'in-person' | 'online';
  contact: string;
  website?: string;
  regions: string[];
  languages: string[];
  available24x7: boolean;
  isVerified: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface SafetyPlan {
  id: string;
  userId: string;
  warningSignals: string[];
  copingStrategies: string[];
  trustedContacts: SafetyContact[];
  reasons: string[];
  createdAt: Date;
  updatedAt: Date;
}

export interface SafetyContact {
  id: string;
  name: string;
  relationship: string;
  contact: string;
}

// ============================================================================
// Learning Management Types
// ============================================================================

export interface LearningPath {
  id: string;
  title: string;
  description: string;
  topic: string;
  estimatedHours: number;
  createdAt: Date;
  updatedAt: Date;
}

export interface Course {
  id: string;
  learningPathId?: string;
  title: string;
  description: string;
  estimatedHours: number;
  clinicallyReviewed: boolean;
  clinicalReviewedBy?: string;
  clinicalReviewDate?: Date;
  createdAt: Date;
  updatedAt: Date;
}

export interface Lesson {
  id: string;
  courseId: string;
  title: string;
  content: string;
  mediaUrl?: string;
  order: number;
  createdAt: Date;
  updatedAt: Date;
}

export interface CourseProgress {
  id: string;
  userId: string;
  courseId: string;
  completedLessons: string[];
  completedAt?: Date;
  lastAccessedAt: Date;
  createdAt: Date;
}

// ============================================================================
// Media Types
// ============================================================================

export interface MediaAttachment {
  id: string;
  url: string;
  type: 'image' | 'video' | 'audio' | 'document';
  mimeType: string;
  size: number;
  altText?: string;
  uploadedAt: Date;
}

export interface UploadJob {
  id: string;
  userId: string;
  status: 'pending' | 'processing' | 'completed' | 'failed';
  originalFileName: string;
  contentType: string;
  fileSize: number;
  mediaId?: string;
  error?: string;
  createdAt: Date;
  completedAt?: Date;
}

// ============================================================================
// Moderation and Reporting Types
// ============================================================================

export type ReportReason =
  | 'spam'
  | 'harassment'
  | 'hate_speech'
  | 'violence'
  | 'self_harm'
  | 'suicide_risk'
  | 'abuse'
  | 'impersonation'
  | 'privacy_violation'
  | 'misinformation'
  | 'other';

export type ReportSeverity = 'low' | 'medium' | 'high' | 'critical';

export type ReportStatus = 'new' | 'reviewing' | 'resolved' | 'appealed' | 'dismissed';

export interface Report {
  id: string;
  reporterId: string;
  reportedContentId?: string;
  reportedUserId?: string;
  reportReason: ReportReason;
  description?: string;
  severity: ReportSeverity;
  status: ReportStatus;
  assignedToId?: string;
  createdAt: Date;
  updatedAt: Date;
  resolvedAt?: Date;
}

export type ModerationActionType =
  | 'warning'
  | 'content_removal'
  | 'restriction'
  | 'suspension'
  | 'ban'
  | 'appeal_granted'
  | 'appeal_denied';

export interface ModerationAction {
  id: string;
  reportId: string;
  actionType: ModerationActionType;
  reason: string;
  duration?: number; // in hours
  createdBy: string;
  createdAt: Date;
}

export interface Appeal {
  id: string;
  actionId: string;
  userId: string;
  reason: string;
  status: 'pending' | 'approved' | 'denied';
  reviewedBy?: string;
  reviewedAt?: Date;
  createdAt: Date;
}

export interface ModerationCase {
  id: string;
  reportId: string;
  status: 'open' | 'in-review' | 'closed';
  assignedToId?: string;
  notes: string;
  createdAt: Date;
  updatedAt: Date;
  closedAt?: Date;
}

// ============================================================================
// Block and Mute Types
// ============================================================================

export interface Block {
  id: string;
  userId: string;
  blockedUserId: string;
  createdAt: Date;
}

export interface Mute {
  id: string;
  userId: string;
  mutedUserId: string;
  createdAt: Date;
}

// ============================================================================
// Notification Types
// ============================================================================

export type NotificationType =
  | 'post_reply'
  | 'post_reaction'
  | 'mention'
  | 'direct_message'
  | 'community_announcement'
  | 'learning_milestone'
  | 'safety_check_in'
  | 'system_notification';

export interface Notification {
  id: string;
  userId: string;
  type: NotificationType;
  title: string;
  message: string;
  relatedId?: string;
  isRead: boolean;
  createdAt: Date;
  readAt?: Date;
}

export interface NotificationPreference {
  id: string;
  userId: string;
  type: NotificationType;
  isEnabled: boolean;
  quietHourStart?: string; // HH:mm format
  quietHourEnd?: string; // HH:mm format
  updatedAt: Date;
}

// ============================================================================
// Audit and Compliance Types
// ============================================================================

export type AuditEventType =
  | 'user_created'
  | 'user_deleted'
  | 'user_suspended'
  | 'login'
  | 'logout'
  | 'password_reset'
  | 'content_created'
  | 'content_deleted'
  | 'moderation_action'
  | 'admin_action'
  | 'data_export'
  | 'data_deletion';

export interface AuditLog {
  id: string;
  eventType: AuditEventType;
  userId: string;
  targetId?: string;
  targetType?: string;
  changes?: Record<string, unknown>;
  ipAddress?: string;
  userAgent?: string;
  status: 'success' | 'failure';
  error?: string;
  createdAt: Date;
}

// ============================================================================
// Data Governance Types
// ============================================================================

export interface DataExportRequest {
  id: string;
  userId: string;
  status: 'pending' | 'processing' | 'completed' | 'failed';
  downloadUrl?: string;
  expiresAt?: Date;
  createdAt: Date;
  completedAt?: Date;
}

export interface DeletionRequest {
  id: string;
  userId: string;
  status: 'pending' | 'processing' | 'completed' | 'failed';
  scheduledFor: Date;
  reason?: string;
  createdAt: Date;
  completedAt?: Date;
}

export interface ConsentRecord {
  id: string;
  userId: string;
  consentType: string;
  granted: boolean;
  version: string;
  createdAt: Date;
}

// ============================================================================
// Direct Messaging Types
// ============================================================================

export interface DirectMessageConversation {
  id: string;
  participant1Id: string;
  participant2Id: string;
  createdAt: Date;
  updatedAt: Date;
}

export interface DirectMessage {
  id: string;
  conversationId: string;
  senderId: string;
  content: string;
  media?: MediaAttachment[];
  isRead: boolean;
  createdAt: Date;
  readAt?: Date;
  deletedAt?: Date;
}

// ============================================================================
// AI Types
// ============================================================================

export interface AIConsent {
  id: string;
  userId: string;
  consentType: 'personalization' | 'training' | 'content_analysis';
  granted: boolean;
  version: string;
  createdAt: Date;
  updatedAt: Date;
}

export interface AIInteractionMetadata {
  id: string;
  userId: string;
  interactionType: string;
  modelVersion: string;
  promptCategory: string;
  safetyClassification: string;
  timestamp: Date;
}
