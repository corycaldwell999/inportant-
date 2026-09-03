/**
 * Trauma-Informed Platform - Typed API Client SDK
 * Provides type-safe API methods for all platform endpoints
 */

import type {
  ApiEnvelope,
  ApiErrorEnvelope,
  User,
  UserProfile,
  AuthToken,
  Community,
  Post,
  Comment,
  JournalEntry,
  CrisisResource,
  SafetyPlan,
  PaginatedEnvelope,
} from '@trauma-platform/types';

export interface ClientConfig {
  baseUrl: string;
  apiVersion?: string;
}

export class TraumaPlatformClient {
  private baseUrl: string;
  private apiVersion: string;
  private accessToken?: string;

  constructor(config: ClientConfig) {
    this.baseUrl = config.baseUrl.replace(/\/$/, '');
    this.apiVersion = config.apiVersion || 'v1';
  }

  setAccessToken(token: string): void {
    this.accessToken = token;
  }

  clearAccessToken(): void {
    this.accessToken = undefined;
  }

  private async request<T>(
    method: string,
    endpoint: string,
    data?: unknown,
  ): Promise<T> {
    const url = `${this.baseUrl}/api/${this.apiVersion}${endpoint}`;
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
    };

    if (this.accessToken) {
      headers.Authorization = `Bearer ${this.accessToken}`;
    }

    const response = await fetch(url, {
      method,
      headers,
      body: data ? JSON.stringify(data) : undefined,
    });

    if (!response.ok) {
      const error = (await response.json()) as ApiErrorEnvelope;
      throw new Error(`API Error (${error.error.code}): ${error.error.message}`);
    }

