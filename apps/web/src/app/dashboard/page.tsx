'use client';

import { FormEvent, useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/lib/auth-context';
import { useProtectedRoute } from '@/lib/protected-route';

type Community = { id: string; name: string; description: string; privacyLevel: string; memberCount: number };
type Post = { id: string; communityId: string; title?: string; content: string; createdAt: string; commentCount: number };
type JournalEntry = { id: string; title?: string; content: string; moodRating?: number; entryDate: string };

const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:7001';

export default function DashboardPage() {
  const router = useRouter();
  const { user, token, logout, isLoading } = useAuth();
  useProtectedRoute();
  const [communities, setCommunities] = useState<Community[]>([]);
  const [selectedCommunityId, setSelectedCommunityId] = useState('');
  const [posts, setPosts] = useState<Post[]>([]);
  const [journalEntries, setJournalEntries] = useState<JournalEntry[]>([]);
  const [notice, setNotice] = useState('');
  const [communityName, setCommunityName] = useState('');
  const [communityDescription, setCommunityDescription] = useState('');
  const [postContent, setPostContent] = useState('');
  const [postTitle, setPostTitle] = useState('');
  const [journalContent, setJournalContent] = useState('');
  const [journalTitle, setJournalTitle] = useState('');
  const [journalMood, setJournalMood] = useState('');

  async function apiRequest<T>(path: string, options: RequestInit = {}): Promise<T> {
    const response = await fetch(`${apiUrl}${path}`, {
      ...options,
      headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token?.accessToken || ''}`, ...(options.headers || {}) },
    });
    if (!response.ok) throw new Error(`Request failed (${response.status})`);
    if (response.status === 204) return undefined as T;
    return response.json() as Promise<T>;
  }

  async function loadCommunities() {
    const result = await apiRequest<{ data: Community[] }>('/api/v1/communities');
    setCommunities(result.data || []);
    if (!selectedCommunityId && result.data?.[0]) setSelectedCommunityId(result.data[0].id);
  }

  async function loadJournalEntries() {
    const result = await apiRequest<{ data: JournalEntry[] }>('/api/v1/journal/entries');
    setJournalEntries(result.data || []);
  }

  async function loadPosts(communityId: string) {
    const result = await apiRequest<{ data: Post[] }>(`/api/v1/posts?communityId=${communityId}`);
    setPosts(result.data || []);
  }

  useEffect(() => {
    if (!token?.accessToken) return;
    Promise.all([loadCommunities(), loadJournalEntries()]).catch(() => setNotice('Some workspace data could not be loaded. You can try again shortly.'));
  }, [token?.accessToken]);

  useEffect(() => {
    if (!selectedCommunityId || !token?.accessToken) return;
    loadPosts(selectedCommunityId).catch(() => setNotice('Posts could not be loaded for this community.'));
  }, [selectedCommunityId, token?.accessToken]);

  async function createCommunity(event: FormEvent) {
    event.preventDefault();
    try {
      await apiRequest('/api/v1/communities', { method: 'POST', body: JSON.stringify({ name: communityName, description: communityDescription, privacyLevel: 'public' }) });
      setCommunityName(''); setCommunityDescription(''); setNotice('Community created.'); await loadCommunities();
    } catch { setNotice('Community could not be created. Check the details and try again.'); }
  }

  async function createPost(event: FormEvent) {
    event.preventDefault();
    if (!selectedCommunityId) return;
    try {
      await apiRequest('/api/v1/posts', { method: 'POST', body: JSON.stringify({ communityId: selectedCommunityId, title: postTitle, content: postContent, postType: 'text', visibility: 'community' }) });
      setPostTitle(''); setPostContent(''); setNotice('Post shared with your community.'); await loadPosts(selectedCommunityId);
    } catch { setNotice('Post could not be shared.'); }
  }

  async function createJournalEntry(event: FormEvent) {
    event.preventDefault();
    try {
      await apiRequest('/api/v1/journal/entries', { method: 'POST', body: JSON.stringify({ title: journalTitle, content: journalContent, moodRating: journalMood ? Number(journalMood) : undefined }) });
      setJournalTitle(''); setJournalContent(''); setJournalMood(''); setNotice('Your private entry was saved.'); await loadJournalEntries();
    } catch { setNotice('Your entry could not be saved.'); }
  }

  if (isLoading) return <div className="loading-state">Loading your workspace...</div>;
  const selectedCommunity = communities.find((community) => community.id === selectedCommunityId);

  return (
    <div className="workspace-shell">
      <header className="workspace-header"><div><p className="eyebrow">Your space</p><h1>Welcome back, {user?.displayName || 'friend'}</h1></div><button className="button button-quiet" onClick={() => { logout(); router.push('/'); }} type="button">Sign out</button></header>
      <main className="workspace-content">
        {notice && <div className="notice" role="status">{notice}</div>}
        <section className="workspace-intro" aria-labelledby="workspace-title"><p className="eyebrow">A gentle starting point</p><h2 id="workspace-title">Choose what feels useful today.</h2><p>Community conversations and private reflection live side by side, with a clear boundary between them.</p></section>
        <div className="workspace-grid">
          <section className="workspace-panel communities-panel" aria-labelledby="communities-title">
            <div className="panel-heading"><div><p className="eyebrow">Shared spaces</p><h2 id="communities-title">Communities</h2></div><span className="count-badge">{communities.length}</span></div>
            <div className="community-list">{communities.length === 0 && <p className="empty-state">No communities yet. You can make the first one.</p>}{communities.map((community) => <button className={`community-item ${selectedCommunityId === community.id ? 'is-selected' : ''}`} key={community.id} onClick={() => setSelectedCommunityId(community.id)} type="button"><span><strong>{community.name}</strong><small>{community.description}</small></span><span className="privacy-label">{community.privacyLevel}</span></button>)}</div>
            <form className="compact-form" onSubmit={createCommunity}><h3>Start a community</h3><label htmlFor="community-name">Name</label><input id="community-name" onChange={(event) => setCommunityName(event.target.value)} required value={communityName} /><label htmlFor="community-description">Purpose</label><textarea id="community-description" onChange={(event) => setCommunityDescription(event.target.value)} required rows={3} value={communityDescription} /><button className="button button-primary" type="submit">Create community</button></form>
          </section>
          <section className="workspace-panel feed-panel" aria-labelledby="feed-title">
            <div className="panel-heading"><div><p className="eyebrow">Finite and chronological</p><h2 id="feed-title">{selectedCommunity?.name || 'Community posts'}</h2></div><span className="count-badge">{posts.length}</span></div>
            <form className="post-form" onSubmit={createPost}><label htmlFor="post-title">Optional title</label><input id="post-title" onChange={(event) => setPostTitle(event.target.value)} value={postTitle} /><label htmlFor="post-content">Share something with this community</label><textarea id="post-content" onChange={(event) => setPostContent(event.target.value)} required rows={4} value={postContent} /><div className="form-footer"><span className="visibility-label">Visible to community members</span><button className="button button-primary" disabled={!selectedCommunityId} type="submit">Share post</button></div></form>
            <div className="post-list">{posts.length === 0 && <p className="empty-state">No posts here yet. A quiet beginning is still a beginning.</p>}{posts.map((post) => <article className="post-item" key={post.id}><p className="post-meta">{new Date(post.createdAt).toLocaleDateString()} · {post.commentCount} comments</p>{post.title && <h3>{post.title}</h3>}<p>{post.content}</p></article>)}</div>
          </section>
          <section className="workspace-panel journal-panel" aria-labelledby="journal-title">
            <div className="panel-heading"><div><p className="eyebrow">Only visible to you</p><h2 id="journal-title">Private journal</h2></div><span className="private-mark">Private</span></div>
            <form className="journal-form" onSubmit={createJournalEntry}><label htmlFor="journal-title-input">Optional title</label><input id="journal-title-input" onChange={(event) => setJournalTitle(event.target.value)} value={journalTitle} /><label htmlFor="journal-content">Write at your own pace</label><textarea id="journal-content" onChange={(event) => setJournalContent(event.target.value)} required rows={5} value={journalContent} /><label htmlFor="journal-mood">Mood, if useful (1 to 10)</label><input id="journal-mood" max="10" min="1" onChange={(event) => setJournalMood(event.target.value)} type="number" value={journalMood} /><button className="button button-secondary" type="submit">Save privately</button></form>
            <div className="journal-list">{journalEntries.length === 0 && <p className="empty-state">Your private entries will appear here.</p>}{journalEntries.slice(0, 5).map((entry) => <article className="journal-item" key={entry.id}><p className="post-meta">{new Date(entry.entryDate).toLocaleDateString()}{entry.moodRating ? ` · Mood ${entry.moodRating}/10` : ''}</p><h3>{entry.title || 'Untitled reflection'}</h3><p>{entry.content}</p></article>)}</div>
          </section>
        </div>
      </main>
    </div>
  );
}