    return (await response.json()) as T;
  }

  // ========================================================================
  // Health & Status
  // ========================================================================

  async getHealth(): Promise<ApiEnvelope<{ status: string }>> {
    return this.request('GET', '/health');
  }

  async getReadiness(): Promise<ApiEnvelope<{ ready: boolean }>> {
    return this.request('GET', '/ready');
  }

  // ========================================================================
  // Authentication
  // ========================================================================

  async register(data: {
    email: string;
    password: string;
    displayName: string;
    acceptTerms: boolean;
    acceptPrivacyPolicy: boolean;
  }): Promise<ApiEnvelope<{ user: User; token: AuthToken }>> {
    return this.request('POST', '/auth/register', data);
  }

  async login(data: {
    email: string;
    password: string;
  }): Promise<ApiEnvelope<{ user: User; token: AuthToken }>> {
    return this.request('POST', '/auth/login', data);
  }

  async logout(): Promise<ApiEnvelope<{ success: boolean }>> {
    return this.request('POST', '/auth/logout');
  }

  async refreshToken(): Promise<ApiEnvelope<{ token: AuthToken }>> {
    return this.request('POST', '/auth/refresh');
  }

  // ========================================================================
  // User Profile
  // ========================================================================

  async getCurrentUser(): Promise<ApiEnvelope<User>> {
    return this.request('GET', '/users/me');
  }

  async updateProfile(data: Partial<UserProfile>): Promise<ApiEnvelope<UserProfile>> {
    return this.request('PATCH', '/users/me/profile', data);
  }

  async getUserProfile(userId: string): Promise<ApiEnvelope<UserProfile>> {
    return this.request('GET', `/users/${userId}/profile`);
  }

  // ========================================================================
  // Communities
  // ========================================================================

  async listCommunities(
    params?: { page?: number; limit?: number },
  ): Promise<PaginatedEnvelope<Community>> {
    const searchParams = new URLSearchParams();
    if (params?.page) searchParams.append('page', params.page.toString());
    if (params?.limit) searchParams.append('limit', params.limit.toString());

    return this.request('GET', `/communities?${searchParams.toString()}`);
  }

  async getCommunity(communityId: string): Promise<ApiEnvelope<Community>> {
    return this.request('GET', `/communities/${communityId}`);
  }

  async createCommunity(data: {
    name: string;
    description: string;
    visibility: 'public' | 'private' | 'invite-only';
  }): Promise<ApiEnvelope<Community>> {
    return this.request('POST', '/communities', data);
  }

  // ========================================================================
  // Posts
  // ========================================================================

  async createPost(data: {
    communityId: string;
    content: string;
    visibility: string;
    contentWarning?: string;
  }): Promise<ApiEnvelope<Post>> {
    return this.request('POST', '/posts', data);
  }

  async getPost(postId: string): Promise<ApiEnvelope<Post>> {
    return this.request('GET', `/posts/${postId}`);
  }

  async updatePost(
    postId: string,
    data: { content?: string; contentWarning?: string },
  ): Promise<ApiEnvelope<Post>> {
    return this.request('PATCH', `/posts/${postId}`, data);
  }

  async deletePost(postId: string): Promise<ApiEnvelope<{ success: boolean }>> {
    return this.request('DELETE', `/posts/${postId}`);
  }

  // ========================================================================
  // Comments
  // ========================================================================

  async createComment(data: {
    postId: string;
    content: string;
  }): Promise<ApiEnvelope<Comment>> {
    return this.request('POST', '/comments', data);
  }

  async getComment(commentId: string): Promise<ApiEnvelope<Comment>> {
    return this.request('GET', `/comments/${commentId}`);
  }

  async deleteComment(commentId: string): Promise<ApiEnvelope<{ success: boolean }>> {
    return this.request('DELETE', `/comments/${commentId}`);
  }

  // ========================================================================
  // Journals
  // ========================================================================

  async createJournalEntry(data: {
    content: string;
    mood?: string;
    tags?: string[];
  }): Promise<ApiEnvelope<JournalEntry>> {
    return this.request('POST', '/journals', data);
  }

  async getJournalEntry(entryId: string): Promise<ApiEnvelope<JournalEntry>> {
    return this.request('GET', `/journals/${entryId}`);
  }

  async listJournalEntries(
    params?: { page?: number; limit?: number },
  ): Promise<PaginatedEnvelope<JournalEntry>> {
    const searchParams = new URLSearchParams();
    if (params?.page) searchParams.append('page', params.page.toString());
    if (params?.limit) searchParams.append('limit', params.limit.toString());

    return this.request('GET', `/journals?${searchParams.toString()}`);
  }

  async updateJournalEntry(
    entryId: string,
    data: { content?: string; mood?: string; tags?: string[] },
  ): Promise<ApiEnvelope<JournalEntry>> {
    return this.request('PATCH', `/journals/${entryId}`, data);
  }

  async deleteJournalEntry(entryId: string): Promise<ApiEnvelope<{ success: boolean }>> {
    return this.request('DELETE', `/journals/${entryId}`);
  }

  // ========================================================================
  // Crisis Resources
  // ========================================================================

  async getCrisisResources(region?: string): Promise<ApiEnvelope<CrisisResource[]>> {
    const searchParams = new URLSearchParams();
    if (region) searchParams.append('region', region);

    return this.request('GET', `/crisis-resources?${searchParams.toString()}`);
  }

  // ========================================================================
  // Safety Plans
  // ========================================================================

  async getSafetyPlan(): Promise<ApiEnvelope<SafetyPlan | null>> {
    return this.request('GET', '/safety-plans/me');
  }

  async createSafetyPlan(data: {
    warningSignals: string[];
    copingStrategies: string[];
    trustedContacts: Array<{ name: string; relationship: string; contact: string }>;
    reasons: string[];
  }): Promise<ApiEnvelope<SafetyPlan>> {
    return this.request('POST', '/safety-plans', data);
  }

  async updateSafetyPlan(
    data: Partial<SafetyPlan>,
  ): Promise<ApiEnvelope<SafetyPlan>> {
    return this.request('PATCH', '/safety-plans/me', data);
  }

  // ========================================================================
  // Reporting
  // ========================================================================

  async submitReport(data: {
    reportedContentId?: string;
    reportedUserId?: string;
    reportReason: string;
    description?: string;
  }): Promise<ApiEnvelope<{ reportId: string }>> {
    return this.request('POST', '/reports', data);
  }

  // ========================================================================
  // Data Access
  // ========================================================================

  async requestDataExport(): Promise<ApiEnvelope<{ requestId: string }>> {
    return this.request('POST', '/data-export/request');
  }

  async getDataExportStatus(
    requestId: string,
  ): Promise<ApiEnvelope<{ status: string; downloadUrl?: string }>> {
    return this.request('GET', `/data-export/${requestId}/status`);
  }

  async requestAccountDeletion(reason?: string): Promise<
    ApiEnvelope<{
      scheduledFor: string;
      confirmationRequired: boolean;
    }>
  > {
    return this.request('POST', '/account/deletion-request', { reason });
  }
}

// Export type-safe instance
export function createClient(config: ClientConfig): TraumaPlatformClient {
  return new TraumaPlatformClient(config);
}

export * from '@trauma-platform/types';
